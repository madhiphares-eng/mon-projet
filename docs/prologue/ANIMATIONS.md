# Les animations du prologue : le bot des mouvements

> Version 1, 30 octobre 2026. Un bot regarde chaque personnage du prologue dans chaque animation au sol, mesure ce qui se voit mal (pieds dans le sol, corps qui flotte, boucle qui saute, pieds qui glissent), corrige ce qui peut l'être, et rend un rapport.
>
> Code du navigateur : [`vitrine/bot-mouvements.js`](../../vitrine/bot-mouvements.js), utilisé par le banc des personnages ([`vitrine/banc-pnj.html`](../../vitrine/banc-pnj.html)). Code Unity : `AncragePieds` dans `Assets/_Projet/Scripts/Prologue/`. Rapport complet : [`animations/rapport-mouvements.json`](animations/rapport-mouvements.json).

## 1. Ce qu'il faut retenir

1. **28 personnages × 23 animations = 644 contrôles, en 48 secondes.** Les animations contrôlées sont toutes celles qu'on joue au sol : attentes, marches, courses, accroupi, fusil, androïdes, détenus, espèces.
2. **Avant le bot**, la bibliothèque reciblée sur des corps très différents laissait les pieds s'enfoncer de 5 à 13 cm chez la plupart, jusqu'à 35 cm chez le saurien pâle. La fin de chaque boucle sautait de 7 à 9 cm, et le gris léger flottait à 23 cm du sol en accroupi.
3. **Après le bot**, sur 26 personnages sur 28 :
   - les pieds touchent le sol à moins de 1,5 cm près (le plus souvent moins de 1 cm) ;
   - aucun ne flotte plus ;
   - toutes les boucles se raccordent (moins de 1 cm).
4. **Le bot a trouvé un vrai bug** dans `outils/rigger-statues.py`, le script qui donne un squelette aux statues. Chez 6 d'entre elles, les pieds restaient 32 à 38 cm sous le sol quoi qu'on fasse. La cause : les pieds suivaient la cuisse, pas le pied (§ 5). Depuis la correction :
   - les 6 statues touchées marchent, pieds au sol ;
   - 4 statues jusque-là « ratées » s'animent aussi : le chef pirate (manteau compris), le mutant biomécanique, l'androïde-soldat et le vieux robot de patrouille.
5. **En jeu**, c'est le composant `AncragePieds` qui fait le travail, sur un vrai sol : marches, rampes, passerelles (§ 6). Le héros l'a d'office, et chaque PNJ animé le reçoit tout seul.
6. **Restent à surveiller** : le saurien pâle quand il avance accroupi (6 cm) et le gris léger dans la marche « formelle » (23 cm). Le gris léger a un squelette Rigify aux jambes coupées en deux. Ces deux animations sont à éviter pour eux.

![Avant et après l'ancrage, à hauteur des pieds](animations/ancrage-avant-apres.jpg)

## 2. Ce que le bot mesure

Le pied est mesuré par **sa semelle** : les sommets de la peau sous la cheville, tels que les os les déforment à l'image. C'est ce qui touche le sol à l'écran, quelle que soit la forme du pied (à plat, sur la pointe, griffu, sabot de robot). Le bot ne garde que la peau que porte vraiment la jambe (le mollet, le pied, les orteils, d'après les poids de la peau). Un pan de manteau ou l'autre jambe qui frôle ne trompent donc pas la mesure.

| Défaut | Ce qu'il mesure | Ce qui se voit |
|---|---|---|
| **Enfoncement** | De combien la semelle la plus basse passe sous le sol | Des pieds coupés par le sol |
| **Flottement** | De combien la semelle la plus basse reste au-dessus du sol | Un personnage qui lévite |
| **Saut de boucle** | L'écart entre la dernière image et la première (mains, pieds, tête, par rapport au bassin) | Un à-coup à chaque tour de boucle |
| **Glissement** | Le déplacement d'un pied posé, par rapport au sol qui défile sous lui | Des pieds qui patinent |
| **Déchirure** | La part des arêtes de la peau étirées au double ou écrasées de moitié | Pour information : les plis des articulations l'élèvent aussi chez les bons personnages |

Les mesures sont rendues **en centimètres d'un personnage de 1,83 m**, quelle que soit la taille réelle, pour comparer un gris de 1,40 m et un androïde lourd de 2,60 m.

