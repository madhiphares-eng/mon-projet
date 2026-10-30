"""Donne un squelette aux personnages qui n'en ont pas (les « statues »), pour qu'ils reçoivent les animations.

Le squelette est celui de la bibliothèque d'animations (Universal Animation Library de Quaternius, CC0) : un squelette humain
standard, que Unity reconnaît en Humanoid et que l'atelier sait recibler. Le script :
  1. pose ce squelette dans le modèle, à sa taille ;
  2. oriente les bras vers les mains et les jambes vers les pieds du modèle (pose en T, en A ou bras le long du corps) ;
  3. calcule les poids : « souple » pour un humain (chaque point suit les deux ou trois os les plus proches),
     « rigide » pour un robot (chaque pièce suit un seul os, sans se déformer) ;
  4. écrit un .glb avec le squelette, prêt pour les animations.

Il tourne avec Blender en module Python :
    pip install bpy scipy
    python outils/rigger-statues.py ~/jeu/prologue            # toutes les statues de la liste STATUES
    python outils/rigger-statues.py ~/jeu/prologue --seulement 03-Gardes/androide-medical

Le modèle d'origine est gardé en <nom>.statue.glb ; le modèle riggé prend sa place (<nom>.glb).
"""
import argparse
import os
import re
import shutil
import sys

import bpy
import numpy as np
from mathutils import Vector

# les statues à rigger : mode rigide pour les robots faits de pièces, souple pour les corps
STATUES = {
    '01-Joueur/tenue-detenu': 'souple',
    '03-Gardes/androide-medical': 'rigide',
    '03-Gardes/androide-lourd': 'rigide',
    '03-Gardes/androide-soldat': 'rigide',          # marche et garde ; des pièces décollent dans les gestes amples
    '03-Gardes/androide-vieux-modele': 'rigide',    # idem : une patrouille, sans arme tenue
    '04-Pirates/pirate-masque': 'souple',
    '04-Pirates/pirate-chef': 'souple',
    '16-Especes/mutant-biomecanique': 'souple',
    # les autres espèces : des corps, donc des poids souples
    '16-Especes/saurien-guerrier': 'souple',
    '16-Especes/ceratopsien': 'souple',
    '16-Especes/gris-vert': 'souple',
    '16-Especes/longiligne-rouge': 'souple',
    '16-Especes/cephalopode': 'souple',
    '16-Especes/ichtyen': 'souple',
}
# Reste sans squelette : androide-fantassin (1 800 pièces détachées sans nom, il s'éparpille ; un androïde en réparation
# dans un atelier). Pour l'animer : le refaire avec Meshy (docs/prologue/ia/PROMPTS-MESHY.md).
# les pièces qui ne font pas partie du corps : socles, armes tenues à la main (on donnera l'arme à part)
RETIRER = re.compile(r'(pcylinder|pedestal|socle|stand|base_?plate|gun|weapon|rifle|pistol)', re.I)
# les pièces nommées d'un robot : elles suivent l'os de leur nom, sans se déformer (anglais, et le russe d'un des modèles)
PIECES = [
    (r'thigh|bedro|upleg', 'thigh'), (r'toe', 'ball'), (r'ankle|foot|stopa', 'foot'),
    (r'shin|golen|calf|knee|nakolennik|(^|_)leg', 'calf'),
    (r'finger|palec|thumb|hand|kist', 'hand'), (r'forearm|lowerarm|predplech|lokot|elbow', 'lowerarm'),
    (r'upperarm|upper_arm|plecho|(^|_)arm', 'upperarm'), (r'shoulder|clavicle', 'clavicle'),
    (r'head|golova|helmet', 'Head'), (r'neck', 'neck_01'), (r'hips|pelvis|(^|_)taz|robo_root$', 'pelvis'),
    (r'lower_spine|root_spine', 'spine_01'), (r'upper_spine|turso|torso|chest|porshni|top_spine|lamp', 'spine_03'),
]
VETEMENTS = re.compile(r'(cape|cloak|coat|manteau|tissu|skirt|jupe|robe|poncho)', re.I)
MEMBRES = ('thigh', 'calf', 'foot', 'ball', 'hand', 'lowerarm', 'upperarm', 'clavicle')
SQUELETTE = os.path.join('00-Animations', 'UAL1_Standard.glb')
HAUTEUR_SQUELETTE = 1.829                  # le mannequin de la bibliothèque, en mètres
DEFORMANTS = ['pelvis', 'spine_01', 'spine_02', 'spine_03', 'neck_01', 'Head',
              'clavicle_l', 'upperarm_l', 'lowerarm_l', 'hand_l', 'clavicle_r', 'upperarm_r', 'lowerarm_r', 'hand_r',
              'thigh_l', 'calf_l', 'foot_l', 'ball_l', 'thigh_r', 'calf_r', 'foot_r', 'ball_r']


