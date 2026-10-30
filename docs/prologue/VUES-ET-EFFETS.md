# Les vues et les effets du prologue

> Version 2, 30 octobre 2026. Ce qui est fait : la vue 3e personne, les deux vues de la navette (cockpit et poursuite), et les effets visuels du Colosse. Chaque partie existe en deux versions :
> - **dans le navigateur**, pour essayer tout de suite ;
> - **dans Unity** (HDRP), en C#.
>
> Les deux versions ont le même comportement et les mêmes réglages.

## 1. Ce qu'il faut retenir

1. **La vue 3e personne.** La caméra regarde par-dessus l'épaule du héros, au bout d'un bras :
   - le bras se raccourcit devant un mur : il rentre vite, puis ressort doucement ;
   - X fait passer la caméra d'une épaule à l'autre ;
   - la caméra a quatre réglages, selon ce que fait le héros : normal, visée, accroupi, course.
2. **La navette a deux vues** (V pour passer de l'une à l'autre) :
   - **poursuite** : derrière et au-dessus. La navette pivote dans l'image avant que la caméra ne la rattrape, et le champ de vision s'ouvre avec la vitesse ;
   - **cockpit** : les yeux du pilote, avec le pupitre, les écrans et la verrière. La tête encaisse les accélérations.
3. **Le héros tient un fusil à deux mains.** Il joue les animations du studio maison (voir [`PERSONNAGES.md`](PERSONNAGES.md), § 3). Les mains sont posées sur l'arme par le **bot de prise** (voir [`PRISES-ARMES.md`](PRISES-ARMES.md)).
4. **Neuf effets.**
   - **Les fumées, les flammes et les explosions sont de vraies simulations** : les flipbooks de Unity Labs, faits dans Houdini et publiés en CC0 (domaine public, utilisables dans un jeu vendu).
   - Le reste est fait par le code : étincelles, tirs, champ de force, propulseur, rayon d'éther, gyrophare.

![Les vues](vues/vues.jpg)

En vidéo : [la vue 3e personne et la navette](personnages/videos/vues-3e-personne-et-navette.mp4) (18 s).

## 2. Les essayer dans le navigateur

Sur le Mac, dans le dossier du jeu (celui qui contient `prologue/00-Animations`, rempli par `telecharger-prologue.sh`) :

```
cd ~/jeu
for f in vues.html banc-effets.html banc-pnj.html effets-colosse.js prise-armes.js; do curl -fsSLO https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/vitrine/$f; done
mkdir -p effets && for f in fumee_volutes_8x8 fumee_volutes2_8x8 fumee_fine_20x4 explosion_5x5 boule_feu_8x8 flamme_16x4; do curl -fsSL -o effets/$f.webp https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/vitrine/effets/$f.webp; done
python3 -m http.server 8766
```

Puis, dans Chrome :
- **les vues** : http://localhost:8766/vues.html?racine=http://localhost:8766/prologue/
- **les effets** : http://localhost:8766/banc-effets.html

Sans les fichiers d'animation, la page des vues marche quand même : le héros est alors une simple silhouette.

**Les commandes** (positions physiques des touches : ZQSD en AZERTY, WASD en QWERTY) :

| À pied | | En navette | |
|---|---|---|---|
| Z Q S D | marcher | Z / S | mettre ou couper les gaz |
| Maj | courir | Maj | postcombustion |
| C | s'accroupir | souris | tangage et lacet |
| clic droit | viser | Q / D | roulis |
| clic | tirer | Espace / C | monter, descendre |
| R | recharger | V | cockpit ou poursuite |
| X | changer d'épaule | E | descendre (navette posée dans le hangar) |
| F | ranger ou sortir le fusil | | |
| E | monter à bord (près de la porte de la navette) | | |

**Le parcours de l'essai.** On commence au fond du hangar :
1. la navette est posée devant le sas ;
2. on monte à bord (E), on met les gaz ;
3. on traverse le champ de force du sas et on sort dans l'espace, avec la planète et les débris.

## 3. Les effets

![Les effets](vues/effets.jpg)

En vidéo : [un travelling le long du banc des effets](personnages/videos/effets-colosse.mp4) (19 s), du feu à l'explosion.

