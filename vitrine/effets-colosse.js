// Échos d'Éther : les effets visuels du Colosse, pour three.js (r160).
// Tout est fabriqué par le code (textures, particules, shaders) : aucun fichier à télécharger, aucune licence à citer.
//
//   import { Effets } from './effets-colosse.js';
//   const fx = new Effets(scene);
//   const p = fx.propulseur({ couleur: 0x7fc8ff }); vaisseau.add(p); p.puissance = 0.8;
//   fx.etincelles(new THREE.Vector3(2, 2.4, 0), { sol: 0 });          // un panneau en court-circuit
//   fx.vapeur(new THREE.Vector3(0, 3, 0), new THREE.Vector3(1, 0, 0));  // une fuite de vapeur
//   fx.tir(bouche, cible, { couleur: 0xff3020 });                       // un tir de l'Empire
//   const c = fx.champ(2, 2.6); porte.add(c); c.toucher(point);         // le champ de force d'une cellule
//   fx.rayonEther(6); fx.gyrophare();                                   // la relique, l'alarme
//   dans la boucle : fx.maj(dt);
import * as THREE from 'three';

// ------------------------------------------------------------------ les textures, dessinées une fois
const cacheTextures = new Map();
function texture(nom, taille, dessiner) {
  if (cacheTextures.has(nom)) return cacheTextures.get(nom);
  const c = document.createElement('canvas');
  c.width = c.height = taille;
  const ctx = c.getContext('2d');
  const img = ctx.createImageData(taille, taille);
  for (let y = 0; y < taille; y++) for (let x = 0; x < taille; x++) {
    const [r, g, b, a] = dessiner((x + 0.5) / taille, (y + 0.5) / taille);
    const i = (y * taille + x) * 4;
    img.data[i] = r * 255; img.data[i + 1] = g * 255; img.data[i + 2] = b * 255; img.data[i + 3] = a * 255;
  }
  ctx.putImageData(img, 0, 0);
  const t = new THREE.CanvasTexture(c);
  t.colorSpace = THREE.SRGBColorSpace;
  cacheTextures.set(nom, t);
  return t;
}
const hash = (x, y) => { const s = Math.sin(x * 127.1 + y * 311.7) * 43758.5453; return s - Math.floor(s); };
function bruitLisse(x, y) {
  const ix = Math.floor(x), iy = Math.floor(y); let fx = x - ix, fy = y - iy;
  fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
  const a = hash(ix, iy), b = hash(ix + 1, iy), c = hash(ix, iy + 1), d = hash(ix + 1, iy + 1);
  return a + (b - a) * fx + (c - a) * fy + (a - b - c + d) * fx * fy;
}
function fbm(x, y, octaves = 5) {
  let s = 0, amp = 0.5, f = 1;
  for (let o = 0; o < octaves; o++) { s += amp * bruitLisse(x * f, y * f); f *= 2.03; amp *= 0.5; }
  return s;
}
const lisse = (a, b, x) => { const t = Math.min(1, Math.max(0, (x - a) / (b - a))); return t * t * (3 - 2 * t); };
export const Textures = {
  // une lueur ronde, très douce
  lueur: () => texture('lueur', 64, (u, v) => {
    const d = Math.hypot(u - 0.5, v - 0.5) * 2;
    const a = Math.max(0, Math.exp(-d * d * 4.5) - 0.012);
    return [1, 1, 1, a];
  }),
  // une bouffée de fumée : un bruit fractal dans un disque aux bords doux
  fumee: () => texture('fumee', 128, (u, v) => {
    const d = Math.hypot(u - 0.5, v - 0.5) * 2;
    const n = fbm(u * 5 + 3.1, v * 5 + 1.7);
    const a = lisse(1, 0.35, d + (n - 0.5) * 0.6) * (0.45 + 0.75 * n);
    const g = 0.75 + 0.25 * n;
    return [g, g, g, Math.min(1, a)];
  }),
  // une étincelle : un trait chaud, effilé aux deux bouts (u : le long du trait)
  etincelle: () => texture('etincelle', 64, (u, v) => {
    const travers = Math.exp(-Math.pow((v - 0.5) * 7, 2));
    const long = Math.pow(Math.sin(Math.PI * u), 0.6) * (0.4 + 0.6 * u);
    return [1, 1, 1, Math.min(1, travers * long * 1.3)];
  }),
  // l'éclair de bouche : une étoile à branches inégales et un cœur
  eclair: () => texture('eclair', 128, (u, v) => {
    const x = u - 0.5, y = v - 0.5, d = Math.hypot(x, y) * 2, ang = Math.atan2(y, x);
    const branches = Math.pow(Math.abs(Math.cos(ang * 3 + 0.4)), 18) * 0.9 + Math.pow(Math.abs(Math.cos(ang * 2.5 + 1.3)), 30) * 0.6;
    const rayons = branches * Math.max(0, 1 - d);
    const coeur = Math.exp(-d * d * 10);
    return [1, 1, 1, Math.min(1, coeur + rayons)];
  }),
  // un anneau (onde au sol)
  anneau: () => texture('anneau', 128, (u, v) => {
    const d = Math.hypot(u - 0.5, v - 0.5) * 2;
    return [1, 1, 1, Math.exp(-Math.pow((d - 0.82) * 14, 2)) + 0.25 * Math.exp(-Math.pow((d - 0.82) * 4, 2)) * (d < 0.82)];
  }),
  // une trace de brûlure (impact sur une paroi)
  brulure: () => texture('brulure', 64, (u, v) => {
    const d = Math.hypot(u - 0.5, v - 0.5) * 2, n = fbm(u * 6, v * 6, 4);
    return [0.05, 0.04, 0.035, lisse(1, 0.2, d + (n - 0.5) * 0.5) * 0.85];
  }),
};

