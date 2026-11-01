// Échos d'Éther : le bot des objets.
// Pour chaque objet du décor qui peut bouger (porte, sas, armoire, console, machine, drone), il :
//   1. inspecte le modèle : ses pièces (nom, taille, place), ses matériaux lumineux, ses animations embarquées ;
//   2. propose une recette d'animation : les battants d'une porte qui coulissent, les pièces qui tournent sur
//      elles-mêmes, les lumières, le vol d'un drone, l'animation embarquée jouée à l'ouverture ;
//   3. l'anime selon l'état d'énergie du Colosse : normal, alarme (portes verrouillées, lumières rouges qui battent,
//      machines qui s'emballent), panne (portes bloquées à demi, lumières qui grésillent, machines qui ralentissent) ;
//   4. contrôle le résultat : le passage laissé libre par une porte ouverte, les pièces qui tournent sur leur centre.
// La recette ne contient que des noms de pièces et des directions géométriques (« vers l'extérieur », « autour de son
// axe court ») : Unity la relit (ObjetAnime) et retrouve les axes de la même façon, quelle que soit sa convention.
import * as THREE from 'three';

export const ETATS = ['normal', 'alarme', 'panne'];
const ROUGE = new THREE.Color(1.0, 0.16, 0.1);
const v = () => new THREE.Vector3();

// ------------------------------------------------------------------ 1. l'inspection
// La catégorie d'un objet, d'après sa clé (dossier/nom)
export function categorie(cle) {
  const n = cle.split('/')[1];
  if (/drone/.test(n)) return 'drone';
  if (/^porte|cloison|armoire/.test(n)) return 'porte';
  if (/console|ecran|terminal|serveur|neons/.test(n)) return 'ecran';
  return 'machine';
}

// Les pièces d'un objet : chaque maillage, avec sa boîte dans le repère de l'objet (sa racine).
export function inspecter(racine, animations, cle) {
  racine.updateMatrixWorld(true);
  const inverse = racine.matrixWorld.clone().invert();
  const pieces = [], lumieres = new Set();
  racine.traverse(o => {
    if (!o.isMesh) return;
    const b = new THREE.Box3();
    const g = o.geometry; if (!g.boundingBox) g.computeBoundingBox();
    b.copy(g.boundingBox).applyMatrix4(new THREE.Matrix4().multiplyMatrices(inverse, o.matrixWorld));
    pieces.push({ noeud: o, nom: nomPiece(o), boite: b, taille: b.getSize(v()), centre: b.getCenter(v()) });
    for (const m of [].concat(o.material)) {
      if (m.emissiveMap || (m.emissive && m.emissive.getHSL({}).l > 0.01)) lumieres.add(m.name || '(sans nom)');
      // un écran ou une lampe sans lumière propre : sa couleur devient sa lumière
      else if (ECRAN.test(m.name || '') || ECRAN.test(nomPiece(o))) { m.emissive = new THREE.Color(1, 1, 1); m.emissiveMap = m.map; m.emissiveIntensity = 0.8; lumieres.add(m.name || '(sans nom)'); }
    }
  });
  const boite = pieces.reduce((a, p) => a.union(p.boite), new THREE.Box3());
  return { cle, categorie: categorie(cle), pieces, boite, taille: boite.getSize(v()), centre: boite.getCenter(v()), lumieres: [...lumieres], animations: animations.map(a => a.name) };
}

const ECRAN = /screen|ecran|tela|monitor|display|lamp|lampe|light|led|neon/i;

// le nom d'une pièce : le premier nom parlant en remontant (les exports nomment souvent le maillage « Object_3 »)
function nomPiece(o) { return o.name || o.parent?.name || ''; }

// ------------------------------------------------------------------ 2. la proposition
// Les axes de l'objet : l'épaisseur (l'axe horizontal le plus court, celui qu'on traverse) et la largeur (l'autre)
export function axes(taille) {
  const epaisseur = taille.x <= taille.z ? 'x' : 'z';
  return { epaisseur, largeur: epaisseur === 'x' ? 'z' : 'x' };
}

