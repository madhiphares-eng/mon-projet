// Échos d'Éther : la prise des armes. Les mains vont exactement sur l'arme, quel que soit le personnage.
//
// C'est l'arme qui commande, et les mains viennent sur elle :
//   - en visée (mode « épaule »), la crosse est posée au creux de l'épaule droite, le canon pointe devant le personnage ;
//   - arme basse (mode « main »), l'arme suit la main droite de l'animation ;
//   - puis les deux bras se replient par cinématique inverse (deux os, le coude garde sa direction), et chaque paume
//     se pose sur sa prise : la droite sur la poignée, la gauche sous le garde-main (elle y glisse si le bras est court).
// La paume de chaque personnage est mesurée sur son squelette (os de la main et des doigts) : rien à régler à la main.
// Le « bot » (calibrer, mesurer) cherche pour chaque personnage le meilleur appui d'épaule et contrôle chaque pose.
//
//   import { Prise, fabriquerArme, tenuePour } from './prise-armes.js';
//   const prise = new Prise(carte, racine);            // carte : les os (bras_d, avantbras_d, main_d, majeur1_d…)
//   const arme = fabriquerArme('fusil'); scene.add(arme);
//   chaque image, après l'animation : prise.tenir(arme, tenuePour(nomDuClip), progression);
import * as THREE from 'three';

const v = () => new THREE.Vector3(), q = () => new THREE.Quaternion();
const lisse = (a, b, x) => { const t = Math.min(1, Math.max(0, (x - a) / (b - a))); return t * t * (3 - 2 * t); };
const HAUT = new THREE.Vector3(0, 1, 0);
const LONGUEUR_BRAS_MANNEQUIN = 0.547;      // épaule → poignet du mannequin du studio (1,83 m)

// ------------------------------------------------------------------ les armes
// Repère de l'arme : z vers la bouche, y vers le haut, x vers la gauche. En mètres.
// Une prise : le centre de la paume, la direction des doigts (du poignet aux jointures), la normale de la paume.
export const ARMES = {
  fusil: {
    nom: "Fusil de l'Empire",
    pieces: [   // largeur, hauteur, longueur, x, y, z, matière, inclinaison (radians, autour de x)
      [0.034, 0.105, 0.042, 0, -0.058, -0.005, 'noir', -0.32],     // la poignée
      [0.05, 0.075, 0.34, 0, 0.03, 0.06, 'acier', 0],              // la carcasse
      [0.04, 0.085, 0.27, 0, 0.012, -0.245, 'noir', 0],            // la crosse
      [0.052, 0.056, 0.26, 0, 0.025, 0.36, 'noir', 0],             // le garde-main
      [0.03, 0.15, 0.06, 0, -0.07, 0.12, 'acier', 0.25],           // le chargeur
      [0.02, 0.026, 0.06, 0, 0.081, 0.03, 'noir', 0],              // le viseur
      [0.012, 0.012, 0.012, 0, 0.098, 0.05, 'rouge', 0],           // sa diode (la seule touche de rouge)
    ],
    canon: { rayon: 0.011, longueur: 0.24, centre: [0, 0.03, 0.61] },
    prises: {
      droite: { centre: [-0.03, -0.045, -0.012], doigts: [0, -0.55, 0.83], paume: [1, 0, 0] },
      // la main gauche glisse du garde-main (0,45) jusqu'au puits du chargeur (0,13), comme les vrais tireurs
      gauche: { centre: [0.014, -0.024, 0.31], doigts: [-0.72, 0.12, 0.68], paume: [-0.3, 0.95, 0], rail: [0.13, 0.45] },
    },
    crosse: [0, 0.02, -0.38],
    bouche: [0, 0.03, 0.73],
  },
  pistolet: {
    nom: "Pistolet de l'Empire",
    pieces: [
      [0.03, 0.1, 0.04, 0, -0.05, 0, 'noir', -0.28],
      [0.032, 0.04, 0.19, 0, 0.018, 0.05, 'acier', 0],
      [0.008, 0.008, 0.008, 0, 0.042, 0.13, 'rouge', 0],
    ],
    canon: { rayon: 0.008, longueur: 0.04, centre: [0, 0.018, 0.16] },
    prises: {
      droite: { centre: [-0.028, -0.04, -0.012], doigts: [0, -0.55, 0.83], paume: [1, 0, 0] },
      // la main gauche enveloppe la droite, seulement si l'animation l'approche (tir à deux mains)
      gauche: { centre: [0.03, -0.06, -0.005], doigts: [-0.4, -0.55, 0.73], paume: [-0.95, 0.15, 0], facultative: 0.14, rail: [-0.07, 0.01] },
    },
    crosse: [0, 0, -0.03],
    bouche: [0, 0.018, 0.18],
  },
};

