# Échos d'Éther : dossier de préproduction (refonte)

> Version 3, 28 octobre 2026. Remplace `GDD.md`, gardé pour mémoire.
> Budget : **0 €**, seulement des assets et des outils gratuits utilisables dans un jeu vendu.
> Statut : **proposition à valider** (voir [Décisions à prendre](#decisions)).

## 1. Ce qui change

Le jeu repart de zéro. Il commence désormais par une évasion :

1. Le joueur **atterrit dans une base ennemie**.
2. Il **s'échappe** en traversant un territoire ouvert, comme dans une campagne, mais sans couloir imposé.
3. Il **vole une navette** et **saute** loin de la planète (hyperespace, ou un autre nom : voir les décisions).

**Mélange de références :**

| Référence | Ce qu'on lui emprunte |
|---|---|
| *Star Wars* | Matériel usé, base impériale, vol de vaisseau, saut spatial, hors-la-loi |
| *Dune* | Désert immense et hostile, soleil, bruit qui attire le danger, ver |
| *Helldivers* | Gameplay : tir à la 3e personne lourd et réaliste, chaos, appels de soutien, tir allié en coop |

**Style visuel : réaliste** (Unity 6 HDRP).

Comme avant, on emprunte des sensations, jamais des noms, des logos ou des designs protégés.

## 2. Ce que font les pros (synthèse des lectures)

### Préproduction et tranche verticale
- **La préproduction a une durée limitée.** Sinon, on l'itère sans fin. On y fait plusieurs prototypes de gameplay avant de s'engager.
- **Le prototype** répond à la question « est-ce amusant ? », avec des formes grises et sans graphismes.
- **La tranche verticale** répond à « sait-on le fabriquer ? ». C'est un petit morceau du jeu au niveau de qualité final : gameplay, graphismes, son, interface.
  - Elle sert de test pour sortir de la préproduction (talk GDC de Volition).
  - Règle courante : **couper du contenu avant de couper de la qualité.**
- **Documents courts** : Stone Librande (GDC) défend les **designs d'une page**, parce que presque personne ne lit au-delà de la première page.
  - 2 à 4 **piliers** servent de juge pour chaque décision.

### Monde ouvert pour une petite équipe
- Les témoignages de développeurs seuls disent la même chose : **le monde ouvert est le premier piège de scope.**
- Les solutions qui reviennent :
  - construire le monde avec des **blocs modulaires** réutilisables ;
  - rendre **un verbe** excellent (se déplacer, se cacher) avant d'ajouter du contenu ;
  - définir précisément ce que veut dire « ouvert » pour son propre jeu.
- **Chemin critique et liberté** :
  - de grandes zones d'exploration, avec un chemin principal plus étroit ;
  - des **repères visuels** visibles de loin (tours, antennes) ;
  - des barrières douces (relief, falaises) plutôt que des murs invisibles.

### Les ouvertures qui servent de modèle
| Jeu | Ce qu'on retient |
|---|---|
| *Zelda : Breath of the Wild*, le Plateau du Prélude | Une **zone ouverte fermée sur elle-même**, qui apprend tous les systèmes. On la quitte quand on a l'outil pour partir (la paravoile). Pour nous, **la navette joue ce rôle**. Les critiques la classent parmi les meilleurs didacticiels. |
| *Metal Gear Solid V : Ground Zeroes*, le camp Omega | **Une base ouverte**, découpée en secteurs, avec plusieurs routes. La discrétion et le combat sont deux options valables. Le terrain est limité, la partie courte, et on la rejoue souvent. |
| *Skyrim*, Helgen | **Prologue d'évasion** : un événement dramatique (le dragon) libère le prisonnier. Les enjeux sont posés tout de suite. Selon un ancien designer, environ 30 % des joueurs sont partis explorer au lieu de suivre la route : la liberté marche. |
| *No Man's Sky* | Le premier objectif est de **remettre un vaisseau en état pour quitter la planète**. Ce qui gêne les joueurs : rester bloqué sans ressources. Il faut toujours une solution de secours. |
| *Star Wars Outlaws* | Le passage du sol à l'espace a l'air continu. Les sauts relient orbite et orbite, et de **courtes cinématiques cachent les chargements**. Exactement ce qu'il nous faut pour le saut final. |
| *Dune : Awakening* | Les vers sont attirés par le mouvement sur le sable, avec une **jauge de vibration** à l'écran. C'est l'élément le plus salué par les critiques. |
| *Helldivers 2* | Le studio met la **crédibilité** avant tout : le tir allié en découle et force à réfléchir en combat. Le chaos émerge des systèmes, pas des scripts. |

## 3. Proposition de jeu

### Les 4 piliers
1. **S'en sortir** : chaque minute est une évasion, et on survit avec ce qu'on trouve.
2. **Crédible et lourd** : le réalisme des armes, du corps et des machines, à la manière d'Helldivers.
3. **Plusieurs chemins** : discrétion, assaut, diversion. Le monde réagit au bruit, comme dans Dune.
4. **Le désert est vivant** : chaleur, tempêtes, et ce qui dort sous le sable.

### Le prologue (première tranche verticale)
- **Durée** : 30 à 45 minutes la première fois.
- **Zone** : environ 1,5 km de côté. La base est au centre, entourée de désert et de canyons.
- **Mêmes règles que le reste du jeu** : c'est le Plateau du Prélude de notre jeu.

| Étape | Ce qui se passe | Ce que le joueur apprend |
|---|---|---|
| 1. L'arrivée | Le joueur atterrit dans la base. La raison est à choisir : capsule abattue, prisonnier, sacrifié par son propre camp. | Regarder, se déplacer |
| 2. Sortir | Court passage guidé : cellule ou épave, couloirs, première arme volée | S'accroupir, se cacher, le bruit, ramasser |
| 3. La base ouverte | **Trois clés à obtenir dans l'ordre qu'on veut** : le code du hangar (un officier), une cellule d'énergie pour la navette (le dépôt), couper le canon antiaérien (la centrale) | Discrétion ou combat, piratage, routes multiples |
| 4. Le désert | Une clé est hors de la base, dans le désert (épave, dépôt avancé). Le sable porte le bruit. | Traversée, gestion du bruit, survie, tempête |
| 5. Le hangar | Alerte générale, la navette décolle sous le feu | Combat sous pression, pilotage |
| 6. Le saut | Fuite en orbite, puis saut. Une cinématique courte cache le chargement, avant l'écran-titre. | Récompense émotionnelle |

**Règle de secours** (la leçon de No Man's Sky) : chaque clé a au moins deux façons d'être obtenue. On ne bloque jamais le joueur.

**Le saut ouvre la suite.** La navette volée devient le refuge du joueur et lui donne plus tard ses appels de soutien. Pendant le prologue, il est seul et sans soutien. C'est voulu : il n'a encore rien.

### Solo d'abord, coop ensuite
- Le prologue se joue **seul**.
- La coop (2 à 4 joueurs, à la Helldivers) vient après la tranche verticale.
- Le code est quand même écrit pour l'accueillir, pour ne pas tout réécrire plus tard.

## 4. Méthode de travail

| Phase | But | Fin de phase (on la juge sur) |
|---|---|---|
| **0. Concept** (cette semaine) | Valider ce dossier et les décisions | Les décisions sont prises, l'ancienne version est archivée |
| **1. Prototype** (2 à 3 semaines, formes grises) | Personnage, discrétion et bruit, 1 type d'ennemi (patrouille, alerte, poursuite), tir, décollage | **S'évader est amusant même sans graphismes** |
| **2. Tranche verticale** | Le prologue complet au niveau final : vrais modèles, lumière, effets, son, interface | 3 personnes le finissent sans aide et veulent la suite |
| **3. Production** | Les mondes après le saut, la coop | Selon un plan fait à la fin de la phase 2 |

Les durées sont indicatives : elles dépendent du temps que tu peux y passer.

**Règles :**
- Formes grises d'abord, graphismes ensuite.
- Une page par système.
- Une capture et un test à chaque étape.
- On coupe du contenu, jamais de la qualité.

### Qui fait quoi
| Qui | Rôle |
|---|---|
| **Toi** | Les décisions, le ressenti, les tests de jeu, les téléchargements qui demandent un compte |
| **Claude du cloud** (ici) | Design, documents, code C#, recherche, scripts de téléchargement, vérification des licences |
| **Claude du PC** (Unity MCP) | Construire les scènes dans Unity, tester en jeu, captures, réglages |
| **Claude du PC avec Blender MCP** (à installer) | Nettoyer, alléger et assembler les modèles 3D, retirer les logos |

## 5. Outils gratuits, IA comprise

### Générer des modèles 3D avec une IA (0 €, usage commercial)
| Outil | Verdict | Pourquoi |
|---|---|---|
| **Meshy**, version gratuite (site web) | ✅ Utilisable | Selon la documentation officielle, ce qu'on génère gratuitement est sous **CC BY 4.0** : jeu vendu permis, à condition de créditer Meshy. Crédits limités. Les modèles Meshy 6 et 7 ne se téléchargent pas en gratuit. |
| **TripoSG** (sur ton PC, open source) | ✅ Utilisable | Licence MIT, environ 8 Go de VRAM : tient sur ta RTX 3060. Il donne la forme, pas la texture. |
| **TripoSR** (sur ton PC) | ✅ Utilisable | Licence MIT, environ 6 Go de VRAM. Plus ancien, qualité moindre, mais rapide. |
| **Stable Fast 3D / SPAR3D** (sur ton PC) | ✅ Utilisable | Licence communautaire de Stability : gratuit tant que le jeu rapporte moins de 1 M$ par an. VRAM à vérifier. |
| **TRELLIS.2** (Microsoft) | ❌ Pas sur ton PC | Licence MIT et la meilleure qualité, mais il demande 16 à 24 Go de VRAM. |
| **Hunyuan3D** (Tencent) | ❌ Interdit en France | Sa licence exclut l'Union européenne, le Royaume-Uni et la Corée du Sud. Même problème à travers Blender MCP. |
| **Tripo**, version gratuite | ❌ À éviter | Les sources se contredisent. Sa page d'aide dit que l'usage commercial demande un abonnement payant. |
| **Rodin (Hyper3D)**, essai gratuit | ⚠️ Prototypes seulement | La licence de l'essai n'est pas claire. |
| **Unity AI** (générateur 3D dans l'éditeur) | ⚠️ Non fiable à 0 € | Points gratuits pendant la bêta, puis payant. |

**Il n'existe pas de connecteur claude.ai pour générer de la 3D** : j'ai cherché dans le registre. La bonne porte d'entrée est **Blender MCP**, un module gratuit qui laisse le Claude du PC piloter Blender. Il peut :
- chercher et importer des modèles Sketchfab et Poly Haven ;
- modéliser par script ;
- prendre des captures pour vérifier son travail.

Ses générateurs intégrés (Rodin, Hunyuan3D) sont à laisser de côté, pour les raisons du tableau.

**Ce qu'il faut savoir sur la 3D par IA :**
- Elle est très bien pour les **objets de décor** : caisses, débris, rochers, machines, armes de fond.
- Elle est moins bonne pour les **pièces principales** : le personnage, la navette, les ennemis proches. On les part d'un bon modèle gratuit, qu'on retouche.
- Ses maillages sont lourds et désordonnés : il faut les alléger dans Blender.

### La chaîne de fabrication proposée
1. **Image de concept** avec FLUX.1 [schnell] : licence Apache 2.0, tourne sur une RTX 3060 en version quantifiée, via ComfyUI.
2. **Modèle 3D** : TripoSG, Stable Fast 3D ou Meshy gratuit. Ou un modèle gratuit de Sketchfab, NASA ou Poly Haven.
3. **Blender**, piloté par Claude grâce à Blender MCP : alléger, réparer, textures PBR, retirer les logos.
4. **Squelette et animations** avec **Mixamo** (gratuit, usage commercial permis, humanoïdes seulement).
5. **Unity HDRP** : matériaux, LOD, test en jeu par le Claude du PC.

### Autres outils
- **Unity 6 HDRP**, **VFX Graph**, **Shader Graph**, **Cinemachine**, **Timeline**, **Terrain Tools** : gratuits.
- **Claude Code Game Studios** : boîte à outils open source (MIT) qui transforme Claude Code en « studio », avec des agents et des points de contrôle par phase. À évaluer avant de l'installer : c'est un projet d'une seule personne.

## 6. L'Arpenteur

Le personnage est **réaliste**. Voici les combinaisons gratuites déjà repérées (aperçus envoyés dans la conversation) :

| N° | Modèle | Licence | À savoir |
|---|---|---|---|
| 1 | [Spacesuit (Confederation of Planets)](https://sketchfab.com/3d-models/spacesuit-confederation-of-planets-da7b5c14bc634f8283e2d21343afa7cc) | CC BY (tnnv) | Prêt pour le jeu (26 000 faces), sans squelette |
| 2 | [Mercenary Astronaut](https://sketchfab.com/3d-models/mercenary-astronaut-7fd399d488ae4576a0fc62715892956c) | CC BY (better_peter) | Usé, armé, textures 4K, 3 animations. **Très lourd** (700 000 faces) : il faut l'alléger |
| 3 | [Astronaute animé](https://sketchfab.com/3d-models/animated-astronaut-character-in-space-suit-loop-8fe5c8d3365e4d87bb7bc253d53a64e1) | CC BY (LasquetiSpice) | Avec squelette, 4 animations |
| 4 | [Space Engineer F](https://sketchfab.com/3d-models/space-engineer-f-rigged-98282a36702941d3941941def844bd68) | CC BY (tegnemaskin) | Avec squelette, léger |
| 5 | [Exosquelette](https://sketchfab.com/3d-models/rigged-for-ue4ue5-exoskeleton-free-c0efb57e2fa940179ebaa58dcb9d0fa7) | CC BY (Andywoodhead) | Squelette pour Unreal, à adapter |
| 6 | [Mech Pilot](https://sketchfab.com/3d-models/mech-pilot-un-722b12227c634489a3436a18a07064d4) | CC BY (gueriero) | Très lourd (600 000 faces) |
| 7 | [Combinaison style Apollo](https://sketchfab.com/3d-models/space-suit-bdc42075279746dab4c8b54784276a20) | CC BY (IgorSholopov) | Classique, sans squelette |
| 8 | Z2 de la NASA | Domaine public | Celle de la vitrine. Sans squelette. Logos et drapeaux à retirer |

Sans squelette, le modèle passe par Mixamo, qui pose les os et les animations gratuitement.

## 7. Risques

| Risque | Parade |
|---|---|
| Scope du monde ouvert | Une zone de 1,5 km, terminée avant d'en commencer une autre |
| Effet patchwork : 50 auteurs, 50 styles | Nos propres matériaux, une palette par lieu, un étalonnage commun |
| Licences | Seulement CC0, CC BY, domaine public, MIT ou Apache. Chaque crédit est noté |
| Performances sur la RTX 3060 | Modèles allégés, LOD, objectif 1080p à 60 images/s |
| Assets stockés seulement sur le PC | Sauvegarde sur disque externe ou cloud |
| Hunyuan3D et autres licences piégées | Le tableau de la section 5 tient lieu de règle |

<a id="decisions"></a>
## 8. Décisions à prendre

1. **Pourquoi le joueur atterrit dans la base ennemie ?**
   - A. Sa capsule est abattue par la défense antiaérienne de la base.
   - B. Il est prisonnier, et un événement (ver, tempête, attaque) lui ouvre la porte.
   - C. Son propre camp l'a largué là en sacrifice, sous couvert d'une « mission glorieuse ». Il devient hors-la-loi en volant la navette.
   - On peut aussi mêler A et C.
2. **Le nom du saut** : « hyperespace » est un mot courant, donc permis. Pour un nom à nous :
   - le **Saut d'Éther** ;
   - **passer dans l'Écho** ;
   - **la Traversée**.
   - À éviter : « plier l'espace », qui vient de Dune.
3. **On garde le nom et l'univers ?** *Échos d'Éther*, l'Éther, la Concorde... ou on renomme tout ?
4. **Quel Arpenteur ?** Le numéro dans la planche d'aperçus.
5. **L'ancienne version** : je propose de l'archiver dans une branche Git (rien n'est perdu) et de repartir d'un dossier propre. On ne reprend un ancien système que s'il sert le nouveau jeu.

## Sources

Certains articles (gamedeveloper.com, itch.io) n'ont pas pu être ouverts depuis le serveur. Je me suis appuyé sur les résumés des moteurs de recherche.

- [GDC Vault : The Vertical Slice (Greg Donovan, Volition)](https://gdcvault.com/play/1022329/The-Vertical-Slice)
- [Game Developer : What you should take out of pre-production](https://www.gamedeveloper.com/game-platforms/what-you-should-take-out-of-pre-production)
- [Rami Ismail : Prototypes and vertical slice](https://ltpf.ramiismail.com/prototypes-and-vertical-slice/)
- [GDC : Six Months to a Playable Demo (Yeariver)](https://schedule.gdconf.com/session/six-months-to-a-playable-demo-building-aelwater-as-indie-devs-presented-by-yeariver/917020)
- [GDC Vault : One-Page Designs (Stone Librande)](https://gdcvault.com/play/1012356/One-Page)
- [Dear Clockmaker : postmortem d'un monde ouvert fait seul](https://dreamnoid.itch.io/dear-clockmaker/devlog/1065157/dear-clockmaker-postmortem)
- [Monde ouvert : interroger ses définitions (Tumblr)](https://tumblr-lukadjo-no-media.statichost.eu/posts/741127488807321600.html)
- [Chemin critique et zones ouvertes (gamedev.net)](https://gamedev.net/blogs/entry/2277800-should-players-be-hand-held)
- [Wikipédia : Great Plateau (Breath of the Wild)](https://en.wikipedia.org/wiki/Great_Plateau)
- [Ground Zeroes, critique Slant](https://www.slantmagazine.com/games/metal-gear-solid-v-ground-zeroes)
- [Ground Zeroes, analyse de fans (limites du camp)](https://metagearsolid.org/2014/05/metal-gear-solid-v-ground-zeroes-part-2b/3/)
- [Skyrim : quête Unbound](https://skyrim.fandom.com/wiki/Unbound_(quest))
- [Skyrim : 30 % des joueurs partent explorer (Bruce Nesmith)](https://otakukart.com/bruce-nesmith-says-30-of-skyrim-players-left-the-first-dungeon-to-explore-the-wilderness-instead-of-following-the-main-path/)
- [No Man's Sky : vaisseau à réparer au départ](https://www.gamerevolution.com/?p=581959)
- [Star Wars Outlaws : transitions sol-espace (Game Informer)](https://www.gameinformer.com/review/star-wars-outlaws/transitional-open-world-approach)
- [Star Wars Outlaws : planète vers espace sans coupure (GamingBolt)](https://gamingbolt.com/star-wars-outlaws-will-feature-completely-seamless-planet-to-space-traversal)
- [Dune : Awakening, critique TechRadar](https://techradar.com/gaming/dune-awakening-review)
- [Helldivers 2 : la crédibilité avant tout (PlayStation Blog)](https://blog.playstation.com/2024/03/12/helldivers-2-how-arrowhead-turned-the-twin-stick-shooter-original-into-a-bombastic-ps5-sequel/)
- [Helldivers 2 : émergence et chaos (Siliconera)](https://www.siliconera.com/helldivers-2-refines-arrowhead-games-style-of-emergent-comedy/)
- [Meshy : ce qui est inclus dans la version gratuite](https://intercom.help/meshy/en/articles/15696428-what-is-included-on-the-free-plan)
- [Meshy : usage commercial](https://intercom.help/meshy/en/articles/16102098-can-i-use-meshy-assets-commercially)
- [Tripo : usage commercial](https://www.tripo3d.ai/help/privacy-policy/how-to-use-tripo-models-commercially)
- [Licence Hunyuan3D 2.0](https://huggingface.co/tencent/Hunyuan3D-2/blob/main/LICENSE)
- [TRELLIS.2 (ComfyUI Wiki)](https://comfyui-wiki.com/en/news/2025-12-18-microsoft-trellis2-3d-generation)
- [Générateurs 3D open source en 2026 (Cinevva)](https://app.cinevva.com/guides/ai-3d-model-generators)
- [TripoSG (miroir du dépôt)](https://gitee.com/mirrors/TripoSG)
- [Licence Stability AI](https://stability.ai/license)
- [Rodin, essai gratuit (Resourify)](https://resourify.com/resources/hyper3d)
- [Unity AI : générateurs](https://docs.unity3d.com/Packages/com.unity.ai.generators@1.6/manual/index.html)
- [Blender MCP (ahujasid)](https://glama.ai/mcp/servers/ahujasid/blender-mcp)
- [FLUX.1 schnell, licence et RTX (NVIDIA)](https://blogs.nvidia.com/blog/ai-decoded-flux-one)
- [Mixamo : questions fréquentes (Adobe)](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html)
- [Claude Code Game Studios](https://www.mdskills.ai/skills/claude-code-game-studios)
