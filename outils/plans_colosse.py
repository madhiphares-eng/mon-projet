"""Les plans détaillés du Colosse, tirés des pièces de la maquette (appelé par outils/maquette-prologue.py).

Écrit dans docs/prologue/plans/ :
  - un plan par secteur, à grande échelle, coupé à 1,20 m du sol comme un plan d'architecte :
    ce que le plan de coupe traverse est plein, ce qui est plus bas est clair, ce qui est plus haut (galeries, passerelles) est en tirets ;
  - des coupes verticales (longitudinale et transversales), avec la limite des deux couches de la bible à 4 m.
"""
import math
import os

CARTE = {'verre': '#6fa8d8', 'scelle': '#ff4a3a', 'champ': '#e8eef6', 'vertdegris': '#7d8a83', 'rouille': '#a0603a',
         'ether': '#5fe0d8', 'sombre': '#4a4f58', 'tole': '#626872', 'acier': '#9aa3b0', 'ambre': '#f2b26b'}
FONDS = {'mono': '#1c2129', 'coursive': '#20242c', 'chantier': '#272219', 'bat': '#0f2324', 'vide': '#2a1414', 'conduit': '#2b2418', 'fac': '#191a20'}
CAMPS = {'bleu': ('#c8322a', 'A'), 'orange': ('#ff8a3a', 'P'), 'jaune': ('#d9b04a', 'D')}
NEONS = {'blanc': '#e8eef6', 'ambre': '#ffb04a', 'rouge': '#ff4a3a', 'vert': '#5fd08a', 'cyan': '#5fe0d8', 'orange': '#ff8a3a'}
SANS_NOM = {'06-Couloirs/sols', '06-Couloirs/tuyaux', '09-Informatique/cables'}   # trop nombreux pour être nommés
COUPE = 1.2          # hauteur du plan de coupe (m)
POLICE = 'font-family="system-ui, -apple-system, Segoe UI, sans-serif"'

SECTEURS = [  # (fichier, titre, x0, x1, z0, z1)
    ('01-reveil', 'Le réveil : laboratoire, bureau, infirmerie et anneau du cœur', -16, 16, -38, 12),
    ('02-nef', 'La nef des cellules, les ateliers et la consigne', -70, -8, -16, 50),
    ('03-reliques', 'La chambre des reliques, le sas et la sortie sur la coque', -16, 16, 8, 52),
    ('04-machines', 'La salle des machines et la coursive éventrée', 8, 62, -16, 16),
    ('05-hangar', 'Le hangar, ses galeries et l\'escalier de la coque', 54, 124, -46, 52),
    ('06-facultatif', 'Zones facultatives : quartiers des officiers et passerelle', -100, -8, -40, -4),
]
COUPES = [  # (fichier, titre, axe de la coupe, position, début, fin)
    ('coupe-A-longitudinale', 'Coupe A-A : tout le vaisseau, de la proue au hangar (z = 0)', 'z', 0, -100, 122),
    ('coupe-B-nef', 'Coupe B-B : la nef des cellules en travers (x = −42)', 'x', -42, -16, 16),
    ('coupe-C-hangar', 'Coupe C-C : le hangar en travers (x = 92)', 'x', 92, -24, 24),
    ('coupe-D-reliques', 'Coupe D-D : du cœur au sas, par la chambre des reliques (x = 0)', 'x', 0, -38, 50),
]


PERSONNAGES = {'02-Prisonniers': 'jaune', '03-Gardes': 'bleu', '04-Pirates': 'orange'}


def camp_de(q):
    """Le camp d'une pièce qui est un personnage (silhouette ou modèle), sinon None."""
    if q.get('cle') == 'humain':
        return q.get('teinte')
    if q.get('type') == 'modele':
        return q.get('camp') or PERSONNAGES.get(q['cle'].split('/')[0])
    return None


