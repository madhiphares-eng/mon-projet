# Échos d'Éther — Game Design Document

> **Remplacé le 28 octobre 2026 par [`PREPRODUCTION.md`](../../docs/PREPRODUCTION.md) (refonte du jeu). Gardé pour mémoire.**

> Version 2.0 (reconstruite) — octobre 2026
> Moteur : Unity 6 · HDRP · Netcode for GameObjects · Cinemachine

---

## 1. Vision

**Échos d'Éther est avant tout un générateur d'émerveillement et de mystère.**
Le combat coopératif existe, mais il est au service de la découverte, pas l'inverse.

Le joueur doit ressentir ce que l'on ressent devant :
- le trou noir Gargantua d'*Interstellar* (échelle, vertige, temps qui se tord) ;
- la boucle de 22 minutes d'*Outer Wilds* (comprendre le monde est la vraie progression) ;
- le désert silencieux de *Journey* (solitude, beauté, rencontre muette) ;
- les Tombeaux du Temps d'*Hyperion* (artefacts qui remontent le temps) ;
- l'immensité hostile d'Arrakis dans *Dune* (le bruit attire la mort).

**Règle d'or de design** : à chaque fois qu'on hésite entre « plus de combat » et « plus de mystère », on choisit le mystère.

## 2. Pitch

Un Empire propagandiste — joyeux, bureaucratique, mensonger (ton satirique à la *Helldivers*) — largue des **Arpenteurs** sur des planètes lointaines pour récolter l'**Éther**, un cristal qui permet le saut hyperespace.

Ce que l'Empire ne dit pas : l'Éther est un **écho**. Chaque cristal contient un fragment de temps figé, et le récolter déchire lentement la trame des mondes. Plus les joueurs explorent, plus ils comprennent que leur mission est la cause du mal qu'ils combattent.

Ton : propagande lumineuse en surface ; mélancolie cosmique en dessous.

## 3. Boucle de jeu

1. **Briefing impérial** (vaisseau-mère, ton propagande, objectifs « officiels »).
2. **Largage** sur une planète (capsule, rentrée atmosphérique spectaculaire).
3. **Exploration** : terrain ouvert, phénomènes physiques étranges, ruines.
4. **Récolte d'Éther** : bruyante, donc dangereuse (voir jauge de bruit).
5. **Rencontres** : factions ennemies, phénomènes, énigmes environnementales.
6. **Extraction** — ou choix de rester pour percer un mystère, au prix du temps.
7. **Retour** : progression matérielle (équipement) **et** progression de savoir (journal, carte des mystères).

La progression principale est **la connaissance**, comme dans Outer Wilds : un joueur expérimenté est plus fort parce qu'il comprend, pas parce qu'il a de meilleures stats.

## 4. Mécaniques originales

### 4.1 Jauge de bruit (inspiration *Dune*)
- Chaque action produit du bruit : courir, tirer, forer, utiliser des moteurs.
- Le bruit se propage dans le sol et l'air ; il dépend du terrain (sable = porte loin, roche = étouffe).
- Au-delà de seuils, il attire la faune et les factions.
- Marche rythmique / déplacement silencieux = vraie stratégie.

### 4.2 Bulles temporelles (inspiration *Hyperion*)
- Zones où le temps s'écoule différemment (ralenti, accéléré, inversé).
- Projectiles figés, pluie qui remonte, ennemis vieillissant en secondes.
- Certaines énigmes ne se résolvent qu'en utilisant le temps local.

### 4.3 Trous noirs et dilatation du temps (inspiration *Interstellar*)
- Certaines planètes orbitent près d'un trou noir.
- Une heure à la surface = des jours/années sur le vaisseau-mère.
- Conséquences : messages qui arrivent « en retard », missions de l'Empire qui changent pendant qu'on est en bas, coéquipiers restés en orbite qui vieillissent (visuellement et narrativement).
- Ciel déformé par lentille gravitationnelle, disque d'accrétion visible en plein jour.

### 4.4 Gravité réglable
- Gravité par planète, et localement variable (anomalies).
- Saut, chute, physique des projectiles et des véhicules en dépendent.

### 4.5 Coop (inspiration *Helldivers*)
- 1 à 4 joueurs.
- Tir allié activé, appels de soutien orbitaux.
- Communication limitée dans certaines zones (interférences) : on se fait signe, comme dans Journey.

## 5. Factions ennemies

| Faction | Inspiration | Comportement | Lien avec le mystère |
|---|---|---|---|
| **Ruche des Sables** | Vers de Dune, insectes | Attirée par le bruit, surgit du sol, nuées | Protège les gisements d'Éther comme un système immunitaire planétaire |
| **Légion Mécanique** | Droïdes, automates | Tactique, patrouilles, réseaux de capteurs | Anciens Arpenteurs d'une civilisation précédente, encore en mission |
| **Chœur du Tombeau** | Gritche d'Hyperion | Rare, terrifiant, se déplace hors du temps normal | Gardien des échos ; ne se combat pas vraiment, s'évite ou se comprend |

## 6. Les six secteurs

Chaque secteur a **ses propres règles physiques et visuelles**.

