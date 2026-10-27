#!/usr/bin/env bash
# Echos d'Ether : modeles 3D, textures et ciels gratuits pour la vitrine Three.js (Mac ou Linux).
#
# Usage (Terminal du Mac) :
#   curl -fsSL https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-vitrine.sh | bash -s -- ~/jeu/vitrine/models
#
# Avec un token Sketchfab gratuit (sketchfab.com > Settings > Password & API), en plus :
# ver des sables anime, astronaute anime, capsules, statue, asteroides, antenne.
#   curl -fsSL https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-vitrine.sh | SKETCHFAB_TOKEN=xxxx bash -s -- ~/jeu/vitrine/models
#
# Uniquement des contenus libres (CC0, domaine public, CC BY : credits dans CREDITS.txt).
# Formats web : .glb ou .gltf (textures a cote), textures .jpg/.png, ciels .hdr.
# Le fichier modeles.json decrit chaque fichier : lieu de la vitrine, role, taille conseillee, licence.
# Relancer le script ne retelecharge que ce qui manque. Necessite seulement curl et unzip (presents sur macOS).

DEST="${1:-./vitrine/models}"
TOKEN="${SKETCHFAB_TOKEN:-}"
DEPOT="${ECHOS_DEPOT:-https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether}"
DEPOT_LFS="${ECHOS_DEPOT_LFS:-https://media.githubusercontent.com/media/madhiphares-eng/mon-projet/echos-ether}"   # fichiers en Git LFS (.png)
SKETCHFAB_MAX_MO=150

mkdir -p "$DEST" || { echo "Impossible de creer $DEST"; exit 1; }
DEST="$(cd "$DEST" && pwd)"
TMP="$(mktemp -d 2>/dev/null || mktemp -d -t echos)"
trap 'rm -rf "$TMP"' EXIT

MANIFESTE="$TMP/manifeste.txt"
CREDITS="$TMP/credits.txt"
ERREURS="$TMP/erreurs.txt"
: > "$MANIFESTE"; : > "$CREDITS"; : > "$ERREURS"
OCTETS=0

titre() { printf '\n=== %s ===\n' "$1"; }
etape() { printf '  %s\n' "$1"; }
erreur() { printf '    [!] %s\n' "$1"; printf '%s\n' "$1" >> "$ERREURS"; }

# telecharger URL CHEMIN : saute un fichier deja present, reessaie 3 fois
telecharger() {
    local url="$1" cible="$2"
    if [ -s "$cible" ]; then return 0; fi
    mkdir -p "$(dirname "$cible")"
    if curl -fL --retry 3 --retry-delay 3 -sS -o "$cible.part" "$url"; then
        mv "$cible.part" "$cible"
        OCTETS=$((OCTETS + $(wc -c < "$cible")))
        return 0
    fi
    rm -f "$cible.part"
    erreur "echec : $url"
    return 1
}

# noter LIEU ROLE TYPE FICHIER TAILLE_M LICENCE CREDIT : une ligne de modeles.json
noter() {
    printf '    {"lieu": "%s", "role": "%s", "type": "%s", "fichier": "%s", "taille_m": %s, "licence": "%s", "credit": "%s"}' \
        "$1" "$2" "$3" "$4" "$5" "$6" "$7" >> "$MANIFESTE"
    printf ',\n' >> "$MANIFESTE"
}

credit() { grep -qxF "$1" "$CREDITS" 2>/dev/null || printf '%s\n' "$1" >> "$CREDITS"; }

# ------------------------------------------------------------------ sources

