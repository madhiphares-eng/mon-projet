#!/usr/bin/env python3
"""Maquette du prologue : le vaisseau-prison en formes grises, à l'échelle réelle,
d'après le plan docs/prologue/plan-vaisseau-prison.svg.

Écrit la maquette dans vitrine/atelier-prologue.html, entre les repères MAQUETTES-DEBUT et MAQUETTES-FIN,
et dans docs/prologue/maquette-vaisseau-prison.json (le format « Exporter » de l'atelier, pour Unity).
Usage : python3 outils/maquette-prologue.py

Repère : mètres. X va de la proue (à gauche du plan) vers la poupe, Z vers le bas du plan, Y vers le haut.
Le pont est en Y = 0 : les sols sont posés dessous, les murs dessus.
Les modèles du prologue ont un cadre : l'atelier les met à l'échelle pour y tenir (0 = côté libre).
"""
import json
import os
import re

RACINE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TAILLES = {k: v for k, v in json.load(open(os.path.join(RACINE, 'docs', 'prologue', 'tailles-modeles.json'), encoding='utf-8')).items() if not k.startswith('_')}
E = 0.2      # épaisseur des murs, des sols et des plafonds
HC = 3.5     # hauteur des coursives
pieces = []


def r(v):
    return round(v, 3)


def bloc(x, z, l, p, h, y=0.0, teinte='acier', plafond=False):
    """Bloc de l (X) × h (Y) × p (Z), centré en (x, z), le bas à la hauteur y. Par défaut en acier : la matière du vaisseau."""
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
    q = {'type': 'forme', 'cle': 'humain', 'position': [r(x), r(y), r(z)]}
    if camp:
        q['teinte'] = camp
    pieces.append(q)


def etiquette(texte, x, z, y, police=1.0, teinte=None):
    q = {'type': 'forme', 'cle': 'etiquette', 'position': [r(x), r(y), r(z)], 'texte': texte, 'police': police}
    if teinte:
        q['teinte'] = teinte
    pieces.append(q)


def mur(axe, ligne, a, b, h, ouvertures=(), y0=0.0, teinte='acier', prolonger=True):
    """Mur sur la ligne z = ligne (axe 'z', il court le long de X) ou x = ligne (axe 'x', le long de Z), de a à b.
    Ouverture : (centre, largeur, haut, genre, bas). Genres : porte, passage (toute la hauteur), fenetre (verre),
    scellee (panneau rouge qu'on traverse en visite), champ (champ de force)."""
    def plein(u0, u1, v0, v1, t=teinte, ep=E):
        if u1 - u0 < 0.01 or v1 - v0 < 0.01:
            return
        if axe == 'z':
            bloc((u0 + u1) / 2, ligne, u1 - u0, ep, v1 - v0, y0 + v0, t)
        else:
            bloc(ligne, (u0 + u1) / 2, ep, u1 - u0, v1 - v0, y0 + v0, t)
    if axe == 'z' and prolonger:   # les murs le long de X débordent d'une demi-épaisseur : les angles sont fermés
        a, b = a - E / 2, b + E / 2
    cur = a
    for o in sorted(ouvertures):
        centre, largeur, haut, *reste = o
        genre, bas = (list(reste) + ['porte', 0.0][len(reste):])[:2]
        if genre == 'passage':
            haut = h
        o0, o1 = max(a, centre - largeur / 2), min(b, centre + largeur / 2)
        plein(cur, o0, 0, h)
        plein(o0, o1, haut, h)
        plein(o0, o1, 0, bas)
        if genre in ('fenetre', 'scellee', 'champ'):
            plein(o0, o1, bas, haut, {'fenetre': 'verre', 'scellee': 'scelle', 'champ': 'champ'}[genre], 0.08)
        cur = o1
    plein(cur, b, 0, h)


def espace(x0, x1, z0, z1, h, murs='nseo', portes=None, sol='tole', plafond=True, nom=None, police=1.8, teinte_nom=None):
    """Une salle ou une coursive : sol, plafond, murs des côtés demandés (n = Z min, s = Z max, o = X min, e = X max)."""
    portes = portes or {}
    cx, cz = (x0 + x1) / 2, (z0 + z1) / 2
    if sol:
        bloc(cx, cz, x1 - x0, z1 - z0, E, -E, sol)
    if plafond:
        bloc(cx, cz, x1 - x0, z1 - z0, E, h, plafond=True)
    for cote, (axe, ligne, a, b) in {'n': ('z', z0, x0, x1), 's': ('z', z1, x0, x1), 'o': ('x', x0, z0, z1), 'e': ('x', x1, z0, z1)}.items():
        if cote in murs:
            mur(axe, ligne, a, b, h, portes.get(cote, ()))
    if nom:
        etiquette(nom, cx, cz, h + 1.0, police, teinte_nom)


def neon(x, z, y, l, axe='x', teinte='blanc', plafond=False, section=(0.06, 0.12)):
    """Un tube lumineux de longueur l, le long de X, de Z ou debout (Y), le bas à la hauteur y ; plafond : caché en vue de dessus."""
    h, p_ = section
    dims = {'x': [r(l), h, p_], 'z': [p_, h, r(l)], 'y': [p_, r(l), p_]}[axe]
    q = {'type': 'forme', 'cle': 'neon', 'position': [r(x), r(y), r(z)], 'dims': dims, 'teinte': teinte}
    if plafond:
        q['plafond'] = True
    pieces.append(q)


