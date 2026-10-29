# Les personnages du prologue : animations, squelettes, espèces

> Version 2, 29 octobre 2026. Les 36 personnages du prologue passés au **banc des personnages** ([`vitrine/banc-pnj.html`](../../vitrine/banc-pnj.html)). Ils comptent :
> - les humains ;
> - les androïdes : la sécurité de l'Empire n'emploie qu'eux (voir la [bible](BIBLE-DA.md), § 4) ;
> - les autres espèces de la galaxie.
>
> Tous reçoivent la même **bibliothèque de 84 animations**.

## 1. Ce qu'il faut retenir

1. **84 animations gratuites pour tout le monde.** La bibliothèque vient de Quaternius (CC0, domaine public : aucune obligation, même pour un jeu vendu). Elle est rangée dans `prologue-anim/00-Animations/` ; les scripts de téléchargement la copient dans `00-Animations/`.
2. **Le reciblage marche sur tous les squelettes** :
   - la bibliothèque est reciblée sur chaque squelette humain, doigts compris, quelle que soit la convention de noms (Mixamo, Character Creator, Unreal, Rigify, et même les quadrupèdes debout) ;
   - 35 personnages sur 36 la reçoivent ;
   - le seul qui ne la reçoit pas est le marcheur blindé, qui a déjà ses 41 animations à lui.
3. **Les statues ont maintenant un squelette.** `outils/rigger-statues.py` (Blender en module Python) en a donné un aux 14 statues :
   - 8 sont réussies (le héros, 5 espèces, 2 androïdes) ;
   - 3 sont bonnes pour marcher et attendre, mais leurs bras se déforment dans les gestes amples : à refaire sur Mixamo ;
   - 3 ont échoué : ce sont des décors de toute façon.
4. **Les drones** ont un vol stationnaire fait par le code : ils montent, descendent et balaient la salle du regard.
5. **Les couleurs de la bible dans les fichiers** : `outils/recolorer-prologue.py` recolore les textures une fois pour toutes, avec le même calcul que l'atelier. Unity ouvre ainsi les modèles déjà aux couleurs du Colosse.
6. **13 espèces** rejoignent les détenus et les pirates :
   - des « gris », des sauriens, un crocodilien, un cératopsien ;
   - un grand alien rouge, un ichtyen à tête de poisson, un céphalopode, un mercenaire en armure ;
   - un spécimen biomécanique pour le décor du laboratoire.
   Un homme-oiseau en tenue de fantasy a été écarté.

![Les détenus, humains et autres espèces](personnages/rangee-detenu.jpg)
![Les autres espèces](personnages/rangee-espece.jpg)
![La sécurité : les androïdes](personnages/rangee-androide.jpg)
![Les pirates](personnages/rangee-pirate.jpg)

## 2. La bibliothèque d'animations

Deux fichiers, *Universal Animation Library* 1 et 2 de Quaternius, en version Standard (gratuite). Les fichiers `_RM`, avec le déplacement intégré, sont sur itch.io.

| Famille | Animations |
|---|---|
| Se déplacer | `Idle_Loop`, `Walk_Loop`, `Walk_Formal_Loop`, `Jog_Fwd_Loop`, `Sprint_Loop`, `Crouch_Idle_Loop`, `Crouch_Fwd_Loop`, `Walk_Carry_Loop`, `Roll`, `Slide_Start` / `_Loop` / `_Exit`, `ClimbUp_1m`, `Jump_Start` / `_Loop` / `_Land`, `NinjaJump_*`, `Swim_Idle_Loop`, `Swim_Fwd_Loop` |
| Attendre, parler | `Idle_Talking_Loop`, `Idle_FoldArms_Loop`, `Idle_No_Loop`, `Yes`, `Idle_Rail_Loop`, `Idle_Rail_Call`, `Idle_TalkingPhone_Loop`, `Idle_Lantern_Loop`, `Idle_Torch_Loop`, `Sitting_Enter` / `_Idle_Loop` / `_Talking_Loop` / `_Exit`, `Dance_Loop` |
| Agir | `Interact`, `PickUp_Table`, `Push_Loop`, `Chest_Open`, `Fixing_Kneeling`, `Consume`, `Driving_Loop`, `OverhandThrow`, `TreeChopping_Loop`, `Farm_*` |
| Tir | `Pistol_Idle_Loop`, `Pistol_Aim_Neutral` / `_Up` / `_Down`, `Pistol_Shoot`, `Pistol_Reload` |
| Arme intégrée des androïdes | `Spell_Simple_Enter` / `_Idle_Loop` / `_Shoot` / `_Exit` : le bras tendu, l'arme sort de l'avant-bras |
| Corps à corps | `Punch_Jab`, `Punch_Cross`, `Melee_Hook`, `Sword_*` (pour une barre de fer), `Shield_*` (un bouclier anti-émeute) |
| Encaisser, tomber | `Hit_Chest`, `Hit_Head`, `Hit_Knockback`, `Death01`, `LayToIdle` (se relever : **le réveil du héros**), `Zombie_*` (un détenu blessé qui se traîne) |

