# Direction artistique du vaisseau-prison

> Version 1, 29 octobre 2026. Pour le prologue d'*Échos d'Éther*.
> Règle d'or : **peu d'objets différents, mais qui se ressemblent**. Un objet qui ne colle pas à la palette est recoloré, ou il attend d'être remplacé.

## 1. L'intention

Un **futur usé, industriel**, comme dans *Star Wars* ou *Alien* : des machines lourdes et réparées cent fois, du métal sombre, des voyants et des écrans cathodiques. Rien de neuf, rien de brillant, rien de décoratif.

Trois familles d'objets, qui ne se mélangent pas :

| Famille | Matières | Couleurs |
|---|---|---|
| **L'Empire** (le vaisseau-prison, les gardes) | Acier peint sombre, écaillé ; caoutchouc ; verre sale | Gris acier, ivoire usé ; marquages jaune et noir |
| **Les pirates et les détenus** | Bricolage, rouille, tissus | Orange (combinaisons des détenus, tenues des pirates) et rouille |
| **Les Bâtisseurs** (les reliques) | Pierre ou métal noir, lisse, ancien | Lignes de glyphes **cyan** lumineuses : la couleur de l'Éther |

## 2. La palette

| Rôle | Couleur |
|---|---|
| Acier (murs, machines) | `#2b2f36` à `#5a606a` |
| Ivoire usé (panneaux, plastiques) | `#cfc7b3` |
| Lumière de travail (néons) | blanc froid `#f2f4ff` |
| Technologie, Éther | cyan `#5fe0d8` |
| Signalétique, machines | ambre `#f2b26b` |
| Alarme, verrouillé, le vide | rouge `#ff4a3a` |
| Détenus, pirates | orange `#d9732b` |

Pas de magenta, pas de vert, pas de bleu vif. Les seules exceptions sont les feux de position vert et rouge de la coque, une convention de navigation.

## 3. La lumière

- **Néons blancs froids** au plafond : la lumière de base, régulière, un peu clinique.
- **Accents de couleur, rares**, qui disent quelque chose :
  - le cyan signale la technologie et les reliques ;
  - l'ambre signale les machines et ce que l'on peut utiliser ;
  - le rouge signale une alarme, une porte scellée ou le vide.
- **Le noir** domine dans l'ambiance de nuit : on lit les volumes par les lumières.
- Au sol, un liseré cyan le long des coursives principales pour guider le joueur.

## 4. Comment l'atelier tient la palette

- **Style unifié** (bouton dans la barre) : tous les modèles passent par le même étalonnage, moins saturé et un peu plus froid. Des modèles d'auteurs différents finissent ainsi par se ressembler.
- **Recoloration par modèle** : le réglage `da` de [`tailles-modeles.json`](tailles-modeles.json) corrige ceux qui détonnent. Par exemple, le lit superposé en bois passe en acier gris, et le prisme orange devient cyan.
- **Les volumes de la maquette** (murs, plafonds, sols) ne sont plus en gris uni : les murs sont en **acier à plaques**, les sols en **tôle**. Les modèles détaillés ne détonnent plus à côté des blocs.
- Dans Unity, on fera la même chose avec un réglage de couleurs commun (*color grading*) et des matériaux retouchés.

## 5. Les 75 modèles face à la palette

| Verdict | Modèles |
|---|---|
| **Gardés tels quels** | tenue-detenu, detenu-brute, detenu-vieux, detenu-atelier, pirate, pirate-chef, pirate-masque, casque-pirate, pirate-eva, porte-cellule, porte-isolement, toilettes, lavabo, couloir-principal, porte-coulissante, sols, casier, armoire-blindee, generateur, pylone-energie, coeur-fusion, baie-serveur, rangee-serveurs, console-murale, cables, cellule-energie, bidon-carburant, vaisseau-prison, vaisseau-prison-variante, navette-a-voler, porte-sas, sas, rocher-spatial, les armes, extincteur, neons, caisse-cargo, chambre-stase, coeur-relique, tablette-glyphes, anneau-relais |
| **Recolorés** (`da`) | lit-superpose (bois → acier), droide-securite, combinaison-veilleur, porte-coffre, reacteur, terminal (beige → gris), caisses, caisse-armes, navette-transfert (blanc → gris sombre), vaisseau-pirate (camouflage → rouille), capsule-sauvetage, cloison-etanche, debris-1, debris-2, gravats-breche (briques → gris), caisse-munitions, capsule-veilleur, cristal-ether et prisme-ether (→ cyan) |
| **À remplacer plus tard** | garde (équipement d'intervention d'aujourd'hui), vaisseau-pirate (un appareil militaire terrien), combinaison-veilleur (sans glyphes : à faire avec une IA, voir [`IA-3D.md`](../IA-3D.md)), lit-superpose (un vrai lit en métal) |
| **Écartés de la maquette** | grand-ecran (un cadre qui ressemble à de la roche) ; bloc-cellules (un bâtiment de prison terrien) ; kit-interieur, kit-couloir et tuyaux (des catalogues de pièces détachées) ; conduit-maintenance (blanc et vert, trop propre) ; hangar (une petite plateforme, pas une salle) |

## 6. Avant d'ajouter un modèle

1. **Sa famille** : il appartient à l'Empire, aux pirates et détenus, ou aux Bâtisseurs.
2. **Ses couleurs** : elles entrent dans la palette, ou une recoloration `da` suffit.
3. **Son niveau de détail** : il est proche de ses voisins (pas un objet très stylisé à côté d'un scan réaliste).
4. **Sa taille réelle** : elle est inscrite dans `tailles-modeles.json`.

Si l'une de ces quatre réponses est non, on ne l'ajoute pas.