# Poly Haven, modele glTF 1k (le .gltf, son .bin et ses textures)
polyhaven_modele() {
    local id="$1" lieu="$2" role="$3" taille="$4"
    etape "Poly Haven : $id"
    local dossier="$DEST/$lieu/polyhaven/$id"
    local json="$TMP/ph_$id.json"
    telecharger "https://dl.polyhaven.org/file/ph-assets/Models/gltf/1k/$id/${id}_1k.gltf" "$dossier/${id}_1k.gltf" || return
    curl -fsS --retry 3 -o "$json" "https://api.polyhaven.com/files/$id" || { erreur "API Poly Haven : $id"; return; }
    # chaque fichier cite par le .gltf (le .bin, les textures) : son adresse reelle est dans la liste "include" de l'API
    grep -oE '"uri"[[:space:]]*:[[:space:]]*"[^"]+"' "$dossier/${id}_1k.gltf" | sed -E 's/.*"([^"]+)"$/\1/' | while read -r uri; do
        case "$uri" in data:*) continue ;; esac
        local cle url
        cle="$(printf '%s' "$uri" | sed 's/[].[*^$\/]/\\&/g')"
        url="$(grep -oE "\"$cle\"[[:space:]]*:[[:space:]]*\{[^}]*\}" "$json" | head -1 | grep -oE '"url"[[:space:]]*:[[:space:]]*"[^"]+"' | sed -E 's/.*"([^"]+)"$/\1/')"
        if [ -z "$url" ]; then erreur "Poly Haven $id : adresse de $uri introuvable"; continue; fi
        telecharger "$url" "$dossier/$uri"
    done
    noter "$lieu" "$role" "modele" "$lieu/polyhaven/$id/${id}_1k.gltf" "$taille" "CC0" "Poly Haven, https://polyhaven.com/a/$id"
    credit "Poly Haven (CC0) : https://polyhaven.com"
}

# Poly Haven, texture de sol ou de roche en 2k (couleur, normale GL, rugosite, ARM, hauteur)
polyhaven_texture() {
    local id="$1" lieu="$2" role="$3"
    etape "Poly Haven : texture $id"
    local json="$TMP/ph_$id.json"
    curl -fsS --retry 3 -o "$json" "https://api.polyhaven.com/files/$id" || { erreur "API Poly Haven : $id"; return; }
    grep -oE "https://dl\.polyhaven\.org/file/ph-assets/Textures/jpg/2k/$id/${id}_(diff|nor_gl|rough|arm|disp)_2k\.jpg" "$json" | sort -u | while read -r url; do
        telecharger "$url" "$DEST/$lieu/textures/$id/$(basename "$url")"
    done
    noter "$lieu" "$role" "texture" "$lieu/textures/$id/${id}_diff_2k.jpg" "null" "CC0" "Poly Haven, https://polyhaven.com/a/$id"
    credit "Poly Haven (CC0) : https://polyhaven.com"
}

# Poly Haven, ciel HDRI 2k (pour l'environnement et l'eclairage)
polyhaven_ciel() {
    local id="$1" lieu="$2" role="$3"
    etape "Poly Haven : ciel $id"
    telecharger "https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/2k/${id}_2k.hdr" "$DEST/$lieu/ciel/${id}_2k.hdr" || return
    noter "$lieu" "$role" "ciel" "$lieu/ciel/${id}_2k.hdr" "null" "CC0" "Poly Haven, https://polyhaven.com/a/$id"
    credit "Poly Haven (CC0) : https://polyhaven.com"
}

# ambientCG, texture 1K (zip)
ambientcg() {
    local id="$1" lieu="$2" role="$3"
    etape "ambientCG : $id"
    local dossier="$DEST/$lieu/textures/$id"
    if [ -n "$(ls "$dossier"/*_Color.jpg 2>/dev/null)" ]; then
        noter "$lieu" "$role" "texture" "$lieu/textures/$id/${id}_1K-JPG_Color.jpg" "null" "CC0" "ambientCG, https://ambientcg.com/view?id=$id"
        credit "ambientCG (CC0) : https://ambientcg.com"
        return
    fi
    local zip="$TMP/$id.zip"
    telecharger "https://ambientcg.com/get?file=${id}_1K-JPG.zip" "$zip" || return
    mkdir -p "$TMP/$id" "$dossier"
    unzip -q -o "$zip" -d "$TMP/$id" || { erreur "zip illisible : $id"; return; }
    find "$TMP/$id" -type f \( -name '*_Color.jpg' -o -name '*_NormalGL.jpg' -o -name '*_Roughness.jpg' -o -name '*_AmbientOcclusion.jpg' -o -name '*_Displacement.jpg' -o -name '*_Emission.jpg' \) -exec cp {} "$dossier/" \;
    noter "$lieu" "$role" "texture" "$lieu/textures/$id/${id}_1K-JPG_Color.jpg" "null" "CC0" "ambientCG, https://ambientcg.com/view?id=$id"
    credit "ambientCG (CC0) : https://ambientcg.com"
}

