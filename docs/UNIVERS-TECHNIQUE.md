# L'univers sans fin : fiche technique

> Prototype 1 bis du dossier [`PREPRODUCTION.md`](PREPRODUCTION.md). Version 1, 28 octobre 2026.
> But : marcher sur une planète, décoller, sortir de l'atmosphère, voler vers une autre planète, passer en hyperespace vers un autre système, **sans écran de chargement visible**, à 60 images/s en 1080p sur une RTX 3060.

## 1. Les échelles

Tout naît d'une **graine** : un nombre dont on tire tout le reste. La même graine donne toujours le même résultat, pour tout le monde. On ne stocke rien, sauf ce que le joueur change.

| Niveau | Ce qu'il contient | Calculé à partir de |
|---|---|---|
| Univers | La graine maîtresse | Fixe |
| Galaxie | La position des systèmes dans un disque en spirale | Graine de l'univers + numéro de la galaxie |
| Système | Une étoile (type, couleur, taille), 1 à 8 planètes, lunes, ceintures d'astéroïdes, stations | Graine de la galaxie + coordonnées |
| Planète | Rayon, gravité, atmosphère, couleur du ciel, biomes, relief, océans | Graine du système + numéro de la planète |
| Morceau de terrain | Relief détaillé, roches, plantes, ruines | Graine de la planète + adresse du morceau |

## 2. Les façons de se déplacer

| Mode | Vitesse | Où |
|---|---|---|
| À pied | Environ 5 m/s | Au sol |
| Vol dans l'atmosphère | Jusqu'à environ 1 000 km/h | Près du sol, décollage et atterrissage |
| Vol spatial | Plusieurs km/s, avec une poussée rapide pour traverser un système en quelques dizaines de secondes | Entre l'orbite et les planètes d'un même système |
| **Hyperespace** | **La vitesse de la lumière, et au-delà** | Entre deux systèmes |

**L'hyperespace** : le joueur choisit un système sur la carte de la galaxie, puis son vaisseau accélère jusqu'à la vitesse de la lumière. Les étoiles s'étirent en traits, puis il entre dans un tunnel de lumière. Le trajet dure de quelques secondes à quelques dizaines de secondes selon la distance. Pendant ce temps, le système de départ est libéré de la mémoire et celui d'arrivée est généré en arrière-plan. **Le voyage lui-même cache le chargement.**

Dans l'histoire, c'est l'Éther (le temps figé) qui permet de dépasser la lumière. Au début, le vaisseau en brûle à chaque voyage ; plus tard, le joueur apprend à voyager par les relais des Bâtisseurs.

**Choix d'échelle** : un vrai système solaire est bien trop grand pour un jeu (150 millions de km entre la Terre et le Soleil). Comme *No Man's Sky*, on **compresse** : des planètes de quelques kilomètres de rayon, des distances de quelques milliers de kilomètres entre elles. L'image reste réaliste, mais les distances sont jouables.

## 3. Une planète

- **Forme** : un cube dont on gonfle les 6 faces en sphère. Chaque face est découpée en morceaux, de plus en plus fins près du joueur (un arbre à 4 branches, ou *quadtree*). Seul le terrain proche est détaillé.
- **Relief** : bruits fractals (collines), bruits « en crêtes » (montagnes), déformation du bruit (formes naturelles), cratères selon le type de planète, niveau de la mer.
- **Biomes** : température (latitude et altitude) × humidité donnent le biome. Les textures Poly Haven et ambientCG déjà téléchargées sont mêlées par un shader qui projette la texture sous trois angles.
- **Calcul** : Unity Jobs et Burst, en arrière-plan, quelques morceaux par image. Les collisions ne sont calculées qu'autour du joueur et de son vaisseau.
- **Ciel** : le ciel physique de HDRP, réglé pour chaque planète (rayon, couleur de l'air). L'étoile du système sert de soleil.
- **Gravité** : déjà écrite dans le projet (`SourceGravite`, `Planete.cs`).

## 4. La précision : l'origine flottante

Les nombres utilisés pour l'affichage perdent en précision loin de l'origine, et l'image se met à trembler. Donc :
- la position « dans l'univers » est gardée en double précision (système + position locale) ;
- l'affichage se fait autour du joueur : dès qu'il s'éloigne de plus de 2 km de l'origine, tout le monde est décalé pour le remettre au centre ;
- les planètes lointaines sont de simples sphères, et les étoiles des points lumineux.

## 5. Peupler l'infini

- **Placement déterministe** par morceau de terrain (points répartis régulièrement mais au hasard), avec la graine du morceau.
- **Instanciation par la carte graphique** et niveaux de détail : des milliers de roches et de plantes sans ralentir.
- **Un catalogue de pièces gratuites par biome** : rochers, falaises, plantes et textures déjà téléchargés.
- **Des lieux générés** : ruines mineures des Bâtisseurs, sites d'extraction de l'empire, épaves pirates, avec des souvenirs secondaires et des ressources.
- **Les mondes-clés** de l'histoire : une graine fixe, plus une scène faite à la main posée par-dessus.

## 6. La sauvegarde

Seulement les différences, rangées par graine et adresse : ce qui a été récolté, les ruines ouvertes, les découvertes, l'état du vaisseau.

## 7. Le plan du prototype, dans l'ordre

1. Une planète en formes grises (cube gonflé, morceaux de plus en plus fins). On mesure les images par seconde.
2. Le relief par bruit, des biomes simples, le ciel physique.
3. L'origine flottante, puis l'atterrissage depuis l'orbite avec la navette, en vol continu.
4. Une étoile et deux planètes dans un système, et le vol de l'une à l'autre.
5. La carte de la galaxie générée, et l'hyperespace vers un système généré.
6. Le peuplement (roches, plantes) et une ruine posée.

**Réussi si** : 60 images/s en 1080p sur la RTX 3060, aucune saccade de plus de 50 ms pendant l'atterrissage, et la même graine redonne toujours la même planète.

## 8. Qui fait quoi

| Qui | Quoi |
|---|---|
| Claude du cloud | Le générateur en C# : graines, bruits, découpage des planètes, système, galaxie, hyperespace |
| Claude du PC | L'intégration dans Unity, les mesures d'images par seconde, les captures, les réglages HDRP |
| Toi | Voler, atterrir, dire si c'est beau et si on a envie d'aller voir plus loin |

## 9. Ce qui existe déjà

- La gravité sphérique (`SourceGravite`, `Planete.cs`) et le vol de la navette (`Navette.cs`).
- La maquette web avait déjà une planète à niveaux de détail et un atterrissage en navette.

**Références** :
- [GDC : Continuous World Generation in No Man's Sky](https://gdcvault.com/play/1024265/Continuous-World-Generation-in-No) ;
- [GDC : Building Worlds Using Math(s)](https://gdcvault.com/play/1024514/Building-Worlds-Using) ;
- [Planètes à niveaux de détail et origine flottante dans Unity](https://discussions.unity.com/t/planetary-terrain-quadtree-planets-for-unity/682382) ;
- [Comparaison de générateurs de planètes (arXiv)](https://arxiv.org/html/2510.24764v1) ;
- la série vidéo *Procedural Planets* de Sebastian Lague.

Licence du code d'exemple à vérifier avant toute reprise.
