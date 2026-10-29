#!/usr/bin/env python3
"""Maquette du prologue : le Colosse, le vaisseau-prison à l'échelle réelle.

D'après le plan du niveau (docs/prologue/PLAN-NIVEAU.md) et la bible de direction artistique (docs/prologue/BIBLE-DA.md).
Le même script écrit :
  - la maquette dans vitrine/atelier-prologue.html, entre les repères MAQUETTES-DEBUT et MAQUETTES-FIN ;
  - docs/prologue/maquette-vaisseau-prison.json (le format « Exporter » de l'atelier, pour Unity) ;
  - docs/prologue/plan-niveau.svg, le plan à l'échelle, tiré des mêmes pièces : le plan et la maquette ne peuvent pas diverger.
Usage : python3 outils/maquette-prologue.py

Repère : mètres. X va de la proue (ouest) vers la poupe (est), Z vers le sud, Y vers le haut. L'origine est le cœur-relique.
Le pont est en Y = 0 : les sols sont posés dessous, les murs dessus.
La règle des deux couches de la bible : au-dessus de 4 m le Monolithe (acier, nervures, lignes de lumière froide),
de 0 à 4 m le Chantier (tôle vert-de-gris, tuyaux, lampes au sodium), dans la part que fixe chaque zone.
"""
import json
import os
import re

RACINE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TAILLES = {k: v for k, v in json.load(open(os.path.join(RACINE, 'docs', 'prologue', 'tailles-modeles.json'), encoding='utf-8')).items() if not k.startswith('_')}
E = 0.2       # épaisseur des murs, des sols et des plafonds
HC = 4.5      # hauteur d'une coursive principale (bible, § 9)
CHANTIER = 4.0  # la limite des deux couches
pieces = []
ZONES = []    # les salles et les coursives, pour le plan : (x0, x1, z0, z1, h, nom, couche)
CHEMINS = []  # les chemins dessinés sur le plan


def r(v):
    return round(v, 3)


def bloc(x, z, l, p, h, y=0.0, teinte='acier', plafond=False):
    """Bloc de l (X) × h (Y) × p (Z), centré en (x, z), le bas à la hauteur y."""
    q = {'type': 'forme', 'cle': 'bloc', 'position': [r(x), r(y), r(z)], 'dims': [r(l), r(h), r(p)]}
    if teinte:
        q['teinte'] = teinte
    if plafond:
        q['plafond'] = True
    pieces.append(q)


def modele(cle, x, z, cadre=None, y=0.0, rot=0, rotx=0, echelle=None):
    """Un modèle du prologue ; sans cadre, il prend sa taille réelle (docs/prologue/tailles-modeles.json)."""
    assert cle in TAILLES, cle
    q = {'type': 'modele', 'cle': cle, 'position': [r(x), r(y), r(z)], 'cadre': cadre or TAILLES[cle]['cadre']}
    if rot or rotx:
        q['rotation'] = [rotx, rot, 0]
    if echelle:
        q['echelle'] = [r(v) for v in echelle]
    pieces.append(q)


def humain(x, z, camp=None, y=0.0):
    """Une silhouette de 1,80 m : bleu les gardes, orange les pirates, jaune les détenus."""
    q = {'type': 'forme', 'cle': 'humain', 'position': [r(x), r(y), r(z)]}
    if camp:
        q['teinte'] = camp
    pieces.append(q)


def etiquette(texte, x, z, y, police=1.0, teinte=None):
    q = {'type': 'forme', 'cle': 'etiquette', 'position': [r(x), r(y), r(z)], 'texte': texte, 'police': police}
    if teinte:
        q['teinte'] = teinte
    pieces.append(q)


def neon(x, z, y, l, axe='x', teinte='blanc', plafond=False, section=(0.06, 0.12)):
    """Un tube lumineux de longueur l, le long de X, de Z ou debout (Y), le bas à la hauteur y."""
    h, p_ = section
    dims = {'x': [r(l), h, p_], 'z': [p_, h, r(l)], 'y': [p_, r(l), p_]}[axe]
    q = {'type': 'forme', 'cle': 'neon', 'position': [r(x), r(y), r(z)], 'dims': dims, 'teinte': teinte}
    if plafond:
        q['plafond'] = True
    pieces.append(q)


# ------------------------------------------------------------------ la lumière de la bible (§ 8)
def ligne_froide(a0, a1, c, y, axe='x'):
    """Une ligne de lumière froide (6 500 K) sous le plafond : elle trace l'axe, donc le chemin."""
    x, z = ((a0 + a1) / 2, c) if axe == 'x' else (c, (a0 + a1) / 2)
    neon(x, z, y, a1 - a0, axe, 'blanc', section=(0.08, 0.14))


def sodium(x, z, y=3.5, axe='x', l=0.8):
    """Une lampe grillagée au sodium (2 200 K) : un îlot chaud, là où quelque chose sert."""
    neon(x, z, y, l, axe, 'ambre', section=(0.12, 0.14))


def gyrophare(x, z, y):
    neon(x, z, y, 0.35, 'y', 'rouge', section=(0.2, 0.2))


def verrou(x, z, y, l, axe='x'):
    """Le voyant rouge au-dessus d'une porte verrouillée."""
    neon(x, z, y, l, axe, 'rouge', section=(0.06, 0.08))


# ------------------------------------------------------------------ murs, salles, structure
def mur(axe, ligne, a, b, h, ouvertures=(), y0=0.0, teinte='acier', prolonger=True, bas=None):
    """Mur sur la ligne z = ligne (axe 'z', il court le long de X) ou x = ligne (axe 'x', le long de Z), de a à b.
    Ouverture : (centre, largeur, haut, genre, bas). Genres : porte, passage (toute la hauteur), fenetre (verre),
    scellee (panneau rouge qu'on traverse en visite), champ (champ de force).
    bas : (teinte, hauteur) ; la couche Chantier, de 0 à hauteur, prend cette teinte (tôle vert-de-gris)."""
    def plein(u0, u1, v0, v1, t=teinte, ep=E):
        if u1 - u0 < 0.01 or v1 - v0 < 0.01:
            return
        if bas and t == teinte and v0 < bas[1] < v1:
            plein(u0, u1, v0, bas[1], bas[0], ep)
            plein(u0, u1, bas[1], v1, t, ep)
            return
        if bas and t == teinte and v1 <= bas[1]:
            t = bas[0]
        if axe == 'z':
            bloc((u0 + u1) / 2, ligne, u1 - u0, ep, v1 - v0, y0 + v0, t)
        else:
            bloc(ligne, (u0 + u1) / 2, ep, u1 - u0, v1 - v0, y0 + v0, t)
    if axe == 'z' and prolonger:   # les murs le long de X débordent d'une demi-épaisseur : les angles sont fermés
        a, b = a - E / 2, b + E / 2
    cur = a
    for o in sorted(ouvertures):
        centre, largeur, haut, *reste = o
        genre, bas_o = (list(reste) + ['porte', 0.0][len(reste):])[:2]
        if genre == 'passage':
            haut = h
        o0, o1 = max(a, centre - largeur / 2), min(b, centre + largeur / 2)
        plein(cur, o0, 0, h)
        plein(o0, o1, haut, h)
        plein(o0, o1, 0, bas_o)
        if genre in ('fenetre', 'scellee', 'champ'):
            plein(o0, o1, bas_o, haut, {'fenetre': 'verre', 'scellee': 'scelle', 'champ': 'champ'}[genre], 0.08)
        cur = o1
    plein(cur, b, 0, h)


def portiques(x0, x1, z0, z1, h, portes, murs, pas=4.0, sans=''):
    """Les nervures du Monolithe : un portique tous les 4 m, deux montants contre les murs et une poutre sous le plafond.
    Pas de montant devant une ouverture."""
    long_x = (x1 - x0) >= (z1 - z0)
    a0, a1 = (x0, x1) if long_x else (z0, z1)
    cotes = (('n', z0, 1), ('s', z1, -1)) if long_x else (('o', x0, 1), ('e', x1, -1))
    k = 1
    while a0 + k * pas < a1 - 1.0:
        a = a0 + k * pas
        for cote, ligne, sens in cotes:
            if cote not in murs or cote in sans:
                continue
            if any(abs(a - o[0]) < o[1] / 2 + 0.6 for o in portes.get(cote, ())):
                continue
            c = ligne + sens * (E / 2 + 0.15)
            if long_x:
                bloc(a, c, 0.5, 0.3, h - 0.6)
            else:
                bloc(c, a, 0.3, 0.5, h - 0.6)
        if long_x:
            bloc(a, (z0 + z1) / 2, 0.5, z1 - z0 - E, 0.6, h - 0.6, plafond=True)
        else:
            bloc((x0 + x1) / 2, a, x1 - x0 - E, 0.5, 0.6, h - 0.6, plafond=True)
        k += 1