// ------------------------------------------------------------------ les flipbooks (Unity Labs, CC0), dans le dossier effets/ à côté de ce module
// Des simulations de fumée, de flammes et d'explosion faites dans Houdini : bien plus riches qu'une texture dessinée.
export const FLIPBOOKS = {
  volutes: { fichier: 'fumee_volutes_8x8.webp', colonnes: 8, lignes: 8 },
  volutes2: { fichier: 'fumee_volutes2_8x8.webp', colonnes: 8, lignes: 8 },
  fumeeFine: { fichier: 'fumee_fine_20x4.webp', colonnes: 20, lignes: 4 },
  explosion: { fichier: 'explosion_5x5.webp', colonnes: 5, lignes: 5 },
  bouleFeu: { fichier: 'boule_feu_8x8.webp', colonnes: 8, lignes: 8 },
  flamme: { fichier: 'flamme_16x4.webp', colonnes: 16, lignes: 4 },
};
const cacheFlipbooks = new Map();
function chargerFlipbook(fichier) {
  if (!cacheFlipbooks.has(fichier)) cacheFlipbooks.set(fichier, new Promise(ok => {
    new THREE.TextureLoader().load(new URL('./effets/' + fichier, import.meta.url).href, t => {
      t.colorSpace = THREE.SRGBColorSpace; t.anisotropy = 4; ok(t);
    }, undefined, () => ok(null));           // sans le fichier : on garde l'effet fait par le code
  }));
  return cacheFlipbooks.get(fichier);
}

// ------------------------------------------------------------------ les particules : des quads instanciés, face à la caméra
const VERTEX_PARTICULES = /* glsl */`
  attribute vec3 iCentre;
  attribute vec3 iVitesse;
  attribute vec4 iCouleur;
  attribute vec3 iForme;      // x : taille, y : rotation, z : étirement selon la vitesse (0 : rond)
  attribute float iImage;     // l'image du flipbook (avec sa fraction, pour le fondu entre deux images)
  varying vec2 vUv;
  varying vec4 vCouleur;
  varying float vImage;
  void main() {
    vUv = uv;
    vCouleur = iCouleur;
    vImage = iImage;
    vec4 c = viewMatrix * vec4(iCentre, 1.0);
    vec2 coin = position.xy;
    float taille = iForme.x;
    if (iForme.z > 0.0) {
      vec3 vv = (viewMatrix * vec4(iVitesse, 0.0)).xyz;
      float l = length(vv.xy);
      vec2 ax = l > 1e-4 ? vv.xy / l : vec2(1.0, 0.0);
      vec2 ay = vec2(-ax.y, ax.x);
      c.xy += ax * coin.x * (taille + l * iForme.z) + ay * coin.y * taille;
    } else {
      float s = sin(iForme.y), co = cos(iForme.y);
      c.xy += vec2(co * coin.x - s * coin.y, s * coin.x + co * coin.y) * taille;
    }
    gl_Position = projectionMatrix * c;
  }`;
const FRAGMENT_PARTICULES = /* glsl */`
  uniform sampler2D carte;
  uniform vec2 grille;        // colonnes, lignes du flipbook (0 : une seule image)
  varying vec2 vUv;
  varying vec4 vCouleur;
  varying float vImage;
  vec2 case_(float i) {
    float c = mod(i, grille.x), l = floor(i / grille.x);
    return (vUv + vec2(c, grille.y - 1.0 - l)) / grille;
  }
  void main() {
    vec4 t;
    if (grille.x > 0.0) {
      float n = grille.x * grille.y, i = floor(vImage), f = vImage - i;
      t = mix(texture2D(carte, case_(mod(i, n))), texture2D(carte, case_(mod(i + 1.0, n))), f);   // fondu entre deux images
    } else t = texture2D(carte, vUv);
    gl_FragColor = vec4(vCouleur.rgb * t.rgb, vCouleur.a * t.a);
  }`;

class Systeme {
  // flipbook : { fichier, colonnes, lignes } ; la texture se charge à côté de ce module (dossier effets/).
  // En attendant (ou sans le fichier), le système garde la texture faite par le code.
  constructor(scene, carte, additif, capacite = 600, flipbook = null) {
    const base = new THREE.PlaneGeometry(1, 1);
    const g = new THREE.InstancedBufferGeometry();
    g.index = base.index;
    g.setAttribute('position', base.getAttribute('position'));
    g.setAttribute('uv', base.getAttribute('uv'));
    this.centre = new THREE.InstancedBufferAttribute(new Float32Array(capacite * 3), 3).setUsage(THREE.DynamicDrawUsage);
    this.vitesse = new THREE.InstancedBufferAttribute(new Float32Array(capacite * 3), 3).setUsage(THREE.DynamicDrawUsage);
    this.couleur = new THREE.InstancedBufferAttribute(new Float32Array(capacite * 4), 4).setUsage(THREE.DynamicDrawUsage);
    this.forme = new THREE.InstancedBufferAttribute(new Float32Array(capacite * 3), 3).setUsage(THREE.DynamicDrawUsage);
    g.setAttribute('iCentre', this.centre); g.setAttribute('iVitesse', this.vitesse);
    this.image = new THREE.InstancedBufferAttribute(new Float32Array(capacite), 1).setUsage(THREE.DynamicDrawUsage);
    g.setAttribute('iCouleur', this.couleur); g.setAttribute('iForme', this.forme); g.setAttribute('iImage', this.image);
    g.instanceCount = 0;
    this.images = 1;
    const m = new THREE.ShaderMaterial({
      uniforms: { carte: { value: carte }, grille: { value: new THREE.Vector2(0, 0) } }, vertexShader: VERTEX_PARTICULES, fragmentShader: FRAGMENT_PARTICULES,
      transparent: true, depthWrite: false, blending: additif ? THREE.AdditiveBlending : THREE.NormalBlending,
    });
    this.mesh = new THREE.Mesh(g, m);
    this.mesh.frustumCulled = false;
    this.mesh.renderOrder = additif ? 3 : 2;
    scene.add(this.mesh);
    this.capacite = capacite;
    this.vivantes = [];
    if (flipbook) chargerFlipbook(flipbook.fichier).then(t => {
      if (!t) return;
      m.uniforms.carte.value = t;
      m.uniforms.grille.value.set(flipbook.colonnes, flipbook.lignes);
      this.images = flipbook.colonnes * flipbook.lignes;
      this.flipbook = true;
    });
  }
  // p : { pos, vel, vie, taille: [début, fin], couleur: [Color début, Color fin], alpha, gravite, frein, sol, rebond, etire, rot, vrot, monte }
  ajouter(p) {
    if (this.vivantes.length >= this.capacite) this.vivantes.shift();
    p.age = 0; p.rot = p.rot ?? Math.random() * 6.283; p.vrot = p.vrot ?? 0;
    this.vivantes.push(p);
    return p;
  }
  maj(dt) {
    const L = this.vivantes;
    let n = 0;
    const c = new THREE.Color();
    for (let i = 0; i < L.length; i++) {
      const p = L[i];
      p.age += dt;
      if (p.age >= p.vie) continue;
      if (p.gravite) p.vel.y -= 9.81 * p.gravite * dt;
      if (p.monte) p.vel.y += p.monte * dt;
      if (p.frein) p.vel.multiplyScalar(Math.exp(-p.frein * dt));
      p.pos.addScaledVector(p.vel, dt);
      if (p.sol !== undefined && p.pos.y < p.sol) {
        p.pos.y = p.sol; p.vel.y = -p.vel.y * (p.rebond ?? 0.3); p.vel.x *= 0.6; p.vel.z *= 0.6;
      }
      p.rot += p.vrot * dt;
      const k = p.age / p.vie;
      const taille = p.taille[0] + (p.taille[1] - p.taille[0]) * k;
      c.copy(p.couleur[0]).lerp(p.couleur[1], k);
      const fondu = Math.min(1, p.age / (p.vie * (p.entree ?? 0.08))) * (1 - lisse(p.sortie ?? 0.55, 1, k));
      this.centre.setXYZ(n, p.pos.x, p.pos.y, p.pos.z);
      this.vitesse.setXYZ(n, p.vel.x, p.vel.y, p.vel.z);
      this.couleur.setXYZW(n, c.r, c.g, c.b, (p.alpha ?? 1) * fondu);
      this.forme.setXYZ(n, taille, p.rot, p.etire || 0);
      // l'image du flipbook : jouée une fois sur la vie de la particule, ou en boucle à p.ips images par seconde
      const N = this.images;
      this.image.setX(n, N <= 1 ? 0 : p.ips ? (p.age * p.ips + (p.decalage ?? 0)) % N : Math.min(k * (N - 1), N - 1.001));
      L[n++] = p;
    }
    L.length = n;
    this.mesh.geometry.instanceCount = n;
    for (const a of [this.centre, this.vitesse, this.couleur, this.forme, this.image]) { a.needsUpdate = true; a.clearUpdateRanges?.(); }
  }
}

