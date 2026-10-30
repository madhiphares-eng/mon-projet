"""Échos d'Éther : le studio d'animation. Fabrique les animations propres au jeu, sur le squelette de la bibliothèque.

La bibliothèque libre (Quaternius, CC0) ne sait pas tenir un fusil, ni marcher comme une machine, ni lever les mains.
Ce script compose ces animations à partir des siennes :
  - le bas du corps vient d'une animation de la bibliothèque (marche, course, attente, accroupi, assis) ;
  - le haut du corps est retouché pose par pose : raidi, tourné, affaissé ; les mains sont placées par cinématique
    inverse (deux os, coude orienté), orientées (paume, doigts) et refermées (poignée, garde, poing, main ouverte) ;
  - chaque image est enregistrée comme une vraie animation, puis exportée en GLB (atelier, banc) et en FBX (Unity, Humanoid).

Le fusil est tenu à droite. La main droite est sur la poignée, la gauche sous le garde-main, 30 cm devant : dans Unity,
le fusil s'attache à l'os hand_r et la main gauche tombe d'elle-même sur le garde-main.

Usage (Blender en module Python, comme rigger-statues.py) :
    pip install bpy pillow
    python outils/studio-animations.py                          # écrit prologue-anim/00-Animations/EE_Animations.glb et .fbx
    python outils/studio-animations.py --seulement Fusil_Visee,Androide_Panne
    python outils/studio-animations.py --planches dossier/       # et une planche de contrôle (face et profil) par animation
"""
import argparse
import math
import os
import random
import sys

import bpy
from mathutils import Matrix, Quaternion, Vector

RACINE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DOSSIER = os.path.join(RACINE, 'prologue-anim', '00-Animations')
BIBLIO = [os.path.join(DOSSIER, 'UAL1_Standard.glb'), os.path.join(DOSSIER, 'UAL2_Standard.glb')]
SORTIE = os.path.join(DOSSIER, 'EE_Animations')
IPS = 30                                   # images par seconde des animations fabriquées

# dans l'espace de l'armature : le personnage regarde vers -Y, sa gauche est +X, le haut est +Z.
# Une rotation positive autour de GAUCHE penche vers l'avant ; autour de HAUT, tourne à gauche ; autour d'AVANT, incline à droite.
AVANT, GAUCHE, HAUT = Vector((0, -1, 0)), Vector((1, 0, 0)), Vector((0, 0, 1))
COLONNE = ['spine_01', 'spine_02', 'spine_03', 'neck_01', 'Head']
BRAS = ['clavicle_l', 'upperarm_l', 'lowerarm_l', 'hand_l', 'clavicle_r', 'upperarm_r', 'lowerarm_r', 'hand_r']
JAMBES = ['thigh_l', 'calf_l', 'foot_l', 'ball_l', 'thigh_r', 'calf_r', 'foot_r', 'ball_r']
COTES = (('l', 1), ('r', -1))              # le côté, et le signe de sa direction (gauche +1, droite -1)

# la paume, dans le repère de chaque main (mesurée sur les poings de la bibliothèque : les doigts se replient vers elle)
PAUME = {'l': Vector((1, 0, 0)), 'r': Vector((-1, 0, 0))}
# les prises : la flexion (degrés) des trois phalanges de chaque doigt, autour de l'axe x de la phalange
PRISES = {
    'poignee': {'index': (12, 20, 10), 'middle': (62, 78, 55), 'ring': (66, 80, 55), 'pinky': (70, 82, 55)},
    'garde':   {'index': (48, 58, 38), 'middle': (54, 64, 44), 'ring': (58, 66, 46), 'pinky': (62, 68, 48)},
    'poing':   {'index': (78, 88, 62), 'middle': (80, 90, 62), 'ring': (82, 90, 62), 'pinky': (84, 90, 62)},
    'ouverte': {'index': (5, 6, 3), 'middle': (4, 5, 3), 'ring': (6, 8, 4), 'pinky': (8, 10, 6)},
    'molle':   {'index': (18, 26, 16), 'middle': (24, 32, 18), 'ring': (28, 36, 20), 'pinky': (32, 40, 22)},
}


# ------------------------------------------------------------------ la scène
def preparer():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.render.fps = IPS
    bpy.ops.import_scene.gltf(filepath=BIBLIO[0])
    arm = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
    mannequin = next((o for o in bpy.data.objects if o.type == 'MESH' and o.name.startswith('Mannequin')), None)
    for f in BIBLIO[1:]:
        bpy.ops.import_scene.gltf(filepath=f)              # on ne garde que ses animations
    for o in list(bpy.data.objects):
        if o not in (arm, mannequin):
            bpy.data.objects.remove(o, do_unlink=True)
    # le mannequin est mis de côté pendant le travail (plus rapide), et remis pour l'export
    if mannequin:
        for c in list(mannequin.users_collection):
            c.objects.unlink(mannequin)
    if arm.animation_data is None:
        arm.animation_data_create()
    return arm, mannequin


def maj():
    bpy.context.view_layer.update()


# ------------------------------------------------------------------ lire une animation de la bibliothèque
def action(nom):
    a = bpy.data.actions.get(nom)
    if a is None:
        raise KeyError(f'animation introuvable dans la bibliothèque : {nom}')
    return a


def duree(nom):
    debut, fin = action(nom).frame_range
    return (fin - debut) / IPS


def echantillon(arm, nom, t, boucle=True):
    """La pose (rotations et positions locales de chaque os) de l'animation nom au temps t (secondes)."""
    a = action(nom)
    debut, fin = a.frame_range
    d = (fin - debut) / IPS
    if boucle and d > 0:
        t = t % d
    f = debut + min(t, d) * IPS
    arm.animation_data.action = a
    arm.animation_data.action_slot = a.slots[0]           # les animations de la 2e bibliothèque ont leur propre case
    bpy.context.scene.frame_set(int(f), subframe=f - int(f))
    pose = photographier(arm)
    arm.animation_data.action = None
    return pose


def photographier(arm):
    return {pb.name: (pb.rotation_quaternion.copy(), pb.location.copy()) for pb in arm.pose.bones}


def poser(arm, pose, os_=None):
    for pb in arm.pose.bones:
        if (os_ is None or pb.name in os_) and pb.name in pose:
            pb.rotation_quaternion, pb.location = pose[pb.name]
    maj()


def melanger(arm, pose, os_, f):
    """Rapproche ces os de la pose donnée (f = 1 : la pose donnée ; 0 : rien ne change)."""
    for n in os_:
        pb = arm.pose.bones[n]
        q, l = pose[n]
        pb.rotation_quaternion = pb.rotation_quaternion.slerp(q, f)
        pb.location = pb.location.lerp(l, f)
    maj()


# ------------------------------------------------------------------ retoucher la pose, dans l'espace de l'armature
def tete_de(arm, n):
    return arm.pose.bones[n].head.copy()


def tourner(arm, n, axe, degres):
    """Tourne l'os n (et ses enfants) autour d'un axe de l'armature, à sa tête."""
    if abs(degres) < 1e-4:
        return
    pb = arm.pose.bones[n]
    p = pb.head.copy()
    r = Matrix.Rotation(math.radians(degres), 4, Vector(axe).normalized())
    pb.matrix = Matrix.Translation(p) @ r @ Matrix.Translation(-p) @ pb.matrix
    maj()