**Comment il sait qu'un pied est posé.** C'est le mouvement qui décide, pas la hauteur seule : un pied porte quand il ne part pas vers l'avant (la jambe qui passe, elle, file vers l'avant) et qu'il est près du plus bas de sa course. Ainsi, un pied mal reciblé qui traverse le sol reste reconnu comme posé.

**Les animations sont sur place**, comme celles de la bibliothèque : le sol défile sous les pieds, comme un tapis roulant. Le bot mesure la vitesse de ce tapis sur les pieds posés eux-mêmes. Une animation « sur place » (attente, visée, garde) a un tapis immobile ; une marche ou une course a un tapis qui recule.

## 3. Ce qu'il corrige

Pour chaque personnage et chaque animation, le bot fabrique une **nouvelle animation corrigée** (le banc la garde en mémoire, l'original n'est pas touché) :

1. **Chaque pied posé est ramené au sol** par cinématique inverse : la jambe se replie (cuisse, mollet, pied), le genou garde la direction de l'animation, et le pied garde son orientation.
2. **Aucun pied ne passe sous le sol**, posé ou non.
3. **Les hanches descendent** quand une jambe ne peut pas atteindre son appui (lissé dans le temps, pour ne pas sautiller). Dans les squelettes où les jambes ne descendent pas des hanches (Rigify), c'est leur ancêtre commun qui descend.
4. **Un corps qui flotte descend tout entier** jusqu'à ce qu'un pied touche (l'accroupi du gris léger).
5. **La boucle est raccordée** : les 8 dernières images rejoignent en douceur la première.
6. **Sur place, le pied posé est tenu immobile.**

Dans une marche ou une course sur place, le pied posé n'est **pas** tenu immobile : un pied qui roule du talon aux orteils n'a pas de point fixe sur un tapis roulant. Le glissement de ces animations n'est donc pas corrigé dans le banc ; c'est en jeu, sur un vrai sol, qu'`AncragePieds` bloque le pied posé.

**Ce qui n'est pas corrigé** : les sauts, roulades, glissades, nage, morts, chutes, conduite et positions assises. Leurs pieds quittent le sol exprès ; le bot les laisse tels quels.

## 4. Le rapport (28 personnages)

Le pire de chaque défaut sur les 23 animations, avant → **après** la correction, en centimètres.

| Personnage | Enfoncement médian (cm) | Enfoncement pire | Saut de boucle pire | Flottement pire | Verdict |
|---|---|---|---|---|---|
| `tenue-detenu` | 2,6 → **0,2** | 6,1 → **0,9** | 7,8 → **0,9** | 0 → **0,1** | ✅ |
| `detenu-brute` | 4,7 → **0** | 9,9 → **0,5** | 7,3 → **0,8** | 0 → **0** | ✅ |
| `detenu-vieux` | 4,2 → **0** | 11 → **0,5** | 7,5 → **0,9** | 0 → **0** | ✅ |
| `detenu-tshirt` | 4,4 → **0,1** | 8,1 → **0,5** | 7,2 → **0,8** | 0 → **0** | ✅ |
| `detenu-figurant` | 1,6 → **0** | 5,4 → **0,4** | 7,2 → **0,8** | 1,1 → **0** | ✅ |
| `saurien-pale` | 20,8 → **0,2** | 35,3 → **5,8** | 7,5 → **1** | 0 → **0** | à surveiller : `Crouch_Fwd_Loop` |
| `saurien-guerrier` | 5 → **0,2** | 10 → **0,7** | 8 → **0,9** | 0 → **0** | ✅ |
| `crocodilien` | 19,5 → **0,5** | 24,5 → **1,5** | 9,1 → **1,1** | 0 → **0** | ✅ |
| `ceratopsien` | 1,4 → **0** | 9,7 → **0,6** | 7,8 → **0,9** | 0 → **0** | ✅ |
| `gris-marcheur` | 2,4 → **0** | 7 → **0,5** | 7,2 → **0,8** | 0,9 → **0** | ✅ |
| `gris-anime` | 0 → **0** | 3,3 → **0,5** | 7,6 → **0,9** | 3,8 → **0** | ✅ |
| `gris-vert` | 1,6 → **0,1** | 5,5 → **0,9** | 7,8 → **0,9** | 1,7 → **0** | ✅ |
| `gris-leger` | 27,6 → **2,9** | 37,4 → **23,5** | 12,3 → **3** | 23,2 → **0** | à surveiller : `Walk_Formal_Loop` |
| `longiligne-rouge` | 0,2 → **0** | 4,5 → **0,4** | 7,7 → **0,9** | 1,6 → **0** | ✅ |
| `cephalopode` | 1,8 → **0,1** | 7,5 → **0,5** | 7,8 → **0,9** | 0 → **0** | ✅ |
| `ichtyen` | 1,5 → **0** | 9,2 → **0,5** | 7,7 → **0,9** | 0 → **0** | ✅ |
| `mercenaire-alien` | 4,7 → **0** | 11 → **0,5** | 7,9 → **0,9** | 0,1 → **0** | ✅ |
| `mutant-biomecanique` | 1,7 → **0** | 6,8 → **0,3** | 7,9 → **0,9** | 1,6 → **0** | ✅ |
| `droide-securite` | 4,4 → **0** | 10,9 → **0,8** | 7,1 → **0,8** | 0 → **0** | ✅ |
| `androide-antiemeute` | 5,7 → **0** | 13,1 → **0,7** | 8,1 → **0,9** | 0 → **0** | ✅ |
| `androide-sentinelle` | 3,4 → **0** | 7,1 → **0,5** | 8 → **0,9** | 1,5 → **0** | ✅ |
| `androide-soldat` | 4 → **0,1** | 6,9 → **0,6** | 7,7 → **0,9** | 0 → **0** | ✅ |
| `androide-lourd` | 4,2 → **0** | 8,7 → **0,5** | 7,8 → **0,9** | 0 → **0** | ✅ |
| `androide-medical` | 3,1 → **0** | 9,3 → **0,3** | 8,1 → **0,9** | 1,6 → **0** | ✅ |
| `androide-vieux-modele` | 4 → **0,1** | 7,1 → **0,6** | 7,9 → **0,9** | 0 → **0** | ✅ |
| `pirate-eva` | 5,9 → **0** | 12,1 → **0,5** | 6,4 → **0,8** | 0 → **0** | ✅ |
| `pirate-chef` | 8,3 → **0,1** | 15,6 → **0,8** | 8,5 → **1** | 0 → **0** | ✅ |
| `pirate-masque` | 1,6 → **0,1** | 5,4 → **0,5** | 7,8 → **0,9** | 0,4 → **0** | ✅ |

- **Le saurien pâle** : seule l'avance accroupie (`Crouch_Fwd_Loop`) garde 6 cm. Ses jambes de reptile, très repliées, n'atteignent pas tout à fait le sol dans cette pose. Pour lui, l'accroupi immobile suffit.
- **Le gris léger** : son squelette Rigify coupe chaque jambe en deux os et le reciblage de la marche « formelle » (`Walk_Formal_Loop`) lui étire les jambes. Le bot réduit l'enfoncement de 32 à 23 cm, mais pas plus. Il garde `Walk_Loop` et `Gris_Marche`, qui sont bonnes (3 cm).
- **Le glissement** reste tel quel dans le banc pour les marches et les courses (voir § 3) : il est corrigé en jeu.

Pour relancer le contrôle : ouvrir le banc, puis dans la console du navigateur, `await banc.controlerMouvements(['Idle_Loop', 'Walk_Loop', …])`. `banc.ancrage(false)` montre les animations d'origine, pour comparer.

## 5. Le bug des statues, et ce qu'il faut refaire sur le Mac

**Le symptôme.** 6 statues riggées par le script (`ichtyen`, `ceratopsien`, `cephalopode`, `saurien-guerrier`, `pirate-masque`, `androide-lourd`) gardaient les pieds 32 à 38 cm dans le sol, même après la correction. Le bot de prise disait déjà que la peau de leurs mains ne suivait pas leurs os.

**La cause.** Dans Blender, le script pose le squelette à la taille du modèle, puis « applique » cette échelle pour qu'elle entre dans les os. Or Blender n'applique une transformation qu'aux objets **sélectionnés**, et le squelette ne l'était pas. Pour un modèle fait en mètres (le héros, le gris vert), l'échelle valait 1 et rien ne se voyait. Pour un modèle fait dans d'autres unités, le squelette était 10 à 20 fois trop grand au moment du calcul des poids de la peau. L'ichtyen, par exemple, est à l'échelle ×12,6. Les poids étaient donc absurdes : chez l'ichtyen, **tous** les sommets des pieds suivaient la cuisse. Le genou ne pliait pas à l'écran, et le pied traversait le sol.

**La correction.** Le script sélectionne maintenant le squelette avant d'appliquer l'échelle. Résultat :
- les 6 statues ont des poids sains : les pieds suivent le pied et les orteils, les mains suivent la main ;
- 4 statues que le script ratait s'animent maintenant : `pirate-chef` (×98), `mutant-biomecanique` (×97), `androide-vieux-modele` (×115) et `androide-soldat` ;
- seul `androide-fantassin`, fait de 1 800 pièces sans nom, s'éparpille encore : il reste un androïde en réparation dans un atelier.

![Les statues riggées par le script corrigé](animations/statues-riggees.jpg)

**Sur le Mac**, il faut relancer le script : les modèles riggés avant la correction ont les mauvais poids. Dans le Terminal :

```
cd ~/jeu
python outils/rigger-statues.py ~/jeu/prologue
python outils/recolorer-prologue.py ~/jeu/prologue
```

- `rigger-statues.py` repart des fichiers `<nom>.statue.glb` gardés à côté : rien n'est perdu, et le relancer ne coûte rien (environ 3 minutes).
- `recolorer-prologue.py` remet les couleurs de la bible sur les modèles re-riggés. Il a aussi été corrigé : un modèle re-riggé devient le nouvel original de ses couleurs, au lieu de garder l'ancien.
- Puis recopier les modèles dans le projet Unity, comme d'habitude : Unity les réimporte.

## 6. Dans Unity : `AncragePieds`

Le même bot, en jeu et sur un vrai sol. Le composant s'exécute après l'animation et avant la prise des armes (`LateUpdate`, ordre 90 ; `PriseArme` passe à 100). Il marche sur n'importe quel corps *Humanoid*.

- **Chaque pied suit le sol sous lui** (un rayon vers le bas par pied) : marches, rampes, passerelles. Le pied garde la hauteur que lui donne l'animation, au-dessus du vrai sol.
- **Un pied posé se bloque** là où il est tant qu'il porte le corps. Il ne glisse plus quand le personnage avance un peu plus vite ou moins vite que son animation, ni quand il tourne sur place. Le blocage se relâche en 0,12 s quand le pied se lève. Si le pied est à plus de 60 cm de son point de blocage (téléportation, demi-tour), il est reposé.
- **Les hanches descendent** du côté du pied le plus bas, pour que la jambe l'atteigne. Elles ne montent jamais plus haut que l'animation.
- **Le pied posé épouse la pente.**
- **L'IK des pieds intégrée aux *Humanoid*** est aussi activée sur chaque animation (`SetApplyFootIK`) : les pieds de la cible retrouvent ceux de l'animation avant l'ancrage.

| Réglage | Par défaut | Rôle |
|---|---|---|
| `sol` | tout | Les couches du sol (exclure les déclencheurs, les débris) |
| `seuilAppui` | 0,04 m | Un pied est posé quand il est à moins de 4 cm de son plus bas |
| `portee` | 0,7 m | Jusqu'où chercher le sol, au-dessus et au-dessous du personnage |
| `poids` | 1 | 0 : l'animation brute ; 1 : l'ancrage complet |
| `bloquerLesPiedsPoses` | oui | Le blocage des pieds posés |
| `epouserLaPente` | oui | Le pied posé s'incline sur la pente |

**Qui l'a ?**
- **Le héros** : `ConstructeurPrologue` le lui ajoute avec `PriseArme`.
- **Chaque PNJ animé** : `AnimationPNJ` l'ajoute tout seul au démarrage s'il manque (réglage `ancrerLesPieds`, coché par défaut). Les drones et les robots non humanoïdes sont ignorés.

## 7. La suite des animations

| Étape | Quoi |
|---|---|
| Fait | Le bot des mouvements, l'ancrage en jeu, le rig des statues corrigé : 14 statues animées proprement, seul l'androïde-fantassin reste un décor |
| À suivre | Les styles par espèce : une attente et une démarche propres à chaque espèce (gris, sauriens, ichtyen, céphalopode, androïdes), faites par le studio et contrôlées par le bot |
| À suivre | La vie : le regard qui suit le joueur, la respiration, les queues et les pièces qui suivent le mouvement |
| À suivre | Les objets : portes, tourelles, drones, consoles, ventilateurs, pistons, grue, sas, avec trois états d'énergie (normal, alarme, panne) |
