#!/usr/bin/env python3
"""Échos d'Éther : le bot de compilation. Vérifie que tous les scripts Unity du projet compilent, sans Unity.

Les scripts (Assets/_Projet/**/*.cs) sont compilés contre une imitation minimale de l'API Unity 6
(outils/bots/unity/UnityStubs*.cs), deux fois : avec le nouveau Input System (ENABLE_INPUT_SYSTEM) et sans.
Une erreur ici est presque toujours une vraie erreur dans Unity ; une API Unity qui manque à l'imitation se
corrige en l'ajoutant aux stubs (avec la signature exacte de Unity).

    python3 outils/bots/verifier_unity.py            # une ligne par erreur, puis le verdict
Il lui faut le SDK .NET 8 (dotnet). S'il manque, il l'installe dans ~/.dotnet (script officiel de Microsoft).
"""
import glob
import os
import re
import shutil
import subprocess
import sys
import tempfile

DEPOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
STUBS = os.path.join(DEPOT, 'outils', 'bots', 'unity')
PROJET = """<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>netstandard2.1</TargetFramework><LangVersion>9.0</LangVersion>
<Nullable>disable</Nullable><DefineConstants>{defines}</DefineConstants><EnableDefaultCompileItems>false</EnableDefaultCompileItems>
<NoWarn>CS0108;CS0114;CS0109</NoWarn></PropertyGroup>
<ItemGroup><Compile Include="{stubs}/*.cs" /><Compile Include="{depot}/Assets/_Projet/**/*.cs" /></ItemGroup></Project>"""


def dotnet():
    for c in [os.environ.get('DOTNET'), shutil.which('dotnet'), os.path.expanduser('~/.dotnet/dotnet')] + glob.glob('/tmp/claude-*/**/dotnet/dotnet', recursive=True)[:1]:
        if c and os.path.exists(c):
            return c
    print('dotnet manque : installation du SDK .NET 8 dans ~/.dotnet…', file=sys.stderr)
    script = os.path.join(tempfile.gettempdir(), 'dotnet-install.sh')
    subprocess.run(['curl', '-sSL', '-o', script, 'https://dot.net/v1/dotnet-install.sh'], check=True)
    subprocess.run(['bash', script, '--channel', '8.0', '--install-dir', os.path.expanduser('~/.dotnet')], check=True, stdout=subprocess.DEVNULL)
    return os.path.expanduser('~/.dotnet/dotnet')


def compiler(dn, nom, defines, dossier):
    os.makedirs(dossier, exist_ok=True)
    chemin = os.path.join(dossier, f'{nom}.csproj')
    with open(chemin, 'w') as f:
        f.write(PROJET.format(defines=defines, stubs=STUBS, depot=DEPOT))
    r = subprocess.run([dn, 'build', chemin, '-nologo', '-v', 'q', '-clp:NoSummary'], capture_output=True, text=True,
                       env={**os.environ, 'DOTNET_CLI_TELEMETRY_OPTOUT': '1', 'DOTNET_NOLOGO': '1'})
    erreurs = sorted({re.sub(r'\s*\[[^\]]*\.csproj\]$', '', l.replace(DEPOT + '/', '')).strip()
                      for l in r.stdout.splitlines() + r.stderr.splitlines() if ' error ' in l})
    return r.returncode == 0, erreurs


def main():
    dn = dotnet()
    dossier = os.path.join(tempfile.gettempdir(), 'echos-verifier-unity')
    total = 0
    for nom, defines in (('avec_input_system', 'UNITY_EDITOR;ENABLE_INPUT_SYSTEM'), ('sans_input_system', 'UNITY_EDITOR')):
        ok, erreurs = compiler(dn, nom, defines, dossier)
        total += len(erreurs) + (0 if ok or erreurs else 1)
        print(f'unity ({nom.replace("_", " ")}) : ' + ('compile' if ok else f'{len(erreurs)} erreur(s)'))
        for e in erreurs[:30]:
            print('  ' + e)
    fichiers = len(glob.glob(os.path.join(DEPOT, 'Assets', '_Projet', '**', '*.cs'), recursive=True))
    print(f'unity : {fichiers} scripts, ' + ('tout compile' if total == 0 else 'à corriger'))
    sys.exit(0 if total == 0 else 1)


if __name__ == '__main__':
    main()
