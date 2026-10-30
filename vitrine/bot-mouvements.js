// Échos d'Éther : le bot des mouvements.
// Sur chaque personnage, dans chaque animation, il mesure ce qui se voit mal :
//   - les pieds qui glissent pendant qu'ils portent le corps ;
//   - les pieds qui s'enfoncent dans le sol, ou le corps qui flotte au-dessus ;
//   - la boucle qui saute (la dernière image ne rejoint pas la première) ;
//   - la peau qui se déchire (pour information).
// Puis il corrige ce qui peut l'être pour tous les corps : chaque pied posé est ramené au sol par cinématique inverse
// (la jambe se replie, les hanches descendent si elle ne peut pas l'atteindre), aucun pied ne passe sous le sol,
// et la fin de chaque boucle est raccordée à son début. Dans une animation sur place (attente, visée, garde), le pied
// posé est aussi tenu immobile. Dans une marche ou une course sur place, il ne l'est pas : un pied qui roule du talon
// aux orteils n'a pas de point fixe sur un tapis roulant ; c'est en jeu, sur un vrai sol, que le pied posé se bloque.
//
// Le pied est mesuré par sa semelle : les sommets de la peau sous la cheville, tels que les os les déforment. C'est ce
// qui touche le sol à l'écran, quelle que soit la forme du pied (à plat, sur la pointe, griffu).
// Les animations « sur place » (marche, course) font défiler le sol sous les pieds comme un tapis roulant : un pied posé
// doit alors reculer à la vitesse du sol, sans glisser par rapport à lui. Le bot mesure cette vitesse sur les pieds posés
// eux-mêmes, si bien que la même mesure vaut pour une animation sur place ou qui avance.
import * as THREE from 'three';
import { ik2, orienterMonde } from './prise-armes.js';

const v = () => new THREE.Vector3(), q = () => new THREE.Quaternion();
const HAUTEUR_HANCHES_MANNEQUIN = 0.95;   // le mannequin du studio (1,83 m) : hanches à 95 cm du sol
const IPS = 30;
const SOMMETS_PAR_PIED = 360;