// ------------------------------------------------------------------ les shaders des volumes (flamme, champ, rayon, faisceau)
const BRUIT_GLSL = /* glsl */`
  float h21(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }
  float bruit(vec2 p) {
    vec2 i = floor(p), f = fract(p); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(h21(i), h21(i + vec2(1, 0)), f.x), mix(h21(i + vec2(0, 1)), h21(i + vec2(1, 1)), f.x), f.y);
  }`;
const VERTEX_VOLUME = /* glsl */`
  varying vec2 vUv;
  varying vec3 vNormale;
  varying vec3 vVue;
  varying vec3 vLocal;
  void main() {
    vUv = uv;
    vLocal = position;
    vec4 mv = modelViewMatrix * vec4(position, 1.0);
    vNormale = normalize(normalMatrix * normal);
    vVue = -mv.xyz;
    gl_Position = projectionMatrix * mv;
  }`;
function materiauVolume(fragment, uniforms) {
  return new THREE.ShaderMaterial({
    uniforms, vertexShader: VERTEX_VOLUME, fragmentShader: BRUIT_GLSL + fragment,
    transparent: true, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide,
  });
}
// la flamme d'un propulseur : un cône, plus chaud au centre et près de la tuyère, parcouru de turbulences
const FRAGMENT_FLAMME = /* glsl */`
  uniform vec3 coeur; uniform vec3 bord; uniform float temps; uniform float puissance;
  varying vec2 vUv; varying vec3 vNormale; varying vec3 vVue;
  void main() {
    float face = abs(dot(normalize(vNormale), normalize(vVue)));
    float pres = vUv.y;                                       // 1 à la tuyère, 0 à la pointe
    float n = bruit(vec2(vUv.x * 9.0, pres * 7.0 + temps * 16.0)) * 0.6 + bruit(vec2(vUv.x * 21.0, pres * 15.0 + temps * 31.0)) * 0.4;
    float diamants = 0.75 + 0.25 * sin(pres * 34.0 - temps * 3.0);   // les « diamants » du jet
    float a = pow(face, 1.6) * smoothstep(0.0, 0.55, pres) * (0.55 + 0.6 * n) * diamants;
    vec3 c = mix(bord, coeur, pow(face, 3.0) * pres);
    gl_FragColor = vec4(c * a * puissance, a);
  }`;
// le champ de force : des hexagones, des lignes de balayage, un cadre, et des ondes là où il est touché
const FRAGMENT_CHAMP = /* glsl */`
  uniform vec3 couleur; uniform float temps; uniform float intensite; uniform vec2 taille; uniform vec4 impacts[8];
  varying vec2 vUv; varying vec3 vNormale; varying vec3 vVue; varying vec3 vLocal;
  float hexDist(vec2 p) { p = abs(p); return max(dot(p, normalize(vec2(1.0, 1.732))), p.x); }
  vec4 hexa(vec2 uv) {
    vec2 r = vec2(1.0, 1.732), h = r * 0.5;
    vec2 a = mod(uv, r) - h, b = mod(uv - h, r) - h;
    vec2 gv = dot(a, a) < dot(b, b) ? a : b;
    return vec4(gv, uv - gv);
  }
  void main() {
    vec2 p = vUv * taille;
    vec4 hx = hexa(p * 5.0);
    float d = 0.5 - hexDist(hx.xy);
    float trait = smoothstep(0.06, 0.0, d);
    float cellule = h21(hx.zw);
    float pulse = 0.5 + 0.5 * sin(temps * 2.0 + cellule * 6.283);
    float balayage = pow(0.5 + 0.5 * sin(p.y * 3.0 - temps * 2.5), 12.0);
    float lignes = 0.5 + 0.5 * sin(p.y * 160.0 + temps * 8.0);
    vec2 bords = min(vUv, 1.0 - vUv) * taille;
    float cadre = smoothstep(0.08, 0.0, min(bords.x, bords.y));
    float onde = 0.0;
    for (int i = 0; i < 8; i++) {
      vec4 im = impacts[i];
      if (im.w < 0.0) continue;
      float age = temps - im.w;
      if (age > 1.2) continue;
      float dist = length(vLocal.xy - im.xy);
      onde += smoothstep(0.12, 0.0, abs(dist - age * 2.4)) * exp(-age * 3.0) * 2.0 + exp(-dist * 6.0) * exp(-age * 8.0) * 2.5;
    }
    float face = abs(dot(normalize(vNormale), normalize(vVue)));
    float a = (trait * (0.35 + 0.4 * pulse) + 0.06 + balayage * 0.35 + cadre * 0.9) * (0.85 + 0.15 * lignes) + onde;
    a *= intensite * mix(1.4, 1.0, face) * (0.92 + 0.08 * bruit(vec2(temps * 30.0, 0.0)));
    gl_FragColor = vec4(couleur * a, a);
  }`;
