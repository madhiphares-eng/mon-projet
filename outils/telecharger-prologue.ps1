# Echos d'Ether - telecharge les modeles 3D du prologue (evasion du vaisseau-prison) dans le projet Unity.
# Usage (PowerShell, Unity ferme) :
#   irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-prologue.ps1 | iex
#
# Les modeles viennent de Sketchfab, qui demande un token API gratuit :
# creer un compte sur sketchfab.com, puis copier le token sur https://sketchfab.com/settings/password (section "API Token").
# Destination : C:\Projets\EchosEther\Assets\_Projet\Externes\Prologue\<dossier>\<nom>\
# Chaque modele a son LICENCE.txt ; les credits a citer sont regroupes dans CREDITS-PROLOGUE.txt a la racine du projet.
# Les fichiers deja presents ne sont pas retelecharges : on peut relancer sans risque.
# Variables facultatives : $env:SKETCHFAB_TOKEN, $env:ECHOS_PROJET, $env:ECHOS_LIMITE (pour tester : n premiers modeles).
# Liste detaillee et apercus : docs/PROLOGUE-MODELES.md

& {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'   # sinon Invoke-WebRequest est tres lent
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $Projet = if ($env:ECHOS_PROJET) { $env:ECHOS_PROJET } else { 'C:\Projets\EchosEther' }
    $Dest = Join-Path $Projet 'Assets\_Projet\Externes\Prologue'
    $Tmp = Join-Path ([IO.Path]::GetTempPath()) 'EchosEther-prologue'
    $MaxMo = 400
    $Limite = if ($env:ECHOS_LIMITE) { [int]$env:ECHOS_LIMITE } else { 0 }
    $GltfastVersion = '6.20.0'

    if (-not (Test-Path (Join-Path $Projet 'Assets'))) {
        Write-Host "Projet Unity introuvable : $Projet" -ForegroundColor Red
        return
    }

    # 'dossier|uid Sketchfab|nom' (licence relue au telechargement : CC0, CC BY ou CC BY-SA seulement)
    $Modeles = @(
        '01-Joueur|e0898165a3e24b34bff0ca230d92c311|tenue-detenu',
        '01-Joueur|da7b5c14bc634f8283e2d21343afa7cc|combinaison-veilleur',
        '02-Prisonniers|dbf9fe0cb81d4758ad2525a56d10e2fc|detenu-brute',
        '02-Prisonniers|6b8009a6e4f747f59ccadb121adc3679|detenu-vieux',
        '02-Prisonniers|9d1098bc72a74bb1b6371aef4b987100|detenu-atelier',
        '03-Gardes|9e82fabf26194896b5ad4a364d864eab|garde',
        '03-Gardes|6c0764c2bc4c4aaa8e03ba1653f455a6|droide-securite',
        '04-Pirates|f94ee65527b14bdcb5bafd1ad573c8ca|pirate-chef',
        '04-Pirates|f029fe3297434ed6b5d03c3ffbc0585d|pirate',
        '04-Pirates|9e77c91967ca4263ad347b395f53c1cb|pirate-masque',
        '04-Pirates|1059232ac0374767bf49ba95978f580f|casque-pirate',
        '04-Pirates|7fd399d488ae4576a0fc62715892956c|pirate-eva',
        '05-Cellules|1e80d7053fd7440f929bc53db8433509|bloc-cellules',
        '05-Cellules|f8148fab4dc44903b9644170edf0e58c|porte-cellule',
        '05-Cellules|23db3362debb4d98a56856416e1e9c58|porte-isolement',
        '05-Cellules|b8bdb85f59a44a8ea4b0783c8d22f373|toilettes',
        '05-Cellules|dcd2b91f54014e95bd488e12f0d54d61|lavabo',
        '05-Cellules|9fb7c54715dd41b98b3dc67b367cf81d|lit-superpose',
        '06-Couloirs|5e6240c9b30145b0bf81fd02287985e3|couloir-principal',
        '06-Couloirs|50e2af8800cc4ab79add375f817b2d76|kit-interieur',
        '06-Couloirs|f3d033a8f3ad4888acc8c0a16878cf85|kit-couloir',
        '06-Couloirs|6987b3d35d2441e4b83ed9f87fbb74ff|conduit-maintenance',
        '06-Couloirs|0b8188107ef040149f2f71eb08e9ce16|tuyaux',
        '06-Couloirs|2e8c613214144d12bce416ce437614d6|porte-coulissante',
        '06-Couloirs|0aea48c99ae24d78b7814d9dfbd94fe6|sols',
        '07-Consigne|fae22ece4d764979aafd614ebe09956d|casier',
        '07-Consigne|d16736e243904210b938590a42c8ea88|armoire-blindee',
        '07-Consigne|20d25f3bae24417a9b6cc5ba690c62f5|porte-coffre',
        '08-Machines|22e55de3be0c46caad827774109572f5|reacteur',
        '08-Machines|bf2e1a087adb4628a376ffbed17c41ff|generateur',
        '08-Machines|32c5436e8e9c459eac7471dce7c3878f|pylone-energie',
        '08-Machines|f717683d5502496d9e1ef1f1e1d1cb45|coeur-fusion',
        '09-Informatique|62f6779cb7e448b19aaf58544c3c7218|baie-serveur',
        '09-Informatique|6fe2cacf836b4aed96c650b286db5486|rangee-serveurs',
        '09-Informatique|98724ee49fba42a2b0ccbe79a36f246e|console-murale',
        '09-Informatique|298b7d9b68064618a3819d1a8fae4baf|grand-ecran',
        '09-Informatique|e03a14fc845e4e2d9573b95e00f79ef4|terminal',
        '09-Informatique|1f6a4938769846b1ae9b512038aec5f8|cables',
        '10-Hangar|4b263701219446f1ab13b37a5b98bac5|hangar',
        '10-Hangar|763fe7cff0534ec8a738b6127827eddd|caisses',
        '10-Hangar|8a1c32f395f549b8b22265b8a459fa8e|caisse-armes',
        '10-Hangar|e43322345d39429d82396a8642bb8d70|cellule-energie',
        '10-Hangar|04103035ea8c483db55ba61173929809|bidon-carburant',
        '11-Vaisseaux|e2d0fbe81c0e4182bec23e1adecba2db|vaisseau-prison',
        '11-Vaisseaux|db9e338f04ae402c82f8ebdf12b55f9e|vaisseau-prison-variante',
        '11-Vaisseaux|d3e5d9303d1d4e8b8a8e30bf22f59a21|navette-a-voler',
        '11-Vaisseaux|7a35a4f82e34446aa6131420bb26cc64|navette-transfert',
        '11-Vaisseaux|86d336e0fa6f40329de8cda4a71d8a17|vaisseau-pirate',
        '11-Vaisseaux|b158f01dd4a9416fb689ca4401856e7a|capsule-sauvetage',
        '12-Exterieur|58729c7672ae4e01a6103617c2f4f55e|porte-sas',
        '12-Exterieur|24521cd09bf54aafbd1dce8d8c731293|sas',
        '12-Exterieur|cdc2fb6ad6cd47cfa96182112b293d3f|cloison-etanche',
        '12-Exterieur|0a503e5bec784cbc9082e2f6b2d9f701|rocher-spatial',
        '12-Exterieur|c962c07f89c34e2a9d6714b3d408b878|debris-1',
        '12-Exterieur|f8fc1c20073448ccb4803335703c9129|debris-2',
        '12-Exterieur|53b50b002a0045c8b9f92858aa8c8dd7|gravats-breche',
        '13-Armes|60129a0e0cd340a38bf4b4c787a2dce6|fusil-gardes',
        '13-Armes|893ed8d2adaf4658847c4f36dab16f61|mitraillette-gardes',
        '13-Armes|54c52df11e164b42992a7d418d99cd3f|pistolet-gardes',
        '13-Armes|2d0d3410970d49c6865d00be15048f72|fusil-pirates',
        '13-Armes|eca46c628f49410081eb73391c62b4ca|arme-pirates',
        '13-Armes|1b91aca44b65417f957d9ec30ce8ee19|fusil-lourd-pirates',
        '13-Armes|fab28aeb14894752b49149ef00926aff|pistolet-use',
        '13-Armes|66b29c557ce04bb0b14bdde2cc204a2f|cle-a-molette',
        '14-Accessoires|5676b179b3b744c0aaae53a3dcea2300|extincteur',
        '14-Accessoires|2a05034d61dc49bf81b33de44add7aa8|neons',
        '14-Accessoires|342efe4d70f94d37a4edd66a15c39502|caisse-munitions',
        '14-Accessoires|ac36898521304e51a8e305aa602f1f5b|caisse-cargo',
        '15-Reliques|ffebf6fb62c840d08b0311ba6b8f9536|capsule-veilleur',
        '15-Reliques|8738f78ae4ec4630ac35a4783303ea95|chambre-stase',
        '15-Reliques|c93820e31f1b4a79a6a94269ed9f41b7|coeur-relique',
        '15-Reliques|c56fc34b2bd74b098b7ea68b6bcf215c|tablette-glyphes',
        '15-Reliques|9c288fb496ab4d15838c0517d0cef47c|anneau-relais',
        '15-Reliques|2eee63550ed6470dac7659211a605795|cristal-ether',
        '15-Reliques|25c72b016ace4f7aac72c5974437c5f2|prisme-ether'
    )

    $Etat = @{ Octets = [long]0; Erreurs = New-Object System.Collections.ArrayList; Lourds = New-Object System.Collections.ArrayList }

    function Telecharger([string]$url, [string]$chemin) {
        if (Test-Path -LiteralPath $chemin) { return $true }
        New-Item -ItemType Directory -Force -Path (Split-Path $chemin) | Out-Null
        $part = "$chemin.part"
        for ($essai = 1; $essai -le 3; $essai++) {
            try {
                Invoke-WebRequest -Uri $url -OutFile $part -UseBasicParsing
                Move-Item -LiteralPath $part -Destination $chemin -Force
                $Etat.Octets += (Get-Item -LiteralPath $chemin).Length
                return $true
            } catch {
                Remove-Item -LiteralPath $part -Force -ErrorAction SilentlyContinue
                if ($essai -eq 3) { throw }
                Start-Sleep -Seconds (5 * $essai)
            }
        }
    }

    function Api([string]$url, $entetes = @{}) {
        for ($essai = 1; $essai -le 3; $essai++) {
            try { return Invoke-RestMethod -Uri $url -Headers $entetes -UseBasicParsing }
            catch {
                $code = 0
                if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
                if ($code -eq 429 -and $essai -lt 3) { Write-Host '    limite du site atteinte, pause de 60 s...'; Start-Sleep -Seconds 60; continue }
                if ($code -ge 500 -and $essai -lt 3) { Start-Sleep -Seconds 10; continue }
                throw
            }
        }
    }

    Write-Host ''
    Write-Host "=== Modeles 3D du prologue d'Echos d'Ether ($($Modeles.Count) modeles, environ 2,6 Go) ===" -ForegroundColor Cyan

    $token = $env:SKETCHFAB_TOKEN
    if (-not $token) {
        Write-Host 'Colle ton token API Sketchfab (compte gratuit, https://sketchfab.com/settings/password, section "API Token").'
        $token = Read-Host 'Token'
    }
    if (-not $token) { Write-Host 'Pas de token : rien a faire.' -ForegroundColor Yellow; return }
    $Auth = @{ Authorization = "Token $($token.Trim())" }
    try { $moi = Api 'https://api.sketchfab.com/v3/me' $Auth; Write-Host "  Connecte a Sketchfab : $($moi.username)" -ForegroundColor Green }
    catch { Write-Host '  Token Sketchfab refuse : verifie-le sur https://sketchfab.com/settings/password' -ForegroundColor Red; return }

    if ((Get-Process -Name 'Unity' -ErrorAction SilentlyContinue) -and -not $env:SKETCHFAB_TOKEN) {
        Write-Host ''
        Write-Host "Unity est ouvert : il va importer chaque fichier pendant le telechargement (tres lent)." -ForegroundColor Yellow
        if ((Read-Host 'Le fermer est conseille. Continuer quand meme ? (o/N)') -notmatch '^[oOyY]') { return }
    }

    New-Item -ItemType Directory -Force -Path $Tmp | Out-Null

    # glTFast : importe les fichiers .glb / .gltf dans Unity
    $manifest = Join-Path $Projet 'Packages\manifest.json'
    if (Test-Path -LiteralPath $manifest) {
        $txt = [IO.File]::ReadAllText($manifest)
        if ($txt -notmatch 'com\.unity\.cloud\.gltfast') {
            $txt = $txt -replace '"dependencies"\s*:\s*\{', "`"dependencies`": {`n    `"com.unity.cloud.gltfast`": `"$GltfastVersion`","
            [IO.File]::WriteAllText($manifest, $txt)
            Write-Host "  Package glTFast $GltfastVersion ajoute au projet (pour les fichiers .glb)." -ForegroundColor Green
        }
    }

    $liste = if ($Limite -gt 0) { @($Modeles | Select-Object -First $Limite) } else { @($Modeles) }
    $i = 0
    foreach ($e in $liste) {
        $i++
        $dossier, $uid, $nom = $e -split '\|', 3
        Write-Host "  [$i/$($liste.Count)] $dossier/$nom"
        try {
            $d = Join-Path $Dest "$dossier\$nom"
            $licence = Join-Path $d 'LICENCE.txt'
            if (Test-Path -LiteralPath $licence) { continue }
            # La licence est relue a chaque fois : si l'auteur l'a changee, le modele est ignore.
            $info = Api "https://api.sketchfab.com/v3/models/$uid"
            if (-not $info.isDownloadable -or @('cc0', 'by', 'by-sa') -notcontains $info.license.slug) {
                Write-Host "    ignore : licence actuelle '$($info.license.slug)'" -ForegroundColor Yellow
                continue
            }
            $dl = Api "https://api.sketchfab.com/v3/models/$uid/download" $Auth
            $fichier = if ($dl.glb) { $dl.glb } else { $dl.gltf }
            if (-not $fichier) { throw 'aucun format glTF propose' }
            if ($fichier.size -gt $MaxMo * 1MB) {
                Write-Host ("    trop lourd ({0:N0} Mo) : a telecharger a la main" -f ($fichier.size / 1MB)) -ForegroundColor Yellow
                [void]$Etat.Lourds.Add(('{0}/{1} ({2:N0} Mo) : {3}' -f $dossier, $nom, ($fichier.size / 1MB), $info.viewerUrl))
                continue
            }
            if ($dl.glb) {
                [void](Telecharger $fichier.url (Join-Path $d "$nom.glb"))
            } else {
                $zip = Join-Path $Tmp "$uid.zip"
                [void](Telecharger $fichier.url $zip)
                Expand-Archive -LiteralPath $zip -DestinationPath $d -Force
                Remove-Item -LiteralPath $zip -Force
            }
            $credit = "`"$($info.name)`" par $($info.user.displayName) (sketchfab.com/$($info.user.username)), $($info.license.label), $($info.viewerUrl)"
            [IO.File]::WriteAllText($licence, $credit + "`r`n", (New-Object Text.UTF8Encoding $false))
            Start-Sleep -Milliseconds 300
        } catch {
            [void]$Etat.Erreurs.Add("$dossier/$nom : $($_.Exception.Message)")
            Write-Host "    [!] $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }

    # credits regroupes
    $credits = New-Object System.Collections.ArrayList
    [void]$credits.Add("Credits des modeles 3D du prologue d'Echos d'Ether (a reprendre dans l'ecran des credits du jeu).")
    [void]$credits.Add('')
    if (Test-Path -LiteralPath $Dest) {
        Get-ChildItem -LiteralPath $Dest -Recurse -Filter 'LICENCE.txt' | Sort-Object FullName | ForEach-Object {
            [void]$credits.Add([IO.File]::ReadAllText($_.FullName).Trim())
        }
    }
    [IO.File]::WriteAllText((Join-Path $Projet 'CREDITS-PROLOGUE.txt'), ($credits -join "`r`n") + "`r`n", (New-Object Text.UTF8Encoding $false))
    Remove-Item -LiteralPath $Tmp -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host ''
    Write-Host ("[OK] Termine : {0:N0} Mo telecharges dans {1}" -f ($Etat.Octets / 1MB), $Dest) -ForegroundColor Green
    foreach ($l in $Etat.Lourds) { Write-Host "  A la main : $l" -ForegroundColor Yellow }
    if ($Etat.Erreurs.Count -gt 0) {
        Write-Host "$($Etat.Erreurs.Count) echec(s) :" -ForegroundColor Yellow
        foreach ($x in $Etat.Erreurs) { Write-Host "  $x" -ForegroundColor Yellow }
        Write-Host 'Relance le script pour reessayer (ce qui est deja la est garde).' -ForegroundColor Yellow
    }
    Write-Host "Credits a citer : $(Join-Path $Projet 'CREDITS-PROLOGUE.txt')"
    if ($env:OS -eq 'Windows_NT' -and (Test-Path -LiteralPath $Dest)) { explorer $Dest }
}
