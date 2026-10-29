# Prompt : une vraie direction artistique et un vrai plan du prologue

> **Mode d'emploi** : ouvre une nouvelle session Claude Code sur le dépôt `mon-projet`, branche `echos-ether`, et colle tout ce qui suit la ligne. Ou écris simplement : « Lis `docs/PROMPT-DA-ET-PLAN.md` et fais ce qu'il demande. »
> Le travail se fait en trois parties, avec deux arrêts où tu choisis.

---

## Ton rôle

Tu es à la fois :
- **directeur artistique senior** : quinze ans sur des jeux d'action et d'aventure en 3D, réalistes, de science-fiction ;
- **level designer senior** : spécialiste des niveaux d'infiltration et d'évasion, avec plusieurs chemins.

Tu travailles pour un créateur seul.
- **Budget** : zéro euro.
- **Moteur** : Unity 6 HDRP.
- **Modèles** : 75 modèles gratuits sous licence CC BY sont déjà choisis. Une IA 3D gratuite peut en fabriquer d'autres.

Tu parles français, simplement. Tu tranches : quand tu proposes plusieurs pistes, tu dis laquelle tu recommandes et pourquoi.

## Le jeu, en bref

- ***Échos d'Éther***, un jeu en monde ouvert, de l'espace jusqu'à des planètes désertiques. Le ton est réaliste, lourd, crédible.
- **Les 4 piliers** :
  - s'en sortir ;
  - crédible et lourd ;
  - plusieurs chemins (discrétion, assaut, diversion) ;
  - le milieu est un ennemi.
- **Le héros** est amnésique. C'est en fait **l'Ancien**, le dernier **Veilleur des Bâtisseurs**, une civilisation disparue qui maîtrisait l'Éther. L'Empire l'étudie comme une relique vivante.
- **Le prologue** (30 à 45 minutes) : l'évasion d'un immense vaisseau-prison de l'Empire, en orbite.
  - **Trois camps s'y affrontent** :
    - les gardes ;
    - les pirates qui abordent le vaisseau ;
    - les prisonniers.
  - **Les 6 étapes** :
    1. le réveil au laboratoire ;
    2. les cellules s'ouvrent ;
    3. récupérer sa combinaison dans la chambre des reliques ;
    4. le vide et la coque extérieure ;
    5. le hangar ;
    6. la fuite en hyperespace.
  - **Les règles** :
    - chaque étape se réussit d'au moins deux façons, et on ne bloque jamais le joueur ;
    - des zones facultatives donnent des indices sur l'identité du héros.

## À lire avant de commencer

- `docs/PREPRODUCTION.md` : section 3, le prologue et l'histoire de l'Ancien.
- `docs/prologue/DA-vaisseau-prison.md` : la direction artistique actuelle (version 1).
- `docs/prologue/plan-vaisseau-prison.svg` : le plan actuel.
- `docs/prologue/maquette-*.jpg` et `docs/prologue/maquette-planche.jpg` : la maquette actuelle.
- `docs/prologue/01-Joueur.jpg` à `15-Reliques.jpg` : les 75 modèles.
- `docs/prologue/tailles-modeles.json` : la taille réelle de chaque modèle, en mètres.
- `outils/maquette-prologue.py` : le script qui construit la maquette dans `vitrine/atelier-prologue.html`.
- `docs/IA-3D.md` : les IA 3D gratuites pour fabriquer les modèles qui manquent.

## Le constat de départ

Ce constat est honnête.
- **La DA actuelle est une palette et des recolorations, pas une direction artistique.** Il lui manque :
  - une intention pour chaque moment ;
  - un langage des formes ;
  - un scénario des couleurs ;
  - des règles de composition.
- **La maquette a été montée salle par salle, sans vrai plan.** Les salles sont des boîtes autour des modèles :
  - pas de rythme ;
  - peu de repères visibles de loin ;
  - pas de points de vue pensés.
- **Ton travail** : faire les deux correctement, comme dans un studio.

---

## Partie 1 : la direction artistique

**Livrable** : `docs/prologue/BIBLE-DA.md`, avec ses images.

1. **La critique de l'existant**, en 10 points au plus.
   - Pour chaque point : ce qui marche, ce qui détonne, et l'image où on le voit.
2. **Trois pistes de DA.** Pour chacune :
   - un nom et une phrase ;
   - 3 adjectifs-piliers ;
   - des références du monde réel (architecture, industrie, photographie), par exemple le brutalisme, les sous-marins militaires, les plateformes pétrolières, les centrales des années 1970 ;
   - la palette ;
   - la lumière ;
   - le nombre des 75 modèles qui restent utilisables.
   - Conclus par ta recommandation. **Arrête-toi et demande-moi de choisir.**
