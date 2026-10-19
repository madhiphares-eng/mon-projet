# Échos d'Éther

Jeu coop en monde ouvert, axé sur l'émerveillement et le mystère. Voir [`docs/GDD.md`](docs/GDD.md).

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

## Maquette jouable (navigateur)

`maquette/echos-ether.html` : prototype WebGL (Three.js) de la Phase 1, publié comme Artifact claude.ai.
Planète en relief à niveaux de détail, textures Poly Haven, modèles Kenney Space Kit (licence CC0, regroupés dans `maquette/modeles/kenney.json`),
navette de largage et d'extraction, trou noir et lentille gravitationnelle, dilatation du temps, jauge de bruit et ver, cristaux d'Éther, monolithe.
Pour tester en local : `python3 -m http.server` dans `maquette/`, puis ouvrir `http://localhost:8000/echos-ether.html` (ajouter `#debug` pour l'accès de test).