def axe_de(arm, n, i):
    """L'axe local i (0 : gauche, 1 : le long de l'os, 2 : avant, pour la colonne et la tête) de l'os n."""
    return (arm.pose.bones[n].matrix.to_3x3() @ Vector([1 if k == i else 0 for k in range(3)])).normalized()


def deplacer(arm, n, v):
    pb = arm.pose.bones[n]
    pb.matrix = Matrix.Translation(v) @ pb.matrix
    maj()


def aligner(arm, n, enfant, point):
    """Tourne l'os n pour que la tête de son enfant aille vers point."""
    pb = arm.pose.bones[n]
    v = arm.pose.bones[enfant].head - pb.head
    w = point - pb.head
    if v.length < 1e-6 or w.length < 1e-6:
        return
    r = v.rotation_difference(w).to_matrix().to_4x4()
    p = pb.head.copy()
    pb.matrix = Matrix.Translation(p) @ r @ Matrix.Translation(-p) @ pb.matrix
    maj()


def ik(arm, haut, bas, bout, cible, pole):
    """Cinématique inverse à deux os : le bout (poignet, cheville) va sur la cible, le coude ou le genou vers le pôle."""
    S = tete_de(arm, haut)
    a = (tete_de(arm, bas) - S).length
    b = (tete_de(arm, bout) - tete_de(arm, bas)).length
    d_vec = cible - S
    d = max(min(d_vec.length, a + b - 1e-4), abs(a - b) + 1e-4)
    axe = d_vec.normalized()
    x = (a * a - b * b + d * d) / (2 * d)
    h = math.sqrt(max(a * a - x * x, 0))
    vers_pole = (pole - S) - axe * (pole - S).dot(axe)
    if vers_pole.length < 1e-6:
        vers_pole = HAUT - axe * HAUT.dot(axe)
    coude = S + axe * x + vers_pole.normalized() * h
    aligner(arm, haut, bas, coude)
    aligner(arm, bas, bout, S + axe * d)


def bras(arm, cote, poignet, pole):
    ik(arm, 'upperarm_' + cote, 'lowerarm_' + cote, 'hand_' + cote, poignet, pole)


def orienter(arm, cote, doigts, paume):
    """Oriente la main : ses doigts (du poignet aux jointures) vers doigts, sa paume tournée vers paume."""
    pb = arm.pose.bones['hand_' + cote]
    y = doigts.normalized()
    p = paume - y * paume.dot(y)
    if p.length < 1e-6:
        return
    p.normalize()
    ly, lp = Vector((0, 1, 0)), PAUME[cote]
    local = Matrix((ly, lp, ly.cross(lp))).transposed()
    monde = Matrix((y, p, y.cross(p))).transposed()
    pb.matrix = Matrix.Translation(pb.head) @ (monde @ local.inverted()).to_4x4()
    maj()


def serrer(arm, cote, prise, f=1.0):
    """Referme les doigts de la main selon une prise (f : de la pose actuelle, 0, à la prise, 1)."""
    for doigt, angles in PRISES[prise].items():
        for i, a in enumerate(angles):
            pb = arm.pose.bones[f'{doigt}_0{i + 1}_{cote}']
            pb.rotation_quaternion = pb.rotation_quaternion.slerp(Quaternion((1, 0, 0), math.radians(a)), f)
    maj()


def pieds_au_sol(arm):
    """Note la place et l'orientation des pieds, pour les y remettre (planter) après avoir bougé le bassin."""
    return {c: arm.pose.bones['foot_' + c].matrix.copy() for c in 'lr'}


def planter(arm, pieds, genou=AVANT):
    for c in 'lr':
        m = pieds[c]
        p = m.translation.copy()
        ik(arm, 'thigh_' + c, 'calf_' + c, 'foot_' + c, p, p + genou * 1.0 + HAUT * 0.5)
        pb = arm.pose.bones['foot_' + c]
        pb.matrix = Matrix.Translation(pb.head) @ m.to_3x3().to_4x4()
        maj()


def niveler_tete(arm, part=1.0):
    """Ramène le regard à l'horizontale (en tangage), quelle que soit la pose de l'animation de base."""
    f = axe_de(arm, 'Head', 2)
    angle = math.degrees(math.asin(max(-1.0, min(1.0, f.z))))      # positif : il regarde vers le haut
    tourner(arm, 'Head', axe_de(arm, 'Head', 0), angle * part)


def regarder_devant(arm, part=1.0):
    """Ramène la tête face à l'avant (en lacet), après une torsion du buste."""
    f = axe_de(arm, 'Head', 2)
    f.z = 0
    if f.length < 1e-6:
        return
    angle = math.degrees(math.atan2(f.cross(AVANT).z, f.dot(AVANT)))
    tourner(arm, 'Head', HAUT, angle * part)


def bruit(t, graine, frequence=1.0):
    """Un petit bruit lisse et bouclable sur une durée de 1 (somme de sinus), entre -1 et 1."""
    r = random.Random(graine)
    p = [r.random() * 6.283 for _ in range(3)]
    f = max(1, round(frequence))
    return (math.sin(t * 6.283 * f + p[0]) * 0.5 + math.sin(t * 6.283 * f * 2 + p[1]) * 0.3
            + math.sin(t * 6.283 * f * 3 + p[2]) * 0.2)


def saccade(t, graine, cadence=14.0):
    """Un bruit en marches d'escalier (servomoteur qui décroche), entre -1 et 1."""
    k = int(t * cadence)
    return random.Random(graine * 1000 + k).uniform(-1, 1)


def adoucir(x):
    x = max(0.0, min(1.0, x))
    return x * x * (3 - 2 * x)


def entre(u, a, b):
    """La progression de u entre a et b (de 0 à 1, adoucie)."""
    return adoucir((u - a) / (b - a)) if b > a else float(u >= b)


# ------------------------------------------------------------------ enregistrer
def enregistrer(arm, nom, images):
    """images : liste de poses déjà calculées. Crée l'action nom, une clé par image."""
    vieille = bpy.data.actions.get(nom)
    if vieille:
        bpy.data.actions.remove(vieille)
    act = bpy.data.actions.new(nom)
    act.use_fake_user = True
    arm.animation_data.action = act
    precedent = {}
    for i, pose in enumerate(images):
        f = 1 + i
        for pb in arm.pose.bones:
            q, l = pose[pb.name]
            if pb.name in precedent and precedent[pb.name].dot(q) < 0:
                q = -q                                      # pas de retournement entre deux clés
            precedent[pb.name] = q
            pb.rotation_quaternion = q
            pb.location = l
            pb.keyframe_insert('rotation_quaternion', frame=f)
            if pb.name in ('root', 'pelvis'):
                pb.keyframe_insert('location', frame=f)
    arm.animation_data.action = None
    return act


def fabriquer(arm, nom, duree_s, fonction, boucle=True):
    """Appelle fonction(t, u) pour chaque image (t en secondes, u de 0 à 1) ; elle pose le squelette.
    Une boucle s'arrête une image avant la fin (la première image la referme) ; une action unique va jusqu'au bout."""
    n = max(2, int(round(duree_s * IPS)))
    images = []
    for i in range(n + (0 if boucle else 1)):
        t = i / IPS
        fonction(t, min(t / duree_s, 1.0))
        images.append(photographier(arm))
    return enregistrer(arm, nom, images), images