// le rayon d'éther : un cylindre plus dense au cœur, traversé de bandes qui montent
const FRAGMENT_RAYON = /* glsl */`
  uniform vec3 couleur; uniform float temps; uniform float intensite;
  varying vec2 vUv; varying vec3 vNormale; varying vec3 vVue;
  void main() {
    float face = abs(dot(normalize(vNormale), normalize(vVue)));
    float n = bruit(vec2(vUv.x * 10.0, vUv.y * 6.0 - temps * 1.6)) * 0.6 + bruit(vec2(vUv.x * 25.0, vUv.y * 18.0 - temps * 3.5)) * 0.4;
    float bandes = 0.85 + 0.15 * sin(vUv.y * 22.0 - temps * 5.0);
    float haut = 1.0 - smoothstep(0.75, 1.0, vUv.y);
    float bas = smoothstep(0.0, 0.04, vUv.y);
    float a = (pow(face, 2.5) * 1.2 + pow(face, 0.6) * 0.15) * (0.5 + 0.7 * n) * bandes * haut * bas * intensite;
    vec3 c = mix(couleur, vec3(1.0), pow(face, 6.0) * 0.6);
    gl_FragColor = vec4(c * a, a);
  }`;
// le faisceau d'un gyrophare : un cône de lumière dans la poussière, qui s'efface en s'éloignant
const FRAGMENT_FAISCEAU = /* glsl */`
  uniform vec3 couleur; uniform float temps; uniform float intensite;
  varying vec2 vUv; varying vec3 vNormale; varying vec3 vVue;
  void main() {
    float face = abs(dot(normalize(vNormale), normalize(vVue)));
    float poussiere = 0.75 + 0.25 * bruit(vec2(vUv.x * 14.0, vUv.y * 5.0 + temps * 0.6));
    float a = pow(face, 1.5) * pow(vUv.y, 2.2) * poussiere * intensite;
    gl_FragColor = vec4(couleur * a, a);
  }`;

const coul = c => (c instanceof THREE.Color ? c.clone() : new THREE.Color(c));
const v3 = (x = 0, y = 0, z = 0) => new THREE.Vector3(x, y, z);
const hasard = (a, b) => a + Math.random() * (b - a);
function dansLeCone(direction, ouverture) {
  // une direction tirée au hasard dans un cône (ouverture en radians)
  const d = direction.clone().normalize();
  const t = Math.abs(d.y) < 0.9 ? v3(0, 1, 0) : v3(1, 0, 0);
  const a = v3().crossVectors(d, t).normalize(), b = v3().crossVectors(d, a);
  const r = Math.tan(ouverture) * Math.sqrt(Math.random()), ang = Math.random() * 6.283;
  return d.addScaledVector(a, Math.cos(ang) * r).addScaledVector(b, Math.sin(ang) * r).normalize();
}

// ------------------------------------------------------------------ le chef d'orchestre
export class Effets {
  constructor(scene, { lumieres = 3 } = {}) {
    this.scene = scene;
    this.temps = 0;
    this.lueurs = new Systeme(scene, Textures.lueur(), true, 1500);
    this.traits = new Systeme(scene, Textures.etincelle(), true, 800);
    this.fumees = new Systeme(scene, Textures.fumee(), false, 500);
    this.eclairs = new Systeme(scene, Textures.eclair(), true, 60);
    // les flipbooks (Unity Labs, CC0) : de vraies fumées, flammes et explosions simulées
    this.volutes = new Systeme(scene, Textures.fumee(), false, 400, FLIPBOOKS.volutes);
    this.fumeeFine = new Systeme(scene, Textures.fumee(), false, 300, FLIPBOOKS.fumeeFine);
    this.explosions = new Systeme(scene, Textures.fumee(), false, 40, FLIPBOOKS.explosion);
    this.feux = new Systeme(scene, Textures.lueur(), true, 80, FLIPBOOKS.bouleFeu);
    this.flammes = new Systeme(scene, Textures.lueur(), false, 200, FLIPBOOKS.flamme);   // en fondu : la forme des flammes reste lisible
    this.systemes = [this.lueurs, this.traits, this.fumees, this.eclairs, this.volutes, this.fumeeFine, this.explosions, this.feux, this.flammes];
    this.animes = [];                    // ce qui a une méthode maj(dt, temps)
    this.ephemeres = [];                 // ce qui disparaît seul : { maj(dt) → false quand fini }
    // un petit lot de lumières pour les éclairs (tirs, courts-circuits) : créées une fois, jamais retirées,
    // sinon three.js recompile tous les matériaux à chaque changement du nombre de lumières
    this.flashs = [];
    for (let i = 0; i < lumieres; i++) {
      const l = new THREE.PointLight(0xffffff, 0, 6, 2);
      scene.add(l);
      this.flashs.push({ l, age: 1, vie: 1, fort: 0 });
    }
  }

  maj(dt) {
    dt = Math.min(dt, 0.05);
    this.temps += dt;
    for (const a of this.animes) a.maj(dt, this.temps);
    this.ephemeres = this.ephemeres.filter(e => e.maj(dt));
    for (const f of this.flashs) {
      f.age += dt;
      f.l.intensity = f.age < f.vie ? f.fort * Math.pow(1 - f.age / f.vie, 2) : 0;
    }
    for (const sys of this.systemes) sys.maj(dt);
  }

