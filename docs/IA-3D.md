# Créer des modèles 3D avec l'IA (0 €)

> Guide pratique pour *Échos d'Éther*. Version 1, 28 octobre 2026. Les licences ont été vérifiées en octobre 2026 : les relire avant de publier le jeu.

## 1. À quoi sert l'IA chez nous

| Oui, avec l'IA | Non, autrement |
|---|---|
| **Les pièces uniques à notre univers**, qu'aucun site ne propose : la combinaison de Veilleur, les reliques et tablettes de glyphes, les cristaux d'Éther | Les animations : Mixamo (gratuit) |
| **Les modules de ruines des Bâtisseurs** (piliers, arches, monolithes), pour les semer sur des milliers de planètes | Les grands vaisseaux et les intérieurs : les modèles gratuits déjà choisis sont meilleurs |
| **La flore et les petits objets extraterrestres**, en beaucoup de variantes pour l'univers généré | Les rochers et les sols : les scans Poly Haven, déjà téléchargés, sont plus réalistes |

L'IA donne des **ébauches** : formes lourdes, maillage désordonné, textures parfois approximatives. Chaque modèle passe ensuite par Blender pour être nettoyé. C'est normal, c'est comme ça que les studios l'utilisent.

## 2. Les outils retenus

| Outil | Où | Licence | Pour quoi |
|---|---|---|---|
| **Meshy**, version gratuite | Site web (Mac ou PC) | CC BY 4.0 : jeu vendu permis, en créditant Meshy. Les créations gratuites sont publiques. Les modèles Meshy 6 et 7 ne se téléchargent pas en gratuit | Les premiers essais, avec texture, sans rien installer |
| **TripoSG** | Sur le PC (RTX 3060) | MIT : tout est permis | La forme seulement, en illimité. Environ 8 Go de mémoire graphique |
| **TripoSR** | Sur le PC | MIT | Plus ancien, plus rapide, environ 6 Go |
| **Stable Fast 3D** | Sur le PC | Licence communautaire de Stability : gratuit sous 1 M$ de revenus par an | Forme et texture en quelques secondes |
| **FLUX.1 [schnell]** | Sur le PC | Apache 2.0 : tout est permis | Les images de concept qui servent de point de départ |
| **ComfyUI** | Sur le PC | Logiciel libre | Le tableau de bord qui fait tourner FLUX et certains modèles 3D |
| **Blender + Blender MCP** | Sur le PC | Libres | Le Claude du PC nettoie, allège, re-texture et exporte les modèles |

**Écartés** :
- **Hunyuan3D** : sa licence exclut l'Union européenne.
- **Tripo gratuit** : l'usage commercial demande un abonnement.
- **Rodin** : la licence de l'essai n'est pas claire.
- **TRELLIS.2** : il demande 16 à 24 Go de mémoire graphique.
- **Unity AI** : payant après la bêta.

## 3. La chaîne de fabrication

1. **Le concept** : une image du modèle, de face, sur fond uni, sans décor. Elle peut être générée avec FLUX, dessinée ou tirée d'une photo. C'est ce qui fait 80 % de la qualité.
2. **La 3D** : TripoSG ou Stable Fast 3D sur le PC, ou Meshy sur le site.
3. **Le nettoyage dans Blender**, par le Claude du PC avec Blender MCP :
   - mise à l'échelle en mètres ;
   - allègement au budget (voir plus bas) ;
   - normales, coordonnées de texture ;
   - matériaux PBR avec nos textures ;
   - suppression des défauts.
4. **Le squelette**, pour un personnage : Mixamo.
5. **Unity** : import, niveaux de détail, collisions. Le modèle est ajouté au registre des crédits.

**Budget de faces** (RTX 3060) :
- personnage : moins de 60 000 ;
- objet tenu en main : moins de 15 000 ;
- module de ruine : moins de 20 000 ;
- plante : moins de 5 000, car il y en aura des milliers.

## 4. Les règles

- **Jamais de nom de franchise** dans une demande (pas de « style Star Wars », « comme Halo »…). On décrit des formes, des matières, une ambiance.
- **Un registre** (`docs/REGISTRE-IA.md`, à créer au premier modèle) pour chaque modèle : outil, version, image ou texte de départ, date, licence, crédit à afficher.
- **Meshy gratuit** : ajouter « Modèles 3D générés avec Meshy (meshy.ai), CC BY 4.0 » aux crédits du jeu.

## 5. Les premières pièces à fabriquer

| Priorité | Pièce | Pourquoi |
|---|---|---|
| 1 | La combinaison de Veilleur | La pièce signature du héros, visible tout le jeu |
| 2 | 3 cristaux d'Éther et 2 reliques à glyphes | Le prologue et la quête de mémoire |
| 3 | 6 modules de ruines des Bâtisseurs | À semer sur les planètes générées |
| 4 | 10 plantes extraterrestres | Pour peupler les biomes |