# ------------------------------------------------------------------ le fusil
# Tout est relatif à l'épaule droite, dans le repère du personnage (gauche, haut, avant), en mètres.
TENUES = {
    # le poignet droit, la direction du canon, la torsion du buste, la tête (penchée, inclinée), le coude droit (vers où)
    'visee':    dict(poignet=(0.035, -0.025, 0.21), canon=(0.0, 0.0, 1.0), vrille=-20, tete=(16, 9), coude=(-0.45, -0.5, 0.0)),
    'repos':    dict(poignet=(0.07, -0.30, 0.17), canon=(0.45, -0.60, 0.66), vrille=-10, tete=(4, 0), coude=(-0.2, -0.2, -0.5)),
    'course':   dict(poignet=(0.10, -0.27, 0.19), canon=(0.68, 0.52, 0.52), vrille=-6, tete=(0, 0), coude=(-0.25, -0.4, -0.4)),
    'recharge': dict(poignet=(0.08, -0.22, 0.24), canon=(0.45, -0.22, 0.86), vrille=-12, tete=(22, 0), coude=(-0.25, -0.4, -0.35)),
}
GARDE_MAIN = 0.30                          # de la poignée à la main gauche, le long du canon


def vec(t):
    return GAUCHE * t[0] + HAUT * t[1] + AVANT * t[2]


def vriller(arm, degres):
    """Tourne le buste (épaule gauche en avant pour un angle négatif), réparti sur les trois vertèbres."""
    for n, part in (('spine_01', 0.3), ('spine_02', 0.3), ('spine_03', 0.4)):
        tourner(arm, n, HAUT, degres * part)


def fusil(arm, tenue, recul=0.0, releve=0.0):
    """Le fusil d'une tenue : le poignet droit, la direction du canon, et les axes gauche et haut de l'arme."""
    T = TENUES[tenue]
    d = vec(T['canon']).normalized()
    l = HAUT.cross(d)
    if l.length < 1e-6:
        l = GAUCHE.copy()
    l.normalize()
    if releve:
        d = (Matrix.Rotation(math.radians(-releve), 3, l) @ d).normalized()
    u = d.cross(l).normalized()
    poignet = tete_de(arm, 'upperarm_r') + vec(T['poignet']) - d * recul
    return poignet, d, l, u


def tenir_fusil(arm, tenue, recul=0.0, releve=0.0, main_gauche=None, paume_gauche=None):
    poignet, d, l, u = fusil(arm, tenue, recul, releve)
    garde = poignet + d * GARDE_MAIN - u * 0.06 + l * 0.035      # le poignet gauche, sous et à gauche du garde-main
    cible_g = garde if main_gauche is None else main_gauche
    bras(arm, 'r', poignet, poignet + vec(TENUES[tenue]['coude']))   # le coude droit
    bras(arm, 'l', cible_g, cible_g + GAUCHE * 0.25 - HAUT * 0.7)    # le coude gauche, sous l'arme
    orienter(arm, 'r', d * 0.8 - u * 0.6, l)                         # la main dans l'axe de l'avant-bras, la paume contre la poignée
    orienter(arm, 'l', -l * 0.7 + d * 0.6 + u * 0.2, paume_gauche if paume_gauche is not None else u * 0.8 - l * 0.4)
    serrer(arm, 'r', 'poignee')
    serrer(arm, 'l', 'garde')
    return poignet, d, l, u


def recette_fusil(arm, base, tenue, raideur=0.5, repos='Idle_Loop', tir=None):
    pose_repos = echantillon(arm, repos, 0.0)
    T = TENUES[tenue]

    def f(t, u):
        poser(arm, echantillon(arm, base, t))
        melanger(arm, pose_repos, COLONNE[:4], raideur)   # le buste reste face à la cible
        vriller(arm, T['vrille'])
        r = releve = 0.0
        if tir:
            cadence, coups = tir
            k = (t * cadence) % 1.0
            if int(t * cadence) < coups:
                r = 0.035 * math.exp(-k * 8)
                releve = 140 * r                           # le canon se relève un peu à chaque coup
                tourner(arm, 'spine_03', GAUCHE, -r * 70)   # le buste encaisse le recul
                tourner(arm, 'spine_02', GAUCHE, -r * 30)
        regarder_devant(arm, 0.9)
        niveler_tete(arm)
        penche, incline = T['tete']
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), penche)
        tourner(arm, 'Head', axe_de(arm, 'Head', 2), -incline)          # la joue sur la crosse (vers la droite)
        tenir_fusil(arm, tenue, r, releve)
    return f


def recette_rechargement(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        vriller(arm, TENUES['recharge']['vrille'])
        regarder_devant(arm, 0.6)
        poignet, d, l, up = fusil(arm, 'recharge')
        prise = poignet + d * GARDE_MAIN - up * 0.06 + l * 0.035
        chargeur = poignet + d * 0.10 - up * 0.16 + l * 0.03   # sous l'arme, devant la poignée
        poche = tete_de(arm, 'pelvis') + GAUCHE * 0.15 + AVANT * 0.13 + HAUT * 0.04   # la poche, à la ceinture
        # 0-0,2 la main va au chargeur ; 0,2-0,4 le retire et le range ; 0,4-0,55 prend le neuf ;
        # 0,55-0,72 l'engage ; 0,72-0,8 un coup du plat de la main ; 0,8-1 reprend le garde-main
        etapes = [(0.0, prise), (0.18, chargeur), (0.24, chargeur), (0.40, poche), (0.50, poche),
                  (0.68, chargeur - up * 0.03), (0.74, chargeur + up * 0.035), (0.80, chargeur), (1.0, prise)]
        p = prise
        for (u0, p0), (u1, p1) in zip(etapes, etapes[1:]):
            if u0 <= u <= u1:
                p = p0.lerp(p1, entre(u, u0, u1))
                break
        paume = (up * 0.8 - l * 0.4).lerp(-HAUT * 0.3 + GAUCHE * 0.6, math.sin(math.pi * entre(u, 0.2, 0.7)))
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 22 * math.sin(math.pi * entre(u, 0.0, 0.95)))   # il regarde son arme
        tenir_fusil(arm, 'recharge', main_gauche=p, paume_gauche=paume)
    return f


# ------------------------------------------------------------------ les androïdes
def recette_androide_garde(arm):
    debout = echantillon(arm, 'Idle_Loop', 0.0)
    regards = [(0.0, 0), (0.30, 0), (0.36, 28), (0.52, 28), (0.58, 0), (0.75, 0), (0.80, -20), (0.92, -20), (0.97, 0), (1.0, 0)]

    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        melanger(arm, debout, COLONNE + JAMBES + ['pelvis'], 0.85)       # immobile, presque une statue
        bassin = tete_de(arm, 'pelvis')
        # les mains jointes devant la ceinture : la droite tient le poignet gauche
        bras(arm, 'l', bassin + AVANT * 0.20 - HAUT * 0.03 + GAUCHE * 0.03, bassin + GAUCHE * 0.6 + HAUT * 0.2)
        bras(arm, 'r', bassin + AVANT * 0.23 + GAUCHE * -0.02, bassin - GAUCHE * 0.6 + HAUT * 0.2)
        orienter(arm, 'l', -GAUCHE * 0.8 - HAUT * 0.5, -AVANT * 0.6 - GAUCHE * 0.3)
        orienter(arm, 'r', GAUCHE * 0.8 - HAUT * 0.5, -AVANT * 0.8 + HAUT * 0.3)
        serrer(arm, 'l', 'poing')
        serrer(arm, 'r', 'garde')
        # un coup d'œil de temps en temps, sec, comme une caméra
        angle = 0.0
        for (u0, a0), (u1, a1) in zip(regards, regards[1:]):
            if u0 <= u <= u1:
                angle = a0 + (a1 - a0) * entre(u, u0, u1)
                break
        tourner(arm, 'Head', HAUT, angle)
    return f