| Effet | Ce qu'il montre | Où dans le Colosse | Navigateur (`effets-colosse.js`) | Unity (composant) |
|---|---|---|---|---|
| **Court-circuit** | Des gerbes d'étincelles irrégulières, parfois en rafale, qui rebondissent au sol ; un éclair de lumière, un peu de fumée | Les panneaux abîmés de la couche Chantier, les ateliers | `fx.etincelles(position, { direction, sol })` | `EtincellesPanne` |
| **Vapeur** | Un jet serré qui ralentit, s'élargit et monte en se diluant | Les conduites, les machines, la nappe au sol | `fx.vapeur(position, direction)` | `FuiteVapeur` |
| **Tir d'énergie** | L'éclair de bouche, un trait blanc dans un halo de couleur, l'impact : étincelles, lueur, fumée, une brûlure qui refroidit | Les combats ; rouge pour l'Empire, cyan pour le héros | `fx.tir(depart, arrivee, { couleur })` | `EffetsColosse.Tir(…)` |
| **Champ de force** | Des hexagones qui respirent, un balayage, un cadre. Un tir y laisse une onde. Il s'éteint en clignotant | Les portes des cellules, le sas du hangar | `fx.champ(largeur, hauteur)`, puis `.toucher(point)` | `ChampDeForce` (`Toucher`, `Couper`, `Allumer`) |
| **Propulseur** | Une flamme de plasma : deux cônes turbulents, les « diamants » du jet, une lueur, des grains ; il suit les gaz | La navette, le vaisseau pirate | `fx.propulseur()`, puis `.puissance` | `Propulseur` |
| **Rayon d'éther** | Une colonne de lumière cyan, des grains qui montent en spirale, une onde au sol toutes les 1,6 s | La chambre des reliques (la capsule du Veilleur) | `fx.rayonEther(hauteur)` | `RayonEther` |
| **Gyrophare** | Deux faisceaux opposés qui tournent dans la poussière, et la lumière qui balaie les murs | L'alarme, quand l'évasion est découverte | `fx.gyrophare()`, puis `.actif` | `Gyrophare` |
| **Explosion** | La boule de feu simulée et sa fumée noire, l'éclair, des étincelles et des débris incandescents, l'onde au sol, puis la colonne de fumée qui monte | Les grenades, les réservoirs, l'abordage des pirates | `fx.explosion(position, { taille })` | `EffetsColosse.Explosion(…)` |
| **Feu** | Des flammes simulées qui se chevauchent, des braises qui montent, une fumée sombre, une lumière qui vacille | Les débris après l'explosion, une conduite de gaz percée | `fx.feu(position, { taille })`, puis `.actif` | `Feu` |

**Les flipbooks** sont dans `vitrine/effets/` (WebP, pour le navigateur) et `Assets/_Projet/Resources/Effets/` (PNG, pour Unity) :

| Fichier | Ce que c'est | Sert à |
|---|---|---|
| `fumee_volutes`, `fumee_volutes2` | Des volutes de fumée (8 × 8 images) | La vapeur, la colonne de fumée des explosions |
| `fumee_fine` | Un filet de fumée (20 × 4) | Les courts-circuits, les impacts, le feu |
| `explosion` | Une explosion, du feu à la fumée (5 × 5) | L'explosion |
| `boule_feu` | Une boule de feu (8 × 8) | Le cœur de l'explosion |
| `flamme` | Une flamme en boucle (16 × 4) | Le feu |

Ils viennent de Unity Labs Paris (2016), sous licence CC0. La mention « Flipbooks VFX : Unity Labs (CC0) » dans les crédits est facultative.

Sans ces fichiers, chaque effet garde sa texture faite par le code.