def vider():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def importer(chemin):
    avant = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=chemin)
    return [o for o in bpy.data.objects if o not in avant]


def squelette(chemin):
    """L'armature de la bibliothèque, sans son mannequin ni ses animations, sans les doigts."""
    objets = importer(chemin)
    arm = next(o for o in objets if o.type == 'ARMATURE')
    for o in objets:
        if o is not arm:
            bpy.data.objects.remove(o, do_unlink=True)
    arm.animation_data_clear()
    for a in list(bpy.data.actions):
        bpy.data.actions.remove(a)
    bpy.context.view_layer.objects.active = arm
    bpy.ops.object.mode_set(mode='EDIT')
    for b in list(arm.data.edit_bones):
        if b.name not in DEFORMANTS and b.name != 'root':
            arm.data.edit_bones.remove(b)
    bpy.ops.object.mode_set(mode='OBJECT')
    return arm


def corps(chemin):
    """Les maillages du modèle, transformations appliquées, sans les socles ni les armes."""
    objets = importer(chemin)
    maillages = [o for o in objets if o.type == 'MESH']
    def ancetres(o):
        while o:
            yield o.name
            o = o.parent
    garder = [o for o in maillages if not any(RETIRER.search(n) for n in ancetres(o))]
    bpy.ops.object.select_all(action='DESELECT')
    for o in maillages:
        o.select_set(True)
    bpy.context.view_layer.objects.active = maillages[0]
    bpy.ops.object.make_single_user(object=True, obdata=True)
    noms = {o: next((n for n in ancetres(o) if not n.startswith('Object_')), o.name) for o in garder}
    bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for o in objets:
        if o.type != 'MESH' or o not in garder:
            bpy.data.objects.remove(o, do_unlink=True)
    return [(o, noms[o]) for o in garder]


def piece_nommee(nom, centre):
    """L'os d'une pièce de robot d'après son nom, ou None."""
    bas = nom.lower()
    for motif, os_ in PIECES:
        if re.search(motif, bas):
            if os_ not in MEMBRES:
                return os_
            if re.search(r'left|(^|[_.])l([_.]|$)', bas):
                cote = 'l'
            elif re.search(r'right|(^|[_.])r([_.]|$)', bas):
                cote = 'r'
            else:
                cote = 'l' if centre[0] > 0 else 'r'
            return f'{os_}_{cote}'
    return None


def sommets(obj):
    n = len(obj.data.vertices)
    p = np.empty(n * 3, dtype=np.float32)
    obj.data.vertices.foreach_get('co', p)
    return p.reshape(n, 3)