  flash(position, couleur, force = 30, vie = 0.08, portee = 6) {
    const f = this.flashs.reduce((a, b) => (a.age / a.vie > b.age / b.vie ? a : b));
    f.l.position.copy(position); f.l.color.set(couleur); f.l.distance = portee;
    f.age = 0; f.vie = vie; f.fort = force;
  }

  // ---------------------------------------------------------------- les étincelles d'un court-circuit
  gerbe(position, { direction = v3(0, 0.3, 1), nombre = 26, force = 1, sol, couleur = 0xffc36a } = {}) {
    const chaud = new THREE.Color(0xfff4d8), c = coul(couleur), froid = new THREE.Color(0xd2401a);
    for (let i = 0; i < nombre; i++) {
      const vel = dansLeCone(direction, 1.1).multiplyScalar(hasard(2, 7) * force);
      this.traits.ajouter({ pos: position.clone(), vel, vie: hasard(0.35, 1.1), taille: [0.025, 0.012], etire: 0.045,
        couleur: [chaud, Math.random() < 0.5 ? c : froid], alpha: 1.6, gravite: 1, frein: 0.6, sol, rebond: 0.35, entree: 0.01, sortie: 0.6 });
    }
    this.lueurs.ajouter({ pos: position.clone(), vel: v3(), vie: 0.12, taille: [0.6, 0.2], couleur: [chaud, c], alpha: 1.5, entree: 0.01 });
    for (let i = 0; i < 2; i++) {
      this.fumeeFine.ajouter({ pos: position.clone().add(v3(hasard(-0.05, 0.05), 0, hasard(-0.05, 0.05))), vel: dansLeCone(direction, 0.6).multiplyScalar(0.35),
        vie: hasard(1.6, 2.4), taille: [0.25, 0.9], couleur: [new THREE.Color(0x6a6a70), new THREE.Color(0x3a3a3e)], alpha: 0.55, monte: 0.3, frein: 1.5, rot: hasard(-0.3, 0.3), entree: 0.05 });
    }
    this.flash(position, c, 25 * force, 0.09, 5);
  }
  etincelles(position, { cadence = 0.6, direction = v3(0, 0.2, 1), sol, couleur } = {}) {
    // un panneau abîmé qui crache des étincelles à intervalles irréguliers, parfois en rafale
    const e = { position: position.clone(), attente: hasard(0.2, 1), actif: true };
    e.maj = dt => {
      if (!e.actif) return;
      e.attente -= dt;
      if (e.attente > 0) return;
      this.gerbe(e.position, { direction, nombre: Math.round(hasard(10, 30)), force: hasard(0.6, 1.2), sol, couleur });
      e.attente = Math.random() < 0.35 ? hasard(0.05, 0.15) : hasard(0.6, 2.6) / cadence;
    };
    this.animes.push(e);
    return e;
  }

  // ---------------------------------------------------------------- vapeur et fumée
  vapeur(position, direction = v3(0, 1, 0), { debit = 18, vitesse = 2.2, couleur = 0xc8ccd2, taille = 1, opacite = 0.16 } = {}) {
    const e = { position: position.clone(), direction: direction.clone().normalize(), reste: 0, actif: true, debit };
    const c0 = coul(couleur), c1 = coul(couleur).multiplyScalar(0.8);
    e.maj = dt => {
      if (!e.actif) return;
      e.reste += dt * e.debit;
      while (e.reste >= 1) {
        e.reste -= 1;
        // un jet serré à la sortie, qui ralentit, s'élargit et monte en se diluant (volutes simulées, jouées sur la vie)
        this.volutes.ajouter({ pos: e.position.clone().addScaledVector(e.direction, Math.random() * 0.1), vel: dansLeCone(e.direction, 0.14).multiplyScalar(vitesse * hasard(0.85, 1.15)),
          vie: hasard(1.4, 2.0), taille: [0.15 * taille, 1.3 * taille], couleur: [c0, c1], alpha: Math.min(1, opacite * 3), monte: 0.45, frein: 1.4,
          vrot: hasard(-0.5, 0.5), entree: 0.06, sortie: 0.45 });
      }
    };
    this.animes.push(e);
    return e;
  }

  // ---------------------------------------------------------------- les tirs
  tir(depart, arrivee, { couleur = 0xff3a22, vitesse = 70, normale, sol } = {}) {
    const c = coul(couleur), blanc = new THREE.Color(0xffffff);
    const dir = arrivee.clone().sub(depart).normalize();
    // l'éclair de bouche
    this.eclairs.ajouter({ pos: depart.clone().addScaledVector(dir, 0.08), vel: v3(), vie: 0.06, taille: [0.55, 0.35], couleur: [blanc, c], alpha: 1.6, entree: 0.01, sortie: 0.3 });
    this.lueurs.ajouter({ pos: depart.clone(), vel: v3(), vie: 0.08, taille: [0.7, 0.3], couleur: [c, c], alpha: 1.2, entree: 0.01 });
    this.flash(depart, c, 40, 0.07, 7);
    // le trait d'énergie : un cœur blanc dans un halo de couleur, qui file vers la cible
    const duree = depart.distanceTo(arrivee) / vitesse;
    const vel = dir.clone().multiplyScalar(vitesse);
    this.traits.ajouter({ pos: depart.clone(), vel: vel.clone(), vie: duree, taille: [0.06, 0.06], etire: 0.012, couleur: [blanc, blanc], alpha: 2, entree: 0.001, sortie: 0.98 });
    this.lueurs.ajouter({ pos: depart.clone(), vel: vel.clone(), vie: duree, taille: [0.32, 0.32], couleur: [c, c], alpha: 1.1, entree: 0.001, sortie: 0.98 });
    let t = 0;
    this.ephemeres.push({ maj: dt => {
      t += dt;
      if (t < duree) return true;
      this.impact(arrivee, normale || dir.clone().negate(), c, sol);
      return false;
    } });
  }
  impact(point, normale, couleur = 0xff3a22, sol) {
    const c = coul(couleur), blanc = new THREE.Color(0xfff2e0);
    for (let i = 0; i < 14; i++) {
      this.traits.ajouter({ pos: point.clone(), vel: dansLeCone(normale, 1.2).multiplyScalar(hasard(2, 6)), vie: hasard(0.2, 0.55), taille: [0.02, 0.01], etire: 0.04,
        couleur: [blanc, c], alpha: 1.5, gravite: 0.8, sol, rebond: 0.3, entree: 0.01 });
    }
    this.lueurs.ajouter({ pos: point.clone().addScaledVector(normale, 0.03), vel: v3(), vie: 0.18, taille: [0.8, 0.2], couleur: [blanc, c], alpha: 1.4, entree: 0.01 });
    this.fumeeFine.ajouter({ pos: point.clone().addScaledVector(normale, 0.05), vel: normale.clone().multiplyScalar(0.5), vie: 1.8, taille: [0.25, 0.9],
      couleur: [new THREE.Color(0x6a6a70), new THREE.Color(0x3a3a3e)], alpha: 0.5, monte: 0.3, frein: 1.5, rot: hasard(-0.3, 0.3), entree: 0.05 });
    this.flash(point, c, 30, 0.1, 5);
    // la brûlure, qui refroidit lentement
    const b = new THREE.Mesh(new THREE.PlaneGeometry(0.22, 0.22), new THREE.MeshBasicMaterial({ map: Textures.brulure(), transparent: true, depthWrite: false, polygonOffset: true, polygonOffsetFactor: -2 }));
    b.position.copy(point).addScaledVector(normale, 0.004);
    b.lookAt(point.clone().add(normale));
    b.rotateZ(Math.random() * 6.283);
    const braise = new THREE.Mesh(b.geometry, new THREE.MeshBasicMaterial({ map: Textures.lueur(), color: 0xff6a20, transparent: true, depthWrite: false, blending: THREE.AdditiveBlending }));
    braise.scale.setScalar(0.5); b.add(braise);
    this.scene.add(b);
    let age = 0;
    this.ephemeres.push({ maj: dt => {
      age += dt;
      braise.material.opacity = Math.max(0, 1 - age / 1.5);
      b.material.opacity = Math.min(1, 6 - age / 2);
      if (age < 12) return true;
      this.scene.remove(b); b.geometry.dispose(); b.material.dispose(); braise.material.dispose();
      return false;
    } });
  }