# Fichier direct : direct URL LIEU SOUS-DOSSIER NOM ROLE TYPE TAILLE LICENCE CREDIT
direct() {
    local url="$1" lieu="$2" sous="$3" nom="$4" role="$5" type="$6" taille="$7" licence="$8" cr="$9"
    etape "$nom"
    telecharger "$url" "$DEST/$lieu/$sous/$nom" || return
    noter "$lieu" "$role" "$type" "$lieu/$sous/$nom" "$taille" "$licence" "$cr"
}

# Kenney, kit 3D : seulement les .glb et leurs textures (repli : copies du depot du jeu)
kenney() {
    local kit="$1" lieu="$2" role="$3" repli="$4"
    etape "Kenney : $kit"
    local dossier="$DEST/$lieu/kenney-$kit"
    local page lien zip="$TMP/$kit.zip"
    mkdir -p "$dossier"
    if [ -z "$(ls "$dossier"/*.glb 2>/dev/null)" ]; then
        page="$(curl -fsSL "https://kenney.nl/assets/$kit" 2>/dev/null)"
        lien="$(printf '%s' "$page" | grep -oE "https://kenney\.nl/media/pages/assets/[^\"' ]+\.zip" | head -1)"
        if [ -n "$lien" ] && curl -fL --retry 2 -sS -o "$zip" "$lien" 2>/dev/null && unzip -q -o "$zip" -d "$TMP/$kit" 2>/dev/null; then
            OCTETS=$((OCTETS + $(wc -c < "$zip")))
            if [ -d "$TMP/$kit/Models/GLB format" ]; then cp -R "$TMP/$kit/Models/GLB format/." "$dossier/"
            else find "$TMP/$kit" -name '*.glb' -exec cp {} "$dossier/" \; ; fi
            [ -f "$TMP/$kit/License.txt" ] && cp "$TMP/$kit/License.txt" "$dossier/LICENCE.txt"
        fi
    fi
    if [ -n "$(ls "$dossier"/*.glb 2>/dev/null)" ]; then
        :
    elif [ -n "$repli" ]; then
        # copies CC0 rangees dans le depot du jeu (Kenney autorise la redistribution)
        etape "  (site Kenney indisponible : copies du depot du jeu)"
        local sous f
        sous="$(printf '%s' "$repli" | cut -d: -f1)"
        for f in $(printf '%s' "$repli" | cut -d: -f2-); do
            case "$f" in
                *.png) telecharger "$DEPOT_LFS/maquette/modeles/$sous$f" "$dossier/$f" ;;
                *) telecharger "$DEPOT/maquette/modeles/$sous$f" "$dossier/$f" ;;
            esac
        done
    else
        erreur "Kenney $kit : telechargement impossible (https://kenney.nl/assets/$kit)"
        return
    fi
    noter "$lieu" "$role" "dossier" "$lieu/kenney-$kit" "null" "CC0" "Kenney, https://kenney.nl/assets/$kit"
    credit "Kenney (CC0) : https://kenney.nl"
}