Pour le tir au fusil, il n'existe pas encore d'animation libre de bonne qualité. Les androïdes n'en ont pas besoin, puisque leur arme est dans l'avant-bras. Pour les pirates, Mixamo en a beaucoup (gratuit, compte Adobe).

## 3. Comment ça marche

**Dans le banc et l'atelier (navigateur)**, chaque animation est recalculée pour chaque squelette, en moins d'un dixième de seconde :
1. Les os comptent : bassin, colonne, cou, tête, clavicules, bras, avant-bras, mains, doigts, cuisses, mollets, pieds, orteils. Ils sont reconnus par leur nom, quelle que soit la convention :
   - Mixamo (`mixamorig:LeftArm`) ;
   - Character Creator (`CC_Base_L_Upperarm`) ;
   - Unreal (`upperarm_l`) ;
   - Rigify (`DEF-upper_arm.L`) ;
   - des noms maison (`L_elbow`, `b_L_front_leg_1`…).
2. On corrige la différence de pose de départ (T, A, bras baissés) et de direction du regard.
3. La position du bassin suit, à l'échelle de la taille de chacun.

**Dans Unity**, c'est encore plus simple : le système *Humanoid* recible tout seul.
1. Importer `UAL1_Standard.fbx` et `UAL2_Standard.fbx` : *Rig* → *Animation Type* : **Humanoid**.
2. Importer chaque personnage de la même façon (*Humanoid*, *Create From This Model*), après `normaliser-prologue.py`, `rigger-statues.py` et `recolorer-prologue.py`.
3. Dans l'*Animator Controller* de n'importe quel personnage, on glisse n'importe quelle animation de la bibliothèque.

## 4. Les statues qui ont reçu un squelette

`outils/rigger-statues.py` procède en quatre temps :
1. Il pose le squelette de la bibliothèque dans le modèle.
2. Il oriente les bras et les jambes :
   - bras écartés (pose en T ou en A) : vers la main la plus éloignée ;
   - bras le long du corps : il suit le bras tranche par tranche jusqu'à la main.
3. Il retire les socles et les armes tenues à la main.
4. Il calcule les poids :
   - **la chaleur des os** de Blender pour un corps, calculée sur une copie aux sommets fusionnés. La forme du corps compte, pas seulement la distance : le flanc ne suit plus le bras.
   - Les vêtements longs (manteau, cape) suivent le buste et le bassin, pas les jambes.
   - Un robot fait de pièces reçoit des poids **rigides** : chaque pièce suit un seul os, celui de son nom quand elle est nommée (`left_thigh`, ou `bedro`, « cuisse » en russe).