def espace(x0, x1, z0, z1, h, murs='nseo', portes=None, sol='tole', plafond=True, nom=None, police=1.8, teinte_nom=None,
           chantier=False, nervures=False, couche='mono', plan=True, sans_montants=''):
    """Une salle ou une coursive : sol, plafond, murs des côtés demandés (n = Z min, s = Z max, o = X min, e = X max).
    chantier : la couche Chantier sur le bas des murs ; nervures : les portiques du Monolithe."""
    portes = portes or {}
    cx, cz = (x0 + x1) / 2, (z0 + z1) / 2
    if plan:
        ZONES.append((x0, x1, z0, z1, h, nom, couche))
    if sol:
        bloc(cx, cz, x1 - x0, z1 - z0, E, -E, sol)
    if plafond:
        bloc(cx, cz, x1 - x0, z1 - z0, E, h, plafond=True)
    bas = ('vertdegris', min(CHANTIER, h)) if chantier else None
    for cote, (axe, ligne, a, b) in {'n': ('z', z0, x0, x1), 's': ('z', z1, x0, x1), 'o': ('x', x0, z0, z1), 'e': ('x', x1, z0, z1)}.items():
        if cote in murs:
            mur(axe, ligne, a, b, h, portes.get(cote, ()), bas=bas)
    if nervures:
        portiques(x0, x1, z0, z1, h, portes, murs, sans=sans_montants)
    if nom:
        etiquette(nom, cx, cz, h + 1.0, police, teinte_nom)


def tuyaux(axe, ligne, a, b, sens, ys=(3.35, 3.7)):
    """Deux tuyaux le long d'un mur, sous la limite des 4 m : la couche Chantier."""
    for y, t in zip(ys, ('rouille', 'sombre')):
        c = ligne + sens * 0.32
        if axe == 'z':
            bloc((a + b) / 2, c, b - a, 0.24, 0.24, y, t)
        else:
            bloc(c, (a + b) / 2, 0.24, b - a, 0.24, y, t)


def garde_corps(axe, ligne, a, b, y, trous=()):
    """Un garde-corps de 1,1 m, interrompu là où arrive un escalier ou un pont (trous : (début, fin))."""
    bornes = [a] + [v for t in sorted(trous) for v in t] + [b]
    for u0, u1 in zip(bornes[::2], bornes[1::2]):
        if u1 - u0 < 0.05:
            continue
        if axe == 'z':
            bloc((u0 + u1) / 2, ligne, u1 - u0, 0.06, 1.1, y)
        else:
            bloc(ligne, (u0 + u1) / 2, 0.06, u1 - u0, 1.1, y)


def dalle(x0, x1, z0, z1, y, ep=0.3, teinte='tole'):
    """Un plancher suspendu (passerelle, niveau) dont le dessus est à la hauteur y."""
    bloc((x0 + x1) / 2, (z0 + z1) / 2, x1 - x0, z1 - z0, ep, y - ep, teinte)


def dalles(x0, x1, z0, z1, y=-0.04):
    """Le sol modulaire (6 × 4 m) posé en damier, étiré pour couvrir juste la zone."""
    nx, nz = max(1, round((x1 - x0) / 6)), max(1, round((z1 - z0) / 4))
    lx, lz = (x1 - x0) / nx, (z1 - z0) / nz
    for i in range(nx):
        for k in range(nz):
            modele('06-Couloirs/sols', x0 + (i + 0.5) * lx, z0 + (k + 0.5) * lz, y=y, rot=180 if (i + k) % 2 else 0, echelle=[lx / 6, 1, lz / 4])


def couloir_habille(a0, a1, c, axe='x', largeur=4.0):
    """Un tronçon du couloir détaillé (l'étalon de qualité de la bible), mis à la largeur de la coursive et étiré de a0 à a1."""
    f = largeur / 3.0
    longueur = a1 - a0
    x, z = ((a0 + a1) / 2, c) if axe == 'x' else (c, (a0 + a1) / 2)
    modele('06-Couloirs/couloir-principal', x, z, rot=90 if axe == 'x' else 0, echelle=[f, f, longueur / 10.56])


def escalier(x0, z, n, montee, giron, largeur, y0=0.0, sens=1):
    """Marches qui montent le long de X (sens = 1 vers +X), centrées en z ; chaque marche est un bloc depuis y0."""
    for i in range(n):
        bloc(x0 + sens * (i + 0.5) * giron, z, giron, largeur, (i + 1) * montee, y0)


porte = lambda c, l=1.6, h=2.6: (c, l, h, 'porte', 0.0)            # porte ordinaire (bible : 1,6 × 2,6 m)
grande_porte = lambda c, l=3.0, h=3.2: (c, l, h, 'porte', 0.0)
passage = lambda c, l=4.0: (c, l, HC, 'passage', 0.0)
scellee = lambda c, l=2.4, h=2.8: (c, l, h, 'scellee', 0.0)
trappe = lambda c: (c, 1.2, 2.0, 'porte', 0.0)          # l'entrée d'un conduit de maintenance
conduit = lambda c: (c, 1.2, 2.0, 'passage', 0.0)


def gaine(x0, x1, z0, z1, murs, portes=None):
    """Un conduit de maintenance. Dans le jeu : 1,2 × 1,2 m, on y avance accroupi. Dans la maquette : 2 m de haut, pour la visite."""
    espace(x0, x1, z0, z1, 2.0, murs, portes, chantier=True, couche='conduit')
    long_x = (x1 - x0) >= (z1 - z0)
    if long_x:
        neon((x0 + x1) / 2, (z0 + z1) / 2, 1.88, min(1.2, x1 - x0 - 0.4), 'x', 'ambre', plafond=True, section=(0.03, 0.04))
    else:
        neon((x0 + x1) / 2, (z0 + z1) / 2, 1.88, min(1.2, z1 - z0 - 0.4), 'z', 'ambre', plafond=True, section=(0.03, 0.04))


# ================================================================== la coque
bloc(11, 5, 220, 92, 1.0, -1.2, 'sombre')                      # sous tout le vaisseau : la coque, d'un seul bloc
for z in (-34, -14, 14, 34):                                    # les tuyères, à la poupe
    bloc(124, z, 6, 10, 10, -1.2, 'sombre')
neon(-98.9, -3, 0.2, 0.6, 'z', 'rouge')                         # feux de position : bâbord rouge, tribord vert
neon(-98.9, 3, 0.2, 0.6, 'z', 'vert')

# ================================================================== 1 · le laboratoire (Monolithe 90 %)
espace(-12, 12, -34, -14, 8, portes={'s': [grande_porte(0)]}, nom='L · Laboratoire', police=2.2)
for x in (-8, -4, 4, 8):                                       # les nervures : montants sur le mur sud, poutres sous le plafond
    bloc(x, -14.25, 0.5, 0.3, 7.4)
    bloc(x, -24, 0.5, 20 - E, 0.6, 7.4, plafond=True)
bloc(-8, -33.75, 0.5, 0.3, 7.4)
ligne_froide(-10, 10, -26.5, 7.6)
ligne_froide(-10, 10, -20.5, 7.6)
# la cellule d'observation : le départ
espace(-4, 4, -34, -28, 4, 'eos', {'s': [(-1.2, 4.4, 3.4, 'fenetre', 0.6), porte(2.6, 1.4, 2.4)], 'e': [trappe(-33.2)]}, sol=None, plan=False)
bloc(-3.2, -32, 1.0, 2.2, 0.5)                                 # la couchette
modele('05-Cellules/porte-isolement', 2.6, -28, [1.4, 2.4, 0.4])
neon(0, -31, 3.88, 1.6, 'x', 'blanc', plafond=True)            # la lampe de la cellule
etiquette('1 · Le réveil', 0, -31, 4.6, 0.6, 'ambre')
# le conduit d'aération de la cellule : la deuxième façon de sortir
gaine(4, 11, -33.9, -32.6, 'se', {'s': [conduit(10.4)]})
gaine(9.8, 11, -32.6, -22, 'eo')
# le bureau de la Conservatrice et l'infirmerie : cloisons vitrées (zones facultatives)
bloc(-8.5, -22, 7, 0.06, 2.6, 0, 'verre')
bloc(-5, -19, 0.06, 6, 2.6, 0, 'verre')
bloc(-9.5, -17.5, 2.4, 1.2, 0.8)                               # le bureau
modele('15-Reliques/tablette-glyphes', -9.0, -17.6, [0.3, 0, 0], 0.8)
modele('09-Informatique/console-murale', -11.75, -19, [1.4, 1.4, 0.4], 1.0, 90)
etiquette('Bureau de la Conservatrice · son dossier : « sujet V-0 »', -8.5, -18, 3.2, 0.42)
ZONES.append((-12, -5, -22, -14, 2.6, 'Bureau', 'fac'))
bloc(7.4, -22, 4.8, 0.06, 2.6, 0, 'verre')
bloc(11.5, -22, 0.8, 0.06, 2.6, 0, 'verre')                    # l'arrivée du conduit, entre les deux vitres
bloc(5, -19, 0.06, 6, 2.6, 0, 'verre')
modele('15-Reliques/chambre-stase', 9, -18, [2.4, 3.0, 2.4])   # le scanner médical
modele('09-Informatique/console-murale', 11.75, -16, [1.4, 1.4, 0.4], 1.0, -90)
etiquette('Infirmerie · le scanner : un sang chargé d\'Éther', 8.5, -18, 3.6, 0.42)
ZONES.append((5, 12, -22, -14, 2.6, 'Infirmerie', 'fac'))
# les chambres d'étude et les machines du laboratoire
for x in (-2.5, 2.5):
    modele('15-Reliques/chambre-stase', x, -24, [2.4, 3.0, 2.4])