def _empreinte(q, tailles, mesures):
    """Taille réelle (l, h, p) d'un modèle posé dans la maquette."""
    cadre = q.get('cadre') or tailles.get(q['cle'], {}).get('cadre') or [1, 1, 1]
    t = (mesures.get(q['cle']) or {}).get('taille')
    if not t:
        t = [c or 1 for c in cadre]
    rapports = [c / v for c, v in zip(cadre, t) if c and v]
    s = min(rapports) if rapports else 1
    e = q.get('echelle') or [1, 1, 1]
    return t[0] * s * e[0], t[1] * s * e[1], t[2] * s * e[2]


def _texte(x, y, t, taille=11, couleur='#ece6d8', ancre='middle', gras=False, extra=''):
    t = str(t).replace('&', '&amp;').replace('<', '&lt;')
    return f'<text x="{x:.1f}" y="{y:.1f}" font-size="{taille}" text-anchor="{ancre}" fill="{couleur}"{" font-weight=\"700\"" if gras else ""} {extra}>{t}</text>'


def _cote(x0, y0, x1, y1, valeur, decal, vertical=False):
    """Une ligne de cote avec ses deux traits et sa valeur."""
    o = []
    if vertical:
        o.append(f'<line x1="{x0 - decal:.1f}" y1="{y0:.1f}" x2="{x0 - decal:.1f}" y2="{y1:.1f}" stroke="#8b8798" stroke-width="1"/>')
        for y in (y0, y1):
            o.append(f'<line x1="{x0 - decal - 5:.1f}" y1="{y:.1f}" x2="{x0 - decal + 5:.1f}" y2="{y:.1f}" stroke="#8b8798"/>')
        my = (y0 + y1) / 2
        o.append(_texte(x0 - decal - 4, my, f'{valeur:g} m', 10, '#b9b5c6', extra=f'transform="rotate(-90 {x0 - decal - 4:.1f} {my:.1f})"'))
    else:
        o.append(f'<line x1="{x0:.1f}" y1="{y0 - decal:.1f}" x2="{x1:.1f}" y2="{y0 - decal:.1f}" stroke="#8b8798" stroke-width="1"/>')
        for x in (x0, x1):
            o.append(f'<line x1="{x:.1f}" y1="{y0 - decal - 5:.1f}" x2="{x:.1f}" y2="{y0 - decal + 5:.1f}" stroke="#8b8798"/>')
        o.append(_texte((x0 + x1) / 2, y0 - decal - 4, f'{valeur:g} m', 10, '#b9b5c6'))
    return ''.join(o)


def _echelle(x, y, k, longueur=10):
    pas = longueur / 2
    return (f'<g transform="translate({x:.0f},{y:.0f})"><rect width="{pas * k:.1f}" height="6" fill="#ece6d8"/><rect x="{pas * k:.1f}" width="{pas * k:.1f}" height="6" fill="#5d5a6a"/>'
            + _texte(0, -4, '0', 10, '#a7a3b4', 'start') + _texte(pas * k, -4, f'{pas:g}', 10, '#a7a3b4') + _texte(longueur * k, -4, f'{longueur:g} m', 10, '#a7a3b4')
            + '</g>')