// ------------------------------------------------------------------ les pieds, au repos
// Pour chaque pied : sa chaîne d'os (cuisse, mollet, pied), la longueur de la jambe, et les sommets de sa semelle.
// k : l'échelle du personnage par rapport au mannequin (les mesures sont rendues en centimètres du mannequin).
export function noterPieds(carte, racine) {
  if (!['pied_g', 'pied_d', 'cuisse_g', 'cuisse_d', 'mollet_g', 'mollet_d', 'hanches'].every(n => carte[n])) return null;
  racine.updateMatrixWorld(true);
  const pos = n => carte[n].getWorldPosition(v());
  const solOs = Math.min(...['pied_g', 'pied_d', 'orteil_g', 'orteil_d'].filter(n => carte[n]).map(n => pos(n).y));
  const k = Math.max(0.2, (pos('hanches').y - solOs) / HAUTEUR_HANCHES_MANNEQUIN);
  // l'avant du personnage, à plat (de la cuisse droite à la gauche, puis vers l'avant)
  const avant = v().crossVectors(new THREE.Vector3(0, 1, 0), pos('cuisse_d').sub(pos('cuisse_g'))).setY(0).normalize();
  // ce qui porte tout le corps : les hanches, ou, dans les squelettes où les jambes n'en descendent pas (Rigify),
  // leur ancêtre commun avec les cuisses
  const ancetres = new Set(); for (let o = carte.hanches; o; o = o.parent) ancetres.add(o);
  let corps = carte.cuisse_g; while (corps && !ancetres.has(corps)) corps = corps.parent;
  const P = { k, hanches: carte.hanches, corps: corps || carte.hanches, carte, racine, avant: new THREE.Vector2(avant.x, avant.z) };
  const peaux = [];
  racine.traverse(n => { if (n.isSkinnedMesh && n.visible !== false) peaux.push(n); });
  for (const c of ['g', 'd']) {
    const autre = c === 'g' ? 'd' : 'g';
    const cheville = pos('pied_' + c), chevilleAutre = pos('pied_' + autre);
    const bout = carte['orteil_' + c] ? pos('orteil_' + c) : cheville.clone();
    // la peau que porte cette jambe : le mollet et tout ce qui en descend (pied, orteils)
    const jambe = new Set(); carte['mollet_' + c].traverse(o => jambe.add(o));
    const portePar = (m, i) => {
      const si = m.geometry.attributes.skinIndex, sw = m.geometry.attributes.skinWeight;
      if (!si || !sw || !m.skeleton) return 1;
      let s = 0;
      for (let k = 0; k < si.itemSize; k++) if (jambe.has(m.skeleton.bones[si.getComponent(i, k)])) s += sw.getComponent(i, k);
      return s;
    };
    const sommets = [], portes = [];
    for (const m of peaux) {
      const n = m.geometry.attributes.position.count, pas = Math.max(1, Math.floor(n / 90000));
      for (let i = 0; i < n; i += pas) {
        const w = m.getVertexPosition(i, v()).applyMatrix4(m.matrixWorld);
        if (w.y > cheville.y + 0.03 * k) continue;                               // sous la cheville
        if (w.distanceTo(cheville) > w.distanceTo(chevilleAutre)) continue;      // de ce côté-ci
        // près du pied : du talon (un peu derrière la cheville) au bout des orteils (un peu devant)
        const axe = bout.clone().sub(cheville); axe.y = 0;
        const l = axe.length(); if (l > 1e-6) axe.divideScalar(l);
        const r = w.clone().sub(cheville); r.y = 0;
        const t = l > 1e-6 ? r.dot(axe) : 0, lat = l > 1e-6 ? r.clone().addScaledVector(axe, -t).length() : r.length();
        if (t < -0.12 * k || t > l + 0.12 * k || lat > 0.09 * k) continue;
        sommets.push({ m, i });
        if (portePar(m, i) >= 0.5) portes.push({ m, i });
      }
    }
    // sans la peau d'à côté (un pan de vêtement, l'autre jambe qui frôle) ; si la jambe ne porte presque rien
    // (une peau liée à d'autres os que ceux du squelette reconnu), on garde tout
    const choisis = portes.length >= Math.max(12, 0.2 * sommets.length) ? portes : sommets;
    const garde = choisis.length > SOMMETS_PAR_PIED ? choisis.filter((_, j) => j % Math.ceil(choisis.length / SOMMETS_PAR_PIED) === 0) : choisis;
    const longueur = pos('cuisse_' + c).distanceTo(pos('mollet_' + c)) + pos('mollet_' + c).distanceTo(cheville);
    P[c] = { cuisse: carte['cuisse_' + c], mollet: carte['mollet_' + c], pied: carte['pied_' + c], orteil: carte['orteil_' + c] || null, sommets: garde, longueur };
  }
  // le sol : la semelle la plus basse, au repos (le personnage est posé debout)
  const s = ['g', 'd'].map(c => semelle(P, c)).filter(x => x);
  P.sol = s.length ? Math.min(...s.map(x => x.y)) : solOs;
  P.parSommets = s.length === 2;
  if (!P.parSommets) {
    // sans peau sous le pied : la cheville et les orteils, à leur hauteur de repos
    for (const c of ['g', 'd']) { P[c].hPied = pos('pied_' + c).y - P.sol; P[c].hOrteil = carte['orteil_' + c] ? pos('orteil_' + c).y - P.sol : null; }
  }
  return P;
}

// la semelle d'un pied, maintenant : son point le plus bas (y) et le centre de la zone qui touche (x, z)
function semelle(P, c) {
  const p = P[c];
  if (!p.sommets.length) return null;
  let min = Infinity;
  const pts = p.sommets.map(({ m, i }) => { const w = m.getVertexPosition(i, v()).applyMatrix4(m.matrixWorld); if (w.y < min) min = w.y; return w; });
  const proche = pts.filter(w => w.y < min + 0.012 * P.k);
  const centre = proche.reduce((a, w) => a.add(w), v()).divideScalar(proche.length);
  return { y: min, point: new THREE.Vector3(centre.x, min, centre.z) };
}

// la hauteur d'un pied au-dessus du sol (e) et le point qui le touche
function hauteurPied(P, c) {
  if (P.parSommets) { const s = semelle(P, c); return { e: s.y - P.sol, point: s.point }; }
  const p = P[c], wP = p.pied.getWorldPosition(v()), eP = wP.y - P.sol - p.hPied;
  if (!p.orteil) return { e: eP, point: wP };
  const wO = p.orteil.getWorldPosition(v()), eO = wO.y - P.sol - p.hOrteil;
  return eO < eP ? { e: eO, point: wO } : { e: eP, point: wP };
}
const plat = w => new THREE.Vector2(w.x, w.z);
const median = l => { const s = [...l].sort((a, b) => a - b); return s.length ? s[s.length >> 1] : 0; };