for x in (-9, -7.4):
    modele('09-Informatique/baie-serveur', x, -32.8, [0.8, 2.2, 1.2])
modele('09-Informatique/cables', -6, -30.5, [2, 0, 2])
for x in (-4.5, 4.5):
    bloc(x, -15.2, 1.2, 0.6, 1.0)
    modele('09-Informatique/console-murale', x, -15.2, [1.0, 1.0, 0.3], 1.0)
humain(6, -26, 'bleu')
for x, z in ((-11.5, -33.5), (11.5, -14.5)):
    gyrophare(x, z, 6.5)                                       # après la panne, tout devient rouge

espace(-2, 2, -14, -10, HC, 'eo', chantier=True, couche='coursive')
neon(0, -12, HC - 0.3, 3.0, 'z', 'blanc', section=(0.08, 0.14))

# ================================================================== l'anneau du cœur-relique (Monolithe 80 %)
espace(-10, 10, -10, -6, HC, 'noe', {'n': [passage(0)]}, nom='Anneau du cœur-relique', police=1.0, chantier=True, couche='coursive')
espace(-10, 10, 6, 10, HC, 'soe', {'s': [passage(0)]}, chantier=True, couche='coursive')
espace(-10, -6, -6, 6, HC, 'o', {'o': [passage(0)]}, chantier=True, couche='coursive')
espace(6, 10, -6, 6, HC, 'e', {'e': [passage(0)]}, chantier=True, couche='coursive')
for (a0, a1, c, axe) in ((-9, 9, -8, 'x'), (-9, 9, 8, 'x'), (-5, 5, -8, 'z'), (-5, 5, 8, 'z')):
    ligne_froide(a0, a1, c, HC - 0.3, axe)
for x, z in ((-8, -3), (8, 3)):
    humain(x, z, 'bleu')
# le puits du cœur : 16 m, des hublots de 8 m sur les quatre côtés (la révélation)
espace(-6, 6, -6, 6, 16, portes={c: [(0, 8, 3.6, 'fenetre', 1.0)] for c in 'nseo'}, nom='Cœur-relique', police=2.0, teinte_nom='ether', couche='bat')
bloc(0, 0, 2.4, 2.4, 0.8, 0, 'sombre')                         # le socle : pierre noire des Bâtisseurs
for (x, z, l, p) in ((0, -2.6, 5.3, 0.1), (0, 2.6, 5.3, 0.1), (-2.6, 0, 0.1, 5.3), (2.6, 0, 0.1, 5.3)):
    bloc(x, z, l, p, 0.03, 0, 'ether')                         # lignes de glyphes au sol
modele('15-Reliques/coeur-relique', 0, 0, [5, 5.8, 5], 0.8)
for (x, z, a) in ((0, -1.4, 'x'), (0, 1.4, 'x'), (-1.4, 0, 'z'), (1.4, 0, 'z')):
    neon(x, z, 0.02, 2.4, a, 'cyan', section=(0.03, 0.04))      # le cœur éclairé par en dessous : le seul cyan avant les reliques
modele('15-Reliques/anneau-relais', 0, 0, [7, 7, 7], 8.5)
for x, z in ((-4.5, -4.5), (4.5, -4.5), (-4.5, 4.5), (4.5, 4.5)):
    modele('08-Machines/pylone-energie', x, z, [2.5, 4, 2.5])
for (x, z, l, a) in ((0, -5.7, 10, 'x'), (0, 5.7, 10, 'x'), (-5.7, 0, 10, 'z'), (5.7, 0, 10, 'z')):
    neon(x, z, 15.4, l, a, 'blanc', section=(0.08, 0.1))        # la couronne froide en haut du puits
neon(0, 0, 4.2, 11, 'y', 'cyan', section=(0.22, 0.22))          # le faisceau d'Éther : le cœur s'emballe, on le voit de partout
etiquette('Il s\'emballe à son réveil', 0, 0, 12, 0.6, 'ether')

# ================================================================== 2 · la nef des cellules (Monolithe 70 %)
NX0, NX1, NH = -66, -18, 24
NIVEAUX = [0, 4.5, 9, 13.5, 18]
LC = 4.0                                                       # une cellule : 4 m de large, 4 m de profondeur
ZONES.append((NX0, NX1, -12, 12, NH, 'C · Nef des cellules', 'mono'))
bloc((NX0 + NX1) / 2, 0, NX1 - NX0, 24, E, -E, 'tole')
bloc((NX0 + NX1) / 2, 0, NX1 - NX0, 24, E, NH, plafond=True)
mur('x', NX1, -12, 12, NH, [(0, 4, HC, 'porte', 0.0)])        # l'entrée, depuis l'anneau
mur('x', NX0, -12, 12, NH)
mur('z', -12, NX0, NX1, NH)
mur('z', 12, NX0, NX1, NH, [(-60, 3.6, 4.4, 'porte', 0.0), trappe(-32)])   # au fond des cellules : la sortie vers les ateliers, le trou du conduit
for s in (-1, 1):                                              # s = -1 : rangée nord, s = 1 : rangée sud
    for i, y in enumerate(NIVEAUX):
        front = s * 8
        if i == 0:
            # au rez-de-chaussée, les portes sont ouvertes : la ruée. Une cellule sert de passage vers les ateliers.
            ouv = [(NX0 + (k + 0.5) * LC, 3.6 if (s == 1 and k == 1) else 1.6, 4.4 if (s == 1 and k == 1) else 2.6, 'porte', 0.0) for k in range(12)]
            mur('z', front, NX0, NX1, 4.5, ouv, bas=('vertdegris', 4.5), prolonger=False)
            for k in range(1, 12):
                bloc(NX0 + k * LC, s * 10, E, 4, 4.5, 0, 'vertdegris')
        else:
            bloc((NX0 + NX1) / 2, front, NX1 - NX0, E, 4.5, y)  # les façades des étages, portes fermées
            for k in range(12):
                bloc(NX0 + (k + 0.5) * LC, front - s * 0.12, 1.6, 0.06, 2.6, y, 'sombre')
                if k % 3 == 1:
                    verrou(NX0 + (k + 0.5) * LC, front - s * 0.16, y + 2.75, 0.8)
        if i:
            dalle(NX0, NX1, min(s * 6, s * 12), max(s * 6, s * 12), y)   # le plancher de l'étage : coursive et cellules
            trous = {(1, 4.5): [(-29.8, -25.6)], (1, 9): [(-34.8, -32.2), (-44, -40)], (-1, 9): [(-44, -40)]}.get((s, y), [])
            garde_corps('z', s * 6, NX0, NX1, y, trous)
    bloc((NX0 + NX1) / 2, s * 10, NX1 - NX0, 4, 0.3, 22.2)          # le plafond des cellules du dernier niveau
for x in range(-62, -18, 8):                                    # les piliers de la nef, tous les 8 m
    for s in (-1, 1):
        bloc(x, s * 6.3, 0.6, 0.6, 22.5)
for x in range(-62, -18, 4):                                    # les nervures du plafond, tous les 4 m
    bloc(x, 0, 0.6, 24 - E, 1.2, NH - 1.2, plafond=True)
dalle(-44, -40, -6, 6, 9)                                      # le pont transversal, au niveau 3
garde_corps('x', -44, -6, 6, 9)
garde_corps('x', -40, -6, 6, 9)
escalier(-22, 5.0, 18, 0.25, 0.3, 1.6, 0, -1)                  # du fond de la nef au niveau 2
escalier(-28, 5.0, 18, 0.25, 0.3, 1.6, 4.5, -1)                # du niveau 2 au niveau 3
# les cellules ouvertes du rez-de-chaussée, et le poste de garde
for (x, s) in ((-52, -1), (-44, -1), (-28, 1), (-40, 1)):
    modele('05-Cellules/lit-superpose', x - 0.9, s * 10.9, [2.1, 1.9, 2.1])
    modele('05-Cellules/toilettes', x + 1.3, s * 11.4, [0.6, 0.8, 0.7], rot=0 if s > 0 else 180)
    sodium(x, s * 10, 4.1, 'x', 0.6)                           # la vie des détenus : une lampe chaude par cellule
