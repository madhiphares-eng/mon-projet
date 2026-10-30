# À faire : les priorités

> 29 octobre 2026. Ce qui compte le plus, dans l'ordre. Chaque étape débloque la suivante.

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
curl -fsSL https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/vitrine/banc-pnj.html -o ~/jeu/banc-pnj.html
cd ~/jeu && python3 -m http.server 8766
```

Puis ouvre dans Chrome : http://localhost:8766/banc-pnj.html?racine=http://localhost:8766/prologue/
- Le menu **« Faire jouer à tous… »** fait jouer une des 84 animations à tout le monde : marche, course, tir, coups, mort…
- Clique sur un camp pour ne voir que lui, sur un nom pour le cadrer.

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
3. `recolorer-prologue.py` : les couleurs de la bible. Il demande une fois : `pip install pillow numpy`.

Ensuite, dans Unity :
- importer `00-Animations/UAL1_Standard.fbx` et `UAL2_Standard.fbx` en **Humanoid** ;
- importer chaque personnage en **Humanoid** aussi.

Les 84 animations s'appliquent alors à tous (détails dans [`prologue/PERSONNAGES.md`](prologue/PERSONNAGES.md), § 4).

## 5. Ensuite, quand tu as le temps

| Quoi | Pourquoi |
|---|---|
| Les autres prompts Meshy : chef pirate animable, vieux détenu habillé, combinaison du Veilleur, porte, lit, console, capsule… | Remplacer tout ce que la bible marque « à remplacer » |
| Le jeton Hugging Face dans les secrets de l'environnement cloud (`HF_TOKEN`) | Le Claude du cloud fabrique alors lui-même des modèles avec une IA libre, sans limite de licence |
| Alléger les modèles trop lourds (pirates, androïde lourd, cératopsien) | Le budget est de 60 000 faces par personnage |

## Ce qui est déjà fait (pour mémoire)

- **La DA** : la bible du Colosse, avec la sécurité 100 % androïdes et les autres espèces.
- **Le niveau** : le plan, les 10 plans détaillés et coupes, la maquette sans chevauchement.
- **Les personnages** : 101 modèles triés ; 84 animations reciblées sur 35 personnages ; un squelette automatique pour les statues ; la recoloration selon la bible.