// ------------------------------------------------------------------ l'échantillonnage d'une animation
// poser(u) pose le personnage à la fraction u de l'animation (0 à 1) ; us : les fractions à échantillonner
function echantillonner(P, poser, us) {
  return us.map(u => {
    poser(u);
    P.racine.updateMatrixWorld(true);
    const image = {};
    for (const c of ['g', 'd']) {
      const h = hauteurPied(P, c);
      image[c] = { e: h.e, point: h.point, cheville: P[c].pied.getWorldPosition(v()), hanche: P[c].cuisse.getWorldPosition(v()) };
    }
    return image;
  });
}

// les phases d'appui de chaque pied et la vitesse du sol sous les pieds.
// Un pied porte quand il ne part pas vers l'avant (la jambe qui passe, elle, file vers l'avant) et qu'il est près du plus
// bas de sa course. C'est le mouvement qui décide, pas la hauteur seule : un pied mal reciblé qui traverse le sol reste
// reconnu, et la même analyse sert avant et après correction.
function appuis(P, images, dt) {
  const n = images.length, phases = { g: [], d: [] }, vitesses = [];
  for (const c of ['g', 'd']) {
    const eMin = Math.min(...images.map(im => im[c].e));
    if (eMin > 0.12 * P.k) continue;                                   // ce pied ne touche jamais le sol
    const touche = images.map((im, i) => {
      const a = images[(i - 1 + n) % n][c].point, b = images[(i + 1) % n][c].point;
      const versAvant = ((b.x - a.x) * P.avant.x + (b.z - a.z) * P.avant.y) / (2 * dt);
      return versAvant < 0.15 * P.k && im[c].e < eMin + 0.06 * P.k;
    });
    const debut = touche.findIndex(t => !t);
    if (debut < 0) phases[c].push({ images: Array.from({ length: n }, (_, i) => i), entier: true });
    else {
      let courante = null;
      for (let j = 1; j <= n; j++) {
        const i = (debut + j) % n;
        if (touche[i]) (courante ??= []).push(i);
        else if (courante) { if (courante.length >= 2) phases[c].push({ images: courante }); courante = null; }
      }
      if (courante && courante.length >= 2) phases[c].push({ images: courante });
    }
    for (const ph of phases[c]) for (let j = 1; j < ph.images.length; j++) {
      const a = images[ph.images[j - 1]][c].point, b = images[ph.images[j]][c].point;
      vitesses.push(plat(b).sub(plat(a)).divideScalar(dt));
    }
  }
  const vSol = new THREE.Vector2(median(vitesses.map(w => w.x)), median(vitesses.map(w => w.y)));
  return { phases, vSol };
}

// une animation où l'on reste sur place (attente, visée, garde), par opposition à une marche ou une course sur place
const surPlace = (P, vSol) => vSol.length() < 0.08 * P.k;

// ------------------------------------------------------------------ la mesure
// L'analyse d'une animation : ses images, les phases d'appui de chaque pied, la vitesse du sol.
export function analyser(P, poser, duree) {
  const n = Math.max(12, Math.round(duree * IPS)), dt = duree / n;
  const images = echantillonner(P, poser, Array.from({ length: n }, (_, i) => i / n));
  return { n, dt, images, ...appuis(P, images, dt) };
}