# Sketchfab (token requis) : licence reverifiee a chaque fois, seulement CC0, CC BY et CC BY-SA
sketchfab() {
    local uid="$1" nom="$2" lieu="$3" role="$4" taille="$5"
    local cible="$DEST/$lieu/sketchfab/$nom.glb"
    if [ -s "$cible" ] && [ -s "$cible.licence" ]; then
        # deja telecharge : credit relu dans le fichier .licence
        local lib0 auteur0
        lib0="$(cut -d'|' -f1 "$cible.licence")"; auteur0="$(cut -d'|' -f2 "$cible.licence")"
        noter "$lieu" "$role" "modele" "$lieu/sketchfab/$nom.glb" "$taille" "$lib0" "$nom par $auteur0, https://sketchfab.com/3d-models/$uid"
        credit "\"$nom\" par $auteur0 (sketchfab.com/$auteur0), $lib0, https://sketchfab.com/3d-models/$uid"
        return
    fi
    [ -z "$TOKEN" ] && return
    etape "Sketchfab : $nom"
    local info="$TMP/sf_$uid.json" dl="$TMP/sf_${uid}_dl.json"
    curl -fsS --retry 3 -o "$info" "https://api.sketchfab.com/v3/models/$uid" || { erreur "Sketchfab : modele $nom introuvable"; return; }
    local licence auteur
    licence="$(grep -oE '"license"[[:space:]]*:[[:space:]]*\{[^}]*\}' "$info" | grep -oE '"slug"[[:space:]]*:[[:space:]]*"[^"]+"' | head -1 | sed -E 's/.*"([^"]+)"$/\1/')"
    auteur="$(grep -oE '"username"[[:space:]]*:[[:space:]]*"[^"]+"' "$info" | head -1 | sed -E 's/.*"([^"]+)"$/\1/')"
    if ! grep -qE '"isDownloadable"[[:space:]]*:[[:space:]]*true' "$info"; then erreur "Sketchfab $nom : plus telechargeable"; return; fi
    case "$licence" in cc0|by|by-sa) ;; *) erreur "Sketchfab $nom : licence '$licence' ignoree"; return ;; esac
    curl -fsS --retry 3 -H "Authorization: Token $TOKEN" -o "$dl" "https://api.sketchfab.com/v3/models/$uid/download" || { erreur "Sketchfab $nom : token refuse ?"; return; }
    local bloc url taille_o
    bloc="$(grep -oE '"glb"[[:space:]]*:[[:space:]]*\{[^}]*\}' "$dl" | head -1)"
    url="$(printf '%s' "$bloc" | grep -oE '"url"[[:space:]]*:[[:space:]]*"[^"]+"' | sed -E 's/.*"([^"]+)"$/\1/' | sed 's/\\u0026/\&/g; s#\\/#/#g')"
    taille_o="$(printf '%s' "$bloc" | grep -oE '"size"[[:space:]]*:[[:space:]]*[0-9]+' | grep -oE '[0-9]+$')"
    if [ -z "$url" ]; then erreur "Sketchfab $nom : pas de version .glb"; return; fi
    if [ -n "$taille_o" ] && [ "$taille_o" -gt $((SKETCHFAB_MAX_MO * 1024 * 1024)) ]; then erreur "Sketchfab $nom : trop lourd ($((taille_o / 1048576)) Mo)"; return; fi
    telecharger "$url" "$cible" || return
    local lib="CC BY 4.0"
    [ "$licence" = "cc0" ] && lib="CC0"
    [ "$licence" = "by-sa" ] && lib="CC BY-SA 4.0"
    printf '%s|%s\n' "$lib" "$auteur" > "$cible.licence"
    noter "$lieu" "$role" "modele" "$lieu/sketchfab/$nom.glb" "$taille" "$lib" "$nom par $auteur, https://sketchfab.com/3d-models/$uid"
    credit "\"$nom\" par $auteur (sketchfab.com/$auteur), $lib, https://sketchfab.com/3d-models/$uid"
    sleep 1
}

NASA="https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model"
NASA_CREDIT="NASA 3D Resources, domaine public (pas de logo NASA, pas de soutien implicite)"
SSS="https://www.solarsystemscope.com/textures/download"
SSS_CREDIT="Solar System Scope, CC BY 4.0, https://www.solarsystemscope.com/textures/"

echo ""
echo "=== Echos d'Ether : modeles 3D pour la vitrine ==="
echo "Dossier : $DEST"
if [ -z "$TOKEN" ]; then echo "Sans token Sketchfab : ver, astronaute anime, capsules et statue Sketchfab ignores (voir l'en-tete du script)."; fi

# ------------------------------------------------------------------ commun a tous les lieux