3. **La bible, sur la piste choisie** :
   - **L'intention** : ce que le joueur doit ressentir à chacune des 6 étapes, avec une émotion et une image en une phrase.
   - **3 piliers visuels**, chacun avec « on fait » et « on ne fait pas ».
   - **Le langage des formes** de chaque famille (Empire, pirates et détenus, Bâtisseurs) :
     - les formes ;
     - les silhouettes ;
     - les proportions ;
     - ce qui les distingue au premier coup d'œil.
   - **Les valeurs et la lisibilité** :
     - la règle des 3 valeurs ;
     - où va l'œil ;
     - comment un objet utilisable se détache du décor.
   - **La palette** :
     - les codes hexadécimaux, avec le rôle de chaque couleur ;
     - la part de l'écran pour chacune (60, 30, 10) ;
     - les couleurs interdites.
   - **Les matières** : une bibliothèque de 8 matières au plus. Pour chacune :
     - la rugosité ;
     - le côté métallique ;
     - l'usure ;
     - où on la met.
   - **La lumière, zone par zone** :
     - la lumière principale et les accents ;
     - la température en kelvins ;
     - l'intensité en unités HDRP ;
     - comment la lumière guide le joueur.
   - **Le scénario des couleurs** : une bande d'images en SVG, une case par étape, avec la couleur dominante et la luminosité.
   - **L'échelle** : les hauteurs et largeurs qui font paraître l'Empire écrasant.
   - **La signalétique** :
     - le style des lettres ;
     - la numérotation des secteurs ;
     - les bandes de danger ;
     - les textes qui font partie du décor.
   - **L'habillage** :
     - la densité d'objets par zone ;
     - les objets qui racontent quelque chose (traces de l'abordage, vie des détenus, indices sur le héros).
   - **L'audit des 75 modèles face à la bible** :
     - pour chacun : gardé, recoloré, remplacé ou retiré, avec une raison ;
     - puis la liste des modèles qui manquent, par priorité, avec où les trouver gratuitement ou un prompt pour l'IA 3D.
     - Dans ces prompts : aucun nom de film, de jeu ni de marque.
   - **Les tests** :
     - la silhouette ;
     - le noir et blanc ;
     - « sur une capture sans interface, où regarde-t-on ? ».
   - **La traduction dans Unity HDRP** :
     - le profil de volume (exposition, étalonnage des couleurs, brouillard, halo) ;
     - les réglages des matériaux.
   - **Ce qui servira au reste du jeu** : ce qui vaut pour l'Empire, les pirates et les Bâtisseurs partout, au-delà du vaisseau.

## Partie 2 : le plan du niveau

**Livrable** : `docs/prologue/PLAN-NIVEAU.md` et un plan SVG à l'échelle.

1. **Le rythme.**
   - Un tableau des séquences, avec pour chacune :
     - la durée ;
     - la tension de 1 à 5 ;
     - le jeu ;
     - l'émotion ;
     - ce qu'on apprend.
   - Une courbe de la tension dans le temps, en SVG.
2. **Le schéma des zones** : des bulles et leurs liaisons, avant toute dimension.
   - Le chemin principal, les chemins secondaires (conduits, coque extérieure) et les zones facultatives.
   - **Arrête-toi et montre-le-moi** avant de passer à la suite.
3. **Le plan à l'échelle** :
   - une grille en mètres, avec les mêmes axes que la maquette : X vers l'est, Z vers le sud, et le cœur-relique au centre.
   - **Pour chaque zone** :
     - les dimensions et la hauteur sous plafond ;
     - les entrées et les sorties ;
     - le chemin principal et les autres chemins ;
     - les abris, les cachettes, les patrouilles et les camps présents ;
     - les objets utilisables ;
     - le repère visible de loin ;
     - la lumière qui guide.
4. **Les mesures** :
   - **le joueur** : 1,80 m, et ses vitesses ;
   - **les passages** :
     - la largeur des portes ;
     - la largeur des coursives, pour une personne et pour deux ;
     - les conduits ;
     - la hauteur des abris ;
     - ce qu'on peut escalader ;
   - **les ennemis** : à quelle distance ils voient et entendent.
5. **Les points de vue clés**, avec la position de la caméra et ce qu'on voit :
   - la première vue du hangar ;
   - la révélation du cœur-relique ;
   - la sortie sur la coque, dans le vide ;
   - les autres que tu juges utiles.
6. **Les boucles et les verrous** :
   - ce qui ouvre quoi : cartes, courant, piratage ;
   - les solutions de secours, pour que le joueur ne soit jamais coincé.
7. **Les modèles dans le plan** : quel modèle va où, à sa taille réelle d'après `tailles-modeles.json`.

## Partie 3 : appliquer et vérifier

- **Réécris `outils/maquette-prologue.py` d'après le plan.**
  - Les coordonnées sont celles du plan.
  - Le bouton Contrôle de l'atelier ne doit trouver aucun chevauchement.
- **Applique la bible dans l'atelier** :
  - les matières et les néons ;
  - les réglages `da` de `tailles-modeles.json`.
- **Rends les points de vue clés dans l'atelier** et compare-les à la bible. Note les écarts, corrige, et recommence jusqu'à ce que les tests de la bible passent.
- **Mets à jour la doc**, puis commite et pousse.

## Règles de travail

- **Du concret** : des mètres, des codes hexadécimaux, des kelvins, des secondes. Jamais « une ambiance sombre » sans chiffres.
- **Des règles qu'on vérifie** : chaque règle de la bible doit pouvoir se tester sur une capture d'écran.
- **Montre des images** : planches, plan, scénario des couleurs, rendus.
- **Je décide des vrais choix** (la piste de DA, ce qu'on coupe). Pour le reste, décide toi-même.
- **Dans la conversation, des réponses courtes.** Les détails vont dans les fichiers.
- **Ne casse pas l'existant** :
  - les tests de l'atelier ;
  - le script `outils/normaliser-prologue.py` ;
  - le fichier `docs/prologue/maquette-vaisseau-prison.json`.