modele('05-Cellules/lavabo', -42.8, -9.3, [0.6, 1.0, 0.5], 0.4)
bloc(-22, -3.5, 3, 1.2, 1.1)                                   # le poste de garde, à l'entrée
modele('09-Informatique/console-murale', -22, -3.5, [1.0, 1.0, 0.3], 1.1)
modele('03-Gardes/droide-securite', -24.5, -2, [0, 2.1, 0], rot=-90)
modele('02-Prisonniers/detenu-brute', -36, 2.5)
modele('02-Prisonniers/detenu-atelier', -50, -2.5)
for x, z in ((-28, 1.5), (-33, -3), (-41, 3.5), (-47, 1), (-55, -3.5), (-58, 2)):
    humain(x, z, 'jaune')
for x, z, y in ((-30, -7, 9), (-52, 7, 9), (-38, -7, 13.5), (-25, 7, 4.5)):
    humain(x, z, 'bleu', y)
modele('13-Armes/cle-a-molette', -27, -1, [0.45, 0.45, 0.45], 0.02, 0, 90)   # l'arme de fortune, au sol
etiquette('Arme de fortune', -27, -1, 0.7, 0.35, 'ambre')
for s in (-3, 3):
    ligne_froide(NX0 + 2, NX1 - 2, s, 22.0)                    # les deux lignes froides de la nef, à 22 m
for s in (-1, 1):
    for y in NIVEAUX[1:]:
        neon((NX0 + NX1) / 2, s * 6.04, y - 0.36, NX1 - NX0 - 1, 'x', 'blanc', section=(0.04, 0.06))   # le bord de chaque niveau : la nef se lit en lignes
sodium(-60, 7.75, 4.45, 'x', 2.4)                             # la sortie, chaude, au fond : la cellule qui mène aux ateliers
for x in (NX0 + 0.6, NX1 - 0.6):
    for z in (-5.5, 5.5):
        gyrophare(x, z, 10)
for i, y in enumerate(NIVEAUX):
    etiquette(f'C{i + 1}', NX1 - 1.5, -7, y + 3.6, 0.6)        # les numéros des niveaux, peints à l'entrée
etiquette('C · Nef des cellules', (NX0 + NX1) / 2, 0, NH + 1.5, 2.6)
etiquette('2 · Les cellules s\'ouvrent', -30, 0, 6.5, 0.7, 'ambre')

espace(-18, -10, -2, 2, HC, 'ns', {'n': [passage(-14.4)], 's': [(-14, 2.4, 3.0, 'porte', 0.0)]}, chantier=True, couche='coursive')   # anneau → nef
ligne_froide(-17.5, -10.5, 0, HC - 0.3)

# ================================================================== l'aile nord-ouest : officiers et passerelle (facultatives)
espace(-16.4, -12.4, -22, -2, HC, 'eo', chantier=True, couche='coursive')
couloir_habille(-22, -12.2, -14.4, 'z')
couloir_habille(-11.8, -2.2, -14.4, 'z')
espace(-40, -12.4, -36, -22, HC, portes={'s': [porte(-14.4, 2.4, 2.8)], 'o': [porte(-29, 2.4, 2.8)]}, nom='Quartiers des officiers', police=1.2, nervures=True, couche='fac')
for x in (-37, -33.5):
    modele('05-Cellules/lit-superpose', x, -34.6, [2.1, 1.9, 2.1])
bloc(-20, -35, 4, 1.2, 1.6)                                    # le râtelier de l'armurerie
modele('10-Hangar/caisse-armes', -20, -32.5, [1.8, 1.0, 1.0])
modele('13-Armes/fusil-gardes', -21, -35, [1.0, 1.0, 1.0], 1.6, 90)
modele('13-Armes/pistolet-gardes', -19, -35, [0.25, 0.25, 0.25], 1.6)
sodium(-20, -35.3, 3.2)                                        # l'armurerie : ce qui sert
bloc(-28, -25, 2.2, 1, 0.9)                                    # le bureau, et le badge posé dessus
bloc(-28, -25, 0.3, 0.2, 0.05, 0.9, 'ambre')
sodium(-28, -25, 3.0)
etiquette('Badge de la Conservatrice : 3e clé des reliques', -28, -25, 1.6, 0.42, 'ambre')
for x, z in ((-32, -27), (-24, -30)):
    humain(x, z, 'bleu')
ligne_froide(-38, -14.4, -29, HC - 0.3)
espace(-80, -40, -31, -27, HC, 'ns', {'s': [passage(-78)]}, chantier=True, couche='coursive')
mur('x', -80, -31, -27, HC)
for (a0, a1) in ((-76, -64.5), (-64, -52.5), (-52, -40.5)):
    couloir_habille(a0, a1, -29)
espace(-80, -76, -27, -10, HC, 'eo', chantier=True, couche='coursive')
espace(-96, -76, -10, 10, 8, portes={'n': [(-78, 4, HC, 'porte', 0.0)], 'o': [(0, 16, 6.5, 'fenetre', 1.0)]}, nom='P · Passerelle', police=2.0, nervures=True, couche='fac')
dalles(-96, -76, -10, 10)
bloc(-86, 0, 8, 10, 0.5)                                       # l'estrade du commandement
for x, z in ((-92, -6), (-92, 6), (-88, -8.6), (-88, 8.6)):
    bloc(x, z, 1.4, 0.8, 1.0)                                  # pupitres
    modele('09-Informatique/console-murale', x, z, [1.0, 1.0, 0.3], 1.0, 90 if abs(z) < 8 else 0)
    sodium(x, z, 1.05, 'x', 0.8)                               # les écrans ambre
modele('09-Informatique/console-murale', -84, 0, [1.2, 1.2, 0.3], 0.5, 90)
etiquette('Commande du hangar : ouvrir la porte des gardes', -84, 0, 2.6, 0.45, 'ambre')
for x, z in ((-86, -3), (-89, 3.5), (-82, 6)):
    humain(x, z, 'bleu', 0.5 if -90 <= x <= -82 and -5 <= z <= 5 else 0)
ligne_froide(-94, -78, -5, 7.6)
ligne_froide(-94, -78, 5, 7.6)

# ================================================================== 3 · les ateliers (Chantier 70 %)
espace(-62, -58, 12, 16, HC, 'eo', chantier=True, couche='coursive')       # nef → ateliers, par une cellule
espace(-66, -38, 16, 40, 10, portes={'n': [(-60, 4, HC, 'porte', 0.0)], 'e': [grande_porte(24), trappe(19)]}, nom='A · Ateliers', police=2.2,
       chantier=True, nervures=True, couche='chantier')
for x in (-61, -54, -47):
    for z in (22, 30):
        bloc(x, z, 4, 1.4, 0.9)                                # les établis
        sodium(x, z, 2.4, 'x', 1.6)                            # un îlot chaud au-dessus de chaque poste
bloc(-52, 34, 26, 0.6, 0.8, 8.0)                               # le pont roulant, à 8 m
bloc(-55, 34, 1.6, 1.6, 1.0, 7.0, 'sombre')
for (x0, x1, z, s) in ((-66, -38, 16, 1), (-66, -38, 40, -1)):
    tuyaux('z', z, x0 + 2, x1 - 2, s)
for x in (-63, -41):
    bloc(x, 39.4, 0.35, 0.35, 10, 0, 'rouille')                # colonnes montantes
for x, z in ((-64, 37), (-64, 34.5), (-40.5, 37.5)):
    modele('10-Hangar/caisses', x, z, [2, 2, 2])
modele('10-Hangar/caisse-armes', -44, 37.5, [1.8, 1.0, 1.0])
for x, z in ((-40.5, 30), (-40.5, 31.2)):
    modele('10-Hangar/cellule-energie', x, z, [0, 1.0, 0])
modele('02-Prisonniers/detenu-atelier', -54, 20.6)
modele('02-Prisonniers/detenu-atelier', -47, 28.6, rot=180)
for x, z in ((-61, 26), (-50, 26), (-44, 21)):
    humain(x, z, 'jaune')
etiquette('Détenus alliés possibles', -52, 26, 2.8, 0.45)
ligne_froide(-64, -40, 28, 9.6)                                # une seule ligne froide : ici, la lumière est celle du travail

espace(-38, -10, 22, 26, HC, 'ns', {'n': [trappe(-32), porte(-14, 2.4, 2.8)], 's': [trappe(-32), passage(-24)]}, chantier=True, nervures=True, couche='coursive')
couloir_habille(-31.0, -25.6, 24)
couloir_habille(-22.4, -15.6, 24)
for x in (-37, -11):
    ligne_froide(x - 0.5, x + 0.5, 24, HC - 0.3)
sodium(-24, 25.6, 3.4)                                         # le croisement de la consigne
sodium(-14, 22.4, 3.4)
etiquette('Raccourci : s\'ouvre de ce côté', -14, 22.6, 3.4, 0.4, 'ambre')