const MATIERES = {};
function matiere(nom) {
  if (!MATIERES[nom]) MATIERES[nom] = nom === 'acier' ? new THREE.MeshStandardMaterial({ color: 0x2c3036, metalness: 0.75, roughness: 0.42 })
    : nom === 'rouge' ? new THREE.MeshStandardMaterial({ color: 0x8a1c1c, emissive: 0x6a1010, roughness: 0.5 })
    : new THREE.MeshStandardMaterial({ color: 0x16181b, metalness: 0.3, roughness: 0.65 });
  return MATIERES[nom];
}
export function fabriquerArme(nom) {
  const def = ARMES[nom], g = new THREE.Group();
  for (const [l, h, p, x, y, z, m, rx] of def.pieces) {
    const b = new THREE.Mesh(new THREE.BoxGeometry(l, h, p), matiere(m));
    b.position.set(x, y, z); b.rotation.x = rx; g.add(b);
  }
  const c = new THREE.Mesh(new THREE.CylinderGeometry(def.canon.rayon, def.canon.rayon, def.canon.longueur, 12), matiere('acier'));
  c.rotation.x = Math.PI / 2; c.position.fromArray(def.canon.centre); g.add(c);
  g.userData.def = def; g.userData.nom = nom;
  g.userData.bouche = new THREE.Vector3().fromArray(def.bouche);
  return g;
}

// ------------------------------------------------------------------ les tenues : comment chaque animation tient l'arme
// mode : 'epaule' (crosse à l'épaule, canon devant), 'main' (l'arme suit la main droite, orientée par les deux mains)
//        ou 'attache' (pistolet : le canon dans l'axe de l'avant-bras droit, la main tourne autour du poignet pour le tenir)
// gauche(u) : le poids de la main gauche sur l'arme, selon la progression u (0 à 1) de l'animation
// tangage(u) : en visée, de combien le canon se relève (degrés)
export const TENUES = {
  Fusil_Visee: { arme: 'fusil', mode: 'epaule' },
  Fusil_Marche_Visee: { arme: 'fusil', mode: 'epaule' },
  // le recul de la rafale du studio : 5 coups à 8 par seconde, le canon se relève de 5° et redescend
  Fusil_Tir: { arme: 'fusil', mode: 'epaule', tangage: u => { const k = (u * 8) % 1; return Math.floor(u * 8) < 5 ? 4.9 * Math.exp(-k * 8) : 0; } },
  Fusil_Accroupi_Visee: { arme: 'fusil', mode: 'epaule' },
  Fusil_Repos: { arme: 'fusil', mode: 'main' },
  Fusil_Marche: { arme: 'fusil', mode: 'main' },
  Fusil_Course: { arme: 'fusil', mode: 'main' },
  Fusil_Rechargement: { arme: 'fusil', mode: 'main', gauche: u => 1 - lisse(0.1, 0.17, u) + lisse(0.81, 0.92, u) },
  Pistol_Idle_Loop: { arme: 'pistolet', mode: 'attache' },
  Pistol_Aim_Neutral: { arme: 'pistolet', mode: 'attache' },
  Pistol_Aim_Up: { arme: 'pistolet', mode: 'attache' },
  Pistol_Aim_Down: { arme: 'pistolet', mode: 'attache' },
  Pistol_Shoot: { arme: 'pistolet', mode: 'attache' },
  Pistol_Reload: { arme: 'pistolet', mode: 'attache', gauche: () => 0 },
};
export const tenuePour = nom => TENUES[nom] || null;

