# Les personnages du prologue : le test

> Version 1, 29 octobre 2026. Les 23 personnages du prologue passés au **banc des personnages** ([`vitrine/banc-pnj.html`](../../vitrine/banc-pnj.html)) : squelette, animations, taille, poids. La sécurité de l'Empire n'emploie plus que des androïdes (voir la [bible](BIBLE-DA.md), § 4).

## 1. Le banc des personnages

Une page qui charge chaque personnage et le mesure tout seul :
- **le squelette** : présent ou non, nombre d'os, humain ou non, nommé façon Mixamo ou non ;
- **la pose de départ** : T (bras à l'horizontale), A (bras à 30-45°) ou bras baissés, mesurée sur l'angle du bras gauche ;
- **chaque animation** : est-ce qu'elle bouge, est-ce qu'elle avance (root motion), est-ce qu'elle grossit le modèle (unités fausses) ;
- **la taille d'origine** : beaucoup de fichiers sont en centimètres, ou plus étranges ;
- **le poids** : faces, textures.

Tous les personnages sont remis à leur taille de jeu et posés en rangée par camp, avec une silhouette cyan de 1,80 m pour comparer. Le banc montre les couleurs d'origine des fichiers ; les recolorations de la bible (acier sombre pour les androïdes) sont appliquées par l'atelier et le seront dans Unity.

**S'en servir sur le Mac** : ouvrir `vitrine/banc-pnj.html` dans Chrome, cliquer sur « Choisir le dossier », choisir `~/jeu/prologue`.
- Clique sur un camp pour ne voir que lui.
- Clique sur un nom pour le cadrer.
- Clique sur un bouton pour jouer une autre animation.

![Les détenus](personnages/rangee-detenu.jpg)
![La sécurité : les androïdes](personnages/rangee-androide.jpg)
![Les pirates](personnages/rangee-pirate.jpg)

## 2. Ce qu'il faut retenir

1. **Les androïdes prêts à jouer** : trois sont animés et utilisables tout de suite, sans retouche.
   - `androide-sentinelle` : squelette humain, pose en T, une animation de mouvements variés.
   - `androide-antiemeute` : squelette humain, pose en A, une marche de 1,4 s.
   - `marcheur-blinde` : 41 animations (marche, course, coups, chute, démarrage, surchauffe), sur place et en root motion.
2. **Les détenus prêts pour Mixamo** : `detenu-tshirt` et `detenu-figurant` ont un squelette Mixamo, donc les centaines d'animations gratuites de Mixamo s'appliquent directement. Ils n'ont pas encore d'animation à eux.
3. **Les statues** : huit personnages n'ont pas de squelette.
   - Pour le héros et les androïdes debout : à faire rigger sur Mixamo, c'est automatique.
   - Le fantassin est posé avec son arme : il reste un décor.
4. **Deux retraits** :
   - le garde humain (la sécurité n'est faite que d'androïdes) ;
   - `detenu-atelier`, dont l'auteur déclare qu'il reprend le personnage d'un autre jeu : même sous licence libre, on ne le garde pas.
5. **Un modèle cassé** : la combinaison étanche (`detenu-corvee-toxique`). Son squelette empile des échelles ×100 et ×0,01 ; le corps s'affiche mal. Il faut appliquer les échelles dans Blender avant Unity. En attendant, il n'est pas placé dans la maquette.
6. **Les unités** : sept fichiers ne sont pas en mètres (du centimètre jusqu'à 30 000 unités pour 1,85 m). L'atelier et le normaliseur les remettent à la bonne taille ; dans Unity, l'import doit le faire aussi.
7. **Trop lourds** pour le budget de 60 000 faces par personnage : `pirate-masque` (925 k), `pirate-eva` (705 k), `pirate-chef` et `androide-lourd` (240 k), `droide-securite` (125 k), `detenu-brute` (111 k).

## 3. Personnage par personnage

| Personnage | Camp | Squelette | Pose | Animations | Faces | Verdict |
|---|---|---|---|---|---:|---|
| `tenue-detenu` | Héros | Aucun | — | Une pose fixe | 12 k | Statue : à rigger sur Mixamo (provisoire de toute façon) |
| `detenu-brute` | Détenus | Humain, 102 os | A | 1 qui bouge (15 s) | 111 k | Bon ; à alléger |
| `detenu-vieux` | Détenus | Humain, 102 os | A | 1 qui bouge (22 s) | 54 k | Bon squelette ; torse nu, à rhabiller (IA prévue) |
| `detenu-tshirt` | Détenus | Mixamo, 69 os | T | Aucune | 19 k | **Le détenu de base** : animations Mixamo directes |
| `detenu-figurant` | Détenus | Mixamo, 58 os | T | Une pose fixe | 2 k | **La foule** : très léger, animations Mixamo directes |
| `detenu-corvee-toxique` | Détenus | Humain, 68 os | A | Aucune | 55 k | Squelette cassé : à corriger dans Blender |
| `detenu-atelier` | Détenus | Non humain, 135 os | — | 1 qui bouge sur 4 | 35 k | **Retiré** : personnage d'un autre jeu |
| `garde` | — | Mixamo, 66 os | Bras baissés | 1 qui bouge | 107 k | **Retiré** : garde humain |
| `droide-securite` | Androïdes | Humain, 84 os | Bras baissés | 1 qui bouge (21 s) | 125 k | Bon ; à alléger. Garde des officiers |
| `androide-sentinelle` | Androïdes | Humain, 265 os | T | 1 qui bouge (17 s) | 28 k | **Prêt** : le garde de base |
| `androide-antiemeute` | Androïdes | Humain, 102 os | A | 1 marche (1,4 s) | 20 k | **Prêt** ; effacer « police » |
| `androide-patrouille` | Androïdes | Aucun, pièces animées | — | 1 qui bouge (5 s) | 68 k | Utilisable tel quel ; en centimètres |
| `androide-soldat` | Androïdes | Aucun | — | Aucune | 21 k | Statue : décor « en réparation » ou Mixamo |
| `androide-fantassin` | Androïdes | Aucun | — | Aucune | 32 k | Statue posée avec son arme : décor |
| `androide-lourd` | Androïdes | Aucun | — | Aucune | 241 k | Statue lourde : à alléger puis rigger |
| `androide-medical` | Androïdes | Aucun | — | Aucune | 21 k | Statue : Mixamo |
| `androide-vieux-modele` | Androïdes | Aucun | — | Aucune | 26 k | Décor : en pièces dans les ateliers |
| `drone-securite` | Androïdes | Aucun | — | Aucune | 11 k | **Prêt** : un drone flotte, ses pièces tournent par script |
| `marcheur-blinde` | Androïdes | Non humain, 21 os | — | **41**, dont 37 qui bougent | 11 k | **Prêt** : le plus complet |
| `pirate` | Pirates | Humain, 88 os | Bras baissés | 1, presque immobile | 99 k | Animation trop faible ; unités énormes (30 000) |
| `pirate-eva` | Pirates | Humain, 72 os | A | 3 : marche, attente, course (Mixamo) | 705 k | Bonnes animations ; à alléger d'urgence |
| `pirate-chef` | Pirates | Aucun | — | Aucune | 242 k | Statue ; à alléger puis rigger |
| `pirate-masque` | Pirates | Aucun | — | Aucune | 925 k | Statue ; à alléger puis rigger |

Les images de chaque personnage, à quatre moments de son animation, sont dans [`personnages/`](personnages/).

## 4. Où ils sont dans la maquette

La maquette (`outils/maquette-prologue.py`) ne pose plus de silhouettes pour la sécurité et les détenus, mais les vrais modèles. Seuls les pirates restent des silhouettes orange.

| Lieu | Sécurité | Détenus |
|---|---|---|
| Laboratoire | `androide-medical` | — |
| Anneau du cœur | 2 androïdes en ronde | — |
| Nef des cellules | `droide-securite` au poste de garde, 4 androïdes sur les galeries, 2 drones | 6 détenus, `detenu-brute`, `detenu-tshirt` |
| Ateliers | `androide-soldat` et `androide-vieux-modele` en réparation | 5 détenus |
| Consigne | 2 androïdes | — |
| Salle des machines | 1 drone | — |
| Quartiers des officiers | 2 androïdes | — |
| Passerelle | 3 `droide-securite` | — |
| Hangar | 5 androïdes, `androide-lourd` à la porte, `marcheur-blinde`, 1 drone | — |

## 5. Ce qu'il reste à faire

| Qui | Quoi |
|---|---|
| Toi, sur le Mac | Relancer `outils/telecharger-prologue.sh` : il ajoute les 13 nouveaux modèles. Puis `normaliser-prologue.py` pour les remettre à leur taille. |
| Mixamo (gratuit) | Rigger le héros, `androide-medical`, `androide-soldat` ; donner des animations (attente, marche, course, tir, mort) aux détenus Mixamo et aux androïdes. |
| Le Claude du PC (Blender) | Corriger le squelette de la combinaison étanche ; alléger les pirates, `androide-lourd`, `droide-securite`, `detenu-brute` ; effacer « police » et « security ». |
| L'IA 3D (avec ton jeton) | L'androïde de l'Empire à la forme exacte de la bible, et le vieux détenu rhabillé ([`ia/fiches.json`](ia/fiches.json)). |
