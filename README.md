# Échos d'Éther

Jeu coop en monde ouvert, axé sur l'émerveillement et le mystère. Voir [`docs/GDD.md`](docs/GDD.md).

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
5. Prochaine étape : passe plein écran (Custom Pass HDRP) pour la **lentille gravitationnelle** autour du trou noir.