// ------------------------------------------------------------------ les outils de la cinématique inverse
function tournerMonde(os, dq) {
  // applique une rotation (repère du monde) à un os, autour de sa tête
  const pw = os.parent.getWorldQuaternion(q());
  os.quaternion.premultiply(pw.clone().invert().multiply(dq).multiply(pw));
  os.updateMatrixWorld(true);
}
function orienterMonde(os, cible) {
  const pw = os.parent.getWorldQuaternion(q());
  os.quaternion.copy(pw.invert().multiply(cible));
  os.updateMatrixWorld(true);
}
function base(a, b) {
  // le repère orthonormé (a, b', a × b'), en quaternion : a est gardé, b est redressé
  const x = a.clone().normalize(), y = b.clone().sub(x.clone().multiplyScalar(b.dot(x))).normalize(), z = v().crossVectors(x, y);
  return q().setFromRotationMatrix(new THREE.Matrix4().makeBasis(x, y, z));
}
// cinématique inverse à deux os : le poignet va sur la cible, le coude du côté du pôle
function ik2(haut, milieu, bout, cible, pole) {
  const S = haut.getWorldPosition(v()), E = milieu.getWorldPosition(v()), W = bout.getWorldPosition(v());
  const a = S.distanceTo(E), b = E.distanceTo(W);
  const vers = cible.clone().sub(S), dist = vers.length();
  const d = Math.min(Math.max(dist, Math.abs(a - b) + 1e-4), a + b - 1e-4);
  vers.normalize();
  const x = (a * a - b * b + d * d) / (2 * d), h = Math.sqrt(Math.max(a * a - x * x, 0));
  const p = pole.clone().sub(S); p.addScaledVector(vers, -p.dot(vers));
  if (p.lengthSq() < 1e-10) p.copy(HAUT).addScaledVector(vers, -HAUT.dot(vers));
  p.normalize();
  const E2 = S.clone().addScaledVector(vers, x).addScaledVector(p, h);
  tournerMonde(haut, q().setFromUnitVectors(E.clone().sub(S).normalize(), E2.clone().sub(S).normalize()));
  const E3 = milieu.getWorldPosition(v()), W3 = bout.getWorldPosition(v());
  tournerMonde(milieu, q().setFromUnitVectors(W3.sub(E3).normalize(), S.clone().addScaledVector(vers, d).sub(E3).normalize()));
  return { etirement: dist / (a + b), coude: Math.acos(Math.max(-1, Math.min(1, (a * a + b * b - d * d) / (2 * a * b)))) };
}

// ------------------------------------------------------------------ la prise d'un personnage
export class Prise {
  // à créer le personnage au repos (ou debout, de face) : on y mesure ses mains et la direction de son regard
  constructor(carte, racine) {
    this.carte = carte; this.racine = racine;
    const droite = carte.cuisse_d && carte.cuisse_g ? carte.cuisse_d.getWorldPosition(v()).sub(carte.cuisse_g.getWorldPosition(v())) : v(-1, 0, 0);
    const face = v().crossVectors(HAUT, droite).setY(0);
    this.devantLocal = (face.lengthSq() > 1e-8 ? face.normalize() : v(0, 0, 1)).applyQuaternion(racine.getWorldQuaternion(q()).invert());
    this.mains = { d: this.calibrerMain('d'), g: this.calibrerMain('g') };
    const S = carte.bras_d.getWorldPosition(v()), E = carte.avantbras_d.getWorldPosition(v()), W = carte.main_d.getWorldPosition(v());
    this.longueurBras = S.distanceTo(E) + E.distanceTo(W);
    this.k = this.longueurBras / LONGUEUR_BRAS_MANNEQUIN;          // l'échelle du personnage, pour les décalages
    this.appui = { gauche: 0.055, avant: 0.06, bas: 0.01 };      // l'appui de la crosse, par rapport à l'épaule (mannequin)
    this.memoire = null;                                         // l'arme vue depuis la main droite (pour le rechargement)
  }