const BATTANT = /door|porte|leaf|battant|panel|bars/i, FIXE = /frame|wall|cadre|mur|semi|ring|holder|lock|hydr/i;

export function proposer(ins) {
  const r = { cle: ins.cle, categorie: ins.categorie, mouvements: [], lumieres: ins.lumieres, notes: [] };
  if (ins.animations.length) {
    r.mouvements.push({ type: 'animation', clip: ins.animations[0], mode: ins.categorie === 'porte' ? 'ouvrir' : 'boucle' });
    if (ins.categorie === 'porte') r.notes.push(`animation embarquée « ${ins.animations[0]} » jouée à l'ouverture, à l'envers à la fermeture`);
  }
  if (ins.categorie === 'porte' && !ins.animations.length) r.mouvements.push(...battants(ins, r.notes));
  if (ins.categorie === 'drone') r.mouvements.push({ type: 'vol', pieces: ['*'] });
  if (ins.categorie === 'machine' && !ins.animations.length) r.mouvements.push(...tournants(ins, r.notes));
  if (ins.lumieres.length) r.notes.push(`${ins.lumieres.length} matériau(x) lumineux : ${ins.lumieres.slice(0, 4).join(', ')}`);
  return r;
}

// Les battants d'une porte : par leur nom, sinon par leur forme (une plaque qui couvre une partie de l'ouverture)
function battants(ins, notes) {
  const { largeur } = axes(ins.taille), F = ins.taille, C = ins.centre;
  let choisis = ins.pieces.filter(p => BATTANT.test(p.nom) && !FIXE.test(p.nom));
  if (!choisis.length && ins.pieces.length === 1) {
    choisis = ins.pieces;
    notes.push('une seule pièce : la porte glisse entière (une grille de cellule, sans cadre)');
  }
  if (!choisis.length) {
    choisis = ins.pieces.filter(p => p.taille[largeur] >= 0.2 * F[largeur] && p.taille[largeur] <= 0.75 * F[largeur]
      && p.taille.y >= 0.45 * F.y && Math.abs(p.centre[largeur] - C[largeur]) <= 0.4 * F[largeur]);
    if (choisis.length) notes.push(`battants trouvés par leur forme : ${choisis.map(p => p.nom).join(', ')}`);
  }
  if (!choisis.length) { notes.push('aucun battant trouvé : la porte reste fermée (lumières seulement)'); return []; }
  // les pièces qui se recouvrent dans la largeur forment un même battant (l'avant et l'arrière d'une porte, son
  // mécanisme) ; à gauche du centre, il glisse vers la gauche ; à droite, vers la droite ; au milieu, d'un seul côté
  const groupes = [];
  for (const p of [...choisis].sort((a, b) => a.boite.min[largeur] - b.boite.min[largeur])) {
    const g = groupes.find(g => {
      const recouvre = Math.min(g.max, p.boite.max[largeur]) - Math.max(g.min, p.boite.min[largeur]);
      return recouvre > 0.5 * Math.min(g.max - g.min, p.taille[largeur]);
    });
    if (g) { g.pieces.push(p); g.min = Math.min(g.min, p.boite.min[largeur]); g.max = Math.max(g.max, p.boite.max[largeur]); }
    else groupes.push({ pieces: [p], min: p.boite.min[largeur], max: p.boite.max[largeur] });
  }
  return groupes.map(g => {
    const c = (g.min + g.max) / 2 - C[largeur];
    return { type: 'coulisser', pieces: [...new Set(g.pieces.map(p => p.nom))], sens: c < -0.05 * F[largeur] ? 'moins' : 'plus', course: 0.95 };
  });
}

