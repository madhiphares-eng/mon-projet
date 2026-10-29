#!/usr/bin/env python3
"""Échos d'Éther : remet chaque modèle du prologue à sa taille réelle, dans son fichier.

Les modèles de Sketchfab sont dans toutes les unités (centimètres, pouces…) : un lit mesure 17, une navette 266.
Ce script réécrit chaque fichier en mètres, les pieds au sol et centré, d'après docs/prologue/tailles-modeles.json.
Il retire aussi les pièces parasites (le portail géant du pirate, le socle du chef pirate…).
Ensuite, Unity, Blender ou l'atelier les ouvrent directement à la bonne taille.

Usage :
  python3 normaliser-prologue.py ~/jeu/prologue            (Mac ou Linux)
  py normaliser-prologue.py C:\\Projets\\EchosEther\\Assets\\_Projet\\Externes\\Prologue   (Windows)

Les fichiers d'origine sont gardés à côté (<nom>.glb.original). Relancer le script ne change rien aux fichiers déjà faits,
sauf si leur réglage ou leur mesure a changé dans les tables : il repart alors de l'original.
Il lit tailles-modeles.json et mesures-modeles.json dans docs/prologue/ s'il est lancé depuis le dépôt, sinon il les télécharge.
Python 3 seulement, sans rien installer.
"""
import json
import os
import shutil
import struct
import sys
import urllib.request

DEPOT = 'https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/docs/prologue/'
VERSION = 2


def table(nom):
    local = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'docs', 'prologue', nom)
    if os.path.exists(local):
        return json.load(open(local, encoding='utf-8'))
    with urllib.request.urlopen(DEPOT + nom, timeout=60) as r:
        return json.loads(r.read().decode('utf-8'))


def lire(chemin):
    """(json, binaire) d'un .glb, ou (json, None) d'un .gltf."""
    if chemin.endswith('.gltf'):
        return json.load(open(chemin, encoding='utf-8')), None
    with open(chemin, 'rb') as f:
        donnees = f.read()
    magie, version, _ = struct.unpack_from('<4sII', donnees, 0)
    if magie != b'glTF' or version != 2:
        raise ValueError('pas un glb 2.0')
    n, genre = struct.unpack_from('<II', donnees, 12)
    gltf = json.loads(donnees[20:20 + n].decode('utf-8'))
    binaire = None
    if len(donnees) > 20 + n:
        m, _ = struct.unpack_from('<II', donnees, 20 + n)
        binaire = donnees[28 + n:28 + n + m]
    return gltf, binaire


def ecrire(chemin, gltf, binaire):
    texte = json.dumps(gltf, ensure_ascii=False, separators=(',', ':'))
    if chemin.endswith('.gltf'):
        open(chemin + '.tmp', 'w', encoding='utf-8').write(texte)
    else:
        js = texte.encode('utf-8')
        js += b' ' * (-len(js) % 4)
        morceaux = struct.pack('<II', len(js), 0x4E4F534A) + js
        if binaire is not None:
            b = binaire + b'\0' * (-len(binaire) % 4)
            morceaux += struct.pack('<II', len(b), 0x004E4942) + b
        open(chemin + '.tmp', 'wb').write(struct.pack('<4sII', b'glTF', 2, 12 + len(morceaux)) + morceaux)
    os.replace(chemin + '.tmp', chemin)


def facteur(cadre, taille):
    r = [cadre[i] / taille[i] for i in range(3) if cadre[i] > 0 and taille[i] > 1e-9]
    return min(r) if r else 1.0


def normaliser(chemin, reglage, mesure):
    signature = {'cadre': reglage['cadre'], 'retirer': reglage.get('retirer', []), 'mesure': mesure['taille']}
    source = chemin
    gltf, binaire = lire(chemin)
    fait = gltf.get('asset', {}).get('extras', {}).get('echos_normalise')
    if fait:
        if all(fait.get(k) == v for k, v in signature.items()):
            return 'déjà fait'
        if not os.path.exists(chemin + '.original'):
            return "déjà fait avec un ancien réglage, mais l'original (.original) manque pour recommencer"
        source = chemin + '.original'           # le réglage a changé : on repart de l'original
        gltf, binaire = lire(source)
    if mesure.get('octets') and os.path.getsize(source) != mesure['octets']:
        return f"ignoré : le fichier n'est pas celui qui a été mesuré ({os.path.getsize(source)} octets au lieu de {mesure['octets']})"
    extras = gltf.setdefault('asset', {}).setdefault('extras', {})
    noeuds = gltf.get('nodes', [])
    # retirer les pièces parasites : on les détache de leur parent (leurs données restent, inutilisées)
    a_retirer = {i for i, n in enumerate(noeuds) if n.get('name') in reglage.get('retirer', [])}
    for n in noeuds:
        if 'children' in n:
            n['children'] = [c for c in n['children'] if c not in a_retirer]
    scene = gltf['scenes'][gltf.get('scene', 0)]
    scene['nodes'] = [c for c in scene.get('nodes', []) if c not in a_retirer]
    # un nœud racine qui met à l'échelle et pose le bas de la boîte au sol, au centre
    f = facteur(reglage['cadre'], mesure['taille'])
    mn, mx = mesure['min'], mesure['max']
    centre = [(mn[0] + mx[0]) / 2, mn[1], (mn[2] + mx[2]) / 2]
    noeuds.append({'name': 'echos_taille_reelle', 'scale': [f, f, f], 'translation': [-f * v for v in centre], 'children': scene['nodes']})
    gltf['nodes'] = noeuds
    scene['nodes'] = [len(noeuds) - 1]
    extras['echos_normalise'] = {'version': VERSION, 'facteur': f, 'taille_m': [round(v * f, 3) for v in mesure['taille']], **signature}
    if not os.path.exists(chemin + '.original'):
        shutil.copy2(chemin, chemin + '.original')
    ecrire(chemin, gltf, binaire)
    t = extras['echos_normalise']['taille_m']
    return f'{t[0]:.2f} × {t[1]:.2f} × {t[2]:.2f} m'


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    racine = os.path.expanduser(sys.argv[1])
    tailles = {k: v for k, v in table('tailles-modeles.json').items() if not k.startswith('_')}
    mesures = {k: v for k, v in table('mesures-modeles.json').items() if not k.startswith('_')}
    faits = 0
    for cle, reglage in sorted(tailles.items()):
        dossier = os.path.join(racine, *cle.split('/'))
        nom = cle.split('/')[1]
        fichier = next((os.path.join(dossier, f) for f in (f'{nom}.glb', 'scene.gltf', 'scene.glb') if os.path.exists(os.path.join(dossier, f))), None)
        if not fichier:
            print(f'  {cle:42s} absent')
            continue
        if cle not in mesures:
            print(f'  {cle:42s} pas de mesure, laissé tel quel')
            continue
        try:
            resultat = normaliser(fichier, reglage, mesures[cle])
            faits += resultat[0].isdigit()
            print(f'  {cle:42s} {resultat}')
        except Exception as e:
            print(f'  {cle:42s} erreur : {e}')
    print(f'\n{faits} modèles remis à leur taille réelle. Les originaux sont gardés en .original.')


if __name__ == '__main__':
    main()