  // la paume d'une main : son centre, la direction des doigts et sa normale, dans le repère de l'os de la main
  // (les têtes des os des doigts sont fixes par rapport à la main : la mesure ne dépend pas de la pose)
  calibrerMain(c) {
    const C = this.carte, main = C['main_' + c];
    if (!main) return null;
    main.updateMatrixWorld(true);
    const pM = main.getWorldPosition(v());
    const majeur = C['majeur1_' + c] || C['annulaire1_' + c] || C['index1_' + c];
    let doigts, longueur;
    if (majeur) { const pj = majeur.getWorldPosition(v()); doigts = pj.clone().sub(pM); longueur = doigts.length(); doigts.normalize(); }
    else { doigts = pM.clone().sub(C['avantbras_' + c].getWorldPosition(v())); longueur = doigts.length() * 0.35; doigts.normalize(); }
    let normale = null;
    const index = C['index1_' + c], petit = C['auriculaire1_' + c] || C['annulaire1_' + c];
    if (index && petit && index !== petit) {
      const travers = index.getWorldPosition(v()).sub(petit.getWorldPosition(v())).normalize();   // vers le pouce
      normale = c === 'd' ? v().crossVectors(travers, doigts) : v().crossVectors(doigts, travers);
      normale.addScaledVector(doigts, -normale.dot(doigts)).normalize();
    }
    const qInv = main.getWorldQuaternion(q()).invert();
    const centre = pM.clone().lerp(pM.clone().addScaledVector(doigts, longueur), 0.5);
    if (normale) centre.addScaledVector(normale, longueur * 0.12);
    return {
      centre: centre.applyMatrix4(main.matrixWorld.clone().invert()),       // en coordonnées de l'os (échelle comprise)
      doigts: doigts.applyQuaternion(qInv),
      normale: normale ? normale.applyQuaternion(qInv) : null,
      longueur,
    };
  }

  // où est la paume d'une main, maintenant
  paume(c) {
    const m = this.mains[c], main = this.carte['main_' + c];
    return m.centre.clone().applyMatrix4(main.matrixWorld);
  }

  // le repère de l'arme, et ses points en coordonnées du monde
  versMonde(arme, local) { return v().fromArray(local).applyMatrix4(arme.matrixWorld); }

  // l'orientation que doit prendre l'os de la main pour que sa paume épouse la prise
  rotationMain(c, arme, prise) {
    const m = this.mains[c];
    const qa = arme.getWorldQuaternion(q());
    const doigts = v().fromArray(prise.doigts).normalize().applyQuaternion(qa);
    if (!m.normale) return q().setFromUnitVectors(m.doigts.clone().applyQuaternion(this.carte['main_' + c].getWorldQuaternion(q())), doigts)
      .multiply(this.carte['main_' + c].getWorldQuaternion(q()));
    const paume = v().fromArray(prise.paume).normalize().applyQuaternion(qa);
    // R × (doigts, normale) locaux = (doigts, paume) voulus
    return base(doigts, paume).multiply(base(m.doigts, m.normale).invert());
  }

  // poser une main sur une prise : rotation de la main, puis bras par cinématique inverse (poignet = paume - décalage)
  poser(c, arme, prise, z = null) {
    const C = this.carte, main = C['main_' + c], m = this.mains[c];
    const local = prise.centre.slice();
    if (z !== null) local[2] = z;
    const cible = this.versMonde(arme, local);
    const rot = this.rotationMain(c, arme, prise);
    const echelle = main.getWorldScale(v()).x;
    const poignet = cible.clone().sub(m.centre.clone().multiplyScalar(echelle).applyQuaternion(rot));
    const pole = C['avantbras_' + c].getWorldPosition(v());
    const r = ik2(C['bras_' + c], C['avantbras_' + c], main, poignet, pole);
    orienterMonde(main, rot);
    r.ecart = this.paume(c).distanceTo(cible);
    return r;
  }

  // l'état d'un bras, sans le bouger
  bras(c) {
    const C = this.carte, S = C['bras_' + c].getWorldPosition(v()), E = C['avantbras_' + c].getWorldPosition(v()), W = C['main_' + c].getWorldPosition(v());
    const a = S.distanceTo(E), b = E.distanceTo(W), d = S.distanceTo(W);
    return { ecart: 0, etirement: d / (a + b), coude: Math.acos(Math.max(-1, Math.min(1, (a * a + b * b - d * d) / (2 * a * b)))) };
  }

