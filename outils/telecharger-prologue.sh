#!/usr/bin/env bash
# Echos d'Ether : modeles 3D du prologue (evasion du vaisseau-prison), version Mac ou Linux.
#
# Usage (Terminal du Mac) :
#   curl -fsSL https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-prologue.sh | bash -s -- ~/jeu/prologue
#
# Les modeles viennent de Sketchfab, qui demande un token API gratuit :
# creer un compte sur sketchfab.com, puis copier le token sur https://sketchfab.com/settings/password (section "API Token").
# Le script le demande ; on peut aussi le donner d'avance : ... | SKETCHFAB_TOKEN=xxxx bash -s -- ~/jeu/prologue
#
# Rangement : <dossier choisi>/<lieu>/<nom>/<nom>.glb (ou scene.gltf et ses textures), avec un LICENCE.txt par modele
# et CREDITS-PROLOGUE.txt a la racine. Meme rangement que sur le PC (Assets/_Projet/Externes/Prologue) :
# le dossier peut etre copie tel quel dans le projet Unity.
# Pour les voir en 3D : vitrine/galerie-prologue.html (mode d'emploi dans docs/PROLOGUE-MODELES.md).
# Relancer le script ne retelecharge que ce qui manque. Necessite seulement curl et unzip (presents sur macOS).

DEST="${1:-./prologue}"
TOKEN="${SKETCHFAB_TOKEN:-}"
API="${ECHOS_SKETCHFAB_API:-https://api.sketchfab.com/v3}"   # remplacable pour les tests
MAX_MO=400
LIMITE="${ECHOS_LIMITE:-0}"

mkdir -p "$DEST" || { echo "Impossible de creer $DEST"; exit 1; }
DEST="$(cd "$DEST" && pwd)"
TMP="$(mktemp -d 2>/dev/null || mktemp -d -t echos)"
trap 'rm -rf "$TMP"' EXIT
ERREURS="$TMP/erreurs.txt"; : > "$ERREURS"
OCTETS=0

etape() { printf '  %s\n' "$1"; }
erreur() { printf '    [!] %s\n' "$1"; printf '%s\n' "$1" >> "$ERREURS"; }

# telecharger URL CHEMIN : reessaie 3 fois, ecrit d'abord dans un .part
telecharger() {
    local url="$1" cible="$2" essai
    mkdir -p "$(dirname "$cible")"
    for essai in 1 2 3; do
        if curl -fsSL --retry 2 -o "$cible.part" "$url"; then
            mv "$cible.part" "$cible"
            OCTETS=$((OCTETS + $(wc -c < "$cible")))
            return 0
        fi
        rm -f "$cible.part"
        sleep $((essai * 5))
    done
    return 1
}

# extrait la valeur texte d'une cle JSON dans un fichier (premiere occurrence)
valeur() { grep -oE "\"$1\"[[:space:]]*:[[:space:]]*\"[^\"]*\"" "$2" | head -1 | sed -E 's/.*:[[:space:]]*"(.*)"$/\1/'; }

