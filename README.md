# Échos d'Éther

Jeu coop en monde ouvert, axé sur l'émerveillement et le mystère. Voir [`docs/GDD.md`](docs/GDD.md).

Assets 3D, textures, ciels et sons gratuits et réalistes (liens vérifiés, licences, crédits) : [`docs/ASSETS.md`](docs/ASSETS.md).
Pour les télécharger directement dans le projet (Unity fermé) :

```powershell
irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-assets.ps1 | iex
```

## Installation rapide (PC Windows)

Dans PowerShell, lancer **deux fois** (avant et après avoir créé le projet HDRP `EchosEther` dans `C:\Projets` avec Unity Hub) :

```powershell
irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/installer.ps1 | iex
```

La relancer plus tard met à jour les scripts et la documentation du jeu.

## Mise en route (PC Windows)

1. Installer **Unity Hub**, puis **Unity 6 (6000.x LTS)** avec le module *Windows Build Support (IL2CPP)*.
2. Installer **Git for Windows** (https://git-scm.com) — Git LFS est inclus — puis, dans un terminal : `git lfs install`.
3. Cloner ce dépôt (ou utiliser GitHub Desktop).
4. Dans Unity Hub : **New project → High Definition 3D (HDRP)**, et choisir **ce dossier** comme emplacement
   (Unity ajoutera `ProjectSettings/`, `Packages/` et complétera `Assets/` sans toucher à `Assets/_Projet/`).
5. Package Manager : installer **Cinemachine** et **Netcode for GameObjects**.
6. Dans Unity, *Edit → Project Settings → Editor* : *Asset Serialization = Force Text* et *Version Control = Visible Meta Files*.

## Phase 1 — Lisière de l'Horizon (planète au bord d'un trou noir)

Scripts dans `Assets/_Projet/Scripts/Physique/` :

| Script | Rôle |
|---|---|
| `SourceGravite` | À poser sur la planète (et plus tard sur les anomalies). Gravité en 1/r². |
| `CorpsGravitationnel` | À poser sur tout objet avec Rigidbody (joueur, caisses). Remplace la gravité Unity et l'oriente debout. |
| `DilatationTemporelle` | Calcule le temps vécu en surface vs le temps écoulé en orbite (formule de Schwarzschild, échelle jouable). |

### Scène de test
1. Sphère de rayon 500 (échelle 1000) → `SourceGravite` (rayonSurface = 500).
2. Capsule posée dessus → Rigidbody + `CorpsGravitationnel`.
3. Objet vide « TrouNoir » loin dans le ciel → référencé par `DilatationTemporelle` sur la capsule.
4. Ciel : *Volume → Physically Based Sky* + *Volumetric Clouds* + *Fog* volumétrique.

### Lentille gravitationnelle (déformation du ciel)

Fichiers : `Assets/_Projet/Shaders/LentilleGravitationnelle.shader` + `Assets/_Projet/Scripts/Rendu/TrouNoir.cs`.

1. Sur l'objet « TrouNoir » : ajouter le composant **TrouNoir** (rayonSchwarzschild = même valeur que dans `DilatationTemporelle`).
2. Clic droit dans le dossier Shaders → *Create → Material*, nommé `M_Lentille`, shader **FullScreen/LentilleGravitationnelle**.
3. Dans la scène : *GameObject → Volume → Custom Pass*. Dans l'Inspector :
   - **Injection Point** : *Before Post Process* (pour que le bloom fasse briller l'anneau) ;
   - *Add → FullScreenCustomPass* ;
   - **FullScreen Material** : `M_Lentille` ;
   - **Fetch Color Buffer** : ✅ coché (obligatoire, sinon écran noir ou image figée).
4. Placer le trou noir loin dans le ciel (ex. 20 000 unités) et regarder vers lui : le ciel et les étoiles se tordent autour d'une ombre noire cerclée de lumière.
5. Réglages à essayer : `forceDeviation` entre 1,5 et 3, `intensiteAnneau` entre 5 et 20.

Limite connue du prototype : la déformation ne peut montrer que ce qui est déjà à l'écran (pas ce qui est derrière la caméra).
Prochaine étape : le **disque d'accrétion** (anneau de matière lumineuse), qui deviendra aussi la source de lumière de la planète.

## Le jeu dans Unity : menus, enrôlement, didacticiel, mission

1. Récupérer le code : relancer la commande d'installation rapide (`installer.ps1`). Un fichier modifié sur le PC est d'abord sauvegardé dans `Sauvegardes\<date>`.
2. Ouvrir la scène du jeu (par exemple `Assets/_Projet/Scenes/LisiereHorizon.unity`, créée par *Echos d'Ether > Créer la scène Lisière de l'Horizon*).
3. Menu **Echos d'Ether > Installer le jeu dans la scène (menus, didacticiel, mission)**. Il ajoute :
   - sur l'Arpenteur : `EtatArpenteur`, `JaugeBruit`, `Scanner`, `ApparenceArpenteur` ;
   - sur la caméra : `CameraNavette`, `CameraSecousse` ;
   - un objet **Systemes de jeu** : `GestionnaireJeu`, `InterfaceJeu` (UI Toolkit), `Horloges`, `Transmissions`, `Mission`, `Ruche`, `Didacticiel`, `BanqueSons`, `Largage` (arrivée en navette), `AmbianceSonore` (vent, nappe grave et grondement générés en direct) ;
   - un objet **Lieux de jeu** posé sur la planète autour du joueur : 9 cristaux d'Éther, une bulle lente avec le monolithe, une bulle rapide, la balise d'extraction et la navette.
   - Il utilise les modèles téléchargés par `telecharger-assets.ps1` s'ils sont là : navette Sketchfab (dossier `Externes/Sketchfab/Vaisseaux`), ver des sables (`Externes/Sketchfab/Personnages`), sons Kenney (`Externes/Sons`).
   - armes, créatures et compétences : `Arsenal`, `VieArpenteur`, `CompetencesArpenteur`, `AnimateurArpenteur` sur l'Arpenteur, `Essaim` dans les systèmes ;
     les armes du Kenney Blaster Kit (groupe 6 de `telecharger-assets.ps1`) sont branchées si elles sont là, sinon des formes générées les remplacent.
4. Menu **Echos d'Ether > Vérifier l'installation** : contrôle HDRP, glTFast, ressources, assets téléchargés, sons branchés et composants de la scène.
   Le rapport est copié dans le presse-papiers (Ctrl+V pour le coller dans une conversation).
5. Menu **Echos d'Ether > Test automatique (rapport dans docs)** : le jeu se joue tout seul pendant environ une minute (arrivée, armes, créatures,
   compétences, dégâts, talents, navette, extraction) et écrit `docs/RAPPORT-TEST.md` avec les erreurs de la Console, plus des captures dans `docs/captures/`.
6. Pour donner un corps à l'Arpenteur : sélectionner la combinaison dans la fenêtre Project, puis **Echos d'Ether > Utiliser le modèle sélectionné comme corps de l'Arpenteur**.
7. **Play** : menu principal, enrôlement (nom, spécialité, couleurs, aperçu 3D), arrivée en navette (Espace pour passer), didacticiel en 13 étapes ou mission complète.

Claude en local (MCP for Unity) a son guide : [`docs/CLAUDE-LOCAL.md`](docs/CLAUDE-LOCAL.md).

Relancer le menu complète l'installation sans rien déplacer. *Replacer les lieux de jeu autour du joueur* les repose à neuf.
*Effacer le profil et les réglages enregistrés* remet l'enrôlement à zéro.

| Dossier | Contenu |
|---|---|
| `Scripts/Jeu` | `GestionnaireJeu` (états de partie), `Largage` (arrivée), `AmbianceSonore`, `Competences` et `CompetencesArpenteur`, `Didacticiel`, `Mission`, `JaugeBruit`, `CristalEther`, `Scanner`, `Ruche`, `BulleTemporelle`, `Monolithe`, `Navette`, `BaliseExtraction`, `Horloges`, `Transmissions`, `BanqueSons`, `ProfilArpenteur`, `Specialites`, `Reglages`, `Planete`, `Formes`, `Invite` |
| `Scripts/Joueur` | `EtatArpenteur`, `ApparenceArpenteur`, `AnimateurArpenteur`, `EntreesJeu`, `CameraNavette`, `CameraSecousse` (plus les scripts existants) |
| `Scripts/Combat` | `Arsenal` et `Armes` (catalogue), `VueArme` (arme tenue et ses animations), `Projectiles` (éclairs, grenade), `EffetsCombat`, `Degats`, `VieArpenteur`, `Echarde` (créatures), `Essaim` |
| `Scripts/Interface` | `InterfaceJeu` : HUD et écrans en UI Toolkit, construits par le code |
| `Resources` | `EchosEtherUI.uss` (styles), `EchosEtherTheme.tss` (thème), `Lueur.shader` (lueurs HDRP des bulles, de l'onde du scanner et des faisceaux) |
| `Editor` | `InstallateurJeu` (menu d'installation), `Diagnostic` (vérification), `ImportExternes` (réglage des textures), `ConstructeurScene` |

### Armes, créatures, compétences, animations

| Arme (touche) | Mode | Particularité |
|---|---|---|
| 1 Pic d'arpentage | corps à corps | silencieux |
| 2 Pistolet Ardent | coup par coup, éclairs visibles | précis |
| 3 Carabine Faucheuse | rafale, traçantes | la dispersion s'ouvre en tirant |
| 4 Fusil Grêle | 9 éclats | dévastateur de près, très bruyant |
| 5 Fusil Sablier | se charge détente tenue | traverse 3 cibles, les ralentit, presque silencieux |
| 6 Grenade leurre Écho | lancer (ou G à tout moment) | explose et appelle la Ruche à cet endroit |

- **Bruit** : chaque tir s'ajoute à la jauge ; la Ruche et les créatures l'entendent.
- **Échardes** (créatures de cristal générées et animées par le code, six pattes qui marchent en trépied) : rôdeuse (bond et morsure),
  cracheuse (éclats à distance), colosse (charge, garde la balise une fois le quota atteint). Elles gardent les gisements, tendent embuscade,
  sont ralenties dans les bulles de temps lent, et le ver les avale. Elles lâchent des cellules d'énergie (munitions, intégrité).
- **Intégrité de la combinaison** : se répare seule ; à zéro, rapatriement d'urgence près de la navette (la moitié de l'Éther est perdue). Munitions rechargées près de la navette.
- **Compétences** : une compétence active au choix (bond propulsé, voile de silence, bulle de stase, leurre holographique, onde de choc, bouclier d'Éther),
  14 talents passifs ; un talent de départ à l'enrôlement, puis un par niveau (expérience : cristaux, créatures, esquives, monolithe). Écran **K** ou menu pause.
- **Animations** : `AnimateurArpenteur` joue les clips du modèle (Animation ou Animator), sinon anime le squelette par le code (foulées, course, accroupi,
  saut, atterrissage, regard) en trouvant les os par leur nom ou par l'avatar Humanoid, sinon anime le corps entier. Le corps se tourne vers la visée,
  et les bras tiennent l'arme par cinématique inverse. L'arme a ses propres animations : recul, balancement, rechargement (le chargeur sort), changement, coups, lancer.

Commandes : Z Q S D se déplacer, souris regarder, Maj courir, C marcher en silence, Espace sauter, E récolter / interagir / navette,
F scanner, J journal, M son, N passer une étape du didacticiel, Échap pause, Espace passer l'arrivée en navette.
Combat : clic gauche tirer, clic droit viser, R recharger, 1 à 6 ou molette changer d'arme, G grenade, V coup, A (Q en QWERTY) compétence, K talents. Le code marche avec le nouvel Input System et l'ancien Input Manager.

Manette (avec le nouvel Input System) : stick gauche avancer, stick droit regarder, A sauter, L3 courir, B marcher en silence,
X récolter / interagir, Y scanner, Select journal, Start pause, croix droite passer une étape. En navette : A ou RB monter, B ou LB descendre, gâchette droite boost.
Combat à la manette : gâchette droite tirer, gâchette gauche viser, croix gauche recharger, croix haut/bas changer d'arme, LB grenade, RB compétence, R3 coup.
Les menus se règlent à la souris.

## Prologue : modèles 3D du vaisseau-prison (PC Windows)

La refonte du jeu (voir `docs/PREPRODUCTION.md`) commence par l'évasion d'un vaisseau-prison.
67 modèles gratuits (CC BY, environ 2,5 Go) ont été choisis pour la construire : héros, détenus, gardes, pirates, cellules, coursives, consigne, machines, hangar, vaisseaux, armes.
Il faut un compte Sketchfab gratuit et son token (https://sketchfab.com/settings/password, section *API Token*). Unity fermé, dans PowerShell :

```powershell
irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-prologue.ps1 | iex
```

Les modèles sont rangés par lieu dans `Assets\_Projet\Externes\Prologue\`, avec les crédits dans `CREDITS-PROLOGUE.txt`. Liste et aperçus : `docs/PROLOGUE-MODELES.md`.

## Modèles 3D pour une vitrine web (Mac ou Linux)

`outils/telecharger-vitrine.sh` télécharge un pack léger (environ 175 Mo, sans rien installer) de modèles, textures et ciels libres,
au format web (.glb, .gltf, .jpg, .hdr), rangés par lieu : `commun`, `horizon`, `dunes`, `largage`, `anneaux`.
Le fichier `modeles.json` décrit chaque fichier (lieu, rôle, taille conseillée, licence) et `CREDITS.txt` liste les crédits.

```bash
curl -fsSL https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-vitrine.sh | bash -s -- ~/jeu/vitrine/models
```

Avec un token Sketchfab gratuit (`SKETCHFAB_TOKEN=... ` devant `bash`), il ajoute le ver des sables animé, un astronaute animé, des capsules, une statue, des astéroïdes et une antenne.
Les modèles Kenney (CC0) sont aussi rangés dans `maquette/modeles/` : le script s'en sert si le site de Kenney refuse le téléchargement.

`vitrine/vitrine-modeles.html` montre ces modèles dans les 4 lieux de la vitrine : L'Horizon, Le Tombeau et le Léviathan, Site de largage, Champ d'anneaux.
Elle les charge avec GLTFLoader (et DRACOLoader pour l'astronaute compressé), éclaire chaque lieu avec son ciel .hdr (RGBELoader) et affiche `CREDITS.txt`.
Pose-la à côté du dossier `models`, puis sers le dossier :

```bash
curl -fsSL https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/vitrine/vitrine-modeles.html -o ~/jeu/vitrine/vitrine-modeles.html
cd ~/jeu/vitrine && python3 -m http.server 8765
```

Ouvre ensuite `http://localhost:8765/vitrine-modeles.html`. Les touches 1 à 4 changent de lieu. Pour lire un autre dossier de modèles, ajoute `?models=autre/dossier/` à l'adresse.
S'il manque des fichiers, la vitrine s'affiche quand même et les liste en bas à gauche. Sans token Sketchfab, le ver des sables est généré par le code.

## Maquette jouable (navigateur)

`maquette/echos-ether.html` : prototype WebGL (Three.js) de la Phase 1, publié comme Artifact claude.ai.
Planète en relief à niveaux de détail, textures Poly Haven, modèles Kenney Space Kit (licence CC0, regroupés dans `maquette/modeles/kenney.json`),
navette de largage et d'extraction, trou noir et lentille gravitationnelle, dilatation du temps, jauge de bruit et ver, cristaux d'Éther, monolithe.
Pour tester en local : `python3 -m http.server` dans `maquette/`, puis ouvrir `http://localhost:8000/echos-ether.html` (ajouter `#debug` pour l'accès de test).
