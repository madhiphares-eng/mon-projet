#!/usr/bin/env python3
"""Échos d'Éther : fabrique avec l'IA les modèles du prologue décrits dans docs/prologue/ia/fiches.json.

Pour chaque fiche :
  1. l'image de concept, avec FLUX.1 [schnell] (licence Apache 2.0) ;
  2. la forme 3D, puis sa texture, avec TripoSG (licence MIT) ;
  3. la mise à la taille réelle : mètres, pieds au sol, centré (comme outils/normaliser-prologue.py) ;
  4. le rangement : prologue-ia/<dossier>/<nom>/<nom>.glb, son LICENCE.txt, l'image de concept,
     une ligne dans docs/REGISTRE-IA.md et dans docs/prologue/ia/faits.json.
À la fin, il ajoute les modèles faits au catalogue de l'atelier et à docs/prologue/tailles-modeles.json,
puis relance outils/maquette-prologue.py, qui les met en place dans la maquette.

Les calculs se font sur les espaces Hugging Face officiels de ces modèles, avec un GPU prêté (ZeroGPU).
Il faut un jeton Hugging Face gratuit (droit de lecture), dans la variable d'environnement HF_TOKEN.
Le quota gratuit est de quelques minutes de GPU par jour : environ 2 à 4 modèles par jour.
Quand il est épuisé, le script s'arrête proprement ; relancé le lendemain, il reprend où il en était.

Usage :
  pip install gradio_client trimesh numpy pillow
  HF_TOKEN=... python3 outils/generer-modeles-ia.py                     # tous les modèles pas encore faits
  HF_TOKEN=... python3 outils/generer-modeles-ia.py --seulement 05-Cellules/lit-acier
  HF_TOKEN=... python3 outils/generer-modeles-ia.py --sortie ~/jeu/prologue    # en copie aussi dans le dossier de l'atelier
Options : --graine N (par défaut 7), --sans-texture, --refaire (refait même ce qui existe), --essai (ne fait que lire les fiches).
"""
import argparse
import datetime
import importlib.util
import json
import os
import re
import shutil
import subprocess
import sys

RACINE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FICHES = os.path.join(RACINE, 'docs', 'prologue', 'ia', 'fiches.json')
FAITS = os.path.join(RACINE, 'docs', 'prologue', 'ia', 'faits.json')
CONCEPTS = os.path.join(RACINE, 'docs', 'prologue', 'ia', 'concepts')
MODELES_IA = os.path.join(RACINE, 'prologue-ia')
REGISTRE = os.path.join(RACINE, 'docs', 'REGISTRE-IA.md')
TAILLES = os.path.join(RACINE, 'docs', 'prologue', 'tailles-modeles.json')
ATELIER = os.path.join(RACINE, 'vitrine', 'atelier-prologue.html')
ESPACE_IMAGE = 'black-forest-labs/FLUX.1-schnell'
ESPACE_3D = 'VAST-AI/TripoSG'
LICENCE = """{titre} : modèle généré pour Échos d'Éther.

Image de concept : FLUX.1 [schnell] (Black Forest Labs), licence Apache 2.0.
Forme et texture : TripoSG (VAST AI Research), licence MIT.
Le résultat appartient au projet Échos d'Éther ; usage commercial permis.
Graine {graine}, généré le {date}. Texte de départ : voir docs/prologue/ia/fiches.json.
"""


class QuotaEpuise(Exception):
    pass