  // ---------------------------------------------------------------- l'explosion (grenade, réservoir, vaisseau)
  explosion(position, { taille = 1, sol = 0 } = {}) {
    const blanc = new THREE.Color(0xffffff), orange = new THREE.Color(0xff8a30), braise = new THREE.Color(0xff4a10);
    // la boule de feu simulée, puis sa fumée noire : trois nuages un peu décalés
    for (let i = 0; i < 3; i++) {
      const d = i ? v3(hasard(-0.5, 0.5), hasard(0, 0.4), hasard(-0.5, 0.5)).multiplyScalar(taille) : v3();
      this.explosions.ajouter({ pos: position.clone().add(d), vel: v3(0, 0.5 * taille, 0), vie: hasard(1.3, 1.7), taille: [(i ? 1.8 : 2.6) * taille, (i ? 2.6 : 3.6) * taille],
        couleur: [blanc, blanc], alpha: 1, entree: 0.001, sortie: 0.8, rot: hasard(0, 6.283), vrot: hasard(-0.3, 0.3) });
    }
    this.feux.ajouter({ pos: position.clone(), vel: v3(), vie: 0.3, taille: [1.2 * taille, 2.2 * taille], couleur: [blanc, orange], alpha: 0.8, entree: 0.001, sortie: 0.3 });
    for (let i = 0; i < 46; i++) {
      this.traits.ajouter({ pos: position.clone(), vel: v3().randomDirection().add(v3(0, 0.5, 0)).normalize().multiplyScalar(hasard(5, 15) * Math.sqrt(taille)),
        vie: hasard(0.5, 1.4), taille: [0.03, 0.012], etire: 0.05, couleur: [blanc, braise], alpha: 1.8, gravite: 1, frein: 0.4, sol, rebond: 0.3, entree: 0.01 });
    }
    for (let i = 0; i < 18; i++) {           // des débris incandescents
      this.lueurs.ajouter({ pos: position.clone(), vel: v3().randomDirection().add(v3(0, 0.8, 0)).multiplyScalar(hasard(3, 7) * Math.sqrt(taille)),
        vie: hasard(1.2, 2.4), taille: [0.09, 0.03], couleur: [orange, braise], alpha: 1.4, gravite: 1, frein: 0.3, sol, rebond: 0.25, entree: 0.01 });
    }
    this.flash(position, 0xffa040, 60 * taille, 0.35, 14 * taille);
    // l'onde de choc, au sol
    const onde = new THREE.Mesh(new THREE.PlaneGeometry(1, 1), new THREE.MeshBasicMaterial({ map: Textures.anneau(), color: 0xffb070, transparent: true, depthWrite: false, blending: THREE.AdditiveBlending }));
    onde.rotation.x = -Math.PI / 2; onde.position.set(position.x, sol + 0.03, position.z); this.scene.add(onde);
    let age = 0, fumee = 0;
    this.ephemeres.push({ maj: dt => {
      age += dt;
      onde.scale.setScalar((0.5 + age * 14) * taille); onde.material.opacity = Math.max(0, 1 - age / 0.45);
      // puis la colonne de fumée qui monte
      if (age > 0.35 && fumee < 6) {
        fumee++;
        this.volutes.ajouter({ pos: position.clone().add(v3(hasard(-0.4, 0.4), 0.3, hasard(-0.4, 0.4)).multiplyScalar(taille)), vel: v3(hasard(-0.2, 0.2), hasard(0.8, 1.4), hasard(-0.2, 0.2)),
          vie: hasard(3, 4.5), taille: [1.2 * taille, 3.2 * taille], couleur: [new THREE.Color(0x8a827a), new THREE.Color(0x56514c)], alpha: 0.75, frein: 0.4, rot: hasard(0, 6.283), vrot: hasard(-0.2, 0.2), entree: 0.15 });
      }
      if (age < 0.6) return true;
      this.scene.remove(onde); onde.geometry.dispose(); onde.material.dispose();
      return false;
    } });
  }