  // la main gauche glisse le long du rail du garde-main si le bras ne l'atteint pas
  poserGauche(arme, prise) {
    if (!prise.rail) return this.poser('g', arme, prise);
    const C = this.carte, S = C.bras_g.getWorldPosition(v());
    const portee = this.longueurBras * 0.97;
    let meilleur = prise.centre[2], ecart = Infinity;
    for (let i = 0; i <= 12; i++) {
      const z = prise.rail[0] + (prise.rail[1] - prise.rail[0]) * i / 12;
      const d = this.versMonde(arme, [prise.centre[0], prise.centre[1], z]).distanceTo(S);
      const trop = Math.max(0, d - portee) * 10 + Math.abs(z - prise.centre[2]);
      if (trop < ecart) { ecart = trop; meilleur = z; }
    }
    const r = this.poser('g', arme, prise, meilleur);
    r.glisse = meilleur - prise.centre[2];
    return r;
  }

  // la distance d'un point au torse (une colonne de section ovale, de la hanche au cou) : < 0 dedans
  torse() {
    const C = this.carte;
    const hanches = C.hanches.getWorldPosition(v()), cou = (C.cou || C.tete).getWorldPosition(v());
    const travers = C.bras_g.getWorldPosition(v()).sub(C.bras_d.getWorldPosition(v()));
    const largeur = travers.length(); travers.normalize();
    const face = this.devant(), axe = cou.clone().sub(hanches);
    const ra = largeur * 0.36, rb = largeur * 0.28;
    return p => {
      const t = Math.max(0, Math.min(1, p.clone().sub(hanches).dot(axe) / axe.lengthSq()));
      const r = p.clone().sub(hanches.clone().addScaledVector(axe, t));
      return (Math.hypot(r.dot(travers) / ra, r.dot(face) / rb) - 1) * rb;
    };
  }

  // la plus petite distance au torse de l'arme, de la poignée à la bouche (la crosse, elle, doit toucher l'épaule)
  distanceArme(arme, torse = this.torse()) {
    const def = arme.userData.def;
    const poignee = this.versMonde(arme, [0, def.bouche[1], def.prises.droite.centre[2]]), bouche = this.versMonde(arme, def.bouche);
    let d = Infinity;
    for (let i = 0; i <= 10; i++) d = Math.min(d, torse(poignee.clone().lerp(bouche, i / 10)));
    return d;
  }

  // l'avant du personnage, à plat
  devant() {
    const f = this.devantLocal.clone().applyQuaternion(this.racine.getWorldQuaternion(q()));
    f.y = 0;
    return f.lengthSq() > 1e-8 ? f.normalize() : v(0, 0, 1);
  }

