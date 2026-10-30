# Pour Claude en local : travailler sur Échos d'Éther dans Unity

> **État au 30 octobre 2026.** Le jeu commence par un prologue : le héros, amnésique, s'évade du Colosse, un vaisseau-prison attaqué par des pirates, et vole une navette.
> Lis d'abord [`PREPRODUCTION.md`](PREPRODUCTION.md), puis les priorités dans [`A-FAIRE.md`](A-FAIRE.md).
> - Le code du prologue est dans `Assets/_Projet/Scripts/Prologue/`. Sa scène d'essai : menu **Echos d'Ether → Prologue → Créer la scène d'essai des vues** (le hangar, le héros, la navette, les effets).
> - Le code de l'ancien jeu (l'Arpenteur, les Échardes, la planète) reste dans le projet, mais le prologue n'en dépend pas. Son mode d'emploi est dans [`archives/LISEZ-MOI.md`](../archives/LISEZ-MOI.md).
>
> **Ta prochaine tâche** :
> 1. Une fois les modèles du prologue téléchargés (`outils/telecharger-prologue.ps1`, liste dans [`PROLOGUE-MODELES.md`](PROLOGUE-MODELES.md)), vérifie qu'ils s'importent tous dans `Assets/_Projet/Externes/Prologue`.
> 2. Crée une scène `Assets/_Projet/Local/GaleriePrologue.unity` qui les aligne par dossier, à la bonne échelle (un humain fait 1,80 m), sous une lumière neutre.
> 3. Fais une capture par dossier dans `docs/captures/prologue/`.
> 4. Note dans `docs/CORRECTIONS-LOCALES.md` ceux qui posent problème : import raté, matériaux roses, échelle fausse, trop lourd pour la RTX 3060.
> 5. Monte ensuite le vaisseau-prison en **formes grises** dans une scène `Assets/_Projet/Local/PrologueGris.unity`. La maquette est déjà faite, à l'échelle réelle : [`prologue/maquette-vaisseau-prison.json`](prologue/maquette-vaisseau-prison.json), à voir dans `vitrine/atelier-prologue.html`.
>    - Chaque bloc est un cube de dimensions `dims` (en mètres), posé par le bas à `position`.
>    - Chaque modèle de `Externes/Prologue/<cle>` est mis à l'échelle sans déformation pour tenir dans son `cadre` (un 0 laisse ce côté libre), puis posé par le bas, centré.
>    - Le champ `repere` du fichier décrit tout. Attention : c'est le repère glTF, droitier, alors qu'Unity est gaucher. Retourne l'axe comme le fait glTFast à l'import des modèles.
>    - Avant l'import, remets les modèles à leur taille réelle : `py outils\normaliser-prologue.py Assets\_Projet\Externes\Prologue` (Python 3 gratuit, python.org). Ils seront alors en mètres, pieds au sol.
>    - Respecte la [direction artistique](prologue/BIBLE-DA.md) : un réglage de couleurs commun (*color grading* HDRP), et les recolorations `da` de [`prologue/tailles-modeles.json`](prologue/tailles-modeles.json) (`outils/recolorer-prologue.py` les applique aux fichiers).
> 6. Ouvre la scène d'essai des vues et joue : note dans `docs/CORRECTIONS-LOCALES.md` ce qui ne va pas (prises d'armes, effets, caméras, navette). Clic droit sur le composant `PriseArme` → « Contrôler les prises (bot) » donne le rapport des mains.
>
> Le reste de ce fichier décrit l'ancien jeu, toujours valable pour ses outils.

Ce fichier est pour toi, Claude qui pilote Unity sur le PC (MCP for Unity). Une autre session de Claude, dans le cloud,
écrit le code du jeu et le publie sur GitHub (branche `echos-ether`). Le joueur le copie dans le projet avec `outils/installer.ps1`.
Cette session-là **ne peut pas lancer Unity** : tout ce qu'elle a écrit compile, mais n'a jamais tourné. Ton rôle est d'être ses yeux et ses mains dans l'éditeur.

## Qui fait quoi

| | Claude cloud | Toi (Claude local) |
|---|---|---|
| Code C# du jeu (`Assets/_Projet/Scripts`, `Editor`, `Resources`) | écrit et publie | teste, corrige ce qui bloque, signale |
| Scène, décor, matériaux, réglages Unity, modèles importés | conseille | fait |
| Tests dans l'éditeur, Console, captures | ne peut pas | fait |