## 6. Les 9 modèles du prologue à refaire

La [bible du prologue](prologue/BIBLE-DA.md) (§ 12) en désigne neuf :
- **les personnages** : l’androïde de l’Empire (il remplace le garde humain : la sécurité n’est faite que d’androïdes), la combinaison du Veilleur, le vieux détenu ;
- **le mobilier et les machines** : la porte de cellule, le lit, la console ;
- **le reste** : la capsule du Veilleur, le vaisseau pirate, les débris de coque.

Leurs fiches sont dans [`prologue/ia/fiches.json`](prologue/ia/fiches.json). Chacune donne :
- le modèle remplacé ;
- le texte de l'image de concept, en anglais et sans nom d'œuvre ;
- la taille réelle ;
- le budget de faces.

**La chaîne, en une commande** : `outils/generer-modeles-ia.py`.
1. **L'image de concept** : FLUX.1 [schnell], licence Apache 2.0.
2. **La forme, puis la texture** : TripoSG, licence MIT.
3. **La mise à la taille réelle** : en mètres, pieds au sol.
4. **Le rangement** :
   - le modèle va dans `prologue-ia/` ;
   - une ligne s'ajoute au registre (`docs/REGISTRE-IA.md`) ;
   - le modèle entre dans le catalogue de l'atelier ;
   - la maquette est refaite : les nouveaux modèles prennent la place des anciens.

Le calcul se fait sur les espaces Hugging Face officiels de ces deux modèles, avec un GPU prêté gratuitement (ZeroGPU).
- **Sans compte, le quota est nul.** Il faut un jeton Hugging Face gratuit, en lecture, dans la variable d'environnement `HF_TOKEN`.
- **Le quota gratuit est de quelques minutes de GPU par jour**, soit 2 à 4 modèles. Quand il est épuisé, le script s'arrête proprement ; relancé le lendemain, il reprend où il en était.

```bash
pip install gradio_client trimesh numpy pillow
python3 outils/generer-modeles-ia.py --essai                  # ce qui reste à faire
HF_TOKEN=... python3 outils/generer-modeles-ia.py --sortie ~/jeu/prologue
```

**Après la génération** :
- `outils/telecharger-prologue.sh` (Mac) et `.ps1` (PC) téléchargent aussi les modèles faits, depuis le dépôt.
- Les trois personnages sortent sans squelette : il faut leur en donner un avec Mixamo avant Unity.
- Comme tout modèle fait par IA, ils passent ensuite par Blender pour être nettoyés (§ 3).

## 7. Qui fait quoi

| Qui | Quoi |
|---|---|
| Claude du cloud | Les fiches de concept (formes, matières, couleurs), les textes de départ, le registre, les budgets |
| Claude du PC | Installer ComfyUI et les modèles, générer, nettoyer dans Blender, importer dans Unity |
| Toi | Choisir parmi les propositions : c'est toi qui décides de ce qui est beau |

Sur le Mac, le plus simple reste **Meshy dans le navigateur**. Les modèles locaux sont faits pour la carte NVIDIA du PC.

## Sources

- [TripoSG (espace Hugging Face)](https://huggingface.co/spaces/VAST-AI/TripoSG)
- [FLUX.1 schnell (espace Hugging Face)](https://huggingface.co/spaces/black-forest-labs/FLUX.1-schnell)

- [Meshy : ce qui est inclus dans la version gratuite](https://intercom.help/meshy/en/articles/15696428-what-is-included-on-the-free-plan)
- [Meshy : usage commercial](https://intercom.help/meshy/en/articles/16102098-can-i-use-meshy-assets-commercially)
- [Tripo : usage commercial](https://www.tripo3d.ai/help/privacy-policy/how-to-use-tripo-models-commercially)
- [Licence Hunyuan3D 2.0](https://huggingface.co/tencent/Hunyuan3D-2/blob/main/LICENSE)
- [TRELLIS.2](https://comfyui-wiki.com/en/news/2025-12-18-microsoft-trellis2-3d-generation)
- [TripoSG (miroir du dépôt)](https://gitee.com/mirrors/TripoSG)
- [Licence Stability AI](https://stability.ai/license)
- [FLUX.1 schnell et RTX (NVIDIA)](https://blogs.nvidia.com/blog/ai-decoded-flux-one)
- [TripoSR dans ComfyUI sous Windows](https://mybyways.com/blog/triposr-image-to-3d-object)
- [Blender MCP](https://glama.ai/mcp/servers/ahujasid/blender-mcp)
