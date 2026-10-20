# Echos d'Ether - telecharge les assets Poly Haven (licence CC0) dans le projet Unity.
# Usage (PowerShell) :
#   irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-assets.ps1 | iex
#
# Destination : C:\Projets\EchosEther\Assets\_Projet\Externes\PolyHaven
# Les fichiers deja presents ne sont pas retelecharges : on peut relancer sans risque.

& {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'   # sinon Invoke-WebRequest est tres lent
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $Projet = 'C:\Projets\EchosEther'
    $Dest = Join-Path $Projet 'Assets\_Projet\Externes\PolyHaven'
    $Res = '2k'

    if (-not (Test-Path (Join-Path $Projet 'Assets'))) {
        Write-Host "Projet Unity introuvable : $Projet" -ForegroundColor Red
        return
    }

    # Rochers, falaises et bois mort scannes dans un vrai desert (collection Namaqualand)
    $Modeles = @(
        'namaqualand_boulder_02', 'namaqualand_boulder_03', 'namaqualand_boulder_04', 'namaqualand_boulder_05',
        'namaqualand_boulder_06', 'namaqualand_boulders_01', 'namaqualand_cliff_01', 'namaqualand_cliff_02',
        'namaqualand_rocks_01', 'namaqualand_stones_01', 'dead_quiver_trunk', 'dead_quiver_branch_01'
    )
    # Materiaux du sol : sable, gravier, roche stratifiee, sol craquele
    $Textures = @(
        'aerial_sand', 'gravelly_sand', 'sandy_gravel_02', 'cracked_red_ground',
        'cliff_side', 'rock_face', 'tiger_rock'
    )

    function Telecharger($url, $chemin) {
        if (-not $url) { return }
        if (Test-Path $chemin) { return }
        New-Item -ItemType Directory -Force -Path (Split-Path $chemin) | Out-Null
        Invoke-WebRequest -Uri $url -OutFile $chemin -UseBasicParsing
    }

    Write-Host ''
    Write-Host "=== Assets Poly Haven ($Res, licence CC0) ===" -ForegroundColor Cyan

    foreach ($m in $Modeles) {
        Write-Host "[..] Modele $m"
        $f = Invoke-RestMethod "https://api.polyhaven.com/files/$m"
        $fbx = $f.fbx.$Res.fbx
        $dossier = Join-Path $Dest "Modeles\$m"
        Telecharger $fbx.url (Join-Path $dossier "$m.fbx")
        foreach ($p in $fbx.include.PSObject.Properties) {
            Telecharger $p.Value.url (Join-Path $dossier ($p.Name -replace '/', '\'))
        }
    }

    foreach ($t in $Textures) {
        Write-Host "[..] Texture $t"
        $f = Invoke-RestMethod "https://api.polyhaven.com/files/$t"
        foreach ($carte in 'Diffuse', 'nor_gl', 'Rough', 'AO', 'Displacement') {
            $c = $f.$carte
            if (-not $c) { continue }
            $u = $c.$Res.jpg.url
            if (-not $u) { $u = $c.$Res.png.url }
            if ($u) { Telecharger $u (Join-Path $Dest "Textures\$t\$(Split-Path $u -Leaf)") }
        }
    }

    $lisezmoi = Join-Path $Dest 'LISEZMOI.txt'
    if (-not (Test-Path $lisezmoi)) {
        "Assets de Poly Haven (https://polyhaven.com), licence CC0 : utilisation libre, meme commerciale, sans obligation de credit.`r`nLes cartes 'nor_gl' sont des normal maps : dans Unity, regler leur Texture Type sur 'Normal map'." |
            Set-Content -Path $lisezmoi -Encoding UTF8
    }

    Write-Host ''
    Write-Host "[OK] Termine : $Dest" -ForegroundColor Green
    Write-Host 'Retourne dans Unity : il importe les fichiers tout seul (cela peut prendre quelques minutes).'
    explorer $Dest
}
