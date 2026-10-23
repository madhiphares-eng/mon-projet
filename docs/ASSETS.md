# Bibliothèque d'assets — Échos d'Éther

378 assets réalistes pour Unity 6 HDRP, classés par usage dans le jeu. Chaque lien a été vérifié le 23 octobre 2026 :
licence lue sur la page (API Sketchfab et Poly Haven quand elles existent), téléchargement possible, page en ligne.
Seuls Sonniss, Freesound et Pixabay, bloqués depuis le serveur, ont été vérifiés par recherche web (voir [Sources](#sources)).

## Règles de licence

| Licence | Ce qu'on peut faire |
|---|---|
| **CC0** / domaine public | Tout, même vendre le jeu, sans crédit. |
| **CC BY** | Tout, mais il faut **créditer l'auteur** (écran des crédits) : voir la liste [Crédits obligatoires](#crédits-obligatoires). |
| **CC BY-SA** | Comme CC BY, et l'asset modifié doit garder la même licence (le jeu entier, non). |
| **Unity Asset Store EULA** | Utilisable dans un jeu Unity, même commercial. Interdit de redistribuer les fichiers sources. |
| **NASA** | Libre, même commercial. Interdit d'utiliser le logo NASA ou de laisser croire à un partenariat. |

Exclu d'office : licences NC (pas d'usage commercial), ND (pas de modification), « editorial only », et tout ce qui copie une franchise (Star Wars, Dune, Warhammer…).

## Résumé

| Catégorie | Assets | Gratuits | Payants |
|---|---:|---:|---:|
| [Terrain : sol, roche, sable](#terrain) | 55 | 55 | 0 |
| [Personnages, créatures, animations](#personnages) | 81 | 73 | 8 |
| [Vaisseaux, véhicules, matériaux de coque](#vaisseaux) | 79 | 75 | 4 |
| [Architecture : bases, ruines, tombeaux](#architecture) | 84 | 78 | 6 |
| [Ciel et espace : HDRI, étoiles, planètes, trou noir](#ciel) | 60 | 57 | 3 |
| [Effets, sons, musique](#effets) | 19 | 15 | 4 |
| **Total** | **378** | | |

## Téléchargement automatique

Tous les assets gratuits qui peuvent se télécharger automatiquement s'installent dans le projet avec une commande PowerShell. **Fermer Unity avant de la lancer** :

```powershell
irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-assets.ps1 | iex
```

Le script propose 6 groupes au choix (Entrée = tout, de 10 à 15 Go en tout) :

| Groupe | Contenu | Taille |
|---|---|---:|
| 1. Poly Haven | 98 rochers, falaises, textures et ciels HDRI (CC0) | ~2,4 Go |
| 2. ambientCG | 43 textures de sol, de roche et de métal, 5 ciels (CC0) | ~1,2 Go |
| 3. Sketchfab | 172 modèles : personnages, créatures, vaisseaux, ruines | 5 à 10 Go |
| 4. NASA | 16 modèles réels : combinaisons, rovers, modules, ISS | ~0,5 Go |
| 5. Espace | cartes d'étoiles, planètes, relief de la Lune, catalogue HYG, images de référence | ~0,6 Go |
| 6. Sons Kenney | bruitages sci-fi, impacts, interface, jingles (CC0) | ~10 Mo |

- **Sketchfab** demande un token API gratuit : créer un compte, puis copier le token depuis [les paramètres du compte](https://sketchfab.com/settings/password) (section *API Token*). Le script relit la licence de chaque modèle juste avant de le télécharger et ignore ceux qui ne sont plus en CC0, CC BY ou CC BY-SA.
- Le package **glTFast** (import des fichiers `.glb`) est ajouté au projet automatiquement.
- Le script écrit `CREDITS-ASSETS.txt` (crédits à citer) et `A-TELECHARGER-A-LA-MAIN.txt` à la racine du projet. Ce second fichier liste ce qui ne peut pas se télécharger tout seul : Unity Asset Store, Quaternius, sons Sonniss et Freesound, cartes de relief géantes.
- Les images de référence (Hubble, Webb, trous noirs) vont dans `References/`, hors du dossier Unity.
- On peut relancer le script sans risque : ce qui est déjà téléchargé est sauté.

<a id="terrain"></a>

## Terrain : sol, roche, sable

### Modèles 3D : rochers, falaises, dunes, minéraux

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Moon Rock 01 (série Moon Rock 01 à 07)](https://polyhaven.com/a/moon_rock_01)<br><sub>Poly Haven</sub> | CC0 1.0 (domaine public) | Gratuit | Secteurs 1 et 6 : rochers de régolithe poussiéreux (collection Moon). Vérifié par l'API : 18 668 triangles, textures jusqu'en 16K. Les modèles moon_rock_02 à 07 existent bien (de 6 000 à 18 500 triangles), donc tous sont légers et utilisables en scatter. |
| [Coastal Cliff 01](https://polyhaven.com/a/coastal_cliff_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 3 : falaise côtière battue par les vagues géantes (collection Smugglers Cove). 865 917 triangles, textures 8K : à décimer ou à passer en LOD. |
| [Coastal Cliff 04 (variantes 01/02/04)](https://polyhaven.com/a/coastal_cliff_04)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 3 : grand pan de falaise côtière. 2,88 millions de triangles, à réserver aux vues lointaines ou à cuire en normal map. Coastal Cliff 02 existe aussi (1,77 million de triangles). Il n'y a pas de numéro 03. La variante 01 est déjà l'entrée ci-dessus. |
| [Coast Rocks 05 (récif)](https://polyhaven.com/a/coast_rocks_05)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 3 : récif rocheux de bord de mer (tags reef, coastal) pour les zones de ressac et les points d'atterrissage de la navette. Correction : le mesh est lourd (1,45 million de triangles), il faut le décimer. |
| [Coast Line 01](https://polyhaven.com/a/coast_line_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 3 : morceau de ligne de côte (roche et galets) pour la transition entre la mer et le terrain. Précision : environ 1 million de triangles, à décimer. |
| [Rock Face 01 (et Rock Face 02)](https://polyhaven.com/a/rock_face_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 4 : parois légères (Rock Face 01 : 20 174 triangles ; Rock Face 02 : 29 566 triangles) pour encadrer les nécropoles. Il y a du lichen à retirer en retintant la texture. Ces modèles 3D sont distincts de la texture rock_face déjà retenue. |
| [Mountainside](https://polyhaven.com/a/mountainside)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 4 : grand flanc rocheux érodé (287 755 triangles, 8K) pour les bords de canyon et les arrière-plans. Il porte du lichen, à désaturer pour le désert. |
| [Sand Rocks Small 01](https://polyhaven.com/a/sand_rocks_small_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 2 et 3 : cailloux posés sur le sable, à disperser sur les plages et les dunes. Le mesh est très dense (1,39 million de triangles) : il faut le décimer avant de l'utiliser en scatter. |
| [Desert Cliffs Terrain Scan (Pers Scans)](https://sketchfab.com/3d-models/desert-cliffs-terrain-scan-561a1628d0b446c7bd3424da48fe1150)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Pers Scans) | Gratuit | Secteurs 1 et 4 : grand pan de falaises désertiques scanné près de Grenade, avec albedo sans éclairage. Confirmé : textures 8K et albedo 16K en fichiers additionnels, 1,999 million de triangles, à décimer. |
| [LiDAR Rock Scan, Ramon Crater, Negev](https://sketchfab.com/3d-models/lidar-rock-scan-ramon-crater-negev-israel-f0e424595ce14df9b82c35b658beb882)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Doron Altaratz) | Gratuit | Secteur 1 : formation rocheuse désertique du Makhtesh Ramon (cratère d'érosion du Néguev), scannée au LiDAR. 432 004 triangles. |
| [Cliff Rock Boulder Field (Pers Scans)](https://sketchfab.com/3d-models/cliff-rock-boulder-field-d1f2ce6f71aa4c19adbdc541cc888194)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Pers Scans) | Gratuit | Secteur 4 : champ de blocs au pied d'une falaise (Pyrénées espagnoles, près du Pedraforca), sans éclairage dans l'albedo. 122 310 triangles, textures 8K. Idéal au pied des tombeaux effondrés. |
| [Rock Pile Scan Dry 1 (Pers Scans)](https://sketchfab.com/3d-models/rock-pile-scan-dry-1-5206d4d96c2d428b9c1f7ee0e13bcffb)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Pers Scans) | Gratuit | Secteurs 1, 4 et 5 : amas de pierres sèches (textures 4K, sans éclairage) pour les éboulements, les barricades naturelles et les gravats de mine. Précision : 305 215 triangles, à décimer pour le jeu. |
| [Rocks Set Stone Collection Scan (Pers Scans)](https://sketchfab.com/3d-models/rocks-set-stone-collection-scan-b45a40fd145c4b9ba7bc5351d344db17)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Pers Scans) | Gratuit | Tous secteurs : kit de pierres scannées très léger (5 974 triangles au total, PBR 4K sans éclairage), prêt pour le scatter de détail sur une RTX 3060. |
| [Coastal Ground Cliff Rocks (Pers Scans)](https://sketchfab.com/3d-models/coastal-ground-cliff-rocks-68d53a1623aa47008fb7088a5694d79b)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Pers Scans) | Gratuit | Secteur 3 : sol de roches côtières avec PBR 8K (albedo, normal, roughness, displacement, AO) et seulement 24 868 triangles. Très rentable en performance. |
| [Rocks Marine – Corsica (AliceVision)](https://sketchfab.com/3d-models/rocks-marine-corsica-416a548a159f42e89a475d0ac17ac6d9)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer AliceVision) | Gratuit | Secteur 3 : formation rocheuse de Corse (Meshroom, 123 photos) pour les îlots et les écueils. 1,12 million de triangles, à décimer. |
| [Sea cliff rock face (Crète)](https://sketchfab.com/3d-models/sea-cliff-rock-face-e4bf57abe35f4df3b6f4c0f32507091f)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer axonite) | Gratuit | Secteur 3 : paroi calcaire de falaise maritime crétoise pour les murs battus par les vagues géantes (538 480 triangles). Attention : l'auteur parle d'un scan d'essai fait avec une GoPro 9, donc la qualité des textures est à contrôler. |
| [Rock_Salines_2 (Martinique)](https://sketchfab.com/3d-models/rock-salines-2-f9c05a8914a04b6aa3417d4a8db17c85)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Maxim.O) | Gratuit | Secteur 3 : rocher sculpté par la mer, scanné sur la plage des Salines, en prop isolé sur l'estran (105 317 triangles). |
| [Wall of ice](https://sketchfab.com/3d-models/wall-of-ice-17b6a81191f045a1af2d9bb57cad428b)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Lassi Kaukonen) | Gratuit | Secteur 6 et zones gelées : mur de glace naturel en photogrammétrie (Metashape). Avec un shader translucide, il devient une paroi de verre ou de cristal. 955 737 triangles, à décimer. |
| [Obsidian (Digital Atlas of Ancient Life)](https://sketchfab.com/3d-models/obsidian-a9f3229f5e57496d81c004a606a43728)<br><sub>Sketchfab</sub> | CC0 1.0 | Gratuit | Secteur 2 : échantillon d'obsidienne de Millard County (Utah), environ 12 cm dans la réalité. À agrandir en éclats de verre noir sur la mer de verre ou à garder en objet de collecte. 287 304 triangles. |
| [Halite crystals (musée des salines de Wieliczka)](https://sketchfab.com/3d-models/halite-crystals-4f4a474678f845f79cb0db63f08306fb)<br><sub>Sketchfab</sub> | CC0 1.0 | Gratuit | Secteurs 2 et 6 : amas de cristaux de sel cubiques (musée des salines de Cracovie à Wieliczka, publié par Virtual Museums of Małopolska), à agrandir en formations salines géantes ou en concrétions cristallines. 217 990 triangles. |
| [Apollo Lunar Sample 70175,0 (NASA Astromaterials 3D)](https://sketchfab.com/3d-models/apollo-lunar-sample-701750-9fe0833c2b3a4dd3b26844c1dd09f441)<br><sub>Sketchfab (NASA Astromaterials 3D)</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 6 : vrai échantillon lunaire d'Apollo 17 scanné par la NASA (200 000 triangles), comme objet de collecte pour les Arpenteurs ou bloc agrandi. Confirmé : les autres échantillons lunaires et météorites antarctiques du compte sont aussi en CC0 (5 vérifiés sur 5). |
| [Sand Dunes (Helindu)](https://sketchfab.com/3d-models/sand-dunes-6bd9a0d96ef4463a8efd45af2be595ac)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Helindu) | Gratuit | Secteur 2 : maillage de dunes avec textures 4K (diffuse, roughness, normal), utile en arrière-plan ou comme référence de forme pour la heightmap. Précision : ce n'est pas un scan, il a été généré dans Blender (addon A.N.T. Landscape). 496 008 triangles. |
| [Sand cliffs (dune de Long Point)](https://sketchfab.com/3d-models/sand-cliffs-1c5f772e12fc44ff995776e38e1953f2)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer outsider) | Gratuit | Secteurs 2 et 3 : front de dune érodé scanné à Long Point (Ontario), 167 023 triangles. Limites vues sur l'aperçu : scan PhotoScan de 2017 avec éclairage cuit dans la texture, herbe sur le sommet et morceaux de ciel au bord. À nettoyer avant usage. |
| [Stránská rock 1](https://sketchfab.com/3d-models/stranska-rock-1-4118802ad14243d59f34c2be3fa77cd4)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer 3dhdscan) | Gratuit | Secteur 4 : falaise calcaire karstique avec une entrée de grotte (84 283 triangles), en prop moyen, en base de monolithe ou en entrée de tombeau. Précision : herbe au sommet et au pied, à retirer ou à masquer pour le désert. |
| [Galet éolisé – ventifact (ENS Géologie)](https://sketchfab.com/3d-models/galet-eolise-ventifact-7f86615ce94e4ac09d31f191e746534a)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer École Nationale Supérieure de Géologie) | Gratuit | Secteurs 1 et 2 : galet facetté par la corrasion éolienne. Agrandi, il donne des rochers aux arêtes vives typiques des déserts balayés de tempêtes. 162 742 triangles. |
| [Asteroids Pack (rocky version)](https://sketchfab.com/3d-models/asteroids-pack-rocky-version-adde1ecf129e4509be8af61b84bafa85)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer SebastianSosnowski) | Gratuit | Secteurs 1 et 6 : roches en orbite autour du trou noir et débris flottants du cœur de l'écho. Retopologie ZRemesher, 23 076 triangles pour tout le pack. |
| [Alaskan Cliff Rock 9 Free](https://sketchfab.com/3d-models/alaskan-cliff-rock-9-free-39e29fec9cf040dd925b804f5040ca04)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer Titaniumwhitefx) | Gratuit | Secteurs 4 et 5 : bloc de falaise anguleux (albedo, roughness et normal en 2K). Environ 1 million de triangles, à décimer ou à cuire en normal map. Confirmé : l'auteur publie environ 17 autres numéros gratuits en CC BY (1 à 4, 8 à 16, 18, Chunk 7 et 23). Le n° 6, « Alaskan CliffRock 6 PBR game ready LODs Free », a déjà des LOD. |

### Textures de sol et de roche

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Moon Dusted 03](https://polyhaven.com/a/moon_dusted_03)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 1 : sol de poussière fine type régolithe, en 16K. Couche de base du désert au bord du trou noir. La série compte moon_dusted_01 à 05. Correction : les traces de roues s'appellent moon_track_01 à 04 (et non « moon_track »), et il existe aussi moon_footprints_01 et 02. |
| [Moon Meteor 01](https://polyhaven.com/a/moon_meteor_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 6 : sol de régolithe constellé de cratères (16K), pour les zones bombardées par les débris attirés par le trou noir. Variante moon_meteor_02 disponible. |
| [Damp Beach Sand 02](https://polyhaven.com/a/damp_beach_sand_02)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 3 : sable humide strié par la marée (8K, tags tidal, striated), pour l'estran qu'on découvre entre deux vagues géantes. |
| [Damp Sand](https://polyhaven.com/a/damp_sand)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 3 : sable grossier mouillé (8K), en variation de Damp Beach Sand 02 pour casser la répétition. |
| [Dark Rock 02](https://polyhaven.com/a/dark_rock_02)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 5 et 3 : roche sombre fracturée et stratifiée (8K) pour les fondations de la forge abandonnée et les falaises sombres. |
| [Seaside Rock](https://polyhaven.com/a/seaside_rock)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 3 : roche côtière usée par l'eau (8K), à appliquer sur les falaises et les récifs ou en triplanaire. |
| [Worn Rock Natural 01](https://polyhaven.com/a/worn_rock_natural_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 4 : grès jaune érodé (catégorie sandstone, 8K) pour les parois de la vallée des tombeaux et les monolithes taillés dans la roche. |
| [Rock Boulder Cracked](https://polyhaven.com/a/rock_boulder_cracked)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 4 : roche orangée craquelée (8K, tags orange, cliff), cohérente avec la lumière rasante d'un disque d'accrétion. |
| [Mud Cracked Dry 03](https://polyhaven.com/a/mud_cracked_dry_03)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 2 : croûte de boue sèche polygonale (8K). Une fois désaturée et éclaircie, elle peut servir de base de salar. |
| [Burned Ground 01](https://polyhaven.com/a/burned_ground_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 et zones de crash d'épaves : sol calciné et cendreux (8K). Précision : il contient des débris végétaux et des feuilles brûlées (tags plant litter, leaves), donc il est plus adapté aux crashs qu'au sol minéral de l'usine. |
| [Snow 02](https://polyhaven.com/a/snow_02)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 4 et 6 : neige poudreuse lisse (8K), pour du givre dans l'ombre des monolithes ou une version gelée du cœur de l'écho. |
| [Ground 093 C (et 093 B) – sable de dune](https://ambientcg.com/view?id=Ground093C)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 2 : sable de dune accumulé par le vent (tags dune, desert, windy, sorti en novembre 2025). Cartes color, normal, roughness, displacement et AO. Ground093B, vérifiée, sert de variante. |
| [Ground 096 A – sable désertique ondulé](https://ambientcg.com/view?id=Ground096A)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 2 : sable sec à rides de vent (tags desert, waves, wavy) pour les crêtes et les creux entre les dunes. |
| [Rock 029 – falaise rouge désertique](https://ambientcg.com/view?id=Rock029)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 1 : falaise rouge-orangé désertique (tags cliff, desert, red), à utiliser en matériau de pente sur le terrain HDRP. |
| [Rock 061 – falaise brune sableuse](https://ambientcg.com/view?id=Rock061)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 2 : roche de falaise brun-gris ensablée (sortie en avril 2025) pour les affleurements qui percent les dunes. |
| [Rock 035 – roche noire de grotte](https://ambientcg.com/view?id=Rock035)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteurs 5 et 6 : roche noire de caverne (tags black, cave, cliff) pour les galeries souterraines, les nids de la Ruche des Sables et les cavités sous la forge. |
| [Rock 041 – roche noire lisse](https://ambientcg.com/view?id=Rock041)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 2 : roche noire lisse. Avec une rugosité baissée et un coat mask HDRP, elle donne du sable vitrifié ou de l'obsidienne pour la mer de verre. |
| [Rock 062 – roche grise fissurée érodée](https://ambientcg.com/view?id=Rock062)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 4 : roche grise fissurée et érodée (tags cracked, eroded, rugged) pour les ruines et les monolithes naturels de la vallée des tombeaux. |
| [Lava 004 – magma (avec emission)](https://ambientcg.com/view?id=Lava004)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 5 : coulées en fusion dans la forge et fissures incandescentes. La carte d'emission est bien fournie (avec color, normal, roughness et displacement), prête pour HDRP. |
| [Ground 061 – sable de plage mouillé](https://ambientcg.com/view?id=Ground061)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 3 : sable de plage humide (tags beach, wet) pour le bord des vagues géantes. |
| [Ground 060 – sable à coquillages](https://ambientcg.com/view?id=Ground060)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 3 : sable de plage mêlé de coquillages (tags shells, ocean) pour les laisses de mer de la planète océan. |
| [Rock 020 – roche plate mouillée](https://ambientcg.com/view?id=Rock020)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 3 : dalle rocheuse grise, plate, lisse et mouillée pour les plateaux de récif balayés par l'eau. |
| [Gravel 024 – gravier noir](https://ambientcg.com/view?id=Gravel024)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 5 : gravier noir type scorie ou mâchefer autour des usines. Sert aussi de plage volcanique noire au secteur 3. |
| [Rocks 015 – cailloux désertiques](https://ambientcg.com/view?id=Rocks015)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 1 : sable semé de cailloux (tags desert, pebbles) pour les pistes et les regs. Correction sur la série : les variantes désertiques sont Rocks012 à 014 et Rocks017 à 021. Rocks016 n'est pas étiquetée désert. Pas de carte AO sur celle-ci. |
| [Ice 003 – glace](https://ambientcg.com/view?id=Ice003)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 6 et zones froides : glace de lac gelé, plate, brillante et teintée de vert (tags frozen, lake, shiny). Retintée, elle convient au verre et au cristal du cœur de l'écho. Texture ancienne (2018), sans carte AO. |
| [Snow 008 A – dunes de neige ondulées](https://ambientcg.com/view?id=Snow008A)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteurs 2 et 6 : neige à vagues dunaires (tags dunes, waves). Retintée, elle donne des dunes de cendre ou de verre pilé. |
| [Ground 111 – sol de gravats et débris](https://ambientcg.com/view?id=Ground111)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 5 : sol de gravats, fragments de brique et gravier (tags rubble, construction, urban, sorti en septembre 2026) pour le monde industriel abandonné. |
| [Onyx 006 – onyx / cristal](https://ambientcg.com/view?id=Onyx006)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteurs 6 et 4 : pierre veinée lisse (tags crystal, magic, onyx) pour les surfaces abstraites du cœur de l'écho et les monolithes du Chœur du Tombeau. Précision : pas de carte de transmission, la translucidité est à faire dans le shader HDRP. |

<a id="personnages"></a>

## Personnages, créatures, animations

### Combinaisons / Arpenteurs

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Spacesuit (Confederation of Planets), game-ready (tnnv)](https://sketchfab.com/3d-models/spacesuit-confederation-of-planets-da7b5c14bc634f8283e2d21343afa7cc)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « tnnv » (licence lue dans l'API Sketchfab) | Gratuit | Combinaison prête pour un jeu à la 3e personne, 26 006 faces. Candidat direct pour le personnage joueur ; à rigger avec AccuRIG ou Mixamo. La tête visible dans le casque vient du modèle standard Adobe Fuse. « Confederation of Planets » est la fiction de l'auteur, pas une franchise connue. |
| [Mercenary Astronaut (better_peter)](https://sketchfab.com/3d-models/mercenary-astronaut-7fd399d488ae4576a0fc62715892956c)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « better_peter » (l'auteur ajoute : « absolutely free for your game ») | Gratuit | Arpenteur armé et usé, 3 animations, 704 714 faces, textures 4K (109 textures) : forte optimisation nécessaire. Joueur, PNJ mercenaire ou cinématiques. |
| [Animated Astronaut Character in Space Suit Loop (LasquetiSpice)](https://sketchfab.com/3d-models/animated-astronaut-character-in-space-suit-loop-8fe5c8d3365e4d87bb7bc253d53a64e1)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « LasquetiSpice » | Gratuit | Astronaute riggé, 58 312 faces, 4 animations (marche, salut, dérive, idle) : prototype jouable immédiat et séquences en apesanteur. Combinaison de type EMU avec écussons à retirer. |
| [Z2 Spacesuit, prototype NASA de combinaison planétaire (GLB)](https://science.nasa.gov/3d-resources/z2-spacesuit/)<br><sub>NASA (science.nasa.gov, 3D Resources)</sub> | NASA 3D Resources (« free to download and use ») + NASA Images and Media Usage Guidelines : contenu en général non soumis au droit d'auteur aux USA. Usage commercial possible sans logo ni insigne NASA et sans laisser croire à un soutien de la NASA. Citer NASA comme source. | Gratuit | Combinaison d'exploration de surface : base pour l'Arpenteur du désert du trou noir et de la mer de verre. GLB de 16,9 Mo vérifié (200, model/gltf-binary). Retirer les marquages NASA. |
| [Extravehicular Mobility Unit (EMU), combinaison de sortie NASA (GLB)](https://science.nasa.gov/3d-resources/extravehicular-mobility-unit/)<br><sub>NASA (science.nasa.gov, 3D Resources)</sub> | NASA 3D Resources + NASA Images and Media Usage Guidelines (non soumis au droit d'auteur aux USA ; pas de logo ni insigne NASA ; pas de soutien sous-entendu ; citer NASA) | Gratuit | Référence réaliste et base de modélisation pour la combinaison des Arpenteurs (sac de survie, joints, connecteurs). GLB de 3,4 Mo vérifié (200). Retirer les écussons NASA. |
| [Space Engineer F Rigged, combinaison de protection féminine (tegnemaskin)](https://sketchfab.com/3d-models/space-engineer-f-rigged-98282a36702941d3941941def844bd68)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « tegnemaskin » | Gratuit | Arpenteuse riggée avec os de torsion, 17 629 faces, un seul jeu de textures : variante féminine du joueur ou équipage du vaisseau-mère. Casque et sac sont vendus à part par l'auteur. |
| [Rigged for UE4/UE5 – Exoskeleton – free (Andywoodhead)](https://sketchfab.com/3d-models/rigged-for-ue4ue5-exoskeleton-free-c0efb57e2fa940179ebaa58dcb9d0fa7)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « Andywoodhead » (l'auteur ajoute : « free to use this model in any way ») | Gratuit | Soldat en exosquelette, 187 133 faces, riggé sur le squelette UE4 Mannequin (à configurer en Humanoid dans Unity) : classe lourde d'Arpenteur ou PNJ de la base. |
| [Mech Pilot UN., pilote en exo-combinaison (gueriero)](https://sketchfab.com/3d-models/mech-pilot-un-722b12227c634489a3436a18a07064d4)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « gueriero » | Gratuit | Pilote en armure souple, PBR 4K, peau SSS ; le .blend contient un rig et 3 animations (course, marche, mort). Correction : 604 379 faces, donc décimation obligatoire. Vérifier et retirer tout emblème « UN » éventuel. Pilote de navette. |
| [Mark III Spacesuit, combinaison rigide NASA (GLB)](https://science.nasa.gov/3d-resources/mark-iii-spacesuit/)<br><sub>NASA (science.nasa.gov, 3D Resources)</sub> | NASA 3D Resources + NASA Images and Media Usage Guidelines (pas de logo NASA ; pas de soutien sous-entendu ; citer NASA) | Gratuit | Variante « hard suit » pour une classe lourde d'Arpenteur ou les corps d'anciennes expéditions dans les épaves. GLB de 0,9 Mo vérifié (200). |
| [Astronaut, astronaute complet NASA (GLB)](https://science.nasa.gov/3d-resources/astronaut/)<br><sub>NASA (science.nasa.gov, 3D Resources)</sub> | NASA 3D Resources + NASA Images and Media Usage Guidelines (pas de logo NASA ; pas de soutien sous-entendu ; citer NASA) | Gratuit | Silhouette d'astronaute légère (GLB de 0,76 Mo vérifié) : tests d'échelle, PNJ de la navette, corps d'équipage dans les épaves. |
| [Space suit inspiré d'Apollo (IgorSholopov)](https://sketchfab.com/3d-models/space-suit-bdc42075279746dab4c8b54784276a20)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « IgorSholopov » | Gratuit | Combinaison souple de style Apollo, 61 334 faces : expédition ancienne dans la vallée des tombeaux, cadavres, statues. Attention : l'aperçu montre un logo NASA (« meatball ») et un drapeau sur la poitrine, à retirer de la texture. |
| [Hazmat Suit, combinaison NRBC Marvelous Designer (specter)](https://sketchfab.com/3d-models/hazmat-suit-095694921e914ae889e91767ed680e4b)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « specter » | Gratuit | Combinaison en tissu simulé, 101 282 faces, sans texture (0 texture dans l'API) et sans masque à gaz : ouvriers disparus du monde industriel (secteur 5). Prévoir texturage et rig. |
| [ASTRONAUT, combinaison EVA (Abrazafarolas)](https://sketchfab.com/3d-models/astronaut-626da9f4336b4abc9f91ce71a7a1af42)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « Abrazafarolas » | Gratuit | Correction : l'API indique 1 seul matériau et 1 seule texture, sans PBR, et l'aperçu Sketchfab est non texturé (gris argile). Les « textures 4K » et le rendu photoréaliste annoncés ne sont donc pas vérifiés. Maillage dense de 966 698 faces, publié en avril 2026, 4 likes. Utile surtout comme sculpture de référence ou statue ; ce n'est pas le meilleur Arpenteur. |

### Humains / PNJ

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [MakeHuman 1.x, générateur d'humains](https://static.makehumancommunity.org/makehuman.html)<br><sub>MakeHuman Community</sub> | Assets de base CC0, sans attribution (page About > License). Logiciel sous AGPL, sans effet sur les modèles exportés. Les assets communautaires peuvent avoir d'autres licences : les vérifier un par un. | Gratuit | Générer les corps des Arpenteurs sous la combinaison, l'équipage et les PNJ. La documentation officielle couvre l'export vers Unity (« Saving models for Unity »). Page vérifiée : 200. |
| [MPFB 2, MakeHuman Plugin For Blender](https://static.makehumancommunity.org/mpfb.html)<br><sub>MakeHuman Community</sub> | Assets de base CC0. Code du plugin sous GPL, sans effet sur les personnages exportés. | Gratuit | Création d'humains directement dans Blender (version 2.0.15 citée sur la page), avec rigs et vêtements, puis export FBX vers Unity HDRP. Pour les visages visibles derrière la visière. Page vérifiée : 200. |
| [Nathan Animated 003, homme scanné animé (Renderpeople)](https://sketchfab.com/3d-models/nathan-animated-003-walking-3d-man-143a2b1ea5eb4385ae90a73657aca3bc)<br><sub>Sketchfab (compte officiel renderpeople)</sub> | CC BY 4.0, créditer « Renderpeople » (licence du téléchargement Sketchfab) | Gratuit | Humain photogrammétrique, 21 506 faces, marche en motion capture déjà appliquée, textures 8K : PNJ civils dans les échos et flashbacks. |
| [Eric Rigged 001, homme scanné riggé (Renderpeople)](https://sketchfab.com/3d-models/eric-rigged-001-rigged-3d-business-man-a46bc9f67aaa415bb4f3241eef900e7f)<br><sub>Sketchfab (compte officiel renderpeople)</sub> | CC BY 4.0, créditer « Renderpeople » | Gratuit | Humain scanné et riggé de 20 542 faces, en tenue de ville : officier ou scientifique du vaisseau-mère, hologrammes du passé. À animer avec UAL ou Mixamo. |
| [Carla Rigged 001, femme scannée riggée (Renderpeople)](https://sketchfab.com/3d-models/carla-rigged-001-rigged-3d-business-women-acf520f450d14dd799f98a6fede3edf5)<br><sub>Sketchfab (compte officiel renderpeople)</sub> | CC BY 4.0, créditer « Renderpeople » | Gratuit | Pendant féminin riggé de 19 988 faces : équipage et échos de colons disparus. |
| [Rigged T-Pose Human Male w 50 Face Blendshapes (mikealger)](https://sketchfab.com/3d-models/rigged-t-pose-human-male-w-50-face-blendshapes-cc7e4596bcd145208a6992c757854c07)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « mikealger » (scan de l'auteur lui-même, qui autorise tout usage) | Gratuit | Scan riggé de 54 683 faces avec 50 blendshapes faciales, déjà testé dans Unity par l'auteur. Une seule texture diffuse : visage expressif à moyenne distance, briefings. |

### Animations / outils

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Universal Animation Library, plus de 120 animations humanoïdes (Quaternius)](https://quaternius.com/packs/universalanimationlibrary.html)<br><sub>Quaternius</sub> | CC0 (domaine public) | Gratuit | Plus de 120 animations sur un rig humanoïde universel, testées par l'auteur dans Unity : locomotion 8 directions, sprint, accroupi, nage, combat, arme à feu, morts, emotes. À retargeter en Humanoid sur les Arpenteurs et les PNJ. Page vérifiée : 200, « License CC0 ». |
| [Human Basic Motions FREE (Kevin Iglesias)](https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | Gratuit | 118 fichiers d'animation en version masculine et féminine (idles, marche et course 8 directions, sprint, sauts, rotations, en root motion et in-place), avec Avatar Masks. Compatibilité HDRP indiquée pour Unity 6000.0 : base de l'Animator des Arpenteurs. Page vérifiée : 200, prix 0,00. |
| [Starter Assets, First Person + Third Person Character Controllers (Unity Technologies)](https://assetstore.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526)<br><sub>Unity Asset Store</sub> | Unity Companion License | Gratuit | Contrôleurs 1re et 3e personne officiels (rig Humanoid et animations, Cinemachine, Input System), compatibles HDRP, page mise à jour en septembre 2026 : squelette du gameplay coop des Arpenteurs. Page vérifiée : 200. |
| [Mesh2Motion, auto-rig et bibliothèque d'animations](https://mesh2motion.org/)<br><sub>Mesh2Motion (open source)</sub> | Animations et fichiers Blender en CC0 ; code sous licence MIT | Gratuit | Rigger et animer des créatures non humanoïdes (rigs araignée, serpent, etc.) pour les insectes et les vers de la Ruche, export GLB ou FBX vers Unity. Page vérifiée : 200, mentions « Licensed under CC0 » et « Code under the MIT License ». |
| [100STYLE, dataset mocap de 100 styles de locomotion (BVH)](https://www.ianxmason.com/100style/)<br><sub>Ian Mason / Université d'Édimbourg</sub> | CC BY 4.0, créditer « The 100STYLE Dataset – Ian Mason » | Gratuit | Mocap BVH de marche et course dans 100 styles (blessé, lourd, accroupi, prudent…) : Arpenteurs épuisés, gravité forte, PNJ variés, motion matching. Page vérifiée : 200, licence CC BY 4.0 citée. |
| [Huge FBX Mocap Library part 1 (cMonkeys)](https://assetstore.unity.com/packages/3d/animations/huge-fbx-mocap-library-part-1-19991)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA. Les données viennent de la base CMU, dont les conditions s'ajoutent : inclusion dans un produit commercial autorisée, revente des données elles-mêmes interdite, même converties. | Gratuit | Conversion FBX de la base CMU Motion Capture (2 534 clips, répartis en 3 parties sur le Store) : marche, escalade, chutes, interactions pour enrichir la locomotion. Qualité variable selon les clips, nettoyage parfois nécessaire. Page vérifiée : 200, prix 0,00, publiée en 2016. |

### Ruche des Sables

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Gold Sandworm, ver animé game-ready (electricToy)](https://sketchfab.com/3d-models/gold-sandworm-41f03702289940b29831dcb771fb5f3a)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « electricToy » | Gratuit | Ver très léger (1 342 faces), FBX avec Idle, Attack, Hit, Die, Die2 et Spawn, textures 1024. Style plutôt stylisé : prototype de l'IA d'émergence et d'attaque, ou petits vers en essaim. |
| [Game-Ready Queen ant Model, reine fourmi (LilCick)](https://sketchfab.com/3d-models/game-ready-queen-ant-model-b04b636a35e14b38bfb05bd21931549d)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « LilCick » | Gratuit | Reine fourmi réaliste, riggée, 13 animations, 47 872 faces : boss ou matriarche au fond des nids. L'auteur fournit aussi le .blend via un lien Google Drive. |
| [Insectoid Monster Rig (SuperKapoo913)](https://sketchfab.com/3d-models/insectoid-monster-rig-01323e4b2563430f9da85cd255b6e176)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « SuperKapoo913 » | Gratuit | Insectoïde ailé riggé, 13 156 faces, 1 animation : guerrier ou volant de base de la Ruche. |
| [Animated Spider, araignée-loup animée (shadocks)](https://sketchfab.com/3d-models/animated-spider-af87017501fc44e39a33c220f2435100)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « shadocks » | Gratuit | Araignée réaliste, environ 20,6 k triangles, 7 animations (marches, course, attaque, mort), avec un tag Unity : chasseuse de la Ruche à agrandir. |
| [Arahnid, Cockroach, blatte géante animée (stasbelyk13)](https://sketchfab.com/3d-models/arahnid-cockroach-d07ec0641ee74ece96a992db64ecff5f)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « stasbelyk13 » | Gratuit | Blatte réaliste riggée, 4 animations, textures 4K, 81 760 faces : unité d'essaim rapide de la Ruche. |
| [Scorpion animé game-ready (anthonyvanoo)](https://sketchfab.com/3d-models/scorpion-4c0fd62d33ab4d2592ee0a1d985a480d)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « anthonyvanoo » | Gratuit | Scorpion riggé, 5 animations, 7 290 faces, bake ZBrush et texturage Substance : ennemi du désert (secteurs 1 et 2), version géante possible. |
| [Spiderthing take 3 (Rasmus82)](https://sketchfab.com/3d-models/spiderthing-take-3-10bb4cf49d304d64afd2b829666f6caf)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « Rasmus82 » | Gratuit | Créature arachnoïde horrifique, 2 animations, 67 972 faces : élite de la Ruche ou mutation. |
| [Spider monster Rigged, rig IK (Ploobert)](https://sketchfab.com/3d-models/spider-monster-rigged-ef71c4f221304a7ea9f01b691867f3d0)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « Ploobert » | Gratuit | Monstre araignée-crabe charnu avec rig IK, 27 380 faces : se prête à une locomotion procédurale (Animation Rigging) sur les dunes. |
| [Phenebub, créature-ver lovecraftienne (Rysky)](https://sketchfab.com/3d-models/phenebub-free-horror-creature-e18dbcb6300b4421aac2bb51cc060aed)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « Rysky » | Gratuit | Larve ou ver organique, 191 296 faces, textures Substance : larves dans les nids de la Ruche. |
| [Sand Death Worm (facerate)](https://sketchfab.com/3d-models/sand-death-worm-7a2b94b6adb242bbb12f116d40d3bcbe)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « facerate » | Gratuit | Correction : c'est une créature à trois têtes de ver tentaculaires (type hydre), pensée pour l'impression 3D, 299 986 faces, sans aucune texture. Sculpture de base pour une variante de ver ou un nid, avec retopo, texturage et rig à faire. Aucune franchise reprise. |
| [CC0 Giant Isopod (Bathynomus doederleinii), scan](https://sketchfab.com/3d-models/cc0-giant-isopod-b-doederleinii-3979c291d1f9454c90851efe291eab60)<br><sub>Sketchfab (ffishAsia-and-floraZia)</sub> | CC0 (domaine public) | Gratuit | Scan réel d'isopode géant, 582 561 faces : base de carapace segmentée pour les rampants blindés de la Ruche ou la faune de la planète océan. |
| [CC0 Chinese Red-headed Centipede (Scolopendra), scan](https://sketchfab.com/3d-models/cc0-chinese-red-headed-centipede-8f8d1c0305d04637b7abaf4cb54d25a8)<br><sub>Sketchfab (ffishAsia-and-floraZia)</sub> | CC0 (domaine public) | Gratuit | Scolopendre scannée, 178 633 faces : base d'un mille-pattes géant fouisseur (secteurs 1 et 4), à rigger en chaîne (rig serpent de Mesh2Motion). |
| [CC0 Japanese Giant Hornet (Vespa mandarinia), scan](https://sketchfab.com/3d-models/cc0-japanese-giant-hornet-f9c1f7e3f373491fbf01adf94ec8169e)<br><sub>Sketchfab (ffishAsia-and-floraZia)</sub> | CC0 (domaine public) | Gratuit | Frelon scanné, 375 650 faces : base de l'unité volante de la Ruche. |
| [Blaps mortisaga, coléoptère scanné (musée de Ciężkowice)](https://sketchfab.com/3d-models/blaps-mortisaga-d7d0e0204d4a4a5cb428f021d9429487)<br><sub>Sketchfab (WirtualneMuzeaMalopolski)</sub> | CC0 (domaine public) | Gratuit | Ténébrion scanné, 512 778 faces, carapace noire mate : scarabée blindé de la Ruche. |
| [Giant Isopod, scan RISD Nature Lab](https://sketchfab.com/3d-models/giant-isopod-46469c568b3743ec8fb4bb711949d49a)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « RISDNaturelab » | Gratuit | Second scan d'isopode géant (Bathynomus giganteus), 500 024 faces : faune abyssale de la planète océan ou larve blindée. |
| [Trilobite E. crassituberculata (PRI 49811), fossile scanné](https://sketchfab.com/3d-models/trilobite-e-crassituberculata-pri-49811-03f611a973144635b4a9019afe7393f0)<br><sub>Sketchfab (DigitalAtlasOfAncientLife)</sub> | CC0 (domaine public) | Gratuit | Fossile réel, 252 596 faces : ancêtre fossile de la Ruche dans les tombeaux et ruines (secteur 4), objet de lore. |
| [Molting Husk Insectile Horror Creature (s8819296)](https://sketchfab.com/3d-models/molting-husk-insectile-horror-creature-0e666090f6fd45b280730db98f287e65)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « s8819296 » | Gratuit | Humanoïde insectoïde en mue, 1 895 906 faces (retopo obligatoire). Attention : l'auteur indique un modèle « Generated with the help of AI », donc une protection par le droit d'auteur moins solide et une topologie brute. Référence d'hybride humain-Ruche. |
| [Sandworm horror creature (FredDrabble)](https://sketchfab.com/3d-models/sandworm-horror-creature-7bf94fbb7e4343c99923c91897beea40)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « FredDrabble » | Gratuit | Correction : malgré son nom, ce n'est pas un ver mais un humanoïde mutant bipède avec des gueules circulaires sur le torse (vu sur l'aperçu), 265 196 faces, sculpté dans Sculptris. Hybride ou mutant de la Ruche, pas un plan d'attaque de ver. Le filigrane « Fred Drabble » n'apparaît que sur l'aperçu. |

### Légion Mécanique

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Medium Mech Striker, mech game-ready, 41 animations (MSGDI)](https://sketchfab.com/3d-models/medium-mech-striker-27ba717c173a40b7841d2f2c6a89d823)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « MSGDI » | Gratuit | Pièce maîtresse : 10 778 faces, 41 animations (in-place et root motion), armes modulaires, bras destructibles, 5 jeux de textures, PSD inclus. Mech standard de la Légion. Seule la version Striker est gratuite ; les autres mechs MSGDI ne sont pas téléchargeables. |
| [Mech Drone (skudgee)](https://sketchfab.com/3d-models/mech-drone-8d06874aac5246c59edb4adbe3606e0e)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « skudgee » | Gratuit | Robot-drone de réparation, 9 131 faces, 1 animation, plus de 2 600 likes : unité de maintenance de la Légion dans les usines. |
| [Biped robot (skudgee)](https://sketchfab.com/3d-models/biped-robot-801d2a245e4a4405a0c2152b35b5e486)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « skudgee » | Gratuit | Robot bipède humanoïde jaune, 16 618 faces, 1 animation : marcheur léger de patrouille. |
| [mech M-6k (comrade1280)](https://sketchfab.com/3d-models/mech-m-6k-6eba9b1addb7411a85b0c86e41a39a2e)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « comrade1280 » | Gratuit | Marcheur bipède métallique, 193 193 faces, non animé : mech lourd de la Légion ou épave du monde industriel. |
| [mech TL-22 (comrade1280)](https://sketchfab.com/3d-models/mech-tl-22-2694c68765aa4f67ab81a259ff39d654)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « comrade1280 » | Gratuit | Plateforme-char bipède armée et usée, 145 292 faces : artillerie mobile de la Légion. |
| [W9231 Combat Mech (Rigged) (anusher3D)](https://sketchfab.com/3d-models/w9231-combat-mech-rigged-2388257bc61049bf841ea4094af7faa7)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « anusher3D » | Gratuit | Mech lourd riggé à canons rotatifs, 70 832 faces, aucune animation fournie : mech d'assaut de première ligne. |
| [AR-22 Armadillo (Heavy Battle Mech) (n1k77)](https://sketchfab.com/3d-models/ar-22-armadillo-heavy-battle-mech-6355e3510a544ce4bb219b0b5832933c)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « n1k77 » | Gratuit | Mech-forteresse à railguns et tourelles anti-aériennes, 986 867 faces (à optimiser) : boss de la Légion. |
| [Combat Robot (ilushandro)](https://sketchfab.com/3d-models/combat-robot-55fe65bbb4074979ac0cb44850df4ddc)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « ilushandro » | Gratuit | Robot de combat lourd à miniguns, hard-surface propre, 122 090 faces, réalisé pour un cours (Kaino) : soldat d'élite ou tourelle mobile de la Légion. |
| [Robot M.A.N.U.L., riggé low poly (milinam2002)](https://sketchfab.com/3d-models/robot-manul-9e95bc80bfa643d8b890cfa4f83e8b3d)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « milinam2002 » | Gratuit | Robot ouvrier ou militaire riggé, 21 566 faces : ouvriers mécaniques des forges retournés par la Légion. |
| [Robot Police Unit (animated) (jungle_jim)](https://sketchfab.com/3d-models/robot-police-unit-animated-36bc284bf5b84a6597db8f6db053c095)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « jungle_jim » | Gratuit | Robot anti-émeute riggé avec cycle de marche, 20 053 faces : sentinelle de la Légion. Remplacer le texte « POLICE » de la poitrine. |
| [Light Assault Mech (Lagst)](https://sketchfab.com/3d-models/light-assault-mech-d1ca5065b5184646bc7028597d0ce049)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « Lagst » | Gratuit | Mech léger animé à deux canons de 80 mm, 72 633 faces : unité de harcèlement rapide. |
| [Buster Drone (lavadragon)](https://sketchfab.com/3d-models/buster-drone-294e79652f494130ad2ab00a13fdbafd)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « lavadragon » (l'auteur crédite aussi Evil Cloud pour le modèle de base, à mentionner) | Gratuit | Drone mécanique riggé et animé, PBR, 16 780 faces, plus de 2 600 likes. Le nom « Dune Express » est celui d'un projet universitaire, sans lien avec la franchise. Drone d'attaque de la Légion. |
| [Scifi Drone 1.1, 5 animations (Jj.Mmhh)](https://sketchfab.com/3d-models/scifi-drone-11-dd21953e81a24bd385d16eaf0c6a8ae7)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « Jj.Mmhh » | Gratuit | Drone sphérique riggé avec 5 animations. Correction : 8 512 faces selon l'API (la description en annonce 4 188). Petit éclaireur en essaim. |
| [XRK-M Autonomous Combat Drone (TacticalGlaceon)](https://sketchfab.com/3d-models/xrk-m-autonomous-combat-drone-2153909c16b242c293b5c296ce142799)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « TacticalGlaceon » | Gratuit | Robot de combat humanoïde modulaire riggé, 25 402 faces. Texture camouflage violette, un peu stylisée, à refaire en PBR : fantassin autonome de la Légion. |
| [Sci-fi Drone « Scorpion-42 » (JazOone)](https://sketchfab.com/3d-models/sci-fi-drone-ac4d06d3a46d4d9cbc5beb33b896cde0)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « JazOone » | Gratuit | Drone volant lourd très détaillé, 567 885 faces, textures 4K : canonnière de la Légion ou plans rapprochés. |
| [Attack Drone Scifi (badge3dart)](https://sketchfab.com/3d-models/attack-drone-scifi-561c10fbfea847c09bbac6910a90f80a)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « badge3dart » | Gratuit | Drone d'attaque d'environ 13 k triangles, textures 4K en 5 variantes de couleur : drone standard en grand nombre. |
| [Futuristic Surveillance Drone (glowbox3d)](https://sketchfab.com/3d-models/futuristic-surveillance-drone-148d8e0ca26c4c5d81375496f4266685)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « glowbox3d » (clause NoAI de l'auteur : pas d'usage pour entraîner une IA, sans effet sur un jeu) | Gratuit | Drone de surveillance statique, 30 416 faces, PBR : sonde de détection qui déclenche l'alerte. |
| [Mechanical Scorpion (sinnik)](https://sketchfab.com/3d-models/mechanical-scorpion-098fe7f9592248a0be5b0d6d56cab056)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « sinnik » | Gratuit | Scorpion robotique à miniguns, 512 095 faces : ennemi qui fait le pont visuel entre Légion et Ruche dans le désert. |
| [Mechanical wasp (Low poly) (vet.doc)](https://sketchfab.com/3d-models/mechanical-wasp-low-poly-91b15801303b49af8a07f8b34218add4)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « vet.doc » | Gratuit | Guêpe mécanique, 20 970 faces, ailes lumineuses : micro-drone d'essaim. |
| [Sci-Fi Humanoid Robot (yur1_val)](https://sketchfab.com/3d-models/sci-fi-humanoid-robot-12ad786ecde246e5854a61fd4f67ed49)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « yur1_val » | Gratuit | Robot humanoïde rouge et noir texturé, 435 616 faces : fantassin ou commandant de la Légion. Prudence : le même auteur déclare, pour un autre modèle (le robot-araignée, rejeté), texturer des modèles « téléchargés » ailleurs. Vérifier l'origine avant un usage commercial. |
| [Destructible Robot – Highpoly (slava)](https://sketchfab.com/3d-models/destructible-robot-highpoly-0f9c08f0d8d94f3a9938d4d8bf423f46)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « slava » | Gratuit | Androïde démembrable un peu cartoon, 1 782 188 faces, 7 matériaux, sans texture. Correction : la version game-ready citée par l'auteur (« Destructible Robot », 19 k faces) n'est pas téléchargeable sur Sketchfab. Carcasses dans les usines, référence pour les dégâts localisés. |
| [T200 Dieselpunk Mech – Freebie (valentinwinkelmann)](https://sketchfab.com/3d-models/t200-dieselpunk-mech-freebie-a439a90d35274f94888f46a138f30615)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « valentinwinkelmann » | Gratuit | Petit marcheur dieselpunk à Gatling et projecteurs, 194 560 faces, riggé ; les pistons de Cinema 4D ne s'exportent pas en FBX. Style rétro et assez stylisé : vieux modèle oublié dans les forges (secteur 5) plutôt qu'ennemi principal. |
| [Sci fi Drones (Lukas Bobor)](https://assetstore.unity.com/packages/3d/characters/robots/sci-fi-drones-90326)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | Gratuit | Pack de 7 drones PBR (4 low-poly en 2K, 3 high-poly en 4K), sans animation annoncée, publié en 2019 sans compatibilité SRP déclarée : matériaux à convertir en HDRP. Variantes de drones de la Légion. Page vérifiée : 200, prix 0,00. |
| [Robonaut 2, robot humanoïde NASA (GLB)](https://science.nasa.gov/3d-resources/robonaut-2/)<br><sub>NASA (science.nasa.gov, 3D Resources)</sub> | NASA 3D Resources + NASA Images and Media Usage Guidelines (pas de logo NASA ; pas de soutien sous-entendu ; citer NASA) | Gratuit | Robot humanoïde réaliste (GLB de 1,07 Mo vérifié) : robot d'assistance du vaisseau-mère ou ancêtre « propre » de la Légion corrompue. |
| [RASSOR, robot excavateur de régolithe NASA (GLB)](https://science.nasa.gov/3d-resources/regolith-advanced-surface-systems-operations-robot-rassor/)<br><sub>NASA (science.nasa.gov, 3D Resources)</sub> | NASA 3D Resources + NASA Images and Media Usage Guidelines (pas de logo NASA ; pas de soutien sous-entendu ; citer NASA) | Gratuit | Robot minier à tambours (GLB de 6,3 Mo vérifié) : machines d'extraction du monde industriel réactivées par la Légion. |

### Chœur du Tombeau

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Dark Faceless cosmic Phantom (s8819296)](https://sketchfab.com/3d-models/dark-faceless-cosmic-phantom-4f16d9ace2904aafb51757d72c017308)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « s8819296 » | Gratuit | Silhouette encapuchonnée sans visage, 1 000 068 faces (à décimer) : forme principale du Chœur, avec un shader HDRP translucide ou dissolve. Même auteur que le Molting Husk, qui déclare utiliser l'IA : topologie à contrôler. |
| [Wraith Ghost Warrior in Torn Ancient Armor (s8819296)](https://sketchfab.com/3d-models/wraith-ghost-warrior-in-torn-ancient-armor-3b4ee6c3131a431db6a67021c6d863c8)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « s8819296 » | Gratuit | Spectre en armure antique déchirée, 1 963 038 faces (à décimer) : gardien des tombeaux du secteur 4. Même remarque sur l'origine probablement IA. |
| [Cloth Ghost, fantôme en tissu (andrey.sk)](https://sketchfab.com/3d-models/cloth-ghost-f4fc1cd448c04a809d106975cfa7011b)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « andrey.sk » | Gratuit | Drapé simulé dans Marvelous Designer et ZBrush, 76 464 faces : apparitions du Chœur et linceuls flottants dans les ruines. |
| [Tentacle (rigged) (CGDanielGlebinski)](https://sketchfab.com/3d-models/tentacle-rigged-8fcc783af94246a0b8febf424a4b96b9)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « CGDanielGlebinski » | Gratuit | Tentacule riggé (enroulement autour d'un objet rond), 9 715 faces, 1 animation. Les matériaux d'origine sont procéduraux dans Blender : prévoir un bake pour Unity. Appendices du Chœur, éléments organiques du cœur de l'écho. |
| [Ghostly Reaper, spectre à faux low poly (kalebiscoolio78)](https://sketchfab.com/3d-models/ghostly-reaper-d5b4e79acd684d52a1f3303edf22039b)<br><sub>Sketchfab</sub> | CC BY 4.0, créditer « kalebiscoolio78 » | Gratuit | Spectre encapuchonné à faux, 30 815 faces, imagerie très « faucheuse » de fantasy : choristes du Chœur en nombre, faux à retirer pour rester dans la sci-fi. |

### Payants (Unity Asset Store)

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Giant Sand Worm (StockLancer)](https://assetstore.unity.com/packages/3d/characters/giant-sand-worm-264332)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 59,00 $ | Option payante principale pour le ver colossal : 60 m de long, 3 skins, 30 morphs, 41 animations in-place (émergence, saut, crachats), 22 os, textures 8K, compatibilité URP et HDRP indiquée (Unity 2021.3.29). Page vérifiée : 200, prix 59,00. |
| [Astronauts (Pack) (Maksim Bugrimov)](https://assetstore.unity.com/packages/3d/characters/astronauts-pack-206540)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 29,99 $ | 3 combinaisons lourdes, un homme et une femme en combinaison légère, et 1 arme. Rigs standard compatibles Humanoid, PBR 4K, quelques animations (vol, idle, arme). HDRP indiqué. Variantes de classes d'Arpenteurs. Page vérifiée : 200, prix 29,99. |
| [Sci-FI Astronaut (HDRP + URP + Built-In, Rigged) (Cobble Games)](https://assetstore.unity.com/packages/3d/characters/sci-fi-astronaut-hdrp-urp-built-in-rigged-200136)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 20,00 $ | Astronaute riggé de 66,5 k triangles, textures 4K, matériaux HDRP natifs, pièces séparées et tête interchangeable : Arpenteur intégrable directement en HDRP. Animations non incluses, pas de LOD. Page vérifiée : 200, prix 20,00. |
| [Animalia – Spider](https://assetstore.unity.com/packages/3d/characters/animals/animalia-spider-52710)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 44,99 $ | Araignée réaliste, 57 animations à 60 fps (root motion et in-place), PBR 2K, 3 LOD, HDRP indiqué : arachnide de la Ruche de qualité professionnelle. Page vérifiée : 200, prix 44,99. |
| [Animated Mechanical Spider (Chris Burns)](https://assetstore.unity.com/packages/3d/characters/animals/insects/animated-mechanical-spider-207719)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 15,99 $ | Araignée mécanique à vérins hydrauliques animés, 107 880 triangles, 4 animations plus une pose statique, Animator Controller fourni, 2 variantes PBR 2K, compatible Built-in, URP et HDRP : marcheur de la Légion prêt à l'emploi. Page vérifiée : 200, prix 15,99. |
| [Sci Fi Space Drone PolygonR](https://assetstore.unity.com/packages/3d/characters/robots/sci-fi-space-drone-polygonr-67268)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 4,99 $ | Drone ennemi avec 2 LOD, 2 Animator (avec et sans root motion), idle, déplacement, impacts directionnels, tir et mêlée. Attention : seuls Built-in et URP sont déclarés, et les shaders de masque de couleur sont personnalisés, donc à refaire en HDRP. Page vérifiée : 200, prix 4,99. |
| [Alien Creatures: Fleshy Giant Worm (Sergi Nicolás)](https://assetstore.unity.com/packages/3d/characters/creatures/alien-creatures-fleshy-giant-worm-67816)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 19,90 $ | Vers organiques et ruche (2 modèles), idle et mort uniquement, 3 LOD, textures 2K. Pack de 2016 sans compatibilité SRP déclarée : matériaux à convertir en HDRP. Décor vivant des nids. Page vérifiée : 200, prix 19,90. |
| [Rifle Animset Pro (Kubold)](https://assetstore.unity.com/packages/3d/animations/rifle-animset-pro-15098)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 50,00 $ | 88 animations mocap de fusil à la 3e personne (visée additive, déplacements armés, tir, grenades, mêlée, morts), Humanoid Mecanim et root motion, sans code : gameplay coop armé. Page vérifiée : 200, prix 50,00. |

<a id="vaisseaux"></a>

## Vaisseaux, véhicules, matériaux de coque

### Rovers et véhicules

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Space Exploration Vehicle (SEV), rover pressurisé](https://science.nasa.gov/3d-resources/space-exploration-vehicle/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA non protégée par copyright aux États-Unis). Les NASA Media Usage Guidelines interdisent le logo NASA et toute suggestion d'un soutien de la NASA. | Gratuit | Rover pressurisé des Arpenteurs pour l'exploration coop (secteurs 1, 2 et 4). Concept réel de la NASA, fourni en GLB. |
| [[MMSEV] Space Exploration Vehicle (SEV), version texturée PBR 4K](https://sketchfab.com/3d-models/mmsev-space-exploration-vehicle-sev-d2f3b528edec4e2e8b938019d68632d0)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Variante texturée PBR 4K du rover pressurisé, prête pour HDRP. C'est le véhicule principal des joueurs. Elle fait environ 413k triangles, donc prévoir des LOD. |
| [Curiosity Rover (Detailed)](https://science.nasa.gov/3d-resources/curiosity-rover-detailed/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; selon les NASA Media Usage Guidelines, pas de logos NASA/JPL ni de suggestion de soutien) | Gratuit | Rover robotique réel. Épave de sonde ancienne, drone de reconnaissance ou base d'un rover autonome de la Légion Mécanique. Effacer les logos. |
| [Mars 2020 Perseverance Rover](https://science.nasa.gov/3d-resources/mars-2020-perseverance-rover/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Rover scientifique réaliste. Sonde abandonnée à retrouver (secteurs 1 et 2) ou base de kitbash pour les rovers des joueurs. |
| [Lunokhod 1](https://sketchfab.com/3d-models/lunokhod-1-3f5a5f02ad53447b9dff335e42d2469c)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Rover robotique historique soviétique. Épave de sonde rétro, ou base pour un petit drone à roues de la Légion. |
| [H500 - Explorer Truck](https://sketchfab.com/3d-models/h500-explorer-truck-89ea8e69b87942a881f9b89fd04ec00f)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Camion d'exploration et de transport sci-fi (48k triangles, game-ready). Véhicule coop pour plusieurs joueurs ou convoi abandonné. |
| [Lunar Rover (concept rétro-futuriste)](https://sketchfab.com/3d-models/lunar-rover-b2503f437acd48ce8cbd05ce7d9a8c2c)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Rover lunaire ou martien d'inspiration soviétique, texturé dans Substance. Variante de rover joueur ou épave dans le monde industriel (5). |
| [Sci-Fi Buggy](https://sketchfab.com/3d-models/sci-fi-buggy-085bd97876a64eacbc041299ac945e9c)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Buggy tout-terrain rapide avec intérieur. Véhicule d'éclaireur pour les dunes de la mer de verre (2). |
| [Military Landvehicle Kit 1.2 (kitbash)](https://sketchfab.com/3d-models/military-landvehicle-kit-12-cc0-91f495435f624d8b97a768f692aa6ce9)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Kit de pièces (roues, chenilles, tourelles, écoutilles, phares, antennes) pour assembler rovers, transports blindés et véhicules de la Légion Mécanique. |

### Épaves

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Mars2020 parachute and backshell crashsite (photogrammétrie réelle)](https://sketchfab.com/3d-models/mars2020-parachute-and-backshell-crashsite-008a288caa504703a2d853fb56a2569f)<br><sub>Sketchfab</sub> | CC BY-SA 4.0 (créditer, partage identique) | Gratuit | Débris réalistes d'un bouclier de rentrée et de son parachute. Points d'intérêt ou caches de butin dans le désert du trou noir (1) et la mer de verre (2). |
| [Ancient Colony Ship](https://sketchfab.com/3d-models/ancient-colony-ship-956d09cb2e53483f90c24c72ab69bb0d)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Vaisseau-colonie interstellaire très ancien et abandonné. Épave géante à moitié enfouie dans la mer de verre (2) ou la vallée des tombeaux (4), en lien avec le mystère du lore. Environ 31k triangles. |
| [Crashed Spaceship (scène d'épave)](https://sketchfab.com/3d-models/crashed-spaceship-f2696bbc764e45a3a22d18e4150635a4)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Site de crash prêt à poser, texturé dans Substance. Épave de navette au bord du trou noir (1) ou objectif de mission de sauvetage. |

### Navettes de largage

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Drop Ship or light attack, mars defense force](https://sketchfab.com/3d-models/drop-ship-or-light-attack-mars-defense-force-0b169f4ebcac4eb287002f129d55bfff)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Navette de largage VTOL militaire et crédible pour déposer les Arpenteurs. Textures 4K, environ 177k triangles. |
| [Transporter Spaceship (transport militaire high-poly)](https://sketchfab.com/3d-models/transporter-spaceship-fe581b60c8ea4ea591a24a483f25ddad)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Grosse navette de transport de troupes ou de matériel, utilisable pour l'extraction en fin de mission. 610k triangles : à décimer ou à réserver aux cinématiques. |
| [Sci-fi Dropship](https://sketchfab.com/3d-models/sci-fi-dropship-709a1916c0cc4cbebe197e7ef26964cb)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Navette de largage légère, environ 36k triangles et textures Substance. Bonne option jouable ou de fond pour les largages répétés. |
| [Sci-fi dropship - modular spaceship](https://assetstore.unity.com/packages/3d/vehicles/space/sci-fi-dropship-modular-spaceship-212617)<br><sub>Unity Asset Store (Mixall)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | 29,99 USD (prix affiché, hors taxes) | Option payante si les navettes gratuites ne suffisent pas : navette de largage modulaire prête pour Unity. Vérifier la compatibilité HDRP avant achat. |

### Grands vaisseaux

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [D.S.S. Harbinger battle cruiser](https://sketchfab.com/3d-models/dss-harbinger-battle-cruiser-474f62d00ed54212b37f93ce91569c53)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Vaisseau-mère en orbite, visible dans le ciel et dans les cinématiques de largage. Design original, 73k triangles, textures tuilées et AO. |
| [Federation Corvette F3](https://assetstore.unity.com/packages/3d/vehicles/space/federation-corvette-f3-79860)<br><sub>Unity Asset Store (CGPitbull)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | Gratuit | Corvette détaillée (mesh unique, sans intérieur). Vaisseau d'escorte du vaisseau-mère ou épave en orbite. |

### Cargos

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [(12) ORYX Very Heavy Freighter (textured)](https://sketchfab.com/3d-models/12-oryx-very-heavy-freighter-textured-de824e4d997640c09bc4ccbce3064fbb)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Cargo lourd de 168 m avec conteneurs et grues. Cargo en orbite, ou épave géante dans le monde industriel (5). Environ 1M de triangles, à décimer. |
| [Corinth Cargo Transporter](https://sketchfab.com/3d-models/corinth-cargo-transporter-7614d85856f6477ba2a603970dcbb0dd)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Cargo moyen hard-surface de bonne qualité. Ravitailleur de la flotte ou épave posée au sol. |
| [Mining Company Space Ships (collection)](https://sketchfab.com/3d-models/mining-company-space-ships-bbfac99760fb49ceb1c35cdb4ed5f883)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Flotte d'une compagnie minière (foreuses, cargos). Vaisseaux et épaves du monde industriel abandonné (5). |

### Chasseurs

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Hammerhead Space Fighter (canonnière)](https://sketchfab.com/3d-models/hammerhead-space-fighter-19daeb543508430c8807edbcf27a9741)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Petite canonnière armée de tourelles. Escorte de la navette de largage ou appareil de la Légion Mécanique. |
| [Space Fighter](https://sketchfab.com/3d-models/space-fighter-e766136d4871441289d37d44a4bbcd3b)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Chasseur spatial low-poly (17k triangles) prévu pour Unity. Patrouilles ou chasseurs drones de la Légion Mécanique. |
| [Star Sparrow Modular Spaceship](https://assetstore.unity.com/packages/3d/vehicles/space/star-sparrow-modular-spaceship-73167)<br><sub>Unity Asset Store (Ebal Studios)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | Gratuit | Chasseur modulaire (nombreuses pièces combinables). Patrouilleurs et escorte. Style plus « jeu » : à retexturer avec les métaux CC0 ci-dessus. |

### Intérieurs de vaisseaux

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Spacefighter Cockpit (Wasp Interdictor)](https://sketchfab.com/3d-models/spacefighter-cockpit-wasp-interdictor-db4aa67fe1164ec088083d7dac2d273f)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Cockpit détaillé pour la vue pilote de la navette ou un intérieur d'épave explorable. |
| [Sci-Fi Hanger (hangar)](https://sketchfab.com/3d-models/sci-fi-hanger-8f10be0e8b5644e1845a2a5f12af5d6c)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Hangar du vaisseau-mère : hub coop où l'on prépare la navette de largage. Low-poly, à habiller avec les matériaux métal. |
| [Sci-Fi Spaceship Bridge](https://sketchfab.com/3d-models/sci-fi-spaceship-bridge-aaa5259e54e14c79936df3d127644287)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Passerelle de commandement du vaisseau-mère (hub, briefing de mission). |
| [Spaceship Corridor](https://sketchfab.com/3d-models/spaceship-corridor-1cdd1db557b8428892af4773bdada913)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Couloir de vaisseau détaillé et modulaire. Coursives du vaisseau-mère ou des grandes épaves. |
| [Sci-fi Corridor (modulaire, abandonné)](https://sketchfab.com/3d-models/sci-fi-corridor-f3d033a8f3ad4888acc8c0a16878cf85)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Pièces de couloir modulaires et usées (PBR 2K). Intérieurs d'épaves explorables et zones du Chœur du Tombeau. |
| [Sci-Fi Styled Modular Pack](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-styled-modular-pack-82913)<br><sub>Unity Asset Store (karboosx)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | Gratuit | Kit modulaire de couloirs et salles sci-fi pour les intérieurs du vaisseau-mère et des épaves. Matériaux à convertir en HDRP. |

### Capsules

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Sci-Fi Personal Space Pod Ship](https://sketchfab.com/3d-models/sci-fi-personal-space-pod-shipweekly-challenge-b158f01dd4a9416fb689ca4401856e7a)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Pod individuel réaliste en PBR. Capsule de sauvetage, point de réapparition des joueurs ou épave de pod. |

### Engins spatiaux réels

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Orbiter Space Shuttle OV-103 Discovery (scan Smithsonian)](https://sketchfab.com/3d-models/orbiter-space-shuttle-ov-103-discovery-5f7176a2ae284d6b99fc970c9d265c04)<br><sub>Sketchfab (Smithsonian)</sub> | CC0 | Gratuit | Navette réelle scannée. Sert de référence ou de base de kitbash pour une navette ancienne, ou d'épave d'une ère oubliée. Retirer les marquages NASA. |
| [Apollo 11 Command Module (Combined, intérieur et extérieur)](https://sketchfab.com/3d-models/apollo-11-command-module-combined-372bb6781922471cada4e0a9bd5c61fb)<br><sub>Sketchfab (Smithsonian)</sub> | CC0 | Gratuit | Capsule de rentrée scannée avec son intérieur. Capsule échouée sur la planète océan (3) ou relique dans la vallée des tombeaux (4). 721k triangles, à décimer. |
| [Viking Lander](https://science.nasa.gov/3d-resources/viking-lander/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Atterrisseur ancien, prop d'épave ou balise mystérieuse en plein désert (1). |
| [Apollo Lunar Module](https://science.nasa.gov/3d-resources/apollo-lunar-module/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Atterrisseur réaliste. Référence pour une petite navette d'atterrissage, ou module abandonné en surface. |
| [Orion Capsule](https://science.nasa.gov/3d-resources/orion-capsule/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Capsule d'équipage moderne. Module d'évacuation du vaisseau-mère ou capsule tombée en mer (3). |

### Modules de base

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Habitat Demonstration Unit](https://science.nasa.gov/3d-resources/habitat-demonstration-unit/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Module d'habitat de la NASA. Base avancée ou camp des Arpenteurs posé à la surface. |
| [HAB - Mars Desert Research Station (scan lidar)](https://sketchfab.com/3d-models/hab-mars-desert-research-station-5c1f4c9726754652946098616b7614a1)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Habitat cylindrique réel de 8 m scanné en lidar. Module de base très crédible dans le désert (1). 500k triangles, à optimiser. |
| [Habitat Lander Pod](https://sketchfab.com/3d-models/habitat-lander-pod-9a55af34f7654097bd72f0642d83ccce)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Module d'habitat atterrisseur. Avant-poste largué depuis le vaisseau-mère. |

### Stations orbitales

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Gateway Lunar Space Station (Gateway Core)](https://science.nasa.gov/3d-resources/gateway-lunar-space-station/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Station orbitale réaliste. Relais orbital ou station autour du trou noir (1). |
| [International Space Station (ISS) (C) (High Res)](https://science.nasa.gov/3d-resources/international-space-station-iss-c-high-res/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | ISS en haute résolution. Station orbitale de fond ou base de kitbash (modules, panneaux solaires, radiateurs) pour le vaisseau-mère. |
| [Space Station Modules](https://sketchfab.com/3d-models/space-station-modules-e3ba39a1c78540448542cf937b11feab)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Modules de station en PBR 4K, assemblables. Station orbitale, ou modules préfabriqués posés au sol comme base. |
| [Space Station Free 3D Asset (HDRP + URP + Built-In)](https://assetstore.unity.com/packages/3d/vehicles/space/space-station-free-3d-asset-hdrp-urp-built-in-188734)<br><sub>Unity Asset Store (Cobble Games)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | Gratuit | Station spatiale en 14 modules réarrangeables, compatible HDRP d'origine (1,8 Go). Vaisseau-mère ou station orbitale. |
| [MODULAR Space Station # MS2](https://assetstore.unity.com/packages/3d/vehicles/space/modular-space-station-ms2-117505)<br><sub>Unity Asset Store (CGPitbull)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | 36 USD (prix affiché, hors taxes) | Station spatiale modulaire pour un vaisseau-mère ou une station autour du trou noir. Option payante abordable. |
| [MODULAR # Mining Station](https://assetstore.unity.com/packages/3d/vehicles/space/modular-mining-station-184582)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | 32 USD (prix affiché, hors taxes) | Station minière modulaire. Infrastructure orbitale du monde industriel abandonné (5). |

### Stations et intérieurs

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [International Space Station (ISS) (E) (Internal)](https://science.nasa.gov/3d-resources/international-space-station-iss-e-internal/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Intérieurs de modules de l'ISS. Coursives réalistes du vaisseau-mère ou d'une station abandonnée. |

### Antennes et radars

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Deep Space Network 70-meter (antenne géante)](https://science.nasa.gov/3d-resources/deep-space-network-70-meter/)<br><sub>NASA 3D Resources</sub> | Usage libre, y compris commercial (œuvre NASA ; pas de logo NASA ni de suggestion de soutien) | Gratuit | Radiotélescope géant réel. Station d'écoute du trou noir (1) ou repère visuel majeur dans un secteur. |
| [Industrial Antenna](https://sketchfab.com/3d-models/industrial-antenna-001970c2594e4aa6bef46e63fc9e0c5a)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Grande antenne industrielle sur socle béton. Relais de communication dans le monde industriel (5) ou le désert (1). |
| [Satellite Comms (parabole militaire)](https://sketchfab.com/3d-models/satellite-comms-4be56951da39429a963ee12140bbff57)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Parabole de communication. Balise de camp de base ou objectif de réparation coop. |
| [Radio Communications Tower (modulaire)](https://sketchfab.com/3d-models/radio-communications-tower-70a1996b614e4c7ba8f01d4295b8fff4)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Tour radio modulaire avec clôture. Avant-postes et relais à reconquérir. |
| [Rusted Antennas](https://sketchfab.com/3d-models/rusted-antennas-86303d7c8cb442c2bcd138abe2b01c9f)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Antennes et paraboles rouillées. Habillage du monde industriel abandonné (5) et des épaves. |
| [Old Scifi satellite dish](https://sketchfab.com/3d-models/old-scifi-satellite-dish-f01a91e9773e4622b25c4d27903e85c6)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Petite parabole low-poly game-ready. À poser en grand nombre sur les modules de base et les toits d'usines. |
| [HDR-O Radar CAT-19](https://sketchfab.com/3d-models/hdr-o-radar-cat-19-30ed76a6b4e44cf4b429a6d764e97054)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Radar hard-surface sci-fi. Capteur de la Légion Mécanique ou équipement de base. |

### Conteneurs et props

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Sci-Fi Container](https://sketchfab.com/3d-models/sci-fi-container-96fb7ded0eb44cd18dd5c1e7bb3eb74f)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Conteneur cargo hard-surface. Ravitaillement largué, caisses de butin. |
| [Military Weapons Container](https://sketchfab.com/3d-models/military-weapons-container-8a1c32f395f549b8b22265b8a459fa8e)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Caisse militaire « near future » usée, en 4K. Caisse d'armes ou d'équipement des Arpenteurs. |
| [Sci-fi cargo crate](https://sketchfab.com/3d-models/sci-fi-cargo-crate-ac36898521304e51a8e305aa602f1f5b)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Caisse cargo pour les soutes et les hangars. L'auteur signale que les textures correctes sont sur sa page ArtStation. |
| [Futuristic cargo container Low-Poly](https://sketchfab.com/3d-models/futuristic-cargo-container-low-poly-015f32ec950744c4898e17af07d43f65)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Conteneur low-poly. Empilements de fond dans les docks, les hangars et les cargos. |
| [Sci-Fi Fuel Tank](https://sketchfab.com/3d-models/sci-fi-fuel-tank-3d13bb3bb7c149cda5bc0a14abedd120)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Réservoir de carburant réaliste et sale. Avant-postes, ravitaillement des navettes, props explosifs. |
| [SCIFI GENERATOR](https://sketchfab.com/3d-models/scifi-generator-bf2e1a087adb4628a376ffbed17c41ff)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Générateur en textures 4K. Source d'énergie de la base, objectif « réactiver le générateur » dans le monde industriel (5). |
| [Solar Panel (sur pied, usé)](https://sketchfab.com/3d-models/solar-panel-83483a66f8974d7e8f2e7bbde518d606)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Panneau solaire usé sur pied. Camps de base et avant-postes du désert (1). |

### Matériaux métal

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Metal Plates 017 A (panneaux sci-fi)](https://ambientcg.com/view?id=MetalPlates017A)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Panneaux métalliques sci-fi (tag « scifi ») pour les coques de navettes, du vaisseau-mère et des modules. |
| [Metal Plates 015 A (panneaux sci-fi)](https://ambientcg.com/view?id=MetalPlates015A)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Seconde variante de panneaux sci-fi pour varier les coques et les murs intérieurs. |
| [Metal Plates 006](https://ambientcg.com/view?id=MetalPlates006)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Plaques métalliques rivetées propres. Coques et blindages de véhicules. |
| [Metal Plates 013 (rouillées)](https://ambientcg.com/view?id=MetalPlates013)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Plaques rouillées pour les épaves, les cargos abandonnés et le monde industriel (5). |
| [Painted Metal 017 (bandes de danger)](https://ambientcg.com/view?id=PaintedMetal017)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Métal peint avec marquage danger. Rampes de navette, zones de largage, portes de hangar. |
| [Painted Metal 009 (jaune rouillé)](https://ambientcg.com/view?id=PaintedMetal009)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Peinture industrielle jaune écaillée. Engins miniers, grues et rovers utilitaires. |
| [Diamond Plate 008 C (tôle larmée)](https://ambientcg.com/view?id=DiamondPlate008C)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Sols de soute, marchepieds de rover, passerelles de hangar. |
| [Corrugated Steel 007 A (tôle ondulée bleue rouillée)](https://ambientcg.com/view?id=CorrugatedSteel007A)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Modules préfabriqués, abris de base et conteneurs. |
| [Metal 046 B (métal sombre sale)](https://ambientcg.com/view?id=Metal046B)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Métal noir encrassé pour les coques de la Légion Mécanique et les pièces moteur. |
| [Metal 053 C (fer endommagé rouillé)](https://ambientcg.com/view?id=Metal053C)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Fer ancien abîmé pour les épaves très vieilles (vaisseau-colonie, sondes oubliées). |
| [Sheet Metal 001 (tôle perforée)](https://ambientcg.com/view?id=SheetMetal001)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Grilles et panneaux perforés (passerelles, aérations, protections de moteur). |
| [Solar Panel 003 (matériau)](https://ambientcg.com/view?id=SolarPanel003)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Cellules solaires pour les ailes de satellites, les stations et les modules de base. |
| [Blue Metal Plate](https://polyhaven.com/a/blue_metal_plate)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Tôle peinte rayée et usée. Livrée de la navette de largage et des modules des Arpenteurs. |
| [Green Metal Rust](https://polyhaven.com/a/green_metal_rust)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Métal peint vert rouillé (style cuve ou citerne). Conteneurs et véhicules militaires abandonnés. |
| [Metal Plate 02 (tôle corrodée)](https://polyhaven.com/a/metal_plate_02)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Tôle corrodée réaliste pour les coques d'épaves et les cargos échoués. |
| [Container Side](https://polyhaven.com/a/container_side)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Paroi de conteneur cabossée pour les conteneurs cargo et les modules préfabriqués. |
| [Rusty Painted Metal](https://polyhaven.com/a/rusty_painted_metal)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Peinture écaillée sur rouille pour les machines et les coques vieillies (secteur 5). |
| [Yughues Free Metal Materials](https://assetstore.unity.com/packages/2d/textures-materials/metals/yughues-free-metal-materials-12949)<br><sub>Unity Asset Store (Nobiax / Yughues)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | Gratuit | Lot de matériaux métal (plaques, grilles, panneaux) en complément d'ambientCG et Poly Haven. Pack ancien : matériaux à reconfigurer pour HDRP. |
| [20 PBR Metal Materials](https://assetstore.unity.com/packages/2d/textures-materials/metals/20-pbr-metal-materials-49140)<br><sub>Unity Asset Store (Adam Bielecki)</sub> | Standard Unity Asset Store EULA (usage commercial dans un jeu autorisé) | 5 USD (prix affiché, hors taxes) | Petit lot de 20 métaux PBR pas cher, en appoint des matériaux CC0 pour les coques. |

<a id="architecture"></a>

## Architecture : bases, ruines, tombeaux

### Industriel (bases, usines, conduites)

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [RG Ross & Sons Steam Hammer (marteau-pilon 1907, scan)](https://sketchfab.com/3d-models/rg-ross-sons-steam-hammer-d80b52dc800e4c76b6b27749bbd8e782)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 5 : pièce maîtresse de la forge abandonnée. Un vrai marteau-pilon scanné (environ 70k triangles), à agrandir pour en faire une machine colossale de la Légion Mécanique. |
| [Modular Industrial Pipes 01](https://polyhaven.com/a/modular_industrial_pipes_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 : réseaux de tuyaux de vapeur pour les usines et les forges, à assembler en kit. Sert aussi à habiller l'intérieur des épaves. |
| [Modular Factory Facade](https://polyhaven.com/a/modular_factory_facade)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 : construire des halls d'usine et des entrepôts abandonnés, à vieillir avec les textures de rouille et de béton. |
| [Thermal power plant, Cooling tower (photogrammétrie par drone)](https://sketchfab.com/3d-models/thermal-power-plant-cooling-tower-ff3cdef5f8e24605b56648918e1f9a8a)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 5 : silhouettes de tours de refroidissement à l'horizon du monde industriel, très lisibles de loin. Prévoir une décimation et des LOD (environ 980k triangles). |
| [Abandoned Building: Polygraphenwerk Leipzig (scan d'usine abandonnée)](https://sketchfab.com/3d-models/abandoned-building-polygraphenwerk-leipzig-6fa85b289def499c95afd232173ecc27)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 5 : vraie usine de 1910 scannée, en ruine. Utilisable comme bâtiment héros ou comme référence pour le kit modulaire. |
| [Overhead Crane (pont roulant)](https://polyhaven.com/a/overhead_crane)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 : ponts roulants dans les halls de forge. Peut aussi servir de structure de maintenance dans le hangar du vaisseau-mère. |
| [Large Industrial Storage Tanks](https://sketchfab.com/3d-models/large-industrial-storage-tanks-bd3ac97071104b64bc388a155b64f79e)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 5 : grandes cuves de stockage. Les tuyaux, escaliers et cuves sont des objets séparés, donc on peut composer des raffineries de la Légion Mécanique. |
| [Industrial asset pack (Free)](https://sketchfab.com/3d-models/industrial-asset-pack-free-94c5011772a84e8791779b342467f245)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 5 : lot d'objets industriels variés pour remplir les halls (environ 64k triangles au total). |
| [Old Pipes Bundle – Modular Industrial Pack](https://sketchfab.com/3d-models/old-pipes-bundle-modular-industrial-pack-fbe3129ea77243ca92c8219f269ac5cf)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 5 : tuyaux vieillis et rouillés, plus usés que ceux de Poly Haven, pour les zones post-apocalyptiques. |
| [Boiler from the puffer « VIC 32 » (chaudière, scan)](https://sketchfab.com/3d-models/boiler-from-the-puffer-vic-32-884ed11e38054aa48a3db1d9025c881d)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 5 : chaudières de forge. Peut aussi servir de réacteur ancien dans les épaves. |
| [Horizontal and Vertical Planing Machine (machine-outil, scan)](https://sketchfab.com/3d-models/horizontal-and-vertical-planing-machine-4cb50841dd6e42938b3bc098bff92423)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 5 : grosse machine-outil d'atelier naval, pour des chaînes de fabrication de mechs à l'arrêt. |
| [Rusted Industrial Water Tank](https://sketchfab.com/3d-models/rusted-industrial-water-tank-cfc37d0aff454f29a27c8ab3325f445b)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteurs 1 et 5 : réservoirs rouillés sur pilotis, comme repères verticaux dans le paysage. |
| [Rusted factory machine](https://sketchfab.com/3d-models/rusted-factory-machine-e1aa6b17ff2a4f7d9b7be67756e12848)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 5 : machine d'usine rouillée et légère (environ 11k triangles), à disperser en nombre. |
| [Realistic Metal Scaffolding](https://sketchfab.com/3d-models/realistic-metal-scaffolding-c1f03864ec72449c8968f0630c287c8d)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteurs 4 et 5 : échafaudages autour des monolithes en cours d'étude et des usines en réparation. |
| [Container Pack (13 conteneurs avec intérieurs)](https://sketchfab.com/3d-models/container-pack-0d416a9bcfc14978aa50e80281dfe9a5)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Zones de largage, dépôts de la Légion Mécanique et cargaisons d'épaves. Comprend 8 conteneurs de 40 pieds et 5 de 20 pieds, avec coque intérieure. |
| [Construction Site Asset Pack](https://sketchfab.com/3d-models/construction-site-asset-pack-a6cccf5af6514f2f9289beeead38fe1b)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Camps d'Arpenteurs et chantiers abandonnés : piles de planches, barrières et petits objets. |
| [Abandoned Old Soviet Factory Facade (scan)](https://sketchfab.com/3d-models/abandoned-old-soviet-factory-facade-free-b3801b97dcda485c9148f361eeec0c60)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 5 : façade de briques avec vitres brisées et gravats au sol, pour les rues d'usines. |
| [QE2 Starboard High Pressure Turbine Pattern](https://sketchfab.com/3d-models/qe2-starboard-high-pressure-turbine-pattern-8e135c1a917c476a98f011a48c4192c7)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 5 et épaves : carter de turbine géant. C'est un modèle de fonderie en bois, il faut le retexturer en métal. |
| [Modular Airduct Rectangular 01](https://polyhaven.com/a/modular_airduct_rectangular_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Plafonds d'usine et coursives techniques du vaisseau-mère. |
| [Barrel 01 (baril métallique)](https://polyhaven.com/a/Barrel_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Barils de carburant ou explosifs pour le gameplay coop, dans tous les secteurs habités. |
| [Portable Generator](https://polyhaven.com/a/portable_generator)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Campements et balises des Arpenteurs, générateurs à réactiver (objectif de mission). |
| [Modular Electric Cables](https://polyhaven.com/a/modular_electric_cables)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Habillage des murs d'usine, des cockpits et des épaves, câbles arrachés. |
| [Industrial Caged Sconce (applique grillagée)](https://polyhaven.com/a/industrial_caged_sconce)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Éclairage d'ambiance HDRP des couloirs d'usine, bunkers et coursives. |
| [Worn Metal Rack](https://polyhaven.com/a/worn_metal_rack)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Entrepôts et ateliers abandonnés, réserves de pièces détachées de la Légion Mécanique. |
| [Old Military Compressor](https://polyhaven.com/a/old_military_compressor)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Petit objet mécanique pour les ateliers et les avant-postes militaires abandonnés. |
| [hdrp warehouse modular pack](https://assetstore.unity.com/packages/3d/environments/industrial/hdrp-warehouse-modular-pack-224830)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | 6,99 USD | Secteur 5 : entrepôts modulaires déjà en HDRP, l'option payante la moins chère. |
| [Old Factory pack with interior](https://assetstore.unity.com/packages/3d/environments/industrial/old-factory-pack-with-interior-271385)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | 25 USD | Secteur 5 : usine ancienne avec intérieur, compatible HDRP. |
| [Abandoned Factory Environment (Tirgames)](https://assetstore.unity.com/packages/3d/environments/industrial/abandoned-factory-environment-83392)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | 35 USD | Secteur 5 : environnement d'usine abandonnée complet, compatible Built-in, URP et HDRP. La meilleure option payante si le kit gratuit ne suffit pas. |

### Ruines, tombeaux, monolithes, statues

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Bunker O3/23/A-160 Z (casemate en béton, photogrammétrie)](https://sketchfab.com/3d-models/bunker-o323a-160-z-11ae7599f3b640569f7ed4692acabf38)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteurs 1 et 5 : avant-postes en béton à moitié enfouis dans le sable, abris pour les Arpenteurs. Ce béton monolithique a un rendu très réaliste. |
| [Daithi's Pillar Stone (menhir en grès rouge, scan)](https://sketchfab.com/3d-models/daithis-pillar-stone-9480c0cb937647afbf0a5778ac6febc4)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 : monolithes de la vallée des tombeaux. Agrandi, il peut aussi servir de pierre-écho au cœur de l'écho. Décimer (1,3M triangles). |
| [Piedras de Callanish / Callanish Stones (cercle de menhirs)](https://sketchfab.com/3d-models/piedras-de-callanish-callanish-stones-7da7219ac3304850b2dab4133b9e183b)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : site rituel du Chœur du Tombeau, un alignement de monolithes autour d'un point d'intérêt. Décimer (1,5M triangles). |
| [Temple Of Augustus Ruins (photogrammétrie)](https://sketchfab.com/3d-models/temple-of-augustus-ruins-a974316de2454609aa841790844e3571)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : ruines de temple au pied d'une falaise et à l'entrée d'une grotte, un décor clé pour la vallée des tombeaux. |
| [St Pancras New Church Crypt (crypte voûtée, photogrammétrie)](https://sketchfab.com/3d-models/st-pancras-new-church-crypt-85c68a9125f946af8f5102bb426e2f86)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : intérieur souterrain des tombeaux, avec des couloirs voûtés pour les apparitions du Chœur du Tombeau. Décimer (près de 2M triangles). |
| [Pyramidion van Ptahemwia (pierre sommitale égyptienne)](https://sketchfab.com/3d-models/pyramidion-van-ptahemwia-f92ca6a968634540aab7960012b6b059)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 : agrandi, il devient une pyramide monolithique gravée. Sinon, petite relique à récupérer dans les tombeaux. |
| [Egyptian false door, about 2400 BCE (stèle-porte de tombeau)](https://sketchfab.com/3d-models/egyptian-false-door-about-2400-bce-ffe4b699e75849d59d8efe47d3538f03)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 : « fausse porte » scellée dans les chapelles funéraires, par laquelle le Chœur du Tombeau traverse les murs. |
| [Statue fragment of Amenhotep III (basalte)](https://sketchfab.com/3d-models/statue-fragment-of-amenhotep-iii-e21e8e13aab64a17b92c6ec4daca7d39)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteurs 2 et 4 : fragments de statues colossales à moitié enfouis dans le sable ou le verre, à agrandir. |
| [Unfinished building, remains, ruins (scan par drone)](https://sketchfab.com/3d-models/unfinished-building-remains-ruins-56210a9c376a41d882508bd15259eca6)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteurs 1 et 5 : carcasses de bâtiments en béton, restes d'une ancienne colonie. |
| [Dirty stones pile (gravats, photogrammétrie)](https://sketchfab.com/3d-models/dirty-stones-pile-893e64e62f414b439672a8770dd2869d)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteurs 4 et 5 : tas de gravats au pied des murs effondrés, pour la transition entre les ruines et le sol. |
| [KE046-022002 - KEEL - Ogham Stone (pierre dressée gravée)](https://sketchfab.com/3d-models/ke046-022002-keel-ogham-stone-0c2692e46cab4c50907578e296cfee99)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 : stèles gravées d'inscriptions anciennes, que les joueurs déchiffrent pour le lore. |
| [Monumental Runic Stone - Optimised, 20k](https://sketchfab.com/3d-models/monumental-runic-stone-optimised-20k-d95a850cd9114828a18b5dd72878d9ec)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : stèle monumentale déjà optimisée (20k triangles), utilisable directement dans le jeu. |
| [Túmulo de D. Mécia de Moura (tombeau sculpté du XVIe siècle)](https://sketchfab.com/3d-models/tumulo-de-d-mecia-de-moura-78130ac3aeac405aba947b712bea02ad)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 : sarcophages et tombeaux dans les chapelles, points d'apparition du Chœur du Tombeau. |
| [Cela / Ruins of a nuns' cell (ruine intérieure)](https://sketchfab.com/3d-models/cela-ruins-of-a-nuns-cell-507935f6790e4c278c0cd723d5dd938c)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 : pièce en ruine à ciel ouvert. Tous les murs sont réels, on peut la réutiliser comme module de ruine. |
| [Romanesque Doorway - Inchagoill (portail roman)](https://sketchfab.com/3d-models/romanesque-doorway-inchagoilll-ga040-013004-75257838d11341a4962a09567176abf6)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 : entrées de cryptes et de tombeaux. Léger (environ 11k triangles). |
| [Venus de Milo (version optimisée, SMK)](https://sketchfab.com/3d-models/venus-de-milo-statuestexturingchallenge-smk-2983d92ac4e744f485492580ca7629f2)<br><sub>Sketchfab</sub> | CC0 | Gratuit | Secteur 4 et cœur de l'écho : statues mutilées et fantomatiques liées au Chœur du Tombeau (24k triangles, à texturer). |
| [Ancient Ruins Pack (murs et piliers modulaires)](https://sketchfab.com/3d-models/ancient-ruins-pack-4dad3b80562842f88d6568c5e1c469c2)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : kit de murs et piliers effondrés en seulement 2 matériaux, pratique pour remplir la vallée de ruines. |
| [Stone Arch Pillars - Ruins (3 niveaux de destruction)](https://sketchfab.com/3d-models/stone-arch-pillars-ruins-a1a8e633c9aa40c08b4d2a0631ac61d2)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : arcades en ruine à différents stades d'effondrement, avec des briques individuelles pour les gros plans. |
| [Abbey Wall Ruins](https://sketchfab.com/3d-models/abbey-wall-ruins-37936ba6a1674f15be60075315469a43)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : pans de murs en ruine pour l'arrière-plan et le remplissage. |
| [Pillar on the agora of antic city of Sagalassos (scan)](https://sketchfab.com/3d-models/pillar-on-the-agora-of-antic-city-of-sagalassos-d623bddcf6ba4fd4a4f6cb919160b6f0)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : piliers à piédestal le long des allées des tombeaux (environ 78k triangles). |
| [Kalamos Cave (grotte marine, photogrammétrie)](https://sketchfab.com/3d-models/kalamos-cave-edb597f00e3e4a84855210219502d29a)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 3 (planète océan) : grotte creusée dans la falaise avec petite plage, refuge pendant les vagues géantes. |
| [Penmaenmawr Cairn (tas de pierres rituel, scan)](https://sketchfab.com/3d-models/penmaenmawr-cairn-9c1e3f0c3b0a49c8bfb032c6a8bf0014)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteurs 1 et 4 : cairns qui marquent des sépultures ou des chemins anciens, repères de navigation. |
| [Temple Ruins Asset Pack](https://sketchfab.com/3d-models/temple-ruins-asset-pack-d1e25cdbe51e467a87baf94ead0afa63)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Secteur 4 : pack de constructions en pierre pour un site de temple. Le style mélange stylisé et réaliste, à harmoniser avec les textures Poly Haven. |
| [Gothic Statue](https://polyhaven.com/a/gothic_statue)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 4 : statues gardiennes des tombeaux, avatars de pierre du Chœur du Tombeau. |
| [Large Castle Door](https://polyhaven.com/a/large_castle_door)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 4 : portes scellées des tombeaux, qui s'ouvrent au fil de la progression. |
| [Modular Fort 01 (remparts modulaires)](https://polyhaven.com/a/modular_fort_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 3 et 4 : remparts anciens en ruine au bord de la mer ou autour de la nécropole. |
| [Altar Ruins Free](https://assetstore.unity.com/packages/3d/environments/fantasy/altar-ruins-free-109065)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | Gratuit | Secteur 4 : autel en ruine pour les rituels du Chœur du Tombeau. Compatibilité HDRP non indiquée, matériaux à convertir. |
| [Ancient Temple Ruins PBR](https://assetstore.unity.com/packages/3d/environments/historic/ancient-temple-ruins-pbr-147525)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | 14,99 USD | Secteur 4 : temple en ruine PBR, si les scans gratuits ne suffisent pas. Compatibilité HDRP non indiquée. |
| [Egyptian Tomb Pack (Nokobot)](https://assetstore.unity.com/packages/3d/environments/historic/egyptian-tomb-pack-104873)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | 14,99 USD | Secteur 4 : couloirs et chambres funéraires modulaires. Compatibilité HDRP non indiquée, matériaux à convertir. |

### Intérieurs sci-fi

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Apollo 11 Command Module Interior (Smithsonian)](https://sketchfab.com/3d-models/apollo-11-command-module-interior-11b6ad900ab7423ea9d3bd7790eedcea)<br><sub>Sketchfab / Smithsonian</sub> | CC0 | Gratuit | Cockpit de la navette de largage : intérieur de capsule réel scanné, dans le ton « Interstellar » réaliste. Sert de base ou de référence pour les panneaux et l'ergonomie. |
| [Vintage Spacecraft Instrument](https://polyhaven.com/a/vintage_spacecraft_instrument)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Tableaux de bord analogiques de la navette et des épaves, dans le style rétro-réaliste d'Interstellar. |
| [Sci-fi Ship interior - Modular asset pack](https://sketchfab.com/3d-models/sci-fi-ship-interior-modular-asset-pack-50e2af8800cc4ab79add375f817b2d76)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Coursives et salles du vaisseau-mère (hub des joueurs), avec murs et objets de détail. |
| [Space Colony Modular Kit Bash](https://sketchfab.com/3d-models/space-colony-modular-kit-bash-1ac694729ad04977ba93dafa66914aff)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Intérieurs de colonies et de bases abandonnées : murs à emboîter pour faire des pièces et des couloirs, avec des objets de décor. |
| [Spaceship Cockpit + Seat (riggé)](https://sketchfab.com/3d-models/spaceship-cockpit-seat-8fe765ad88bf4da4a32e3656dfbe9133)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Cockpit jouable de la navette de largage (commandes et écrans riggés). Prévoir une optimisation (environ 590k triangles). |
| [Free Airlock Door (porte de sas animée)](https://sketchfab.com/3d-models/free-airlock-door-58729c7672ae4e01a6103617c2f4f55e)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Sas de la navette, du vaisseau-mère et des bunkers. Animation incluse, environ 4,4k triangles. |
| [Sci Fi Hallway FREE](https://sketchfab.com/3d-models/sci-fi-hallway-free-44fd25310d2240908027390a078368e1)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Couloirs du vaisseau-mère : sol, murs, plafond, sortie et porte séparés. Non optimisé pour le jeu selon l'auteur, retopologie à prévoir. |
| [3D Scifi Kit Starter Kit (Creepy Cat)](https://assetstore.unity.com/packages/3d/environments/3d-scifi-kit-starter-kit-92152)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | Gratuit | Kit modulaire sci-fi gratuit et compatible HDRP pour prototyper les intérieurs du vaisseau-mère et des bases. |
| [Space Station - Modular Scifi Environment (Ilumisoft)](https://assetstore.unity.com/packages/3d/environments/sci-fi/space-station-modular-scifi-environment-197617)<br><sub>Unity Asset Store</sub> | Licence standard Unity Asset Store (EULA, usage commercial autorisé) | 20 USD | Intérieurs du vaisseau-mère de bonne qualité, compatibles HDRP. |

### Textures : béton, métal, pierre

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Rabdentse Ruins Wall (texture)](https://polyhaven.com/a/rabdentse_ruins_wall)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 4 : murs de ruines anciennes et de tombeaux, avec un rendu de maçonnerie érodée. |
| [Medieval Blocks 06 (texture, gros blocs anciens)](https://polyhaven.com/a/medieval_blocks_06)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 4 : murs de forteresse et de temple en gros appareil, socles de monolithes. |
| [Large Sandstone Blocks 01 (texture)](https://polyhaven.com/a/large_sandstone_blocks_01)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteurs 1 et 4 : temples et tombeaux désertiques, en accord avec le sable des secteurs désertiques. |
| [Rebar Reinforced Concrete (texture)](https://polyhaven.com/a/rebar_reinforced_concrete)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 : béton qui s'effrite avec les fers à béton visibles, pour les usines effondrées et les bunkers. |
| [Concrete 042 C (béton endommagé, armatures rouillées)](https://ambientcg.com/view?id=Concrete042C)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 5 : murs et dalles d'usine éventrés. Complète la texture de béton armé de Poly Haven. |
| [Metal Walkway 014 (passerelle métallique rouillée)](https://ambientcg.com/view?id=MetalWalkway014)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 5 et vaisseau-mère : passerelles ajourées (avec alpha) au-dessus des cuves et des machines. |
| [Rusty Corrugated Iron (texture)](https://polyhaven.com/a/rusty_corrugated_iron)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 : bardages d'entrepôts et abris de fortune. Sert aussi aux camps de récupérateurs. |
| [Stone Block Wall (texture « ancient walls »)](https://polyhaven.com/a/stone_block_wall)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 4 : murs de fortifications antiques et de temples. |
| [Red Sandstone Wall (texture)](https://polyhaven.com/a/red_sandstone_wall)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 1 : ruines teintées de rouge sur la planète proche du trou noir, en accord avec cracked_red_ground. |
| [Concrete Wall 008 (texture)](https://polyhaven.com/a/concrete_wall_008)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 et bunkers : murs en béton vieillis. |
| [Hangar Concrete Floor (texture)](https://polyhaven.com/a/hangar_concrete_floor)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Sol du hangar du vaisseau-mère et des usines : béton rayé et fissuré. |
| [Metal Plate (texture, plaque d'usine)](https://polyhaven.com/a/metal_plate)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Secteur 5 : sols et parois en plaques de fer peintes et rouillées. |
| [Metal Grate Rusty (texture)](https://polyhaven.com/a/metal_grate_rusty)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Grilles de ventilation et caillebotis des usines et des épaves. |
| [Concrete Debris (texture)](https://polyhaven.com/a/concrete_debris)<br><sub>Poly Haven</sub> | CC0 1.0 | Gratuit | Sols jonchés de débris dans les usines effondrées, à peindre en couche sur le terrain. |
| [Painted Metal 016 (rayures de danger jaunes)](https://ambientcg.com/view?id=PaintedMetal016)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Signalétique industrielle de la Légion Mécanique et bords de plateformes de la navette. |
| [Rust 004 (rouille industrielle)](https://ambientcg.com/view?id=Rust004)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Couche de rouille à mélanger en shader HDRP sur tous les métaux du secteur 5. |
| [Paving Stones 054 (pavés romains anciens)](https://ambientcg.com/view?id=PavingStones054)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 4 : voies anciennes qui mènent aux tombeaux. |
| [Paving Stones 149 (dalles géométriques fissurées)](https://ambientcg.com/view?id=PavingStones149)<br><sub>ambientCG</sub> | CC0 1.0 | Gratuit | Secteur 4 et cœur de l'écho : parvis géométriques et fissurés autour des monolithes. |

<a id="ciel"></a>

## Ciel et espace : HDRI, étoiles, planètes, trou noir

### Étoiles, nébuleuses, skybox

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Deep Star Maps 2020 (carte d'étoiles HDR, de 4K à 64K)](https://svs.gsfc.nasa.gov/4851)<br><sub>NASA SVS</sub> | Domaine public NASA (NASA/Goddard SVS, crédit demandé ; pas d'usage du logo NASA) | Gratuit | Skybox de base de tous les secteurs. Fichiers EXR équirectangulaires (Voie lactée + étoiles réelles), à brancher directement en HDRI Sky / Cubemap dans HDRP. Le 16K suffit pour une RTX 3060. |
| [An Elsewhere Starfield (ciel étoilé « d'ailleurs »)](https://svs.gsfc.nasa.gov/4856)<br><sub>NASA SVS</sub> | Domaine public NASA (NASA/Goddard SVS) | Gratuit | Même qualité que les Deep Star Maps, mais avec des étoiles redistribuées et une Voie lactée inclinée à 63°. Donne un ciel crédible d'un autre système stellaire (secteurs 1 à 3) sans constellations terrestres reconnaissables. EXR de 4K à 64K. |
| [The Tycho Catalog Skymap v2.0](https://svs.gsfc.nasa.gov/3572)<br><sub>NASA SVS</sub> | Domaine public NASA (NASA/Goddard SVS) | Gratuit | Couche d'étoiles seules (catalogues Tycho/Hipparcos, sans lueur galactique). À superposer en additif pour faire scintiller les étoiles ou pour un ciel nocturne très net au-dessus de la mer de verre. |
| [Gaia's sky in colour (carte du ciel Gaia DR2)](https://www.esa.int/ESA_Multimedia/Images/2018/04/Gaia_s_sky_in_colour2)<br><sub>ESA</sub> | CC BY-SA 3.0 IGO (ou ESA Standard Licence au choix) – crédit ESA/Gaia/DPAC | Gratuit | Voie lactée scientifique (1,7 milliard d'étoiles) pour un ciel nocturne spectaculaire, par exemple sur l'écran du vaisseau-mère ou de la carte stellaire. Partage à l'identique obligatoire sur l'image modifiée. |
| [The Milky Way panorama (ESO, 360°)](https://www.eso.org/public/images/eso0932a/)<br><sub>ESO</sub> | CC BY 4.0 – crédit ESO/S. Brunier | Gratuit | Panorama photographique réel de 6000×3000 de toute la Voie lactée. Rendu plus « photo » que les cartes NASA, idéal pour le ciel nocturne du secteur désertique et de la vallée des tombeaux. |
| [VISTA gigapixel mosaic of the central Milky Way](https://www.eso.org/public/images/eso1242a/)<br><sub>ESO</sub> | CC BY 4.0 – crédit ESO/VVV Survey/D. Minniti | Gratuit | Mosaïque de 9 gigapixels du centre galactique (108199×81503). On peut y découper des textures de champs d'étoiles très denses pour le « Cœur de l'écho » (secteur 6) et pour les arrière-plans de cinématiques. |
| [HYG Stellar Database (Hipparcos, Yale, Gliese)](https://codeberg.org/astronexus/hyg)<br><sub>Codeberg / Astronomy Nexus</sub> | CC BY-SA 4.0 | Gratuit | Catalogue CSV d'environ 120 000 étoiles réelles (position, magnitude, couleur). Sert à générer dans Unity un ciel procédural exact (VFX Graph ou particules) et une carte de navigation interactive pour le vaisseau-mère. |
| [NIRCam « Cosmic Cliffs » in Carina (James Webb)](https://esawebb.org/images/weic2205a/)<br><sub>ESA/Webb</sub> | CC BY 4.0 – crédit NASA, ESA, CSA, and STScI | Gratuit | Image de nébuleuse de 14575×8441 px. Matière première pour les nébuleuses du ciel (plans billboard ou décalques dans la skybox), notamment au-dessus de la planète océan et dans le secteur 6. |
| [Pillars of Creation – vue visible (Hubble 2015)](https://esahubble.org/images/heic1501a/)<br><sub>ESA/Hubble</sub> | CC BY 4.0 – crédit NASA, ESA/Hubble and the Hubble Heritage Team | Gratuit | Référence et texture pour des colonnes de gaz cosmiques. Utile pour l'imagerie du Chœur du Tombeau (entité spectrale) et pour l'architecture abstraite du Cœur de l'écho. |
| [Carina Nebula « Star birth in the extreme » (mosaïque Hubble)](https://esahubble.org/images/heic0707a/)<br><sub>ESA/Hubble</sub> | CC BY 4.0 – crédit NASA, ESA, N. Smith (UC Berkeley) and The Hubble Heritage Team (STScI/AURA) | Gratuit | Mosaïque panoramique de 29566×14321 px. Assez grande pour une bande de nébuleuse qui traverse tout le ciel du secteur 1, près du trou noir. |
| [Hubble Ultra Deep Field (« Hubble sees galaxies galore »)](https://esahubble.org/images/heic0406a/)<br><sub>ESA/Hubble</sub> | CC BY 4.0 – crédit NASA, ESA, S. Beckwith (STScI) and the HUDF Team | Gratuit | Champ de milliers de galaxies, en 6200×6200. Fond pour le Cœur de l'écho, les écrans de chargement et l'interface de la carte galactique. |
| [Milky Way Skybox HDRI panorama](https://sketchfab.com/3d-models/milky-way-skybox-hdri-panorama-b57711d6a450410ca612c4a36f08ce21)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Panorama 4K de Voie lactée fait dans Blender (l'auteur indique « No AI »). Skybox alternative, plus stylisée, pour l'espace autour du vaisseau-mère. |
| [Space nebula HDRI panorama 360 skydome](https://sketchfab.com/3d-models/space-nebula-hdri-panorama-360-skydome-f7a5798b5b374576879973272d27300e)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Panorama de nébuleuse HDR 32 bits en 4K. Ciel coloré pour le Cœur de l'écho (secteur 6). |
| [Nebula Skyboxes (4 skyboxes EXR, Road Turtle Games)](https://assetstore.unity.com/packages/2d/textures-materials/sky/nebula-skyboxes-219924)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA (usage commercial autorisé dans un jeu) | Gratuit | 4 cubemaps de nébuleuses en EXR (HDR), compatibles HDRP d'après la fiche. Skyboxes prêtes à l'emploi pour le Cœur de l'écho et les séquences en orbite. |
| [Deep Space Skybox Pack (Sean Duffy)](https://assetstore.unity.com/packages/2d/textures-materials/deep-space-skybox-pack-11056)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | Gratuit | Skyboxes de galaxies, nébuleuses et planètes. Textures réutilisables en Cubemap Sky HDRP pour les menus, le hangar du vaisseau-mère ou les prototypes. |

### Outils

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Space 3D – générateur de skybox spatiale](https://tools.wwwtyro.net/space-3d/index.html)<br><sub>wwwtyro (outil web open source)</sub> | Unlicense (domaine public), code wwwtyro/space-3d | Gratuit | Génère dans le navigateur des cubemaps d'étoiles et de nébuleuses, exportables en 6 faces. Pratique pour des ciels stylisés et cohérents par secteur, en particulier les teintes irréelles du Cœur de l'écho. |

### Trous noirs (références et shaders)

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Black Hole Accretion Disk Visualization (NASA 2019)](https://svs.gsfc.nasa.gov/13326)<br><sub>NASA SVS</sub> | Domaine public NASA (NASA/Goddard SVS / J. Schnittman) | Gratuit | Référence scientifique principale pour le trou noir du secteur 1 : lentille gravitationnelle, disque visible au-dessus et au-dessous, effet Doppler (un côté plus brillant). Les images fixes et les frames peuvent aussi servir de texture lointaine. |
| [NASA Black Hole Visualization Takes Viewers Beyond the Brink (2024)](https://svs.gsfc.nasa.gov/14576)<br><sub>NASA SVS</sub> | Domaine public NASA pour la visualisation ; la musique « Tidal Force » est sous licence et doit être exclue | Gratuit | Vol jusqu'à l'horizon des événements, avec annotations (sphère de photons, distorsions). Référence pour la cinématique d'approche de la navette et pour l'effet de plongée dans le Cœur de l'écho. |
| [Doubly Warped World of Binary Black Holes (NASA 2021)](https://svs.gsfc.nasa.gov/13831)<br><sub>NASA SVS</sub> | Domaine public NASA (NASA/Goddard SVS) | Gratuit | Deux trous noirs avec disques rouge et bleu et images multiples. Bonne inspiration pour le Cœur de l'écho (lieu cosmique abstrait), ou pour un boss lié à un « écho » gravitationnel. |
| [First Image of a Black Hole – M87* (EHT)](https://www.eso.org/public/images/eso1907a/)<br><sub>ESO / Event Horizon Telescope</sub> | CC BY 4.0 – crédit EHT Collaboration (politique d'usage ESO) | Gratuit | Image réelle de l'anneau de photons. Référence de couleur et de flou pour le trou noir vu de très loin ; utilisable aussi telle quelle sur un écran scientifique dans le vaisseau. |
| [First image of our black hole – Sagittarius A* (EHT)](https://www.eso.org/public/images/eso2208-eht-mwa/)<br><sub>ESO / Event Horizon Telescope</sub> | CC BY 4.0 – crédit EHT Collaboration (politique d'usage ESO) | Gratuit | Seconde image réelle d'un trou noir (4076×4076). À utiliser pour le lore, les terminaux de données et les écrans d'artefacts. |
| [A Real-time High-quality Black Hole Shader (Eric Bruneton)](https://ebruneton.github.io/black_hole_shader/)<br><sub>GitHub Pages (ebruneton/black_hole_shader)</sub> | BSD 3-Clause (code), Copyright (c) 2020 Eric Bruneton | Gratuit | Shader temps réel physiquement correct (lentille gravitationnelle, disque d'accrétion, décalage Doppler et gravitationnel), avec démo web et article détaillé. Le GLSL est à porter en Custom Pass ou Fullscreen Shader Graph HDRP pour le trou noir du secteur 1. C'est la meilleure option gratuite trouvée, car aucun shader de trou noir HDRP vérifié n'existe sur l'Asset Store. |
| [Black Hole (modèle 3D, Nestaeric)](https://sketchfab.com/3d-models/black-hole-e410da98b1e5445eae2acafaaa53587d)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Modèle de trou noir avec disque d'accrétion (534k faces, 1600 likes). Placeholder visuel rapide ou objet lointain dans le ciel du secteur 1, en attendant un shader dédié. |
| [Black Hole – trou noir stellaire avec disque (SebastianSosnowski)](https://sketchfab.com/3d-models/black-hole-cfd16738ad2c402b9dc8e38a9c05c8d4)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Version légère (10k faces) d'un trou noir avec disque d'accrétion. Utile pour la carte stellaire, les hologrammes du vaisseau-mère ou un LOD très lointain. |

### Planètes, lunes, astéroïdes

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Solar System Scope – Textures (planètes, lunes, étoiles, jusqu'à 8K)](https://www.solarsystemscope.com/textures/)<br><sub>Solar System Scope</sub> | CC BY 4.0 (basé sur des données NASA) – crédit Solar System Scope | Gratuit | Pack complet : albédo, normales, nuages, anneaux de Saturne et fond de Voie lactée. Base pour les planètes et lunes visibles dans le ciel de chaque secteur, à recolorer et mélanger pour créer des mondes fictifs, ainsi que pour le globe de la planète océan vu depuis l'orbite. |
| [CGI Moon Kit (couleur + élévation LRO)](https://svs.gsfc.nasa.gov/4720)<br><sub>NASA SVS</sub> | Domaine public NASA (NASA/Goddard SVS, données LRO) | Gratuit | Cartes couleur et déplacement de la Lune, prévues pour le rendu 3D (TIFF jusqu'à 27K et 64 ppd). Lune géante dans le ciel de la vallée des tombeaux, ou base d'un satellite fracturé. |
| [Cassini's Best Maps of Jupiter – Cylindrical Map (PIA07782)](https://science.nasa.gov/photojournal/cassinis-best-maps-of-jupiter-cylindrical-map/)<br><sub>NASA / JPL Photojournal</sub> | Domaine public NASA (NASA/JPL/Space Science Institute) | Gratuit | Carte cylindrique réelle de Jupiter. Texture de géante gazeuse dominant l'horizon de la planète océan ; en changeant la teinte, on obtient une géante différente par secteur. |
| [Europa Voyager–Galileo SSI Global Mosaic 500m](https://astrogeology.usgs.gov/search/map/europa_voyager_galileo_ssi_global_mosaic_500m)<br><sub>USGS Astrogeology</sub> | Domaine public (USGS/NASA – Access et Use Constraints : None) | Gratuit | Lune de glace fracturée, à placer dans le ciel de la planète océan : cohérent avec un monde aquatique et ses marées géantes. |
| [Mars Viking Colorized Global Mosaic 232m](https://astrogeology.usgs.gov/search/map/mars_viking_colorized_global_mosaic_232m)<br><sub>USGS Astrogeology</sub> | Domaine public (USGS/NASA – Use Constraints : None) | Gratuit | Carte couleur globale de Mars. Planète désertique vue depuis la navette ou le vaisseau-mère (secteur 1), et palette de référence pour les ocres du sol. |
| [Moon LRO LROC WAC Global Morphology Mosaic 100m](https://astrogeology.usgs.gov/search/map/moon_lro_lroc_wac_global_morphology_mosaic_100m)<br><sub>USGS Astrogeology</sub> | Domaine public (USGS/NASA – citer les auteurs) | Gratuit | Mosaïque lunaire à 100 m/pixel, plus détaillée que le Moon Kit. Sert de texture de détail pour un astéroïde ou une lune visitable lors d'une mission de largage. |
| [Ringed World (planète à anneaux)](https://sketchfab.com/3d-models/ringed-world-6fdfb3c84fb348188d6968092be1b1b5)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Planète à anneaux détaillée (428k faces), à placer comme astre géant dans le ciel de la planète océan ou de la mer de verre. |
| [Asteroids Pack (metallic version)](https://sketchfab.com/3d-models/asteroids-pack-metallic-version-eff495d9315c47dbb2777ec80bef40d8)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Astéroïdes texturés (23k faces au total). Champ d'astéroïdes et débris autour des épaves, ainsi que la ceinture de matière qui spirale vers le trou noir. |
| [Planets URP & HDRP (Nova Shade)](https://assetstore.unity.com/packages/3d/environments/sci-fi/planets-urp-hdrp-163061)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 20,00 $ | Planètes avec shader personnalisable, compatible HDRP (Unity 2023.2 testé). Option payante abordable pour les astres vus depuis le sol ou l'orbite, sans écrire de shader d'atmosphère. |
| [Planets HDRP (FORGE3D)](https://assetstore.unity.com/packages/3d/environments/sci-fi/planets-hdrp-245357)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 73,00 $ | 39 planètes avec shader d'atmosphère volumétrique, HDRP uniquement, testé sur Unity 6000.0.41f1. Rendu le plus qualitatif trouvé pour des planètes vues de l'orbite (largage, vaisseau-mère). |
| [Space Graphics Toolkit (Carlos Wilkes)](https://assetstore.unity.com/packages/tools/level-design/space-graphics-toolkit-4160)<br><sub>Unity Asset Store</sub> | Standard Unity Asset Store EULA | 99,95 $ (souvent en promo à -50 %) | Boîte à outils spatiale complète : atmosphères, anneaux, ceintures d'astéroïdes, champs d'étoiles, jets, compatible Built-in/URP/HDRP. À n'acheter que si l'équipe veut éviter de coder ses shaders d'espace ; c'est le plus cher de la liste. |

### Reliefs réels (cartes de hauteur)

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Mars MGS MOLA DEM 463m (carte de hauteur globale)](https://astrogeology.usgs.gov/search/map/mars_mgs_mola_dem_463m)<br><sub>USGS Astrogeology</sub> | CC0 (domaine public) | Gratuit | GeoTIFF d'élévation de toute la planète Mars. On en extrait des zones (canyons, cratères) qu'on importe comme heightmap Unity Terrain pour les macro-reliefs du secteur 1. |
| [Mars MGS MOLA – MEX HRSC Blended DEM Global 200m](https://astrogeology.usgs.gov/search/map/mars_mgs_mola_mex_hrsc_blended_dem_global_200m)<br><sub>USGS Astrogeology</sub> | MOLA : CC0 ; HRSC : CC BY-SA 3.0 IGO (citer les auteurs) | Gratuit | DEM martien à 200 m, plus net que le MOLA seul. Valles Marineris et chaos martiens donnent d'excellentes falaises pour la vallée des tombeaux (secteur 4). La part HRSC impose le partage à l'identique pour les heightmaps dérivées. |
| [Moon LRO LOLA DEM 118m](https://astrogeology.usgs.gov/search/map/moon_lro_lola_dem_118m)<br><sub>USGS Astrogeology</sub> | Domaine public (USGS/NASA – citer les auteurs) | Gratuit | Élévation globale de la Lune. Les cratères réels servent de reliefs pour des zones d'impact et des cuvettes vitrifiées (mer de verre, secteur 2). |
| [Moon LRO LOLA – SELENE Kaguya TC DEM Merge 60N60S 59m](https://astrogeology.usgs.gov/search/map/moon_lro_lola_selene_kaguya_tc_dem_merge_60n60s_59m)<br><sub>USGS Astrogeology</sub> | Domaine public (USGS/NASA – citer les auteurs) | Gratuit | DEM lunaire à 59 m/pixel, le plus détaillé à l'échelle globale. Heightmaps de terrains jouables très réalistes pour les secteurs rocheux et cratérisés. |
| [HiRISE Digital Terrain Models (Mars, échelle métrique)](https://www.uahirise.org/dtm/)<br><sub>HiRISE / University of Arizona</sub> | Domaine public (politique d'usage HiRISE : « no copyright restriction » ; crédit NASA/JPL/University of Arizona demandé) | Gratuit | MNT de Mars à environ 1 m/pixel, avec des centaines de sites : champs de dunes, ravines, couches sédimentaires. Les champs de dunes martiennes forment des reliefs réels pour la mer de verre (secteur 2) et le désert du secteur 1. |
| [Moon – Petavius crater (terrain 3D)](https://sketchfab.com/3d-models/moon-petavius-crater-ce9c009b517b421eab8c8429b536382f)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Grand cratère lunaire haute résolution, déjà en mesh. Arène d'impact ou site de crash d'épave, à intégrer directement dans une scène. |
| [Mars – Olympus Mons (terrain 3D)](https://sketchfab.com/3d-models/mars-olympus-mons-53679864b41a43b0ae0459ce99d6adf0)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Volcan bouclier martien en mesh. Méga-relief visible à l'horizon du secteur 1, ou formation lointaine en skybox 3D. |
| [Mars landscape (photogrammétrie Perseverance)](https://sketchfab.com/3d-models/mars-landscape-83f6f32b0a694f939cd16bf04f5565ef)<br><sub>Sketchfab</sub> | CC BY 4.0 (créditer l'auteur) | Gratuit | Terrain martien réel reconstruit à partir de 47 photos du rover. Référence d'échelle et de matériaux, ou morceau de sol jouable pour le désert du secteur 1. |

### HDRI (ciels réels photographiés)

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Satara Night (No Lamps) – HDRI 16K](https://polyhaven.com/a/satara_night_no_lamps)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Nuit avec Voie lactée très nette et sans lampadaire. Éclairage nocturne des secteurs désertiques, combiné à une lumière directionnelle lunaire faible. |
| [Kloppenheim 02 (Pure Sky) – HDRI 24K](https://polyhaven.com/a/kloppenheim_02_puresky)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Ciel nocturne pur avec lune, sans sol. Base d'éclairage ambiant nocturne pour n'importe quel terrain Unity. |
| [Qwantani Night (Pure Sky) – HDRI 24K](https://polyhaven.com/a/qwantani_night_puresky)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Nuit claire à faible contraste, sans sol. Éclairage doux pour les phases d'exploration nocturne de la vallée des tombeaux. |
| [Dikhololo Night – HDRI 24K](https://polyhaven.com/a/dikhololo_night)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Voie lactée en très haute résolution (24576×12288). Ciel de nuit « astronomie » au-dessus des dunes vitrifiées (secteur 2). |
| [Qwantani Moon Noon (Pure Sky) – HDRI 24K](https://polyhaven.com/a/qwantani_moon_noon_puresky)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Lune visible en plein jour sous un ciel clair. Montre l'astre géant d'un monde étranger en journée, à compléter par une planète ajoutée en billboard. |
| [Belfast Sunset (Pure Sky) – HDRI 24K](https://polyhaven.com/a/belfast_sunset_puresky)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Coucher de soleil doux et nuageux. Lumière dorée rasante pour les dunes de la mer de verre, avec des reflets spectaculaires sur le sable vitrifié. |
| [The Sky Is On Fire – HDRI 16K](https://polyhaven.com/a/the_sky_is_on_fire)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Crépuscule rouge-orangé au-dessus de l'océan. Ambiance dramatique pour la planète océan (secteur 3) ou pour le désert au bord du trou noir. |
| [Industrial Sunset (Pure Sky) – HDRI 16K](https://polyhaven.com/a/industrial_sunset_puresky)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Ciel de fin de journée voilé. Éclairage du monde industriel abandonné (secteur 5) et des zones de la Légion Mécanique. |
| [Wasteland Clouds (Pure Sky) – HDRI 16K](https://polyhaven.com/a/wasteland_clouds_puresky)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Ciel de terre désolée, nuages bas et lumière diffuse. Tempête approchante ou ciel chargé de poussière dans les secteurs désertiques. |
| [Dry Cracked Lake – HDRI](https://polyhaven.com/a/dry_cracked_lake)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Lac asséché sous un soleil dur. Éclairage de plein jour aride et contrasté pour le secteur 1 ; le sol sert aussi de référence pour les zones craquelées. |
| [Quarry Cloudy – HDRI](https://polyhaven.com/a/quarry_cloudy)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Ciel d'orage lourd. Lumière plate et menaçante pour la planète océan avant l'arrivée des vagues géantes. |
| [Kloofendal Misty Morning (Pure Sky) – HDRI 24K](https://polyhaven.com/a/kloofendal_misty_morning_puresky)<br><sub>Poly Haven</sub> | CC0 | Gratuit | Matin brumeux et couvert. Ambiance mystérieuse de la vallée des tombeaux et des monolithes (Chœur du Tombeau), à combiner avec le Volumetric Fog HDRP. |
| [Night Sky HDRI 008 (espace profond, 16K)](https://ambientcg.com/view?id=NightSkyHDRI008)<br><sub>ambientCG</sub> | CC0 | Gratuit | Ciel spatial sans sol, jusqu'en 16K. Skybox d'orbite pour les scènes du vaisseau-mère, des épaves et du largage. |
| [Night Sky HDRI 009 (espace sci-fi, 16K)](https://ambientcg.com/view?id=NightSkyHDRI009)<br><sub>ambientCG</sub> | CC0 | Gratuit | Champ d'étoiles au style science-fiction. Variante de skybox pour le Cœur de l'écho ou les transitions entre secteurs. |
| [Night Sky HDRI 002 (Voie lactée, 16K)](https://ambientcg.com/view?id=NightSkyHDRI002)<br><sub>ambientCG</sub> | CC0 | Gratuit | Voie lactée en HDRI CC0, en complément des cartes NASA, pour varier les nuits selon les secteurs. |
| [Evening Sky HDRI 030 A (crépuscule d'orage, 16K)](https://ambientcg.com/view?id=EveningSkyHDRI030A)<br><sub>ambientCG</sub> | CC0 | Gratuit | Coucher de soleil orageux et pluvieux. Éclairage de tempête sur la planète océan (secteur 3). |
| [Day Sky HDRI 071 A (ciel brumeux, 16K)](https://ambientcg.com/view?id=DaySkyHDRI071A)<br><sub>ambientCG</sub> | CC0 | Gratuit | Ciel de jour voilé, avec soleil diffus. Base pour un ciel chargé de particules en suspension (secteurs 1 et 2), à teinter vers l'ocre. |

<a id="effets"></a>

## Effets, sons, musique

Aucun pack VFX Graph HDRP sci-fi gratuit et de bonne qualité n'a été trouvé : le plus simple est de partir des exemples officiels de Unity, ou des petits packs payants d'Eric VFX Studio (3 à 7 $).

### VFX (particules)

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Particle Pack (Unity Technologies)](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325) | Standard Unity Asset Store EULA | Gratuit | Feu, fumée, étincelles, explosions, poussière. Shuriken (pas VFX Graph), marche en HDRP après conversion des matériaux. |
| [VisualEffectGraph-Samples (Unity Technologies)](https://github.com/Unity-Technologies/VisualEffectGraph-Samples) | Unity Companion License | Gratuit | Exemples officiels VFX Graph HDRP : tempêtes, champs de particules, effets GPU. Base pour la poussière d'Éther et les bulles temporelles. |
| [VFX Graph : Sci-fi Particles Char and BG (Eric VFX Studio)](https://wangray0110.itch.io/unity-vfx-graphsci-fi-particles-char-and-bg) | Licence du vendeur itch.io | 4 $ | Particules sci-fi en VFX Graph : aura de personnage, fond animé. |
| [VFX Graph : Galaxy and Meteor (Eric VFX Studio)](https://wangray0110.itch.io/unity-vfx-graphgalaxy-and-meteor) | Licence du vendeur itch.io | 4 $ | Galaxie et pluie de météores : ciel du secteur du trou noir. |
| [VFX Graph : Particles Gather, Deform, Explode (Eric VFX Studio)](https://wangray0110.itch.io/unity-vfx-graph-particles-gather-deform-explode) | Licence du vendeur itch.io | 3 $ | Particules qui se rassemblent puis explosent : récolte des cristaux d'Éther, activation du monolithe. |
| [VFX Graph : Mesh Particle Fragments (Eric VFX Studio)](https://wangray0110.itch.io/unity-vfx-graphmesh-particle-fragments) | Licence du vendeur itch.io | 7 $ | Objet qui se désagrège en fragments : distorsions temporelles, désintégration. |

### Sons

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Sci-fi Sounds (Kenney)](https://kenney.nl/assets/sci-fi-sounds) | CC0 | Gratuit | Lasers, moteurs, propulseurs, sons d'ordinateur : navette, scanner. |
| [Impact Sounds (Kenney)](https://kenney.nl/assets/impact-sounds) | CC0 | Gratuit | Pas, chocs sur métal, roche, verre : déplacements de l'Arpenteur. |
| [Interface Sounds (Kenney)](https://kenney.nl/assets/interface-sounds) | CC0 | Gratuit | Clics, confirmations, alertes : menus et HUD. |
| [Free Sound Effects Pack (Unity Asset Store)](https://assetstore.unity.com/packages/audio/sound-fx/free-sound-effects-pack-155776) | Standard Unity Asset Store EULA | Gratuit | Pack de bruitages variés, importable directement dans Unity. |
| [Sonniss GameAudioGDC Bundle](https://sonniss.com/gameaudiogdc) | Licence Sonniss : libre de droits, usage commercial, sans crédit ; entraînement d'IA interdit | Gratuit | Plusieurs dizaines de Go de sons pro (vent, sable, machines, créatures) offerts chaque année au GDC (7,47 Go en 2026). Le meilleur rapport qualité/prix pour un jeu réaliste. |
| [Freesound (filtre CC0)](https://freesound.org/search/?f=license%3A%22Creative+Commons+0%22) | CC0 (garder ce filtre ; les autres licences du site varient) | Gratuit | Énorme banque de sons enregistrés : désert, vent, grondements, ambiances. Compte gratuit pour télécharger. |
| [Pixabay Sound Effects](https://pixabay.com/sound-effects/) | Licence Pixabay (usage commercial, sans crédit) | Gratuit | Bruitages et ambiances. Licence non vérifiée à nouveau ici (site bloqué depuis le serveur) : relire la page avant usage. |
| [Chandra : sonifications de l'espace (NASA)](https://chandra.si.edu/sound/) | Domaine public NASA (crédit demandé) | Gratuit | Données de télescope converties en son (trou noir de Persée, centre galactique) : ambiance du trou noir. |
| [NASA Audio and Ringtones](https://www.nasa.gov/audio-and-ringtones/) | Domaine public NASA (pas d'usage du logo) | Gratuit | Vraies voix de mission, décollages, sons de sondes : radio de la navette, propagande. |

### Musique

| Asset | Licence | Prix | Usage dans le jeu |
|---|---|---|---|
| [Music Jingles (Kenney)](https://kenney.nl/assets/music-jingles) | CC0 | Gratuit | Courts jingles : victoire, échec, découverte. |
| [Incompetech (Kevin MacLeod)](https://incompetech.com/music/royalty-free/music.html) | CC BY 4.0 (créditer Kevin MacLeod) | Gratuit | Musiques d'ambiance et de tension, triables par style. |
| [Pixabay Music](https://pixabay.com/music/) | Licence Pixabay (usage commercial, sans crédit) | Gratuit | Musiques ambient et cinématiques. Licence non vérifiée à nouveau ici : relire la page avant usage. |
| [OpenGameArt](https://opengameart.org/) | Variable : prendre seulement CC0, CC BY ou CC BY-SA | Gratuit | Musiques et sons de jeu ; filtrer par licence sur chaque fiche. |

## Crédits obligatoires

À recopier dans l'écran des crédits pour chaque asset **CC BY / CC BY-SA** réellement utilisé dans le jeu (format : titre, auteur, licence, lien).

<details><summary>Terrain : sol, roche, sable (16)</summary>

- « Desert Cliffs Terrain Scan (Pers Scans) » par Pers Scans (sketchfab.com/perz_scans), CC BY 4.0, https://sketchfab.com/3d-models/desert-cliffs-terrain-scan-561a1628d0b446c7bd3424da48fe1150
- « LiDAR Rock Scan, Ramon Crater, Negev » par Doron Altaratz (sketchfab.com/doron_altaratz), CC BY 4.0, https://sketchfab.com/3d-models/lidar-rock-scan-ramon-crater-negev-israel-f0e424595ce14df9b82c35b658beb882
- « Cliff Rock Boulder Field (Pers Scans) » par Pers Scans (sketchfab.com/perz_scans), CC BY 4.0, https://sketchfab.com/3d-models/cliff-rock-boulder-field-d1f2ce6f71aa4c19adbdc541cc888194
- « Rock Pile Scan Dry 1 (Pers Scans) » par Pers Scans (sketchfab.com/perz_scans), CC BY 4.0, https://sketchfab.com/3d-models/rock-pile-scan-dry-1-5206d4d96c2d428b9c1f7ee0e13bcffb
- « Rocks Set Stone Collection Scan (Pers Scans) » par Pers Scans (sketchfab.com/perz_scans), CC BY 4.0, https://sketchfab.com/3d-models/rocks-set-stone-collection-scan-b45a40fd145c4b9ba7bc5351d344db17
- « Coastal Ground Cliff Rocks (Pers Scans) » par Pers Scans (sketchfab.com/perz_scans), CC BY 4.0, https://sketchfab.com/3d-models/coastal-ground-cliff-rocks-68d53a1623aa47008fb7088a5694d79b
- « Rocks Marine – Corsica (AliceVision) » par AliceVision, CC BY 4.0, https://sketchfab.com/3d-models/rocks-marine-corsica-416a548a159f42e89a475d0ac17ac6d9
- « Sea cliff rock face (Crète) » par axonite, CC BY 4.0, https://sketchfab.com/3d-models/sea-cliff-rock-face-e4bf57abe35f4df3b6f4c0f32507091f
- « Rock_Salines_2 (Martinique) » par Maxim.O, CC BY 4.0, https://sketchfab.com/3d-models/rock-salines-2-f9c05a8914a04b6aa3417d4a8db17c85
- « Wall of ice » par Lassi Kaukonen (sketchfab.com/thesidekick), CC BY 4.0, https://sketchfab.com/3d-models/wall-of-ice-17b6a81191f045a1af2d9bb57cad428b
- « Sand Dunes (Helindu) » par Helindu, CC BY 4.0, https://sketchfab.com/3d-models/sand-dunes-6bd9a0d96ef4463a8efd45af2be595ac
- « Sand cliffs (dune de Long Point) » par outsider, CC BY 4.0, https://sketchfab.com/3d-models/sand-cliffs-1c5f772e12fc44ff995776e38e1953f2
- « Stránská rock 1 » par 3dhdscan, CC BY 4.0, https://sketchfab.com/3d-models/stranska-rock-1-4118802ad14243d59f34c2be3fa77cd4
- « Galet éolisé – ventifact (ENS Géologie) » par Ecole Nationale Supérieure de Géologie (sketchfab.com/ENSGeologie), CC BY 4.0, https://sketchfab.com/3d-models/galet-eolise-ventifact-7f86615ce94e4ac09d31f191e746534a
- « Asteroids Pack (rocky version) » par SebastianSosnowski, CC BY 4.0, https://sketchfab.com/3d-models/asteroids-pack-rocky-version-adde1ecf129e4509be8af61b84bafa85
- « Alaskan Cliff Rock 9 Free » par Titaniumwhitefx, CC BY 4.0, https://sketchfab.com/3d-models/alaskan-cliff-rock-9-free-39e29fec9cf040dd925b804f5040ca04

</details>

<details><summary>Personnages, créatures, animations (54)</summary>

- « Spacesuit (Confederation of Planets), game-ready (tnnv) » par tnnv, CC BY 4.0, https://sketchfab.com/3d-models/spacesuit-confederation-of-planets-da7b5c14bc634f8283e2d21343afa7cc
- « Mercenary Astronaut (better_peter) » par Peter_D (sketchfab.com/better_peter), CC BY 4.0, https://sketchfab.com/3d-models/mercenary-astronaut-7fd399d488ae4576a0fc62715892956c
- « Animated Astronaut Character in Space Suit Loop (LasquetiSpice) » par LasquetiSpice, CC BY 4.0, https://sketchfab.com/3d-models/animated-astronaut-character-in-space-suit-loop-8fe5c8d3365e4d87bb7bc253d53a64e1
- « Space Engineer F Rigged, combinaison de protection féminine (tegnemaskin) » par tegnemaskin, CC BY 4.0, https://sketchfab.com/3d-models/space-engineer-f-rigged-98282a36702941d3941941def844bd68
- « Rigged for UE4/UE5 – Exoskeleton – free (Andywoodhead) » par Andy Woodhead (sketchfab.com/Andywoodhead), CC BY 4.0, https://sketchfab.com/3d-models/rigged-for-ue4ue5-exoskeleton-free-c0efb57e2fa940179ebaa58dcb9d0fa7
- « Mech Pilot UN., pilote en exo-combinaison (gueriero) » par gueriero, CC BY 4.0, https://sketchfab.com/3d-models/mech-pilot-un-722b12227c634489a3436a18a07064d4
- « Space suit inspiré d'Apollo (IgorSholopov) » par IgorSholopov, CC BY 4.0, https://sketchfab.com/3d-models/space-suit-bdc42075279746dab4c8b54784276a20
- « Hazmat Suit, combinaison NRBC Marvelous Designer (specter) » par Lukas Hahn 3D (sketchfab.com/specter), CC BY 4.0, https://sketchfab.com/3d-models/hazmat-suit-095694921e914ae889e91767ed680e4b
- « ASTRONAUT, combinaison EVA (Abrazafarolas) » par Abrazafarolas, CC BY 4.0, https://sketchfab.com/3d-models/astronaut-626da9f4336b4abc9f91ce71a7a1af42
- « Nathan Animated 003, homme scanné animé (Renderpeople) » par Renderpeople, CC BY 4.0, https://sketchfab.com/3d-models/nathan-animated-003-walking-3d-man-143a2b1ea5eb4385ae90a73657aca3bc
- « Eric Rigged 001, homme scanné riggé (Renderpeople) » par Renderpeople, CC BY 4.0, https://sketchfab.com/3d-models/eric-rigged-001-rigged-3d-business-man-a46bc9f67aaa415bb4f3241eef900e7f
- « Carla Rigged 001, femme scannée riggée (Renderpeople) » par Renderpeople, CC BY 4.0, https://sketchfab.com/3d-models/carla-rigged-001-rigged-3d-business-women-acf520f450d14dd799f98a6fede3edf5
- « Rigged T-Pose Human Male w 50 Face Blendshapes (mikealger) » par Mike Alger (sketchfab.com/mikealger), CC BY 4.0, https://sketchfab.com/3d-models/rigged-t-pose-human-male-w-50-face-blendshapes-cc7e4596bcd145208a6992c757854c07
- « 100STYLE, dataset mocap de 100 styles de locomotion (BVH) » par The 100STYLE Dataset – Ian Mason, CC BY 4.0, https://www.ianxmason.com/100style/
- « Gold Sandworm, ver animé game-ready (electricToy) » par Fubbi (sketchfab.com/electricToy), CC BY 4.0, https://sketchfab.com/3d-models/gold-sandworm-41f03702289940b29831dcb771fb5f3a
- « Game-Ready Queen ant Model, reine fourmi (LilCick) » par Msassasa (sketchfab.com/LilCick), CC BY 4.0, https://sketchfab.com/3d-models/game-ready-queen-ant-model-b04b636a35e14b38bfb05bd21931549d
- « Insectoid Monster Rig (SuperKapoo913) » par DM-913 (sketchfab.com/SuperKapoo913), CC BY 4.0, https://sketchfab.com/3d-models/insectoid-monster-rig-01323e4b2563430f9da85cd255b6e176
- « Animated Spider, araignée-loup animée (shadocks) » par Shadocks, CC BY 4.0, https://sketchfab.com/3d-models/animated-spider-af87017501fc44e39a33c220f2435100
- « Arahnid, Cockroach, blatte géante animée (stasbelyk13) » par stasbelyk13, CC BY 4.0, https://sketchfab.com/3d-models/arahnid-cockroach-d07ec0641ee74ece96a992db64ecff5f
- « Scorpion animé game-ready (anthonyvanoo) » par anthonyvanoo, CC BY 4.0, https://sketchfab.com/3d-models/scorpion-4c0fd62d33ab4d2592ee0a1d985a480d
- « Spiderthing take 3 (Rasmus82) » par Rasmus (sketchfab.com/Rasmus82), CC BY 4.0, https://sketchfab.com/3d-models/spiderthing-take-3-10bb4cf49d304d64afd2b829666f6caf
- « Spider monster Rigged, rig IK (Ploobert) » par Ploobert, CC BY 4.0, https://sketchfab.com/3d-models/spider-monster-rigged-ef71c4f221304a7ea9f01b691867f3d0
- « Phenebub, créature-ver lovecraftienne (Rysky) » par Jorma Rysky (sketchfab.com/Rysky), CC BY 4.0, https://sketchfab.com/3d-models/phenebub-free-horror-creature-e18dbcb6300b4421aac2bb51cc060aed
- « Sand Death Worm (facerate) » par eon (sketchfab.com/facerate), CC BY 4.0, https://sketchfab.com/3d-models/sand-death-worm-7a2b94b6adb242bbb12f116d40d3bcbe
- « Giant Isopod, scan RISD Nature Lab » par RISD Nature Lab (sketchfab.com/RISDNaturelab), CC BY 4.0, https://sketchfab.com/3d-models/giant-isopod-46469c568b3743ec8fb4bb711949d49a
- « Molting Husk Insectile Horror Creature (s8819296) » par Pigcraft (sketchfab.com/s8819296), CC BY 4.0, https://sketchfab.com/3d-models/molting-husk-insectile-horror-creature-0e666090f6fd45b280730db98f287e65
- « Sandworm horror creature (FredDrabble) » par Fred Drabble (sketchfab.com/FredDrabble), CC BY 4.0, https://sketchfab.com/3d-models/sandworm-horror-creature-7bf94fbb7e4343c99923c91897beea40
- « Medium Mech Striker, mech game-ready, 41 animations (MSGDI) » par MSGDI, CC BY 4.0, https://sketchfab.com/3d-models/medium-mech-striker-27ba717c173a40b7841d2f2c6a89d823
- « Mech Drone (skudgee) » par Willy Decarpentrie (sketchfab.com/skudgee), CC BY 4.0, https://sketchfab.com/3d-models/mech-drone-8d06874aac5246c59edb4adbe3606e0e
- « Biped robot (skudgee) » par Willy Decarpentrie (sketchfab.com/skudgee), CC BY 4.0, https://sketchfab.com/3d-models/biped-robot-801d2a245e4a4405a0c2152b35b5e486
- « mech M-6k (comrade1280) » par Comrade1280, CC BY 4.0, https://sketchfab.com/3d-models/mech-m-6k-6eba9b1addb7411a85b0c86e41a39a2e
- « mech TL-22 (comrade1280) » par Comrade1280, CC BY 4.0, https://sketchfab.com/3d-models/mech-tl-22-2694c68765aa4f67ab81a259ff39d654
- « W9231 Combat Mech (Rigged) (anusher3D) » par anusher3D, CC BY 4.0, https://sketchfab.com/3d-models/w9231-combat-mech-rigged-2388257bc61049bf841ea4094af7faa7
- « AR-22 Armadillo (Heavy Battle Mech) (n1k77) » par N1K (sketchfab.com/n1k77), CC BY 4.0, https://sketchfab.com/3d-models/ar-22-armadillo-heavy-battle-mech-6355e3510a544ce4bb219b0b5832933c
- « Combat Robot (ilushandro) » par ilushandro, CC BY 4.0, https://sketchfab.com/3d-models/combat-robot-55fe65bbb4074979ac0cb44850df4ddc
- « Robot M.A.N.U.L., riggé low poly (milinam2002) » par Andrei Milin (sketchfab.com/milinam2002), CC BY 4.0, https://sketchfab.com/3d-models/robot-manul-9e95bc80bfa643d8b890cfa4f83e8b3d
- « Robot Police Unit (animated) (jungle_jim) » par Jungle Jim (sketchfab.com/jungle_jim), CC BY 4.0, https://sketchfab.com/3d-models/robot-police-unit-animated-36bc284bf5b84a6597db8f6db053c095
- « Light Assault Mech (Lagst) » par Lagst, CC BY 4.0, https://sketchfab.com/3d-models/light-assault-mech-d1ca5065b5184646bc7028597d0ce049
- « Buster Drone (lavadragon) » par LaVADraGoN, CC BY 4.0, https://sketchfab.com/3d-models/buster-drone-294e79652f494130ad2ab00a13fdbafd
- « Scifi Drone 1.1, 5 animations (Jj.Mmhh) » par Javier Martín Hidalgo (sketchfab.com/Jj.Mmhh), CC BY 4.0, https://sketchfab.com/3d-models/scifi-drone-11-dd21953e81a24bd385d16eaf0c6a8ae7
- « XRK-M Autonomous Combat Drone (TacticalGlaceon) » par moog! (sketchfab.com/TacticalGlaceon), CC BY 4.0, https://sketchfab.com/3d-models/xrk-m-autonomous-combat-drone-2153909c16b242c293b5c296ce142799
- « Sci-fi Drone « Scorpion-42 » (JazOone) » par JazOone, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-drone-ac4d06d3a46d4d9cbc5beb33b896cde0
- « Attack Drone Scifi (badge3dart) » par Badge (sketchfab.com/badge3dart), CC BY 4.0, https://sketchfab.com/3d-models/attack-drone-scifi-561c10fbfea847c09bbac6910a90f80a
- « Futuristic Surveillance Drone (glowbox3d) » par Glowbox 3D (sketchfab.com/glowbox3d), CC BY 4.0, https://sketchfab.com/3d-models/futuristic-surveillance-drone-148d8e0ca26c4c5d81375496f4266685
- « Mechanical Scorpion (sinnik) » par SINNIK, CC BY 4.0, https://sketchfab.com/3d-models/mechanical-scorpion-098fe7f9592248a0be5b0d6d56cab056
- « Mechanical wasp (Low poly) (vet.doc) » par Yulia_vet.doc (sketchfab.com/vet.doc), CC BY 4.0, https://sketchfab.com/3d-models/mechanical-wasp-low-poly-91b15801303b49af8a07f8b34218add4
- « Sci-Fi Humanoid Robot (yur1_val) » par yur1_val, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-humanoid-robot-12ad786ecde246e5854a61fd4f67ed49
- « Destructible Robot – Highpoly (slava) » par Slava Z. (sketchfab.com/slava), CC BY 4.0, https://sketchfab.com/3d-models/destructible-robot-highpoly-0f9c08f0d8d94f3a9938d4d8bf423f46
- « T200 Dieselpunk Mech – Freebie (valentinwinkelmann) » par Valentin Winkelmann (sketchfab.com/valentinwinkelmann), CC BY 4.0, https://sketchfab.com/3d-models/t200-dieselpunk-mech-freebie-a439a90d35274f94888f46a138f30615
- « Dark Faceless cosmic Phantom (s8819296) » par Pigcraft (sketchfab.com/s8819296), CC BY 4.0, https://sketchfab.com/3d-models/dark-faceless-cosmic-phantom-4f16d9ace2904aafb51757d72c017308
- « Wraith Ghost Warrior in Torn Ancient Armor (s8819296) » par Pigcraft (sketchfab.com/s8819296), CC BY 4.0, https://sketchfab.com/3d-models/wraith-ghost-warrior-in-torn-ancient-armor-3b4ee6c3131a431db6a67021c6d863c8
- « Cloth Ghost, fantôme en tissu (andrey.sk) » par ANDRE (sketchfab.com/andrey.sk), CC BY 4.0, https://sketchfab.com/3d-models/cloth-ghost-f4fc1cd448c04a809d106975cfa7011b
- « Tentacle (rigged) (CGDanielGlebinski) » par CG Daniel Glebinski (sketchfab.com/CGDanielGlebinski), CC BY 4.0, https://sketchfab.com/3d-models/tentacle-rigged-8fcc783af94246a0b8febf424a4b96b9
- « Ghostly Reaper, spectre à faux low poly (kalebiscoolio78) » par SkellyCooks (sketchfab.com/kalebiscoolio78), CC BY 4.0, https://sketchfab.com/3d-models/ghostly-reaper-d5b4e79acd684d52a1f3303edf22039b

</details>

<details><summary>Vaisseaux, véhicules, matériaux de coque (40)</summary>

- « [MMSEV] Space Exploration Vehicle (SEV), version texturée PBR 4K » par Marco_Romero (sketchfab.com/MarcoRomero), CC BY 4.0, https://sketchfab.com/3d-models/mmsev-space-exploration-vehicle-sev-d2f3b528edec4e2e8b938019d68632d0
- « Lunokhod 1 » par sheffrator, CC BY 4.0, https://sketchfab.com/3d-models/lunokhod-1-3f5a5f02ad53447b9dff335e42d2469c
- « H500 - Explorer Truck » par Render Blue (sketchfab.com/pablojcd), CC BY 4.0, https://sketchfab.com/3d-models/h500-explorer-truck-89ea8e69b87942a881f9b89fd04ec00f
- « Lunar Rover (concept rétro-futuriste) » par bigbadcat, CC BY 4.0, https://sketchfab.com/3d-models/lunar-rover-b2503f437acd48ce8cbd05ce7d9a8c2c
- « Sci-Fi Buggy » par TiyaMakes, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-buggy-085bd97876a64eacbc041299ac945e9c
- « Military Landvehicle Kit 1.2 (kitbash) » par britdawgmasterfunk, CC BY 4.0, https://sketchfab.com/3d-models/military-landvehicle-kit-12-cc0-91f495435f624d8b97a768f692aa6ce9
- « Mars2020 parachute and backshell crashsite (photogrammétrie réelle) » par Simeon Schmauß (sketchfab.com/semeion), CC BY-SA 4.0, https://sketchfab.com/3d-models/mars2020-parachute-and-backshell-crashsite-008a288caa504703a2d853fb56a2569f
- « Ancient Colony Ship » par Vaportrash, CC BY 4.0, https://sketchfab.com/3d-models/ancient-colony-ship-956d09cb2e53483f90c24c72ab69bb0d
- « Crashed Spaceship (scène d'épave) » par osmosikum, CC BY 4.0, https://sketchfab.com/3d-models/crashed-spaceship-f2696bbc764e45a3a22d18e4150635a4
- « Drop Ship or light attack, mars defense force » par marsmotion, CC BY 4.0, https://sketchfab.com/3d-models/drop-ship-or-light-attack-mars-defense-force-0b169f4ebcac4eb287002f129d55bfff
- « Transporter Spaceship (transport militaire high-poly) » par pirate8888, CC BY 4.0, https://sketchfab.com/3d-models/transporter-spaceship-fe581b60c8ea4ea591a24a483f25ddad
- « Sci-fi Dropship » par Pascal T. Monette (sketchfab.com/baltazo), CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-dropship-709a1916c0cc4cbebe197e7ef26964cb
- « D.S.S. Harbinger battle cruiser » par Comrade1280, CC BY 4.0, https://sketchfab.com/3d-models/dss-harbinger-battle-cruiser-474f62d00ed54212b37f93ce91569c53
- « (12) ORYX Very Heavy Freighter (textured) » par Binkley-Spacetrucker (sketchfab.com/Binkley-Piratepants), CC BY 4.0, https://sketchfab.com/3d-models/12-oryx-very-heavy-freighter-textured-de824e4d997640c09bc4ccbce3064fbb
- « Corinth Cargo Transporter » par Yvo Pors (sketchfab.com/yvopors14), CC BY 4.0, https://sketchfab.com/3d-models/corinth-cargo-transporter-7614d85856f6477ba2a603970dcbb0dd
- « Mining Company Space Ships (collection) » par iedalton, CC BY 4.0, https://sketchfab.com/3d-models/mining-company-space-ships-bbfac99760fb49ceb1c35cdb4ed5f883
- « Hammerhead Space Fighter (canonnière) » par Pedro B. Goulart (sketchfab.com/Pebegou), CC BY 4.0, https://sketchfab.com/3d-models/hammerhead-space-fighter-19daeb543508430c8807edbcf27a9741
- « Space Fighter » par Comrade1280, CC BY 4.0, https://sketchfab.com/3d-models/space-fighter-e766136d4871441289d37d44a4bbcd3b
- « Spacefighter Cockpit (Wasp Interdictor) » par Comrade1280, CC BY 4.0, https://sketchfab.com/3d-models/spacefighter-cockpit-wasp-interdictor-db4aa67fe1164ec088083d7dac2d273f
- « Sci-Fi Hanger (hangar) » par dylanheyes, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-hanger-8f10be0e8b5644e1845a2a5f12af5d6c
- « Sci-Fi Spaceship Bridge » par dylanheyes, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-spaceship-bridge-aaa5259e54e14c79936df3d127644287
- « Spaceship Corridor » par the_table, CC BY 4.0, https://sketchfab.com/3d-models/spaceship-corridor-1cdd1db557b8428892af4773bdada913
- « Sci-fi Corridor (modulaire, abandonné) » par Eliu_Villanueva, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-corridor-f3d033a8f3ad4888acc8c0a16878cf85
- « Sci-Fi Personal Space Pod Ship » par futaba@blender (sketchfab.com/futaba_blender), CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-personal-space-pod-shipweekly-challenge-b158f01dd4a9416fb689ca4401856e7a
- « HAB - Mars Desert Research Station (scan lidar) » par The Mars Society (sketchfab.com/marssociety), CC BY 4.0, https://sketchfab.com/3d-models/hab-mars-desert-research-station-5c1f4c9726754652946098616b7614a1
- « Habitat Lander Pod » par jmleach1987, CC BY 4.0, https://sketchfab.com/3d-models/habitat-lander-pod-9a55af34f7654097bd72f0642d83ccce
- « Space Station Modules » par re1monsen, CC BY 4.0, https://sketchfab.com/3d-models/space-station-modules-e3ba39a1c78540448542cf937b11feab
- « Industrial Antenna » par Ricardo_PG (sketchfab.com/RickPG001), CC BY 4.0, https://sketchfab.com/3d-models/industrial-antenna-001970c2594e4aa6bef46e63fc9e0c5a
- « Satellite Comms (parabole militaire) » par Duane's Mind (sketchfab.com/duanesmind), CC BY 4.0, https://sketchfab.com/3d-models/satellite-comms-4be56951da39429a963ee12140bbff57
- « Radio Communications Tower (modulaire) » par Duane's Mind (sketchfab.com/duanesmind), CC BY 4.0, https://sketchfab.com/3d-models/radio-communications-tower-70a1996b614e4c7ba8f01d4295b8fff4
- « Rusted Antennas » par Blank2574, CC BY 4.0, https://sketchfab.com/3d-models/rusted-antennas-86303d7c8cb442c2bcd138abe2b01c9f
- « Old Scifi satellite dish » par CGKnuenz (sketchfab.com/Knuenzj), CC BY 4.0, https://sketchfab.com/3d-models/old-scifi-satellite-dish-f01a91e9773e4622b25c4d27903e85c6
- « HDR-O Radar CAT-19 » par BlackMike, CC BY 4.0, https://sketchfab.com/3d-models/hdr-o-radar-cat-19-30ed76a6b4e44cf4b429a6d764e97054
- « Sci-Fi Container » par EdwinCGStudio, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-container-96fb7ded0eb44cd18dd5c1e7bb3eb74f
- « Military Weapons Container » par MeshSlinger (sketchfab.com/thelastphysician), CC BY 4.0, https://sketchfab.com/3d-models/military-weapons-container-8a1c32f395f549b8b22265b8a459fa8e
- « Sci-fi cargo crate » par andreas9343, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-cargo-crate-ac36898521304e51a8e305aa602f1f5b
- « Futuristic cargo container Low-Poly » par Begle, CC BY 4.0, https://sketchfab.com/3d-models/futuristic-cargo-container-low-poly-015f32ec950744c4898e17af07d43f65
- « Sci-Fi Fuel Tank » par FRE3N, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-fuel-tank-3d13bb3bb7c149cda5bc0a14abedd120
- « SCIFI GENERATOR » par LordCinn, CC BY 4.0, https://sketchfab.com/3d-models/scifi-generator-bf2e1a087adb4628a376ffbed17c41ff
- « Solar Panel (sur pied, usé) » par rivetech, CC BY 4.0, https://sketchfab.com/3d-models/solar-panel-83483a66f8974d7e8f2e7bbde518d606

</details>

<details><summary>Architecture : bases, ruines, tombeaux (30)</summary>

- « Thermal power plant, Cooling tower (photogrammétrie par drone) » par Metazeon, CC BY 4.0, https://sketchfab.com/3d-models/thermal-power-plant-cooling-tower-ff3cdef5f8e24605b56648918e1f9a8a
- « Abandoned Building: Polygraphenwerk Leipzig (scan d'usine abandonnée) » par Geddart, CC BY 4.0, https://sketchfab.com/3d-models/abandoned-building-polygraphenwerk-leipzig-6fa85b289def499c95afd232173ecc27
- « Large Industrial Storage Tanks » par Duane's Mind (sketchfab.com/duanesmind), CC BY 4.0, https://sketchfab.com/3d-models/large-industrial-storage-tanks-bd3ac97071104b64bc388a155b64f79e
- « Industrial asset pack (Free) » par ForevereQ, CC BY 4.0, https://sketchfab.com/3d-models/industrial-asset-pack-free-94c5011772a84e8791779b342467f245
- « Old Pipes Bundle – Modular Industrial Pack » par Alex Krush (sketchfab.com/Alex_Krush), CC BY 4.0, https://sketchfab.com/3d-models/old-pipes-bundle-modular-industrial-pack-fbe3129ea77243ca92c8219f269ac5cf
- « Rusted Industrial Water Tank » par Arghanion's PuzzleBox (sketchfab.com/kain2008), CC BY 4.0, https://sketchfab.com/3d-models/rusted-industrial-water-tank-cfc37d0aff454f29a27c8ab3325f445b
- « Rusted factory machine » par Mark Bai (sketchfab.com/bcfbox), CC BY 4.0, https://sketchfab.com/3d-models/rusted-factory-machine-e1aa6b17ff2a4f7d9b7be67756e12848
- « Realistic Metal Scaffolding » par Jungle Jim (sketchfab.com/jungle_jim), CC BY 4.0, https://sketchfab.com/3d-models/realistic-metal-scaffolding-c1f03864ec72449c8968f0630c287c8d
- « Container Pack (13 conteneurs avec intérieurs) » par drcrazzie, CC BY 4.0, https://sketchfab.com/3d-models/container-pack-0d416a9bcfc14978aa50e80281dfe9a5
- « Construction Site Asset Pack » par vmatthew, CC BY 4.0, https://sketchfab.com/3d-models/construction-site-asset-pack-a6cccf5af6514f2f9289beeead38fe1b
- « Abandoned Old Soviet Factory Facade (scan) » par Abandoned Scans (sketchfab.com/realMrAnderson), CC BY 4.0, https://sketchfab.com/3d-models/abandoned-old-soviet-factory-facade-free-b3801b97dcda485c9148f361eeec0c60
- « Bunker O3/23/A-160 Z (casemate en béton, photogrammétrie) » par matousekfoto, CC BY 4.0, https://sketchfab.com/3d-models/bunker-o323a-160-z-11ae7599f3b640569f7ed4692acabf38
- « Piedras de Callanish | Callanish Stones (cercle de menhirs) » par Arqueomodel3D (sketchfab.com/juanbrualla), CC BY 4.0, https://sketchfab.com/3d-models/piedras-de-callanish-callanish-stones-7da7219ac3304850b2dab4133b9e183b
- « Temple Of Augustus Ruins (photogrammétrie) » par Abby Gancz (sketchfab.com/agancz), CC BY 4.0, https://sketchfab.com/3d-models/temple-of-augustus-ruins-a974316de2454609aa841790844e3571
- « St Pancras New Church Crypt (crypte voûtée, photogrammétrie) » par artfletch, CC BY 4.0, https://sketchfab.com/3d-models/st-pancras-new-church-crypt-85c68a9125f946af8f5102bb426e2f86
- « Unfinished building, remains, ruins (scan par drone) » par Metazeon, CC BY 4.0, https://sketchfab.com/3d-models/unfinished-building-remains-ruins-56210a9c376a41d882508bd15259eca6
- « Dirty stones pile (gravats, photogrammétrie) » par matousekfoto, CC BY 4.0, https://sketchfab.com/3d-models/dirty-stones-pile-893e64e62f414b439672a8770dd2869d
- « Monumental Runic Stone - Optimised, 20k » par Thomas Flynn (sketchfab.com/nebulousflynn), CC BY 4.0, https://sketchfab.com/3d-models/monumental-runic-stone-optimised-20k-d95a850cd9114828a18b5dd72878d9ec
- « Ancient Ruins Pack (murs et piliers modulaires) » par Hawtor Studio (sketchfab.com/zul_gv), CC BY 4.0, https://sketchfab.com/3d-models/ancient-ruins-pack-4dad3b80562842f88d6568c5e1c469c2
- « Stone Arch Pillars - Ruins (3 niveaux de destruction) » par Unreal Designer (sketchfab.com/unrealdesigner.ig), CC BY 4.0, https://sketchfab.com/3d-models/stone-arch-pillars-ruins-a1a8e633c9aa40c08b4d2a0631ac61d2
- « Abbey Wall Ruins » par tonyflanagan, CC BY 4.0, https://sketchfab.com/3d-models/abbey-wall-ruins-37936ba6a1674f15be60075315469a43
- « Pillar on the agora of antic city of Sagalassos (scan) » par Mikhail Volkov (sketchfab.com/mgv), CC BY 4.0, https://sketchfab.com/3d-models/pillar-on-the-agora-of-antic-city-of-sagalassos-d623bddcf6ba4fd4a4f6cb919160b6f0
- « Kalamos Cave (grotte marine, photogrammétrie) » par 9of9, CC BY 4.0, https://sketchfab.com/3d-models/kalamos-cave-edb597f00e3e4a84855210219502d29a
- « Penmaenmawr Cairn (tas de pierres rituel, scan) » par megalitharchive, CC BY 4.0, https://sketchfab.com/3d-models/penmaenmawr-cairn-9c1e3f0c3b0a49c8bfb032c6a8bf0014
- « Temple Ruins Asset Pack » par Bl4ckGh0st, CC BY 4.0, https://sketchfab.com/3d-models/temple-ruins-asset-pack-d1e25cdbe51e467a87baf94ead0afa63
- « Sci-fi Ship interior - Modular asset pack » par Rejala, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-ship-interior-modular-asset-pack-50e2af8800cc4ab79add375f817b2d76
- « Space Colony Modular Kit Bash » par R-LAB (sketchfab.com/rakshaan), CC BY 4.0, https://sketchfab.com/3d-models/space-colony-modular-kit-bash-1ac694729ad04977ba93dafa66914aff
- « Spaceship Cockpit + Seat (riggé) » par sparshgupta, CC BY 4.0, https://sketchfab.com/3d-models/spaceship-cockpit-seat-8fe765ad88bf4da4a32e3656dfbe9133
- « Free Airlock Door (porte de sas animée) » par michael_grodkowski, CC BY 4.0, https://sketchfab.com/3d-models/free-airlock-door-58729c7672ae4e01a6103617c2f4f55e
- « Sci Fi Hallway FREE » par Seesha, CC BY 4.0, https://sketchfab.com/3d-models/sci-fi-hallway-free-44fd25310d2240908027390a078368e1

</details>

<details><summary>Ciel et espace : HDRI, étoiles, planètes, trou noir (20)</summary>

- « Gaia's sky in colour (carte du ciel Gaia DR2) » par ESA/Gaia/DPAC, CC BY-SA 3.0 IGO, https://www.esa.int/ESA_Multimedia/Images/2018/04/Gaia_s_sky_in_colour2
- « The Milky Way panorama (ESO, 360°) » par ESO/S. Brunier, CC BY 4.0, https://www.eso.org/public/images/eso0932a/
- « VISTA gigapixel mosaic of the central Milky Way » par ESO/VVV Survey/D. Minniti, CC BY 4.0, https://www.eso.org/public/images/eso1242a/
- « HYG Stellar Database (Hipparcos, Yale, Gliese) » par Astronexus (HYG Database), CC BY-SA 4.0, https://codeberg.org/astronexus/hyg
- « NIRCam « Cosmic Cliffs » in Carina (James Webb) » par NASA, ESA, CSA, and STScI, CC BY 4.0, https://esawebb.org/images/weic2205a/
- « Pillars of Creation – vue visible (Hubble 2015) » par NASA, ESA/Hubble and the Hubble Heritage Team, CC BY 4.0, https://esahubble.org/images/heic1501a/
- « Carina Nebula « Star birth in the extreme » (mosaïque Hubble) » par NASA, ESA, N. Smith, CC BY 4.0, https://esahubble.org/images/heic0707a/
- « Hubble Ultra Deep Field (« Hubble sees galaxies galore ») » par NASA, ESA, S. Beckwith, CC BY 4.0, https://esahubble.org/images/heic0406a/
- « Milky Way Skybox HDRI panorama » par Aliaksandr.melas (sketchfab.com/alexandr.melas), CC BY 4.0, https://sketchfab.com/3d-models/milky-way-skybox-hdri-panorama-b57711d6a450410ca612c4a36f08ce21
- « Space nebula HDRI panorama 360 skydome » par Aliaksandr.melas (sketchfab.com/alexandr.melas), CC BY 4.0, https://sketchfab.com/3d-models/space-nebula-hdri-panorama-360-skydome-f7a5798b5b374576879973272d27300e
- « First Image of a Black Hole – M87* (EHT) » par EHT Collaboration, CC BY 4.0, https://www.eso.org/public/images/eso1907a/
- « First image of our black hole – Sagittarius A* (EHT) » par EHT Collaboration, CC BY 4.0, https://www.eso.org/public/images/eso2208-eht-mwa/
- « Black Hole (modèle 3D, Nestaeric) » par Nestaeric, CC BY 4.0, https://sketchfab.com/3d-models/black-hole-e410da98b1e5445eae2acafaaa53587d
- « Black Hole – trou noir stellaire avec disque (SebastianSosnowski) » par SebastianSosnowski, CC BY 4.0, https://sketchfab.com/3d-models/black-hole-cfd16738ad2c402b9dc8e38a9c05c8d4
- « Solar System Scope – Textures (planètes, lunes, étoiles, jusqu'à 8K) » par Solar System Scope, CC BY 4.0, https://www.solarsystemscope.com/textures/
- « Ringed World (planète à anneaux) » par Tycho Magnetic Anomaly (sketchfab.com/Tycho_Magnetic_Anomaly), CC BY 4.0, https://sketchfab.com/3d-models/ringed-world-6fdfb3c84fb348188d6968092be1b1b5
- « Asteroids Pack (metallic version) » par SebastianSosnowski, CC BY 4.0, https://sketchfab.com/3d-models/asteroids-pack-metallic-version-eff495d9315c47dbb2777ec80bef40d8
- « Moon – Petavius crater (terrain 3D) » par SebastianSosnowski, CC BY 4.0, https://sketchfab.com/3d-models/moon-petavius-crater-ce9c009b517b421eab8c8429b536382f
- « Mars – Olympus Mons (terrain 3D) » par SebastianSosnowski, CC BY 4.0, https://sketchfab.com/3d-models/mars-olympus-mons-53679864b41a43b0ae0459ce99d6adf0
- « Mars landscape (photogrammétrie Perseverance) » par David Vacas Madrid (sketchfab.com/vacasmadrid), CC BY 4.0, https://sketchfab.com/3d-models/mars-landscape-83f6f32b0a694f939cd16bf04f5565ef

</details>

- Musique « … » par Kevin MacLeod (incompetech.com), CC BY 4.0, si utilisée.

## Écartés après vérification

- **Giant's Causeway Wall – orgues basaltiques (Sircher)** : textures reprises d'une vidéo YouTube sous licence standard, la CC BY n'est pas fiable.
- **Lava Flow – PBR 3D Scan (Hawaï, Sircher)** : même problème (vidéo YouTube sous licence standard) ; pour la lave, prendre Lava004 d'ambientCG (CC0).
- **Fulgurite 2 (Extinktor)** : en réalité une figurine Warhammer 40k (franchise protégée).
- **Sci-Fi Spider Robot (yur1_val)** : copie de R2-D2 (Star Wars) et modèle repris ailleurs sans crédit.
- **Mantis (Leyde)** : femme-mante stylisée, pas une mante réaliste.
- **Crawling robotic centipede (OuterspaceSoftware)** : démo promotionnelle (texte « Your Title » et logos dans la scène).
- **CMU Graphics Lab Motion Capture Database** : doublon de la Huge FBX Mocap Library, déjà gardée.

## Sources

Pages consultées pour les informations qui n'ont pas pu être vérifiées directement (sites bloqués depuis le serveur) :

- Sonniss GDC 2026 (taille, licence) : https://rekkerd.org/sonniss-releases-gdc-2026-game-audio-bundle/ et https://gamefromscratch.com/sonniss-27-5gb-sound-effect-giveaway-at-gdc-2024/
- Licences Freesound et Pixabay : https://app.cinevva.com/guides/free-sound-effects-music
- Prix Eric VFX Studio : https://wangray0110.itch.io/unity-vfx-graphsci-fi-particles-char-and-bg
