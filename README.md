# Échos d'Éther

Jeu d'action et d'aventure à la troisième personne, réaliste (Unity 6 HDRP). Il commence par un **prologue** : amnésique, le héros s'évade du Colosse, un vaisseau-prison de l'Empire attaqué par des pirates, et vole une navette. Ensuite, la galaxie est ouverte. Le projet complet : [`docs/PREPRODUCTION.md`](docs/PREPRODUCTION.md).

- **Voir les avancées** : la bibliothèque en ligne (personnages, animations, effets, sons, décors, cinématiques, gameplay), lien ci-dessous.
- **Ce qu'il reste à faire, dans l'ordre** : [`docs/A-FAIRE.md`](docs/A-FAIRE.md).

<!--BIBLIOTHEQUE-->

## Où trouver quoi

| Où | Ce qu'il y a |
|---|---|
| [`docs/PREPRODUCTION.md`](docs/PREPRODUCTION.md) | Le jeu : l'histoire, les références, le prologue, la galaxie ouverte |
| [`docs/prologue/`](docs/prologue/) | Le prologue : [bible de direction artistique](docs/prologue/BIBLE-DA.md), [plan du niveau](docs/prologue/PLAN-NIVEAU.md), [personnages](docs/prologue/PERSONNAGES.md), [prises d'armes](docs/prologue/PRISES-ARMES.md), [vues et effets](docs/prologue/VUES-ET-EFFETS.md). Les images sont rangées par sujet : `catalogue/`, `maquette/`, `da/`, `personnages/` (et ses vidéos), `plans/`, `prises/`, `vues/` |
| [`docs/PROLOGUE-MODELES.md`](docs/PROLOGUE-MODELES.md) | Les 101 modèles du prologue, leurs auteurs et licences |
| [`docs/IA-3D.md`](docs/IA-3D.md), [`docs/prologue/ia/`](docs/prologue/ia/) | Fabriquer des modèles avec une IA (Meshy) ; les déposer dans [`depot-modeles/`](depot-modeles/LISEZ-MOI.md) |
| [`docs/ASSETS.md`](docs/ASSETS.md), [`docs/UNIVERS-TECHNIQUE.md`](docs/UNIVERS-TECHNIQUE.md) | Pour la suite : les assets libres de la galaxie ouverte, la fiche technique de l'univers |
| [`docs/CLAUDE-LOCAL.md`](docs/CLAUDE-LOCAL.md) | Le guide du Claude du PC, qui travaille dans Unity |
| `vitrine/` | Les outils du navigateur : l'atelier (la maquette du Colosse), le banc des personnages, le banc des effets, l'essai des vues, la galerie des modèles |
| `outils/` | Les scripts : téléchargement, taille réelle, recoloration, squelettes, studio d'animation, maquette |
| `Assets/_Projet/` | Le code Unity. `Scripts/Prologue/` : le prologue. Le reste : des utilitaires communs et le code de l'ancien jeu, gardé dans le projet mais dont le prologue ne dépend pas |
| `prologue-anim/` | La bibliothèque d'animations : 84 libres (Quaternius, CC0) et 23 du studio maison |
| `ressources/` | Le kit d'armes de Kenney (CC0), en copie de secours pour `outils/telecharger-assets.ps1` |
| [`archives/`](archives/LISEZ-MOI.md) | L'ancien jeu (avant la refonte du 28 octobre 2026) et les prompts déjà exécutés |

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

## Prologue : modèles 3D du vaisseau-prison (PC Windows)

La refonte du jeu (voir `docs/PREPRODUCTION.md`) commence par l'évasion d'un vaisseau-prison.
101 modèles gratuits (CC BY, environ 1,2 Go) ont été choisis pour la construire : héros, détenus, androïdes de sécurité, pirates, autres espèces de la galaxie, cellules, coursives, consigne, machines, hangar, vaisseaux, armes, reliques des Bâtisseurs.
Il faut un compte Sketchfab gratuit et son token (https://sketchfab.com/settings/password, section *API Token*). Unity fermé, dans PowerShell :

```powershell
irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-prologue.ps1 | iex
```

Les modèles sont rangés par lieu dans `Assets\_Projet\Externes\Prologue\`, avec les crédits dans `CREDITS-PROLOGUE.txt`. Liste et aperçus : `docs/PROLOGUE-MODELES.md`.
Sur Mac ou Linux : `outils/telecharger-prologue.sh`, `vitrine/galerie-prologue.html` pour les voir en 3D dans le navigateur, et `vitrine/atelier-prologue.html` pour les assembler dans le hangar du vaisseau-prison et visiter la scène à hauteur d'homme. L'atelier contient la maquette du niveau entier, à l'échelle, faite par `outils/maquette-prologue.py`. C'est le Colosse : le niveau construit d'après le plan `docs/prologue/PLAN-NIVEAU.md`, avec la direction artistique de `docs/prologue/BIBLE-DA.md` (structure monumentale au-dessus de 4 m, chantier usé en dessous). Le même script dessine le plan à l'échelle, `docs/prologue/plan-niveau.svg`, et les plans détaillés par secteur avec leurs coupes (`docs/prologue/plans/`). La sécurité du vaisseau n'est faite que d'androïdes, et les détenus et pirates sont de plusieurs espèces. `vitrine/banc-pnj.html` teste chaque personnage et lui fait jouer les 84 animations de la bibliothèque libre (Quaternius, CC0, dans `prologue-anim/`), reciblées sur son squelette. `outils/rigger-statues.py` donne un squelette aux personnages qui n'en ont pas, `outils/recolorer-prologue.py` les recolore selon la bible ; compte rendu dans `docs/prologue/PERSONNAGES.md`. `outils/studio-animations.py` fabrique les animations propres au jeu (fusil à deux mains, androïdes, détenus, espèces). `vitrine/vues.html` fait essayer la vue 3e personne et la navette (cockpit, poursuite), `vitrine/banc-effets.html` les effets du Colosse ; leur version Unity est dans `Assets/_Projet/Scripts/Prologue/` (voir `docs/prologue/VUES-ET-EFFETS.md`). `outils/normaliser-prologue.py` remet les fichiers des modèles à leur taille réelle (mode d'emploi dans `docs/PROLOGUE-MODELES.md`).

## Assets libres pour la suite (PC Windows)

Assets 3D, textures, ciels et sons gratuits et réalistes (liens vérifiés, licences, crédits) : [`docs/ASSETS.md`](docs/ASSETS.md).
Pour les télécharger directement dans le projet (Unity fermé) :

```powershell
irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-assets.ps1 | iex
```