def recette_androide_marche(arm):
    repos = echantillon(arm, 'Idle_Loop', 0.0)

    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t))
        melanger(arm, repos, BRAS, 0.7)                    # les bras ne balancent presque plus
        melanger(arm, repos, COLONNE, 0.75)                # le buste droit, la tête à l'horizon
        pb = arm.pose.bones['pelvis']
        pb.location = pb.location.lerp(repos['pelvis'][1], 0.4)    # le bassin ne roule presque plus
        maj()
        for c, s in COTES:                                 # les coudes un peu pliés, les poings fermés
            tourner(arm, 'lowerarm_' + c, GAUCHE, -18)
            serrer(arm, c, 'poing')
        # un petit à-coup de servo à chaque pas, et un balayage lent de la tête
        tourner(arm, 'Head', GAUCHE, 1.2 * math.sin(u * 4 * math.pi))
        tourner(arm, 'Head', HAUT, 12 * math.sin(u * 2 * math.pi))
    return f


def recette_androide_scan(arm):
    repos = echantillon(arm, 'Idle_Loop', 0.0)
    cles = [(0.0, 0), (0.12, 0), (0.30, 50), (0.46, 50), (0.66, -50), (0.82, -50), (1.0, 0)]

    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        melanger(arm, repos, COLONNE + BRAS, 0.8)
        angle = 0.0
        for (u0, a0), (u1, a1) in zip(cles, cles[1:]):
            if u0 <= u <= u1:
                angle = a0 + (a1 - a0) * entre(u, u0, u1)
                break
        tourner(arm, 'spine_03', HAUT, angle * 0.25)
        tourner(arm, 'Head', HAUT, angle * 0.75)
        # pendant les arrêts, la tête « lit » la zone : de petits pas verticaux
        if abs(abs(angle) - 50) < 1:
            tourner(arm, 'Head', axe_de(arm, 'Head', 0), 4 * saccade(t, 7, 3))
        for c, _ in COTES:
            serrer(arm, c, 'poing')
    return f


def affaisser(arm, corps, tete, pieds):
    """L'androïde éteint : genoux pliés, buste et tête tombés vers l'avant, bras ballants. corps, tete : de 0 à 1."""
    deplacer(arm, 'pelvis', Vector((0, 0.04 * corps, -0.12 * corps)))
    tourner(arm, 'spine_01', GAUCHE, 8 * corps)
    tourner(arm, 'spine_02', GAUCHE, 8 * corps)
    tourner(arm, 'spine_03', GAUCHE, 9 * corps)
    tourner(arm, 'neck_01', GAUCHE, 10 * tete)
    tourner(arm, 'Head', GAUCHE, 16 * tete)
    tourner(arm, 'Head', AVANT, 9 * tete)
    for c, s in COTES:                                     # les bras pendent, tirés par leur poids
        epaule = tete_de(arm, 'upperarm_' + c)
        coude0, main0 = tete_de(arm, 'lowerarm_' + c), tete_de(arm, 'hand_' + c)
        coude1 = epaule - HAUT * 0.27 + AVANT * 0.04 + GAUCHE * s * 0.03
        main1 = coude1 - HAUT * 0.26 + AVANT * 0.07
        bras(arm, c, main0.lerp(main1, min(corps, 1.0)), coude0.lerp(coude1, min(corps, 1.0)) + AVANT * 0.3)
        serrer(arm, c, 'molle', min(corps, 1.0))
    planter(arm, pieds)


def recette_androide_panne(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        pieds = pieds_au_sol(arm)
        # 0-0,3 : les servos décrochent ; 0,3-0,6 : il s'effondre (de plus en plus vite) ; 0,6-1 : il se tasse
        glitch = max(0.0, 1 - abs(u - 0.18) / 0.15)
        if glitch:
            tourner(arm, 'Head', HAUT, 9 * glitch * saccade(t, 1))
            tourner(arm, 'Head', GAUCHE, 6 * glitch * saccade(t, 2))
            for c, s in COTES:
                tourner(arm, 'upperarm_' + c, AVANT, 5 * s * glitch * saccade(t, 3 + s))
        k = max(0.0, min(1.0, (u - 0.3) / 0.3))
        e = k * k                                           # la chute accélère
        if u > 0.6:
            v = (u - 0.6) / 0.4
            e = 1 + 0.07 * math.exp(-v * 5) * math.sin(v * 14)   # le rebond du tassement
        affaisser(arm, e, min(1.1, e * 1.15), pieds)
    return f


def recette_androide_redemarrage(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        pieds = pieds_au_sol(arm)
        # la tête se relève d'un coup, puis le corps par crans, puis un balayage de contrôle
        tete = 1 - 0.75 * entre(u, 0.12, 0.16) - 0.25 * entre(u, 0.55, 0.62)
        crans = [(0.20, 0.24, 0.25), (0.30, 0.34, 0.25), (0.40, 0.43, 0.25), (0.50, 0.55, 0.25)]
        corps = 1 - sum(p * entre(u, a, b) for a, b, p in crans)
        if 0.08 < u < 0.11:                                # un premier sursaut
            tourner(arm, 'Head', HAUT, 6 * saccade(t, 4, 30))
        affaisser(arm, corps, tete, pieds)
        scan = 30 * math.sin(math.pi * 2 * entre(u, 0.66, 0.90)) * (1 - entre(u, 0.86, 0.92))
        tourner(arm, 'Head', HAUT, scan)
    return f


# ------------------------------------------------------------------ les détenus
def recette_reddition(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 10 + 3 * bruit(u, 1))     # la tête basse, qui guette
        for c, s in COTES:
            tourner(arm, 'clavicle_' + c, AVANT, s * 7)                        # les épaules remontées par la peur
            epaule = tete_de(arm, 'upperarm_' + c)
            tremble = 0.008 * bruit(u, 3 + s, 5)
            main = epaule + GAUCHE * s * 0.17 + HAUT * (0.27 + tremble) + AVANT * 0.07
            bras(arm, c, main, epaule + GAUCHE * s * 0.6 - HAUT * 0.2)
            orienter(arm, c, HAUT + GAUCHE * s * 0.15, AVANT)                  # les paumes vers le garde
            serrer(arm, c, 'ouverte')
    return f


def mains_sur_la_tete(arm, coudes='avant', ecart=0.07):
    hl, hu, hf = (axe_de(arm, 'Head', i) for i in range(3))
    centre = tete_de(arm, 'Head') + hu * 0.10
    for c, s in COTES:
        poignet = centre + hu * 0.09 - hf * 0.10 + hl * s * ecart
        if coudes == 'avant':
            pole = poignet + hf * 0.6 + hl * s * 0.3 - hu * 0.2
        else:
            pole = poignet + hl * s * 0.7 - hf * 0.1
        bras(arm, c, poignet, pole)
        orienter(arm, c, -hl * s + hu * 0.3, hf + hu * 0.3)                   # les paumes sur l'arrière du crâne
        serrer(arm, c, 'molle')


def recette_protection(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Crouch_Idle_Loop', t))
        tourner(arm, 'spine_02', GAUCHE, 12)
        tourner(arm, 'spine_03', GAUCHE, 16)
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 28)
        tourner(arm, 'spine_03', HAUT, 4 * bruit(u, 9, 3))                   # il tremble
        mains_sur_la_tete(arm, 'avant', 0.05)
    return f


def recette_frappe(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        o = tete_de(arm, 'spine_03')
        impacts = []
        for c, s, decale in (('r', -1, 0.0), ('l', 1, 0.5)):
            k = (u + decale) % 1.0
            impact = o + AVANT * 0.47 + HAUT * 0.20 + GAUCHE * s * 0.12          # la porte, à 47 cm
            arme = o + AVANT * 0.28 + HAUT * 0.42 + GAUCHE * s * 0.15            # le poing levé
            if k < 0.35:
                p = impact.lerp(arme, entre(k, 0.0, 0.35))
            elif k < 0.48:
                v = (k - 0.35) / 0.13
                p = arme.lerp(impact, v * v)                                    # le coup accélère
            else:
                p = impact + AVANT * 0.01 * math.exp(-(k - 0.48) * 30)
                impacts.append(max(0.0, 1 - (k - 0.48) / 0.15))
            bras(arm, c, p, p + GAUCHE * s * 0.5 - HAUT * 0.6)
            orienter(arm, c, AVANT + HAUT * 0.3, -GAUCHE * s)                   # le poing de côté, comme un marteau
            serrer(arm, c, 'poing')
        choc = max(impacts) if impacts else 0.0
        tourner(arm, 'spine_03', GAUCHE, 4 + 5 * choc)
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 10 + 4 * choc)
    return f


def recette_panique(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Sprint_Loop', t))
        tourner(arm, 'spine_03', GAUCHE, 8)
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 14)
        mains_sur_la_tete(arm, 'avant', 0.06)
        tourner(arm, 'Head', HAUT, 18 * bruit(u, 12, 1))                       # il regarde derrière lui
    return f


def recette_marche_entravee(arm):
    repos = echantillon(arm, 'Idle_Loop', 0.0)
    lent = 1.25

    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t / lent))
        melanger(arm, repos, JAMBES, 0.45)                 # des pas courts : les chevilles aussi sont entravées
        tourner(arm, 'spine_03', GAUCHE, 10)
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 18)   # la tête basse
        bassin = tete_de(arm, 'pelvis')
        balance = 0.015 * math.sin(u * 4 * math.pi)
        for c, s in COTES:                                 # les poignets liés devant le ventre
            poignet = bassin + AVANT * 0.26 + HAUT * (0.04 + balance) + GAUCHE * s * 0.045
            bras(arm, c, poignet, poignet + GAUCHE * s * 0.6 - HAUT * 0.3)
            orienter(arm, c, AVANT * 0.6 - GAUCHE * s * 0.6 - HAUT * 0.3, -GAUCHE * s)
            serrer(arm, c, 'molle')
    return f, duree('Walk_Loop') * lent