def ajuster(arm, P):
    """Le squelette à la taille du modèle, puis les membres vers les mains et les pieds."""
    zmin, zmax = P[:, 2].min(), P[:, 2].max()
    H = zmax - zmin
    s = H / HAUTEUR_SQUELETTE
    cx, cy = (P[:, 0].min() + P[:, 0].max()) / 2, np.median(P[:, 1])
    arm.scale = (s, s, s)
    arm.location = (cx, cy, zmin)
    # l'échelle doit entrer dans les os : transform_apply ne touche que les objets sélectionnés (sans cela, sur un modèle
    # qui n'est pas en mètres, les os restaient 10 à 20 fois trop grands pour le calcul des poids, et les pieds suivaient la cuisse)
    bpy.ops.object.select_all(action='DESELECT')
    arm.select_set(True)
    bpy.context.view_layer.objects.active = arm
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.ops.object.mode_set(mode='EDIT')
    eb = arm.data.edit_bones
    for cote, signe in (('l', 1), ('r', -1)):
        # la main : le point le plus éloigné sur le côté, au-dessus des hanches
        epaule = eb['upperarm_' + cote].head.copy()
        haut = P[(P[:, 2] > zmin + 0.45 * H) & (P[:, 2] < zmin + 0.92 * H) & (signe * P[:, 0] > 0.12 * H)]
        ecarte = len(haut) and (signe * (haut[:, 0] - cx)).max() > 0.28 * H
        trace = None if ecarte else suivre_bras(P, signe, epaule, H, zmin, cx)
        if trace:
            # les bras le long du corps : on suit le bras tranche par tranche jusqu'à la main
            coude, poignet, main = trace
        elif len(haut):
            # les bras écartés (pose en T ou en A) : la main est le point le plus éloigné sur le côté
            i = np.argmax(signe * haut[:, 0] - 0.3 * np.abs(haut[:, 2] - epaule.z))
            main = Vector(haut[i])
            d = main - epaule
            l1, l2, l3 = (eb[n + '_' + cote].length for n in ('upperarm', 'lowerarm', 'hand'))
            total = l1 + l2 + l3
            coude = epaule + d * (l1 / total)
            poignet = epaule + d * ((l1 + l2) / total)
            coude.y += 0.02 * H                                  # un coude à peine plié, vers l'arrière
        if trace or len(haut):
            eb['upperarm_' + cote].tail = coude
            eb['lowerarm_' + cote].head = coude
            eb['lowerarm_' + cote].tail = poignet
            eb['hand_' + cote].head = poignet
            eb['hand_' + cote].tail = main
            eb['clavicle_' + cote].tail = epaule
        # le pied : le centre des points les plus bas de ce côté
        bas = P[(P[:, 2] < zmin + 0.08 * H) & (signe * P[:, 0] > 0)]
        if len(bas):
            cheville = Vector((np.median(bas[:, 0]), eb['foot_' + cote].head.y, zmin + 0.06 * H))
            hanche = eb['thigh_' + cote].head.copy()
            genou = hanche.lerp(cheville, 0.5)
            genou.y -= 0.01 * H                                  # un genou à peine plié, vers l'avant
            eb['thigh_' + cote].tail = genou
            eb['calf_' + cote].head = genou
            eb['calf_' + cote].tail = cheville
            pied = eb['foot_' + cote]
            decal = cheville - pied.head
            pied.head = cheville
            pied.tail = pied.tail + decal
            eb['ball_' + cote].head = pied.tail
            eb['ball_' + cote].tail = eb['ball_' + cote].tail + decal
    bpy.ops.object.mode_set(mode='OBJECT')
    return H


def suivre_bras(P, signe, epaule, H, zmin, cx):
    """Un bras qui pend le long du corps : à chaque hauteur, sous l'épaule, le groupe de points le plus à l'extérieur,
    séparé du torse par un vide, est le bras. Rend (coude, poignet, main), ou None si on ne trouve pas de bras séparé."""
    cote = P[signe * (P[:, 0] - cx) > 0.03 * H]
    pas, centres, precedent = 0.015 * H, [], None
    z = epaule.z - 0.05 * H
    while z > zmin + 0.25 * H:
        tranche = cote[np.abs(cote[:, 2] - z) < pas]
        z -= pas
        if len(tranche) < 6:
            break
        xs = np.sort(signe * (tranche[:, 0] - cx))
        ecarts = np.diff(xs)
        k = int(np.argmax(ecarts))
        if ecarts[k] < 0.012 * H:
            if precedent is None:
                continue                                         # encore collé au torse, sous l'aisselle
            break
        bras = tranche[signe * (tranche[:, 0] - cx) > xs[k] + ecarts[k] / 2]
        c = bras.mean(axis=0)
        if precedent is not None and np.linalg.norm(c[:2] - precedent[:2]) > 0.08 * H:
            break
        centres.append(c)
        precedent = c
    if len(centres) < 6:
        return None
    longueur = np.concatenate([[0], np.cumsum(np.linalg.norm(np.diff(np.array([np.array(epaule)] + centres), axis=0), axis=1))])
    def a(f):
        i = int(np.searchsorted(longueur, f * longueur[-1]))
        return Vector(([np.array(epaule)] + centres)[min(i, len(centres))])
    return a(0.45), a(0.85), a(1.0)


def distances(P, arm):
    """Distance de chaque point à chaque os déformant (segment tête → queue)."""
    D = np.empty((len(P), len(DEFORMANTS)), dtype=np.float32)
    for j, nom in enumerate(DEFORMANTS):
        b = arm.data.bones[nom]
        a, c = np.array(arm.matrix_world @ b.head_local), np.array(arm.matrix_world @ b.tail_local)
        ab = c - a
        t = np.clip(((P - a) @ ab) / max(ab @ ab, 1e-9), 0, 1)
        D[:, j] = np.linalg.norm(P - (a + t[:, None] * ab), axis=1)
    # un point d'un côté ne suit pas les os de l'autre côté
    x = P[:, 0]
    for j, nom in enumerate(DEFORMANTS):
        if nom.endswith('_l'):
            D[x < -0.02, j] = np.inf
        elif nom.endswith('_r'):
            D[x > 0.02, j] = np.inf
    return D


