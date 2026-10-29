#!/usr/bin/env python3
"""Échos d'Éther : recolore les modèles du prologue selon la bible, dans leurs fichiers.

L'atelier recolore les modèles à l'affichage (réglage « da » de docs/prologue/tailles-modeles.json). Ce script fait la même
chose une fois pour toutes, dans les textures des fichiers : Unity, Blender ou un autre outil les ouvrent alors déjà aux
couleurs du Colosse (acier sombre, un seul rouge pour l'Empire, pas de cyan hors des Bâtisseurs).

Le calcul est celui de l'atelier :
  - la saturation est réduite (da.saturation, 0,7 par défaut : le style unifié) ;
  - la couleur est multipliée par une teinte froide (0,90 ; 0,94 ; 1,00), elle-même multipliée par da.teinte ;
  - si da.emissif est donné, les parties lumineuses prennent cette couleur, à intensité égale (un voyant bleu devient ambre).

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


def etalonner_rgb(rgb, saturation, teinte):
    """rgb linéaire (…, 3) → recoloré, comme le shader de l'atelier."""
    lum = rgb @ np.array([0.2126, 0.7152, 0.0722])
    return (lum[..., None] + (rgb - lum[..., None]) * saturation) * np.array(teinte)


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


def recolorer(chemin, da):
    saturation = da.get('saturation', 0.7)
    teinte = [FROID[i] * (da.get('teinte') or [1, 1, 1])[i] for i in range(3)]
    emissif = da.get('emissif')
    signature = {'saturation': saturation, 'teinte': [round(v, 4) for v in teinte], 'emissif': emissif}
    origine = chemin[:-4] + '.couleurs-origine.glb'
    gltf, binaire = lire(chemin)
    fait = gltf.get('asset', {}).get('extras', {}).get('echos_couleurs')
    if fait == signature:
        return 'déjà fait'
    if fait is not None:
        if not os.path.exists(origine):
            return "déjà recoloré avec un autre réglage, mais l'original manque pour recommencer"
        gltf, binaire = lire(origine)
    elif not os.path.exists(origine):
        shutil.copy2(chemin, origine)
    # quelles images servent de couleur de base, lesquelles de lumière
    couleur, lumiere = set(), set()
    for m in gltf.get('materials', []):
        pbr = m.setdefault('pbrMetallicRoughness', {})
        if 'baseColorTexture' in pbr:
            couleur.add(gltf['textures'][pbr['baseColorTexture']['index']]['source'])
        f = pbr.get('baseColorFactor', [1, 1, 1, 1])
        pbr['baseColorFactor'] = [round(float(v), 5) for v in etalonner_rgb(np.array(f[:3]), saturation, teinte)] + [f[3]]
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
            remplacements[image['bufferView']] = traiter_image(octets, image.get('mimeType', 'image/png'),
                                                              lambda c: etalonner_rgb(c, saturation, teinte))
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
    return f'{len(remplacements)} textures recolorées (saturation {saturation}, teinte {", ".join(f"{t:.2f}" for t in teinte)})'


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