**Pour aller plus loin**, deux sources de très bonne qualité, à récupérer à la main :
- **Unity Spaceship Demo** ([github.com/Unity-Technologies/SpaceshipDemo](https://github.com/Unity-Technologies/SpaceshipDemo)) :
  - des effets VFX Graph de science-fiction, en HDRP : étincelles, vapeur, hologrammes, réacteurs, arcs électriques ;
  - licence *Unity Companion License* : utilisable dans un jeu fait avec Unity, donc dans le nôtre ;
  - c'est le meilleur gisement pour la version Unity, à ouvrir sur le PC.
- **CGHEVEN** ([cgheven.com](https://cgheven.com), section *Flipbooks*) :
  - des flipbooks gratuits en CC0 : éclairs de bouche réalistes (*Muzzle Flash 01*), jets de vapeur continus (*Steam Jet*), fuites d'énergie (*Energy Leaks*, parfaites pour l'éther) ;
  - leur téléchargement passe par le site (bouton *Download*, sans compte) ;
  - à déposer ensuite dans `vitrine/effets/` et `Resources/Effets/`.

Les couleurs suivent la [bible](BIBLE-DA.md) :
- le rouge est réservé à l'Empire (tirs, alarme) ;
- le cyan à l'éther et aux Bâtisseurs ;
- l'orange sodium aux lampes de chantier.

## 4. Dans Unity

**Pour tout voir d'un coup :**
1. Importer `prologue/00-Animations/` dans le projet. Les trois FBX (`UAL1_Standard`, `UAL2_Standard`, `EE_Animations`) se règlent en *Rig* → *Animation Type* : **Humanoid**.
2. Menu **Echos d'Ether → Prologue → Créer la scène d'essai des vues (hangar, navette)**. Il construit `Assets/_Projet/Scenes/PrologueEssaiVues.unity`, avec :
   - le hangar, son sas en champ de force, le gyrophare et le court-circuit ;
   - le héros : le mannequin du studio, avec ses animations et son fusil ;
   - la navette, avec ses propulseurs et l'intérieur du cockpit ;
   - les deux caméras et la passerelle entre elles.
3. **Play**, puis clic pour prendre la souris.

**Le banc des effets** : un objet vide dans une scène vide, le composant `BancEffetsColosse`, puis Play.
- T tire une rafale ;
- C coupe ou rallume le champ ;
- G arrête ou relance l'alarme ;
- + et - règlent le propulseur.

**Les pièces, une par une** (dossier `Assets/_Projet/Scripts/Prologue/`) :

| Composant | Rôle |
|---|---|
| `CameraTroisiemePersonne` | Sur la caméra à pied. `cible` = le héros. `murs` = ce que la caméra ne traverse pas. Les quatre réglages (normal, visée, accroupi, course) se changent dans l'inspecteur. |
| `HerosPrologue` | Sur le héros, avec un `CharacterController`. Il joue lui-même ses animations : un graphe Playables, sans Animator Controller à monter. Pour lui donner ses clips : clic droit sur le composant → « Trouver les animations du prologue ». `fusil` = l'objet qui porte `ArmeTenue`. |
| `PriseArme`, `ArmeTenue` | Le bot de prise : les mains se posent exactement sur la poignée et le garde-main, quel que soit le personnage (voir [`PRISES-ARMES.md`](PRISES-ARMES.md)). |
| `Feu` | Un feu : flammes simulées, braises, fumée, lumière. |
| `ControleVaisseau` | Sur le vaisseau, avec un `Rigidbody` sans gravité. Il règle les gaz, la postcombustion, l'assistance de vol et la vitesse bridée dans la zone du hangar. `place` = les yeux du pilote, `porte` = là où l'on monte. |
| `CameraVaisseau` | Sur la caméra de vol : les vues poursuite et cockpit. `interieur` = l'intérieur du cockpit, montré seulement en vue cockpit. |
| `PasserelleVaisseau` | Le passage à pied ↔ à bord : E près de la porte, E une fois posé. Il bascule les caméras et les commandes. |
| `EffetsColosse` | Les textures, matériaux, maillages et flipbooks des effets, et les effets ponctuels (`Gerbe`, `Tir`, `Impact`, `Explosion`). |

**Les shaders** sont dans `Assets/_Projet/Resources/` (HDRP) :
- `Particule` : additif ou fondu, avec particules molles ;
- `Volume` : flamme, rayon, faisceau ;
- `ChampDeForce`.

Comme la lueur existante, ils écrivent la couleur telle quelle : c'est le bloom du *Volume* HDRP qui fait briller les effets. Il faut donc un *Bloom* actif dans le profil de la scène.

Les nouvelles touches sont dans `EntreesJeu` : `Accroupir` (C), `ChangerEpaule` (X), `ChangerVue` (V). Elles marchent aussi à la manette : B, croix droite, croix haut.

**La vérification.** Tout le C# compile, avec et sans le nouveau *Input System*, contre une copie de l'API d'Unity. Les shaders, eux, n'ont pas pu être compilés hors d'Unity. Ils reprennent les mêmes inclusions HDRP que `Lueur.shader`, qui marche déjà.

## 5. Ce qu'il reste à faire

| Quoi | Pourquoi |
|---|---|
| Ouvrir la scène d'essai dans Unity et régler à l'œil : intensité du bloom, lumens des lampes, couleur du champ | Les valeurs viennent du navigateur ; l'exposition de HDRP est différente |
| Remplacer le mannequin par le héros (`01-Joueur/tenue-detenu`, Humanoid) et le fusil par un vrai modèle | La scène d'essai prend ce qu'elle trouve |
| Le vrai vaisseau-navette (Meshy, prompt 12 pour le pirate) à la place de la navette en blocs | Même chose |
| Poser les effets dans la maquette du Colosse : courts-circuits dans les ateliers, vapeur dans les machines, champs de force aux cellules, rayon d'éther chez les reliques, gyrophares aux passerelles | Le décor est prêt ; les effets se posent comme des objets |