espace(-15.2, -12.8, 2, 22, 3.0, 'eo', chantier=True, couche='coursive')   # le raccourci de service vers l'anneau
neon(-14, 12, 2.85, 1.2, 'z', 'ambre', plafond=True)

# la consigne (Monolithe 60 %)
espace(-26, -22, 26, 30, HC, 'eo', chantier=True, couche='coursive')
espace(-34, -14, 30, 46, 6, portes={'n': [grande_porte(-24), trappe(-32)]}, nom='Consigne', police=1.6, chantier=True, nervures=True, couche='mono', sans_montants='s')
for i in range(9):
    x = -32.6 + i * 1.65
    modele('07-Consigne/casier', x, 45.3, [1.6, 2.0, 0], rot=180)
    if i % 3 == 0:
        sodium(x, 45.0, 2.4, 'x', 0.6)                         # les casiers qu'on peut ouvrir, en ambre
for z in (36, 38):
    modele('07-Consigne/armoire-blindee', -33.35, z, [1.6, 2.4, 1.0], rot=90)
modele('07-Consigne/porte-coffre', -18, 42, [2.8, 2.8, 0])     # la chambre forte
sodium(-18, 39.7, 3.4, 'x', 1.6)
for z in (35, 39):
    bloc(-24, z, 8, 0.6, 2.2)                                  # rayonnages
modele('15-Reliques/tablette-glyphes', -24, 39, [0.5, 0, 0], 2.2)
etiquette('Objet saisi : il en lit les glyphes', -24, 39, 3.3, 0.42, 'ambre')
modele('09-Informatique/console-murale', -19, 30.35, [1.2, 1.2, 0.3], 1.0)
sodium(-19, 30.6, 2.5)
etiquette('Console : pirater la porte des reliques', -19, 31, 2.9, 0.42, 'ambre')
for x, z in ((-29, 41), (-18, 34)):
    humain(x, z, 'bleu')
ligne_froide(-32, -16, 38, 5.6)

# les conduits de maintenance : nef → coursive sud → consigne, avec une branche vers les ateliers
gaine(-32.6, -31.4, 12, 22, 'eo', {'o': [conduit(19)]})
gaine(-38, -32.6, 18.4, 19.6, 'ns')
gaine(-32.6, -31.4, 26, 30, 'eo')
etiquette('Conduits', -32, 17, 2.6, 0.45)

# ================================================================== 3 · la chambre des reliques (les Bâtisseurs)
espace(-2, 2, 10, 14, HC, 'eo', chantier=True, couche='coursive')
espace(-10, 10, 14, 34, 12, portes={'o': [scellee(24)], 'n': [scellee(0)], 's': [grande_porte(0)]}, nom='X · Chambre des reliques', police=2.0,
       teinte_nom='ether', nervures=True, couche='bat')
modele('12-Exterieur/cloison-etanche', -10, 24, [0, 2.8, 0])                # les portes scellées : sans courant
modele('12-Exterieur/cloison-etanche', 0, 14, [0, 2.8, 0], rot=90)
verrou(-10.15, 24, 3.0, 2.4, 'z')
verrou(0, 13.85, 3.0, 2.4, 'x')
etiquette('3 · Ouvrir : le courant, le piratage ou le badge', -12, 24, 4.0, 0.5, 'ambre')
# le sanctuaire : pierre noire, aucune usure, des lignes de glyphes cyan
bloc(0, 24, 4, 8, 0.6, 0, 'sombre')                            # le socle de la capsule
modele('15-Reliques/capsule-veilleur', 0, 24, [2.4, 1.2, 1.2], 0.6, 90)
for (x, z, l, p) in ((-2.2, 24, 0.08, 8.4), (2.2, 24, 0.08, 8.4), (-6, 24, 8, 0.08)):
    bloc(x, z, l, p, 0.03, 0, 'ether')                         # les lignes qui mènent à la capsule
etiquette('Sa capsule de sommeil', 0, 24, 2.6, 0.45)
bloc(6, 18, 1.4, 1.4, 0.3, 0, 'sombre')
modele('01-Joueur/combinaison-veilleur', 6, 18, [0, 2.0, 0], 0.3)
bloc(6, 18, 1.6, 1.6, 2.6, 0.3, 'verre')                       # la vitrine
neon(6, 18, 2.92, 1.6, 'x', 'cyan', section=(0.02, 0.04))
etiquette('Sa combinaison de Veilleur', 6, 18, 3.5, 0.45, 'ambre')
for x, z in ((-7.5, 17), (-7.5, 31), (7.5, 31)):
    bloc(x, z, 0.9, 0.9, 1.6, 0, 'sombre')                     # les stèles noires
modele('15-Reliques/tablette-glyphes', -7.5, 17, [0.6, 0, 0], 1.6)
modele('15-Reliques/prisme-ether', -7.5, 31, [0, 1.2, 0], 1.6)
modele('15-Reliques/cristal-ether', 7.5, 31, [0.8, 0.8, 0.8], 1.6)
for x, z in ((-7.5, 17), (-7.5, 31), (7.5, 31)):
    neon(x, z - 0.47, 0.02, 0.9, 'x', 'cyan', section=(0.02, 0.03))
neon(0, 24, 11.4, 3.0, 'z', 'blanc', section=(0.3, 0.3))       # une seule lumière froide, très haut, sur la capsule
modele('07-Consigne/armoire-blindee', 9, 32.6, [1.6, 2.4, 1.0], rot=180)

# ================================================================== 4 · le sas et la coque extérieure (le vide)
espace(-4, 4, 34, 42, HC, 'eos', {'s': [grande_porte(0)]}, nom='Sas', police=0.8, chantier=True, couche='mono')
modele('12-Exterieur/porte-sas', 0, 34)
modele('12-Exterieur/porte-sas', 0, 42)
sodium(0, 38, 4.1, 'x', 1.6)
etiquette('4 · Le vide : il faut la combinaison', 0, 38, 5.6, 0.6, 'ambre')
# le chemin sur la coque : des tôles posées sur le dos du vaisseau, bordées de balises rouges
COQUE = [(-2, 2, 42, 48), (2, 62, 44, 48), (58, 62, 20.2, 44)]
for (x0, x1, z0, z1) in COQUE:
    bloc((x0 + x1) / 2, (z0 + z1) / 2, x1 - x0, z1 - z0, E, -E, 'tole')
    ZONES.append((x0, x1, z0, z1, 0, None, 'vide'))
for x in range(4, 60, 4):
    for z in (44.15, 47.85):
        neon(x, z, 0.02, 0.5, 'x', 'rouge', section=(0.04, 0.06))
for z in range(24, 44, 4):
    for x in (58.15, 61.85):
        neon(x, z, 0.02, 0.5, 'z', 'rouge', section=(0.04, 0.06))
etiquette('4 · La coque extérieure : bottes magnétiques, oxygène', 30, 46, 2.4, 0.8, 'ambre')
for x, z, h in ((14, 41, 9), (36, 41, 6), (48, 30, 12)):
    bloc(x, z, 0.5, 0.5, h, -0.2, 'sombre')                    # antennes
    neon(x, z, h, 0.4, 'y', 'rouge', section=(0.12, 0.12))
for x, z in ((24, 38), (44, 38)):
    bloc(x, z, 6, 4, 2.2, -0.2, 'sombre')                      # bouches d'aération de la coque
modele('12-Exterieur/rocher-spatial', 30, 60, [4, 4, 4], 9)
modele('12-Exterieur/debris-1', 8, 58, [7, 4, 7], 5)
modele('12-Exterieur/debris-2', 70, 56, [9, 5, 9], 7)
# la montée le long du flanc du hangar, jusqu'au sas de la galerie à 12 m
escalier(62, 21.2, 48, 0.25, 0.3, 2.0)
bloc(78.4, 21.7, 4, 3, 12, 0, 'sombre')                        # le palier
for x in range(64, 77, 4):
    neon(x, 22.1, ((x - 62) / 0.3 + 1) * 0.25, 0.4, 'x', 'rouge', section=(0.04, 0.06))   # le bord des marches
modele('12-Exterieur/porte-sas', 78.4, 20, y=12)
sodium(78.4, 20.35, 14.9, 'x', 1.6)                            # le sas d'arrivée : ce qui sert
neon(78.4, 23.0, 12.0, 0.6, 'x', 'blanc', section=(0.35, 0.35))  # le projecteur du palier : on le voit depuis le sas des reliques
for x, z in ((58, 20), (118, 20), (58, -20), (118, -20), (88, 20)):
    gyrophare(x, z + (0.3 if z > 0 else -0.3), 30.2)           # les balises du hangar, sur son toit : le but, visible de loin