  // ---------------------------------------------------------------- le feu (débris qui brûlent, conduite de gaz)
  feu(position, { taille = 1, lumiere = true } = {}) {
    const e = { position: position.clone(), actif: true, reste: 0, braises: 0, fumee: 0 };
    const jaune = new THREE.Color(0xffc890).multiplyScalar(1.9), orange = new THREE.Color(0xff7a30).multiplyScalar(1.5), rouge = new THREE.Color(0xc0300c);
    let lum = null;
    if (lumiere) { lum = new THREE.PointLight(0xff8a3a, 0, 7 * taille, 2); lum.position.copy(position).y += 0.6 * taille; this.scene.add(lum); }
    e.maj = (dt, t) => {
      if (lum) lum.intensity = e.actif ? 12 * taille * (0.75 + 0.25 * Math.sin(t * 23) * Math.sin(t * 7.1)) : 0;
      if (!e.actif) return;
      // des flammes simulées qui se chevauchent, chacune en boucle sur le flipbook (64 images à 30 par seconde)
      e.reste += dt * 5;
      while (e.reste >= 1) {
        e.reste -= 1;
        this.flammes.ajouter({ pos: e.position.clone().add(v3(hasard(-0.12, 0.12), 0.45, hasard(-0.12, 0.12)).multiplyScalar(taille)), vel: v3(0, 0.1, 0),
          vie: hasard(0.8, 1.1), taille: [1.3 * taille, 1.45 * taille], couleur: [jaune, orange], alpha: 0.9, ips: 30, decalage: hasard(0, 64), rot: hasard(-0.15, 0.15), entree: 0.25, sortie: 0.6 });
      }
      e.braises += dt * 14;
      while (e.braises >= 1) {
        e.braises -= 1;
        this.lueurs.ajouter({ pos: e.position.clone().add(v3(hasard(-0.2, 0.2), hasard(0.2, 0.6), hasard(-0.2, 0.2)).multiplyScalar(taille)),
          vel: v3(hasard(-0.4, 0.4), hasard(0.8, 2), hasard(-0.4, 0.4)), vie: hasard(1, 2), taille: [0.035, 0.01], couleur: [orange, rouge], alpha: 1.6, frein: 0.6, entree: 0.05 });
      }
      e.fumee += dt * 3;
      while (e.fumee >= 1) {
        e.fumee -= 1;
        this.fumeeFine.ajouter({ pos: e.position.clone().add(v3(hasard(-0.1, 0.1), 1.0 * taille, hasard(-0.1, 0.1))), vel: v3(hasard(-0.1, 0.1), hasard(0.6, 1), hasard(-0.1, 0.1)),
          vie: hasard(2.5, 3.5), taille: [0.5 * taille, 2.2 * taille], couleur: [new THREE.Color(0x5a5550), new THREE.Color(0x3a3734)], alpha: 0.5, frein: 0.3, rot: hasard(-0.3, 0.3), entree: 0.1 });
      }
    };
    this.animes.push(e);
    return e;
  }

  // ---------------------------------------------------------------- le propulseur (navette, vaisseau pirate)
  propulseur({ couleur = 0x6fb8ff, coeur = 0xeaf6ff, longueur = 1.6, rayon = 0.28, lumiere = true } = {}) {
    const g = new THREE.Group();
    g.puissance = 1;
    const flamme = materiauVolume(FRAGMENT_FLAMME, {
      coeur: { value: coul(coeur) }, bord: { value: coul(couleur) }, temps: { value: 0 }, puissance: { value: 1 },
    });
    // deux cônes emboîtés, la pointe vers -Z (le jet part vers l'arrière) ; la tuyère est à l'origine
    const cones = [[rayon, longueur, 1.0], [rayon * 0.55, longueur * 0.6, 1.6]].map(([r, l, f]) => {
      const geo = new THREE.CylinderGeometry(r, r * 0.12, l, 28, 1, true);
      geo.translate(0, -l / 2, 0); geo.rotateX(Math.PI / 2);
      const m = new THREE.Mesh(geo, flamme.clone());
      m.material.uniforms.puissance.value = f;
      m.renderOrder = 4;
      g.add(m);
      return m;
    });
    const halo = new THREE.Sprite(new THREE.SpriteMaterial({ map: Textures.lueur(), color: coul(couleur), blending: THREE.AdditiveBlending, depthWrite: false, transparent: true }));
    halo.scale.setScalar(rayon * 6); g.add(halo);
    let lum = null;
    if (lumiere) { lum = new THREE.PointLight(coul(couleur), 8, 6, 2); lum.position.z = -0.5; g.add(lum); }
    const monde = v3(), axe = v3();
    let reste = 0;
    g.maj = (dt, temps) => {
      const p = Math.max(0, g.puissance);
      const vacille = 0.92 + 0.08 * Math.sin(temps * 61) * Math.sin(temps * 17.3);
      cones.forEach((m, i) => {
        m.material.uniforms.temps.value = temps;
        m.material.uniforms.puissance.value = (i ? 1.6 : 1) * p;
        m.scale.set(1, 1, Math.max(0.05, p) * vacille);
      });
      halo.material.opacity = Math.min(1, p) * vacille;
      if (lum) lum.intensity = 8 * p * vacille;
      // le plasma qui s'échappe
      g.getWorldPosition(monde);
      axe.set(0, 0, -1).transformDirection(g.matrixWorld);
      reste += dt * 40 * p;
      while (reste >= 1) {
        reste -= 1;
        this.lueurs.ajouter({ pos: monde.clone().addScaledVector(axe, longueur * 0.2), vel: dansLeCone(axe, 0.12).multiplyScalar(hasard(6, 11) * p),
          vie: hasard(0.12, 0.3), taille: [rayon * 0.9, rayon * 0.2], couleur: [coul(coeur), coul(couleur)], alpha: 0.35, entree: 0.05 });
      }
    };
    this.animes.push(g);
    return g;
  }