titre "Commun : Arpenteur, kits, ciel etoile"
direct "$NASA/astronaut/Astronaut.glb" commun nasa astronaute.glb "Arpenteur (statique, compresse Draco : DRACOLoader de Three.js)" modele 1.8 "Domaine public" "$NASA_CREDIT"
direct "$NASA/z2-spacesuit/Z2%20Spacesuit.glb" commun nasa combinaison-z2.glb "Arpenteur, combinaison detaillee (statique)" modele 1.8 "Domaine public" "$NASA_CREDIT"
credit "$NASA_CREDIT : https://science.nasa.gov/3d-resources/"
kenney space-kit commun "Kit spatial : structures, antennes, rovers, vaisseaux (low-poly)" ":astronautB.glb barrels.glb bones.glb craft_cargoA.glb craft_cargoB.glb craft_miner.glb rocket_baseA.glb rover.glb satelliteDish_detailed.glb structure_detailed.glb LICENCE-Kenney.txt"
BLASTERS="blaster-a.glb blaster-b.glb blaster-c.glb blaster-d.glb blaster-e.glb blaster-f.glb blaster-g.glb blaster-h.glb blaster-i.glb blaster-j.glb blaster-k.glb blaster-l.glb blaster-m.glb blaster-n.glb blaster-o.glb blaster-p.glb blaster-q.glb blaster-r.glb grenade-a.glb grenade-b.glb clip-large.glb clip-small.glb scope-large-a.glb scope-large-b.glb scope-small.glb silencer-small.glb crate-medium.glb crate-small.glb crate-wide.glb target-large.glb target-small.glb smoke.glb Textures/colormap.png LICENCE.txt"
kenney blaster-kit commun "Armes : blaster-a pistolet, blaster-d carabine, blaster-l fusil a eclats, blaster-e fusil long, grenade-a (texture commune Textures/colormap.png)" "kenney-blaster-kit/:$BLASTERS"
direct "$SSS/8k_stars_milky_way.jpg" commun ciel etoiles_voie_lactee_8k.jpg "Ciel etoile (sphere equirectangulaire)" ciel null "CC BY 4.0" "$SSS_CREDIT"
credit "$SSS_CREDIT"
sketchfab 8fe5c8d3365e4d87bb7bc253d53a64e1 astronaute-anime commun "Arpenteur anime (marche, idle, salut, derive)" 1.8

# ------------------------------------------------------------------ 1. L'Horizon (Bord de l'Abime)

titre "1. L'Horizon : plateau de basalte, rochers flottants"
polyhaven_texture dark_rock_02 horizon "Sol du plateau de basalte"
polyhaven_texture rock_face horizon "Falaises et bord du plateau"
for r in moon_rock_01 moon_rock_02 moon_rock_03 moon_rock_04 moon_rock_05; do
    polyhaven_modele "$r" horizon "Rocher flottant (marees gravitationnelles)" 2
done
polyhaven_modele coastal_cliff_01 horizon "Bord du plateau, falaise" 8

# ------------------------------------------------------------------ 2. Le Tombeau et le Leviathan (Dunes d'Ocre)

titre "2. Le Tombeau et le Leviathan : dunes, monolithe, ver"
polyhaven_texture aerial_sand dunes "Dunes de sable"
polyhaven_texture sandy_gravel_02 dunes "Crete de gravier"
polyhaven_texture red_sandstone_wall dunes "Monolithe (pierre gravee)"
for r in sand_rocks_small_01 namaqualand_boulder_02 namaqualand_boulder_04; do
    polyhaven_modele "$r" dunes "Rochers des dunes" 2
done
polyhaven_modele namaqualand_cliff_01 dunes "Affleurement rocheux" 6
polyhaven_modele dead_quiver_trunk dunes "Arbre mort" 4
polyhaven_ciel industrial_sunset_puresky dunes "Ciel de crepuscule (deux soleils a ajouter)"
sketchfab 41f03702289940b29831dcb771fb5f3a ver-des-sables dunes "Leviathan : ver des sables anime (Idle, Attack, Spawn, Die)" 30

# ------------------------------------------------------------------ 3. Site de largage (Mondes de Cendre)