  // ---------------------------------------------------------------- tenir l'arme, à chaque image (après l'animation)
  tenir(arme, tenue, u = 0, appui = this.appui) {
    const def = arme.userData.def, C = this.carte;
    if (!tenue || !this.mains.d) return null;
    const Rp = this.paume('d');
    const poidsGauche = tenue.gauche ? tenue.gauche(u) : 1;
    let d;
    // le canon : en visée, droit devant le personnage (relevé par le recul s'il y en a) ;
    // arme basse, la direction des deux mains de l'animation
    const Lp = this.mains.g ? this.paume('g') : Rp.clone().add(this.devant());
    const brut = Lp.clone().sub(Rp).normalize();
    if (tenue.mode === 'epaule') {
      const tangage = (tenue.tangage ? tenue.tangage(u) : 0) * Math.PI / 180;
      d = this.devant().multiplyScalar(Math.cos(tangage)).addScaledVector(HAUT, Math.sin(tangage));
    } else d = brut;
    const ecartPrises = v().fromArray(def.prises.gauche.centre).sub(v().fromArray(def.prises.droite.centre));
    let u2 = HAUT.clone(), l = v();
    for (let i = 0; i < 3 && tenue.mode !== 'epaule'; i++) {
      // corriger la direction : les deux paumes ne sont pas exactement sur l'axe du canon
      l.crossVectors(u2, d).normalize(); u2.crossVectors(d, l).normalize();
      const travers = l.clone().multiplyScalar(ecartPrises.x).addScaledVector(u2, ecartPrises.y);
      d = Lp.clone().sub(Rp).sub(travers).normalize();
    }
    l.crossVectors(HAUT, d).normalize(); u2.crossVectors(d, l).normalize();
    let qArme = q().setFromRotationMatrix(new THREE.Matrix4().makeBasis(l, u2, d));
    if (tenue.mode === 'attache') {
      // le canon dans l'axe de l'avant-bras (bras tendu ou plié, c'est là que le pistolet vise), les organes de visée en haut
      d = C.main_d.getWorldPosition(v()).sub(C.avantbras_d.getWorldPosition(v())).normalize();
      l.crossVectors(HAUT, d).normalize(); u2.crossVectors(d, l).normalize();
      qArme = q().setFromRotationMatrix(new THREE.Matrix4().makeBasis(l, u2, d));
    }

    if (tenue.mode === 'epaule') {
      // la crosse au creux de l'épaule droite, puis les deux mains
      const S = C.bras_d.getWorldPosition(v()), k = this.k;
      const creux = S.addScaledVector(l, appui.gauche * k).addScaledVector(d, appui.avant * k).addScaledVector(u2, -appui.bas * k);
      arme.quaternion.copy(qArme);
      arme.position.copy(creux).sub(v().fromArray(def.crosse).applyQuaternion(qArme));
      arme.updateMatrixWorld(true);
      this.memoire = null;
    } else if (tenue.mode === 'attache') {
      // la main tourne autour du poignet pour épouser la poignée, puis l'arme se pose dans sa paume
      arme.quaternion.copy(qArme);
      arme.updateMatrixWorld(true);
      orienterMonde(C.main_d, this.rotationMain('d', arme, def.prises.droite));
      arme.position.copy(this.paume('d')).sub(v().fromArray(def.prises.droite.centre).applyQuaternion(qArme));
      arme.updateMatrixWorld(true);
    } else {
      // l'arme suit la main droite ; pendant le rechargement, elle garde son orientation par rapport à la main
      const qMain = C.main_d.getWorldQuaternion(q());
      if (poidsGauche > 0.99 || !this.memoire) this.memoire = qMain.clone().invert().multiply(qArme);
      const qVoulue = poidsGauche > 0.99 ? qArme : qMain.clone().multiply(this.memoire);
      arme.quaternion.copy(qVoulue);
      arme.position.copy(Rp).sub(v().fromArray(def.prises.droite.centre).applyQuaternion(qVoulue));
      arme.updateMatrixWorld(true);
      // l'arme ne rentre pas dans le ventre : si elle le frôle, on l'écarte du corps (les mains suivront)
      const marge = 0.03 * this.k, dedans = this.distanceArme(arme);
      if (dedans < marge) { arme.position.addScaledVector(this.devant(), marge - dedans); arme.updateMatrixWorld(true); }
    }
    const droite = tenue.mode === 'attache' ? this.bras('d') : this.poser('d', arme, def.prises.droite);
    if (tenue.mode === 'main' && droite.etirement > 0.995) {
      // la main droite n'a pas pu suivre jusqu'au bout : on recale l'arme sur sa paume
      const qv = arme.quaternion.clone();
      arme.position.copy(this.paume('d')).sub(v().fromArray(def.prises.droite.centre).applyQuaternion(qv));
      arme.updateMatrixWorld(true);
      droite.ecart = 0;
    }
    let gauche = null;
    const pg = def.prises.gauche;
    const proche = !pg.facultative || this.paume('g').distanceTo(this.versMonde(arme, pg.centre)) < pg.facultative * this.k;
    if (this.mains.g && poidsGauche > 0.01 && proche) {
      if (poidsGauche > 0.99) gauche = this.poserGauche(arme, pg);
      else {
        // fondu : on mélange la pose de l'animation et la pose sur l'arme
        const os = ['bras_g', 'avantbras_g', 'main_g'].map(n => C[n]);
        const avant = os.map(o => o.quaternion.clone());
        gauche = this.poserGauche(arme, pg);
        os.forEach((o, i) => { o.quaternion.copy(avant[i]).slerp(o.quaternion.clone(), poidsGauche); o.updateMatrixWorld(true); });
      }
    }
    return { droite, gauche, poidsGauche, mode: tenue.mode };
  }