1. **Lisière de l'Horizon** — planète au bord d'un trou noir. Ciel déformé, dilatation du temps, gravité forte. *(Phase 1 du prototype.)*
2. **Mer de Verre** — désert de sable vitrifié, jauge de bruit au cœur du gameplay, Ruche des Sables.
3. **Archipel des Marées** — planète-océan, vagues géantes périodiques (clin d'œil à la planète de Miller).
4. **Vallée des Tombeaux** — bulles temporelles, artefacts, Chœur du Tombeau.
5. **Forge Silencieuse** — monde industriel abandonné, Légion Mécanique, atmosphère quasi absente (son étouffé).
6. **Le Cœur de l'Écho** — secteur final, règles qui changent selon ce que les joueurs ont compris.

## 7. Direction artistique

- **Lumière avant tout** : soleils multiples, disques d'accrétion, aurores, brouillard volumétrique.
- **Échelle** : structures et phénomènes immenses ; le joueur est minuscule.
- **Silence** : moments sans musique, sons environnementaux, puis montée orchestrale/synthé (Hans Zimmer comme repère d'intention).
- **Palette** : chaque secteur a une couleur dominante identifiable.
- Moodboard : `docs/moodboard/`.

## 8. Références — ce qu'on emprunte (sans copier)

On emprunte des **sensations et principes**, jamais des noms, personnages, logos ou designs protégés.

| Référence | Ce qu'on prend | Ce qu'on évite |
|---|---|---|
| *Helldivers* | Coop chaotique, tir allié, satire de propagande | Noms, look des Helldivers, Super-Terre |
| *Star Wars / Mandalorian* | Western spatial, solitude du chasseur, matériel usé | Mandaloriens, sabres laser, beskar, noms |
| *Dune* | Danger du bruit, désert sacré, ressource rare | Vers des sables tels quels, Épice, noms |
| *Hyperion* | Tombeaux du temps, créature hors du temps, récits croisés | Gritche, noms de personnages |
| *Interstellar* | Trou noir réaliste, dilatation du temps, émotion de la distance | Gargantua, TARS, design exact |
| *Outer Wilds* | Progression par la connaissance, mystère systémique | Boucle temporelle identique |
| *Journey* | Communication muette, beauté du désert | Écharpe, design du voyageur |

## 9. Technique

- **Unity 6 + HDRP** : ciel physique, nuages volumétriques, brouillard volumétrique, éclairage réaliste.
- **Netcode for GameObjects** pour la coop (hôte-client au départ).
- **Cinemachine** pour caméras.
- **Gravité personnalisée** (pas la gravité globale d'Unity) pour supporter planètes et anomalies.
- **Lentille gravitationnelle** : passe plein écran (Custom Pass HDRP / Fullscreen Shader Graph).
- **Git + Git LFS** pour les assets lourds.
- Cible matérielle : PC Windows (DirectX 12) en priorité.

### 9.1 Machine de développement : RTX 3060 12 Go
Objectif : **1080p / 60 i/s** en qualité « Haute » et **1440p / 60 i/s** grâce à DLSS.

| Fonction HDRP | Réglage sur RTX 3060 | Pourquoi |
|---|---|---|
| Nuages volumétriques | Activés, qualité moyenne à haute | Cœur de l'émerveillement, coût maîtrisé |
| Brouillard volumétrique | Activé | Rayons de lumière du disque d'accrétion |
| Physically Based Sky | Activé | Ciels de planètes réalistes |
| Ray tracing — ombres | Soleil principal uniquement | Bon rapport beauté/coût |
| Ray tracing — reflets | Désactivé par défaut, SSR à la place | Trop coûteux en monde ouvert |
| Ray tracing — GI | Désactivé ; SSGI ou sondes APV | La 3060 ne tient pas le RTGI à 60 i/s |
| DLSS | Activé (mode Qualité) | Gain de 30 à 50 % sur les cartes RTX |
| Textures | Jusqu'à 4K sur les éléments proches | Les 12 Go de VRAM le permettent |

La 3060 sert de **référence « config recommandée »** : si le jeu tourne bien dessus, il tournera bien chez la majorité des joueurs PC.

## 10. Roadmap (24 phases)

### Bloc A — Prototype de l'émerveillement
1. Planète au bord d'un trou noir : ciel déformé, gravité réglable.
2. Personnage à la 3e personne + caméra Cinemachine.
3. Dilatation du temps (horloge locale vs horloge orbitale).
4. Cycle jour/nuit + disque d'accrétion comme source de lumière.

### Bloc B — Monde ouvert
5. Terrain de grande taille (streaming).
6. Méthode de génération planétaire (manuelle / procédurale / hybride — à trancher).
7. Météo et volumétrie.
8. Premier mystère environnemental jouable.

### Bloc C — Systèmes de jeu
9. Jauge de bruit.
10. Récolte d'Éther.
11. Arme(s) et combat de base.
12. Première faction : Ruche des Sables.

### Bloc D — Coop
13. Netcode : 2 joueurs.
14. Synchronisation physique et temporelle.
15. Appels de soutien orbitaux.
16. 4 joueurs + tir allié.

### Bloc E — Contenu
17. Bulles temporelles.
18. Légion Mécanique.
19. Chœur du Tombeau.
20. Secteurs 2 à 5.

### Bloc F — Narration et finition
21. Vaisseau-mère et propagande.
22. Journal de savoir / carte des mystères.
23. Secteur final « Cœur de l'Écho ».
24. Polish, optimisation, audio, tests joueurs.

## 11. Questions ouvertes

- Unity 6 est-il installé ?
- Génération des planètes : manuelle, procédurale ou hybride ?
- Jusqu'où pousser le réalisme physique (relativité « crédible » ou « exacte ») ?
