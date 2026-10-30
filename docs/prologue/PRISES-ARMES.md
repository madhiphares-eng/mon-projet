# Le bot de prise des armes

> Version 1, 30 octobre 2026. Les mains se posent exactement sur l'arme, quel que soit le personnage, sans réglage à la main, et un bot vérifie chaque personnage dans chaque animation d'arme.
>
> Code du navigateur : [`vitrine/prise-armes.js`](../../vitrine/prise-armes.js), utilisé par le banc des personnages et l'essai des vues. Code Unity : `PriseArme` et `ArmeTenue` dans `Assets/_Projet/Scripts/Prologue/`.

## 1. Ce qu'il faut retenir

1. **L'arme commande, les mains viennent sur elle.** C'est la méthode des jeux professionnels :
   - **en visée**, la crosse est posée au creux de l'épaule droite et le canon pointe droit devant ; au tir, il se relève au recul ;
   - **arme basse** (attente, marche, course, rechargement), l'arme suit la main droite de l'animation, orientée par les deux mains ;
   - **au pistolet**, le canon prolonge l'avant-bras droit.
2. **Puis les bras se replient par cinématique inverse** (deux os, le coude garde la direction de l'animation) :
   - la paume droite se pose sur la poignée ;
   - la paume gauche se pose sous le garde-main.
   Quand le bras gauche est un peu court, la main glisse vers l'arrière, jusqu'au puits du chargeur, comme le font les vrais tireurs.
3. **Rien à régler pour un nouveau personnage.**
   - Le bot mesure sa paume sur son squelette (les os de la main et des doigts : centre, direction des doigts, côté de la paume).
   - Il essaie 80 appuis de crosse sur ses poses de visée et garde le meilleur : bras jamais tendus à fond, coude pas trop replié, arme hors du corps.
4. **Le bot contrôle tout.** Pour chaque personnage, il passe chaque animation d'arme à huit instants et mesure :
   - l'écart entre chaque paume et sa prise ;
   - l'extension des bras ;
   - l'angle des coudes ;
   - la distance entre l'arme et le torse.
5. **Résultat : 20 personnages parfaits sur 24 armables**, 2 bons et 2 inaptes au fusil (§ 3).
   - Arme basse, si le fusil frôle le ventre d'un personnage corpulent, il l'écarte du corps.
   - Un corps qui ne peut pas tenir un fusil proprement est déclaré inapte : le banc ne le lui donne pas, plutôt que de montrer une pose cassée.

![La visée, sur chaque personnage](prises/visee.jpg)
![Arme basse](prises/repos.jpg)
![Le pistolet](prises/pistolet.jpg)

## 2. Ajouter une arme, ou un personnage

**Un personnage** : rien à faire. Le banc le calibre à sa première animation d'arme (moins d'une seconde), et l'essai des vues fait de même.

**Une arme** : on décrit ses points de prise, dans le repère de l'arme (z vers la bouche, y vers le haut) :

