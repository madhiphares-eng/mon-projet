# Modèles 3D du prologue : l'évasion du vaisseau-prison

67 modèles gratuits choisis pour le prologue (voir [`PREPRODUCTION.md`](PREPRODUCTION.md)), environ 2,5 Go.
Tous sont sous licence **CC BY** (créditer l'auteur, usage commercial permis), vérifiée par l'API Sketchfab le 28 octobre 2026.
Ils viennent de Sketchfab, qui demande un **compte gratuit** pour télécharger.

## Les ranger dans le projet Unity, en une commande

1. Crée un compte gratuit sur [sketchfab.com](https://sketchfab.com), puis copie ton token sur [sketchfab.com/settings/password](https://sketchfab.com/settings/password) (section *API Token*).
2. **Ferme Unity**, ouvre PowerShell et colle :

```powershell
irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-prologue.ps1 | iex
```

3. Colle ton token quand il le demande.

Les modèles arrivent rangés par lieu dans `C:\Projets\EchosEther\Assets\_Projet\Externes\Prologue\` :

```
01-Joueur\   tenue-detenu, combinaison
02-Prisonniers\   detenu-brute, detenu-vieux, detenu-atelier
03-Gardes\   garde, droide-securite
04-Pirates\   pirate-chef, pirate, pirate-masque, casque-pirate
05-Cellules\   bloc-cellules, porte-cellule, porte-isolement, toilettes, lavabo, lit-superpose
06-Couloirs\   couloir-principal, kit-interieur, kit-couloir, conduit-maintenance, tuyaux, porte-coulissante, sols
07-Consigne\   casier, armoire-blindee, porte-coffre
08-Machines\   reacteur, generateur, pylone-energie, coeur-fusion
09-Informatique\   baie-serveur, rangee-serveurs, console-murale, grand-ecran, terminal, cables
10-Hangar\   hangar, caisses, caisse-armes, cellule-energie, bidon-carburant
11-Vaisseaux\   vaisseau-prison, vaisseau-prison-variante, navette-a-voler, navette-transfert, vaisseau-pirate, capsule-sauvetage
12-Exterieur\   porte-sas, sas, cloison-etanche, rocher-spatial, debris-1, debris-2, gravats-breche
13-Armes\   fusil-gardes, mitraillette-gardes, pistolet-gardes, fusil-pirates, arme-pirates, fusil-lourd-pirates, pistolet-use, cle-a-molette
14-Accessoires\   extincteur, neons, caisse-munitions, caisse-cargo
```

Chaque modèle a son `LICENCE.txt`. Les crédits à citer dans le jeu sont regroupés dans `CREDITS-PROLOGUE.txt`, à la racine du projet.
On peut relancer le script sans risque : ce qui est déjà téléchargé est sauté.

## Déjà sur ton PC (téléchargé le 23 octobre)

Inutile de les retélécharger : ils servent aussi au vaisseau-prison.

| Où | Quoi | Pour |
|---|---|---|
| `Externes\PolyHaven\Modeles` | `overhead_crane`, `modular_industrial_pipes_01`, `modular_airduct_rectangular_01`, `modular_electric_cables`, `industrial_caged_sconce`, `worn_metal_rack`, `Barrel_01`, `portable_generator`, `old_military_compressor`, `vintage_spacecraft_instrument` | Pont roulant du hangar, gaines d'aération (chemins de maintenance), câbles, lampes grillagées, étagères, machines |
| `Externes\PolyHaven\Textures` | `metal_plate`, `metal_plate_02`, `rusty_painted_metal`, `metal_grate_rusty`, `blue_metal_plate`, `green_metal_rust`, `container_side`, `hangar_concrete_floor`, `concrete_wall_008`, `rusty_corrugated_iron` | Murs, sols et caillebotis du vaisseau, usés et rouillés |
| `Externes\ambientCG\Textures` | `MetalPlates017A`, `MetalPlates015A`, `MetalPlates006`, `MetalPlates013`, `PaintedMetal009`, `PaintedMetal016`, `PaintedMetal017`, `DiamondPlate008C`, `MetalWalkway014`, `CorrugatedSteel007A`, `Rust004` | Coque, passerelles, tôles peintes |
| `Externes\Ciel` (si tu as pris le groupe 5) | Cartes d'étoiles 8K, planètes 8K (Jupiter, Saturne…) | Le ciel vu depuis la coque, la planète sous le vaisseau |
| `Externes\NASA\Modeles` (si tu as pris le groupe 4) | Intérieur de l'ISS, station Gateway, Robonaut 2 | Références d'intérieurs exigus, robot de maintenance |

## Ce qu'il faudra faire ensuite (le Claude du PC, avec Blender MCP)

- **Alléger les modèles très lourds** : la combinaison (700 000 faces), le pirate masqué (900 000), la rangée de serveurs, le kit d'intérieur. Objectif : moins de 100 000 faces par personnage.
- **Retirer les écussons et marquages réels** : la tenue anti-émeute du garde porte des insignes de police, à remplacer par ceux de la prison.
- **Squelettes et animations** : les personnages sans squelette passent par [Mixamo](https://www.mixamo.com) (gratuit), comme les autres pour leurs animations.
- **Unifier les matériaux** : mêmes métaux usés, même palette, pour que 50 auteurs donnent un seul vaisseau.
- **La tenue de détenu** est provisoire : la version finale sera un humain réaliste, avec une combinaison de détenu ajustée dans Blender.

## Le héros (`01-Joueur`)

![Le héros](prologue/01-Joueur.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `tenue-detenu` | [Prisoner](https://sketchfab.com/3d-models/prisoner-e0898165a3e24b34bff0ca230d92c311) | asadkhan1881 | 11 686, 1 anim. | Le héros en tenue de détenu (provisoire : à remplacer plus tard par un humain réaliste) |
| `combinaison` | [Mercenary Astronaut](https://sketchfab.com/3d-models/mercenary-astronaut-7fd399d488ae4576a0fc62715892956c) | Peter_D | 704 714, 3 anim. | Sa propre combinaison, récupérée à la consigne (à alléger : 700 000 faces) |

## Les autres détenus (`02-Prisonniers`)

![Les autres détenus](prologue/02-Prisonniers.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `detenu-brute` | [Balthazar (rigged & animated)](https://sketchfab.com/3d-models/balthazar-rigged-animated-dbf9fe0cb81d4758ad2525a56d10e2fc) | Jungle Jim | 110 940, 1 anim. | Codétenu costaud, hostile ou allié |
| `detenu-vieux` | [Realistic Old Russian Guy (animated)](https://sketchfab.com/3d-models/realistic-old-russian-guy-animated-6b8009a6e4f747f59ccadb121adc3679) | Jungle Jim | 53 891, 1 anim. | Vieux détenu (indices, dialogues) |
| `detenu-atelier` | [Employee](https://sketchfab.com/3d-models/employee-9d1098bc72a74bb1b6371aef4b987100) | dotflare | 34 718, 4 anim. | Détenu des ateliers toxiques, masque à gaz |

## Les gardes (`03-Gardes`)

![Les gardes](prologue/03-Gardes.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `garde` | [S.W.A.T. Operator](https://sketchfab.com/3d-models/swat-operator-9e82fabf26194896b5ad4a364d864eab) | SpatialNeglect | 106 714, 1 anim. | Garde de la prison en tenue anti-émeute (écussons à remplacer) |
| `droide-securite` | [Sci-fi O.B. Robot Unit TH-ICC02 (animated)](https://sketchfab.com/3d-models/sci-fi-ob-robot-unit-th-icc02-animated-6c0764c2bc4c4aaa8e03ba1653f455a6) | Jungle Jim | 124 648, 1 anim. | Droïde de sécurité |

## Les pirates (`04-Pirates`)

![Les pirates](prologue/04-Pirates.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `pirate-chef` | [Space Pirate](https://sketchfab.com/3d-models/space-pirate-f94ee65527b14bdcb5bafd1ad573c8ca) | Odlab | 241 821 | Chef des pirates |
| `pirate` | [Scavenger](https://sketchfab.com/3d-models/scavenger-f029fe3297434ed6b5d03c3ffbc0585d) | Vasian-Digital3D | 98 743, 1 anim. | Pirate d'abordage |
| `pirate-masque` | [post apo character](https://sketchfab.com/3d-models/post-apo-character-9e77c91967ca4263ad347b395f53c1cb) | Am I dead? | 924 658 | Pirate masqué (lourd : à alléger) |
| `casque-pirate` | [Vintage 2725 " Spacer Armoured Pressure Helmet "](https://sketchfab.com/3d-models/vintage-2725-spacer-armoured-pressure-helmet-1059232ac0374767bf49ba95978f580f) | KallMor | 42 376 | Casque pressurisé de pirate, pour varier les tenues |

## Le bloc des cellules (`05-Cellules`)

![Le bloc des cellules](prologue/05-Cellules.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `bloc-cellules` | [Abandoned prison modular scene](https://sketchfab.com/3d-models/abandoned-prison-modular-scene-1e80d7053fd7440f929bc53db8433509) | Ali2-D2 | 138 134 | Bloc de cellules modulaire |
| `porte-cellule` | [Prison cell door](https://sketchfab.com/3d-models/prison-cell-door-f8148fab4dc44903b9644170edf0e58c) | Rivaldragon | 1 544 | Porte de cellule à barreaux |
| `porte-isolement` | [Door Of The Solitary Cell Of The Prison 14MB](https://sketchfab.com/3d-models/door-of-the-solitary-cell-of-the-prison-14mb-23db3362debb4d98a56856416e1e9c58) | Mehdi Shahsavan | 3 228 | Porte de cellule d'isolement (celle du héros) |
| `toilettes` | [Filthy Prison Toilet - 4096px²](https://sketchfab.com/3d-models/filthy-prison-toilet-4096px2-b8bdb85f59a44a8ea4b0783c8d22f373) | Mark Peters | 3 652 | Toilettes en acier |
| `lavabo` | [Filthy Metal Sink - 4096px²](https://sketchfab.com/3d-models/filthy-metal-sink-4096px2-dcd2b91f54014e95bd488e12f0d54d61) | Mark Peters | 18 610 | Lavabo en acier |
| `lit-superpose` | [Simple Bunk Bed](https://sketchfab.com/3d-models/simple-bunk-bed-9fb7c54715dd41b98b3dc67b367cf81d) | Blender3D | 9 324 | Lit superposé |

## Coursives et conduits (`06-Couloirs`)

![Coursives et conduits](prologue/06-Couloirs.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `couloir-principal` | [Sci-fi corridor](https://sketchfab.com/3d-models/sci-fi-corridor-5e6240c9b30145b0bf81fd02287985e3) | 3dhdscan | 164 086 | Segment de coursive principal, à répéter |
| `kit-interieur` | [Sci-fi Ship interior - Modular asset pack](https://sketchfab.com/3d-models/sci-fi-ship-interior-modular-asset-pack-50e2af8800cc4ab79add375f817b2d76) | Rejala | 356 806 | Kit modulaire d'intérieur de vaisseau |
| `kit-couloir` | [Sci-fi Corridor](https://sketchfab.com/3d-models/sci-fi-corridor-f3d033a8f3ad4888acc8c0a16878cf85) | Eliu_Villanueva | 22 279 | Kit de couloir en pièces (murs, portes, sols) |
| `conduit-maintenance` | [Sci-fi Hallway Tube](https://sketchfab.com/3d-models/sci-fi-hallway-tube-6987b3d35d2441e4b83ed9f87fbb74ff) | SlyEye | 201 178 | Conduit tubulaire : chemins de maintenance |
| `tuyaux` | [Sci-Fi Machinery: Pipes Modular Asset Pack](https://sketchfab.com/3d-models/sci-fi-machinery-pipes-modular-asset-pack-0b8188107ef040149f2f71eb08e9ce16) | somni | 7 978 | Tuyaux modulaires |
| `porte-coulissante` | [Sci-fi Door (high poly)](https://sketchfab.com/3d-models/sci-fi-door-high-poly-2e8c613214144d12bce416ce437614d6) | Scarecrow_original | 53 530 | Porte coulissante |
| `sols` | [PBR Sci-Fi Modular Flooring](https://sketchfab.com/3d-models/pbr-sci-fi-modular-flooring-0aea48c99ae24d78b7814d9dfbd94fe6) | JamesDawson | 696 | Dalles de sol modulaires |

## La consigne des effets saisis (`07-Consigne`)

![La consigne des effets saisis](prologue/07-Consigne.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `casier` | [Locker Equipment](https://sketchfab.com/3d-models/locker-equipment-fae22ece4d764979aafd614ebe09956d) | Siberia | 5 226 | Casiers de la consigne |
| `armoire-blindee` | [Armored Cabinet](https://sketchfab.com/3d-models/armored-cabinet-d16736e243904210b938590a42c8ea88) | Ray Ferric | 4 581, 1 anim. | Armoire blindée où est rangée la combinaison (porte animée) |
| `porte-coffre` | [Vault Door](https://sketchfab.com/3d-models/vault-door-20d25f3bae24417a9b6cc5ba690c62f5) | Ray Ferric | 14 556, 1 anim. | Porte blindée de la consigne (animée) |

## La salle des machines (`08-Machines`)

![La salle des machines](prologue/08-Machines.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `reacteur` | [ARC Pulse Core](https://sketchfab.com/3d-models/arc-pulse-core-22e55de3be0c46caad827774109572f5) | TooManyDemons | 24 020 | Réacteur de la salle des machines |
| `generateur` | [SCIFI GENERATOR](https://sketchfab.com/3d-models/scifi-generator-bf2e1a087adb4628a376ffbed17c41ff) | LordCinn | 68 574 | Générateur |
| `pylone-energie` | [Sci-fi generator](https://sketchfab.com/3d-models/sci-fi-generator-32c5436e8e9c459eac7471dce7c3878f) | aksinia0987 | 16 566 | Pylône d'énergie |
| `coeur-fusion` | [FUSION CORE](https://sketchfab.com/3d-models/fusion-core-f717683d5502496d9e1ef1f1e1d1cb45) | Fred Drabble | 20 700 | Cœur de fusion |

## Les ordinateurs (ceux qui disjonctent) (`09-Informatique`)

![Les ordinateurs (ceux qui disjonctent)](prologue/09-Informatique.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `baie-serveur` | [Server Rack](https://sketchfab.com/3d-models/server-rack-62f6779cb7e448b19aaf58544c3c7218) | Spellkaze | 744 | Baie de serveurs (les ordinateurs qui disjonctent) |
| `rangee-serveurs` | [Server Racking System](https://sketchfab.com/3d-models/server-racking-system-6fe2cacf836b4aed96c650b286db5486) | wpanayides | 299 604 | Rangée de serveurs |
| `console-murale` | [Wall Console](https://sketchfab.com/3d-models/wall-console-98724ee49fba42a2b0ccbe79a36f246e) | LuddePudde | 11 780 | Console murale |
| `grand-ecran` | [Large Wall-Mounted Computer Console](https://sketchfab.com/3d-models/large-wall-mounted-computer-console-298b7d9b68064618a3819d1a8fae4baf) | Inditrion Dradnon | 1 092 | Grand écran mural |
| `terminal` | [Sci-fi Computer Terminal](https://sketchfab.com/3d-models/sci-fi-computer-terminal-e03a14fc845e4e2d9573b95e00f79ef4) | Pedro Belthori | 13 892 | Terminal à pirater |
| `cables` | [Electric cable - 13MB](https://sketchfab.com/3d-models/electric-cable-13mb-1f6a4938769846b1ae9b512038aec5f8) | Mehdi Shahsavan | 14 986 | Câbles électriques (étincelles du court-circuit) |

## Le hangar (`10-Hangar`)

![Le hangar](prologue/10-Hangar.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `hangar` | [SCI FI HANGAR](https://sketchfab.com/3d-models/sci-fi-hangar-4b263701219446f1ab13b37a5b98bac5) | Kerem Kavalci | 229 786 | Le hangar |
| `caisses` | [Game Ready Sci-Fi Crates](https://sketchfab.com/3d-models/game-ready-sci-fi-crates-763fe7cff0534ec8a738b6127827eddd) | celebornArt | 14 168 | Caisses de fret |
| `caisse-armes` | [Military Weapons Container](https://sketchfab.com/3d-models/military-weapons-container-8a1c32f395f549b8b22265b8a459fa8e) | MeshSlinger | 9 964 | Caisse d'armes |
| `cellule-energie` | [Sci-Fi Power Cell Prop](https://sketchfab.com/3d-models/sci-fi-power-cell-prop-e43322345d39429d82396a8642bb8d70) | pixelgrapher | 2 408 | Cellule d'énergie |
| `bidon-carburant` | [Sci-fi Fuel Cell Canister](https://sketchfab.com/3d-models/sci-fi-fuel-cell-canister-04103035ea8c483db55ba61173929809) | Daniel.Sharma | 8 134 | Bidon de carburant |

## Les vaisseaux (`11-Vaisseaux`)

![Les vaisseaux](prologue/11-Vaisseaux.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `vaisseau-prison` | [The Star Prison (Basemesh&Greebles)](https://sketchfab.com/3d-models/the-star-prison-basemeshgreebles-e2d0fbe81c0e4182bec23e1adecba2db) | Relvean | 29 744 | Le vaisseau-prison vu de l'extérieur |
| `vaisseau-prison-variante` | [Gangut space hub](https://sketchfab.com/3d-models/gangut-space-hub-db9e338f04ae402c82f8ebdf12b55f9e) | Comrade1280 | 14 832 | Variante plus industrielle du vaisseau-prison |
| `navette-a-voler` | [Sci-fi shuttle](https://sketchfab.com/3d-models/sci-fi-shuttle-d3e5d9303d1d4e8b8a8e30bf22f59a21) | Hrvoje Wächter | 48 502 | La navette que le héros vole |
| `navette-transfert` | [X-4A Enforcer Assault Transport](https://sketchfab.com/3d-models/x-4a-enforcer-assault-transport-7a35a4f82e34446aa6131420bb26cc64) | pandoraskitten | 39 035 | Navette de transfert des prisonniers |
| `vaisseau-pirate` | [Heavy VTOL gunship](https://sketchfab.com/3d-models/heavy-vtol-gunship-86d336e0fa6f40329de8cda4a71d8a17) | Kai Xiang | 46 669 | Vaisseau pirate d'abordage |
| `capsule-sauvetage` | [Sci-Fi Personal Space Pod Ship(weekly challenge)](https://sketchfab.com/3d-models/sci-fi-personal-space-pod-shipweekly-challenge-b158f01dd4a9416fb689ca4401856e7a) | futaba@blender | 55 136 | Capsule de sauvetage (décor du hangar) |

## Sas, brèches et vide (`12-Exterieur`)

![Sas, brèches et vide](prologue/12-Exterieur.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `porte-sas` | [Free Airlock Door](https://sketchfab.com/3d-models/free-airlock-door-58729c7672ae4e01a6103617c2f4f55e) | michael_grodkowski | 4 440, 1 anim. | Porte de sas (animée) |
| `sas` | [Sci-Fi Airlock](https://sketchfab.com/3d-models/sci-fi-airlock-24521cd09bf54aafbd1dce8d8c731293) | hamraj15 | 14 632 | Sas |
| `cloison-etanche` | [Ship Door / Bulkhead](https://sketchfab.com/3d-models/ship-door-bulkhead-cdc2fb6ad6cd47cfa96182112b293d3f) | lumoize | 20 146 | Cloison étanche |
| `rocher-spatial` | [Meteorite](https://sketchfab.com/3d-models/meteorite-0a503e5bec784cbc9082e2f6b2d9f701) | Hailphilly | 82 097 | Rocher qui dérive dans le vide |
| `debris-1` | [SpaceShips Junk Yard  ASSET (part1)](https://sketchfab.com/3d-models/spaceships-junk-yard-asset-part1-c962c07f89c34e2a9d6714b3d408b878) | SebastianSosnowski | 31 348 | Débris de vaisseaux (1) |
| `debris-2` | [SpaceShips Junk Yard ASSET (part2)](https://sketchfab.com/3d-models/spaceships-junk-yard-asset-part2-f8fc1c20073448ccb4803335703c9129) | SebastianSosnowski | 53 734 | Débris de vaisseaux (2) |
| `gravats-breche` | [Construction debris](https://sketchfab.com/3d-models/construction-debris-53b50b002a0045c8b9f92858aa8c8dd7) | Alex Sade | 194 835 | Gravats des brèches de l'abordage |

## Les armes (`13-Armes`)

![Les armes](prologue/13-Armes.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `fusil-gardes` | [ASL - Standart Issue Assault Rifle](https://sketchfab.com/3d-models/asl-standart-issue-assault-rifle-60129a0e0cd340a38bf4b4c787a2dce6) | JmCVoyager | 9 382 | Fusil d'assaut des gardes |
| `mitraillette-gardes` | [Futuristic Submachine Gun](https://sketchfab.com/3d-models/futuristic-submachine-gun-893ed8d2adaf4658847c4f36dab16f61) | Newsome Interactive | 63 720 | Mitraillette des gardes |
| `pistolet-gardes` | [Pistol](https://sketchfab.com/3d-models/pistol-54c52df11e164b42992a7d418d99cd3f) | SINNIK | 50 208 | Pistolet des gardes |
| `fusil-pirates` | [Sci-Fi AK Rifle gameready (Free)](https://sketchfab.com/3d-models/sci-fi-ak-rifle-gameready-free-2d0d3410970d49c6865d00be15048f72) | Egor Likhachev | 12 242 | Fusil des pirates |
| `arme-pirates` | [Sci Fi Weapon. Gameready Gun / Rifle.](https://sketchfab.com/3d-models/sci-fi-weapon-gameready-gun-rifle-eca46c628f49410081eb73391c62b4ca) | gbarzu | 28 026, 2 anim. | Arme bricolée des pirates |
| `fusil-lourd-pirates` | [Heavy Assault Rifle](https://sketchfab.com/3d-models/heavy-assault-rifle-1b91aca44b65417f957d9ec30ce8ee19) | valerij1987 | 47 267, 1 anim. | Fusil lourd des pirates |
| `pistolet-use` | [Sci-Fi Pistol](https://sketchfab.com/3d-models/sci-fi-pistol-fab28aeb14894752b49149ef00926aff) | Andrei Milin | 17 452 | Pistolet usé, trouvé en route |
| `cle-a-molette` | [Weathered Wrench (Game Model)](https://sketchfab.com/3d-models/weathered-wrench-game-model-66b29c557ce04bb0b14bdde2cc204a2f) | Michael Makivic | 2 642 | Arme de fortune : clé à molette |

## Accessoires (`14-Accessoires`)

![Accessoires](prologue/14-Accessoires.jpg)

| Nom dans le projet | Modèle | Auteur | Faces | Rôle dans le jeu |
|---|---|---|---:|---|
| `extincteur` | [Fire Extinguisher](https://sketchfab.com/3d-models/fire-extinguisher-5676b179b3b744c0aaae53a3dcea2300) | Loïc | 2 792 | Extincteur |
| `neons` | [Fluorescent Light Fixtures - 4x 4096px²](https://sketchfab.com/3d-models/fluorescent-light-fixtures-4x-4096px2-2a05034d61dc49bf81b33de44add7aa8) | Mark Peters | 21 456 | Néons de plafond |
| `caisse-munitions` | [Sci-Fi crate / ammunition box (2)](https://sketchfab.com/3d-models/sci-fi-crate-ammunition-box-2-342efe4d70f94d37a4edd66a15c39502) | ul1tka | 648 | Caisse de munitions |
| `caisse-cargo` | [Sci-fi cargo crate](https://sketchfab.com/3d-models/sci-fi-cargo-crate-ac36898521304e51a8e305aa602f1f5b) | andreas9343 | 4 052 | Caisse de cargo |

## Crédits (CC BY)

- « Prisoner » par asadkhan1881 (sketchfab.com/asadkhan1881), CC Attribution
- « Mercenary Astronaut » par Peter_D (sketchfab.com/better_peter), CC Attribution
- « Balthazar (rigged & animated) » par Jungle Jim (sketchfab.com/jungle_jim), CC Attribution
- « Realistic Old Russian Guy (animated) » par Jungle Jim (sketchfab.com/jungle_jim), CC Attribution
- « Employee » par dotflare (sketchfab.com/Dotflare), CC Attribution
- « S.W.A.T. Operator » par SpatialNeglect (sketchfab.com/jeandiz), CC Attribution
- « Sci-fi O.B. Robot Unit TH-ICC02 (animated) » par Jungle Jim (sketchfab.com/jungle_jim), CC Attribution
- « Space Pirate » par Odlab (sketchfab.com/Odlab), CC Attribution
- « Scavenger » par Vasian-Digital3D (sketchfab.com/Vasian-Digital3D), CC Attribution
- « post apo character » par Am I dead? (sketchfab.com/eqisX), CC Attribution
- « Vintage 2725 " Spacer Armoured Pressure Helmet " » par KallMor (sketchfab.com/kallmor), CC Attribution
- « Abandoned prison modular scene » par Ali2-D2 (sketchfab.com/Ali2-D2), CC Attribution
- « Prison cell door » par Rivaldragon (sketchfab.com/Rivaldragon), CC Attribution
- « Door Of The Solitary Cell Of The Prison 14MB » par Mehdi Shahsavan (sketchfab.com/ahmagh2e), CC Attribution
- « Filthy Prison Toilet - 4096px² » par Mark Peters (sketchfab.com/mark-peters), CC Attribution
- « Filthy Metal Sink - 4096px² » par Mark Peters (sketchfab.com/mark-peters), CC Attribution
- « Simple Bunk Bed » par Blender3D (sketchfab.com/Blender3D), CC Attribution
- « Sci-fi corridor » par 3dhdscan (sketchfab.com/3dhdscan), CC Attribution
- « Sci-fi Ship interior - Modular asset pack » par Rejala (sketchfab.com/Rejala), CC Attribution
- « Sci-fi Corridor » par Eliu_Villanueva (sketchfab.com/Eliu_Villanueva), CC Attribution
- « Sci-fi Hallway Tube » par SlyEye (sketchfab.com/SlyEyePro), CC Attribution
- « Sci-Fi Machinery: Pipes Modular Asset Pack » par somni (sketchfab.com/3d.somni), CC Attribution
- « Sci-fi Door (high poly) » par Scarecrow_original (sketchfab.com/Scarecrow_original), CC Attribution
- « PBR Sci-Fi Modular Flooring » par JamesDawson (sketchfab.com/JamBoink), CC Attribution
- « Locker Equipment » par Siberia (sketchfab.com/LtxxwSibeRia), CC Attribution
- « Armored Cabinet » par Ray Ferric (sketchfab.com/rayferric), CC Attribution
- « Vault Door » par Ray Ferric (sketchfab.com/rayferric), CC Attribution
- « ARC Pulse Core » par TooManyDemons (sketchfab.com/toomanydemons), CC Attribution
- « SCIFI GENERATOR » par LordCinn (sketchfab.com/LordCinn), CC Attribution
- « Sci-fi generator » par aksinia0987 (sketchfab.com/aksinia0987), CC Attribution
- « FUSION CORE » par Fred Drabble (sketchfab.com/FredDrabble), CC Attribution
- « Server Rack » par Spellkaze (sketchfab.com/Spellkaze), CC Attribution
- « Server Racking System » par wpanayides (sketchfab.com/mcpanayides), CC Attribution
- « Wall Console » par LuddePudde (sketchfab.com/luddepubde), CC Attribution
- « Large Wall-Mounted Computer Console » par Inditrion Dradnon (sketchfab.com/dradnon), CC Attribution
- « Sci-fi Computer Terminal » par Pedro Belthori (sketchfab.com/pedrobelthori), CC Attribution
- « Electric cable - 13MB » par Mehdi Shahsavan (sketchfab.com/ahmagh2e), CC Attribution
- « SCI FI HANGAR » par Kerem Kavalci (sketchfab.com/Keremz), CC Attribution
- « Game Ready Sci-Fi Crates » par celebornArt (sketchfab.com/celebornArt), CC Attribution
- « Military Weapons Container » par MeshSlinger (sketchfab.com/thelastphysician), CC Attribution
- « Sci-Fi Power Cell Prop » par pixelgrapher (sketchfab.com/pixelgrapher), CC Attribution
- « Sci-fi Fuel Cell Canister » par Daniel.Sharma (sketchfab.com/Daniel.Sharma), CC Attribution
- « The Star Prison (Basemesh&Greebles) » par Relvean (sketchfab.com/Relvean), CC Attribution
- « Gangut space hub » par Comrade1280 (sketchfab.com/comrade1280), CC Attribution
- « Sci-fi shuttle » par Hrvoje Wächter (sketchfab.com/gazdahrco), CC Attribution
- « X-4A Enforcer Assault Transport » par pandoraskitten (sketchfab.com/pandoraskitten), CC Attribution
- « Heavy VTOL gunship » par Kai Xiang (sketchfab.com/kirikom9000), CC Attribution
- « Sci-Fi Personal Space Pod Ship(weekly challenge) » par futaba@blender (sketchfab.com/futaba_blender), CC Attribution
- « Free Airlock Door » par michael_grodkowski (sketchfab.com/michael_grodkowski), CC Attribution
- « Sci-Fi Airlock » par hamraj15 (sketchfab.com/hamraj15), CC Attribution
- « Ship Door / Bulkhead » par lumoize (sketchfab.com/lumoize), CC Attribution
- « Meteorite » par Hailphilly (sketchfab.com/hailphilly), CC Attribution
- « SpaceShips Junk Yard  ASSET (part1) » par SebastianSosnowski (sketchfab.com/SebastianSosnowski), CC Attribution
- « SpaceShips Junk Yard ASSET (part2) » par SebastianSosnowski (sketchfab.com/SebastianSosnowski), CC Attribution
- « Construction debris » par Alex Sade (sketchfab.com/Alexsade), CC Attribution
- « ASL - Standart Issue Assault Rifle » par JmCVoyager (sketchfab.com/JmCVoyager), CC Attribution
- « Futuristic Submachine Gun » par Newsome Interactive (sketchfab.com/newsomellc), CC Attribution
- « Pistol » par SINNIK (sketchfab.com/sinnik), CC Attribution
- « Sci-Fi AK Rifle gameready (Free) » par Egor Likhachev (sketchfab.com/egor_likhachev), CC Attribution
- « Sci Fi Weapon. Gameready Gun / Rifle. » par gbarzu (sketchfab.com/gbarzu), CC Attribution
- « Heavy Assault Rifle » par valerij1987 (sketchfab.com/valerij1987), CC Attribution
- « Sci-Fi Pistol » par Andrei Milin (sketchfab.com/milinam2002), CC Attribution
- « Weathered Wrench (Game Model) » par Michael Makivic (sketchfab.com/makivic), CC Attribution
- « Fire Extinguisher » par Loïc (sketchfab.com/loichuet1), CC Attribution
- « Fluorescent Light Fixtures - 4x 4096px² » par Mark Peters (sketchfab.com/mark-peters), CC Attribution
- « Sci-Fi crate / ammunition box (2) » par ul1tka (sketchfab.com/ul1tka), CC Attribution
- « Sci-fi cargo crate » par andreas9343 (sketchfab.com/andreas9343), CC Attribution