  // ---------------------------------------------------------------- le champ de force (une cellule, une porte)
  champ(largeur = 2, hauteur = 2.6, { couleur = 0x58d8ff, intensite = 1 } = {}) {
    const impacts = Array.from({ length: 8 }, () => new THREE.Vector4(0, 0, 0, -1));
    const m = materiauVolume(FRAGMENT_CHAMP, {
      couleur: { value: coul(couleur) }, temps: { value: 0 }, intensite: { value: intensite },
      taille: { value: new THREE.Vector2(largeur, hauteur) }, impacts: { value: impacts },
    });
    const geo = new THREE.PlaneGeometry(largeur, hauteur);
    geo.translate(0, hauteur / 2, 0);
    const champ = new THREE.Mesh(geo, m);
    champ.renderOrder = 4;
    champ.actif = true;
    let niveau = 1, prochain = 0, temps = 0;
    champ.toucher = pointMonde => {
      const local = champ.worldToLocal(pointMonde.clone());
      impacts[prochain].set(local.x, local.y, 0, temps);
      prochain = (prochain + 1) % impacts.length;
      this.gerbe(pointMonde, { direction: v3(0, 0, 1).transformDirection(champ.matrixWorld), nombre: 8, force: 0.5, couleur });
    };
    champ.maj = (dt, t) => {
      temps = t;
      // il s'éteint en clignotant, et se rallume par à-coups
      const cible = champ.actif ? 1 : 0;
      niveau += (cible - niveau) * Math.min(1, dt * (champ.actif ? 3 : 6));
      const coupure = !champ.actif && niveau > 0.05 ? (Math.random() < 0.3 ? 0 : 1) : 1;
      m.uniforms.temps.value = t;
      m.uniforms.intensite.value = intensite * niveau * coupure;
      champ.visible = niveau > 0.01;
    };
    this.animes.push(champ);
    return champ;
  }

  // ---------------------------------------------------------------- le rayon d'éther (la relique)
  rayonEther(hauteur = 6, { rayon = 0.45, couleur = 0x5ef2ff, position = v3() } = {}) {
    const g = new THREE.Group();
    g.position.copy(position);
    const m = materiauVolume(FRAGMENT_RAYON, { couleur: { value: coul(couleur) }, temps: { value: 0 }, intensite: { value: 1 } });
    const geo = new THREE.CylinderGeometry(rayon, rayon * 1.25, hauteur, 32, 1, true);
    geo.translate(0, hauteur / 2, 0);
    const faisceau = new THREE.Mesh(geo, m); faisceau.renderOrder = 4; g.add(faisceau);
    const coeur = new THREE.Mesh(new THREE.CylinderGeometry(rayon * 0.25, rayon * 0.35, hauteur, 16, 1, true).translate(0, hauteur / 2, 0), m.clone());
    coeur.material.uniforms.intensite.value = 1.6; coeur.renderOrder = 4; g.add(coeur);
    const lum = new THREE.PointLight(coul(couleur), 12, 9, 2); lum.position.y = 1.2; g.add(lum);
    const sol = new THREE.Mesh(new THREE.PlaneGeometry(1, 1), new THREE.MeshBasicMaterial({ map: Textures.anneau(), color: coul(couleur), transparent: true, depthWrite: false, blending: THREE.AdditiveBlending }));
    sol.rotation.x = -Math.PI / 2; sol.position.y = 0.02; g.add(sol);
    const base = v3();
    let reste = 0, onde = 0;
    g.intensite = 1;
    g.maj = (dt, t) => {
      const k = g.intensite * (0.85 + 0.15 * Math.sin(t * 2.1));
      m.uniforms.temps.value = t; m.uniforms.intensite.value = k;
      coeur.material.uniforms.temps.value = t * 1.3; coeur.material.uniforms.intensite.value = 1.6 * k;
      lum.intensity = 12 * k;
      // une onde au sol toutes les 1,6 s
      onde = (onde + dt / 1.6) % 1;
      sol.scale.setScalar(rayon * 2 + onde * 4.5); sol.material.opacity = (1 - onde) * 0.9 * g.intensite;
      // des grains de lumière qui montent en spirale
      g.getWorldPosition(base);
      reste += dt * 18 * g.intensite;
      while (reste >= 1) {
        reste -= 1;
        const a = Math.random() * 6.283, r = rayon * hasard(0.6, 2.2);
        const p = base.clone().add(v3(Math.cos(a) * r, hasard(0, 0.3), Math.sin(a) * r));
        this.lueurs.ajouter({ pos: p, vel: v3(-Math.sin(a) * 0.5, hasard(0.8, 2.2), Math.cos(a) * 0.5), vie: hasard(1.5, 3), taille: [0.07, 0.02],
          couleur: [new THREE.Color(0xe8ffff), coul(couleur)], alpha: 1.2, frein: 0.2, entree: 0.2 });
      }
    };
    this.animes.push(g);
    return g;
  }

  // ---------------------------------------------------------------- le gyrophare d'alarme
  gyrophare({ couleur = 0xff2a1a, vitesse = 3.2, portee = 9, lumiere = true } = {}) {
    const g = new THREE.Group();
    const corps = new THREE.Mesh(new THREE.CylinderGeometry(0.09, 0.11, 0.16, 16), new THREE.MeshStandardMaterial({ color: coul(couleur), emissive: coul(couleur), emissiveIntensity: 1.5, roughness: 0.3 }));
    g.add(corps);
    const tete = new THREE.Group(); g.add(tete);
    const fx = materiauVolume(FRAGMENT_FAISCEAU, { couleur: { value: coul(couleur) }, temps: { value: 0 }, intensite: { value: 0.9 } });
    for (const s of [1, -1]) {
      const geo = new THREE.ConeGeometry(portee * 0.22, portee * 0.7, 24, 1, true);
      geo.translate(0, -portee * 0.35, 0); geo.rotateZ(s * Math.PI / 2);      // le sommet au gyrophare, ouvert vers ±X
      const cone = new THREE.Mesh(geo, fx); cone.renderOrder = 4; tete.add(cone);
    }
    let spot = null;
    if (lumiere) {
      spot = new THREE.SpotLight(coul(couleur), 30, portee, 0.45, 0.5, 2);
      spot.target.position.set(1, -0.15, 0); tete.add(spot, spot.target);
    }
    const halo = new THREE.Sprite(new THREE.SpriteMaterial({ map: Textures.lueur(), color: coul(couleur), blending: THREE.AdditiveBlending, depthWrite: false, transparent: true }));
    halo.scale.setScalar(0.8); g.add(halo);
    g.actif = true;
    g.maj = (dt, t) => {
      tete.visible = g.actif; halo.visible = g.actif;
      if (spot) spot.intensity = g.actif ? 30 : 0;
      tete.rotation.y += dt * vitesse;
      fx.uniforms.temps.value = t;
      halo.material.opacity = 0.6 + 0.4 * Math.abs(Math.sin(tete.rotation.y));
    };
    this.animes.push(g);
    return g;
  }
}
