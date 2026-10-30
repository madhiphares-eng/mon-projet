# À faire : les priorités

> 30 octobre 2026. Ce qui compte le plus, dans l'ordre. Chaque étape débloque la suivante.

## 1. Récupérer les nouveautés sur le Mac (5 minutes)

Dans le Terminal, relance le téléchargement du prologue. Il ajoute :
- les androïdes de la sécurité ;
- les 13 espèces ;
- la bibliothèque des 84 animations.

```
curl -fsSL https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-prologue.sh | bash -s -- ~/jeu/prologue
```

Il te demande ton token Sketchfab, comme la première fois. Il ne retélécharge pas ce que tu as déjà.

## 2. Voir les personnages bouger (2 minutes)

```
cd ~/jeu && for f in banc-pnj.html prise-armes.js; do curl -fsSLO https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/vitrine/$f; done
cd ~/jeu && python3 -m http.server 8766
```

Puis ouvre dans Chrome : http://localhost:8766/banc-pnj.html?racine=http://localhost:8766/prologue/
- Le menu **« Faire jouer à tous… »** fait jouer une des 84 animations à tout le monde : marche, course, tir, coups, mort…
- Clique sur un camp pour ne voir que lui, sur un nom pour le cadrer.

Les **23 animations du studio maison** sont dans la même liste : `Fusil_Visee`, `Androide_Panne`, `Detenu_Reddition`, `Gris_Marche`… Quand une animation `Fusil_` ou `Pistol_` joue, le banc met l'arme dans les mains, et le **bot de prise** y pose exactement les deux mains ([`prologue/PRISES-ARMES.md`](prologue/PRISES-ARMES.md)).

## 2 bis. Essayer les vues et les effets (2 minutes)

Toujours dans `~/jeu` :
```
for f in vues.html banc-effets.html effets-colosse.js prise-armes.js; do curl -fsSLO https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/vitrine/$f; done
mkdir -p effets && for f in fumee_volutes_8x8 fumee_volutes2_8x8 fumee_fine_20x4 explosion_5x5 boule_feu_8x8 flamme_16x4; do curl -fsSL -o effets/$f.webp https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/vitrine/effets/$f.webp; done
python3 -m http.server 8766
```
- **La vue 3e personne et la navette** : http://localhost:8766/vues.html?racine=http://localhost:8766/prologue/
  - à pied : ZQSD, clic droit pour viser, clic pour tirer, X pour changer d'épaule ;
  - E près de la navette pour monter à bord, Z pour les gaz, V pour passer du cockpit à la poursuite.
- **Les effets** : http://localhost:8766/banc-effets.html (étincelles, vapeur, tirs, champ de force, propulseur, rayon d'éther, gyrophare, feu, explosion).

Tout est expliqué dans [`prologue/VUES-ET-EFFETS.md`](prologue/VUES-ET-EFFETS.md).

## 3. Fabriquer les 3 modèles les plus urgents, sur Meshy (pas Tripo)

**Tripo gratuit interdit l'usage commercial.** Pour le jeu vendu, utilise Meshy gratuit (licence CC BY 4.0). Les prompts sont prêts dans [`prologue/ia/PROMPTS-MESHY.md`](prologue/ia/PROMPTS-MESHY.md). Les plus urgents :
1. **L'androïde de l'Empire** (prompt 1) : le garde de base, celui qu'on voit partout.
2. **Le pirate d'abordage** (prompt 2) : il remplace le pirate cassé.
3. **Le détenu de corvée** (prompt 4) : il remplace la combinaison cassée.

Exporte en **GLB**, puis dépose les fichiers dans le dossier [`depot-modeles`](../depot-modeles/LISEZ-MOI.md) sur GitHub. C'est gratuit, depuis le navigateur : *Add file* puis *Upload files*. Dis-le ensuite au Claude du cloud : il les range, leur donne un squelette, les 84 animations et les couleurs de la bible.

## 4. Préparer les modèles pour Unity (sur le PC, ou le Claude du PC)

Dans l'ordre, sur le dossier du prologue :
1. `normaliser-prologue.py` : la taille réelle, en mètres.
2. `rigger-statues.py` : un squelette pour les statues. Il demande une fois : `pip install bpy scipy`.
3. `recolorer-prologue.py` : les couleurs de la bible. Il demande une fois : `pip install pillow numpy`. **Relance-le même si tu l'as déjà fait** : sa version 2 refait chaque modèle depuis son original (elle corrige une teinte appliquée deux fois, assombrit les deux androïdes candidats et retire le « police » du bleu).

Ensuite, dans Unity :
- importer `00-Animations/UAL1_Standard.fbx` et `UAL2_Standard.fbx` en **Humanoid** ;
- importer `00-Animations/EE_Animations.fbx` (le studio maison) en **Humanoid** ;
- importer chaque personnage en **Humanoid** aussi ;
- menu **Echos d'Ether → Prologue → Créer la scène d'essai des vues** : le hangar, le héros, la navette et les effets, prêts à jouer.

Les 84 animations de la bibliothèque et les 23 du studio s'appliquent alors à tous (détails dans [`prologue/PERSONNAGES.md`](prologue/PERSONNAGES.md), § 4).

## 5. Ensuite, quand tu as le temps

| Quoi | Pourquoi |
|---|---|
| Choisir l'androïde principal : le bleu (conseillé) ou l'orange, d'après la planche [`androides-sombres.jpg`](prologue/personnages/androides-sombres.jpg) | Les deux sont déjà assombris ; l'orange se confond avec les détenus, sa variante rouge sombre non (bible, § 4) |
| Les autres prompts Meshy : chef pirate animable, vieux détenu habillé, combinaison du Veilleur, porte, lit, console, capsule… | Remplacer tout ce que la bible marque « à remplacer » |
| Le jeton Hugging Face dans les secrets de l'environnement cloud (`HF_TOKEN`) | Le Claude du cloud fabrique alors lui-même des modèles avec une IA libre, sans limite de licence |
| Alléger les modèles trop lourds (pirates, androïde lourd, cératopsien) | Le budget est de 60 000 faces par personnage |

## Ce qui est déjà fait (pour mémoire)

- **La DA** : la bible du Colosse, avec la sécurité 100 % androïdes et les autres espèces.
- **Le niveau** : le plan, les 10 plans détaillés et coupes, la maquette sans chevauchement.
- **Les personnages** : 101 modèles triés ; 84 animations reciblées sur 35 personnages ; un squelette automatique pour les statues ; la recoloration selon la bible.
- **Les animations maison** : 23 animations fabriquées par `outils/studio-animations.py` (fusil à deux mains, androïdes, détenus, espèces).
- **Les vues et les effets** : la vue 3e personne, la navette (cockpit, poursuite), neuf effets (fumées, flammes et explosions simulées) ; dans le navigateur et dans Unity.
- **Le bot de prise des armes** : les mains exactement sur l'arme, pour tous les personnages, avec un contrôle automatique des os et de la peau.
