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
4. Menu **Echos d'Ether > Vérifier l'installation** : contrôle HDRP, glTFast, ressources, assets téléchargés, sons branchés et composants de la scène.
   Le rapport est copié dans le presse-papiers (Ctrl+V pour le coller dans une conversation).
5. **Play** : menu principal, enrôlement (nom, spécialité, couleurs, aperçu 3D), arrivée en navette (Espace pour passer), didacticiel en 13 étapes ou mission complète.

Relancer le menu complète l'installation sans rien déplacer. *Replacer les lieux de jeu autour du joueur* les repose à neuf.
*Effacer le profil et les réglages enregistrés* remet l'enrôlement à zéro.

| Dossier | Contenu |
|---|---|
| `Scripts/Jeu` | `GestionnaireJeu` (états de partie), `Largage` (arrivée), `AmbianceSonore`, `Didacticiel`, `Mission`, `JaugeBruit`, `CristalEther`, `Scanner`, `Ruche`, `BulleTemporelle`, `Monolithe`, `Navette`, `BaliseExtraction`, `Horloges`, `Transmissions`, `BanqueSons`, `ProfilArpenteur`, `Specialites`, `Reglages`, `Planete`, `Formes`, `Invite` |
| `Scripts/Joueur` | `EtatArpenteur`, `ApparenceArpenteur`, `EntreesJeu`, `CameraNavette`, `CameraSecousse` (plus les scripts existants) |
| `Scripts/Interface` | `InterfaceJeu` : HUD et écrans en UI Toolkit, construits par le code |
| `Resources` | `EchosEtherUI.uss` (styles), `EchosEtherTheme.tss` (thème), `Lueur.shader` (lueurs HDRP des bulles, de l'onde du scanner et des faisceaux) |
| `Editor` | `InstallateurJeu` (menu d'installation), `Diagnostic` (vérification), `ImportExternes` (réglage des textures), `ConstructeurScene` |

Commandes : Z Q S D se déplacer, souris regarder, Maj courir, C marcher en silence, Espace sauter, E récolter / interagir / navette,
F scanner, J journal, M son, N passer une étape du didacticiel, Échap pause, Espace passer l'arrivée en navette. Le code marche avec le nouvel Input System et l'ancien Input Manager.

Manette (avec le nouvel Input System) : stick gauche avancer, stick droit regarder, A sauter, gâchette droite ou L3 courir, B marcher en silence,
X récolter / interagir, Y scanner, Select journal, Start pause, croix droite passer une étape. En navette : A ou RB monter, B ou LB descendre, gâchette droite boost.
Les menus se règlent à la souris.

## Maquette jouable (navigateur)

`maquette/echos-ether.html` : prototype WebGL (Three.js) de la Phase 1, publié comme Artifact claude.ai.
Planète en relief à niveaux de détail, textures Poly Haven, modèles Kenney Space Kit (licence CC0, regroupés dans `maquette/modeles/kenney.json`),
navette de largage et d'extraction, trou noir et lentille gravitationnelle, dilatation du temps, jauge de bruit et ver, cristaux d'Éther, monolithe.
Pour tester en local : `python3 -m http.server` dans `maquette/`, puis ouvrir `http://localhost:8000/echos-ether.html` (ajouter `#debug` pour l'accès de test).
