# Echos d'Ether - installation sur Windows.
# Usage (PowerShell) :
#   irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/installer.ps1 | iex
#
# 1er lancement : cree C:\Projets. Creer ensuite le projet HDRP "EchosEther" dans Unity Hub.
# 2e lancement  : telecharge les scripts et la doc, et les copie dans le projet Unity.
# Relancer la commande plus tard met a jour les fichiers du jeu. Les fichiers modifies sur ce PC
# (par exemple par Claude en local) sont sauvegardes dans C:\Projets\EchosEther\Sauvegardes avant d'etre remplaces.

& {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $Racine = 'C:\Projets'
    $Projet = 'C:\Projets\EchosEther'
    if ($env:ECHOS_PROJET) { $Projet = $env:ECHOS_PROJET; $Racine = Split-Path $Projet }   # pour les tests
    $UrlZip = 'https://github.com/madhiphares-eng/mon-projet/archive/refs/heads/echos-ether.zip'
    $Temp   = Join-Path ([IO.Path]::GetTempPath()) 'EchosEther-install'

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
        if ($env:OS -eq 'Windows_NT') { explorer $Racine }
        return
    }

    Write-Host '[..] Telechargement des fichiers du jeu depuis GitHub...'
    Remove-Item $Temp -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $Temp -Force | Out-Null
    $Zip = Join-Path $Temp 'echos-ether.zip'
    Invoke-WebRequest -Uri $UrlZip -OutFile $Zip -UseBasicParsing
    Expand-Archive -Path $Zip -DestinationPath $Temp -Force
    $Source = Get-ChildItem $Temp -Directory | Select-Object -First 1

    # Copie fichier par fichier : un fichier modifie sur ce PC (par exemple par Claude en local)
    # est d'abord sauvegarde dans Sauvegardes\<date>, hors du dossier Assets.
    $Sauvegarde = Join-Path $Projet ('Sauvegardes\' + (Get-Date -Format 'yyyy-MM-dd_HH-mm'))
    $Etat = @{ Sauves = 0; Copies = 0 }
    $RacineSource = $Source.FullName
    $Fichiers = @(Get-ChildItem -LiteralPath (Join-Path $RacineSource 'Assets\_Projet') -Recurse -File)
    $Fichiers += @(Get-ChildItem -LiteralPath (Join-Path $RacineSource 'docs') -Recurse -File)
    foreach ($f in 'README.md', '.gitignore', '.gitattributes') { $Fichiers += Get-Item -LiteralPath (Join-Path $RacineSource $f) }
    foreach ($f in $Fichiers) {
        $Relatif = $f.FullName.Substring($RacineSource.Length).TrimStart('\', '/')
        $Cible = Join-Path $Projet $Relatif
        if (Test-Path -LiteralPath $Cible) {
            if ((Get-FileHash -LiteralPath $f.FullName).Hash -eq (Get-FileHash -LiteralPath $Cible).Hash) { continue }
            $Copie = Join-Path $Sauvegarde $Relatif
            New-Item -ItemType Directory -Force -Path (Split-Path $Copie) | Out-Null
            Copy-Item -LiteralPath $Cible -Destination $Copie -Force
            $Etat.Sauves++
        }
        New-Item -ItemType Directory -Force -Path (Split-Path $Cible) | Out-Null
        Copy-Item -LiteralPath $f.FullName -Destination $Cible -Force
        $Etat.Copies++
    }
    Remove-Item $Temp -Recurse -Force
    Write-Host "[OK] $($Etat.Copies) fichier(s) nouveaux ou mis a jour." -ForegroundColor Green
    if ($Etat.Sauves -gt 0) {
        Write-Host "[i] $($Etat.Sauves) fichier(s) modifies sur ce PC ont ete remplaces ; l'ancienne version est gardee dans :" -ForegroundColor Yellow
        Write-Host "    $Sauvegarde" -ForegroundColor Yellow
    }

    Write-Host '[OK] Scripts dans Assets\_Projet, documentation dans docs\' -ForegroundColor Green
    Write-Host ''
    Write-Host 'Retourne dans Unity : il importe les fichiers tout seul (quelques secondes).'
    Write-Host "Puis menu Echos d'Ether > Installer le jeu dans la scene, et Play."
    if ($env:OS -eq 'Windows_NT') { explorer (Join-Path $Projet 'Assets\_Projet') }
}
