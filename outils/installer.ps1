# Echos d'Ether - installation sur Windows.
# Usage (PowerShell) :
#   irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/installer.ps1 | iex
#
# 1er lancement : cree C:\Projets. Creer ensuite le projet HDRP "EchosEther" dans Unity Hub.
# 2e lancement  : telecharge les scripts et la doc, et les copie dans le projet Unity.
# Relancer la commande plus tard met a jour les fichiers du jeu.

& {
    $ErrorActionPreference = 'Stop'
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $Racine = 'C:\Projets'
    $Projet = Join-Path $Racine 'EchosEther'
    $UrlZip = 'https://github.com/madhiphares-eng/mon-projet/archive/refs/heads/echos-ether.zip'
    $Temp   = Join-Path $env:TEMP 'EchosEther-install'

    Write-Host ''
    Write-Host '=== Echos d''Ether : installation ===' -ForegroundColor Cyan

    New-Item -ItemType Directory -Path $Racine -Force | Out-Null
    Write-Host "[OK] Dossier $Racine pret."

    if (-not (Test-Path (Join-Path $Projet 'Assets'))) {
        Write-Host ''
        Write-Host 'Le projet Unity n''existe pas encore. Dans Unity Hub :' -ForegroundColor Yellow
        Write-Host '  1. New project -> High Definition 3D'
        Write-Host '  2. Project name : EchosEther'
        Write-Host "  3. Location     : $Racine"
        Write-Host '  4. Create project, attendre la fin, puis RELANCER cette commande.'
        explorer $Racine
        return
    }

    Write-Host '[..] Telechargement des fichiers du jeu depuis GitHub...'
    Remove-Item $Temp -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $Temp -Force | Out-Null
    $Zip = Join-Path $Temp 'echos-ether.zip'
    Invoke-WebRequest -Uri $UrlZip -OutFile $Zip -UseBasicParsing
    Expand-Archive -Path $Zip -DestinationPath $Temp -Force
    $Source = Get-ChildItem $Temp -Directory | Select-Object -First 1

    Copy-Item (Join-Path $Source.FullName 'Assets\_Projet') (Join-Path $Projet 'Assets') -Recurse -Force
    Copy-Item (Join-Path $Source.FullName 'docs') $Projet -Recurse -Force
    foreach ($f in 'README.md', '.gitignore', '.gitattributes') {
        Copy-Item (Join-Path $Source.FullName $f) $Projet -Force
    }
    Remove-Item $Temp -Recurse -Force

    Write-Host '[OK] Scripts copies dans Assets\_Projet' -ForegroundColor Green
    Write-Host '[OK] GDD et documentation copies dans docs\' -ForegroundColor Green
    Write-Host ''
    Write-Host 'Retourne dans Unity : il importe les fichiers tout seul (quelques secondes).'
    explorer (Join-Path $Projet 'Assets\_Projet')
}