## Règles

1. **Tu as le droit d'écrire du code.** Mets tes nouveaux scripts dans `Assets/_Projet/Local/` : `installer.ps1` n'y touche jamais.
2. Tu peux aussi corriger les scripts existants quand c'est nécessaire. Mais la prochaine mise à jour les remplacera : `installer.ps1` garde ta version dans `Sauvegardes\<date>\`.
   **Note chaque correction dans `docs/CORRECTIONS-LOCALES.md`** (fichier, ligne, avant, après, pourquoi) pour que la session cloud la reprenne pour de bon.
3. Ne renomme ni ne déplace aucun script du jeu : les composants de la scène pointent dessus.
4. Ne modifie pas le contenu de `Assets/_Projet/Externes` (assets téléchargés). Changer leurs réglages d'import, c'est permis.

## Démarrage rapide

Tous les outils sont dans le menu **Echos d'Ether** de Unity. Tu peux les lancer par MCP avec l'outil qui exécute un élément de menu
(`execute_menu_item` dans MCP for Unity, selon la version).

1. Ouvre la scène du jeu (par exemple `Assets/_Projet/Scenes/LisiereHorizon.unity`).
2. **Echos d'Ether/Installer le jeu dans la scène (menus, didacticiel, mission)** : ajoute ou complète tout. Sans risque à relancer.
3. **Echos d'Ether/Vérifier l'installation** : rapport dans la Console (et dans le presse-papiers).
4. **Echos d'Ether/Test automatique (rapport dans docs)** : le jeu se joue tout seul pendant environ une minute, sans que personne ne touche à rien.
   Il parcourt le menu, l'enrôlement, l'arrivée en navette, les 6 armes, les 3 créatures, les 6 compétences, les dégâts et le rapatriement, les talents, la pause, la navette et l'extraction.
   Ensuite :
   - lis **`docs/RAPPORT-TEST.md`** : échecs, erreurs de la Console sans doublons, images par seconde ;
   - regarde les captures dans **`docs/captures/`**.
5. Corrige ce qui bloque, relance le test, recommence jusqu'à zéro échec et zéro erreur.

## Habiller l'Arpenteur (animations)

L'Arpenteur est une capsule. Pour lui donner un corps :

1. Sélectionne le modèle de combinaison dans la fenêtre Project.
2. Lance **Echos d'Ether/Utiliser le modèle sélectionné comme corps de l'Arpenteur** : le modèle est mis à 1,80 m, pieds au sol, la capsule est cachée, l'animation et les couleurs sont branchées.
3. S'il regarde en arrière : **Echos d'Ether/Retourner le corps de l'Arpenteur (180°)**.

`AnimateurArpenteur` joue les clips du modèle s'il en a. Sinon, il anime son **squelette** par le code (les os sont trouvés par leur nom : Hips, Spine, UpperArm, Thigh…, ou par l'avatar Humanoid). Les bras tiennent l'arme par cinématique inverse.
Un modèle **sans squelette** ne peut bouger qu'en bloc. Dans ce cas, dis au joueur de le passer dans Mixamo (gratuit, mixamo.com, depuis son navigateur), puis importe le .fbx obtenu.

## Ce que la session cloud n'a pas pu vérifier (à regarder en premier)

- **Shader `Resources/Lueur.shader`** (HDRP, passe ForwardOnly, additif) : erreurs de compilation éventuelles dans l'Inspector du shader.
  Il sert aux bulles, au scanner, aux faisceaux, aux réacteurs et à tous les effets de combat.
- **Interface UI Toolkit** construite par le code (`Scripts/Interface`) : textes lisibles, panneaux qui ne se chevauchent pas en 1920×1080 et en 2560×1440. Styles dans `Resources/EchosEtherUI.uss`.
- **Armes Kenney** (`Externes/Modeles/Kenney/blaster-kit`, importées par glTFast) :
  - elles doivent pointer canon vers l'avant, sinon décoche **Canon vers moins Z** sur le composant `Arsenal` de l'Arpenteur ;
  - leur taille dans la main et la position des mains (`VueArme`) sont à vérifier.