# lieu|uid Sketchfab|nom|credit (licence relue au telechargement : CC0, CC BY ou CC BY-SA seulement)
MODELES=$(cat <<'LISTE'
01-Joueur|e0898165a3e24b34bff0ca230d92c311|tenue-detenu|"Prisoner" par asadkhan1881 (sketchfab.com/asadkhan1881), CC Attribution, https://sketchfab.com/3d-models/e0898165a3e24b34bff0ca230d92c311
01-Joueur|7fd399d488ae4576a0fc62715892956c|combinaison|"Mercenary Astronaut" par Peter_D (sketchfab.com/better_peter), CC Attribution, https://sketchfab.com/3d-models/7fd399d488ae4576a0fc62715892956c
02-Prisonniers|dbf9fe0cb81d4758ad2525a56d10e2fc|detenu-brute|"Balthazar (rigged & animated)" par Jungle Jim (sketchfab.com/jungle_jim), CC Attribution, https://sketchfab.com/3d-models/dbf9fe0cb81d4758ad2525a56d10e2fc
02-Prisonniers|6b8009a6e4f747f59ccadb121adc3679|detenu-vieux|"Realistic Old Russian Guy (animated)" par Jungle Jim (sketchfab.com/jungle_jim), CC Attribution, https://sketchfab.com/3d-models/6b8009a6e4f747f59ccadb121adc3679
02-Prisonniers|9d1098bc72a74bb1b6371aef4b987100|detenu-atelier|"Employee" par dotflare (sketchfab.com/Dotflare), CC Attribution, https://sketchfab.com/3d-models/9d1098bc72a74bb1b6371aef4b987100
03-Gardes|9e82fabf26194896b5ad4a364d864eab|garde|"S.W.A.T. Operator" par SpatialNeglect (sketchfab.com/jeandiz), CC Attribution, https://sketchfab.com/3d-models/9e82fabf26194896b5ad4a364d864eab
03-Gardes|6c0764c2bc4c4aaa8e03ba1653f455a6|droide-securite|"Sci-fi O.B. Robot Unit TH-ICC02 (animated)" par Jungle Jim (sketchfab.com/jungle_jim), CC Attribution, https://sketchfab.com/3d-models/6c0764c2bc4c4aaa8e03ba1653f455a6
04-Pirates|f94ee65527b14bdcb5bafd1ad573c8ca|pirate-chef|"Space Pirate" par Odlab (sketchfab.com/Odlab), CC Attribution, https://sketchfab.com/3d-models/f94ee65527b14bdcb5bafd1ad573c8ca
04-Pirates|f029fe3297434ed6b5d03c3ffbc0585d|pirate|"Scavenger" par Vasian-Digital3D (sketchfab.com/Vasian-Digital3D), CC Attribution, https://sketchfab.com/3d-models/f029fe3297434ed6b5d03c3ffbc0585d
04-Pirates|9e77c91967ca4263ad347b395f53c1cb|pirate-masque|"post apo character" par Am I dead? (sketchfab.com/eqisX), CC Attribution, https://sketchfab.com/3d-models/9e77c91967ca4263ad347b395f53c1cb
04-Pirates|1059232ac0374767bf49ba95978f580f|casque-pirate|"Vintage 2725 ' Spacer Armoured Pressure Helmet '" par KallMor (sketchfab.com/kallmor), CC Attribution, https://sketchfab.com/3d-models/1059232ac0374767bf49ba95978f580f
05-Cellules|1e80d7053fd7440f929bc53db8433509|bloc-cellules|"Abandoned prison modular scene" par Ali2-D2 (sketchfab.com/Ali2-D2), CC Attribution, https://sketchfab.com/3d-models/1e80d7053fd7440f929bc53db8433509
05-Cellules|f8148fab4dc44903b9644170edf0e58c|porte-cellule|"Prison cell door" par Rivaldragon (sketchfab.com/Rivaldragon), CC Attribution, https://sketchfab.com/3d-models/f8148fab4dc44903b9644170edf0e58c
05-Cellules|23db3362debb4d98a56856416e1e9c58|porte-isolement|"Door Of The Solitary Cell Of The Prison 14MB" par Mehdi Shahsavan (sketchfab.com/ahmagh2e), CC Attribution, https://sketchfab.com/3d-models/23db3362debb4d98a56856416e1e9c58
05-Cellules|b8bdb85f59a44a8ea4b0783c8d22f373|toilettes|"Filthy Prison Toilet - 4096px²" par Mark Peters (sketchfab.com/mark-peters), CC Attribution, https://sketchfab.com/3d-models/b8bdb85f59a44a8ea4b0783c8d22f373
05-Cellules|dcd2b91f54014e95bd488e12f0d54d61|lavabo|"Filthy Metal Sink - 4096px²" par Mark Peters (sketchfab.com/mark-peters), CC Attribution, https://sketchfab.com/3d-models/dcd2b91f54014e95bd488e12f0d54d61
05-Cellules|9fb7c54715dd41b98b3dc67b367cf81d|lit-superpose|"Simple Bunk Bed" par Blender3D (sketchfab.com/Blender3D), CC Attribution, https://sketchfab.com/3d-models/9fb7c54715dd41b98b3dc67b367cf81d
06-Couloirs|5e6240c9b30145b0bf81fd02287985e3|couloir-principal|"Sci-fi corridor" par 3dhdscan (sketchfab.com/3dhdscan), CC Attribution, https://sketchfab.com/3d-models/5e6240c9b30145b0bf81fd02287985e3
06-Couloirs|50e2af8800cc4ab79add375f817b2d76|kit-interieur|"Sci-fi Ship interior - Modular asset pack" par Rejala (sketchfab.com/Rejala), CC Attribution, https://sketchfab.com/3d-models/50e2af8800cc4ab79add375f817b2d76
06-Couloirs|f3d033a8f3ad4888acc8c0a16878cf85|kit-couloir|"Sci-fi Corridor" par Eliu_Villanueva (sketchfab.com/Eliu_Villanueva), CC Attribution, https://sketchfab.com/3d-models/f3d033a8f3ad4888acc8c0a16878cf85
06-Couloirs|6987b3d35d2441e4b83ed9f87fbb74ff|conduit-maintenance|"Sci-fi Hallway Tube" par SlyEye (sketchfab.com/SlyEyePro), CC Attribution, https://sketchfab.com/3d-models/6987b3d35d2441e4b83ed9f87fbb74ff
06-Couloirs|0b8188107ef040149f2f71eb08e9ce16|tuyaux|"Sci-Fi Machinery: Pipes Modular Asset Pack" par somni (sketchfab.com/3d.somni), CC Attribution, https://sketchfab.com/3d-models/0b8188107ef040149f2f71eb08e9ce16
06-Couloirs|2e8c613214144d12bce416ce437614d6|porte-coulissante|"Sci-fi Door (high poly)" par Scarecrow_original (sketchfab.com/Scarecrow_original), CC Attribution, https://sketchfab.com/3d-models/2e8c613214144d12bce416ce437614d6
06-Couloirs|0aea48c99ae24d78b7814d9dfbd94fe6|sols|"PBR Sci-Fi Modular Flooring" par JamesDawson (sketchfab.com/JamBoink), CC Attribution, https://sketchfab.com/3d-models/0aea48c99ae24d78b7814d9dfbd94fe6
07-Consigne|fae22ece4d764979aafd614ebe09956d|casier|"Locker Equipment" par Siberia (sketchfab.com/LtxxwSibeRia), CC Attribution, https://sketchfab.com/3d-models/fae22ece4d764979aafd614ebe09956d
07-Consigne|d16736e243904210b938590a42c8ea88|armoire-blindee|"Armored Cabinet" par Ray Ferric (sketchfab.com/rayferric), CC Attribution, https://sketchfab.com/3d-models/d16736e243904210b938590a42c8ea88
07-Consigne|20d25f3bae24417a9b6cc5ba690c62f5|porte-coffre|"Vault Door" par Ray Ferric (sketchfab.com/rayferric), CC Attribution, https://sketchfab.com/3d-models/20d25f3bae24417a9b6cc5ba690c62f5
08-Machines|22e55de3be0c46caad827774109572f5|reacteur|"ARC Pulse Core" par TooManyDemons (sketchfab.com/toomanydemons), CC Attribution, https://sketchfab.com/3d-models/22e55de3be0c46caad827774109572f5
08-Machines|bf2e1a087adb4628a376ffbed17c41ff|generateur|"SCIFI GENERATOR" par LordCinn (sketchfab.com/LordCinn), CC Attribution, https://sketchfab.com/3d-models/bf2e1a087adb4628a376ffbed17c41ff
08-Machines|32c5436e8e9c459eac7471dce7c3878f|pylone-energie|"Sci-fi generator" par aksinia0987 (sketchfab.com/aksinia0987), CC Attribution, https://sketchfab.com/3d-models/32c5436e8e9c459eac7471dce7c3878f
08-Machines|f717683d5502496d9e1ef1f1e1d1cb45|coeur-fusion|"FUSION CORE" par Fred Drabble (sketchfab.com/FredDrabble), CC Attribution, https://sketchfab.com/3d-models/f717683d5502496d9e1ef1f1e1d1cb45
09-Informatique|62f6779cb7e448b19aaf58544c3c7218|baie-serveur|"Server Rack" par Spellkaze (sketchfab.com/Spellkaze), CC Attribution, https://sketchfab.com/3d-models/62f6779cb7e448b19aaf58544c3c7218
09-Informatique|6fe2cacf836b4aed96c650b286db5486|rangee-serveurs|"Server Racking System" par wpanayides (sketchfab.com/mcpanayides), CC Attribution, https://sketchfab.com/3d-models/6fe2cacf836b4aed96c650b286db5486
09-Informatique|98724ee49fba42a2b0ccbe79a36f246e|console-murale|"Wall Console" par LuddePudde (sketchfab.com/luddepubde), CC Attribution, https://sketchfab.com/3d-models/98724ee49fba42a2b0ccbe79a36f246e
09-Informatique|298b7d9b68064618a3819d1a8fae4baf|grand-ecran|"Large Wall-Mounted Computer Console" par Inditrion Dradnon (sketchfab.com/dradnon), CC Attribution, https://sketchfab.com/3d-models/298b7d9b68064618a3819d1a8fae4baf
09-Informatique|e03a14fc845e4e2d9573b95e00f79ef4|terminal|"Sci-fi Computer Terminal" par Pedro Belthori (sketchfab.com/pedrobelthori), CC Attribution, https://sketchfab.com/3d-models/e03a14fc845e4e2d9573b95e00f79ef4
09-Informatique|1f6a4938769846b1ae9b512038aec5f8|cables|"Electric cable - 13MB" par Mehdi Shahsavan (sketchfab.com/ahmagh2e), CC Attribution, https://sketchfab.com/3d-models/1f6a4938769846b1ae9b512038aec5f8
10-Hangar|4b263701219446f1ab13b37a5b98bac5|hangar|"SCI FI HANGAR" par Kerem Kavalci (sketchfab.com/Keremz), CC Attribution, https://sketchfab.com/3d-models/4b263701219446f1ab13b37a5b98bac5
10-Hangar|763fe7cff0534ec8a738b6127827eddd|caisses|"Game Ready Sci-Fi Crates" par celebornArt (sketchfab.com/celebornArt), CC Attribution, https://sketchfab.com/3d-models/763fe7cff0534ec8a738b6127827eddd
10-Hangar|8a1c32f395f549b8b22265b8a459fa8e|caisse-armes|"Military Weapons Container" par MeshSlinger (sketchfab.com/thelastphysician), CC Attribution, https://sketchfab.com/3d-models/8a1c32f395f549b8b22265b8a459fa8e
10-Hangar|e43322345d39429d82396a8642bb8d70|cellule-energie|"Sci-Fi Power Cell Prop" par pixelgrapher (sketchfab.com/pixelgrapher), CC Attribution, https://sketchfab.com/3d-models/e43322345d39429d82396a8642bb8d70
10-Hangar|04103035ea8c483db55ba61173929809|bidon-carburant|"Sci-fi Fuel Cell Canister" par Daniel.Sharma (sketchfab.com/Daniel.Sharma), CC Attribution, https://sketchfab.com/3d-models/04103035ea8c483db55ba61173929809
11-Vaisseaux|e2d0fbe81c0e4182bec23e1adecba2db|vaisseau-prison|"The Star Prison (Basemesh&Greebles)" par Relvean (sketchfab.com/Relvean), CC Attribution, https://sketchfab.com/3d-models/e2d0fbe81c0e4182bec23e1adecba2db
11-Vaisseaux|db9e338f04ae402c82f8ebdf12b55f9e|vaisseau-prison-variante|"Gangut space hub" par Comrade1280 (sketchfab.com/comrade1280), CC Attribution, https://sketchfab.com/3d-models/db9e338f04ae402c82f8ebdf12b55f9e
11-Vaisseaux|d3e5d9303d1d4e8b8a8e30bf22f59a21|navette-a-voler|"Sci-fi shuttle" par Hrvoje Wächter (sketchfab.com/gazdahrco), CC Attribution, https://sketchfab.com/3d-models/d3e5d9303d1d4e8b8a8e30bf22f59a21
11-Vaisseaux|7a35a4f82e34446aa6131420bb26cc64|navette-transfert|"X-4A Enforcer Assault Transport" par pandoraskitten (sketchfab.com/pandoraskitten), CC Attribution, https://sketchfab.com/3d-models/7a35a4f82e34446aa6131420bb26cc64
11-Vaisseaux|86d336e0fa6f40329de8cda4a71d8a17|vaisseau-pirate|"Heavy VTOL gunship" par Kai Xiang (sketchfab.com/kirikom9000), CC Attribution, https://sketchfab.com/3d-models/86d336e0fa6f40329de8cda4a71d8a17
11-Vaisseaux|b158f01dd4a9416fb689ca4401856e7a|capsule-sauvetage|"Sci-Fi Personal Space Pod Ship(weekly challenge)" par futaba@blender (sketchfab.com/futaba_blender), CC Attribution, https://sketchfab.com/3d-models/b158f01dd4a9416fb689ca4401856e7a
12-Exterieur|58729c7672ae4e01a6103617c2f4f55e|porte-sas|"Free Airlock Door" par michael_grodkowski (sketchfab.com/michael_grodkowski), CC Attribution, https://sketchfab.com/3d-models/58729c7672ae4e01a6103617c2f4f55e
12-Exterieur|24521cd09bf54aafbd1dce8d8c731293|sas|"Sci-Fi Airlock" par hamraj15 (sketchfab.com/hamraj15), CC Attribution, https://sketchfab.com/3d-models/24521cd09bf54aafbd1dce8d8c731293
12-Exterieur|cdc2fb6ad6cd47cfa96182112b293d3f|cloison-etanche|"Ship Door / Bulkhead" par lumoize (sketchfab.com/lumoize), CC Attribution, https://sketchfab.com/3d-models/cdc2fb6ad6cd47cfa96182112b293d3f
12-Exterieur|0a503e5bec784cbc9082e2f6b2d9f701|rocher-spatial|"Meteorite" par Hailphilly (sketchfab.com/hailphilly), CC Attribution, https://sketchfab.com/3d-models/0a503e5bec784cbc9082e2f6b2d9f701
12-Exterieur|c962c07f89c34e2a9d6714b3d408b878|debris-1|"SpaceShips Junk Yard ASSET (part1)" par SebastianSosnowski (sketchfab.com/SebastianSosnowski), CC Attribution, https://sketchfab.com/3d-models/c962c07f89c34e2a9d6714b3d408b878
12-Exterieur|f8fc1c20073448ccb4803335703c9129|debris-2|"SpaceShips Junk Yard ASSET (part2)" par SebastianSosnowski (sketchfab.com/SebastianSosnowski), CC Attribution, https://sketchfab.com/3d-models/f8fc1c20073448ccb4803335703c9129
12-Exterieur|53b50b002a0045c8b9f92858aa8c8dd7|gravats-breche|"Construction debris" par Alex Sade (sketchfab.com/Alexsade), CC Attribution, https://sketchfab.com/3d-models/53b50b002a0045c8b9f92858aa8c8dd7
13-Armes|60129a0e0cd340a38bf4b4c787a2dce6|fusil-gardes|"ASL - Standart Issue Assault Rifle" par JmCVoyager (sketchfab.com/JmCVoyager), CC Attribution, https://sketchfab.com/3d-models/60129a0e0cd340a38bf4b4c787a2dce6
13-Armes|893ed8d2adaf4658847c4f36dab16f61|mitraillette-gardes|"Futuristic Submachine Gun" par Newsome Interactive (sketchfab.com/newsomellc), CC Attribution, https://sketchfab.com/3d-models/893ed8d2adaf4658847c4f36dab16f61
13-Armes|54c52df11e164b42992a7d418d99cd3f|pistolet-gardes|"Pistol" par SINNIK (sketchfab.com/sinnik), CC Attribution, https://sketchfab.com/3d-models/54c52df11e164b42992a7d418d99cd3f
13-Armes|2d0d3410970d49c6865d00be15048f72|fusil-pirates|"Sci-Fi AK Rifle gameready (Free)" par Egor Likhachev (sketchfab.com/egor_likhachev), CC Attribution, https://sketchfab.com/3d-models/2d0d3410970d49c6865d00be15048f72
13-Armes|eca46c628f49410081eb73391c62b4ca|arme-pirates|"Sci Fi Weapon. Gameready Gun / Rifle." par gbarzu (sketchfab.com/gbarzu), CC Attribution, https://sketchfab.com/3d-models/eca46c628f49410081eb73391c62b4ca
13-Armes|1b91aca44b65417f957d9ec30ce8ee19|fusil-lourd-pirates|"Heavy Assault Rifle" par valerij1987 (sketchfab.com/valerij1987), CC Attribution, https://sketchfab.com/3d-models/1b91aca44b65417f957d9ec30ce8ee19
13-Armes|fab28aeb14894752b49149ef00926aff|pistolet-use|"Sci-Fi Pistol" par Andrei Milin (sketchfab.com/milinam2002), CC Attribution, https://sketchfab.com/3d-models/fab28aeb14894752b49149ef00926aff
13-Armes|66b29c557ce04bb0b14bdde2cc204a2f|cle-a-molette|"Weathered Wrench (Game Model)" par Michael Makivic (sketchfab.com/makivic), CC Attribution, https://sketchfab.com/3d-models/66b29c557ce04bb0b14bdde2cc204a2f
14-Accessoires|5676b179b3b744c0aaae53a3dcea2300|extincteur|"Fire Extinguisher" par Loïc (sketchfab.com/loichuet1), CC Attribution, https://sketchfab.com/3d-models/5676b179b3b744c0aaae53a3dcea2300
14-Accessoires|2a05034d61dc49bf81b33de44add7aa8|neons|"Fluorescent Light Fixtures - 4x 4096px²" par Mark Peters (sketchfab.com/mark-peters), CC Attribution, https://sketchfab.com/3d-models/2a05034d61dc49bf81b33de44add7aa8
14-Accessoires|342efe4d70f94d37a4edd66a15c39502|caisse-munitions|"Sci-Fi crate / ammunition box (2)" par ul1tka (sketchfab.com/ul1tka), CC Attribution, https://sketchfab.com/3d-models/342efe4d70f94d37a4edd66a15c39502
14-Accessoires|ac36898521304e51a8e305aa602f1f5b|caisse-cargo|"Sci-fi cargo crate" par andreas9343 (sketchfab.com/andreas9343), CC Attribution, https://sketchfab.com/3d-models/ac36898521304e51a8e305aa602f1f5b
LISTE
)