# ================================================================== l'aile est : salle des machines (Chantier 75 %)
espace(10, 14, -2, 2, HC, 'ns', chantier=True, couche='coursive')
neon(12, 0, HC - 0.3, 3.0, 'x', 'blanc', section=(0.08, 0.14))
espace(14, 38, -12, 12, 16, portes={'o': [(0, 4, HC, 'porte', 0.0)], 'e': [scellee(0, 3.0, 3.2)]}, nom='M · Salle des machines', police=2.2,
       sol=None, chantier=True, nervures=True, couche='chantier')
dalles(14, 38, -12, 12)
for y in (4, 8):                                               # les passerelles à 4 et 8 m, le long des murs nord et sud
    for s in (-1, 1):
        dalle(14.2, 37.8, min(s * 9.5, s * 11.9), max(s * 9.5, s * 11.9), y, 0.25, 'tole')
        garde_corps('z', s * 9.5, 14.2, 37.8, y, {(-1, 4): [(19.8, 22.6)], (-1, 8): [(26.0, 27.6)]}.get((s, y), []))
escalier(16, -8.75, 16, 0.25, 0.3, 1.5)                        # du sol à 4 m
escalier(22, -8.75, 16, 0.25, 0.3, 1.5, 4.0)                   # de 4 à 8 m
for y in (4, 8):
    neon(26, -9.4, y - 0.05, 23, 'x', 'ambre', section=(0.03, 0.05))   # le bord des passerelles, en ambre
modele('08-Machines/reacteur', 26, 0, [7, 8, 7])
for x in (17.5, 34.5):
    modele('08-Machines/generateur', x, 6, [3, 3, 3])
for x in (18, 34):
    modele('08-Machines/pylone-energie', x, -6, [2.5, 4.5, 2.5])
modele('08-Machines/coeur-fusion', 35.5, -2.6, [2.5, 3.5, 2.5])
modele('09-Informatique/console-murale', 32, -11.75, [1.2, 1.2, 0.3], 8.8)
sodium(32, -11.5, 10.6)
etiquette('3 · Rétablir le courant (passerelle à 8 m)', 32, -11, 11.4, 0.5, 'ambre')
for (x0, x1, z, s) in ((14, 38, -12, 1), (14, 38, 12, -1)):
    tuyaux('z', z, x0 + 1, x1 - 1, s, ys=(2.6, 3.0))
for z in (-11.5, 11.5):
    for x in (16, 36):
        bloc(x, z, 0.35, 0.35, 16, 0, 'rouille')               # colonnes montantes
for x in (16, 22, 28, 34):
    for z in (-6, 6):
        sodium(x, z, 14.6, 'x', 2.4)                           # les rampes au sodium, sous le plafond
modele('04-Pirates/pirate', 29, 9, rot=180)
for x, z in ((20, 8), (32, -8), (24, -10.7)):
    humain(x, z, 'orange', 4 if z == -10.7 else 0)

# la coursive éventrée : ouverte sur le vide (il faut la combinaison)
modele('12-Exterieur/cloison-etanche', 38, 0, [0, 3.2, 0])
verrou(37.85, 0, 3.35, 3.0, 'z')
espace(38, 58, -2, 2, HC, 'n', sol='tole', plafond=False, nom='Coursive éventrée : ouverte sur le vide', police=1.2, teinte_nom='rouge', couche='vide')
bloc(42, 0, 8, 4, E, HC, plafond=True)                         # le plafond arraché après 8 m
mur('z', 2, 38, 43, HC)                                        # ce qui reste du mur sud
for x, l in ((47, 2), (53, 1.5)):
    bloc(x, 2.2, l, 0.4, 1.2, 0, 'rouille')                    # les bords déchirés
modele('12-Exterieur/debris-1', 51, 8, [7, 4, 7], 1)
modele('04-Pirates/pirate-eva', 52, -0.8, [0, 1.9, 0], rot=-90)
modele('09-Informatique/cables', 44, -1.6, [3, 0, 3], 1.6)
for x in (40, 44):
    gyrophare(x, -1.8, 3.6)
etiquette('Ou par ici, en combinaison', 48, 0, 3.0, 0.5, 'ambre')

# ================================================================== 5 · le hangar (Monolithe 60 %)
HX0, HX1, HZ, HH = 58, 118, 20, 30
espace(HX0, HX1, -HZ, HZ, HH, portes={'o': [grande_porte(0)], 's': [(78.4, 2.4, 14.6, 'porte', 12.0)],
                                       'n': [scellee(74, 8, 10), (104, 4, 3, 'porte', 0.0)], 'e': [(0, 30, 20, 'champ', 0.0)]},
       nom='H · Hangar', police=2.8, chantier=True, nervures=True, couche='chantier')
dalles(76, 108, -12, 12, 0.0)                                       # l'aire d'envol
for (z0, z1) in ((-HZ, -HZ + 3), (HZ - 3, HZ)):                # les galeries à 12 m, au nord et au sud
    dalle(HX0 + 0.1, HX1 - 0.1, z0 + (0.1 if z0 < 0 else 0), z1 - (0 if z0 < 0 else 0.1), 12)
    garde_corps('z', z1 if z0 < 0 else z0, HX0 + 3, HX1 - 0.1, 12, [] if z0 < 0 else [(74.6, 77.0)])
dalle(HX0 + 0.1, HX0 + 3, -HZ + 3, HZ - 3, 12)                  # la galerie ouest, qui les relie
garde_corps('x', HX0 + 3, -HZ + 3, HZ - 3, 12, [(-8, -4)])
escalier(61.6, 16, 48, 0.25, 0.3, 2.0)                         # de l'aire d'envol à la galerie sud
dalle(HX0 + 3, HX0 + 6, -8, -4, 12)                            # le poste de contrôle, en saillie sur la galerie ouest
bloc(HX0 + 6, -6, 0.06, 4, 2.4, 12, 'verre')
for z in (-8, -4):
    bloc(HX0 + 4.5, z, 3, 0.06, 2.4, 12, 'verre')
modele('09-Informatique/console-murale', HX0 + 5.6, -6, [1.2, 1.2, 0.3], 13.0, -90)
sodium(HX0 + 4.5, -6, 14.3, 'z', 1.6)
etiquette('Poste de contrôle : ouvrir le sas sur le vide', HX0 + 4.5, -6, 15.4, 0.45, 'ambre')
modele('11-Vaisseaux/navette-a-voler', 92, 0, [11, 5, 14])
for x, z in ((84, -9), (100, -9), (84, 9), (100, 9)):
    bloc(x, z, 0.4, 0.4, 8, 0, 'sombre')                       # les mâts des projecteurs
    neon(x, z, 8, 1.6, 'x', 'blanc', section=(0.4, 0.4))       # projecteurs froids : la navette est la zone la plus claire
for s in (-1, 1):
    ligne_froide(HX0 + 4, HX1 - 4, s * 8, HH - 0.5)
for x in range(62, 118, 8):
    for z in (-19.6, 19.6):
        sodium(x, z, 3.5)                                      # les lampes de travail le long des murs
for (z, s) in ((-HZ, 1), (HZ, -1)):
    tuyaux('z', z, HX0 + 1, HX1 - 1, s, ys=(2.8, 3.2))
modele('11-Vaisseaux/navette-transfert', 70, -11, [8, 4, 11])
for z in (13, 16.5):
    modele('11-Vaisseaux/capsule-sauvetage', 115, z, [2.4, 2.6, 3])
# le côté des gardes (ouest) et le côté des pirates (est, près du tube d'abordage)
for x, z, l, p in ((68, -2, 1, 3), (72, 5, 3, 1), (66, 9, 3, 1), (106, -6, 3, 1), (110, 2, 1, 3), (104, 8, 3, 1)):
    bloc(x, z, l, p, 1.2)                                      # abris
for x, z in ((64, -3), (69, 3), (63, 12), (70, 13)):
    humain(x, z, 'bleu')
humain(HX0 + 4, -5, 'bleu', 12)
for x, z in ((64, 11), (66.2, 11)):
    modele('10-Hangar/caisses', x, z, [2, 2, 2])
modele('10-Hangar/caisse-armes', 64, 6, [1.8, 1.0, 1.0])
modele('04-Pirates/pirate-chef', 108, -3, [0, 1.95, 0], rot=-90)
for x, z in ((111, -8), (106, 4), (112, 7), (104, -12)):
    humain(x, z, 'orange')
for x, z in ((109, 12), (107.6, 12.6)):
    modele('10-Hangar/bidon-carburant', x, z, [0, 0.8, 0])
for x, z in ((110.5, -14), (111.5, -14)):
    modele('10-Hangar/cellule-energie', x, z, [0, 1.0, 0])
for x, z in ((108, 14), (113, -16)):
    neon(x, z, 0.05, 1.4, 'x', 'ambre', section=(0.6, 0.8))     # les incendies des pirates
    neon(x, z, 0.5, 0.8, 'y', 'rouge', section=(0.4, 0.4))
for x in (100, 108):
    gyrophare(x, -19.5, 10)
