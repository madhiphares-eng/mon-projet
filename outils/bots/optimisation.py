#!/usr/bin/env python3
"""Échos d'Éther : le bot d'optimisation des modèles.

1. Il mesure chaque modèle du prologue (.glb) : faces, textures (nombre, plus grande, mémoire), poids du fichier,
   et le compare au budget de sa catégorie (un personnage : 60 000 faces, des textures de 2048 au plus).
2. Avec --appliquer (Blender en module Python : pip install bpy), il allège ceux qui dépassent :
   - les maillages sont simplifiés (Decimate, en gardant les poids de la peau et les coutures des textures),
     jusqu'au budget, jamais sous 25 % de l'original ;
   - les textures plus grandes que le budget sont réduites (par moitiés) ;
   - l'original est gardé à côté (<nom>.avant-optimisation.glb) : relancer repart toujours de lui.
   Ensuite, repasser les bots (mouvements, prises) sur les modèles allégés vérifie que rien n'est cassé.

    python3 outils/bots/optimisation.py ~/jeu/prologue                  # le rapport (sans Blender)
    python3 outils/bots/optimisation.py ~/jeu/prologue --appliquer --seulement pirate-eva,pirate-masque
    python3 outils/bots/optimisation.py ~/jeu/prologue --profil web --sortie /tmp/web   # des copies légères à part
Le profil « web » (la vitrine 3D en ligne) vise 40 000 faces et des textures de 1024 par personnage, et 15 Mo par
fichier ; avec --sortie, les copies allégées sont écrites dans ce dossier (même arborescence), les originaux ne
changent pas.
Le rapport est court : une ligne par modèle hors budget, puis le total. --tout montre aussi ceux qui vont bien,
--json écrit le détail dans un fichier.
"""
import argparse
import json
import os
import shutil
import struct
import sys

# le budget de chaque catégorie, d'après le dossier du modèle : faces, côté de texture, Mo
BUDGETS = {
    'personnage': (60000, 2048, 40), 'arme': (15000, 2048, 15), 'accessoire': (20000, 2048, 15),
    'decor': (80000, 2048, 40), 'grand-decor': (150000, 4096, 80), 'vaisseau': (250000, 4096, 120),
}
CATEGORIE = {'01': 'personnage', '02': 'personnage', '03': 'personnage', '04': 'personnage', '16': 'personnage',
             '13': 'arme', '14': 'accessoire', '05': 'decor', '06': 'decor', '07': 'decor', '09': 'decor',
             '08': 'grand-decor', '10': 'grand-decor', '12': 'grand-decor', '15': 'grand-decor', '11': 'vaisseau'}
# le profil web (la vitrine 3D en ligne : 15 Mo au plus par fichier)
BUDGETS_WEB = {k: (min(f, 40000), 1024, 12) for k, (f, c, m) in BUDGETS.items()}
# les kits (beaucoup de pièces dans un fichier) : un budget de grand décor
KITS = {'kit-couloir', 'kit-interieur', 'couloir-principal', 'bloc-cellules', 'hangar', 'sas', 'tuyaux', 'cables'}


def lire_glb(chemin):
    with open(chemin, 'rb') as f:
        magie, version, longueur = struct.unpack('<III', f.read(12))
        if magie != 0x46546C67:
            raise ValueError('pas un .glb')
        gltf, binaire = None, b''
        while f.tell() < longueur:
            n, t = struct.unpack('<II', f.read(8))
            d = f.read(n)
            if t == 0x4E4F534A:
                gltf = json.loads(d)
            elif t == 0x004E4942:
                binaire = d
    return gltf, binaire


def taille_image(octets):
    """(largeur, hauteur) d'une image PNG ou JPEG, d'après son en-tête."""
    if octets[:8] == b'\x89PNG\r\n\x1a\n':
        return struct.unpack('>II', octets[16:24])
    if octets[:2] == b'\xff\xd8':
        i = 2
        while i < len(octets) - 9:
            if octets[i] != 0xFF:
                i += 1
                continue
            m = octets[i + 1]
            if m in (0xC0, 0xC1, 0xC2):
                h, w = struct.unpack('>HH', octets[i + 5:i + 9])
                return w, h
            i += 2 + struct.unpack('>H', octets[i + 2:i + 4])[0]
    return None