def charger_normaliseur():
    spec = importlib.util.spec_from_file_location('normaliser', os.path.join(RACINE, 'outils', 'normaliser-prologue.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def appel(client, *args, **kwargs):
    """Un appel à un espace Hugging Face ; le quota épuisé devient une erreur à part, pour s'arrêter proprement."""
    try:
        return client.predict(*args, **kwargs)
    except Exception as e:
        if 'quota' in str(e).lower():
            raise QuotaEpuise(str(e)) from e
        raise


def chemin_de(resultat):
    if isinstance(resultat, dict):
        return resultat.get('path') or resultat.get('value')
    return resultat


def a_la_taille(glb, cadre, titre):
    """Réécrit le .glb pour qu'il mesure son cadre en mètres, pieds au sol et centré, comme le normaliseur du prologue."""
    import trimesh
    normaliseur = charger_normaliseur()
    scene = trimesh.load(glb, force='scene')
    mn, mx = scene.bounds
    taille = [float(mx[i] - mn[i]) for i in range(3)]
    mesure = {'taille': taille, 'min': [float(v) for v in mn], 'max': [float(v) for v in mx]}
    gltf, binaire = normaliseur.lire(glb)
    noeuds = gltf.setdefault('nodes', [])
    scene_gltf = gltf['scenes'][gltf.get('scene', 0)]
    f = normaliseur.facteur(cadre, taille)
    centre = [(mn[0] + mx[0]) / 2, mn[1], (mn[2] + mx[2]) / 2]
    noeuds.append({'name': 'echos_taille_reelle', 'scale': [f, f, f], 'translation': [-f * float(v) for v in centre], 'children': scene_gltf['nodes']})
    scene_gltf['nodes'] = [len(noeuds) - 1]
    gltf.setdefault('asset', {}).setdefault('extras', {})['echos_normalise'] = {
        'version': normaliseur.VERSION, 'facteur': f, 'taille_m': [round(v * f, 3) for v in taille], 'cadre': cadre, 'retirer': [], 'mesure': taille}
    normaliseur.ecrire(glb, gltf, binaire)
    return [round(v * f, 2) for v in taille]


def faire(fiche, graine, texture, client_image, client_3d):
    from gradio_client import handle_file
    from PIL import Image
    dossier, nom = fiche['cle'].split('/')
    os.makedirs(CONCEPTS, exist_ok=True)
    concept = os.path.join(CONCEPTS, nom + '.png')
    if not os.path.exists(concept):
        print('    image de concept…')
        img, _ = appel(client_image, prompt=fiche['prompt'], seed=graine, randomize_seed=False, width=1024, height=1024,
                       num_inference_steps=4, api_name='/infer')
        Image.open(chemin_de(img)).convert('RGB').save(concept)
    print('    détourage…')
    detoure = chemin_de(appel(client_3d, image=handle_file(concept), api_name='/run_segmentation'))
    print('    forme 3D…')
    forme = chemin_de(appel(client_3d, image=handle_file(detoure), seed=graine, num_inference_steps=50, guidance_scale=7.0,
                            simplify=True, target_face_num=fiche['faces'], api_name='/image_to_3d'))
    if texture:
        print('    texture…')
        forme = chemin_de(appel(client_3d, image=handle_file(detoure), mesh_path=handle_file(forme), seed=graine, api_name='/run_texture'))
    cible = os.path.join(MODELES_IA, dossier, nom)
    os.makedirs(cible, exist_ok=True)
    glb = os.path.join(cible, nom + '.glb')
    shutil.copy(forme, glb)
    taille = a_la_taille(glb, fiche['cadre'], fiche['titre'])
    date = datetime.date.today().isoformat()
    open(os.path.join(cible, 'LICENCE.txt'), 'w', encoding='utf-8').write(LICENCE.format(titre=fiche['titre'], graine=graine, date=date))
    return {'remplace': fiche['remplace'], 'date': date, 'graine': graine, 'texture': texture, 'faces': fiche['faces'], 'taille_m': taille}


def registre(fiche, fait):
    if not os.path.exists(REGISTRE):
        open(REGISTRE, 'w', encoding='utf-8').write(
            "# Registre des modèles faits avec l'IA\n\n"
            "> Un modèle par ligne : l'outil, la licence et le crédit à afficher (docs/IA-3D.md, § 4). Tenu à jour par `outils/generer-modeles-ia.py`.\n\n"
            "| Modèle | Remplace | Outils | Graine | Date | Licence | Crédit |\n|---|---|---|---|---|---|---|\n")
    outils = 'FLUX.1 [schnell] + TripoSG' + (' (forme et texture)' if fait['texture'] else ' (forme)')
    with open(REGISTRE, 'a', encoding='utf-8') as f:
        f.write(f"| `{fiche['cle']}` | `{fiche['remplace']}` | {outils} | {fait['graine']} | {fait['date']} | Apache 2.0 et MIT : usage commercial permis | Aucun obligatoire ; « Modèles générés avec FLUX.1 [schnell] et TripoSG » dans les crédits |\n")


def ajouter_au_projet(fiches, faits):
    """Les modèles faits entrent dans tailles-modeles.json et dans le catalogue de l'atelier ; puis la maquette est refaite."""
    tailles = json.load(open(TAILLES, encoding='utf-8'))
    for fiche in fiches:
        if fiche['cle'] in faits:
            tailles.setdefault(fiche['cle'], {'cadre': fiche['cadre']})
    texte = '{\n' + ',\n'.join(f'"{k}": ' + json.dumps(v, ensure_ascii=False) for k, v in tailles.items()) + '\n}\n'
    open(TAILLES, 'w', encoding='utf-8').write(texte)
    page = open(ATELIER, encoding='utf-8').read()
    m = re.search(r'const MODELES = (\[.*?\]);\n', page, flags=re.S)
    modeles = json.loads(m.group(1))
    connus = {f"{x['dossier']}/{x['nom']}" for x in modeles}
    for fiche in fiches:
        if fiche['cle'] in faits and fiche['cle'] not in connus:
            dossier, nom = fiche['cle'].split('/')
            modeles.append({'dossier': dossier, 'nom': nom, 'titre': fiche['titre'], 'auteur': 'IA : FLUX.1 schnell + TripoSG', 'compte': '',
                            'faces': fiche['faces'], 'anims': 0, 'usage': fiche['usage'] + ' (généré par IA)', 'uid': None, 'ia': True})
    modeles.sort(key=lambda x: (x['dossier'], x.get('ia', False)))
    page = page[:m.start(1)] + json.dumps(modeles, ensure_ascii=False, separators=(',', ':')) + page[m.end(1):]
    open(ATELIER, 'w', encoding='utf-8').write(page)
    subprocess.run([sys.executable, os.path.join(RACINE, 'outils', 'maquette-prologue.py')], check=True)


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument('--seulement', default='', help='les clés à faire, séparées par des virgules')
    p.add_argument('--sortie', default='', help='copier aussi les modèles dans ce dossier (celui que sert l\'atelier, ~/jeu/prologue)')
    p.add_argument('--graine', type=int, default=7)
    p.add_argument('--sans-texture', action='store_true')
    p.add_argument('--refaire', action='store_true')
    p.add_argument('--essai', action='store_true', help='lire les fiches et dire ce qui reste à faire, sans rien générer')
    a = p.parse_args()
    fiches = json.load(open(FICHES, encoding='utf-8'))['modeles']
    faits = json.load(open(FAITS, encoding='utf-8')) if os.path.exists(FAITS) else {}
    voulus = [f for f in fiches if (not a.seulement or f['cle'] in a.seulement.split(',')) and (a.refaire or f['cle'] not in faits)]
    print(f'{len(fiches)} fiches, {len(faits)} modèles déjà faits, {len(voulus)} à faire.')
    if a.essai or not voulus:
        for f in voulus:
            print('  à faire :', f['cle'], '(remplace', f['remplace'] + ')')
        return
    jeton = os.environ.get('HF_TOKEN')
    if not jeton:
        sys.exit('Il faut un jeton Hugging Face (gratuit, droit de lecture) dans la variable HF_TOKEN.')
    from gradio_client import Client
    client_image = Client(ESPACE_IMAGE, token=jeton, verbose=False)
    client_3d = Client(ESPACE_3D, token=jeton, verbose=False)
    try:
        for fiche in voulus:
            print(f"  {fiche['cle']}")
            fait = faire(fiche, a.graine, not a.sans_texture, client_image, client_3d)
            faits[fiche['cle']] = fait
            json.dump(faits, open(FAITS, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
            registre(fiche, fait)
            if a.sortie:
                dossier, nom = fiche['cle'].split('/')
                cible = os.path.join(os.path.expanduser(a.sortie), dossier, nom)
                shutil.copytree(os.path.join(MODELES_IA, dossier, nom), cible, dirs_exist_ok=True)
            print(f"    fait : {' × '.join(str(v) for v in fait['taille_m'])} m")
    except QuotaEpuise:
        print("\nLe quota GPU gratuit du jour est épuisé. Ce qui est fait est gardé : relance demain, le script reprendra là.")
    finally:
        if faits:
            ajouter_au_projet(fiches, faits)
    print(f'{len(faits)} modèles faits sur {len(fiches)}.')


if __name__ == '__main__':
    main()
