# Les prompts Meshy du prologue

> Version 1, 29 octobre 2026. À copier dans Meshy ([meshy.ai](https://www.meshy.ai)), en **Text to 3D**. Chaque prompt suit la [bible](../BIBLE-DA.md) :
> - l'Empire est en acier sombre, avec une seule touche de rouge ;
> - les détenus sont en orange ;
> - les pirates sont chauds et faits de pièces de récupération ;
> - les Bâtisseurs sont en pierre noire, avec des glyphes cyan ;
> - aucun nom d'œuvre ni de marque.

## 1. Les réglages

| Réglage | Valeur |
|---|---|
| Style (*Art style*) | **Realistic** |
| Personnages | Pose **T** ou **A**, si Meshy propose le choix : c'est ce qui permet de leur donner un squelette ensuite |
| Symétrie | **Auto** (ou *On* pour les personnages et les portes) |
| Polygones (*Target polycount*) | Personnages 30 000 à 50 000 ; objets 10 000 à 20 000 ; vaisseau 40 000 |
| Version du modèle | Une version que **l'offre gratuite permet de télécharger** (pas Meshy 6 ni 7 en gratuit, voir [`IA-3D.md`](../../IA-3D.md)) |
| Téléchargement | **GLB** |

**Si un champ « Negative prompt » existe**, y mettre :
```
cartoon, stylized, toy, low detail, text, logo, letters, watermark, multiple objects, base, pedestal, ground plane, background scene, blurry
```

**La méthode** :
1. Lancer le prompt.
2. Garder la meilleure des variantes et la texturer (*Texture* ou *Refine*).
3. Télécharger le GLB.

Si le résultat est flou ou mal construit, passer par une image : générer d'abord l'image de concept (*Text to Image*, avec le même prompt), puis **Image to 3D**. C'est souvent bien meilleur pour les personnages.

**La licence** : en gratuit, les modèles Meshy sont en CC BY 4.0 et publics. Il faut ajouter aux crédits du jeu : « Modèles 3D générés avec Meshy (meshy.ai), CC BY 4.0 ».

## 2. Les personnages

Après Meshy, chaque personnage passe par la même chaîne :
1. `normaliser-prologue.py` ;
2. `rigger-statues.py`, ou le squelette automatique de Meshy s'il est proposé ;
3. il reçoit alors les 84 animations de la bibliothèque.

**1. L'androïde de l'Empire** (le garde de base) → `03-Gardes/androide-empire`
```
Full body humanoid security android standing in a T-pose, faceless head with one narrow horizontal red sensor slit, broad shoulders, visible mechanical joints at the neck, elbows and knees, dark gunmetal angular armor plates with chamfered edges, one thin red stripe on the chest, worn scratched metal on the lower legs, clean upper body, no weapon, realistic, high detail, game character
```

**2. Le pirate d'abordage** (remplace le pirate « Scavenger », cassé) → `04-Pirates/pirate-abordage`
```
Full body human space pirate standing in a T-pose, scavenged mismatched armor plates welded together, asymmetric shoulder pad on one side, rust orange and worn brown leather, utility belts and pouches, cables hanging from the backpack, scratched breathing mask hanging on the chest, rugged face with stubble, no weapon in hands, realistic, high detail, game character
```

**3. Le chef pirate**, en version animable (un manteau court, qui ne gêne pas les jambes) → `04-Pirates/chef-pirate`
```
Full body human pirate captain standing in a T-pose, short heavy jacket ending at the hips made of patched dark leather and salvaged armor plates, one mechanical prosthetic forearm, braided beard, scarred face, warm rust and dark brown colors, belts with tools and trophies, no long coat, no cape, no weapon in hands, realistic, high detail, game character
```

**4. Le détenu de corvée** (remplace la combinaison étanche, cassée) → `02-Prisonniers/detenu-corvee`
```
Full body prisoner in a worn orange hazmat work suit standing in a T-pose, rubber gloves and boots, gas mask with two round filters, stained and patched suit, number stenciled on the chest without letters, realistic, high detail, game character
```

**5. Le vieux détenu**, habillé (remplace le vieux torse nu) → `02-Prisonniers/detenu-vieux-habille`
```
Full body old thin prisoner standing in a T-pose, worn faded orange jumpsuit with rolled sleeves, shaved grey head, tired wrinkled face, grey stubble, old work boots, realistic, high detail, game character
```

**6. La combinaison du Veilleur** (le héros, après la chambre des reliques) → `01-Joueur/combinaison-veilleur-meshy`
```
Full body sleek ancient armored suit standing in a T-pose, matte black smooth material like polished stone, slender elongated proportions, thin glowing cyan line patterns like engraved glyphs on the chest, arms and legs, closed smooth featureless helmet, pristine without rust or scratches, realistic, high detail, game character
```

**7. Un détenu d'une autre espèce** (pour varier la foule) → `16-Especes/detenu-reptilien`
```
Full body reptilian alien prisoner standing in a T-pose, grey-green scaled skin, long narrow head with small dark eyes, thin build, wearing a worn orange prison jumpsuit with rolled sleeves, bare clawed feet, realistic, high detail, game character
```

## 3. Les objets à remplacer (bible, § 12)

**8. La porte de cellule en acier** → `05-Cellules/porte-cellule-acier`
```
Heavy solid steel prison cell door, upright, small sliding hatch at eye level, thick chamfered frame, dark gunmetal painted steel, chipped paint, rust streaks at the bottom, single object, realistic, high detail
```

**9. Le lit de détenu** → `05-Cellules/lit-acier`
```
Steel prison bunk bed with two levels welded to a wall bracket, thin grey mattresses, folded grey wool blankets, worn dark painted steel frame, single object, realistic, high detail
```

**10. La console de l'Empire** (celle qu'on pirate) → `09-Informatique/console-empire`
```
Heavy industrial computer console on a pedestal, dark gunmetal casing with chamfered corners, small amber monochrome screen, rows of heavy toggle switches and buttons, one red warning light, worn paint and scratches on the edges, single object, realistic, high detail
```

**11. La capsule du Veilleur** → `15-Reliques/capsule-veilleur-ia`
```
Ancient sarcophagus pod made of polished black stone, elongated proportions, sharp thin edges, thin glowing cyan line glyphs engraved on the lid, pristine without rust, lying horizontally, single object, realistic, high detail
```

**12. Le vaisseau pirate** → `11-Vaisseaux/vaisseau-pirate-ia`
```
Salvaged pirate gunship spacecraft assembled from mismatched hull plates, rust orange and dark brown paint, welded patches, asymmetric silhouette with one larger engine, exposed pipes and cables, boarding tube under the nose, single object, realistic, high detail
```

**13. Les débris de coque** (à la place des gravats en briques) → `12-Exterieur/debris-coque`
```
Pile of twisted torn steel hull plates and broken metal beams, dark gunmetal paint, burnt and melted edges, a few loose cables, single object, realistic, high detail
```

**14. L'extincteur, sans texte** (remplace l'extincteur d'aujourd'hui) → `14-Accessoires/extincteur-empire`
```
Industrial fire suppression canister mounted on a wall bracket, dark gunmetal cylinder with one thick yellow band, chunky valve and short hose, no text, no labels, worn and scratched, single object, realistic, high detail
```