def recette_a_genoux(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        pieds = pieds_au_sol(arm)
        # à genoux : le bassin descend, les genoux touchent le sol, les pieds restent derrière
        deplacer(arm, 'pelvis', Vector((0, 0.10, -0.42)))
        for c, s in COTES:
            hanche = tete_de(arm, 'thigh_' + c)
            genou = Vector((hanche.x + s * 0.03, hanche.y - 0.10, 0.06))
            cheville = Vector((genou.x, genou.y + 0.42, 0.10))
            aligner(arm, 'thigh_' + c, 'calf_' + c, genou)
            aligner(arm, 'calf_' + c, 'foot_' + c, cheville)
            pb = arm.pose.bones['foot_' + c]                # le cou-de-pied à plat sur le sol
            pied = Matrix.Rotation(math.radians(-70), 4, GAUCHE) @ pieds[c].to_3x3().to_4x4()
            pb.matrix = Matrix.Translation(pb.head) @ pied
            maj()
        tourner(arm, 'spine_01', GAUCHE, -4 + 1.5 * bruit(u, 2))                # le dos droit, qui respire
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 16)
        mains_sur_la_tete(arm, 'cotes', 0.06)
    return f


def recette_desespoir(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Sitting_Idle_Loop', t))
        tourner(arm, 'spine_01', GAUCHE, 14)
        tourner(arm, 'spine_02', GAUCHE, 12)
        tourner(arm, 'spine_03', GAUCHE, 10)
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 24 + 3 * bruit(u, 4))
        hl, hu, hf = (axe_de(arm, 'Head', i) for i in range(3))
        visage = tete_de(arm, 'Head') + hu * 0.08 + hf * 0.11
        sanglot = 0.006 * max(0.0, bruit(u, 8, 2))
        for c, s in COTES:                                  # la tête dans les mains, les coudes sur les genoux
            genou = tete_de(arm, 'calf_' + c)
            poignet = visage + hl * s * 0.06 - hu * (0.10 + sanglot) + hf * 0.03
            bras(arm, c, poignet, genou + HAUT * 0.05 + GAUCHE * s * 0.05)
            orienter(arm, c, hu + hl * s * -0.2, -hf)
            serrer(arm, c, 'molle')
    return f


# ------------------------------------------------------------------ les autres espèces
def recette_gris_marche(arm):
    repos = echantillon(arm, 'Idle_Loop', 0.0)
    lent = 1.45

    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t / lent))
        melanger(arm, repos, JAMBES, 0.4)                  # de petits pas prudents
        melanger(arm, repos, BRAS, 0.75)                   # les bras pendent, longs et mous
        pb = arm.pose.bones['pelvis']
        pb.location = pb.location.lerp(repos['pelvis'][1], 0.4)
        maj()
        tourner(arm, 'spine_03', GAUCHE, 12)               # voûté
        tourner(arm, 'Head', GAUCHE, -10 + 4 * math.sin(u * 4 * math.pi))   # la grosse tête oscille à chaque pas
        tourner(arm, 'Head', AVANT, 5 * math.sin(u * 2 * math.pi))
        for c, _ in COTES:
            tourner(arm, 'upperarm_' + c, GAUCHE, 10)       # les mains un peu en avant
            serrer(arm, c, 'ouverte')
    return f, duree('Walk_Loop') * lent