def poids_souples(D, H):
    """Chaque point suit les os vraiment proches de lui (au plus trois) : un os deux fois plus loin que le plus proche ne compte pas.
    Sans cela, le flanc suit le bras quand le bras se lève, et le torse s'étire."""
    d = np.maximum(D / H, 1e-3)
    proche = np.nanmin(np.where(np.isfinite(d), d, np.nan), axis=1, keepdims=True)
    W = 1.0 / d ** 6
    W[~np.isfinite(D)] = 0
    W[d > proche * 1.6 + 0.01] = 0
    garde = np.argsort(-W, axis=1)[:, :3]
    M = np.zeros_like(W)
    np.put_along_axis(M, garde, np.take_along_axis(W, garde, axis=1), axis=1)
    return M / np.maximum(M.sum(axis=1, keepdims=True), 1e-9)


def poids_rigides(obj, D, H):
    """Chaque pièce détachée suit un seul os ; une très grande pièce garde des poids souples."""
    from scipy.sparse import coo_matrix
    from scipy.sparse.csgraph import connected_components
    n = len(obj.data.vertices)
    e = np.empty(len(obj.data.edges) * 2, dtype=np.int64)
    obj.data.edges.foreach_get('vertices', e)
    e = e.reshape(-1, 2)
    g = coo_matrix((np.ones(len(e)), (e[:, 0], e[:, 1])), shape=(n, n))
    nb, etiquette = connected_components(g, directed=False)
    S = poids_souples(D, H)
    W = np.zeros_like(S)
    for k in range(nb):
        idx = np.where(etiquette == k)[0]
        if len(idx) > 0.6 * n:
            W[idx] = S[idx]
        else:
            W[idx, np.argmax(S[idx].sum(axis=0))] = 1.0
    return W, nb


def chaleur_des_os(obj, arm, H):
    """Les poids par la chaleur des os, calculés sur une copie dont les sommets en double sont fusionnés
    (la chaleur échoue sur les maillages faits de morceaux), puis reportés point par point sur le vrai maillage."""
    import bmesh
    from scipy.spatial import cKDTree
    copie = obj.copy()
    copie.data = obj.data.copy()
    bpy.context.collection.objects.link(copie)
    bm = bmesh.new()
    bm.from_mesh(copie.data)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=H * 0.002)
    bm.to_mesh(copie.data)
    bm.free()
    for m in list(copie.modifiers):
        copie.modifiers.remove(m)
    bpy.ops.object.select_all(action='DESELECT')
    copie.select_set(True)
    arm.select_set(True)
    bpy.context.view_layer.objects.active = arm
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    Wc = poids_actuels(copie)
    arbre = cKDTree(sommets(copie))
    _, plus_proche = arbre.query(sommets(obj))
    W = Wc[plus_proche]
    bpy.data.objects.remove(copie, do_unlink=True)
    return W


def poids_actuels(obj):
    """Les poids de chaque point pour chaque os déformant (tableau points × os)."""
    W = np.zeros((len(obj.data.vertices), len(DEFORMANTS)), dtype=np.float32)
    index = {g.index: DEFORMANTS.index(g.name) for g in obj.vertex_groups if g.name in DEFORMANTS}
    for v in obj.data.vertices:
        for g in v.groups:
            if g.group in index:
                W[v.index, index[g.group]] = g.weight
    return W


def groupes(obj, W):
    for j, nom in enumerate(DEFORMANTS):
        groupe = obj.vertex_groups.get(nom) or obj.vertex_groups.new(name=nom)
        q = np.round(W[:, j] * 20) / 20                           # par paliers de 0,05 : beaucoup plus rapide
        for valeur in np.unique(q[q > 0]):
            groupe.add(np.where(q == valeur)[0].tolist(), float(valeur), 'REPLACE')