- **Créatures** (`Combat/Echarde.cs`) :
  - elles doivent marcher sur le relief sans s'enfoncer ni flotter ;
  - les pattes ne doivent pas traverser le sol ;
  - elles ne doivent pas rester bloquées contre les rochers.
- **Lumières créées par le code** (éclairs, explosions, réacteurs) : intensité visible en HDRP, ni éblouissante ni invisible.
- **Sons** branchés par l'installateur (champs de `BanqueSons` sur « Systemes de jeu ») : un champ vide n'est pas grave.
- **Performances** : viser 60 images/s sur une RTX 3060 avec une dizaine de créatures. La ligne « Images par seconde » du rapport en donne une idée.

## Corrections courantes

| Symptôme | Cause probable | Que faire |
|---|---|---|
| Écran noir ou sans interface | `Resources/EchosEtherUI.uss` ou `EchosEtherTheme.tss` absent | relancer `installer.ps1`, puis Vérifier l'installation |
| Lueurs roses ou invisibles | `Lueur.shader` en erreur | lire l'erreur dans l'Inspector du shader, corriger, noter dans CORRECTIONS-LOCALES |
| Les touches ne font rien | Input System seul ou ancien Input Manager seul | *Project Settings > Player > Active Input Handling = Both* |
| Arme à l'envers | orientation du modèle glTF | décocher **Canon vers moins Z** sur `Arsenal` |
| Personnage figé | pas de squelette sous l'Arpenteur | voir « Habiller l'Arpenteur » |
| Deux navettes au même endroit | vaisseau de décor en plus de la navette du jeu | supprimer le vaisseau de décor (Vérifier l'installation le signale) |
| Cristaux sous des rochers | décor ajouté après l'installation | **Echos d'Ether/Replacer les lieux de jeu autour du joueur** |
| Pas de son | aucun AudioListener, ou son coupé (touche M) | un seul AudioListener, sur la caméra |

## Repères dans le code

- **États de partie** (`Jeu/GestionnaireJeu.cs`) :
  - le joueur passe par Menu → Enrôlement → Largage (arrivée) → Jeu ⇄ Vol, avec Pause, puis Extraction → Fin ;
  - recommencer recharge la scène.
- **Bruit** (`Jeu/JaugeBruit.cs`) :
  - à 100, la Ruche frappe (`Jeu/Ruche.cs`) ;
  - les tirs, les explosions et les créatures émettent des bruits via `JaugeBruit.Signaler`.
- **Combat** (`Scripts/Combat`) :
  - `Arsenal` : tir, munitions ;
  - `VueArme` : arme tenue et ses animations ;
  - `Echarde` et `Essaim` : créatures ;
  - `VieArpenteur` : intégrité de la combinaison ;
  - `EffetsCombat` : effets visuels ;
  - `Degats` : interface `ICible`.
- **Compétences** : `Jeu/Competences.cs` (catalogue, courbe d'expérience) et `Jeu/CompetencesArpenteur.cs` (compétence active, talents). Les données du joueur sont dans `Jeu/ProfilArpenteur.cs` (PlayerPrefs).
- **Outils de l'éditeur** (`Editor`) :
  - `InstallateurJeu` : installation ;
  - `Diagnostic` : vérification ;
  - `MenuTestAutomatique` (le test lui-même est dans `Scripts/Outils/TestAutomatique.cs`) ;
  - `CorpsArpenteur` : habiller l'Arpenteur ;
  - `ImportExternes` : réglages des textures.

## Transmettre à la session cloud

Le joueur fait le lien entre les deux sessions. À la fin de ton travail, dis-lui de copier-coller à l'autre Claude :

1. `docs/RAPPORT-TEST.md` : le dernier rapport du test automatique ;
2. `docs/CORRECTIONS-LOCALES.md` : ce que tu as changé dans les scripts du jeu ;
3. si utile, 2 ou 3 captures de `docs/captures/`.

Format conseillé pour `docs/CORRECTIONS-LOCALES.md` :

```
## 2026-10-24 · Scripts/Combat/VueArme.cs, ligne 123
Problème : l'arme traversait la main en visée.
Avant : cibleP = epaule + viseeDir * 0.3f ...
Après : cibleP = epaule + viseeDir * 0.4f ...
Vérifié : test automatique sans erreur, capture test-05-arme-ardent.png.
```