def recette_gris_curieux(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        tourner(arm, 'spine_03', GAUCHE, 10)
        tourner(arm, 'Head', GAUCHE, -4 + 6 * math.sin(u * 4 * math.pi))
        tourner(arm, 'Head', axe_de(arm, 'Head', 2), 22 * math.sin(u * 2 * math.pi))   # il penche la tête d'un côté, puis de l'autre
        # la main droite monte vers le menton, pensive
        menton = tete_de(arm, 'Head') + axe_de(arm, 'Head', 2) * 0.10 - HAUT * 0.04
        lever = entre(u, 0.2, 0.35) * (1 - entre(u, 0.75, 0.9))
        main = tete_de(arm, 'hand_r').lerp(menton - GAUCHE * 0.02 - HAUT * 0.04, lever)
        bras(arm, 'r', main, main - GAUCHE * 0.4 - HAUT * 0.5)
        serrer(arm, 'r', 'molle')
        serrer(arm, 'l', 'ouverte')
    return f


def recette_saurien_marche(arm):
    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t))
        tourner(arm, 'spine_01', GAUCHE, 20)               # penché en avant, comme un prédateur
        tourner(arm, 'spine_03', GAUCHE, 8)
        tourner(arm, 'Head', GAUCHE, -26)                  # la tête reste à l'horizontale
        tourner(arm, 'Head', HAUT, 8 * math.sin(u * 2 * math.pi))
        for c, s in COTES:
            tourner(arm, 'upperarm_' + c, GAUCHE, -15)
            tourner(arm, 'lowerarm_' + c, GAUCHE, -35)      # les griffes en avant
            serrer(arm, c, 'garde')
    return f


# ------------------------------------------------------------------ chaque espèce : son attente, sa démarche
def suivre(u, cles):
    """La valeur au temps u (0 à 1) d'une suite de clés (u, valeur), adoucie entre deux clés."""
    for (u0, a0), (u1, a1) in zip(cles, cles[1:]):
        if u0 <= u <= u1:
            return a0 + (a1 - a0) * entre(u, u0, u1)
    return cles[-1][1]


def bosse(u, centre, largeur):
    """Une bosse lisse autour de centre (0 à 1), qui boucle : 1 au centre, presque 0 à deux largeurs."""
    d = min(abs(u - centre), 1 - abs(u - centre))
    return math.exp(-(d / largeur) ** 2)


def respirer(arm, souffle, epaules=1.0):
    """La poitrine qui se soulève (souffle de 0 à 1) : le buste se redresse un peu, les épaules montent."""
    tourner(arm, 'spine_02', GAUCHE, -2.5 * souffle)
    tourner(arm, 'spine_03', GAUCHE, -1.5 * souffle)
    for c, s in COTES:
        tourner(arm, 'clavicle_' + c, AVANT, s * 3.0 * souffle * epaules)
    tourner(arm, 'Head', GAUCHE, 2.0 * souffle)            # la tête garde son regard


def hausser(arm, degres_g, degres_d):
    """Monte (ou baisse, en négatif) chaque épaule."""
    tourner(arm, 'clavicle_l', AVANT, degres_g)
    tourner(arm, 'clavicle_r', AVANT, -degres_d)


def mains_dans_le_dos(arm, balance=0.0):
    """Les mains jointes dans le dos, au creux des reins, la droite tenant le poignet gauche."""
    bassin = tete_de(arm, 'pelvis')
    for c, s in COTES:
        poignet = bassin - AVANT * 0.13 + HAUT * (0.04 + balance) + GAUCHE * s * (0.035 if c == 'l' else 0.05)
        bras(arm, c, poignet, poignet + GAUCHE * s * 0.6 - AVANT * 0.4)
        orienter(arm, c, -GAUCHE * s * 0.7 - HAUT * 0.5, -AVANT)
        serrer(arm, c, 'garde' if c == 'r' else 'molle')


def recette_saurien_attente(arm):
    """Le prédateur à l'affût : genoux fléchis, griffes en avant ; la tête tourne par à-coups, comme celle d'un lézard."""
    lacet = [(0.0, 0), (0.14, 0), (0.16, 24), (0.36, 24), (0.38, -12), (0.55, -12), (0.57, 6), (0.62, 6), (0.64, -28),
             (0.82, -28), (0.84, 0), (1.0, 0)]
    roulis = [(0.0, 0), (0.36, 0), (0.38, 8), (0.55, 8), (0.57, -6), (0.82, -6), (0.84, 0), (1.0, 0)]

    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        pieds = pieds_au_sol(arm)
        deplacer(arm, 'pelvis', Vector((0, -0.02, -0.07)))  # le bassin descend, un peu en avant
        planter(arm, pieds)
        tourner(arm, 'spine_01', GAUCHE, 16)
        tourner(arm, 'spine_03', GAUCHE, 6)
        respirer(arm, 0.5 - 0.5 * math.cos(u * 3 * 2 * math.pi), 0.5)
        tourner(arm, 'Head', GAUCHE, -22)                  # la tête reste à l'horizontale
        tourner(arm, 'Head', HAUT, suivre(u, lacet))
        tourner(arm, 'Head', axe_de(arm, 'Head', 2), suivre(u, roulis))
        for c, s in COTES:
            tourner(arm, 'upperarm_' + c, GAUCHE, -14)
            tourner(arm, 'lowerarm_' + c, GAUCHE, -42)      # les griffes en avant, prêtes
            serrer(arm, c, 'garde', 0.8 + 0.2 * bosse(u, 0.16, 0.03))
    return f


def recette_ichtyen_attente(arm):
    """L'homme-poisson : voûté, les épaules remontées, la tête en avant ; il déglutit, et se balance doucement."""
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        tourner(arm, 'spine_01', AVANT, 3 * math.sin(u * 2 * math.pi))
        tourner(arm, 'spine_03', GAUCHE, 16)
        tourner(arm, 'neck_01', GAUCHE, 10)
        hausser(arm, 7, 7)
        tourner(arm, 'Head', GAUCHE, -20)
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 5 * math.sin(u * 4 * 2 * math.pi))   # il déglutit, la tête qui pompe
        tourner(arm, 'Head', HAUT, 10 * math.sin(u * 2 * math.pi + 1.0))
        for c, s in COTES:
            tourner(arm, 'upperarm_' + c, GAUCHE, 12)       # les mains pendent en avant
            tourner(arm, 'lowerarm_' + c, GAUCHE, -12)
            serrer(arm, c, 'molle')
    return f


def recette_ichtyen_marche(arm):
    """Il se dandine : des pas courts, le corps qui roule d'un pied sur l'autre, la tête qui pompe à chaque pas."""
    repos = echantillon(arm, 'Idle_Loop', 0.0)
    lent = 1.2

    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t / lent))
        melanger(arm, repos, JAMBES, 0.25)
        melanger(arm, repos, BRAS, 0.5)
        tourner(arm, 'spine_01', AVANT, 7 * math.sin(u * 2 * math.pi))
        tourner(arm, 'spine_03', GAUCHE, 16)
        tourner(arm, 'neck_01', GAUCHE, 8)
        hausser(arm, 6, 6)
        tourner(arm, 'Head', GAUCHE, -18 + 4 * math.sin(u * 4 * math.pi))
        for c, s in COTES:
            tourner(arm, 'upperarm_' + c, GAUCHE, 10)
            serrer(arm, c, 'molle')
    return f, duree('Walk_Loop') * lent


def recette_lourd_attente(arm):
    """Un grand corps (le cératopsien) : une respiration lourde qui soulève les épaules, les bras écartés du corps."""
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        pieds = pieds_au_sol(arm)
        deplacer(arm, 'pelvis', Vector((0, 0, -0.03)))
        planter(arm, pieds)
        tourner(arm, 'spine_03', GAUCHE, 8)
        respirer(arm, 0.5 - 0.5 * math.cos(u * 2 * 2 * math.pi), 1.8)
        tourner(arm, 'Head', HAUT, 12 * math.sin(u * 2 * math.pi))
        for c, s in COTES:
            tourner(arm, 'upperarm_' + c, AVANT, s * 9)      # les bras écartés, trop massifs pour pendre
            tourner(arm, 'lowerarm_' + c, GAUCHE, -12)
            serrer(arm, c, 'poing', 0.7)
    return f