def rigger(entree, sortie, bibliotheque, mode, chaleur=True):
    vider()
    arm = squelette(bibliotheque)
    pieces = corps(entree)
    P = np.concatenate([sommets(o) for o, _ in pieces])
    H = ajuster(arm, P)
    nommees = detachees = 0
    for o, nom in pieces:
        Po = sommets(o)
        D = distances(Po, arm)
        os_ = piece_nommee(nom, Po.mean(axis=0)) if mode == 'rigide' else None
        if os_ in DEFORMANTS:
            W = np.zeros((len(Po), len(DEFORMANTS)), dtype=np.float32)
            W[:, DEFORMANTS.index(os_)] = 1.0
            nommees += 1
        elif mode == 'rigide':
            W, nb = poids_rigides(o, D, H)
            detachees += nb
        elif VETEMENTS.search(nom + ' ' + o.name):
            o.vertex_groups.new(name='_vetement').add(list(range(len(Po))), 1.0, 'REPLACE')
            # un manteau ou une cape suit le buste et le bassin, pas les jambes : il ne s'étire pas quand on marche
            D2 = D.copy()
            D2[:, [j for j, n in enumerate(DEFORMANTS) if n.startswith(('thigh', 'calf', 'foot', 'ball'))]] = np.inf
            W = poids_souples(D2, H)
        else:
            W = poids_souples(D, H)
        groupes(o, W)
    bpy.ops.object.select_all(action='DESELECT')
    for o, _ in pieces:
        o.select_set(True)
    bpy.context.view_layer.objects.active = pieces[0][0]
    if len(pieces) > 1:
        bpy.ops.object.join()
    obj = bpy.context.view_layer.objects.active
    if mode == 'souple' and chaleur:
        # la « chaleur des os » de Blender : la forme du corps compte, pas seulement la distance (le flanc ne suit plus le bras)
        avant = poids_actuels(obj)
        marque = obj.vertex_groups.get('_vetement')
        vetement = np.zeros(len(obj.data.vertices), dtype=bool)
        if marque:
            for v in obj.data.vertices:
                vetement[v.index] = any(g.group == marque.index for g in v.groups)
        for g in list(obj.vertex_groups):
            obj.vertex_groups.remove(g)
        apres = chaleur_des_os(obj, arm, H)
        apres[vetement] = avant[vetement]                        # les vêtements longs gardent leurs poids de buste et de bassin
        for g in list(obj.vertex_groups):
            obj.vertex_groups.remove(g)
        mod = obj.modifiers.new('Squelette', 'ARMATURE')
        mod.object = arm
        obj.parent = arm
        groupes(obj, apres)
        vides = apres.sum(axis=1) < 1e-6
        if vides.any():                                          # là où la chaleur échoue, les poids de distance
            apres[vides] = avant[vides]
            for g in list(obj.vertex_groups):
                obj.vertex_groups.remove(g)
            groupes(obj, apres)
        detail_chaleur = f', chaleur des os ({100 * (1 - vides.mean()):.0f} % des points)'
    else:
        mod = obj.modifiers.new('Squelette', 'ARMATURE')
        mod.object = arm
        obj.parent = arm
        detail_chaleur = ''
    bpy.ops.export_scene.gltf(filepath=sortie, export_format='GLB', export_animations=False, export_skins=True, export_yup=True)
    detail = f'{nommees} pièces suivent l\'os de leur nom, {detachees} pièces détachées' if mode == 'rigide' else 'poids souples' + detail_chaleur
    return f'{len(P)} points, {H:.2f} unités de haut, {detail}'


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument('dossier', help='le dossier du prologue (celui qui contient 00-Animations, 01-Joueur…)')
    p.add_argument('--seulement', default='', help='les clés à faire, séparées par des virgules')
    a = p.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else sys.argv[1:])
    racine = os.path.expanduser(a.dossier)
    biblio = os.path.join(racine, SQUELETTE)
    if not os.path.exists(biblio):
        sys.exit(f'La bibliothèque d\'animations manque : {biblio} (outils/telecharger-prologue.sh la télécharge).')
    for cle, mode in STATUES.items():
        if a.seulement and cle not in a.seulement.split(','):
            continue
        nom = cle.split('/')[1]
        fichier = os.path.join(racine, *cle.split('/'), nom + '.glb')
        statue = fichier[:-4] + '.statue.glb'
        if not os.path.exists(fichier) and not os.path.exists(statue):
            print(f'  {cle:36s} absent')
            continue
        if not os.path.exists(statue):
            shutil.copy2(fichier, statue)
        try:
            print(f'  {cle:36s} {rigger(statue, fichier, biblio, mode)}')
        except Exception as e:
            print(f'  {cle:36s} erreur : {e}')


if __name__ == '__main__':
    main()