| Statue | Résultat |
|---|---|
| `tenue-detenu` (le héros) | Réussi : il marche, attend, vise |
| `gris-vert`, `longiligne-rouge`, `ichtyen`, `ceratopsien` | Réussi |
| `cephalopode`, `pirate-masque`, `saurien-guerrier` | Réussi ; un léger pli sous le bras quand il est levé très haut |
| `androide-medical`, `androide-lourd` | Réussi (quelques petites pièces flottent sur l'androïde lourd) |
| `pirate-chef` | Raté (son long manteau) : il reste une statue, le chef qui attend. Une version animable est à faire avec Meshy |
| `mutant-biomecanique` | Raté : il reste un spécimen de décor |
| `androide-soldat`, `androide-fantassin`, `androide-vieux-modele` | Pièces sans nom : ils restent des décors (androïdes en réparation) |

**Deux modèles sont cassés à la source** et sont retirés de la maquette : le pirate « Scavenger » et la combinaison étanche (`detenu-corvee-toxique`).
- Leurs squelettes empilent des échelles (×1000, ×0,001, ×71), et chaque pièce est dans un repère différent.
- Ni three.js ni Blender ne les affichent correctement.

Leurs remplaçants sont à faire avec Meshy : voir [`ia/PROMPTS-MESHY.md`](ia/PROMPTS-MESHY.md).

## 5. Personnage par personnage

| Personnage | Camp | Squelette | Bibliothèque | Faces | Verdict |
|---|---|---|---|---:|---|
| `tenue-detenu` | Héros | Fait par le script | Oui | 12 k | Provisoire, mais il s'anime |
| `detenu-brute` | Détenus | Character Creator, 102 os | Oui | 111 k | Bon ; à alléger |
| `detenu-vieux` | Détenus | Character Creator, 102 os | Oui | 54 k | Torse nu, à rhabiller (IA prévue) |
| `detenu-tshirt` | Détenus | Mixamo, 69 os | Oui | 19 k | **Le détenu de base** |
| `detenu-figurant` | Détenus | Mixamo, 58 os | Oui | 2 k | **La foule** |
| `detenu-corvee-toxique` | — | Cassé à la source | — | 55 k | **Retiré** : à refaire avec Meshy |
| `detenu-atelier` | — | Non humain | — | 35 k | **Retiré** : personnage d'un autre jeu |
| `garde` | — | Mixamo | — | 107 k | **Retiré** : garde humain |
| `droide-securite` | Androïdes | Character Creator, 84 os | Oui | 125 k | Garde des officiers ; à alléger |
| `androide-sentinelle` | Androïdes | Character Creator, 265 os | Oui | 28 k | **Le garde de base** |
| `androide-antiemeute` | Androïdes | Character Creator, 102 os | Oui | 20 k | Effacer « police » |
| `androide-patrouille` | Androïdes | Pièces animées | — | 68 k | Sa propre animation |
| `androide-medical`, `androide-lourd` | Androïdes | Fait par le script | Oui | 21 k, 241 k | Le lourd est à alléger |
| `androide-soldat`, `-fantassin`, `-vieux-modele` | Androïdes | Script : échec | — | 21-32 k | Décors |
| `drone-securite` | Androïdes | Aucun | Vol par le code | 11 k | Prêt |
| `marcheur-blinde` | Androïdes | Le sien, 21 os | Ses 41 animations | 11 k | Prêt |
| `pirate` | — | Cassé à la source | — | 99 k | **Retiré** : à refaire avec Meshy |
| `pirate-eva` | Pirates | Mixamo | Oui | 705 k | À alléger d'urgence |
| `pirate-chef` | Pirates | Statue | — | 242 k | Il attend, immobile ; à refaire avec Meshy pour l'animer |
| `pirate-masque` | Pirates | Fait par le script | Oui | 925 k | À alléger |
| `saurien-pale`, `crocodilien`, `gris-leger` | Espèces | Maison, quadrupède debout, Rigify | Oui | 7-26 k | Animés |
| `gris-marcheur`, `gris-anime`, `mercenaire-alien` | Espèces | Mixamo, Character Creator | Oui | 50-55 k | Animés |
| `gris-vert`, `longiligne-rouge`, `ichtyen`, `ceratopsien` | Espèces | Fait par le script | Oui | 5-290 k | Le cératopsien est à alléger |
| `saurien-guerrier`, `cephalopode` | Espèces | Fait par le script | Oui | 34 k, 36 k | Gestes amples à refaire sur Mixamo |
| `mutant-biomecanique` | Décor | Statue | — | 26 k | Spécimen de décor (une cuve Meshy est prévue) |

Les images de chacun, à plusieurs moments de ses animations, sont dans [`personnages/`](personnages/).

## 6. Où ils sont dans la maquette

La maquette (`outils/maquette-prologue.py`) ne pose plus aucune silhouette : partout de vrais modèles, pris à tour de rôle.

| Lieu | Sécurité | Détenus et pirates |
|---|---|---|
| Laboratoire | `androide-medical` | — |
| Anneau du cœur | 2 androïdes en ronde | — |
| Nef des cellules | `droide-securite` au poste de garde, 4 androïdes sur les galeries, 2 drones | Détenus humains et d'autres espèces (gris, crocodilien, saurien, ichtyen…) |
| Ateliers | `androide-soldat` et `androide-vieux-modele` en réparation | Détenus |
| Consigne, quartiers des officiers | 2 androïdes chacun | — |
| Salle des machines | 1 drone | Un pirate |
| Passerelle | 3 `droide-securite` | — |
| Hangar | 5 androïdes, `androide-lourd` à la porte, `marcheur-blinde`, 1 drone | Le chef pirate, des pirates humains et aliens (mercenaire, saurien, céphalopode) |

## 7. Ce qu'il reste à faire

| Qui | Quoi |
|---|---|
| Toi, sur le Mac | Relancer `outils/telecharger-prologue.sh` : il ajoute les androïdes, les espèces et la bibliothèque d'animations. Puis, dans l'ordre : `normaliser-prologue.py`, `rigger-statues.py` (`pip install bpy scipy`), `recolorer-prologue.py` (`pip install pillow numpy`). |
| Meshy (gratuit) | Les remplaçants et les pièces qui manquent : [`ia/PROMPTS-MESHY.md`](ia/PROMPTS-MESHY.md) (androïde de l'Empire, pirate, chef pirate animable, détenu de corvée, vieux détenu habillé…). |
| Mixamo (gratuit) | Des animations au fusil pour les pirates. |
| Le Claude du PC (Blender) | Corriger le squelette de la combinaison étanche ; alléger les modèles trop lourds ; effacer « police » et « security ». |
| L'IA 3D (avec ton jeton) | L'androïde de l'Empire à la forme exacte de la bible, et le vieux détenu rhabillé ([`ia/fiches.json`](ia/fiches.json)). |