// Les pièces qui tournent sur elles-mêmes : les anneaux, roues, sphères et champs de force d'une machine ou d'une relique
const TOURNANT = /ring|anneau|circle|ball|sphere|rotor|fan|ventil|turbine|blade|forcefield|wheel|disc/i;
function tournants(ins, notes) {
  let l = ins.pieces.filter(p => TOURNANT.test(p.nom) && !/base|holder|steel/i.test(p.nom));
  if (!l.length && ins.pieces.length === 1) {
    l = ins.pieces;
    notes.push('une seule pièce : elle tourne entière, lentement, autour de son axe court');
  }
  if (!l.length) { notes.push('aucune pièce tournante reconnue : lumières seulement'); return []; }
  return l.map((p, i) => ({ type: 'tourner', pieces: [p.nom], axe: 'court', vitesse: (i % 2 ? -1 : 1) * (ins.pieces.length === 1 ? 6 : 25 + 10 * i) }));
}

// ------------------------------------------------------------------ 3. l'animation
// Chaque pièce mobile est accrochée à un pivot placé à son centre (ou au centre de son groupe), pour tourner sur
// elle-même et glisser dans le repère de l'objet. ouverture : de 0 (fermée) à 1 (ouverte).
export function animer(racine, recette, animations, ins) {
  const mixeur = animations.length ? new THREE.AnimationMixer(racine) : null;
  const parNom = n => n === '*' ? [racine] : ins.pieces.filter(p => p.nom === n).map(p => p.noeud);
  const { largeur } = axes(ins.taille);
  const mobiles = [];
  for (const mv of recette.mouvements) {
    if (mv.type === 'animation') {
      const clip = animations.find(a => a.name === mv.clip);
      const action = mixeur.clipAction(clip); action.play(); action.paused = true;
      mobiles.push({ mv, action, duree: clip.duration });
      continue;
    }
    const noeuds = mv.pieces.flatMap(parNom);
    if (!noeuds.length) continue;
    if (mv.type === 'vol') { mobiles.push({ mv, noeuds, depart: racine.position.clone(), cap: racine.rotation.y }); continue; }
    const boites = noeuds.map(n => ins.pieces.find(p => p.noeud === n)?.boite).filter(Boolean);
    const b = boites.reduce((a, x) => a.union(x), new THREE.Box3()), taille = b.getSize(v());
    // l'axe court d'une pièce qui tourne : celui de sa plus petite dimension (un anneau tourne dans son plan)
    let axe = null, pas = null;
    if (mv.type === 'tourner') { const k = ['x', 'y', 'z'].reduce((a, c) => taille[c] < taille[a] ? c : a, 'x'); axe = new THREE.Vector3(); axe[k] = 1; }
    const repos = noeuds.map(n => ({ n, p: n.position.clone(), q: n.quaternion.clone(), parent: n.parent }));
    const mob = { mv, noeuds, repos, centre: b.getCenter(v()), taille, axe, angle: 0, vitesse: 0 };
    // le glissement d'un battant : vers un côté (moins, plus : le long de la largeur) ou vers le haut, de sa propre taille
    mob.calculerPas = () => {
      mob.pas = new THREE.Vector3();
      if (mv.sens === 'haut') mob.pas.y = taille.y * mv.course;
      else mob.pas[largeur] = (mv.sens === 'moins' ? -1 : 1) * taille[largeur] * mv.course;
    };
    if (mv.type === 'coulisser') mob.calculerPas();
    mobiles.push(mob);
  }
  // les matériaux lumineux, avec leur lumière de départ
  const lumineux = [];
  racine.traverse(o => { if (o.isMesh) for (const m of [].concat(o.material)) if (recette.lumieres.includes(m.name || '(sans nom)') && !lumineux.some(x => x.m === m)) lumineux.push({ m, couleur: m.emissive.clone(), intensite: m.emissiveIntensity }); });

  let ouverture = 0, temps = 0, panneOuverture = 0.35 + Math.random() * 0.2;
  const inverse = new THREE.Matrix4(), m4 = new THREE.Matrix4(), q = new THREE.Quaternion();
  // place une pièce : ses transformations sont faites dans le repère de la racine, autour du centre du groupe
  function placer(mob, rot, decal) {
    racine.updateMatrixWorld(true);
    for (const r of mob.repos) {
      // la pose de repos dans le repère de la racine
      r.parent.updateMatrixWorld(true);
      inverse.copy(racine.matrixWorld).invert();
      const enRacine = new THREE.Matrix4().multiplyMatrices(inverse, r.parent.matrixWorld).multiply(new THREE.Matrix4().compose(r.p, r.q, new THREE.Vector3(1, 1, 1).copy(r.n.scale)));
      // rotation autour du centre, puis glissement
      const t = new THREE.Matrix4().makeTranslation(mob.centre.x, mob.centre.y, mob.centre.z);
      const ti = new THREE.Matrix4().makeTranslation(-mob.centre.x, -mob.centre.y, -mob.centre.z);
      const rm = rot ? new THREE.Matrix4().makeRotationAxis(mob.axe, rot) : new THREE.Matrix4();
      const dm = decal ? new THREE.Matrix4().makeTranslation(decal.x, decal.y, decal.z) : new THREE.Matrix4();
      const voulu = dm.multiply(t).multiply(rm).multiply(ti).multiply(enRacine);
      // de retour dans le repère du parent
      m4.multiplyMatrices(inverse.copy(r.parent.matrixWorld).invert().multiply(racine.matrixWorld), voulu);
      const s = new THREE.Vector3(); m4.decompose(r.n.position, r.n.quaternion, s);
    }
  }

  return {
    mobiles, lumineux,
    get ouverture() { return ouverture; },
    // dt : secondes ; etat : normal, alarme, panne ; proche : quelqu'un devant la porte
    maj(dt, etat, proche, forcer = null) {
      temps += dt;
      // la porte : ouverte si quelqu'un approche ; verrouillée en alarme ; bloquée à demi, par à-coups, en panne
      let voulue = proche && etat !== 'alarme' ? 1 : 0;
      if (etat === 'panne') voulue = proche ? panneOuverture + 0.04 * Math.sign(Math.sin(temps * 9)) : panneOuverture;
      const vitesse = etat === 'panne' ? 0.25 : 0.9;
      if (forcer !== null) ouverture = forcer;
      else ouverture += Math.max(-vitesse * dt, Math.min(vitesse * dt, voulue - ouverture));
      const o = ouverture * ouverture * (3 - 2 * ouverture);
      for (const mob of mobiles) {
        const t = mob.mv.type;
        if (t === 'animation') {
          // une porte : de l'instant le plus fermé à l'instant le plus ouvert de son animation (le bot les a trouvés)
          const a = mob.mv.debut ?? 0, b = mob.mv.fin ?? 1;
          mob.action.time = mob.mv.mode === 'ouvrir' ? (a + (b - a) * o) * mob.duree * 0.999 : (temps * (etat === 'panne' ? 0.3 : etat === 'alarme' ? 1.6 : 1)) % mob.duree;
        } else if (t === 'coulisser') placer(mob, 0, mob.pas.clone().multiplyScalar(o));
        else if (t === 'tourner') {
          // la vitesse de croisière ; en panne, la machine ralentit jusqu'à presque s'arrêter, avec de l'inertie
          const cible = THREE.MathUtils.degToRad(mob.mv.vitesse) * (etat === 'alarme' ? 1.8 : etat === 'panne' ? 0.08 : 1);
          mob.vitesse += (cible - mob.vitesse) * (1 - Math.exp(-dt / (etat === 'panne' ? 2.5 : 0.8)));
          mob.angle += mob.vitesse * dt;
          placer(mob, mob.angle, null);
        } else if (t === 'vol') {
          // le drone : il flotte, balaie la salle du regard ; en alarme il balaie vite ; en panne il tombe et s'incline
          const n = racine, h = ins.taille.y;
          const chute = etat === 'panne' ? Math.min(1, (mob.chute = (mob.chute || 0) + dt * 0.8)) : (mob.chute = Math.max(0, (mob.chute || 0) - dt));
          n.position.y = mob.depart.y + (1 - chute) * (0.05 * h * Math.sin(temps * 2.1)) - chute * mob.depart.y * 0.98;
          n.rotation.y = mob.cap + (etat === 'alarme' ? 0.9 * Math.sin(temps * 2.4) : 0.5 * Math.sin(temps * 0.5));
          n.rotation.z = chute * 0.5 + (1 - chute) * 0.04 * Math.sin(temps * 1.3);
        }
      }
      if (mixeur) mixeur.update(0);
      // les lumières : stables (un léger souffle) ; en alarme, rouges et battantes ; en panne, elles grésillent
      for (const l of lumineux) {
        let f = 1 + 0.03 * Math.sin(temps * 13.1);
        l.m.emissive.copy(l.couleur);
        if (etat === 'alarme') { const b = 0.5 + 0.5 * Math.sin(temps * Math.PI * 2 * 0.9); l.m.emissive.lerp(ROUGE, 0.85); f = 0.25 + 1.6 * b; }
        if (etat === 'panne') { const k = Math.floor(temps * 12); const hasard = Math.abs(Math.sin(k * 12.9898) * 43758.5453) % 1; f = hasard < 0.55 ? 0.05 : hasard < 0.8 ? 0.5 : 1.1; }
        l.m.emissiveIntensity = l.intensite * f;
      }
    },
  };
}