def recette_lourd_marche(arm):
    """Un pas pesant : le corps roule d'un côté à l'autre, les épaules en avant, les bras écartés."""
    repos = echantillon(arm, 'Idle_Loop', 0.0)
    lent = 1.3

    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t / lent))
        melanger(arm, repos, BRAS, 0.45)
        tourner(arm, 'spine_01', AVANT, 5 * math.sin(u * 2 * math.pi))
        tourner(arm, 'spine_03', GAUCHE, 10)
        hausser(arm, 3, 3)
        tourner(arm, 'Head', GAUCHE, -8)
        for c, s in COTES:
            tourner(arm, 'upperarm_' + c, AVANT, s * 10)
            tourner(arm, 'lowerarm_' + c, GAUCHE, -14)
            serrer(arm, c, 'poing', 0.7)
    return f, duree('Walk_Loop') * lent


def recette_longiligne_attente(arm):
    """Le grand alien : droit, le menton haut, les mains dans le dos ; il observe la salle lentement."""
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        tourner(arm, 'spine_01', GAUCHE, -3)
        respirer(arm, 0.5 - 0.5 * math.cos(u * 2 * 2 * math.pi), 0.4)
        tourner(arm, 'Head', GAUCHE, -5)
        tourner(arm, 'Head', HAUT, 22 * math.sin(u * 2 * math.pi))
        tourner(arm, 'Head', axe_de(arm, 'Head', 2), 6 * math.sin(u * 4 * math.pi))
        mains_dans_le_dos(arm)
    return f


def recette_longiligne_marche(arm):
    """Une marche glissée, sans rebond, les mains dans le dos, la tête immobile."""
    repos = echantillon(arm, 'Idle_Loop', 0.0)
    lent = 1.2

    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t / lent))
        pb = arm.pose.bones['pelvis']
        pb.location = pb.location.lerp(repos['pelvis'][1], 0.5)   # le bassin glisse, sans rebondir
        maj()
        melanger(arm, repos, COLONNE, 0.6)
        tourner(arm, 'spine_01', GAUCHE, -3)
        tourner(arm, 'Head', GAUCHE, -4)
        mains_dans_le_dos(arm, 0.008 * math.sin(u * 4 * math.pi))
    return f, duree('Walk_Loop') * lent


def recette_mutant_attente(arm):
    """Un corps qui ne lui obéit plus : une épaule plus haute, la tête penchée, des spasmes, un souffle rauque."""
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        pieds = pieds_au_sol(arm)
        deplacer(arm, 'pelvis', Vector((0.02, 0, -0.04)))
        planter(arm, pieds)
        spasme_tete, spasme_bras, spasme_dos = bosse(u, 0.22, 0.025), bosse(u, 0.55, 0.02), bosse(u, 0.8, 0.03)
        tourner(arm, 'spine_01', AVANT, -4)
        tourner(arm, 'spine_03', GAUCHE, 10 + 6 * spasme_dos)
        respirer(arm, 0.5 - 0.5 * math.cos(u * 3 * 2 * math.pi), 0.8)
        hausser(arm, 9 + 6 * spasme_bras, -3)
        tourner(arm, 'Head', axe_de(arm, 'Head', 2), 14 + 18 * spasme_tete)
        tourner(arm, 'Head', HAUT, -20 * spasme_tete + 8 * math.sin(u * 2 * math.pi))
        tourner(arm, 'lowerarm_l', GAUCHE, -32 - 25 * spasme_bras)   # le bras gauche replié, en griffe
        serrer(arm, 'l', 'garde', 0.8 + 0.2 * spasme_bras)
        serrer(arm, 'r', 'molle')
    return f


def recette_mutant_marche(arm):
    """Il boite : la jambe gauche raide, le buste qui tombe sur le côté à chaque pas, la tête penchée."""
    repos = echantillon(arm, 'Idle_Loop', 0.0)
    lent = 1.35

    def f(t, u):
        poser(arm, echantillon(arm, 'Walk_Loop', t / lent))
        melanger(arm, repos, ['thigh_l', 'calf_l', 'foot_l'], 0.4)  # la jambe gauche plie à peine
        melanger(arm, repos, ['upperarm_r', 'lowerarm_r'], 0.6)
        tourner(arm, 'spine_01', AVANT, 7 * math.sin(u * 2 * math.pi))
        tourner(arm, 'spine_03', GAUCHE, 12)
        hausser(arm, 9, -3)
        tourner(arm, 'Head', axe_de(arm, 'Head', 2), 14)
        tourner(arm, 'Head', GAUCHE, -10)
        tourner(arm, 'lowerarm_l', GAUCHE, -35)
        serrer(arm, 'l', 'garde')
        serrer(arm, 'r', 'molle')
    return f, duree('Walk_Loop') * lent


def recette_detenu_fatigue(arm):
    """Un détenu épuisé : les épaules tombées, la tête basse, le poids d'une jambe sur l'autre, un soupir."""
    def f(t, u):
        poser(arm, echantillon(arm, 'Idle_Loop', t))
        pieds = pieds_au_sol(arm)
        deplacer(arm, 'pelvis', GAUCHE * 0.03 * math.sin(u * 2 * math.pi) - HAUT * 0.015)
        planter(arm, pieds)
        soupir = bosse(u, 0.6, 0.06)
        tourner(arm, 'spine_03', GAUCHE, 12 - 6 * soupir)
        respirer(arm, soupir, 1.5)
        hausser(arm, -5, -5)
        tourner(arm, 'Head', axe_de(arm, 'Head', 0), 16 - 8 * soupir)
        tourner(arm, 'Head', HAUT, 6 * math.sin(u * 2 * math.pi + 0.5))
        for c, _ in COTES:
            serrer(arm, c, 'molle')
    return f


