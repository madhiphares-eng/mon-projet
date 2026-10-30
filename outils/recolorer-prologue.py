#!/usr/bin/env python3
"""Échos d'Éther : recolore les modèles du prologue selon la bible, dans leurs fichiers.

L'atelier recolore les modèles à l'affichage (réglage « da » de docs/prologue/tailles-modeles.json). Ce script fait la même
chose une fois pour toutes, dans les textures des fichiers : Unity, Blender ou un autre outil les ouvrent alors déjà aux
couleurs du Colosse (acier sombre, un seul rouge pour l'Empire, pas de cyan hors des Bâtisseurs).

Le calcul est celui de l'atelier :
  - la saturation est réduite (da.saturation, 0,7 par défaut : le style unifié) ;
  - la couleur est multipliée par une teinte froide (0,90 ; 0,94 ; 1,00), elle-même multipliée par da.teinte ;
  - si da.emissif est donné, les parties lumineuses prennent cette couleur, à intensité égale (un voyant bleu devient ambre) ;
  - si da.peinture est donné, la peinture d'un modèle (ses pixels saturés, autour de la teinte « teinte », en degrés) est
    traitée à part, au lieu de devenir de l'acier : sa teinte peut changer (« vers », en degrés), sa saturation et sa
    luminosité sont multipliées (« saturation », « luminosite »). Un bleu vif devient un bleu nuit, sans griser le reste ;
  - si da.inscriptions est donné, les triangles entièrement dans une de ses boîtes (coordonnées du maillage) sont retirés :
    une inscription en relief posée devant une coque (« police » sur un torse).

Usage :
  pip install pillow numpy
  python3 outils/recolorer-prologue.py ~/jeu/prologue
  python3 outils/recolorer-prologue.py ~/jeu/prologue --seulement 03-Gardes/androide-antiemeute

À lancer après normaliser-prologue.py (et rigger-statues.py). Les fichiers d'origine sont gardés à côté
(<nom>.couleurs-origine.glb). Relancer le script ne refait que les modèles dont le réglage a changé.
"""
import argparse
import io
import json
import os
import shutil
import struct
import sys
import urllib.request

import numpy as np
from PIL import Image

DEPOT = 'https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/docs/prologue/'
FROID = (0.90, 0.94, 1.00)
POIDS = np.array([0.2126, 0.7152, 0.0722])
# 2 : la couleur de base d'un matériau texturé n'est plus étalonnée deux fois (facteur et texture) ; peinture, inscriptions
VERSION = 2


def table(nom):
    local = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'docs', 'prologue', nom)
    if os.path.exists(local):
        return json.load(open(local, encoding='utf-8'))
    with urllib.request.urlopen(DEPOT + nom, timeout=60) as r:
        return json.loads(r.read().decode('utf-8'))


def lire(chemin):
    with open(chemin, 'rb') as f:
        donnees = f.read()
    magie, version, _ = struct.unpack_from('<4sII', donnees, 0)
    if magie != b'glTF' or version != 2:
        raise ValueError('pas un glb 2.0')
    n, _ = struct.unpack_from('<II', donnees, 12)
    gltf = json.loads(donnees[20:20 + n].decode('utf-8'))
    binaire = b''
    if len(donnees) > 20 + n:
        m, _ = struct.unpack_from('<II', donnees, 20 + n)
        binaire = donnees[28 + n:28 + n + m]
    return gltf, binaire


def ecrire(chemin, gltf, binaire):
    js = json.dumps(gltf, ensure_ascii=False, separators=(',', ':')).encode('utf-8')
    js += b' ' * (-len(js) % 4)
    b = binaire + b'\0' * (-len(binaire) % 4)
    morceaux = struct.pack('<II', len(js), 0x4E4F534A) + js + struct.pack('<II', len(b), 0x004E4942) + b
    open(chemin + '.tmp', 'wb').write(struct.pack('<4sII', b'glTF', 2, 12 + len(morceaux)) + morceaux)
    os.replace(chemin + '.tmp', chemin)


def lineaire(c):
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def srgb(c):
    c = np.clip(c, 0, 1)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * c ** (1 / 2.4) - 0.055)