neon(HX1 - 0.15, 0, 20.1, 30, 'z', 'blanc', section=(0.08, 0.08))   # le cadre du sas sur le vide
neon(HX1 - 0.4, 0, 0.02, 30, 'z', 'ambre', section=(0.04, 0.3))     # la ligne d'envol
etiquette('5 · Observer d\'en haut, puis choisir', 80, 18.5, 15.2, 0.6, 'ambre')
etiquette('La navette à voler', 92, 0, 7, 0.8, 'ambre')
etiquette('Sas sur le vide : il aspire les ennemis', HX1 - 1, 0, 21.5, 0.6, 'ambre')
etiquette('6 · Le saut', 140, 0, 8, 1.2, 'ether')
# le tube d'abordage et le vaisseau des pirates
espace(102, 106, -28, -20, 3, 'eo', chantier=True, couche='vide')
modele('11-Vaisseaux/vaisseau-pirate', 104, -42, [24, 10, 18])
etiquette('Vaisseau pirate amarré', 104, -42, 12, 1.2, 'orange')

# ================================================================== le plan : les chemins
CHEMINS += [
    ('principal', [(0, -31), (2.6, -27), (0, -16), (0, -8), (-8, -8), (-8, 0), (-18, 0), (-42, 0), (-56, 4), (-60, 9), (-60, 20), (-50, 25), (-38, 24),
                   (-10, 24), (0, 28), (0, 38), (0, 46), (60, 46), (60, 21.2), (76.4, 21.2), (78.4, 18.5), (76, 16), (62, 16), (70, 8), (86, 2)]),
    ('autre', [(-32, 10), (-32, 24), (-32, 30)]),                       # les conduits sous la nef, vers la consigne
    ('autre', [(-38, 19), (-32, 19)]),
    ('autre', [(4, -33.2), (10.4, -33.2), (10.4, -22)]),                # le conduit d'aération de la cellule
    ('autre', [(8, 0), (14, 0), (26, -9), (32, -10.7)]),                 # le courant, dans la salle des machines
    ('autre', [(38, 0), (58, 0)]),                                       # la coursive éventrée
    ('raccourci', [(-14, 22), (-14, 0)]),
    ('facultatif', [(-14.4, -2), (-14.4, -22), (-28, -25), (-40, -29), (-78, -29), (-78, -10), (-86, 0)]),
]
VUES = [  # les points de vue clés : (nom, position, direction regardée)
    ('1', 'Révélation du cœur', (0, -9.2), (0, 0)),
    ('2', 'Première vue de la nef', (-19.5, 0), (-60, 0)),
    ('3', 'Les machines', (16, 8.5), (26, -1)),
    ('4', 'La chambre des reliques', (-8.5, 24), (0, 24)),
    ('5', 'La sortie sur la coque', (0, 44), (60, 30)),
    ('6', 'Le hangar, d\'en haut', (80, 18.5), (92, 0)),
]

# ================================================================== écrire la maquette dans l'atelier et pour Unity
MAQUETTE = {
    'vaisseau-prison': {
        'nom': 'Vaisseau-prison (prologue)',
        'description': 'Le Colosse : le niveau du prologue à l\'échelle de la bible, du réveil au laboratoire jusqu\'au hangar.',
        'decor': 'espace',
        'vue': {'camera': [20, 150, 150], 'cible': [10, 0, 4]},
        'depart': {'position': [0.0, 0, -31.5], 'lacet': 180},   # dans la cellule d'observation, face à la vitre
        'pieces': pieces,
    }
}

chemin = os.path.join(RACINE, 'vitrine', 'atelier-prologue.html')
page = open(chemin, encoding='utf-8').read()
donnees = json.dumps(MAQUETTE, ensure_ascii=False, separators=(',', ':'))
page, n = re.subn(r'/\*MAQUETTES-DEBUT\*/.*?/\*MAQUETTES-FIN\*/', lambda _: '/*MAQUETTES-DEBUT*/' + donnees + '/*MAQUETTES-FIN*/', page, flags=re.S)
assert n == 1, 'repères MAQUETTES introuvables'
tailles = json.dumps(TAILLES, ensure_ascii=False, separators=(',', ':'))
page, n = re.subn(r'/\*TAILLES-DEBUT\*/.*?/\*TAILLES-FIN\*/', lambda _: '/*TAILLES-DEBUT*/' + tailles + '/*TAILLES-FIN*/', page, flags=re.S)
assert n == 1, 'repères TAILLES introuvables'
open(chemin, 'w', encoding='utf-8').write(page)
mq = MAQUETTE['vaisseau-prison']
export = {
    'format': 'echos-ether/atelier', 'version': 1,
    'repere': "Mètres, Y vers le haut, repère glTF (droitier). Rotations en degrés, ordre XYZ. Origine de chaque pièce : centre du bas de sa boîte englobante "
              "à l'échelle 1 (le modèle est décalé pour y poser ses pieds). Un modèle avec un cadre est d'abord réduit ou agrandi, sans déformation, "
              "pour tenir dans le cadre (0 = côté libre), puis multiplié par echelle. Un bloc mesure dims (L, H, P). "
              "Les blocs marqués plafond sont cachés en vue de dessus ; scelle et champ se traversent ; les étiquettes flottent face à la caméra.",
    'decor': mq['decor'], 'depart': mq['depart'], 'pieces': pieces,
}
with open(os.path.join(RACINE, 'docs', 'prologue', 'maquette-vaisseau-prison.json'), 'w', encoding='utf-8') as f:
    json.dump(export, f, ensure_ascii=False, indent=0)