def mesurer(chemin):
    gltf, binaire = lire_glb(chemin)
    acc = gltf.get('accessors', [])
    # les faces : chaque maillage autant de fois que des nœuds l'utilisent
    usages = {}
    for n in gltf.get('nodes', []):
        if 'mesh' in n:
            usages[n['mesh']] = usages.get(n['mesh'], 0) + 1
    faces = 0
    for i, m in enumerate(gltf.get('meshes', [])):
        for p in m.get('primitives', []):
            if p.get('mode', 4) != 4:
                continue
            k = p['indices'] if 'indices' in p else p['attributes'].get('POSITION')
            if k is not None:
                faces += acc[k]['count'] // 3 * usages.get(i, 0)
    images = []
    for im in gltf.get('images', []):
        octets = None
        if 'bufferView' in im:
            bv = gltf['bufferViews'][im['bufferView']]
            octets = binaire[bv.get('byteOffset', 0):bv.get('byteOffset', 0) + bv['byteLength']]
        elif 'uri' in im and not im['uri'].startswith('data:'):
            p = os.path.join(os.path.dirname(chemin), im['uri'])
            if os.path.exists(p):
                octets = open(p, 'rb').read(65536)
        t = taille_image(octets) if octets else None
        if t:
            images.append(t)
    memoire = sum(w * h * 4 * 4 / 3 for w, h in images) / 2 ** 20      # Mo en mémoire, mipmaps compris
    peau = bool(gltf.get('skins'))
    return {'faces': faces, 'textures': len(images), 'plusGrande': max((max(t) for t in images), default=0),
            'memoireTextures': round(memoire), 'mo': round(os.path.getsize(chemin) / 2 ** 20, 1), 'peau': peau,
            'animations': len(gltf.get('animations', []))}


def modeles(racine, seulement):
    for dossier in sorted(os.listdir(racine)):
        if dossier[:2] not in CATEGORIE:
            continue
        for nom in sorted(os.listdir(os.path.join(racine, dossier))):
            chemin = os.path.join(racine, dossier, nom, nom + '.glb')
            if os.path.exists(chemin) and (not seulement or nom in seulement):
                yield dossier, nom, chemin


def budget(dossier, nom, table=BUDGETS):
    cat = 'grand-decor' if nom in KITS else CATEGORIE[dossier[:2]]
    return cat, table[cat]


