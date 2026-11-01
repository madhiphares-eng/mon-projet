// Échos d'Éther : lance un des bots du navigateur, sans fenêtre, et écrit son rapport.
//   node outils/bots/banc.js <bot> --modeles <dossier du prologue> [--sortie dossier] [--seulement a,b] [--captures spec]
// <bot> :
//   mouvements  le bot des mouvements (vitrine/bot-mouvements.js) : pieds au sol, boucles, sur chaque personnage
//   prises      le bot de prise des armes (vitrine/prise-armes.js) : mains sur le fusil et le pistolet, peau des mains
//   objets      le bot des objets (vitrine/bot-objets.js) : portes, machines, drone, et l'énergie du Colosse
// --captures (prises, objets) : prend aussi des images (prises : « Fusil_Visee:0.3,Fusil_Repos:0.3 » ; objets : « 1 »).
// --clips (mouvements) : les animations à contrôler, séparées par des virgules (par défaut, les 23 au sol).
// Il sert lui-même la page et les modèles (un petit serveur local), et prend three.js en local s'il le trouve
// (--three dossier, ou node_modules/three), sinon sur le CDN. Playwright doit être installé (npm i playwright ;
// dans le cloud de Claude, il l'est déjà, avec Chromium).
const { chromium } = require('playwright');
const fs = require('fs'), path = require('path'), http = require('http');

const DEPOT = path.resolve(__dirname, '..', '..');
const args = process.argv.slice(2);
const bot = args[0];
const opt = n => { const i = args.indexOf('--' + n); return i > 0 ? args[i + 1] : null; };
const MODELES = path.resolve(opt('modeles') || process.env.PROLOGUE || path.join(DEPOT, 'prologue'));
const SORTIE = path.resolve(opt('sortie') || path.join(DEPOT, 'bots-rapports', bot || ''));
const SEUL = opt('seulement') || '';
const CAPTURES = opt('captures') || '';
const THREE = [opt('three'), process.env.THREE_LOCAL, path.join(DEPOT, 'node_modules/three')].find(d => d && fs.existsSync(path.join(d, 'build/three.module.min.js')));
const CLIPS_SOL = ['Idle_Loop', 'Walk_Loop', 'Jog_Fwd_Loop', 'Sprint_Loop', 'Crouch_Idle_Loop', 'Crouch_Fwd_Loop', 'Walk_Formal_Loop', 'Idle_Talking_Loop',
  'Idle_FoldArms_Loop', 'Zombie_Walk_Fwd_Loop', 'Walk_Carry_Loop', 'Fusil_Repos', 'Fusil_Marche', 'Fusil_Course', 'Fusil_Marche_Visee', 'Fusil_Visee',
  'Androide_Marche', 'Androide_Garde', 'Gris_Marche', 'Saurien_Marche', 'Detenu_Marche_Entravee', 'Detenu_Panique', 'Detenu_Reddition'];
const CLIPS_ARMES = ['Fusil_Visee', 'Fusil_Marche_Visee', 'Fusil_Tir', 'Fusil_Accroupi_Visee', 'Fusil_Repos', 'Fusil_Marche', 'Fusil_Course', 'Fusil_Rechargement', 'Pistol_Idle_Loop', 'Pistol_Aim_Neutral', 'Pistol_Shoot'];
const PAGES = { mouvements: 'banc-pnj.html', prises: 'banc-pnj.html', objets: 'banc-objets.html' };
const pause = ms => new Promise(r => setTimeout(r, ms));

if (!PAGES[bot]) { console.error('Bot inconnu. Usage : node outils/bots/banc.js mouvements|prises|objets --modeles <dossier du prologue>'); process.exit(2); }
if (!fs.existsSync(MODELES)) { console.error(`Le dossier des modèles manque : ${MODELES} (--modeles, ou la variable PROLOGUE)`); process.exit(2); }