// Le pire de chaque défaut, en centimètres du mannequin. boucle : true pour une animation qui se répète.
// reference : l'analyse de l'animation d'origine, pour mesurer une version corrigée sur les mêmes phases d'appui.
export function mesurer(P, poser, duree, { boucle = true, peau = null, reference = null } = {}) {
  const a = reference ? { ...reference, images: echantillonner(P, poser, Array.from({ length: reference.n }, (_, i) => i / reference.n)) } : analyser(P, poser, duree);
  const { images, phases, vSol, dt } = a;
  let glisse = 0;
  for (const c of ['g', 'd']) for (const ph of phases[c]) {
    const l = ph.images, m = l.length >> 1, pm = plat(images[l[m]][c].point);
    l.forEach((i, j) => { glisse = Math.max(glisse, plat(images[i][c].point).distanceTo(pm.clone().addScaledVector(vSol, (j - m) * dt))); });
  }
  const plusBas = images.map(im => Math.min(im.g.e, im.d.e));
  const enfoncement = Math.max(0, -Math.min(...plusBas));
  const flottement = Math.max(0, Math.min(...plusBas));
  let saut = 0;
  if (boucle) {
    // la pose de la dernière image doit rejoindre la première (positions relatives aux hanches)
    const relatif = () => {
      const h = P.hanches.getWorldPosition(v()), C = P.carte;
      return ['main_g', 'main_d', 'pied_g', 'pied_d', 'tete'].filter(k => C[k]).map(k => C[k].getWorldPosition(v()).sub(h));
    };
    poser(0); P.racine.updateMatrixWorld(true); const a = relatif();
    poser(0.9999); P.racine.updateMatrixWorld(true); const b = relatif();
    saut = Math.max(...a.map((x, i) => x.distanceTo(b[i])));
  }
  let dechirure = null;
  if (peau) {
    dechirure = 0;
    for (const u of [0.1, 0.35, 0.6, 0.85]) { poser(u); const m = peau(); if (m) dechirure = Math.max(dechirure, m.dechirure); }
  }
  const cm = x => Math.round(x / P.k * 1000) / 10;
  return {
    glisse: cm(glisse), enfoncement: cm(enfoncement), flottement: cm(flottement), saut: cm(saut), dechirure,
    vitesseSol: Math.round(vSol.length() / P.k * 100) / 100, appuis: phases.g.length + phases.d.length, surPlace: surPlace(P, vSol),
  };
}

// ------------------------------------------------------------------ la correction : pieds ancrés, boucle raccordée
const lisse = (a, b, x) => { const t = Math.min(1, Math.max(0, (x - a) / (b - a))); return t * t * (3 - 2 * t); };