def plan_secteur(titre, X0, X1, Z0, Z1, pieces, ZONES, CHEMINS, VUES, tailles, mesures):
    K = min(26.0, 1500 / (X1 - X0), 1500 / (Z1 - Z0))   # pixels par mètre
    M, HAUT = 70, 92
    W, H = (X1 - X0) * K + 2 * M, (Z1 - Z0) * K + 2 * M + HAUT + 40
    px = lambda x: M + (x - X0) * K
    pz = lambda z: HAUT + M + (z - Z0) * K
    dedans = lambda x, z, marge=0: X0 - marge <= x <= X1 + marge and Z0 - marge <= z <= Z1 + marge
    o = [f'<rect width="{W:.0f}" height="{H:.0f}" fill="#0e1016"/>',
         _texte(M, 36, f'Le Colosse · {titre}', 22, '#f2b26b', 'start', True),
         _texte(M, 58, f'Plan coupé à {COUPE:g} m du sol. Proue à gauche (ouest). Une petite case = 1 m, une grande = 5 m.', 12, '#a7a3b4', 'start'),
         _texte(M, 76, 'Plein : coupé par le plan · clair : plus bas · tirets : au-dessus (galeries, passerelles).', 12, '#a7a3b4', 'start'),
         f'<clipPath id="cadre"><rect x="{px(X0):.0f}" y="{pz(Z0):.0f}" width="{(X1 - X0) * K:.0f}" height="{(Z1 - Z0) * K:.0f}"/></clipPath>']
    g = []
    # la grille
    for x in range(math.ceil(X0), math.floor(X1) + 1):
        g.append(f'<line x1="{px(x):.1f}" x2="{px(x):.1f}" y1="{pz(Z0):.0f}" y2="{pz(Z1):.0f}" stroke="{"#252a33" if x % 5 == 0 else "#14171d"}"/>')
    for z in range(math.ceil(Z0), math.floor(Z1) + 1):
        g.append(f'<line x1="{px(X0):.0f}" x2="{px(X1):.0f}" y1="{pz(z):.1f}" y2="{pz(z):.1f}" stroke="{"#252a33" if z % 5 == 0 else "#14171d"}"/>')
    # les sols des salles
    for (x0, x1, z0, z1, h, nom, couche) in ZONES:
        g.append(f'<rect x="{px(x0):.1f}" y="{pz(z0):.1f}" width="{(x1 - x0) * K:.1f}" height="{(z1 - z0) * K:.1f}" fill="{FONDS[couche]}"'
                 + (' stroke="#6b6878" stroke-dasharray="5 4"' if couche == 'fac' else '') + ' opacity="0.9"/>')
    # les blocs : plus bas que la coupe, puis au-dessus (tirets), puis coupés (plein)
    bas, haut, coupes = [], [], []
    for q in pieces:
        if q['cle'] != 'bloc' or q.get('teinte') == 'invisible':
            continue
        (x, y, z), (l, h, p) = q['position'], q['dims']
        if not (dedans(x - l / 2, z - p / 2, 200) and (x + l / 2 >= X0 and x - l / 2 <= X1 and z + p / 2 >= Z0 and z - p / 2 <= Z1)):
            continue
        if l > 60 and p > 60:                                         # la coque entière
            continue
        if y > COUPE:
            if h <= 0.45 and l * p >= 2 and y < 26 and not q.get('plafond'):
                haut.append(q)
        elif y + h >= COUPE:
            coupes.append(q)
        elif y + h > 0.05:
            bas.append(q)
    rect = lambda q, extra: (lambda x, z, l, p: f'<rect x="{px(x - l / 2):.1f}" y="{pz(z - p / 2):.1f}" width="{max(l * K, 1.5):.1f}" height="{max(p * K, 1.5):.1f}" {extra}/>')(
        q['position'][0], q['position'][2], q['dims'][0], q['dims'][2])
    for q in bas:
        g.append(rect(q, f'fill="{CARTE.get(q.get("teinte", "acier"), "#9aa3b0")}" opacity="0.35"'))
    niveaux = {}
    for q in haut:
        g.append(rect(q, 'fill="none" stroke="#7b8494" stroke-width="1" stroke-dasharray="6 4"'))
        cle = round(q['position'][1], 1)
        niveaux.setdefault(cle, []).append(q)
    for q in coupes:
        t = q.get('teinte', 'acier')
        extra = f'fill="{CARTE.get(t, "#9aa3b0")}"'
        if t == 'champ':
            extra = 'fill="#e8eef6" fill-opacity="0.25" stroke="#e8eef6" stroke-dasharray="3 2"'
        elif t == 'verre':
            extra = 'fill="#6fa8d8" fill-opacity="0.55"'
        g.append(rect(q, extra))
    # les niveaux au-dessus : une étiquette par niveau et par grand plateau
    for niv, qs in sorted(niveaux.items()):
        grand = max(qs, key=lambda q: q['dims'][0] * q['dims'][2])
        x, _, z = grand['position']
        if dedans(x, z):
            g.append(_texte(px(x), pz(z) + 4, f'+{niv:g} m', 10, '#9aa6b5', extra='font-style="italic"'))
    # les néons-projecteurs et les lignes de lumière en couleur, très fins
    for q in pieces:
        if q['cle'] != 'neon':
            continue
        (x, y, z), (l, h, p) = q['position'], q['dims']
        if dedans(x, z) and (l * p) > 0.3:
            g.append(rect(q, f'fill="{NEONS.get(q.get("teinte", "blanc"), "#e8eef6")}" opacity="0.7"'))
    # les modèles : leur empreinte au sol et leur nom
    for q in pieces:
        if q['type'] != 'modele' or camp_de(q):
            continue
        x, y, z = q['position']
        if not dedans(x, z, 2) or y > 12:
            continue
        l, h, p = _empreinte(q, tailles, mesures)
        a = math.radians((q.get('rotation') or [0, 0, 0])[1])
        c, s = math.cos(a), math.sin(a)
        coins = [(x + u * c + v * s, z - u * s + v * c) for u, v in ((-l / 2, -p / 2), (l / 2, -p / 2), (l / 2, p / 2), (-l / 2, p / 2))]
        style = 'stroke="#d9cfa8" stroke-width="1.1" fill="#d9cfa8" fill-opacity="0.12"' if y < COUPE else 'stroke="#d9cfa8" stroke-width="1" stroke-dasharray="3 3" fill="none"'
        g.append('<polygon points="' + ' '.join(f'{px(u):.1f},{pz(v):.1f}' for u, v in coins) + f'" {style}/>')
        if max(l, p) * K > 18 and q['cle'] not in SANS_NOM:
            g.append(_texte(px(x), pz(z) + 3, q['cle'].split('/')[1], 9 if max(l, p) * K < 80 else 11, '#e9e1c0'))
    # les chemins
    styles = {'principal': 'stroke="#f2b26b" stroke-width="3" opacity="0.85"', 'autre': 'stroke="#a7a3b4" stroke-width="2" stroke-dasharray="7 5"',
              'raccourci': 'stroke="#ffb35c" stroke-width="2" stroke-dasharray="2 4"', 'facultatif': 'stroke="#8a8798" stroke-width="1.6" stroke-dasharray="3 4"'}
    for genre, pts in CHEMINS:
        d = 'M' + ' L'.join(f'{px(x):.1f},{pz(z):.1f}' for x, z in pts)
        g.append(f'<path d="{d}" fill="none" {styles[genre]} stroke-linejoin="round"/>')
    # les personnages
    for q in pieces:
        camp = camp_de(q)
        if not camp:
            continue
        x, y, z = q['position']
        if not dedans(x, z):
            continue
        couleur, lettre = CAMPS.get(camp, ('#888', '?'))
        g.append(f'<circle cx="{px(x):.1f}" cy="{pz(z):.1f}" r="{max(6, 0.35 * K):.1f}" fill="{couleur}" stroke="#0e1016" stroke-width="1.5"/>'
                 + _texte(px(x), pz(z) + 4, lettre, 10, '#0e1016', gras=True) + (_texte(px(x) + 10, pz(z) - 8, f'+{y:g}', 9, couleur, 'start') if y > 0.5 else ''))
    # les points de vue
    for num, nom, (x, z), (tx, tz) in VUES:
        if not dedans(x, z):
            continue
        a = math.atan2(tz - z, tx - x)
        pts = [(px(x), pz(z))] + [(px(x) + 60 * math.cos(a + d), pz(z) + 60 * math.sin(a + d)) for d in (-0.4, 0.4)]
        g.append('<polygon points="' + ' '.join(f'{u:.1f},{v:.1f}' for u, v in pts) + '" fill="#e8eef6" opacity="0.2"/>')
        g.append(f'<circle cx="{px(x):.1f}" cy="{pz(z):.1f}" r="10" fill="#e8eef6"/>' + _texte(px(x), pz(z) + 4, num, 12, '#0e1016', gras=True)
                 + _texte(px(x) + 14, pz(z) + 22, nom, 11, '#e8eef6', 'start'))
    # les noms et les cotes des salles
    for (x0, x1, z0, z1, h, nom, couche) in ZONES:
        if not nom or not (X0 <= (x0 + x1) / 2 <= X1 and Z0 <= (z0 + z1) / 2 <= Z1):
            continue
        cx, cz = (x0 + x1) / 2, (z0 + z1) / 2
        court = nom.split(' : ')[0]
        if min(x1 - x0, z1 - z0) <= 4.5:
            g.append(_texte(px(cx), pz(cz) + 4, court, 11, '#ece6d8', gras=True))
            continue
        g.append(_texte(px(cx), pz(z0) + 22, court, 15, '#ece6d8', gras=True))
        if h:
            g.append(_texte(px(cx), pz(z0) + 38, f'{(x1 - x0):g} × {(z1 - z0):g} m · {h:g} m sous plafond', 11, '#a7a3b4'))
        if x1 - x0 >= 6:
            g.append(_cote(px(x0), pz(z0), px(x1), pz(z0), x1 - x0, 10))
        if z1 - z0 >= 6:
            g.append(_cote(px(x0), pz(z0), px(x0), pz(z1), z1 - z0, 10, vertical=True))
    o.append('<g clip-path="url(#cadre)">' + ''.join(g) + '</g>')
    o.append(f'<rect x="{px(X0):.0f}" y="{pz(Z0):.0f}" width="{(X1 - X0) * K:.0f}" height="{(Z1 - Z0) * K:.0f}" fill="none" stroke="#3a3f4a"/>')
    for x in range(math.ceil(X0 / 5) * 5, math.floor(X1) + 1, 5):
        o.append(_texte(px(x), pz(Z0) - 6, x, 10, '#5d5a6a'))
    for z in range(math.ceil(Z0 / 5) * 5, math.floor(Z1) + 1, 5):
        o.append(_texte(px(X0) - 6, pz(z) + 3, z, 10, '#5d5a6a', 'end'))
    # la légende
    ly = pz(Z1) + 34
    leg = [('<rect x="0" y="-6" width="30" height="12" fill="#9aa3b0"/>', 'Mur coupé (acier)'),
           ('<rect x="0" y="-6" width="30" height="12" fill="#7d8a83"/>', 'Tôle vert-de-gris (Chantier)'),
           ('<rect x="0" y="-6" width="30" height="12" fill="none" stroke="#7b8494" stroke-dasharray="6 4"/>', 'Galerie, passerelle (au-dessus)'),
           ('<rect x="0" y="-6" width="30" height="12" fill="#d9cfa8" fill-opacity="0.12" stroke="#d9cfa8"/>', 'Modèle 3D (empreinte)'),
           ('<rect x="0" y="-6" width="30" height="12" fill="#ff4a3a"/>', 'Porte scellée'),
           ('<circle cx="8" cy="0" r="7" fill="#c8322a"/><circle cx="24" cy="0" r="7" fill="#d9b04a"/><circle cx="40" cy="0" r="7" fill="#ff8a3a"/>', 'Androïde, détenu, pirate'),
           ('<line x1="0" y1="0" x2="30" y2="0" stroke="#f2b26b" stroke-width="3"/>', 'Chemin principal')]
    colonnes = max(1, int((W - 2 * M) // 215))
    for i, (s, t) in enumerate(leg):
        o.append(f'<g transform="translate({M + (i % colonnes) * 215},{ly + (i // colonnes) * 22:.0f})">{s}' + _texte(52, 4, t, 12, '#a7a3b4', 'start') + '</g>')
    o.append(_echelle(W - M - 10 * K, ly + 50 + 22 * ((len(leg) - 1) // colonnes), K))
    H = ly + 80 + 22 * ((len(leg) - 1) // colonnes)
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W:.0f} {H:.0f}" width="{W:.0f}" height="{H:.0f}" {POLICE}>{"".join(o)}</svg>'


def coupe(titre, axe, pos, a0, a1, pieces, tailles, mesures):
    """Coupe verticale : axe 'z' = coupe le long de X à z = pos ; axe 'x' = coupe le long de Z à x = pos."""
    Y0, Y1 = -2.5, 33
    K = min(22.0, 1900 / (a1 - a0))
    M, HAUT = 70, 92
    W, H = (a1 - a0) * K + 2 * M, (Y1 - Y0) * K + 2 * M + HAUT + 30
    pa = lambda a: M + (a - a0) * K
    py = lambda y: HAUT + M + (Y1 - y) * K
    long_ = 0 if axe == 'z' else 2       # l'axe horizontal du dessin dans la position
    trav = 2 if axe == 'z' else 0        # l'axe coupé
    o = [f'<rect width="{W:.0f}" height="{H:.0f}" fill="#0e1016"/>',
         _texte(M, 36, f'Le Colosse · {titre}', 22, '#f2b26b', 'start', True),
         _texte(M, 58, ('Vue de côté, proue à gauche. ' if axe == 'z' else 'Vue depuis la proue, bâbord à gauche. ') + 'Une petite case = 1 m.', 12, '#a7a3b4', 'start'),
         _texte(M, 76, 'Sous la ligne des 4 m : le Chantier (tôle, tuyaux, sodium chaud) ; au-dessus : le Monolithe (acier, nervures, lignes froides).', 12, '#a7a3b4', 'start'),
         f'<clipPath id="cadre"><rect x="{pa(a0):.0f}" y="{py(Y1):.0f}" width="{(a1 - a0) * K:.0f}" height="{(Y1 - Y0) * K:.0f}"/></clipPath>']
    g = []
    for a in range(math.ceil(a0), math.floor(a1) + 1):
        g.append(f'<line x1="{pa(a):.1f}" x2="{pa(a):.1f}" y1="{py(Y1):.0f}" y2="{py(Y0):.0f}" stroke="{"#252a33" if a % 5 == 0 else "#14171d"}"/>')
    for y in range(math.ceil(Y0), math.floor(Y1) + 1):
        g.append(f'<line x1="{pa(a0):.0f}" x2="{pa(a1):.0f}" y1="{py(y):.1f}" y2="{py(y):.1f}" stroke="{"#252a33" if y % 5 == 0 else "#14171d"}"/>')
    # la bande du Chantier
    g.append(f'<rect x="{pa(a0):.0f}" y="{py(4):.1f}" width="{(a1 - a0) * K:.0f}" height="{4 * K:.1f}" fill="#2b251c" opacity="0.35"/>')
    for q in pieces:
        if q['cle'] != 'bloc' or q.get('teinte') == 'invisible':
            continue
        p, d = q['position'], q['dims']
        if not (p[trav] - d[trav] / 2 <= pos <= p[trav] + d[trav] / 2):
            continue
        if d[0] > 60 and d[2] > 60:
            continue
        l = d[long_]
        g.append(f'<rect x="{pa(p[long_] - l / 2):.1f}" y="{py(p[1] + d[1]):.1f}" width="{max(l * K, 1.5):.1f}" height="{max(d[1] * K, 1.5):.1f}" '
                 f'fill="{CARTE.get(q.get("teinte", "acier"), "#9aa3b0")}"{" fill-opacity=\"0.5\"" if q.get("teinte") in ("verre", "champ") else ""}/>')
    for q in pieces:
        p = q['position']
        if q['cle'] == 'neon' and abs(p[trav] - pos) < 2.5:
            d = q['dims']
            g.append(f'<rect x="{pa(p[long_] - d[long_] / 2):.1f}" y="{py(p[1] + d[1]):.1f}" width="{max(d[long_] * K, 2.5):.1f}" height="{max(d[1] * K, 2.5):.1f}" '
                     f'fill="{NEONS.get(q.get("teinte", "blanc"), "#e8eef6")}"/>')
        elif camp_de(q) and abs(p[trav] - pos) < 3:
            couleur = CAMPS.get(camp_de(q), ('#888', '?'))[0]
            h = _empreinte(q, tailles, mesures)[1] if q['type'] == 'modele' else 1.8
            x, y = pa(p[long_]), py(p[1])
            g.append(f'<rect x="{x - 0.11 * h * K:.1f}" y="{y - 0.8 * h * K:.1f}" width="{0.22 * h * K:.1f}" height="{0.8 * h * K:.1f}" rx="{0.08 * h * K:.1f}" fill="{couleur}"/>'
                     f'<circle cx="{x:.1f}" cy="{y - 0.9 * h * K:.1f}" r="{0.08 * h * K:.1f}" fill="{couleur}"/>')
        elif q['type'] == 'modele':
            l, h, pr = _empreinte(q, tailles, mesures)
            ep = pr if axe == 'z' else l
            if abs(p[trav] - pos) > ep / 2 + 0.3:
                continue
            larg = l if axe == 'z' else pr
            g.append(f'<rect x="{pa(p[long_] - larg / 2):.1f}" y="{py(p[1] + h):.1f}" width="{larg * K:.1f}" height="{h * K:.1f}" fill="#d9cfa8" fill-opacity="0.12" stroke="#d9cfa8"/>')
            if larg * K > 26 and q['cle'] not in SANS_NOM:
                g.append(_texte(pa(p[long_]), py(p[1] + h) - 4, q['cle'].split('/')[1], 9, '#e9e1c0'))
    g.append(f'<line x1="{pa(a0):.0f}" x2="{pa(a1):.0f}" y1="{py(4):.1f}" y2="{py(4):.1f}" stroke="#f2b26b" stroke-width="1.2" stroke-dasharray="8 5"/>')
    g.append(_texte(pa(a0) + 6, py(4) - 5, '4 m : Monolithe au-dessus', 11, '#e8eef6', 'start'))
    g.append(_texte(pa(a0) + 6, py(4) + 14, 'Chantier en dessous', 11, '#f2b26b', 'start'))
    o.append('<g clip-path="url(#cadre)">' + ''.join(g) + '</g>')
    o.append(f'<rect x="{pa(a0):.0f}" y="{py(Y1):.0f}" width="{(a1 - a0) * K:.0f}" height="{(Y1 - Y0) * K:.0f}" fill="none" stroke="#3a3f4a"/>')
    for a in range(math.ceil(a0 / 5) * 5, math.floor(a1) + 1, 5 if a1 - a0 < 80 else 10):
        o.append(_texte(pa(a), py(Y0) + 14, a, 10, '#5d5a6a'))
    for y in range(0, math.floor(Y1) + 1, 5):
        o.append(_texte(pa(a0) - 6, py(y) + 3, f'{y} m', 10, '#5d5a6a', 'end'))
    o.append(_echelle(W - M - 10 * K, py(Y0) + 44, K))
    o.append(_texte(M, py(Y0) + 50, 'Silhouettes à leur taille : androïdes en rouge, détenus en jaune, pirates en orange.', 11, '#a7a3b4', 'start'))
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W:.0f} {H:.0f}" width="{W:.0f}" height="{H:.0f}" {POLICE}>{"".join(o)}</svg>'


def tout(racine, pieces, ZONES, CHEMINS, VUES, tailles, mesures):
    dossier = os.path.join(racine, 'docs', 'prologue', 'plans')
    os.makedirs(dossier, exist_ok=True)
    faits = []
    for fichier, titre, x0, x1, z0, z1 in SECTEURS:
        open(os.path.join(dossier, f'plan-{fichier}.svg'), 'w', encoding='utf-8').write(
            plan_secteur(titre, x0, x1, z0, z1, pieces, ZONES, CHEMINS, VUES, tailles, mesures))
        faits.append(f'plan-{fichier}.svg')
    for fichier, titre, axe, pos, a0, a1 in COUPES:
        open(os.path.join(dossier, f'{fichier}.svg'), 'w', encoding='utf-8').write(coupe(titre, axe, pos, a0, a1, pieces, tailles, mesures))
        faits.append(f'{fichier}.svg')
    return faits