// ------------------------------------------------------------------ 4. le contrôle
// Le passage laissé par une porte ouverte : la plus grande bande libre de la largeur de l'ouverture, à mi-hauteur
// (en mètres). Une personne passe à partir de 0,8 m.
export function controler(racine, recette, ctrl, ins) {
  const rapport = { cle: ins.cle, categorie: ins.categorie, mouvements: recette.mouvements.map(m => m.type), notes: recette.notes.slice() };
  const { largeur, epaisseur } = axes(ins.taille);
  if (ins.categorie === 'porte') {
    // des rayons à travers la porte, dans son épaisseur, tous les 3 cm de sa largeur, à trois hauteurs (jusqu'à 2 m) :
    // un endroit est libre quand aucun rayon ne touche rien (cadre, battants, mécanismes)
    const echelle = racine.getWorldScale(v()).x, B = ins.boite;
    const rayon = new THREE.Raycaster();
    const mesure = ouv => {
      ctrl.maj(0, 'normal', false, ouv);
      racine.updateMatrixWorld(true);
      const pasL = 0.03 / echelle, H = Math.min(B.max.y - B.min.y, 2.1 / echelle), marge = 0.2 / echelle;
      // un meuble (armoire, casier) a un fond : on regarde seulement si sa façade est dégagée, d'un côté ou de l'autre
      const facade = /armoire|casier/.test(ins.cle), prof = B.max[epaisseur] - B.min[epaisseur];
      let meilleur = 0, courant = 0;
      for (let x = B.min[largeur]; x <= B.max[largeur]; x += pasL) {
        // à hauteur de passage : de 30 à 80 % (on enjambe un seuil, on ne se cogne pas la tête)
        const traverse = cote => [0.3, 0.55, 0.8].every(f => {
          const o = v(); o[largeur] = x; o.y = B.min.y + f * H; o[epaisseur] = cote > 0 ? B.min[epaisseur] - marge : B.max[epaisseur] + marge;
          const d = v(); d[epaisseur] = cote;
          rayon.set(racine.localToWorld(o), d.transformDirection(racine.matrixWorld));
          rayon.far = (facade ? marge + 0.35 * prof : prof + 2 * marge) * echelle;
          return !rayon.intersectObject(racine, true).length;
        });
        const libre = facade ? traverse(1) || traverse(-1) : traverse(1);
        courant = libre ? courant + pasL : 0;
        meilleur = Math.max(meilleur, courant);
      }
      return meilleur * echelle;
    };
    // une animation embarquée : le bot cherche l'instant le plus fermé et le plus ouvert (beaucoup de portes de
    // Sketchfab commencent ouvertes, ou s'ouvrent puis se referment dans la même boucle)
    for (const mob of ctrl.mobiles.filter(m => m.mv.type === 'animation' && m.mv.mode === 'ouvrir')) {
      const passages = [];
      for (let k = 0; k <= 24; k++) { mob.mv.debut = mob.mv.fin = k / 24; passages.push(mesure(0)); }
      const mini = Math.min(...passages), maxi = Math.max(...passages);
      mob.mv.debut = passages.indexOf(mini) / 24;
      mob.mv.fin = passages.indexOf(maxi) / 24;
      rapport.notes.push(`dans son animation, la porte est la plus fermée à ${Math.round(mob.mv.debut * 100)} % et la plus ouverte à ${Math.round(mob.mv.fin * 100)} %`);
    }
    // des battants qui coulissent : le bot essaie chaque sens (vers un côté, l'autre, le haut) et garde le plus large passage
    const glissants = ctrl.mobiles.filter(m => m.mv.type === 'coulisser');
    if (glissants.length && glissants.length <= 3) {
      const SENS = ['moins', 'plus', 'haut'];
      let meilleur = { passage: -1 }, essais = 0;
      const depart = glissants.map(m => m.mv.sens);
      for (let k = 0; k < SENS.length ** glissants.length; k++) {
        glissants.forEach((m, i) => { m.mv.sens = SENS[Math.floor(k / SENS.length ** i) % SENS.length]; m.calculerPas(); });
        const p = mesure(1); essais++;
        // le sens naturel (sur les côtés) est gardé, sauf si un autre ouvre nettement plus (15 %)
        const naturel = glissants.every((m, i) => m.mv.sens === depart[i]);
        const garde = naturel ? p >= meilleur.passage / 1.15 : (meilleur.naturel ? p > meilleur.passage * 1.15 : p > meilleur.passage);
        if (garde) meilleur = { passage: p, sens: glissants.map(m => m.mv.sens), naturel };
      }
      glissants.forEach((m, i) => { m.mv.sens = meilleur.sens[i]; m.calculerPas(); });
      rapport.notes.push(`${essais} façons d'ouvrir essayées ; gardée : ${meilleur.sens.join(', ')}`);
    }
    if (ctrl.mobiles.length) {
      rapport.passageFerme = +mesure(0).toFixed(2);
      rapport.passageOuvert = +mesure(1).toFixed(2);
      // une personne passe de face à partir de 0,8 m, de profil (une écoutille, un par un) à partir de 0,6 m ;
      // la façade d'un meuble est ouverte à partir de 0,35 m
      const facade = /armoire|casier/.test(ins.cle);
      rapport.verdict = facade ? (rapport.passageOuvert >= 0.35 ? 'ok' : 'trop étroit')
        : rapport.passageOuvert >= 0.8 ? 'ok' : rapport.passageOuvert >= 0.6 ? 'étroit' : 'trop étroit';
      if (rapport.verdict === 'étroit') rapport.notes.push('étroit : on y passe un par un, de profil');
      if (rapport.passageFerme > 0.3) rapport.notes.push(`fermée, elle laisse déjà ${rapport.passageFerme} m libres`);
      mesure(0);
    } else rapport.verdict = 'fixe';
  }
  if (ctrl.mobiles.some(m => m.mv.type === 'tourner')) {
    // une pièce qui tourne doit rester en place : son centre ne bouge pas de plus de 1 % de sa taille
    let pire = 0;
    for (const mob of ctrl.mobiles.filter(m => m.mv.type === 'tourner')) {
      const centre = () => new THREE.Box3().setFromObject(mob.noeuds[0]).getCenter(v());
      const c0 = centre(), t = new THREE.Box3().setFromObject(mob.noeuds[0]).getSize(v()).length();
      for (const a of [0.7, 1.9, 3.1]) {
        mob.angle = a; ctrl.maj(0, 'normal', false);
        pire = Math.max(pire, centre().distanceTo(c0) / Math.max(t, 1e-6));
      }
    }
    rapport.derive = +(pire * 100).toFixed(1);
    rapport.verdict = rapport.verdict || (pire < 0.05 ? 'ok' : 'pivot décalé');
  }
  rapport.verdict = rapport.verdict || (recette.mouvements.length || recette.lumieres.length ? 'ok' : 'immobile');
  return rapport;
}