// Corrige une animation reciblée (un THREE.AnimationClip dont les pistes portent l'uuid des os) et en rend une nouvelle.
// poser(u) pose le personnage à la fraction u de l'animation clip.
export function ancrer(P, clip, poser, { boucle = true } = {}) {
  const pisteHanches = clip.tracks.find(t => t.name === P.hanches.uuid + '.position');
  const os = ['g', 'd'].flatMap(c => [P[c].cuisse, P[c].mollet, P[c].pied]);
  const pistesOs = os.map(o => clip.tracks.find(t => t.name === o.uuid + '.quaternion'));
  if (!pisteHanches || pistesOs.some(t => !t)) return null;
  const temps = pisteHanches.times, n = temps.length, duree = clip.duration;
  const dt = n > 1 ? temps[1] - temps[0] : duree;
  const us = Array.from(temps, t => Math.min(t / duree, 0.9999));
  const images = echantillonner(P, poser, us);
  const { phases, vSol } = appuis(P, images, dt);
  const fixe = surPlace(P, vSol);

  // pour chaque pied et chaque image : où doit être le point d'appui, et le poids de l'ancrage (fondu aux bords des phases).
  // Sur place, le pied posé reste où il est ; en marche, seule sa hauteur est corrigée.
  const cibles = { g: new Array(n).fill(null), d: new Array(n).fill(null) };
  for (const c of ['g', 'd']) for (const ph of phases[c]) {
    const l = ph.images, m = l.length >> 1, ancre = images[l[m]][c].point.clone();
    const bord = ph.entier ? 0 : Math.min(2, Math.floor(l.length / 3));
    l.forEach((i, j) => {
      const cible = fixe ? ancre.clone() : images[i][c].point.clone(); cible.y = P.sol;
      const poids = bord ? lisse(0, 1, Math.min(1, (j + 1) / (bord + 1), (l.length - j) / (bord + 1))) : 1;
      cibles[c][i] = { cible, poids };
    });
  }

  // les hanches descendent quand une jambe ne peut pas atteindre son appui (lissé dans le temps) ;
  // si aucun pied ne touche jamais le sol (un accroupi mal reciblé), tout le corps descend jusqu'à ce qu'il le touche
  const flotte = phases.g.length + phases.d.length === 0 ? Math.min(...images.map(im => Math.min(im.g.e, im.d.e))) : 0;
  const descente = new Array(n).fill(Math.max(0, flotte));
  for (let i = 0; i < n; i++) for (const c of ['g', 'd']) {
    const ci = cibles[c][i]; if (!ci) continue;
    const im = images[i][c], cheville = im.cheville.clone().addScaledVector(ci.cible.clone().sub(im.point), ci.poids);
    const portee = P[c].longueur * 0.985, H = im.hanche, dx = Math.hypot(cheville.x - H.x, cheville.z - H.z);
    if (dx < portee) descente[i] = Math.max(descente[i], H.y - (cheville.y + Math.sqrt(portee * portee - dx * dx)));
  }
  const lissee = descente.map((_, i) => {
    let s = 0, w = 0;
    for (let j = -4; j <= 4; j++) { const k = boucle ? (i + j + n) % n : Math.min(n - 1, Math.max(0, i + j)); const p = 1 - Math.abs(j) / 5; s += Math.max(0, descente[k]) * p; w += p; }
    return Math.max(Math.max(0, descente[i]), s / w);
  });

  // la nouvelle pose, image par image ; la descente s'applique à ce qui porte tout le corps
  const valeursOs = os.map(() => new Float32Array(n * 4)), valeursHanches = new Float32Array(n * 3);
  const autreCorps = P.corps !== P.hanches, valeursCorps = autreCorps ? new Float32Array(n * 3) : null;
  const corpsRepos = P.corps.position.clone();
  for (let i = 0; i < n; i++) {
    poser(us[i]);
    if (autreCorps) P.corps.position.copy(corpsRepos);
    P.racine.updateMatrixWorld(true);
    if (lissee[i] > 1e-5) {
      const H = P.corps.getWorldPosition(v()); H.y -= lissee[i];
      P.corps.position.copy(P.corps.parent.worldToLocal(H)); P.corps.updateMatrixWorld(true);
    }
    for (const c of ['g', 'd']) {
      const p = P[c], ci = cibles[c][i];
      const h = hauteurPied(P, c);
      let decal = null;
      if (ci) { decal = ci.cible.clone().sub(h.point).multiplyScalar(ci.poids); if (!fixe) decal.x = decal.z = 0; }
      if (h.e + (decal ? decal.y : 0) < 0) (decal ??= v()).y = -h.e;   // jamais sous le sol
      if (!decal || decal.lengthSq() < 1e-10) continue;
      const piedMonde = p.pied.getWorldQuaternion(q());
      ik2(p.cuisse, p.mollet, p.pied, p.pied.getWorldPosition(v()).add(decal), p.mollet.getWorldPosition(v()));
      orienterMonde(p.pied, piedMonde);
    }
    os.forEach((o, k) => o.quaternion.toArray(valeursOs[k], i * 4));
    P.hanches.position.toArray(valeursHanches, i * 3);
    if (autreCorps) P.corps.position.toArray(valeursCorps, i * 3);
  }
  if (autreCorps) P.corps.position.copy(corpsRepos);

  // la boucle : les dernières images rejoignent la première (la dernière clé, à la fin, lui devient égale)
  const m = Math.min(8, Math.floor(n / 4));
  const raccorderQ = val => {
    if (!boucle || n <= 8) return val;
    const qa = q(), qb = q();
    for (let i = n - m; i < n; i++) { const w = lisse(0, 1, (i - (n - m) + 1) / m); qa.fromArray(val, i * 4); qb.fromArray(val, 0); qa.slerp(qb, w); qa.toArray(val, i * 4); }
    return val;
  };
  if (boucle && n > 8) for (let i = n - m; i < n; i++) {
    const w = lisse(0, 1, (i - (n - m) + 1) / m);
    for (let k = 0; k < 3; k++) valeursHanches[i * 3 + k] += (valeursHanches[k] - valeursHanches[i * 3 + k]) * w;
  }
  const pistes = clip.tracks.map(t => {
    const k = pistesOs.indexOf(t);
    if (k >= 0) return new THREE.QuaternionKeyframeTrack(t.name, temps, raccorderQ(valeursOs[k]));
    if (t === pisteHanches) return new THREE.VectorKeyframeTrack(t.name, temps, valeursHanches);
    // les autres os : la même jonction de boucle
    if (t.ValueTypeName === 'quaternion' && t.times.length === n) return new THREE.QuaternionKeyframeTrack(t.name, temps, raccorderQ(Float32Array.from(t.values)));
    return t;
  });
  if (autreCorps) pistes.push(new THREE.VectorKeyframeTrack(P.corps.uuid + '.position', temps, valeursCorps));
  return new THREE.AnimationClip(clip.name, duree, pistes);
}