def appliquer(chemin, faces_voulues, cote_max, mesure, sortie=None, web=False):
    """Allège un modèle avec Blender : simplifie les maillages et réduit les textures. Repart de l'original ;
    avec sortie, écrit la copie allégée là, sans toucher au fichier."""
    import bpy
    original = chemin[:-4] + '.avant-optimisation.glb'
    if sortie:
        original = original if os.path.exists(original) else chemin
    elif not os.path.exists(original):
        shutil.copy2(chemin, original)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=original)
    ratio = max(0.25, min(1.0, faces_voulues / max(1, mesure['faces'])))
    sautes = []
    if web:
        # la vitrine n'anime pas les visages : les formes (morph targets) partent, elles pèsent souvent plus que le reste
        for o in [o for o in bpy.data.objects if o.type == 'MESH' and o.data.shape_keys]:
            o.shape_key_clear()
    if ratio < 0.999:
        for o in [o for o in bpy.data.objects if o.type == 'MESH']:
            if o.data.shape_keys:
                sautes.append(o.name)                       # les formes (visage, muscles) bloquent la simplification
                continue
            mod = o.modifiers.new('Optimisation', 'DECIMATE')
            mod.decimate_type = 'COLLAPSE'
            mod.ratio = ratio
            mod.use_collapse_triangulate = True
            # les coutures des textures et les bords restent en place (sinon les textures glissent)
            mod.delimit = {'UV', 'SEAM'} if hasattr(mod, 'delimit') else set()
            bpy.context.view_layer.objects.active = o
            # le squelette reste au-dessus de la simplification
            while o.modifiers[0] != mod:
                bpy.ops.object.modifier_move_up(modifier=mod.name)
            bpy.ops.object.modifier_apply(modifier=mod.name)
    reduites = 0
    for im in bpy.data.images:
        w, h = im.size
        while max(w, h) > cote_max:
            w, h = w // 2, h // 2
        if (w, h) != tuple(im.size) and w > 0:
            im.scale(w, h)
            im.pack()
            reduites += 1
    cible = sortie or chemin
    os.makedirs(os.path.dirname(cible), exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=cible, export_format='GLB', export_skins=True, export_animations=True,
                              export_yup=True, export_image_format='AUTO', export_morph=not web)
    return ratio, reduites, sautes


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument('dossier', help='le dossier du prologue (celui qui contient 01-Joueur, 02-Prisonniers…)')
    p.add_argument('--appliquer', action='store_true', help='alléger les modèles hors budget (Blender)')
    p.add_argument('--seulement', default='', help='les noms des modèles, séparés par des virgules')
    p.add_argument('--tout', action='store_true', help='montrer aussi les modèles dans leur budget')
    p.add_argument('--json', default='', help='écrire le détail dans ce fichier')
    p.add_argument('--profil', default='jeu', choices=['jeu', 'web'], help='les budgets : du jeu, ou de la vitrine 3D en ligne')
    p.add_argument('--sortie', default='', help='écrire les copies allégées dans ce dossier (les originaux ne changent pas)')
    a = p.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else sys.argv[1:])
    racine = os.path.expanduser(a.dossier)
    seulement = set(filter(None, a.seulement.split(',')))
    detail, hors, total = [], 0, {'faces': 0, 'mo': 0, 'memoireTextures': 0}
    for dossier, nom, chemin in modeles(racine, seulement):
        try:
            m = mesurer(chemin)
        except Exception as e:
            print(f'  {nom:26s} illisible : {e}')
            continue
        cat, (fmax, cmax, momax) = budget(dossier, nom, BUDGETS_WEB if a.profil == 'web' else BUDGETS)
        copie = os.path.join(os.path.expanduser(a.sortie), dossier, nom, nom + '.glb') if a.sortie else None
        defauts = []
        if m['faces'] > fmax:
            defauts.append(f"{m['faces'] // 1000} k faces (budget {fmax // 1000} k)")
        if m['plusGrande'] > cmax:
            defauts.append(f"textures de {m['plusGrande']} (budget {cmax})")
        if m['mo'] > momax:
            defauts.append(f"{m['mo']:.0f} Mo (budget {momax})")
        for k in total:
            total[k] += m[k]
        detail.append({'cle': f'{dossier}/{nom}', 'categorie': cat, **m, 'defauts': defauts})
        if defauts:
            hors += 1
        if defauts or a.tout:
            print(f"  {nom:26s} {cat:11s} " + ('; '.join(defauts) if defauts else f"ok ({m['faces'] // 1000} k faces)"))
        if copie and not defauts:
            os.makedirs(os.path.dirname(copie), exist_ok=True)
            shutil.copy2(chemin, copie)                         # dans son budget : copié tel quel
        if defauts and (a.appliquer or copie):
            ratio, reduites, sautes = appliquer(chemin, fmax, cmax, m, copie, a.profil == 'web')
            apres = mesurer(copie or chemin)
            print(f"    → allégé : {m['faces'] // 1000} k → {apres['faces'] // 1000} k faces, {reduites} texture(s) réduite(s), "
                  f"{m['mo']:.0f} → {apres['mo']:.0f} Mo" + (f" ; laissés tels quels (formes) : {', '.join(sautes)}" if sautes else ''))
            detail[-1]['apres'] = apres
    print(f"optimisation : {len(detail)} modèles, {hors} hors budget ; en tout {total['faces'] / 1e6:.1f} M faces, "
          f"{total['mo']:.0f} Mo de fichiers, {total['memoireTextures'] / 1024:.1f} Go de textures en mémoire")
    if a.json:
        os.makedirs(os.path.dirname(os.path.abspath(a.json)), exist_ok=True)
        json.dump(detail, open(a.json, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)


if __name__ == '__main__':
    main()