def lisse(a, b, x):
    t = np.clip((x - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)


def teinte_deg(rgb):
    mx, mn = rgb.max(-1), rgb.min(-1)
    d = mx - mn + 1e-8
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    return np.where(mx == r, ((g - b) / d) % 6, np.where(mx == g, (b - r) / d + 2, (r - g) / d + 4)) * 60


def changer_teinte(rgb, h):
    """Même saturation, même valeur (TSV), autre teinte (degrés)."""
    mx, mn = rgb.max(-1, keepdims=True), rgb.min(-1, keepdims=True)
    k = (np.array([5, 3, 1]) + h / 60) % 6
    return mx - (mx - mn) * np.clip(np.minimum(k, 4 - k), 0, 1)


def desaturer(rgb, saturation):
    lum = (rgb @ POIDS)[..., None]
    return lum + (rgb - lum) * saturation


def etalonnage(da):
    """La fonction rgb linéaire (…, 3) → recoloré, comme le shader de l'atelier."""
    saturation = da.get('saturation', 0.7)
    teinte = np.array([FROID[i] * (da.get('teinte') or [1, 1, 1])[i] for i in range(3)])
    peinture = da.get('peinture')

    def f(rgb):
        sortie = desaturer(rgb, saturation) * teinte
        if peinture:
            mx = rgb.max(-1)
            s = (mx - rgb.min(-1)) / np.maximum(mx, 1e-4)
            ecart = np.abs((teinte_deg(rgb) - peinture['teinte'] + 180) % 360 - 180)
            w = lisse(0.4, 0.65, s) * (1 - lisse(30, 50, ecart)) * lisse(0.003, 0.012, mx)
            p = changer_teinte(rgb, peinture['vers']) if 'vers' in peinture else rgb
            p = desaturer(p, peinture.get('saturation', 1)) * peinture.get('luminosite', 1)
            sortie = sortie + (p - sortie) * w[..., None]
        return sortie
    return f


def traiter_image(octets, mime, fonction):
    im = Image.open(io.BytesIO(octets))
    alpha = im.getchannel('A') if im.mode in ('RGBA', 'LA') else None
    rgb = np.asarray(im.convert('RGB'), dtype=np.float32) / 255
    sortie = (srgb(fonction(lineaire(rgb))) * 255 + 0.5).astype(np.uint8)
    nouvelle = Image.fromarray(sortie, 'RGB')
    if alpha is not None:
        nouvelle.putalpha(alpha)
    tampon = io.BytesIO()
    if mime == 'image/jpeg' and alpha is None:
        nouvelle.save(tampon, 'JPEG', quality=92)
    else:
        nouvelle.save(tampon, 'PNG', optimize=True)
        mime = 'image/png'
    return tampon.getvalue(), mime


def attribut(gltf, binaire, i):
    """Un attribut de sommets en flottants serrés : (tableau, position dans le binaire), ou (None, None)."""
    a = gltf['accessors'][i]
    n = {'SCALAR': 1, 'VEC2': 2, 'VEC3': 3, 'VEC4': 4}[a['type']]
    if 'bufferView' not in a or a['componentType'] != 5126 or a.get('sparse'):
        return None, None
    v = gltf['bufferViews'][a['bufferView']]
    if v.get('byteStride', 4 * n) != 4 * n:
        return None, None
    debut = v.get('byteOffset', 0) + a.get('byteOffset', 0)
    return np.frombuffer(binaire, np.float32, a['count'] * n, debut).reshape(-1, n).copy(), debut


def retirer(gltf, binaire, boites):
    """Retire les triangles entièrement dans une des boîtes (coordonnées du maillage) : une inscription en relief posée
    devant une coque. Ils deviennent dégénérés (trois fois le même sommet) ; rien d'autre ne bouge dans le fichier."""
    binaire, retires = bytearray(binaire), 0
    for maillage in gltf.get('meshes', []):
        for p in maillage['primitives']:
            if 'indices' not in p or p.get('mode', 4) != 4 or 'POSITION' not in p['attributes']:
                continue
            pos, _ = attribut(gltf, binaire, p['attributes']['POSITION'])
            a = gltf['accessors'][p['indices']]
            if pos is None or 'bufferView' not in a:
                continue
            v = gltf['bufferViews'][a['bufferView']]
            debut = v.get('byteOffset', 0) + a.get('byteOffset', 0)
            tri = np.frombuffer(binaire, {5121: np.uint8, 5123: np.uint16, 5125: np.uint32}[a['componentType']],
                                a['count'], debut).reshape(-1, 3).copy()
            dedans = np.zeros(len(pos), bool)
            for b0, b1 in boites:
                dedans |= np.all((pos >= b0) & (pos <= b1), axis=1)
            t = dedans[tri].all(axis=1) & (tri[:, 0] != tri[:, 1])
            tri[t] = tri[t, :1]
            retires += int(t.sum())
            binaire[debut:debut + tri.nbytes] = tri.tobytes()
    return bytes(binaire), retires


def recolorer(chemin, da):
    saturation = da.get('saturation', 0.7)
    teinte = [FROID[i] * (da.get('teinte') or [1, 1, 1])[i] for i in range(3)]
    emissif = da.get('emissif')
    signature = {'version': VERSION, 'saturation': saturation, 'teinte': [round(v, 4) for v in teinte], 'emissif': emissif,
                 'peinture': da.get('peinture'), 'inscriptions': da.get('inscriptions')}
    f = etalonnage(da)
    origine = chemin[:-4] + '.couleurs-origine.glb'
    gltf, binaire = lire(chemin)
    fait = gltf.get('asset', {}).get('extras', {}).get('echos_couleurs')
    if fait == signature:
        return 'déjà fait'
    if fait is not None:
        if not os.path.exists(origine):
            return "déjà recoloré avec un autre réglage, mais l'original manque pour recommencer"
        gltf, binaire = lire(origine)
    else:
        # pas encore recoloré : c'est le nouvel original (un modèle re-riggé ou re-téléchargé), il remplace l'ancien
        shutil.copy2(chemin, origine)
    retires = 0
    if da.get('inscriptions'):
        binaire, retires = retirer(gltf, binaire, da['inscriptions'])
    # quelles images servent de couleur de base, lesquelles de lumière
    couleur, lumiere = set(), set()
    for m in gltf.get('materials', []):
        pbr = m.setdefault('pbrMetallicRoughness', {})
        if 'baseColorTexture' in pbr:
            # la texture est recolorée ; le facteur (souvent blanc) reste, sinon la teinte s'appliquerait deux fois
            couleur.add(gltf['textures'][pbr['baseColorTexture']['index']]['source'])
        else:
            c = pbr.get('baseColorFactor', [1, 1, 1, 1])
            pbr['baseColorFactor'] = [round(float(v), 5) for v in f(np.array(c[:3]))] + [c[3]]
        if emissif and 'emissiveTexture' in m:
            lumiere.add(gltf['textures'][m['emissiveTexture']['index']]['source'])
        if emissif and any(m.get('emissiveFactor', [0, 0, 0])):
            e = max(m['emissiveFactor'])
            m['emissiveFactor'] = [round(e * c, 5) for c in emissif]
    # les autres images (normales, rugosité) ne doivent pas changer : une image partagée n'est pas touchée
    partagees = set()
    for m in gltf.get('materials', []):
        pbr = m.get('pbrMetallicRoughness', {})
        for cle in ('normalTexture', 'occlusionTexture'):
            if cle in m:
                partagees.add(gltf['textures'][m[cle]['index']]['source'])
        if 'metallicRoughnessTexture' in pbr:
            partagees.add(gltf['textures'][pbr['metallicRoughnessTexture']['index']]['source'])
    vues = gltf.get('bufferViews', [])
    remplacements = {}
    for i, image in enumerate(gltf.get('images', [])):
        if 'bufferView' not in image or i in partagees:
            continue
        v = vues[image['bufferView']]
        octets = binaire[v.get('byteOffset', 0):v.get('byteOffset', 0) + v['byteLength']]
        if i in couleur:
            remplacements[image['bufferView']] = traiter_image(octets, image.get('mimeType', 'image/png'), f)
        elif i in lumiere:
            remplacements[image['bufferView']] = traiter_image(octets, image.get('mimeType', 'image/png'),
                                                              lambda c: c.max(axis=-1, keepdims=True) * np.array(emissif))
        else:
            continue
        image['mimeType'] = remplacements[image['bufferView']][1]
    # le binaire est réassemblé, vue par vue, avec les images refaites
    nouveau = bytearray()
    for j, v in enumerate(vues):
        if j in remplacements:
            octets = remplacements[j][0]
        else:
            octets = binaire[v.get('byteOffset', 0):v.get('byteOffset', 0) + v['byteLength']]
        nouveau += b'\0' * (-len(nouveau) % 4)
        v['byteOffset'] = len(nouveau)
        v['byteLength'] = len(octets)
        nouveau += octets
    if gltf.get('buffers'):
        gltf['buffers'][0]['byteLength'] = len(nouveau)
    gltf.setdefault('asset', {}).setdefault('extras', {})['echos_couleurs'] = signature
    ecrire(chemin, gltf, bytes(nouveau))
    detail = f'saturation {saturation}, teinte {", ".join(f"{t:.2f}" for t in teinte)}'
    if da.get('peinture'):
        detail += ', peinture à part'
    if retires:
        detail += f', {retires} triangles retirés'
    return f'{len(remplacements)} textures recolorées ({detail})'


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument('dossier', help='le dossier du prologue')
    p.add_argument('--seulement', default='', help='les clés à faire, séparées par des virgules')
    p.add_argument('--tous', action='store_true', help='appliquer aussi le style unifié aux modèles sans réglage « da »')
    a = p.parse_args()
    racine = os.path.expanduser(a.dossier)
    tailles = {k: v for k, v in table('tailles-modeles.json').items() if not k.startswith('_')}
    faits = 0
    for cle, reglage in sorted(tailles.items()):
        if a.seulement and cle not in a.seulement.split(','):
            continue
        if 'da' not in reglage and not a.tous:
            continue
        nom = cle.split('/')[1]
        fichier = os.path.join(racine, *cle.split('/'), nom + '.glb')
        if not os.path.exists(fichier):
            print(f'  {cle:42s} absent')
            continue
        try:
            resultat = recolorer(fichier, reglage.get('da', {}))
            faits += resultat[0].isdigit()
            print(f'  {cle:42s} {resultat}')
        except Exception as e:
            print(f'  {cle:42s} erreur : {e}')
    print(f'\n{faits} modèles recolorés. Les originaux sont gardés en .couleurs-origine.glb.')


if __name__ == '__main__':
    main()