# ------------------------------------------------------------------ le catalogue
def catalogue(arm):
    marche, course, attente, accroupi = (duree(n) for n in ('Walk_Loop', 'Jog_Fwd_Loop', 'Idle_Loop', 'Crouch_Idle_Loop'))
    gris_marche, gris_duree = recette_gris_marche(arm)
    entrave, entrave_duree = recette_marche_entravee(arm)
    ichtyen_marche, ichtyen_duree = recette_ichtyen_marche(arm)
    lourd_marche, lourd_duree = recette_lourd_marche(arm)
    longiligne_marche, longiligne_duree = recette_longiligne_marche(arm)
    mutant_marche, mutant_duree = recette_mutant_marche(arm)
    return [
        # nom, durée (s), fonction, boucle
        ('Fusil_Repos', attente, recette_fusil(arm, 'Idle_Loop', 'repos', 0.3), True),
        ('Fusil_Marche', marche, recette_fusil(arm, 'Walk_Loop', 'repos', 0.5), True),
        ('Fusil_Course', course, recette_fusil(arm, 'Jog_Fwd_Loop', 'course', 0.4), True),
        ('Fusil_Visee', attente, recette_fusil(arm, 'Idle_Loop', 'visee', 0.6), True),
        ('Fusil_Marche_Visee', marche, recette_fusil(arm, 'Walk_Loop', 'visee', 0.8), True),
        ('Fusil_Tir', 1.0, recette_fusil(arm, 'Idle_Loop', 'visee', 0.6, tir=(8, 5)), True),
        ('Fusil_Accroupi_Visee', accroupi, recette_fusil(arm, 'Crouch_Idle_Loop', 'visee', 0.6), True),
        ('Fusil_Rechargement', 2.4, recette_rechargement(arm), False),
        ('Androide_Garde', 8.0, recette_androide_garde(arm), True),
        ('Androide_Marche', marche, recette_androide_marche(arm), True),
        ('Androide_Scan', 6.0, recette_androide_scan(arm), True),
        ('Androide_Panne', 1.8, recette_androide_panne(arm), False),
        ('Androide_Redemarrage', 2.6, recette_androide_redemarrage(arm), False),
        ('Detenu_Reddition', 3.0, recette_reddition(arm), True),
        ('Detenu_Protection', 3.0, recette_protection(arm), True),
        ('Detenu_Frappe_Porte', 1.4, recette_frappe(arm), True),
        ('Detenu_Panique', duree('Sprint_Loop'), recette_panique(arm), True),
        ('Detenu_Marche_Entravee', entrave_duree, entrave, True),
        ('Detenu_A_Genoux', 4.0, recette_a_genoux(arm), True),
        ('Detenu_Desespoir', 4.0, recette_desespoir(arm), True),
        ('Gris_Marche', gris_duree, gris_marche, True),
        ('Gris_Curieux', 6.0, recette_gris_curieux(arm), True),
        ('Saurien_Marche', marche, recette_saurien_marche(arm), True),
        ('Saurien_Attente', 6.0, recette_saurien_attente(arm), True),
        ('Ichtyen_Attente', 5.0, recette_ichtyen_attente(arm), True),
        ('Ichtyen_Marche', ichtyen_duree, ichtyen_marche, True),
        ('Lourd_Attente', 5.0, recette_lourd_attente(arm), True),
        ('Lourd_Marche', lourd_duree, lourd_marche, True),
        ('Longiligne_Attente', 6.0, recette_longiligne_attente(arm), True),
        ('Longiligne_Marche', longiligne_duree, longiligne_marche, True),
        ('Mutant_Attente', 5.0, recette_mutant_attente(arm), True),
        ('Mutant_Marche', mutant_duree, mutant_marche, True),
        ('Detenu_Fatigue', 6.0, recette_detenu_fatigue(arm), True),
    ]


# ------------------------------------------------------------------ la planche de contrôle : face et profil, en bâtons
SEGMENTS = [('pelvis', 'spine_01'), ('spine_01', 'spine_02'), ('spine_02', 'spine_03'), ('spine_03', 'neck_01'),
            ('neck_01', 'Head'), ('Head', 'Head_bout')] + [(a + c, b + c) for c in ('_l', '_r') for a, b in (
                ('clavicle', 'upperarm'), ('upperarm', 'lowerarm'), ('lowerarm', 'hand'), ('hand', 'middle_01'),
                ('middle_01', 'middle_03'), ('hand', 'thumb_02'), ('thigh', 'calf'), ('calf', 'foot'), ('foot', 'ball'))] \
    + [('spine_03', 'clavicle_l'), ('spine_03', 'clavicle_r'), ('pelvis', 'thigh_l'), ('pelvis', 'thigh_r')]


def positions(arm, pose):
    poser(arm, pose)
    p = {pb.name: pb.head.copy() for pb in arm.pose.bones}
    hb = arm.pose.bones['Head']
    p['Head_bout'] = hb.head + (hb.tail - hb.head).normalized() * 0.22   # le haut du crâne
    return p


def planche(arm, nom, images, dossier):
    try:
        from PIL import Image, ImageDraw
    except ImportError:
        print('  (pas de planche : pip install pillow)')
        return
    choix = [images[round(i * (len(images) - 1) / 5)] for i in range(6)]
    W, H, E = 150, 230, 115                                 # une vue ; 115 pixels par mètre
    img = Image.new('RGB', (W * 12, H + 24), (24, 26, 30))
    d = ImageDraw.Draw(img)
    d.text((6, 4), nom, fill=(230, 230, 230))
    for i, pose in enumerate(choix):
        P = positions(arm, pose)
        for v, (axe_x, signe) in enumerate(((0, 1), (1, -1))):    # face : x ; profil (vu de sa droite) : -y, l'avant à droite
            ox = (i * 2 + v) * W + W // 2
            d.line([(ox - W // 2 + 4, H + 14), (ox + W // 2 - 4, H + 14)], fill=(70, 70, 70))
            for a, b in SEGMENTS:
                cote = a[-2:] if a[-2:] in ('_l', '_r') else b[-2:] if b[-2:] in ('_l', '_r') else ''
                couleur = (90, 160, 255) if cote == '_l' else (255, 110, 90) if cote == '_r' else (220, 220, 220)
                pa, pb_ = P[a], P[b]
                d.line([(ox + signe * pa[axe_x] * E, H + 14 - pa[2] * E), (ox + signe * pb_[axe_x] * E, H + 14 - pb_[2] * E)],
                       fill=couleur, width=2)
            if nom.startswith('Fusil'):                    # le fusil, de la main droite à la gauche
                a, b = P['hand_r'], P['hand_l']
                d.line([(ox + signe * a[axe_x] * E, H + 14 - a[2] * E), (ox + signe * b[axe_x] * E, H + 14 - b[2] * E)],
                       fill=(255, 210, 80), width=1)
    os.makedirs(dossier, exist_ok=True)
    img.save(os.path.join(dossier, nom + '.png'))


def exporter(arm, mannequin, faites):
    # seules nos animations partent dans le fichier
    for a in list(bpy.data.actions):
        if a.name not in faites:
            bpy.data.actions.remove(a)
    for pb in arm.pose.bones:                              # le squelette revient au repos
        pb.rotation_quaternion = Quaternion()
        pb.location = Vector()
    if mannequin:
        bpy.context.scene.collection.objects.link(mannequin)
    bpy.context.view_layer.objects.active = arm
    act = bpy.data.actions[faites[0]]
    arm.animation_data.action = act
    arm.animation_data.action_slot = act.slots[0]
    bpy.ops.export_scene.gltf(filepath=SORTIE + '.glb', export_format='GLB', export_animations=True,
                              export_animation_mode='ACTIONS', export_skins=True, export_yup=True)
    bpy.ops.export_scene.fbx(filepath=SORTIE + '.fbx', bake_anim=True, bake_anim_use_all_actions=True,
                             bake_anim_use_nla_strips=False, add_leaf_bones=False, axis_forward='-Z', axis_up='Y')


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument('--seulement', default='')
    p.add_argument('--planches', default='')
    a = p.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else sys.argv[1:])
    arm, mannequin = preparer()
    faites = []
    for nom, d, fonction, boucle in catalogue(arm):
        if a.seulement and nom not in a.seulement.split(','):
            continue
        _, images = fabriquer(arm, nom, d, fonction, boucle)
        faites.append(nom)
        print(f'  {nom:24s} {d:5.2f} s  {len(images)} images')
        if a.planches:
            planche(arm, nom, images, a.planches)
    exporter(arm, mannequin, faites)
    print(f'\n{len(faites)} animations dans {SORTIE}.glb et .fbx')


if __name__ == '__main__':
    main()