// --- le serveur : /vitrine/… depuis le dépôt, /modeles/… depuis le dossier du prologue
const TYPES = { '.html': 'text/html; charset=utf-8', '.js': 'application/javascript', '.json': 'application/json', '.glb': 'model/gltf-binary', '.png': 'image/png', '.jpg': 'image/jpeg', '.bin': 'application/octet-stream' };
const serveur = http.createServer((q, r) => {
  const u = decodeURIComponent(q.url.split('?')[0]);
  const racine = u.startsWith('/modeles/') ? MODELES : path.join(DEPOT, 'vitrine');
  const f = path.join(racine, u.replace(/^\/(modeles|vitrine)\//, '/'));
  if (!f.startsWith(racine) || !fs.existsSync(f) || fs.statSync(f).isDirectory()) { r.writeHead(404); r.end(); return; }
  r.writeHead(200, { 'content-type': TYPES[path.extname(f)] || 'application/octet-stream' });
  fs.createReadStream(f).pipe(r);
});

(async () => {
  await new Promise(ok => serveur.listen(0, '127.0.0.1', ok));
  const base = `http://127.0.0.1:${serveur.address().port}`;
  const b = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const page = await (await b.newContext({ viewport: { width: 480, height: 420 } })).newPage();
  page.setDefaultTimeout(3600000);
  if (THREE) await page.route('https://cdn.jsdelivr.net/npm/three@0.160.0/**', r => {
    const f = path.join(THREE, r.request().url().split('three@0.160.0/')[1]);
    if (!fs.existsSync(f)) return r.continue();
    r.fulfill({ status: 200, body: fs.readFileSync(f), headers: { 'content-type': 'application/javascript' } });
  });
  page.on('pageerror', e => console.log('ERREUR PAGE', e.message));
  page.on('console', m => { if (m.type() === 'error') console.log('CONSOLE', m.text().slice(0, 300)); });
  const t0 = Date.now();
  await page.goto(`${base}/vitrine/${PAGES[bot]}?racine=${base}/modeles/${SEUL ? '&seulement=' + SEUL : ''}`);
  const pret = bot === 'objets' ? 'window.bancObjets && window.bancObjets.pret' : 'window.banc && window.banc.pret';
  await page.waitForFunction(pret, null, { timeout: 3000000, polling: 2000 });
  fs.mkdirSync(SORTIE, { recursive: true });
  const chargement = (Date.now() - t0) / 1000;
  let rapport;
  if (bot === 'mouvements') rapport = await page.evaluate(c => window.banc.controlerMouvements(c), (opt('clips') || '').split(',').filter(Boolean).length ? opt('clips').split(',') : CLIPS_SOL);
  if (bot === 'prises') rapport = await page.evaluate(c => window.banc.controlerPrises(c), CLIPS_ARMES);
  if (bot === 'objets') {
    rapport = await page.evaluate(() => window.bancObjets.rapport());
    fs.writeFileSync(path.join(SORTIE, 'recettes.json'), JSON.stringify(await page.evaluate(() => window.bancObjets.recettes()), null, 1));
  }
  fs.writeFileSync(path.join(SORTIE, 'rapport.json'), JSON.stringify(rapport, null, 1));

  // --- les captures
  if (CAPTURES && bot === 'prises') {
    await page.evaluate(() => window.banc.plein());
    const n = await page.evaluate(() => window.banc.bancs.length);
    for (let i = 0; i < n; i++) {
      const info = await page.evaluate(i => { const r = window.banc.bancs[i]; return { ok: !!r.objet && !!r.reciblage?.cib && !r.ecarte, nom: r.cle.split('/')[1] }; }, i);
      if (!info.ok) continue;
      await page.evaluate(i => window.banc.seul(i), i);
      for (const [clip, f] of CAPTURES.split(',').map(x => x.split(':'))) {
        await page.evaluate(([i, c]) => window.banc.jouerBiblio(i, c), [i, clip]);
        await page.evaluate(f => window.banc.figer(+f), f || '0.3');
        for (const [vue, cam] of [['face', [-1.0, 0.75, 1.6]], ['profil', [-1.45, 0.7, 0.3]]]) {
          await page.evaluate(([i, c]) => { const B = window.banc, r = B.bancs[i], x = r.objet.position.x, h = r.hauteur; B.vue(x + h * c[0], h * c[1], h * c[2], x, h * 0.55, 0); }, [i, cam]);
          await pause(250);
          await page.screenshot({ path: path.join(SORTIE, `${info.nom}-${clip}-${vue}.png`) });
        }
      }
    }
  }
  if (CAPTURES && bot === 'objets') {
    await page.evaluate(() => { window.bancObjets.plein(); window.bancObjets.pas(0); });
    const n = await page.evaluate(() => window.bancObjets.bancs.length);
    for (let i = 0; i < n; i++) {
      const nom = await page.evaluate(i => window.bancObjets.bancs[i].racine ? window.bancObjets.bancs[i].nom : null, i);
      if (!nom) continue;
      await page.evaluate(i => window.bancObjets.cadrer(i), i);
      for (const [e, proche] of [['normal', false], ['normal', true], ['alarme', true], ['panne', true]]) {
        await page.evaluate(([e, p]) => { const B = window.bancObjets; B.etat(e); B.approcher(p); B.avancer(0.05, 80); }, [e, proche]);
        await pause(200);
        await page.screenshot({ path: path.join(SORTIE, `${nom}-${e}${proche ? '-proche' : ''}.png`) });
      }
      await page.evaluate(() => { const B = window.bancObjets; B.etat('normal'); B.approcher(false); B.avancer(0.05, 80); });
    }
  }
  console.log(`${bot} : ${rapport.length} contrôlés, chargement ${chargement.toFixed(0)} s, en tout ${((Date.now() - t0) / 1000).toFixed(0)} s → ${path.relative(DEPOT, SORTIE)}/rapport.json`);
  await b.close();
  serveur.close();
})().catch(e => { console.error(e); process.exit(1); });