# ================================================================== le plan à l'échelle (SVG), tiré des mêmes pièces
def plan_svg():
    X0, X1, Z0, Z1, K, M = -102, 146, -46, 66, 6.0, 40
    W, H = (X1 - X0) * K + 2 * M, (Z1 - Z0) * K + 2 * M + 70
    px = lambda x: M + (x - X0) * K
    pz = lambda z: M + 50 + (z - Z0) * K
    o = [f'<rect width="{W:.0f}" height="{H:.0f}" fill="#0e1016"/>',
         f'<text x="{M}" y="34" font-size="22" font-weight="700" fill="#f2b26b">Le Colosse · plan du prologue à l\'échelle</text>',
         f'<text x="{M}" y="56" font-size="13" fill="#a7a3b4">Vu de dessus, proue à gauche. Une case = 4 m. Tiré des pièces de la maquette (outils/maquette-prologue.py).</text>']
    for x in range(-100, 147, 4):
        o.append(f'<line x1="{px(x):.0f}" x2="{px(x):.0f}" y1="{pz(Z0):.0f}" y2="{pz(Z1):.0f}" stroke="{"#232731" if x % 20 == 0 else "#15181e"}"/>')
        if x % 20 == 0:
            o.append(f'<text x="{px(x) + 2:.0f}" y="{pz(Z1) + 12:.0f}" font-size="10" fill="#5d5a6a">{x}</text>')
    for z in range(-44, 67, 4):
        o.append(f'<line x1="{px(X0):.0f}" x2="{px(X1):.0f}" y1="{pz(z):.0f}" y2="{pz(z):.0f}" stroke="{"#232731" if z % 20 == 0 else "#15181e"}"/>')
        if z % 20 == 0:
            o.append(f'<text x="{px(X0) + 2:.0f}" y="{pz(z) - 2:.0f}" font-size="10" fill="#5d5a6a">{z}</text>')
    o.append(f'<rect x="{px(-99):.0f}" y="{pz(-41):.0f}" width="{220 * K:.0f}" height="{92 * K:.0f}" fill="#13161c" stroke="#2a2f38" stroke-width="2" rx="6"/>')
    fonds = {'mono': '#1f242c', 'coursive': '#22262e', 'chantier': '#2b251c', 'bat': '#0f2324', 'vide': '#2a1414', 'conduit': '#2b2418', 'fac': '#1b1c22'}
    for (x0, x1, z0, z1, h, nom, couche) in ZONES:
        o.append(f'<rect x="{px(x0):.1f}" y="{pz(z0):.1f}" width="{(x1 - x0) * K:.1f}" height="{(z1 - z0) * K:.1f}" fill="{fonds[couche]}"'
                 + (' stroke="#6b6878" stroke-dasharray="4 3"' if couche == 'fac' else '') + '/>')
    couleurs = {'verre': '#6fa8d8', 'scelle': '#ff4a3a', 'champ': '#e8eef6', 'vertdegris': '#7d8a83', 'rouille': '#a0603a', 'ether': '#5fe0d8', 'sombre': '#4a4f58', 'tole': '#555a62'}
    for q in pieces:
        if q['cle'] != 'bloc' or q.get('plafond'):
            continue
        (x, y, z), (l, h, p) = q['position'], q['dims']
        if h < 0.9 or y > 13 or y < -0.5 or (l > 30 and p > 30):
            if not (h <= 0.4 and y >= 3 and l * p > 4):           # les galeries et les niveaux, en contour
                continue
            o.append(f'<rect x="{px(x - l / 2):.1f}" y="{pz(z - p / 2):.1f}" width="{l * K:.1f}" height="{p * K:.1f}" fill="none" stroke="#3d434d" stroke-width="1"/>')
            continue
        t = q.get('teinte', 'acier')
        o.append(f'<rect x="{px(x - l / 2):.1f}" y="{pz(z - p / 2):.1f}" width="{max(l * K, 1.8):.1f}" height="{max(p * K, 1.8):.1f}" fill="{couleurs.get(t, "#9aa3b0")}"/>')
    styles = {'principal': 'stroke="#f2b26b" stroke-width="3.5"', 'autre': 'stroke="#a7a3b4" stroke-width="2" stroke-dasharray="7 5"',
              'raccourci': 'stroke="#ffb35c" stroke-width="2" stroke-dasharray="2 4"', 'facultatif': 'stroke="#6b6878" stroke-width="1.6" stroke-dasharray="3 4"'}
    for genre, pts in CHEMINS:
        d = 'M' + ' L'.join(f'{px(x):.1f},{pz(z):.1f}' for x, z in pts)
        o.append(f'<path d="{d}" fill="none" {styles[genre]} stroke-linejoin="round"/>')
    camps = {'bleu': '#c8322a', 'orange': '#ff8a3a', 'jaune': '#d9b04a'}
    for q in pieces:
        if q['cle'] == 'humain':
            x, y, z = q['position']
            o.append(f'<circle cx="{px(x):.1f}" cy="{pz(z):.1f}" r="3.4" fill="{camps.get(q.get("teinte"), "#888")}" stroke="#0e1016"/>')
    for num, nom, (x, z), (tx, tz) in VUES:
        import math
        a = math.atan2(tz - z, tx - x)
        pts = [(px(x), pz(z))] + [(px(x) + 34 * math.cos(a + d), pz(z) + 34 * math.sin(a + d)) for d in (-0.4, 0.4)]
        o.append('<polygon points="' + ' '.join(f'{u:.1f},{v:.1f}' for u, v in pts) + '" fill="#e8eef6" opacity="0.25"/>')
        o.append(f'<circle cx="{px(x):.1f}" cy="{pz(z):.1f}" r="8" fill="#e8eef6"/><text x="{px(x):.1f}" y="{pz(z) + 4:.1f}" font-size="11" font-weight="700" text-anchor="middle" fill="#0e1016">{num}</text>')
    for (x0, x1, z0, z1, h, nom, couche) in ZONES:
        if not nom:
            continue
        cx = px((x0 + x1) / 2)
        if min(x1 - x0, z1 - z0) <= 4.5:                          # une coursive : son nom court, sans les mesures
            o.append(f'<text x="{cx:.1f}" y="{pz((z0 + z1) / 2) + 4:.1f}" font-size="10" font-weight="600" text-anchor="middle" fill="#ece6d8">{nom.split(" : ")[0]}</text>')
            continue
        y = pz(z0 + 0.3 * (z1 - z0))
        o.append(f'<text x="{cx:.1f}" y="{y + 4:.1f}" font-size="{13 if (x1 - x0) > 12 else 10}" font-weight="600" text-anchor="middle" fill="#ece6d8">{nom}</text>')
        if h:
            o.append(f'<text x="{cx:.1f}" y="{y + 18:.1f}" font-size="10" text-anchor="middle" fill="#a7a3b4">{(x1 - x0):g} × {(z1 - z0):g} m · {h:g} m de haut</text>')
    for x, z, t in ((0, -36.5, 'Départ'), (30, 50.5, 'La coque extérieure (le vide)'), (140, 0, 'Le saut'), (104, -32, 'Tube d\'abordage')):
        o.append(f'<text x="{px(x):.0f}" y="{pz(z):.0f}" font-size="11" text-anchor="middle" fill="#a7a3b4">{t}</text>')
    ly = pz(Z1) + 34
    leg = [('<line x1="0" y1="0" x2="34" y2="0" stroke="#f2b26b" stroke-width="3.5"/>', 'Chemin principal'),
           ('<line x1="0" y1="0" x2="34" y2="0" stroke="#a7a3b4" stroke-width="2" stroke-dasharray="7 5"/>', 'Autre chemin'),
           ('<line x1="0" y1="0" x2="34" y2="0" stroke="#ffb35c" stroke-width="2" stroke-dasharray="2 4"/>', 'Raccourci (s\'ouvre d\'un côté)'),
           ('<line x1="0" y1="0" x2="34" y2="0" stroke="#6b6878" stroke-width="1.6" stroke-dasharray="3 4"/>', 'Zones facultatives'),
           ('<circle cx="8" cy="0" r="8" fill="#e8eef6"/>', 'Point de vue clé'),
           ('<circle cx="6" cy="0" r="4" fill="#c8322a"/><circle cx="18" cy="0" r="4" fill="#ff8a3a"/><circle cx="30" cy="0" r="4" fill="#d9b04a"/>', 'Gardes, pirates, détenus'),
           ('<rect x="0" y="-5" width="34" height="10" fill="#ff4a3a"/>', 'Porte scellée')]
    for i, (s, t) in enumerate(leg):
        o.append(f'<g transform="translate({M + i * 205},{ly:.0f})">{s}<text x="42" y="4" font-size="12" fill="#a7a3b4">{t}</text></g>')
    o.append(f'<g transform="translate({W - M - 20 * K - 10:.0f},{ly + 26:.0f})"><rect width="{10 * K:.0f}" height="6" fill="#ece6d8"/><rect x="{10 * K:.0f}" width="{10 * K:.0f}" height="6" fill="#5d5a6a"/>'
             f'<text y="-4" font-size="11" fill="#a7a3b4">0</text><text x="{10 * K:.0f}" y="-4" font-size="11" fill="#a7a3b4">10</text><text x="{20 * K:.0f}" y="-4" font-size="11" fill="#a7a3b4">20 m</text></g>')
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W:.0f} {H:.0f}" width="{W:.0f}" height="{H:.0f}" font-family="system-ui, -apple-system, Segoe UI, sans-serif">{"".join(o)}</svg>'


open(os.path.join(RACINE, 'docs', 'prologue', 'plan-niveau.svg'), 'w', encoding='utf-8').write(plan_svg())


def modeles_par_zone():
    """Quel modèle va où : écrit dans docs/prologue/PLAN-NIVEAU.md, entre les repères MODELES-DEBUT et MODELES-FIN."""
    nommees = sorted(((x0, x1, z0, z1, nom.split(' : ')[0]) for (x0, x1, z0, z1, h, nom, c) in ZONES if nom), key=lambda z: (z[1] - z[0]) * (z[3] - z[2]))
    par_zone = {}
    for q in pieces:
        if q['type'] != 'modele' or q['cle'] == '06-Couloirs/sols':
            continue
        x, y, z = q['position']
        zone = next((n for (x0, x1, z0, z1, n) in nommees if x0 <= x <= x1 and z0 <= z <= z1), 'Dehors : la coque et l\'espace')
        if q['cle'] == '06-Couloirs/couloir-principal':
            zone = 'Coursives'
        nom = q['cle'].split('/')[1]
        par_zone.setdefault(zone, {}).setdefault(nom, 0)
        par_zone[zone][nom] += 1
    lignes = ['| Zone | Modèles |', '|---|---|']
    for zone, m in par_zone.items():
        lignes.append(f'| {zone} | ' + ', '.join(f'`{k}`' + (f' × {n}' if n > 1 else '') for k, n in sorted(m.items())) + ' |')
    n_sols = sum(1 for q in pieces if q['type'] == 'modele' and q['cle'] == '06-Couloirs/sols')
    lignes.append(f'\nEt {n_sols} dalles de sol (`sols`), dans la passerelle, la salle des machines et l\'aire d\'envol du hangar. '
                  'Chaque modèle est posé à sa taille réelle (`tailles-modeles.json`) ; le bouton Contrôle de l\'atelier vérifie qu\'aucun ne chevauche un mur.')
    chemin_plan = os.path.join(RACINE, 'docs', 'prologue', 'PLAN-NIVEAU.md')
    if os.path.exists(chemin_plan):
        texte = open(chemin_plan, encoding='utf-8').read()
        texte, n = re.subn(r'<!--MODELES-DEBUT-->.*?<!--MODELES-FIN-->', lambda _: '<!--MODELES-DEBUT-->\n' + '\n'.join(lignes) + '\n<!--MODELES-FIN-->', texte, flags=re.S)
        if n:
            open(chemin_plan, 'w', encoding='utf-8').write(texte)


modeles_par_zone()
types = {}
for q in pieces:
    types[q['cle'] if q['type'] == 'forme' else 'modele'] = types.get(q['cle'] if q['type'] == 'forme' else 'modele', 0) + 1
print(f'{len(pieces)} pièces', types, f'{len(donnees) // 1024} Ko')