titre "3. Site de largage : capsule, cendre, statues"
polyhaven_texture burned_ground_01 largage "Sol de cendre"
ambientcg Lava004 largage "Braises et lave refroidie"
direct "$NASA/apollo-lunar-module/Apollo%20Lunar%20Module.glb" largage nasa module-lunaire.glb "Capsule d'atterrissage (a la place de la capsule)" modele 7 "Domaine public" "$NASA_CREDIT"
direct "$NASA/viking-lander/Viking%20Lander.glb" largage nasa atterrisseur-viking.glb "Sonde posee, epave" modele 3 "Domaine public" "$NASA_CREDIT"
polyhaven_modele gothic_statue largage "Statue geante dans la brume (a agrandir)" 25
polyhaven_ciel the_sky_is_on_fire largage "Ciel rougeoyant (naine rouge)"
sketchfab 9a55af34f7654097bd72f0642d83ccce capsule-habitat largage "Capsule de largage" 5
sketchfab 709a1916c0cc4cbebe197e7ef26964cb navette-largage largage "Navette de largage" 14
sketchfab e21e8e13aab64a17b92c6ec4daca7d39 statue-fragment largage "Statue colossale (fragment)" 20

# ------------------------------------------------------------------ 4. Champ d'anneaux (Couronne Brisee)

titre "4. Champ d'anneaux : geante gazeuse, asteroides, avant-poste"
direct "$SSS/8k_jupiter.jpg" anneaux planetes geante_gazeuse_8k.jpg "Geante gazeuse (texture de Jupiter)" texture null "CC BY 4.0" "$SSS_CREDIT"
direct "$SSS/8k_saturn.jpg" anneaux planetes geante_annelee_8k.jpg "Variante de geante gazeuse (Saturne)" texture null "CC BY 4.0" "$SSS_CREDIT"
direct "$SSS/8k_saturn_ring_alpha.png" anneaux planetes anneaux_alpha_8k.png "Anneaux (couleur et transparence)" texture null "CC BY 4.0" "$SSS_CREDIT"
polyhaven_modele moon_rock_06 anneaux "Asteroide" 3
polyhaven_modele moon_rock_07 anneaux "Asteroide" 3
direct "$NASA/habitat-demonstration-unit/Habitat%20Demonstration%20Unit%20(part%201).glb" anneaux nasa habitat.glb "Avant-poste (module d'habitat)" modele 6 "Domaine public" "$NASA_CREDIT"
direct "$NASA/deep-space-network-70-meter/Deep%20Space%20Network%2070-meter.glb" anneaux nasa antenne-parabolique.glb "Antenne de l'avant-poste (a reduire)" modele 20 "Domaine public" "$NASA_CREDIT"
sketchfab adde1ecf129e4509be8af61b84bafa85 asteroides anneaux "Pack d'asteroides" 5
sketchfab 001970c2594e4aa6bef46e63fc9e0c5a antenne-industrielle anneaux "Antenne industrielle" 10

# ------------------------------------------------------------------ fichiers de description

{
    echo "{"
    echo "  \"genere\": \"$(date +%Y-%m-%d)\","
    echo "  \"note\": \"Chemins relatifs a ce dossier. taille_m : hauteur ou taille conseillee a l'ecran, en metres. Les .gltf ont leurs .bin et textures a cote. astronaute.glb est compresse Draco (DRACOLoader).\","
    echo "  \"fichiers\": ["
    sed '$ s/,$//' "$MANIFESTE"
    echo "  ]"
    echo "}"
} > "$DEST/modeles.json"
{
    echo "Credits des assets de la vitrine d'Echos d'Ether"
    echo "Genere le $(date +%Y-%m-%d). Les CC BY doivent etre cites dans le jeu ou ses credits."
    echo ""
    cat "$CREDITS"
} > "$DEST/CREDITS.txt"

echo ""
echo "[OK] Termine : $(du -sh "$DEST" | cut -f1 | tr -d ' ') dans $DEST"
echo "     Liste des fichiers : $DEST/modeles.json  ·  Credits : $DEST/CREDITS.txt"
if [ -s "$ERREURS" ]; then
    echo "[!] $(wc -l < "$ERREURS" | tr -d ' ') probleme(s) :"
    sed 's/^/    /' "$ERREURS"
fi