total=$(printf '%s\n' "$MODELES" | grep -c '|')
echo ""
echo "=== Modeles 3D du prologue d'Echos d'Ether ($total modeles, environ 2,5 Go) ==="
echo "Dossier : $DEST"

if [ -z "$TOKEN" ]; then
    echo 'Colle ton token API Sketchfab (compte gratuit, https://sketchfab.com/settings/password, section "API Token"), puis Entree :'
    if [ -r /dev/tty ]; then read -r TOKEN < /dev/tty; fi
fi
TOKEN="$(printf '%s' "$TOKEN" | tr -d '[:space:]')"
if [ -z "$TOKEN" ]; then echo "Pas de token : rien a faire."; exit 1; fi
if ! curl -fsS -o "$TMP/moi.json" -H "Authorization: Token $TOKEN" "$API/me"; then
    echo "Token Sketchfab refuse : verifie-le sur https://sketchfab.com/settings/password"; exit 1
fi
echo "  Connecte a Sketchfab : $(valeur username "$TMP/moi.json")"

i=0
printf '%s\n' "$MODELES" | grep '|' > "$TMP/liste.txt"
while IFS='|' read -r lieu uid nom credit; do
    i=$((i + 1))
    if [ "$LIMITE" -gt 0 ] && [ "$i" -gt "$LIMITE" ]; then break; fi
    d="$DEST/$lieu/$nom"
    etape "[$i/$total] $lieu/$nom"
    [ -s "$d/LICENCE.txt" ] && continue
    info="$TMP/info_$uid.json"; dl="$TMP/dl_$uid.json"
    if ! curl -fsS --retry 3 -o "$info" "$API/models/$uid"; then erreur "$lieu/$nom : modele introuvable"; continue; fi
    # La licence est relue a chaque fois : si l'auteur l'a changee, le modele est ignore.
    licence="$(grep -oE '"license"[[:space:]]*:[[:space:]]*\{[^}]*\}' "$info" | grep -oE '"slug"[[:space:]]*:[[:space:]]*"[^"]+"' | head -1 | sed -E 's/.*"([^"]+)"$/\1/')"
    if ! grep -qE '"isDownloadable"[[:space:]]*:[[:space:]]*true' "$info"; then erreur "$lieu/$nom : plus telechargeable"; continue; fi
    case "$licence" in cc0|by|by-sa) ;; *) erreur "$lieu/$nom : licence '$licence' ignoree"; continue ;; esac
    if ! curl -fsS --retry 3 -H "Authorization: Token $TOKEN" -o "$dl" "$API/models/$uid/download"; then erreur "$lieu/$nom : telechargement refuse"; continue; fi
    format=glb
    bloc="$(grep -oE '"glb"[[:space:]]*:[[:space:]]*\{[^}]*\}' "$dl" | head -1)"
    if [ -z "$bloc" ]; then format=gltf; bloc="$(grep -oE '"gltf"[[:space:]]*:[[:space:]]*\{[^}]*\}' "$dl" | head -1)"; fi
    url="$(printf '%s' "$bloc" | grep -oE '"url"[[:space:]]*:[[:space:]]*"[^"]+"' | sed -E 's/.*"([^"]+)"$/\1/' | sed 's/\\u0026/\&/g; s#\\/#/#g')"
    taille="$(printf '%s' "$bloc" | grep -oE '"size"[[:space:]]*:[[:space:]]*[0-9]+' | grep -oE '[0-9]+$')"
    if [ -z "$url" ]; then erreur "$lieu/$nom : aucun format glTF propose"; continue; fi
    if [ -n "$taille" ] && [ "$taille" -gt $((MAX_MO * 1024 * 1024)) ]; then
        erreur "$lieu/$nom : trop lourd ($((taille / 1048576)) Mo), a telecharger a la main : https://sketchfab.com/3d-models/$uid"; continue
    fi
    if [ "$format" = glb ]; then
        telecharger "$url" "$d/$nom.glb" || { erreur "$lieu/$nom : echec du telechargement"; continue; }
    else
        telecharger "$url" "$TMP/$uid.zip" || { erreur "$lieu/$nom : echec du telechargement"; continue; }
        mkdir -p "$d" && unzip -oq "$TMP/$uid.zip" -d "$d" || { erreur "$lieu/$nom : archive illisible"; continue; }
        rm -f "$TMP/$uid.zip"
    fi
    printf '%s\n' "$credit" > "$d/LICENCE.txt"
    sleep 1
done < "$TMP/liste.txt"

{
    echo "Credits des modeles 3D du prologue d'Echos d'Ether (a reprendre dans l'ecran des credits du jeu)."
    echo ""
    find "$DEST" -name LICENCE.txt | sort | while read -r f; do cat "$f"; done
} > "$DEST/CREDITS-PROLOGUE.txt"

echo ""
echo "[OK] Termine : $((OCTETS / 1048576)) Mo telecharges dans $DEST"
if [ -s "$ERREURS" ]; then
    echo "$(wc -l < "$ERREURS" | tr -d ' ') probleme(s) :"
    sed 's/^/  /' "$ERREURS"
    echo "Relance la meme commande pour reessayer (ce qui est deja la est garde)."
fi
echo "Credits a citer : $DEST/CREDITS-PROLOGUE.txt"