## 4. Des objets nouveaux pour la DA

**15. La cuve du spécimen** (pour le mutant biomécanique, au laboratoire) → `15-Reliques/cuve-specimen`
```
Tall cylindrical laboratory specimen tank, thick glass cylinder filled with murky green liquid, dark gunmetal base and top cap with chamfered edges, pipes and cables plugged into the base, small amber status screen, empty inside, single object, realistic, high detail
```

**16. La station de recharge des androïdes** (le long des coursives) → `03-Gardes/station-recharge`
```
Wall-mounted charging alcove for a humanoid robot, dark gunmetal frame with chamfered edges, two clamps at shoulder height, thick power cables, one red status light, worn floor plate, empty, single object, realistic, high detail
```

**17. La lampe de chantier au sodium** (la couche Chantier, sous 4 m) → `14-Accessoires/lampe-chantier`
```
Industrial caged work lamp with a warm orange sodium bulb, heavy metal wire cage, wall bracket, frayed power cable, dirty and worn, single object, realistic, high detail
```

**18. Le cristal d'Éther** → `15-Reliques/cristal-ether-meshy`
```
Raw crystal cluster made of black glassy stone with thin glowing cyan veins inside, sharp angular facets, pristine, single object, realistic, high detail
```

## 5. Après Meshy

1. **Ranger** le GLB sous `~/jeu/prologue/<dossier>/<nom>/<nom>.glb` (le chemin est indiqué sous chaque prompt).
2. **Envoyer le fichier** au Claude du cloud (ou le déposer dans le dépôt, sous `prologue-ia/`) : il l'ajoute au catalogue, à la maquette et au registre (`docs/REGISTRE-IA.md`).
3. **Les personnages** : `normaliser-prologue.py`, puis `rigger-statues.py` (ajouter sa clé dans la liste `STATUES`, en mode `souple`, ou `rigide` pour un robot fait de pièces). Ensuite, vérifier dans le banc des personnages (`vitrine/banc-pnj.html`) qu'il marche, attend et vise sans s'étirer.
4. **Les couleurs** : `recolorer-prologue.py`, si un réglage `da` lui est donné dans `docs/prologue/tailles-modeles.json`.