  // ---------------------------------------------------------------- le contrôle d'une pose (pour le bot)
  mesurer(arme, resultat) {
    const C = this.carte, def = arme.userData.def;
    const traverse = this.distanceArme(arme);
    const crosse = this.versMonde(arme, def.crosse);
    const epaule = C.bras_d.getWorldPosition(v());
    return {
      mainDroite: resultat.droite.ecart, mainGauche: resultat.gauche ? resultat.gauche.ecart : null,
      etirementDroit: resultat.droite.etirement, etirementGauche: resultat.gauche ? resultat.gauche.etirement : null,
      coudeDroit: resultat.droite.coude * 180 / Math.PI, coudeGauche: resultat.gauche ? resultat.gauche.coude * 180 / Math.PI : null,
      traverse: traverse / this.k,                         // < 0 : l'arme entre dans le torse (en mètres du mannequin)
      crosseEpaule: crosse.distanceTo(epaule) / this.k,
      glisse: resultat.gauche?.glisse ?? 0,
    };
  }
}

// le coût d'une pose (plus il est bas, mieux c'est) : bras tendus à fond, coude replié, arme dans le corps, main décrochée
export function cout(m) {
  let c = 0;
  c += Math.max(0, m.etirementDroit - 0.95) * 40 + Math.max(0, (m.etirementGauche ?? 0) - 0.95) * 40;
  c += Math.max(0, 35 - m.coudeDroit) * 0.08 + Math.max(0, 35 - (m.coudeGauche ?? 90)) * 0.08;
  c += Math.max(0, 0.02 - m.traverse) * 60;
  c += (m.mainDroite + (m.mainGauche ?? 0)) * 30;
  c += Math.abs(m.glisse) * 4;
  return c;
}

// le bot : pour un personnage, cherche l'appui de la crosse qui donne les plus belles poses de visée.
// poser(instant) pose le personnage à cet instant ({ clip, u }) et renvoie la tenue de l'animation
export function calibrerAppui(prise, arme, poser, instants) {
  const poses = instants.map(i => ({ tenue: poser(i), u: i.u }));
  let meilleur = null;
  for (const gauche of [0.02, 0.04, 0.06, 0.08])
    for (const avant of [0.0, 0.03, 0.06, 0.09, 0.12])
      for (const bas of [-0.03, 0.0, 0.03, 0.06]) {
        const appui = { gauche, avant, bas };
        let total = 0;
        for (const [j, p] of poses.entries()) {
          poser(instants[j]);
          const r = prise.tenir(arme, p.tenue, p.u, appui);
          total += cout(prise.mesurer(arme, r));
        }
        // une petite préférence pour l'appui de référence (le mannequin du studio)
        total += (Math.abs(gauche - 0.055) + Math.abs(avant - 0.06) + Math.abs(bas - 0.01)) * 0.5;
        if (!meilleur || total < meilleur.total) meilleur = { appui, total };
      }
  prise.appui = meilleur.appui;
  // le verdict : avec le meilleur appui, ce corps tient-il le fusil proprement ? (sinon, mieux vaut ne pas le lui donner)
  let pireMain = 0, pireEtirement = 0, pireTorse = 9;
  for (const [j, p] of poses.entries()) {
    poser(instants[j]);
    const m = prise.mesurer(arme, prise.tenir(arme, p.tenue, p.u));
    pireMain = Math.max(pireMain, m.mainDroite, m.mainGauche ?? 0);
    pireEtirement = Math.max(pireEtirement, m.etirementDroit, m.etirementGauche ?? 0);
    pireTorse = Math.min(pireTorse, m.traverse);
  }
  meilleur.pireMain = pireMain; meilleur.pireEtirement = pireEtirement; meilleur.pireTorse = pireTorse;
  meilleur.apte = pireMain < 0.03 * prise.k && pireEtirement < 1.06 && pireTorse > -0.05;
  prise.inapte = meilleur.apte ? null : { fusil: true, raison: pireTorse <= -0.05 ? 'le fusil entre dans son corps' : 'ses bras sont trop courts pour son corps' };
  return meilleur;
}