def eclairer(x0, x1, z0, z1, h, teinte='blanc', pas=4.0, ecart=5.0, longueur=1.6, y=None):
    """Des néons au plafond, alignés sur le grand côté : une rangée par tranche de ecart mètres. Ils restent visibles en vue de dessus : c'est le plan des lumières."""
    long_x = (x1 - x0) >= (z1 - z0)
    a0, a1, b0, b1 = (x0, x1, z0, z1) if long_x else (z0, z1, x0, x1)
    rangs = max(1, round((b1 - b0) / ecart))
    n = max(1, int((a1 - a0) // pas))
    for i in range(rangs):
        b = b0 + (i + 0.5) * (b1 - b0) / rangs
        for k in range(n):
            a = a0 + (k + 0.5) * (a1 - a0) / n
            x, z = (a, b) if long_x else (b, a)
            neon(x, z, h - 0.1 if y is None else y, min(longueur, (a1 - a0) / n - 0.4), 'x' if long_x else 'z', teinte)


def dalles(x0, x1, z0, z1, y=-0.04):
    """Le sol modulaire (6 × 4 m) posé en damier, étiré pour couvrir juste la salle."""
    nx, nz = max(1, round((x1 - x0) / 6)), max(1, round((z1 - z0) / 4))
    lx, lz = (x1 - x0) / nx, (z1 - z0) / nz
    for i in range(nx):
        for k in range(nz):
            modele('06-Couloirs/sols', x0 + (i + 0.5) * lx, z0 + (k + 0.5) * lz, y=y, rot=180 if (i + k) % 2 else 0, echelle=[lx / 6, 1, lz / 4])


def couloir_habille(a0, a1, c, axe='x'):
    """Un tronçon du couloir détaillé (3 m de large, 10,56 m de long à l'échelle), étiré pour aller de a0 à a1."""
    longueur = a1 - a0
    x, z = ((a0 + a1) / 2, c) if axe == 'x' else (c, (a0 + a1) / 2)
    modele('06-Couloirs/couloir-principal', x, z, rot=90 if axe == 'x' else 0, echelle=[1, 1, longueur / 10.56])


def escalier(x0, z, n, montee, giron, largeur, y0=0.0, sens=1):
    """Marches qui montent le long de X (sens = 1 vers +X), centrées en z."""
    for i in range(n):
        bloc(x0 + sens * (i + 0.5) * giron, z, giron, largeur, (i + 1) * montee, y0)


porte = lambda c, l=2.4, h=2.8: (c, l, h, 'porte', 0.0)
passage = lambda c, l=3.0: (c, l, HC, 'passage', 0.0)
trappe = lambda c: (c, 1.2, 2.0, 'porte', 0.0)          # entrée d'un conduit de maintenance
conduit = lambda c: (c, 1.2, 2.0, 'passage', 0.0)

# ------------------------------------------------------------------ la coque : le vaisseau se lit d'un seul bloc
bloc(4, 2.5, 128, 65, 1.0, -1.2, 'sombre')                    # sous tout le pont
bloc(-63, 0, 6, 16, 1.4, -1.2, 'sombre')                      # la proue
bloc(-68, 0, 4, 8, 1.0, -1.2, 'sombre')
for z in (-24, -16, -8):                                       # les réacteurs, à la poupe
    bloc(71, z, 6, 6, 6, -1.2, 'sombre')
    bloc(74.5, z, 1, 4.5, 4.5, -0.45, 'ether')
neon(-69.9, -3, 0.2, 0.6, 'z', 'rouge')                         # feux de position : bâbord rouge, tribord vert
neon(-69.9, 3, 0.2, 0.6, 'z', 'vert')


# ------------------------------------------------------------------ passerelle (proue)
espace(-54, -39, -9, 9, 5, portes={'e': [porte(0, 3, 3.0)], 'o': [(0, 12, 4.2, 'fenetre', 1.0)]}, sol=None, nom='Passerelle', police=2.2)
dalles(-54, -39, -9, 9)
eclairer(-54, -39, -9, 9, 5, 'blanc', ecart=6)
modele('06-Couloirs/porte-coulissante', -47, 8.5, rot=90)       # porte vers les quartiers du commandant (décor)
for z in (-6.5, 6.5):
    neon(-53.85, z, 1.0, 5, 'z', 'cyan')                       # sous la baie vitrée
bloc(-49, 0, 6, 8, 0.5)                                        # estrade du commandement
bloc(-45.5, 0, 1, 3, 0.25)
for x, z in ((-52, -5), (-52, 5), (-47, -7.6), (-47, 7.6)):
    bloc(x, z, 1.2, 0.8, 1.0)                                  # pupitres
modele('09-Informatique/terminal', -51.2, -2.5, [1.6, 0, 1.8], 0.5)
modele('09-Informatique/terminal', -51.2, 2.5, [1.6, 0, 1.8], 0.5)
modele('09-Informatique/console-murale', -44.5, -8.75, [1.5, 1.5, 0.4], 1.0)
modele('09-Informatique/console-murale', -40.0, -8.6, [1.5, 1.5, 0.4], 1.0)
modele('03-Gardes/garde', -49, 1.5, [0, 1.85, 0], 0.5, 90)
for x, z in ((-48, -2), (-47, -4.5), (-50, -3)):
    humain(x, z, 'bleu', 0.5 if -52 <= x <= -46 and -4 <= z <= 4 else 0)
for x, z in ((-41, 3), (-42, 5.5)):
    humain(x, z, 'orange')
etiquette('Gardes retranchés, les pirates attaquent', -46.5, 0, 3.4, 0.5)
for x, z in ((-52, -5), (-52, 5), (-47, -7.6), (-47, 7.6)):
    neon(x, z, 1.0, 1.0, 'x' if abs(z) > 7 else 'z', 'cyan', section=(0.03, 0.05))   # le bord des pupitres

# ------------------------------------------------------------------ coursive ouest, officiers, consigne
espace(-39, -9, -1.5, 1.5, HC, 'ns', {'n': [passage(-27), trappe(-12)], 's': [passage(-27), trappe(-12)]})
espace(-28.5, -25.5, -12, -1.5, HC, 'eo')
espace(-28.5, -25.5, 1.5, 12, HC, 'eo')
couloir_habille(-39, -28.5, 0)
couloir_habille(-25.5, -12.6, 0)
couloir_habille(-12, -1.5, -27, 'z')
couloir_habille(1.5, 12, -27, 'z')
for (a0, a1) in ((-11.4, -9), (-28.5, -25.5)):
    eclairer(a0, a1, -1.5, 1.5, HC, 'blanc', pas=2.4)
for z in (-1.32, 1.32):                                        # le liseré qui guide au sol
    neon(-27, z, 0.02, 2.6, 'x', 'cyan', section=(0.03, 0.04))

espace(-37, -17, -26, -12, 4, portes={'s': [porte(-27)]}, nom='Officiers : quartiers, armurerie')
for x in (-34, -30.5):
    modele('05-Cellules/lit-superpose', x, -24.6, [2.1, 1.9, 2.1])
bloc(-20, -24.9, 4, 2, 0.6)                                    # râtelier
modele('10-Hangar/caisse-armes', -20, -22.4, [1.8, 1.0, 1.0])
modele('14-Accessoires/caisse-munitions', -22.6, -22.4, [0, 0, 0.8])
bloc(-23, -17, 2, 1, 0.9)
modele('13-Armes/fusil-gardes', -23.3, -17, [1.0, 1.0, 1.0], 0.9, 90)
modele('13-Armes/pistolet-gardes', -22.3, -16.8, [0.25, 0.25, 0.25], 0.9)
bloc(-29.5, -15.5, 2, 1, 0.9)                                  # bureau, et le badge posé dessus
bloc(-29.5, -15.5, 0.3, 0.2, 0.05, 0.9, 'ambre')
etiquette('Badge de la Conservatrice', -29.5, -15.5, 1.5, 0.45, 'ambre')
humain(-32, -17, 'bleu')
eclairer(-37, -17, -26, -12, 4, 'blanc', ecart=7)

espace(-37, -17, 12, 24, 4, portes={'n': [porte(-27)]}, nom='Consigne : effets saisis')
modele('07-Consigne/porte-coffre', -21, 21, [2.8, 2.8, 0])        # la chambre forte
for i in range(8):
    modele('07-Consigne/casier', -35.2 + i * 1.65, 23.4, [1.6, 2.0, 0], rot=180)
for z in (16, 19.5):
    bloc(-27, z, 6, 0.6, 2.2)                                  # rayonnages
modele('10-Hangar/caisse-armes', -20, 15, [1.8, 1.0, 1.0])
modele('14-Accessoires/extincteur', -36.5, 13, [0, 0.6, 0])
eclairer(-37, -17, 12, 24, 4, 'blanc', ecart=6)

# ------------------------------------------------------------------ l'anneau autour du cœur-relique
espace(-9, 9, -9, -6, HC, 'noe', {'n': [passage(0)]})
espace(-9, 9, 6, 9, HC, 'soe', {'s': [passage(0)]})
espace(-9, -6, -6, 6, HC, 'o', {'o': [passage(0)]})
espace(6, 9, -6, 6, HC, 'e', {'e': [passage(0)]})
espace(-6, 6, -6, 6, 10, portes={'o': [porte(0)], 'e': [porte(0)], 'n': [(0, 8, 3.0, 'fenetre', 1.0)], 's': [(0, 8, 3.0, 'fenetre', 1.0)]},
       sol='tole', nom='Cœur-relique (hyperespace)', police=2.2, teinte_nom='ether')
bloc(0, 0, 2.4, 2.4, 0.8, 0, 'sombre')                         # le socle : pierre noire des Bâtisseurs, lignes cyan
for (x, z, l, p) in ((0, -2.6, 5.3, 0.1), (0, 2.6, 5.3, 0.1), (-2.6, 0, 0.1, 5.3), (2.6, 0, 0.1, 5.3)):
    bloc(x, z, l, p, 0.03, 0, 'ether')                         # lignes de glyphes au sol
modele('15-Reliques/coeur-relique', 0, 0, [3, 3.5, 3], 1.3)
for (x, z, a) in ((0, -1.22, 'x'), (0, 1.22, 'x'), (-1.22, 0, 'z'), (1.22, 0, 'z')):
    neon(x, z, 0.8, 2.44, a, 'cyan', section=(0.04, 0.06))      # le bord du socle : le cœur éclairé par en dessous
modele('15-Reliques/anneau-relais', 0, 0, [5, 5, 5], 4.6)
for x, z in ((-4.5, -4.5), (4.5, -4.5), (-4.5, 4.5), (4.5, 4.5)):
    modele('08-Machines/pylone-energie', x, z, [2.5, 4, 2.5])
etiquette('Il s\'emballe à son réveil', 0, 0, 7.5, 0.6, 'ether')
for (x0, x1, z0, z1) in ((-9, 9, -9, -6), (-9, 9, 6, 9), (-9, -6, -6, 6), (6, 9, -6, 6)):
    eclairer(x0, x1, z0, z1, HC, 'cyan', pas=3)
for (x, z, l, a) in ((0, -5.85, 10, 'x'), (0, 5.85, 10, 'x'), (-5.85, 0, 10, 'z'), (5.85, 0, 10, 'z')):
    neon(x, z, 9.5, l, a, 'cyan', section=(0.08, 0.1))         # la couronne en haut du puits

# ------------------------------------------------------------------ laboratoire et cellule d'observation du héros
espace(-1.5, 1.5, -13, -9, HC, 'eo')
espace(-14, 14, -28, -13, 5, portes={'s': [porte(0), trappe(-12), trappe(11.6)], 'e': [trappe(-24)]}, nom='Laboratoire', police=2.2)
espace(-12.5, -7.5, -26.5, -21.5, 3, portes={'e': [porte(-24, 1.4, 2.4)], 's': [(-10, 4.4, 2.6, 'fenetre', 0.4)]}, sol=None)
bloc(-11.6, -24, 1, 2.2, 0.5)                                  # couchette
modele('05-Cellules/porte-isolement', -7.5, -24, [1.4, 2.4, 0.4], rot=90)
modele('01-Joueur/tenue-detenu', -10.6, -22.4, [0, 1.8, 0])
etiquette('1 · Réveil : départ de la visite', -10, -24, 2.3, 0.4, 'ambre')
for x in (-2, 2):
    modele('15-Reliques/chambre-stase', x, -24, [2.4, 3.0, 2.4])
etiquette('Chambres d\'étude', 0, -24, 3.6, 0.5)
for x in (-4, 4):
    modele('09-Informatique/terminal', x, -16, [1.6, 0, 1.8])
for x in (-1.2, 1.2):
    modele('09-Informatique/console-murale', x, -27.75, [1.5, 1.5, 0.4], 1.0)
modele('09-Informatique/console-murale', -13.6, -18, [1.4, 1.4, 0.4], 1.0, 90)
for (x0, x1) in ((6, 7), (8.2, 13.8)):                         # bureau de la Conservatrice, cloisons vitrées
    bloc((x0 + x1) / 2, -21, x1 - x0, 0.06, 2.4, 0, 'verre')
bloc(6, -24.5, 0.06, 7, 2.4, 0, 'verre')
bloc(10, -25, 2.4, 1.2, 0.8)
modele('09-Informatique/terminal', 10.6, -25.2, [0.6, 0.6, 0.6], 0.8)
modele('15-Reliques/tablette-glyphes', 9.3, -24.8, [0.3, 0, 0], 0.8)
etiquette('Bureau de la Conservatrice', 10, -24, 2.9, 0.45)
eclairer(-14, 14, -28, -13, 5, 'blanc', ecart=5)
for (x, z, l, a) in ((-10, -21.45, 5, 'x'), (-7.45, -24, 5, 'z')):
    neon(x, z, 2.95, l, a, 'cyan', section=(0.04, 0.05))      # le haut des vitres de la cellule
neon(-10, -24, 2.9, 1.2, 'x', 'blanc', plafond=True)            # la lampe de la cellule

# ------------------------------------------------------------------ chambre des reliques, scellée
espace(-1.5, 1.5, 9, 13, HC, 'eo')
etiquette('3 · Porte scellée : courant, badge ou conduit', 0, 11, 2.9, 0.4, 'ambre')
espace(-14, 14, 13, 25, 5, portes={'n': [(0, 2.4, 2.8, 'scellee', 0.0), trappe(-12)], 's': [porte(-4, 2.0, 2.6)]}, nom='Chambre des reliques', police=2.2, teinte_nom='ether')
modele('12-Exterieur/cloison-etanche', 0, 13, rot=90)          # la porte scellée
neon(0, 12.85, 2.95, 2.4, 'x', 'rouge', section=(0.05, 0.06))   # voyant : verrouillée
modele('12-Exterieur/cloison-etanche', -4, 25, [0, 2.6, 0], rot=90)
eclairer(-14, 14, 13, 25, 5, 'cyan', ecart=6)
bloc(-6, 19, 3, 4.2, 0.3, 0, 'ether')
modele('15-Reliques/capsule-veilleur', -6, 19, [2.4, 1.2, 1.2], 0.3, 90)
etiquette('Sa capsule de sommeil', -6, 19, 2.4, 0.45)
bloc(4, 21, 1.4, 1.4, 0.3, 0, 'ether')
modele('01-Joueur/combinaison-veilleur', 4, 21, [0, 2.0, 0], 0.3)
bloc(4, 21, 1.6, 1.6, 2.6, 0.3, 'verre')                       # la vitrine
etiquette('4 · Sa combinaison de Veilleur', 4, 21, 3.4, 0.45, 'ambre')
modele('07-Consigne/armoire-blindee', 9, 24.2, [1.6, 2.4, 1.0], rot=180)
for z in (15, 23):
    bloc(-11, z, 0.8, 0.8, 1.0)
    modele('15-Reliques/tablette-glyphes', -11, z, [0.6, 0, 0], 1.0)
bloc(11, 15, 0.8, 0.8, 1.0)
modele('15-Reliques/prisme-ether', 11, 15, [0, 1.2, 0], 1.0)
bloc(1.5, 24.2, 4, 0.8, 1.0)
for x in (0, 1.5, 3):
    modele('15-Reliques/cristal-ether', x, 24.2, [0.8, 0.8, 0.8], 1.0)
neon(1.5, 23.75, 1.02, 4, 'x', 'cyan', section=(0.02, 0.04))     # le bord de l'étagère des cristaux
neon(4, 21, 2.92, 1.6, 'x', 'blanc', section=(0.02, 0.04))      # la vitrine éclairée par le haut

# ------------------------------------------------------------------ les conduits de maintenance (discrétion)
espace(-12.6, -11.4, -13, -1.5, 2.0, 'eo', sol='tole')       # labo → coursive ouest
espace(-12.6, -11.4, 1.5, 13, 2.0, 'eo', sol='tole')         # coursive ouest → reliques
espace(14, 15, -24.6, -23.4, 2.0, 'ns', sol='tole')          # labo → cellules
espace(11, 12.2, -13, -5.2, 2.0, 'eos', {'e': [conduit(-5.8)]}, sol='tole')   # labo → informatique
espace(12.2, 17, -6.4, -5.2, 2.0, 'ns', sol='tole')
espace(33, 34.2, -10.1, -7, 2.0, 'neo', {'e': [conduit(-9.5)]}, sol='tole')   # informatique → machines
espace(34.2, 41, -10.1, -8.9, 2.0, 'ns', sol='tole')
for x, z in ((-12, -7), (-12, 7), (14.5, -24), (14.6, -5.8), (37.5, -9.5)):
    etiquette('Conduit', x, z, 2.5, 0.4)
for (x, z, l, a) in ((-12, -7.25, 11, 'z'), (-12, 7.25, 11, 'z'), (11.6, -9.1, 7.4, 'z'), (14.6, -5.8, 4.4, 'x'), (37.6, -9.5, 6.4, 'x'), (33.6, -8.55, 2.8, 'z')):
    neon(x, z, 1.9, l, a, 'ambre', plafond=True, section=(0.03, 0.04))

# ------------------------------------------------------------------ sas et coque extérieure (le vide)
espace(-6, -2, 25, 28.5, 3, 'seo', {'s': [porte(-4, 3.0, 2.6)]}, nom='Sas', police=0.6)
modele('12-Exterieur/porte-sas', -4, 28.5)
neon(-4, 26.75, 2.9, 1.6, 'x', 'ambre', plafond=True)
espace(-5.5, -2.5, 28.5, 32, 0, '', sol='tole', plafond=False)
espace(-5.5, 45, 32, 35, 0, '', sol='tole', plafond=False)
etiquette('5 · Le vide : la coque extérieure', 20, 33.5, 1.4, 0.8, 'ambre')
for x in range(-4, 46, 3):                                     # feux de piste le long du chemin
    for z in (32.1, 34.9):
        neon(x, z, 0.02, 0.4, 'x', 'rouge', section=(0.04, 0.06))
modele('12-Exterieur/rocher-spatial', 24, 46, [4, 4, 4], 6)
modele('12-Exterieur/debris-2', 6, 52, [9, 5, 9], 4)

# ------------------------------------------------------------------ informatique, cellules, ateliers
espace(9, 17, -1.5, 1.5, HC, 'ns')
couloir_habille(9, 17, 0)
espace(17, 35, -7, 7, 4, portes={'o': [porte(0, 3, 3.2), trappe(-5.8)], 'e': [porte(0, 3, 3.2)], 'n': [porte(26, 3, 3.2), trappe(33.6)], 's': [porte(26, 3, 3.2)]},
       nom='Salle informatique')
for x in (19.5, 21.5, 30.5, 32.5):
    for z in (-4.8, 4.8):
        modele('09-Informatique/baie-serveur', x, z, [0.8, 2.2, 1.2])
modele('09-Informatique/cables', 21, -6.7, [2, 0, 2], 1.0)
modele('09-Informatique/terminal', 33.5, -2.8, [1.6, 0, 1.8])
modele('09-Informatique/console-murale', 17.4, 4, [1.4, 1.4, 0.4], 1.0, 90)
etiquette('Serveurs en court-circuit · pirater portes et carte', 26, -4.8, 2.9, 0.4)
eclairer(17, 35, -7, 7, 4, 'cyan', ecart=7)
for x in (19.5, 21.5, 30.5, 32.5):
    for z in (-4.1, 4.1):
        neon(x, z, 0.02, 0.7, 'x', 'cyan', section=(0.02, 0.04))   # le pied des baies

espace(24.5, 27.5, -12, -7, HC, 'eo')
espace(15, 37, -28, -12, 4.5, portes={'s': [porte(26, 3, 3.2)], 'o': [trappe(-24)]}, nom='Blocs de cellules : l\'émeute')
LC = 22 / 6
for k in range(6):                                             # rangée nord : 6 cellules
    cx = 15 + (k + 0.5) * LC
    if k:
        bloc(15 + k * LC, -26.5, E, 3, 3.0)
    modele('05-Cellules/lit-superpose', cx - 0.6, -27.1, [2.1, 1.9, 2.1])
    modele('05-Cellules/toilettes', cx + 1.25, -27.4, [0.6, 0.8, 0.7])
    if k % 2:
        modele('05-Cellules/lavabo', cx + 1.35, -26.2, [0.6, 1.0, 0.5], 0.4)
    if k in (0, 2, 5):
        modele('05-Cellules/porte-cellule', cx, -25, [LC - 0.1, 3.0, 0])
    else:
        mur('z', -25, 15 + k * LC, 15 + (k + 1) * LC, 3.0, [porte(cx, 1.2, 2.4)], prolonger=False)
bloc(26, -26.5, 22, 3, E, 3.0, plafond=True)
for (x0, n) in ((15, 2), (37 - 2 * LC, 2)):                    # rangée sud : 4 cellules, la porte au milieu
    for k in range(n):
        cx = x0 + (k + 0.5) * LC
        modele('05-Cellules/lit-superpose', cx - 0.6, -12.9, [2.1, 1.9, 2.1])
    for k in range(n + 1):
        if 15 < x0 + k * LC < 36.9:                            # pas sur les murs de la salle
            bloc(x0 + k * LC, -13.5, E, 3, 3.0)
    mur('z', -15, x0, x0 + n * LC, 3.0, [porte(x0 + (k + 0.5) * LC, 1.2, 2.4) for k in range(n)])
    bloc(x0 + n * LC / 2, -13.5, n * LC, 3, E, 3.0, plafond=True)
bloc(26, -19.5, 3, 1.2, 1.0)                                   # poste des gardes
modele('09-Informatique/terminal', 26, -19.5, [0.6, 0.6, 0.6], 1.0)
modele('03-Gardes/droide-securite', 23.6, -19.5, [0, 2.1, 0])
modele('02-Prisonniers/detenu-brute', 20, -21, [0, 1.95, 0])
modele('02-Prisonniers/detenu-vieux', 31.5, -25.7)               # dans sa cellule, bras en croix : 1,6 m d'envergure
modele('03-Gardes/garde', 27.8, -18, [0, 1.85, 0], rot=180)
for x, z in ((18, -19), (22, -22.5), (24, -17), (31, -21.5)):
    humain(x, z, 'jaune')
for x, z in ((28.5, -20.8), (30, -17.5)):
    humain(x, z, 'bleu')
modele('13-Armes/cle-a-molette', 23, -20, [0.45, 0.45, 0.45], 0.02, 0, 90)   # couchée au sol
etiquette('Arme de fortune', 23, -20, 0.7, 0.35, 'ambre')
etiquette('2 · Émeute : première arme', 26, -21, 3.4, 0.6, 'ambre')
eclairer(15, 37, -25, -15, 4.5, 'blanc', ecart=5)
for k in range(6):                                             # le voyant rouge au-dessus de chaque cellule
    neon(15 + (k + 0.5) * LC, -24.85, 3.1, 0.8, 'x', 'rouge', section=(0.04, 0.05))

espace(24.5, 27.5, 7, 12, HC, 'eo')
espace(15, 37, 12, 27, 6, portes={'n': [porte(26, 3, 3.2)]}, nom='Ateliers : travail forcé, air toxique')
for x, z in ((20, 17), (20, 22), (32, 17), (32, 22)):
    bloc(x, z, 4, 1.4, 0.9)                                    # établis
for y in (2.2, 2.8):                                           # conduites le long du mur (le pack de tuyaux n'est qu'un catalogue de pièces)
    bloc(26, 26.75, 20, 0.3, 0.3, y, 'sombre')
modele('10-Hangar/caisses', 35, 25, [2, 2, 2])
modele('10-Hangar/caisses', 17, 25, [2, 2, 2])
modele('02-Prisonniers/detenu-atelier', 20, 15.8)
modele('02-Prisonniers/detenu-atelier', 32, 20.4)
for x, z in ((24, 18), (29, 23)):
    humain(x, z, 'jaune')
etiquette('Détenus alliés possibles', 26, 20, 2.6, 0.45)
eclairer(15, 37, 12, 27, 6, 'blanc', ecart=7)
for x, z in ((20, 17), (20, 22), (32, 17), (32, 22)):
    neon(x, z - 0.72, 0.85, 3.8, 'x', 'ambre', section=(0.02, 0.03))   # lampes des établis

# ------------------------------------------------------------------ vers la poupe : coursive éventrée, machines, hangar
espace(35, 40, -1.5, 1.5, HC, 'ns')
eclairer(35, 40, -1.5, 1.5, HC, 'blanc', pas=2.4)
etiquette('Porte scellée : le vide derrière', 38.2, 0, 2.9, 0.4, 'rouge')
modele('12-Exterieur/cloison-etanche', 40, 0, [0, 3.2, 0])
neon(39.85, 0, 3.3, 3, 'z', 'rouge', section=(0.05, 0.06))
espace(40, 64, -4.5, 4.5, 4, 'nso', {'o': [(0, 3, 3.2, 'scellee', 0.0)], 'n': [porte(47, 2.4, 3.0)], 's': [porte(52, 4, 3.6)]},
       sol='tole', plafond=False, nom='Coursive éventrée : ouverte sur le vide', police=1.5, teinte_nom='rouge')
bloc(47.5, 0, 15, 9, E, 4, plafond=True)                       # plafond arraché après 55 m
modele('12-Exterieur/gravats-breche', 59, 0, [4, 3, 4])
modele('12-Exterieur/debris-1', 70, -1, [7, 4, 7], 1.5)
modele('04-Pirates/pirate-eva', 56, 2.5, [0, 1.9, 0])
modele('09-Informatique/cables', 48, -4.2, [3, 0, 3], 1.6)
etiquette('5 · Ou par ici, en combinaison', 52, 0, 3.0, 0.55, 'ambre')
for x in (43, 47, 51):
    neon(x, 0, 3.9, 1.6, 'x', 'rouge', plafond=True)            # l'alarme, sous ce qui reste du plafond

espace(41, 65, -28, -4.5, 10, 'neo', {'o': [trappe(-9.5)]}, sol=None, nom='Salle des machines', police=2.2)
dalles(41, 65, -28, -4.5)
eclairer(41, 65, -28, -4.5, 10, 'ambre', ecart=6, longueur=2.4, y=7.0)
modele('06-Couloirs/porte-coulissante', 41.5, -20.5)            # porte vers les soutes (décor)
mur('z', -4.5, 41, 64, 10, [(52.5, 23, 4.0, 'porte', 0.0)])   # au-dessus de la coursive éventrée
bloc(64.5, -4.5, 1, E, 10)
modele('08-Machines/reacteur', 53, -16.5, [7, 8, 7])
bloc(53.5, -26.6, 22, 2.4, 0.3, 3.7)                           # passerelle technique, à 4 m
escalier(45.0, -24.6, 16, 0.25, 0.3, 1.6)
modele('08-Machines/generateur', 45, -10, [3, 3, 3])
modele('08-Machines/generateur', 61, -10, [3, 3, 3])
modele('08-Machines/coeur-fusion', 57, -23, [2.5, 3.5, 2.5])
for x, z in ((46, -19), (59.5, -15)):
    modele('08-Machines/pylone-energie', x, z, [2.5, 4.5, 2.5])
for y in (2.0, 2.8, 3.6):
    bloc(64.7, -16, 0.4, 20, 0.4, y, 'sombre')
modele('09-Informatique/terminal', 56, -26.6, [1.2, 0, 1.4], 4.0)
neon(53.5, -25.45, 4.0, 22, 'x', 'ambre', section=(0.03, 0.05))   # le bord de la passerelle technique
etiquette('3 · Rétablir le courant de la chambre des reliques', 56, -26.6, 6.0, 0.5, 'ambre')
for x, z in ((50, -9), (56, -11), (48, -13)):
    humain(x, z, 'orange')

espace(40, 68, 4.5, 32, 12, 'seo', {'s': [porte(42, 3.0, 2.6), (57, 14, 9, 'champ', 0.0)], 'e': [porte(18, 3, 3)]}, sol=None, nom='Hangar', police=2.8)
dalles(40, 68, 4.5, 32)
eclairer(40, 68, 4.5, 32, 12, 'blanc', ecart=6, longueur=3.2, y=7.0)   # rampes suspendues
for z in (10, 17, 24):
    neon(40.15, z, 5.0, 3, 'z', 'blanc', section=(0.1, 0.12))   # projecteurs muraux
    neon(67.85, z, 5.0, 3, 'z', 'blanc', section=(0.1, 0.12))
modele('06-Couloirs/porte-coulissante', 40.5, 14)               # porte vers les ateliers de maintenance (décor)
modele('12-Exterieur/porte-sas', 42, 32)
for x in (50.05, 63.95):
    neon(x, 31.85, 0, 9, 'y', 'cyan', section=(0.08, 0.08))      # les montants du champ de force
neon(57, 31.85, 9.05, 14, 'x', 'cyan', section=(0.08, 0.08))    # le cadre du champ de force
neon(57, 31.6, 0.02, 14, 'x', 'ambre', section=(0.04, 0.3))      # la ligne d'envol
mur('z', 4.5, 40, 64, 12, [(52, 24, 4.0, 'porte', 0.0)])      # au-dessus de la coursive éventrée
bloc(66, 4.5, 4, E, 12)
espace(40, 44, 28.5, 32, 3, 'ne', {'n': [porte(42, 2.0, 2.6)]}, sol=None, nom='Sas', police=0.5)
modele('12-Exterieur/cloison-etanche', 42, 28.5, [0, 2.6, 0], rot=90)
neon(42, 30.25, 2.9, 1.6, 'x', 'ambre', plafond=True)
modele('11-Vaisseaux/navette-a-voler', 56, 20, [11, 5, 14])
etiquette('La navette à voler', 56, 20, 6.5, 0.8, 'ambre')
etiquette('6 · Le hangar : voler la navette', 52, 10, 4.5, 0.7, 'ambre')
modele('11-Vaisseaux/navette-transfert', 46.5, 13, [8, 4, 11])
for z in (25, 29):
    modele('11-Vaisseaux/capsule-sauvetage', 66.2, z, [2.4, 2.6, 3])
for x, z in ((60.5, 13), (62.8, 13)):
    modele('10-Hangar/caisses', x, z, [2, 2, 2])
modele('10-Hangar/caisse-armes', 44.5, 7, [1.8, 1.0, 1.0])
for x, z in ((66.8, 11.5), (66.8, 12.7), (65.6, 11.5)):
    modele('10-Hangar/cellule-energie', x, z, [0, 1.0, 0])
for x, z in ((41, 20), (41, 21), (41.8, 20.5)):
    modele('10-Hangar/bidon-carburant', x, z, [0, 0.8, 0])
for x, z, l, p in ((48, 23, 2.4, 1), (51, 27, 2.4, 1), (45.5, 22, 1, 2.4), (63.2, 23, 2.4, 1), (64, 26.5, 1, 2.4)):
    bloc(x, z, l, p, 1.2)                                      # abris pour le combat
bloc(64, 7, 6, 4, 3)                                           # poste de contrôle surélevé
escalier(57.4, 7, 12, 0.25, 0.3, 1.6)
bloc(64, 9, 6, 0.06, 1.0, 3, 'verre')
modele('09-Informatique/terminal', 64.5, 6.2, [1.6, 0, 1.8], 3)
etiquette('Poste de contrôle', 64, 7, 5.2, 0.5)
modele('04-Pirates/pirate-chef', 64.5, 18, [0, 1.95, 0], rot=-90)
modele('04-Pirates/pirate', 62, 16.5, rot=-90)                  # réparé : sa pièce parasite est retirée
for x, z in ((63, 20.5), (60, 18), (61.5, 21.8)):
    humain(x, z, 'orange')
for x, z, y in ((48, 24.2, 0), (51, 28.2, 0), (44.5, 23.5, 0), (63.5, 8, 3)):
    humain(x, z, 'bleu', y)
etiquette('Champ de force : sortie des navettes', 57, 31.4, 9.6, 0.6, 'ether')
neon(64, 9.0, 3.0, 6, 'x', 'ambre', section=(0.03, 0.05))       # le bord du poste de contrôle
etiquette('7 · Décoller, puis l\'hyperespace', 57, 34, 10.8, 0.8, 'ambre')

espace(68, 74, 16.5, 19.5, 3, 'ns', sol='tole', nom='Tube d\'abordage', police=0.5)
modele('11-Vaisseaux/vaisseau-pirate', 86, 18, [24, 10, 18])
etiquette('Vaisseau pirate amarré', 86, 18, 12, 1.2, 'orange')

# ------------------------------------------------------------------ écrire la maquette dans l'atelier
MAQUETTE = {
    'vaisseau-prison': {
        'nom': 'Vaisseau-prison (prologue)',
        'description': 'Le niveau du prologue en formes grises, à l\'échelle, d\'après le plan : du réveil au laboratoire jusqu\'au hangar.',
        'decor': 'espace',
        'vue': {'camera': [26, 110, 107], 'cible': [6, 0, 2]},
        'depart': {'position': [-9.0, 0, -24.0], 'lacet': -90},   # à la porte de la cellule, face au laboratoire
        'pieces': pieces,
    }
}

racine = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
chemin = os.path.join(racine, 'vitrine', 'atelier-prologue.html')
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
with open(os.path.join(racine, 'docs', 'prologue', 'maquette-vaisseau-prison.json'), 'w', encoding='utf-8') as f:
    json.dump(export, f, ensure_ascii=False, indent=0)
types = {}
for q in pieces:
    types[q['cle'] if q['type'] == 'forme' else 'modele'] = types.get(q['cle'] if q['type'] == 'forme' else 'modele', 0) + 1
print(f'{len(pieces)} pièces', types, f'{len(donnees) // 1024} Ko')