| Point | Ce que c'est |
|---|---|
| `droite` | Le centre de la paume droite sur la poignée, la direction de ses doigts, le côté où regarde la paume |
| `gauche` | Pareil pour la main gauche. Pour un fusil, on ajoute le rail (de combien elle peut glisser vers l'arrière) ; pour un pistolet, `facultative` (elle ne vient que si l'animation l'approche) |
| `crosse` | Le bout de la crosse (ce qui touche l'épaule en visée) |
| `bouche` | Le bout du canon (le départ des tirs) |

- **Dans le navigateur**, on ajoute une entrée à `ARMES` dans `prise-armes.js` (le fusil et le pistolet de l'Empire servent de modèles), puis une ligne à `TENUES` pour chaque animation qui la tient.
- **Dans Unity**, on met le composant `ArmeTenue` sur le modèle de l'arme, avec quatre repères enfants :
  - « Prise droite » et « Prise gauche » : leur avant (Z) suit les doigts, leur haut (Y) sort de la paume ;
  - « Crosse » et « Bouche ».

  Clic droit → « Créer le fusil de l'Empire » remplit tout d'un coup, il suffit ensuite de déplacer les repères sur le vrai modèle.

## 3. Le rapport du bot (navigateur, 24 personnages armables)

Appui de la crosse en centimètres (gauche / avant / bas, pour un bras de 55 cm). Les autres colonnes donnent le pire sur toutes les animations d'arme.

| Personnage | Camp | Appui | Main la plus éloignée de sa prise | Bras le plus tendu | Glissement de la main gauche | Verdict |
|---|---|---|---|---|---|---|
| `tenue-detenu` | heros | 6 / 0 / 0 | 0 mm | 84 % | 18 cm | Parfait |
| `detenu-brute` | detenu | 6 / 0 / 0 | 0 mm | 92 % | 13 cm | Parfait |
| `detenu-vieux` | detenu | 8 / 0 / 0 | 0 mm | 92 % | 18 cm | Parfait |
| `detenu-tshirt` | detenu | 6 / 0 / 0 | 0 mm | 93 % | 15 cm | Parfait |
| `detenu-figurant` | detenu | 8 / 0 / 0 | 0 mm | 94 % | 18 cm | Parfait |
| `saurien-pale` | espece | 6 / 0 / 3 | 0 mm | 93 % | 7 cm | Parfait |
| `saurien-guerrier` | espece | 6 / 6 / 0 | 0 mm | 80 % | 1 cm | Parfait |
| `crocodilien` | espece | 8 / 9 / 0 | 166 mm | 136 % | 18 cm | Inapte au fusil : le fusil entre dans son corps |
| `ceratopsien` | espece | 6 / 0 / 0 | 0 mm | 95 % | 18 cm | Parfait |
| `gris-marcheur` | espece | 8 / 0 / 0 | 0 mm | 95 % | 13 cm | Parfait |
| `gris-anime` | espece | 8 / 0 / 0 | 0 mm | 92 % | 13 cm | Parfait |
| `gris-vert` | espece | 8 / 0 / 0 | 0 mm | 91 % | 13 cm | Parfait |
| `gris-leger` | espece | 8 / 0 / 0 | 5 mm | 91 % | 2 cm | Bon (contact du fusil contre la poitrine en course, l'arme en travers) |
| `longiligne-rouge` | espece | 6 / 6 / 0 | 0 mm | 90 % | 5 cm | Parfait |
| `cephalopode` | espece | 6 / 6 / 0 | 0 mm | 83 % | 1 cm | Parfait |
| `ichtyen` | espece | 6 / 0 / 0 | 0 mm | 96 % | 7 cm | Parfait |
| `mercenaire-alien` | espece | 6 / 0 / 0 | 0 mm | 87 % | 15 cm | Parfait |
| `droide-securite` | androide | 8 / 6 / 0 | 0 mm | 94 % | 18 cm | Parfait |
| `androide-antiemeute` | androide | 8 / 3 / 0 | 0 mm | 90 % | 10 cm | Parfait |
| `androide-sentinelle` | androide | 8 / 0 / -3 | 0 mm | 92 % | 18 cm | Parfait |
| `androide-lourd` | androide | 6 / 6 / 0 | 0 mm | 95 % | 5 cm | Parfait |
| `androide-medical` | androide | 6 / 0 / -3 | 113 mm | 146 % | 18 cm | Inapte au fusil : ses bras sont trop courts pour son corps |
| `pirate-eva` | pirate | 6 / 0 / 0 | 0 mm | 96 % | 18 cm | Bon (contact du fusil contre la poitrine en course, l'arme en travers) |
| `pirate-masque` | pirate | 6 / 6 / 0 | 0 mm | 80 % | 1 cm | Parfait |

- **Le crocodilien** a le torse d'un quadrupède et des bras courts : un fusil épaulé lui rentre dans le corps. Il lui faut une arme à sa forme : une arme intégrée, ou un pistolet (son pistolet est accepté).
- **L'androïde médical** a des bras trop courts pour épauler. Il peut tenir un pistolet ; c'est un androïde de soin, ce n'est pas un défaut.
- **Le gris léger et la pirate en combinaison** : en course, le fusil tenu en travers touche la poitrine. C'est normal pour cette tenue.
- Les 11 personnages restants (statues sans bras, robots non humanoïdes, personnages écartés) ne prennent pas d'arme.

## 4. Dans Unity

- `PriseArme` (sur le héros, ou tout personnage Humanoid) :
  - il s'exécute après l'animation (`LateUpdate`) ;
  - `HerosPrologue` lui donne le mode : visée = Epaule, sinon Main ; pendant le rechargement, la main gauche lâche l'arme ; au tir, le canon se relève ;
  - au démarrage, il calibre l'appui de la crosse sur les animations de visée (`Fusil_Visee`, `Fusil_Marche_Visee`, `Fusil_Accroupi_Visee`).
- En jeu, clic droit sur le composant → **« Contrôler les prises (bot) »** : le rapport de chaque animation d'arme s'écrit dans la console.
- La scène d'essai (menu **Echos d'Ether → Prologue**) les installe déjà sur le héros.
