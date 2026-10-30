# Echos d'Ether - telecharge les assets gratuits de docs/ASSETS.md dans le projet Unity.
# Usage (PowerShell) :
#   irm https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/outils/telecharger-assets.ps1 | iex
#
# Le script demande quels groupes telecharger (Entree = tout). Fermer Unity avant de lancer.
# Destination : C:\Projets\EchosEther\Assets\_Projet\Externes
# Images de reference (Hubble, trous noirs...) : C:\Projets\EchosEther\References (hors Unity)
# Les fichiers deja presents ne sont pas retelecharges : on peut relancer sans risque, par exemple apres une coupure.
#
# Sketchfab demande un token API gratuit : creer un compte, puis https://sketchfab.com/settings/password (section "API Token").
# Variables facultatives : $env:SKETCHFAB_TOKEN, $env:ECHOS_CHOIX (ex. "1,2,5"), $env:ECHOS_PROJET.

& {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'   # sinon Invoke-WebRequest est tres lent
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $Projet = if ($env:ECHOS_PROJET) { $env:ECHOS_PROJET } else { 'C:\Projets\EchosEther' }
    $Ext = Join-Path $Projet 'Assets\_Projet\Externes'
    $Tmp = Join-Path ([IO.Path]::GetTempPath()) 'EchosEther-assets'
    $Res = '2k'                 # textures Poly Haven et ambientCG
    $MaxSketchfabMo = 300       # au-dela, le modele est note dans la liste "a la main"
    $Limite = if ($env:ECHOS_LIMITE) { [int]$env:ECHOS_LIMITE } else { 0 }   # pour les tests : n elements par groupe

    if (-not (Test-Path (Join-Path $Projet 'Assets'))) {
        Write-Host "Projet Unity introuvable : $Projet" -ForegroundColor Red
        return
    }

    # ---------------------------------------------------------------- listes (generees depuis docs/ASSETS.md)

    $GltfastVersion = '6.20.0'

    $Groupes = @(
        @('1', 'Poly Haven   : 98 rochers, falaises, textures et ciels HDRI (CC0)  ~2,4 Go'),
        @('2', 'ambientCG    : 43 textures de sol, roche, metal + 5 ciels (CC0)  ~1,2 Go'),
        @('3', 'Sketchfab    : 172 modeles (persos, creatures, vaisseaux, ruines)  ~5 a 10 Go, token gratuit requis'),
        @('4', 'NASA         : 16 modeles 3D reels (combinaisons, rovers, ISS)  ~0,5 Go'),
        @('5', 'Espace       : 34 cartes d''etoiles, planetes, Lune, images de reference  ~0,6 Go'),
        @('6', 'Kenney       : bruitages sci-fi, impacts, interface, jingles, armes Blaster Kit (CC0)  ~12 Mo')
    )

    # Poly Haven (CC0)
    $PolyHavenModeles = @(
        'moon_rock_01', 'coastal_cliff_01', 'coastal_cliff_04', 'coast_rocks_05',
        'coast_line_01', 'rock_face_01', 'mountainside', 'sand_rocks_small_01',
        'modular_industrial_pipes_01', 'modular_factory_facade', 'overhead_crane', 'modular_airduct_rectangular_01',
        'Barrel_01', 'portable_generator', 'modular_electric_cables', 'industrial_caged_sconce',
        'worn_metal_rack', 'old_military_compressor', 'vintage_spacecraft_instrument', 'gothic_statue',
        'large_castle_door', 'modular_fort_01', 'moon_rock_02', 'moon_rock_03',
        'moon_rock_04', 'moon_rock_05', 'moon_rock_06', 'moon_rock_07',
        'coastal_cliff_02', 'namaqualand_boulder_02', 'namaqualand_boulder_03', 'namaqualand_boulder_04',
        'namaqualand_boulder_05', 'namaqualand_boulder_06', 'namaqualand_boulders_01', 'namaqualand_cliff_01',
        'namaqualand_cliff_02', 'namaqualand_rocks_01', 'namaqualand_stones_01', 'dead_quiver_trunk',
        'dead_quiver_branch_01'
    )

    $PolyHavenTextures = @(
        'moon_dusted_03', 'moon_meteor_01', 'damp_beach_sand_02', 'damp_sand',
        'dark_rock_02', 'seaside_rock', 'worn_rock_natural_01', 'rock_boulder_cracked',
        'mud_cracked_dry_03', 'burned_ground_01', 'snow_02', 'blue_metal_plate',
        'green_metal_rust', 'metal_plate_02', 'container_side', 'rusty_painted_metal',
        'rabdentse_ruins_wall', 'medieval_blocks_06', 'large_sandstone_blocks_01', 'rebar_reinforced_concrete',
        'rusty_corrugated_iron', 'stone_block_wall', 'red_sandstone_wall', 'concrete_wall_008',
        'hangar_concrete_floor', 'metal_plate', 'metal_grate_rusty', 'concrete_debris',
        'moon_dusted_01', 'moon_dusted_02', 'moon_dusted_04', 'moon_dusted_05',
        'moon_track_01', 'moon_track_02', 'moon_track_03', 'moon_track_04',
        'moon_footprints_01', 'moon_footprints_02', 'aerial_sand', 'gravelly_sand',
        'sandy_gravel_02', 'cracked_red_ground', 'cliff_side', 'rock_face',
        'tiger_rock'
    )

    $PolyHavenHdri = @(
        'satara_night_no_lamps', 'kloppenheim_02_puresky', 'qwantani_night_puresky',
        'dikhololo_night', 'qwantani_moon_noon_puresky', 'belfast_sunset_puresky',
        'the_sky_is_on_fire', 'industrial_sunset_puresky', 'wasteland_clouds_puresky',
        'dry_cracked_lake', 'quarry_cloudy', 'kloofendal_misty_morning_puresky'
    )

    # ambientCG (CC0)
    $AmbientCgTextures = @(
        'Ground093C', 'Ground096A', 'Rock029', 'Rock061', 'Rock035', 'Rock041',
        'Rock062', 'Lava004', 'Ground061', 'Ground060', 'Rock020', 'Gravel024',
        'Rocks015', 'Ice003', 'Snow008A', 'Ground111', 'Onyx006', 'MetalPlates017A',
        'MetalPlates015A', 'MetalPlates006', 'MetalPlates013', 'PaintedMetal017', 'PaintedMetal009', 'DiamondPlate008C',
        'CorrugatedSteel007A', 'Metal046B', 'Metal053C', 'SheetMetal001', 'SolarPanel003', 'Concrete042C',
        'MetalWalkway014', 'PaintedMetal016', 'Rust004', 'PavingStones054', 'PavingStones149', 'Rocks012',
        'Rocks013', 'Rocks014', 'Rocks017', 'Rocks018', 'Rocks019', 'Rocks020',
        'Rocks021'
    )

    $AmbientCgHdri = @(
        'NightSkyHDRI008', 'NightSkyHDRI009', 'NightSkyHDRI002', 'EveningSkyHDRI030A', 'DaySkyHDRI071A'
    )

    # Sketchfab : 'categorie|uid|dossier' (licence reverifiee au telechargement)
    $Sketchfab = @(
        'Terrain|561a1628d0b446c7bd3424da48fe1150|desert-cliffs-terrain-scan',
        'Terrain|f0e424595ce14df9b82c35b658beb882|lidar-rock-scan-ramon-crater-negev-israe',
        'Terrain|d1f2ce6f71aa4c19adbdc541cc888194|cliff-rock-boulder-field',
        'Terrain|5206d4d96c2d428b9c1f7ee0e13bcffb|rock-pile-scan-dry-1',
        'Terrain|b45a40fd145c4b9ba7bc5351d344db17|rocks-set-stone-collection-scan',
        'Terrain|68d53a1623aa47008fb7088a5694d79b|coastal-ground-cliff-rocks',
        'Terrain|416a548a159f42e89a475d0ac17ac6d9|rocks-marine-corsica',
        'Terrain|e4bf57abe35f4df3b6f4c0f32507091f|sea-cliff-rock-face',
        'Terrain|f9c05a8914a04b6aa3417d4a8db17c85|rock-salines-2',
        'Terrain|17b6a81191f045a1af2d9bb57cad428b|wall-of-ice',
        'Terrain|a9f3229f5e57496d81c004a606a43728|obsidian',
        'Terrain|4f4a474678f845f79cb0db63f08306fb|halite-crystals',
        'Terrain|9fe0833c2b3a4dd3b26844c1dd09f441|apollo-lunar-sample-701750',
        'Terrain|6bd9a0d96ef4463a8efd45af2be595ac|sand-dunes',
        'Terrain|1c5f772e12fc44ff995776e38e1953f2|sand-cliffs',
        'Terrain|4118802ad14243d59f34c2be3fa77cd4|stranska-rock-1',
        'Terrain|7f86615ce94e4ac09d31f191e746534a|galet-eolise-ventifact',
        'Terrain|adde1ecf129e4509be8af61b84bafa85|asteroids-pack-rocky-version',
        'Terrain|39e29fec9cf040dd925b804f5040ca04|alaskan-cliff-rock-9-free',
        'Personnages|da7b5c14bc634f8283e2d21343afa7cc|spacesuit-confederation-of-planets',
        'Personnages|7fd399d488ae4576a0fc62715892956c|mercenary-astronaut',
        'Personnages|8fe5c8d3365e4d87bb7bc253d53a64e1|animated-astronaut-character-in-space-su',
        'Personnages|98282a36702941d3941941def844bd68|space-engineer-f-rigged',
        'Personnages|c0efb57e2fa940179ebaa58dcb9d0fa7|rigged-for-ue4ue5-exoskeleton-free',
        'Personnages|722b12227c634489a3436a18a07064d4|mech-pilot-un',
        'Personnages|bdc42075279746dab4c8b54784276a20|space-suit',
        'Personnages|095694921e914ae889e91767ed680e4b|hazmat-suit',
        'Personnages|626da9f4336b4abc9f91ce71a7a1af42|astronaut',
        'Personnages|143a2b1ea5eb4385ae90a73657aca3bc|nathan-animated-003-walking-3d-man',
        'Personnages|a46bc9f67aaa415bb4f3241eef900e7f|eric-rigged-001-rigged-3d-business-man',
        'Personnages|acf520f450d14dd799f98a6fede3edf5|carla-rigged-001-rigged-3d-business-wome',
        'Personnages|cc7e4596bcd145208a6992c757854c07|rigged-t-pose-human-male-w-50-face-blend',
        'Personnages|41f03702289940b29831dcb771fb5f3a|gold-sandworm',
        'Personnages|b04b636a35e14b38bfb05bd21931549d|game-ready-queen-ant-model',
        'Personnages|01323e4b2563430f9da85cd255b6e176|insectoid-monster-rig',
        'Personnages|af87017501fc44e39a33c220f2435100|animated-spider',
        'Personnages|d07ec0641ee74ece96a992db64ecff5f|arahnid-cockroach',
        'Personnages|4c0fd62d33ab4d2592ee0a1d985a480d|scorpion',
        'Personnages|10bb4cf49d304d64afd2b829666f6caf|spiderthing-take-3',
        'Personnages|ef71c4f221304a7ea9f01b691867f3d0|spider-monster-rigged',
        'Personnages|e18dbcb6300b4421aac2bb51cc060aed|phenebub-free-horror-creature',
        'Personnages|7a2b94b6adb242bbb12f116d40d3bcbe|sand-death-worm',
        'Personnages|3979c291d1f9454c90851efe291eab60|cc0-giant-isopod-b-doederleinii',
        'Personnages|8f8d1c0305d04637b7abaf4cb54d25a8|cc0-chinese-red-headed-centipede',
        'Personnages|f9c1f7e3f373491fbf01adf94ec8169e|cc0-japanese-giant-hornet',
        'Personnages|d7d0e0204d4a4a5cb428f021d9429487|blaps-mortisaga',
        'Personnages|46469c568b3743ec8fb4bb711949d49a|giant-isopod',
        'Personnages|03f611a973144635b4a9019afe7393f0|trilobite-e-crassituberculata-pri-49811',
        'Personnages|0e666090f6fd45b280730db98f287e65|molting-husk-insectile-horror-creature',
        'Personnages|7bf94fbb7e4343c99923c91897beea40|sandworm-horror-creature',
        'Personnages|27ba717c173a40b7841d2f2c6a89d823|medium-mech-striker',
        'Personnages|8d06874aac5246c59edb4adbe3606e0e|mech-drone',
        'Personnages|801d2a245e4a4405a0c2152b35b5e486|biped-robot',
        'Personnages|6eba9b1addb7411a85b0c86e41a39a2e|mech-m-6k',
        'Personnages|2694c68765aa4f67ab81a259ff39d654|mech-tl-22',
        'Personnages|2388257bc61049bf841ea4094af7faa7|w9231-combat-mech-rigged',
        'Personnages|6355e3510a544ce4bb219b0b5832933c|ar-22-armadillo-heavy-battle-mech',
        'Personnages|55fe65bbb4074979ac0cb44850df4ddc|combat-robot',
        'Personnages|9e95bc80bfa643d8b890cfa4f83e8b3d|robot-manul',
        'Personnages|36bc284bf5b84a6597db8f6db053c095|robot-police-unit-animated',
        'Personnages|d1ca5065b5184646bc7028597d0ce049|light-assault-mech',
        'Personnages|294e79652f494130ad2ab00a13fdbafd|buster-drone',
        'Personnages|dd21953e81a24bd385d16eaf0c6a8ae7|scifi-drone-11',
        'Personnages|2153909c16b242c293b5c296ce142799|xrk-m-autonomous-combat-drone',
        'Personnages|ac4d06d3a46d4d9cbc5beb33b896cde0|sci-fi-drone',
        'Personnages|561c10fbfea847c09bbac6910a90f80a|attack-drone-scifi',
        'Personnages|148d8e0ca26c4c5d81375496f4266685|futuristic-surveillance-drone',
        'Personnages|098fe7f9592248a0be5b0d6d56cab056|mechanical-scorpion',
        'Personnages|91b15801303b49af8a07f8b34218add4|mechanical-wasp-low-poly',
        'Personnages|12ad786ecde246e5854a61fd4f67ed49|sci-fi-humanoid-robot',
        'Personnages|0f9c08f0d8d94f3a9938d4d8bf423f46|destructible-robot-highpoly',
        'Personnages|a439a90d35274f94888f46a138f30615|t200-dieselpunk-mech-freebie',
        'Personnages|4f16d9ace2904aafb51757d72c017308|dark-faceless-cosmic-phantom',
        'Personnages|3b4ee6c3131a431db6a67021c6d863c8|wraith-ghost-warrior-in-torn-ancient-arm',
        'Personnages|f4fc1cd448c04a809d106975cfa7011b|cloth-ghost',
        'Personnages|8fcc783af94246a0b8febf424a4b96b9|tentacle-rigged',
        'Personnages|d5b4e79acd684d52a1f3303edf22039b|ghostly-reaper',
        'Vaisseaux|d2f3b528edec4e2e8b938019d68632d0|mmsev-space-exploration-vehicle-sev',
        'Vaisseaux|008a288caa504703a2d853fb56a2569f|mars2020-parachute-and-backshell-crashsi',
        'Vaisseaux|956d09cb2e53483f90c24c72ab69bb0d|ancient-colony-ship',
        'Vaisseaux|f2696bbc764e45a3a22d18e4150635a4|crashed-spaceship',
        'Vaisseaux|0b169f4ebcac4eb287002f129d55bfff|drop-ship-or-light-attack-mars-defense-f',
        'Vaisseaux|fe581b60c8ea4ea591a24a483f25ddad|transporter-spaceship',
        'Vaisseaux|709a1916c0cc4cbebe197e7ef26964cb|sci-fi-dropship',
        'Vaisseaux|474f62d00ed54212b37f93ce91569c53|dss-harbinger-battle-cruiser',
        'Vaisseaux|de824e4d997640c09bc4ccbce3064fbb|12-oryx-very-heavy-freighter-textured',
        'Vaisseaux|7614d85856f6477ba2a603970dcbb0dd|corinth-cargo-transporter',
        'Vaisseaux|bbfac99760fb49ceb1c35cdb4ed5f883|mining-company-space-ships',
        'Vaisseaux|19daeb543508430c8807edbcf27a9741|hammerhead-space-fighter',
        'Vaisseaux|e766136d4871441289d37d44a4bbcd3b|space-fighter',
        'Vaisseaux|db4aa67fe1164ec088083d7dac2d273f|spacefighter-cockpit-wasp-interdictor',
        'Vaisseaux|b158f01dd4a9416fb689ca4401856e7a|sci-fi-personal-space-pod-shipweekly-cha',
        'Vaisseaux|5f7176a2ae284d6b99fc970c9d265c04|orbiter-space-shuttle-ov-103-discovery',
        'Vaisseaux|372bb6781922471cada4e0a9bd5c61fb|apollo-11-command-module-combined',
        'Vaisseaux|3f5a5f02ad53447b9dff335e42d2469c|lunokhod-1',
        'Vaisseaux|89ea8e69b87942a881f9b89fd04ec00f|h500-explorer-truck',
        'Vaisseaux|b2503f437acd48ce8cbd05ce7d9a8c2c|lunar-rover',
        'Vaisseaux|085bd97876a64eacbc041299ac945e9c|sci-fi-buggy',
        'Vaisseaux|91f495435f624d8b97a768f692aa6ce9|military-landvehicle-kit-12-cc0',
        'Vaisseaux|5c1f4c9726754652946098616b7614a1|hab-mars-desert-research-station',
        'Vaisseaux|9a55af34f7654097bd72f0642d83ccce|habitat-lander-pod',
        'Vaisseaux|e3ba39a1c78540448542cf937b11feab|space-station-modules',
        'Vaisseaux|8f10be0e8b5644e1845a2a5f12af5d6c|sci-fi-hanger',
        'Vaisseaux|aaa5259e54e14c79936df3d127644287|sci-fi-spaceship-bridge',
        'Vaisseaux|1cdd1db557b8428892af4773bdada913|spaceship-corridor',
        'Vaisseaux|f3d033a8f3ad4888acc8c0a16878cf85|sci-fi-corridor',
        'Vaisseaux|001970c2594e4aa6bef46e63fc9e0c5a|industrial-antenna',
        'Vaisseaux|4be56951da39429a963ee12140bbff57|satellite-comms',
        'Vaisseaux|70a1996b614e4c7ba8f01d4295b8fff4|radio-communications-tower',
        'Vaisseaux|86303d7c8cb442c2bcd138abe2b01c9f|rusted-antennas',
        'Vaisseaux|f01a91e9773e4622b25c4d27903e85c6|old-scifi-satellite-dish',
        'Vaisseaux|30ed76a6b4e44cf4b429a6d764e97054|hdr-o-radar-cat-19',
        'Vaisseaux|96fb7ded0eb44cd18dd5c1e7bb3eb74f|sci-fi-container',
        'Vaisseaux|8a1c32f395f549b8b22265b8a459fa8e|military-weapons-container',
        'Vaisseaux|ac36898521304e51a8e305aa602f1f5b|sci-fi-cargo-crate',
        'Vaisseaux|015f32ec950744c4898e17af07d43f65|futuristic-cargo-container-low-poly',
        'Vaisseaux|3d13bb3bb7c149cda5bc0a14abedd120|sci-fi-fuel-tank',
        'Vaisseaux|bf2e1a087adb4628a376ffbed17c41ff|scifi-generator',
        'Vaisseaux|83483a66f8974d7e8f2e7bbde518d606|solar-panel',
        'Architecture|d80b52dc800e4c76b6b27749bbd8e782|rg-ross-sons-steam-hammer',
        'Architecture|ff3cdef5f8e24605b56648918e1f9a8a|thermal-power-plant-cooling-tower',
        'Architecture|6fa85b289def499c95afd232173ecc27|abandoned-building-polygraphenwerk-leipz',
        'Architecture|bd3ac97071104b64bc388a155b64f79e|large-industrial-storage-tanks',
        'Architecture|11ae7599f3b640569f7ed4692acabf38|bunker-o323a-160-z',
        'Architecture|11b6ad900ab7423ea9d3bd7790eedcea|apollo-11-command-module-interior',
        'Architecture|9480c0cb937647afbf0a5778ac6febc4|daithis-pillar-stone',
        'Architecture|7da7219ac3304850b2dab4133b9e183b|piedras-de-callanish-callanish-stones',
        'Architecture|a974316de2454609aa841790844e3571|temple-of-augustus-ruins',
        'Architecture|85c68a9125f946af8f5102bb426e2f86|st-pancras-new-church-crypt',
        'Architecture|f92ca6a968634540aab7960012b6b059|pyramidion-van-ptahemwia',
        'Architecture|ffe4b699e75849d59d8efe47d3538f03|egyptian-false-door-about-2400-bce',
        'Architecture|e21e8e13aab64a17b92c6ec4daca7d39|statue-fragment-of-amenhotep-iii',
        'Architecture|94c5011772a84e8791779b342467f245|industrial-asset-pack-free',
        'Architecture|fbe3129ea77243ca92c8219f269ac5cf|old-pipes-bundle-modular-industrial-pack',
        'Architecture|884ed11e38054aa48a3db1d9025c881d|boiler-from-the-puffer-vic-32',
        'Architecture|4cb50841dd6e42938b3bc098bff92423|horizontal-and-vertical-planing-machine',
        'Architecture|cfc37d0aff454f29a27c8ab3325f445b|rusted-industrial-water-tank',
        'Architecture|e1aa6b17ff2a4f7d9b7be67756e12848|rusted-factory-machine',
        'Architecture|c1f03864ec72449c8968f0630c287c8d|realistic-metal-scaffolding',
        'Architecture|0d416a9bcfc14978aa50e80281dfe9a5|container-pack',
        'Architecture|a6cccf5af6514f2f9289beeead38fe1b|construction-site-asset-pack',
        'Architecture|b3801b97dcda485c9148f361eeec0c60|abandoned-old-soviet-factory-facade-free',
        'Architecture|56210a9c376a41d882508bd15259eca6|unfinished-building-remains-ruins',
        'Architecture|893e64e62f414b439672a8770dd2869d|dirty-stones-pile',
        'Architecture|8e135c1a917c476a98f011a48c4192c7|qe2-starboard-high-pressure-turbine-patt',
        'Architecture|50e2af8800cc4ab79add375f817b2d76|sci-fi-ship-interior-modular-asset-pack',
        'Architecture|1ac694729ad04977ba93dafa66914aff|space-colony-modular-kit-bash',
        'Architecture|8fe765ad88bf4da4a32e3656dfbe9133|spaceship-cockpit-seat',
        'Architecture|58729c7672ae4e01a6103617c2f4f55e|free-airlock-door',
        'Architecture|44fd25310d2240908027390a078368e1|sci-fi-hallway-free',
        'Architecture|0c2692e46cab4c50907578e296cfee99|ke046-022002-keel-ogham-stone',
        'Architecture|d95a850cd9114828a18b5dd72878d9ec|monumental-runic-stone-optimised-20k',
        'Architecture|78130ac3aeac405aba947b712bea02ad|tumulo-de-d-mecia-de-moura',
        'Architecture|507935f6790e4c278c0cd723d5dd938c|cela-ruins-of-a-nuns-cell',
        'Architecture|75257838d11341a4962a09567176abf6|romanesque-doorway-inchagoilll-ga040-013',
        'Architecture|2983d92ac4e744f485492580ca7629f2|venus-de-milo-statuestexturingchallenge',
        'Architecture|4dad3b80562842f88d6568c5e1c469c2|ancient-ruins-pack',
        'Architecture|a1a8e633c9aa40c08b4d2a0631ac61d2|stone-arch-pillars-ruins',
        'Architecture|37936ba6a1674f15be60075315469a43|abbey-wall-ruins',
        'Architecture|d623bddcf6ba4fd4a4f6cb919160b6f0|pillar-on-the-agora-of-antic-city-of-sag',
        'Architecture|edb597f00e3e4a84855210219502d29a|kalamos-cave',
        'Architecture|9c1e3f0c3b0a49c8bfb032c6a8bf0014|penmaenmawr-cairn',
        'Architecture|d1e25cdbe51e467a87baf94ead0afa63|temple-ruins-asset-pack',
        'Ciel|e410da98b1e5445eae2acafaaa53587d|black-hole',
        'Ciel|cfd16738ad2c402b9dc8e38a9c05c8d4|black-hole-2',
        'Ciel|ce9c009b517b421eab8c8429b536382f|moon-petavius-crater',
        'Ciel|53679864b41a43b0ae0459ce99d6adf0|mars-olympus-mons',
        'Ciel|83f6f32b0a694f939cd16bf04f5565ef|mars-landscape',
        'Ciel|b57711d6a450410ca612c4a36f08ce21|milky-way-skybox-hdri-panorama',
        'Ciel|f7a5798b5b374576879973272d27300e|space-nebula-hdri-panorama-360-skydome',
        'Ciel|6fdfb3c84fb348188d6968092be1b1b5|ringed-world',
        'Ciel|eff495d9315c47dbb2777ec80bef40d8|asteroids-pack-metallic-version'
    )

    # NASA 3D Resources : 'dossier|fichier|url' (un .zip est decompresse dans un dossier du meme nom)
    $Nasa = @(
        'Assets\_Projet\Externes\NASA\Modeles|Combinaison-Z2.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/z2-spacesuit/Z2%20Spacesuit.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Combinaison-EMU.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/extravehicular-mobility-unit/Extravehicular%20Mobility%20Unit.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Combinaison-Mark-III.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/mark-iii-spacesuit/Mark%20III%20Spacesuit.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Astronaute.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/astronaut/Astronaut.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Robonaut-2.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/robonaut-2/Robonaut%202.glb',
        'Assets\_Projet\Externes\NASA\Modeles|RASSOR-excavateur.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/regolith-advanced-surface-systems-operations-robot-(rassor)/Regolith%20Advanced%20Surface%20Systems%20Operations%20Robot%20(RASSOR).glb',
        'Assets\_Projet\Externes\NASA\Modeles|Rover-SEV.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/space-exploration-vehicle/Space%20Exploration%20Vehicle.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Rover-Perseverance.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/mars-2020-perseverance-rover/Mars%202020%20Perseverance%20Rover.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Viking-Lander.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/viking-lander/Viking%20Lander.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Apollo-Lunar-Module.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/apollo-lunar-module/Apollo%20Lunar%20Module.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Habitat-HDU-partie1.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/habitat-demonstration-unit/Habitat%20Demonstration%20Unit%20(part%201).glb',
        'Assets\_Projet\Externes\NASA\Modeles|Habitat-HDU-partie2.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/habitat-demonstration-unit/Habitat%20Demonstration%20Unit%20(part%202).glb',
        'Assets\_Projet\Externes\NASA\Modeles|Station-Gateway.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/gateway/Gateway%20Core.glb',
        'Assets\_Projet\Externes\NASA\Modeles|Antenne-DSN-70m.glb|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/deep-space-network-70-meter/Deep%20Space%20Network%2070-meter.glb',
        'Assets\_Projet\Externes\NASA\Modeles|ISS-exterieur.zip|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/international-space-station-(iss)-(c)-(high-res)/International%20Space%20Station%20(ISS)%20(C)%20(High%20Res).zip',
        'Assets\_Projet\Externes\NASA\Modeles|ISS-interieur.zip|https://assets.science.nasa.gov/content/dam/science/cds/3d/resources/model/international-space-station-(iss)-(e)-(internal)/International%20Space%20Station%20(ISS)%20(E)%20(internal).zip'
    )

    # Espace : 'dossier|fichier|url'
    $Espace = @(
        'Assets\_Projet\Externes\Ciel\Etoiles|starmap_2020_8k.exr|https://svs.gsfc.nasa.gov/vis/a000000/a004800/a004851/starmap_2020_8k.exr',
        'Assets\_Projet\Externes\Ciel\Etoiles|starmap_random_2020_8k.exr|https://svs.gsfc.nasa.gov/vis/a000000/a004800/a004856/starmap_random_2020_8k.exr',
        'Assets\_Projet\Externes\Ciel\Etoiles|TychoSkymapII_8k.jpg|https://svs.gsfc.nasa.gov/vis/a000000/a003500/a003572/TychoSkymapII.t5_08192x04096.jpg',
        'Assets\_Projet\Externes\Ciel\Etoiles|eso0932a_voie_lactee_360.jpg|https://cdn.eso.org/images/large/eso0932a.jpg',
        'Assets\_Projet\Externes\Ciel\Etoiles|8k_stars_milky_way.jpg|https://www.solarsystemscope.com/textures/download/8k_stars_milky_way.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_mars.jpg|https://www.solarsystemscope.com/textures/download/8k_mars.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_moon.jpg|https://www.solarsystemscope.com/textures/download/8k_moon.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_mercury.jpg|https://www.solarsystemscope.com/textures/download/8k_mercury.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_venus_surface.jpg|https://www.solarsystemscope.com/textures/download/8k_venus_surface.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|4k_venus_atmosphere.jpg|https://www.solarsystemscope.com/textures/download/4k_venus_atmosphere.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_jupiter.jpg|https://www.solarsystemscope.com/textures/download/8k_jupiter.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_saturn.jpg|https://www.solarsystemscope.com/textures/download/8k_saturn.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_saturn_ring_alpha.png|https://www.solarsystemscope.com/textures/download/8k_saturn_ring_alpha.png',
        'Assets\_Projet\Externes\Ciel\Planetes|2k_uranus.jpg|https://www.solarsystemscope.com/textures/download/2k_uranus.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|2k_neptune.jpg|https://www.solarsystemscope.com/textures/download/2k_neptune.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|8k_sun.jpg|https://www.solarsystemscope.com/textures/download/8k_sun.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|4k_ceres_fictional.jpg|https://www.solarsystemscope.com/textures/download/4k_ceres_fictional.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|4k_eris_fictional.jpg|https://www.solarsystemscope.com/textures/download/4k_eris_fictional.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|4k_haumea_fictional.jpg|https://www.solarsystemscope.com/textures/download/4k_haumea_fictional.jpg',
        'Assets\_Projet\Externes\Ciel\Planetes|4k_makemake_fictional.jpg|https://www.solarsystemscope.com/textures/download/4k_makemake_fictional.jpg',
        'Assets\_Projet\Externes\Ciel\Lune|lroc_color_poles_4k.tif|https://svs.gsfc.nasa.gov/vis/a000000/a004700/a004720/lroc_color_poles_4k.tif',
        'Assets\_Projet\Externes\Ciel\Lune|ldem_16_uint.tif|https://svs.gsfc.nasa.gov/vis/a000000/a004700/a004720/ldem_16_uint.tif',
        'Assets\_Projet\Externes\Ciel\Donnees|hyg_v44.csv.gz|https://codeberg.org/astronexus/hyg/media/branch/main/data/hyg/CURRENT/hyg_v44.csv.gz',
        'References\Trous noirs|nasa_disque_accretion.jpg|https://svs.gsfc.nasa.gov/vis/a010000/a013300/a013326/BH_labeled.jpg',
        'References\Trous noirs|nasa_survol_360.jpg|https://svs.gsfc.nasa.gov/vis/a010000/a014500/a014576/14576_BHFlyBy_360_Still.jpg',
        'References\Trous noirs|nasa_lentille_binaire_8k.png|https://svs.gsfc.nasa.gov/vis/a010000/a013800/a013831/8k_Lensing_8k.png',
        'References\Trous noirs|eht_m87.jpg|https://cdn.eso.org/images/large/eso1907a.jpg',
        'References\Trous noirs|eht_sagittarius_a.jpg|https://cdn.eso.org/images/large/eso2208-eht-mwa.jpg',
        'References\Nebuleuses|hubble_piliers_creation.jpg|https://cdn.esahubble.org/archives/images/large/heic1501a.jpg',
        'References\Nebuleuses|hubble_carene.jpg|https://cdn.esahubble.org/archives/images/large/heic0707a.jpg',
        'References\Nebuleuses|hubble_champ_ultra_profond.jpg|https://cdn.esahubble.org/archives/images/large/heic0406a.jpg',
        'References\Nebuleuses|webb_falaises_cosmiques.jpg|https://cdn.esawebb.org/archives/images/large/weic2205a.jpg',
        'References\Nebuleuses|vista_centre_voie_lactee.jpg|https://cdn.eso.org/images/publicationjpg/eso1242a.jpg',
        'References\Nebuleuses|gaia_ciel_en_couleur.jpg|https://www.esa.int/var/esa/storage/images/esa_multimedia/images/2018/04/gaia_s_sky_in_colour2/17475368-10-eng-GB/Gaia_s_sky_in_colour.jpg'
    )

    # Kenney (CC0)
    $Kenney = @(
        'sci-fi-sounds', 'impact-sounds', 'interface-sounds', 'music-jingles'
    )

    $CreditsFixes = @(
        'NASA (domaine public, pas de logo NASA) :',
        '- Modeles 3D : NASA 3D Resources, https://science.nasa.gov/3d-resources/',
        '- Cartes d''etoiles et Lune : NASA''s Scientific Visualization Studio, https://svs.gsfc.nasa.gov/4851 , /4856 , /3572 , /4720',
        '- Visualisations de trous noirs : NASA Goddard Space Flight Center, https://svs.gsfc.nasa.gov/13326 , /14576 , /13831',
        '',
        '"Gaia''s sky in colour (carte du ciel Gaia DR2)" par ESA/Gaia/DPAC, CC BY-SA 3.0 IGO, https://www.esa.int/ESA_Multimedia/Images/2018/04/Gaia_s_sky_in_colour2',
        '"The Milky Way panorama (ESO, 360)" par ESO/S. Brunier, CC BY 4.0, https://www.eso.org/public/images/eso0932a/',
        '"VISTA gigapixel mosaic of the central Milky Way" par ESO/VVV Survey/D. Minniti, CC BY 4.0, https://www.eso.org/public/images/eso1242a/',
        '"HYG Stellar Database (Hipparcos, Yale, Gliese)" par Astronexus (HYG Database), CC BY-SA 4.0, https://codeberg.org/astronexus/hyg',
        '"NIRCam " Cosmic Cliffs " in Carina (James Webb)" par NASA, ESA, CSA, and STScI, CC BY 4.0, https://esawebb.org/images/weic2205a/',
        '"Pillars of Creation - vue visible (Hubble 2015)" par NASA, ESA/Hubble and the Hubble Heritage Team, CC BY 4.0, https://esahubble.org/images/heic1501a/',
        '"Carina Nebula " Star birth in the extreme " (mosaique Hubble)" par NASA, ESA, N. Smith, CC BY 4.0, https://esahubble.org/images/heic0707a/',
        '"Hubble Ultra Deep Field (" Hubble sees galaxies galore ")" par NASA, ESA, S. Beckwith, CC BY 4.0, https://esahubble.org/images/heic0406a/',
        '"First Image of a Black Hole - M87* (EHT)" par EHT Collaboration, CC BY 4.0, https://www.eso.org/public/images/eso1907a/',
        '"First image of our black hole - Sagittarius A* (EHT)" par EHT Collaboration, CC BY 4.0, https://www.eso.org/public/images/eso2208-eht-mwa/',
        '"Solar System Scope - Textures (planetes, lunes, etoiles, jusqu''a 8K)" par Solar System Scope, CC BY 4.0, https://www.solarsystemscope.com/textures/'
    )

    $AlaMain = @(
        'Assets gratuits a recuperer a la main (pas de telechargement automatique possible).',
        '',
        'UNITY ASSET STORE : ouvrir le lien, "Add to My Assets", puis dans Unity : Window > Package Manager > My Assets > Download > Import.',
        '- Human Basic Motions FREE (Kevin Iglesias) : https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271',
        '- Starter Assets, First Person + Third Person Character Controllers (Unity Technologies) : https://assetstore.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526',
        '- Huge FBX Mocap Library part 1 (cMonkeys) : https://assetstore.unity.com/packages/3d/animations/huge-fbx-mocap-library-part-1-19991',
        '- Sci fi Drones (Lukas Bobor) : https://assetstore.unity.com/packages/3d/characters/robots/sci-fi-drones-90326',
        '- Space Station Free 3D Asset (HDRP + URP + Built-In) : https://assetstore.unity.com/packages/3d/vehicles/space/space-station-free-3d-asset-hdrp-urp-built-in-188734',
        '- Star Sparrow Modular Spaceship : https://assetstore.unity.com/packages/3d/vehicles/space/star-sparrow-modular-spaceship-73167',
        '- Sci-Fi Styled Modular Pack : https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-styled-modular-pack-82913',
        '- Federation Corvette F3 : https://assetstore.unity.com/packages/3d/vehicles/space/federation-corvette-f3-79860',
        '- Yughues Free Metal Materials : https://assetstore.unity.com/packages/2d/textures-materials/metals/yughues-free-metal-materials-12949',
        '- 3D Scifi Kit Starter Kit (Creepy Cat) : https://assetstore.unity.com/packages/3d/environments/3d-scifi-kit-starter-kit-92152',
        '- Altar Ruins Free : https://assetstore.unity.com/packages/3d/environments/fantasy/altar-ruins-free-109065',
        '- Nebula Skyboxes (4 skyboxes EXR, Road Turtle Games) : https://assetstore.unity.com/packages/2d/textures-materials/sky/nebula-skyboxes-219924',
        '- Deep Space Skybox Pack (Sean Duffy) : https://assetstore.unity.com/packages/2d/textures-materials/deep-space-skybox-pack-11056',
        '- Particle Pack (Unity) : https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325',
        '- Free Sound Effects Pack : https://assetstore.unity.com/packages/audio/sound-fx/free-sound-effects-pack-155776',
        '',
        'OUTILS, ANIMATIONS, CARTES GEANTES (plusieurs Go), MODELES STL :',
        '- MakeHuman 1.x, generateur d''humains : https://static.makehumancommunity.org/makehuman.html',
        '- MPFB 2, MakeHuman Plugin For Blender : https://static.makehumancommunity.org/mpfb.html',
        '- Universal Animation Library, plus de 120 animations humanoides (Quaternius) : https://quaternius.com/packs/universalanimationlibrary.html',
        '- Mesh2Motion, auto-rig et bibliotheque d''animations : https://mesh2motion.org/',
        '- 100STYLE, dataset mocap de 100 styles de locomotion (BVH) : https://www.ianxmason.com/100style/',
        '- Curiosity Rover (Detailed) : https://science.nasa.gov/3d-resources/curiosity-rover-detailed/',
        '- Orion Capsule : https://science.nasa.gov/3d-resources/orion-capsule/',
        '- Space 3D - generateur de skybox spatiale : https://tools.wwwtyro.net/space-3d/index.html',
        '- A Real-time High-quality Black Hole Shader (Eric Bruneton) : https://ebruneton.github.io/black_hole_shader/',
        '- Cassini''s Best Maps of Jupiter - Cylindrical Map (PIA07782) : https://science.nasa.gov/photojournal/cassinis-best-maps-of-jupiter-cylindrical-map/',
        '- Europa Voyager-Galileo SSI Global Mosaic 500m : https://astrogeology.usgs.gov/search/map/europa_voyager_galileo_ssi_global_mosaic_500m',
        '- Mars Viking Colorized Global Mosaic 232m : https://astrogeology.usgs.gov/search/map/mars_viking_colorized_global_mosaic_232m',
        '- Moon LRO LROC WAC Global Morphology Mosaic 100m : https://astrogeology.usgs.gov/search/map/moon_lro_lroc_wac_global_morphology_mosaic_100m',
        '- Mars MGS MOLA DEM 463m (carte de hauteur globale) : https://astrogeology.usgs.gov/search/map/mars_mgs_mola_dem_463m',
        '- Mars MGS MOLA - MEX HRSC Blended DEM Global 200m : https://astrogeology.usgs.gov/search/map/mars_mgs_mola_mex_hrsc_blended_dem_global_200m',
        '- Moon LRO LOLA DEM 118m : https://astrogeology.usgs.gov/search/map/moon_lro_lola_dem_118m',
        '- Moon LRO LOLA - SELENE Kaguya TC DEM Merge 60N60S 59m : https://astrogeology.usgs.gov/search/map/moon_lro_lola_selene_kaguya_tc_dem_merge_60n60s_59m',
        '- HiRISE Digital Terrain Models (Mars, echelle metrique) : https://www.uahirise.org/dtm/',
        '- VisualEffectGraph-Samples (exemples VFX Graph HDRP) : https://github.com/Unity-Technologies/VisualEffectGraph-Samples',
        '',
        'SONS ET MUSIQUE :',
        '- Sonniss GameAudioGDC (plusieurs dizaines de Go, libre de droits) : https://sonniss.com/gameaudiogdc',
        '- Freesound, filtre CC0 (compte gratuit) : https://freesound.org/search/?f=license%3A%22Creative+Commons+0%22',
        '- Pixabay Sound Effects : https://pixabay.com/sound-effects/',
        '- Pixabay Music : https://pixabay.com/music/',
        '- Incompetech (CC BY, crediter Kevin MacLeod) : https://incompetech.com/music/royalty-free/music.html',
        '- Chandra, sons de l''espace (NASA) : https://chandra.si.edu/sound/',
        '- NASA Audio : https://www.nasa.gov/audio-and-ringtones/',
        '- OpenGameArt (prendre CC0, CC BY ou CC BY-SA) : https://opengameart.org/',
        '',
        'MODELES SKETCHFAB TROP LOURDS POUR LE TELECHARGEMENT AUTOMATIQUE (ajoutes par le script) :'
    )

    $LisezMoi = @(
        'Assets externes d''Echos d''Ether, telecharges par outils/telecharger-assets.ps1. Liste complete et licences : docs/ASSETS.md.',
        '',
        'Dans Unity :',
        '- Normal maps (fichiers ''nor_gl'' ou ''NormalGL'') : Texture Type = Normal map.',
        '- Ciels HDRI (.hdr, .exr) : Texture Shape = Cube, puis Volume > Visual Environment > Sky type = HDRI Sky.',
        '- Modeles .glb (Sketchfab, NASA) : importes par le package glTFast, ajoute automatiquement au projet.',
        '- Scans tres detailles (plus de 500 000 triangles) : a reduire (Blender > Decimate) ou a garder pour les vues lointaines.',
        '- Assets CC BY : credit obligatoire, voir CREDITS-ASSETS.txt a la racine du projet.',
        '',
        'Dossiers :',
        '- PolyHaven, ambientCG : rochers, falaises, textures de sol, de metal et de beton, ciels (CC0).',
        '- Sketchfab : personnages, creatures, vaisseaux, ruines, terrain (CC0, CC BY, CC BY-SA). Un LICENCE.txt par modele.',
        '- NASA : combinaisons, rovers, modules et stations reels (domaine public).',
        '- Ciel : cartes d''etoiles, textures de planetes, relief de la Lune, catalogue d''etoiles HYG.',
        '- Sons : bruitages et jingles Kenney (CC0).',
        'Images de reference (Hubble, Webb, trous noirs) : dossier References a la racine du projet, hors Unity.'
    )

    # ---------------------------------------------------------------- outils

    $Etat = @{ Octets = [long]0; Erreurs = New-Object System.Collections.ArrayList; AlaMain = New-Object System.Collections.ArrayList }

    function Premiers($liste) {
        if ($Limite -gt 0) { return @($liste | Select-Object -First $Limite) }
        return @($liste)
    }

    function Telecharger([string]$url, [string]$chemin) {
        if (-not $url) { return $false }
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
                if ($essai -eq 3) {
                    [void]$Etat.Erreurs.Add("$url : $($_.Exception.Message)")
                    Write-Host "    [!] echec : $(Split-Path $chemin -Leaf)" -ForegroundColor Yellow
                    return $false
                }
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

    function Extraire([string]$zip, [string]$dossier) {
        if (Test-Path -LiteralPath $dossier) { Remove-Item -LiteralPath $dossier -Recurse -Force }
        Expand-Archive -LiteralPath $zip -DestinationPath $dossier -Force
    }

    function Etape([string]$titre, [scriptblock]$action) {
        Write-Host "  $titre"
        try { & $action }
        catch {
            [void]$Etat.Erreurs.Add("$titre : $($_.Exception.Message)")
            Write-Host "    [!] $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }

    function Titre([string]$t) { Write-Host ''; Write-Host "=== $t ===" -ForegroundColor Cyan }

    # ---------------------------------------------------------------- choix

    Write-Host ''
    Write-Host "=== Assets gratuits d'Echos d'Ether ===" -ForegroundColor Cyan
    foreach ($g in $Groupes) { Write-Host ("  {0}. {1}" -f $g[0], $g[1]) }
    $choix = $env:ECHOS_CHOIX
    if (-not $choix) { $choix = Read-Host 'Numeros separes par des virgules (ex. 1,2,5), Entree = tout' }
    if (-not $choix) { $choix = '1,2,3,4,5,6' }
    $choisis = @($choix -split '[,; ]+' | Where-Object { $_ -match '^[1-6]$' })
    if ($choisis.Count -eq 0) { Write-Host 'Rien a faire.'; return }

    $Auth = $null
    if ($choisis -contains '3') {
        $token = $env:SKETCHFAB_TOKEN
        if (-not $token -and -not $env:ECHOS_CHOIX) {
            Write-Host ''
            Write-Host 'Sketchfab : colle ton token API (compte gratuit, https://sketchfab.com/settings/password, section "API Token").'
            $token = Read-Host 'Token (Entree pour passer Sketchfab)'
        }
        if ($token) {
            $Auth = @{ Authorization = "Token $($token.Trim())" }
            try { $moi = Api 'https://api.sketchfab.com/v3/me' $Auth; Write-Host "  Connecte a Sketchfab : $($moi.username)" -ForegroundColor Green }
            catch { Write-Host '  Token Sketchfab refuse : groupe 3 ignore.' -ForegroundColor Yellow; $Auth = $null }
        }
        if (-not $Auth) { $choisis = @($choisis | Where-Object { $_ -ne '3' }) }
    }

    if ((Get-Process -Name 'Unity' -ErrorAction SilentlyContinue) -and -not $env:ECHOS_CHOIX) {
        Write-Host ''
        Write-Host "Unity est ouvert : il va importer chaque fichier pendant le telechargement (tres lent)." -ForegroundColor Yellow
        if ((Read-Host 'Le fermer est conseille. Continuer quand meme ? (o/N)') -notmatch '^[oOyY]') { return }
    }

    try {
        $libre = (Get-Item -LiteralPath $Projet).PSDrive.Free
        if ($libre -and $libre -lt 25GB) { Write-Host ("Attention : seulement {0:N0} Go libres sur le disque (prevoir 25 Go avec Sketchfab)." -f ($libre / 1GB)) -ForegroundColor Yellow }
    } catch { }

    New-Item -ItemType Directory -Force -Path $Tmp | Out-Null

    # glTFast : importe les modeles .glb / .gltf (Sketchfab et NASA)
    $manifest = Join-Path $Projet 'Packages\manifest.json'
    if (($choisis -contains '3' -or $choisis -contains '4') -and (Test-Path -LiteralPath $manifest)) {
        $txt = [IO.File]::ReadAllText($manifest)
        if ($txt -notmatch 'com\.unity\.cloud\.gltfast') {
            $txt = $txt -replace '"dependencies"\s*:\s*\{', "`"dependencies`": {`n    `"com.unity.cloud.gltfast`": `"$GltfastVersion`","
            [IO.File]::WriteAllText($manifest, $txt)
            Write-Host "  Package glTFast $GltfastVersion ajoute au projet (pour les fichiers .glb)." -ForegroundColor Green
        }
    }

    # ---------------------------------------------------------------- 1. Poly Haven

    if ($choisis -contains '1') {
        Titre 'Poly Haven (CC0)'
        $i = 0; $l = Premiers $PolyHavenModeles
        foreach ($id in $l) {
            $i++
            Etape "[$i/$($l.Count)] modele $id" {
                $f = Api "https://api.polyhaven.com/files/$id"
                $fbx = $f.fbx.$Res.fbx
                $d = Join-Path $Ext "PolyHaven\Modeles\$id"
                [void](Telecharger $fbx.url (Join-Path $d "$id.fbx"))
                foreach ($p in $fbx.include.PSObject.Properties) {
                    [void](Telecharger $p.Value.url (Join-Path $d ($p.Name -replace '/', '\')))
                }
            }
        }
        $i = 0; $l = Premiers $PolyHavenTextures
        foreach ($id in $l) {
            $i++
            Etape "[$i/$($l.Count)] texture $id" {
                $f = Api "https://api.polyhaven.com/files/$id"
                foreach ($carte in 'Diffuse', 'nor_gl', 'Rough', 'AO', 'Displacement', 'Metal') {
                    $c = $f.$carte
                    if (-not $c) { continue }
                    $u = $c.$Res.jpg.url
                    if (-not $u) { $u = $c.$Res.png.url }
                    if ($u) { [void](Telecharger $u (Join-Path $Ext "PolyHaven\Textures\$id\$(Split-Path $u -Leaf)")) }
                }
            }
        }
        $i = 0; $l = Premiers $PolyHavenHdri
        foreach ($id in $l) {
            $i++
            Etape "[$i/$($l.Count)] ciel HDRI $id" {
                $f = Api "https://api.polyhaven.com/files/$id"
                [void](Telecharger $f.hdri.'4k'.hdr.url (Join-Path $Ext "PolyHaven\HDRI\${id}_4k.hdr"))
            }
        }
    }

    # ---------------------------------------------------------------- 2. ambientCG

    if ($choisis -contains '2') {
        Titre 'ambientCG (CC0)'
        $i = 0; $l = Premiers $AmbientCgTextures
        foreach ($id in $l) {
            $i++
            Etape "[$i/$($l.Count)] texture $id" {
                $d = Join-Path $Ext "ambientCG\Textures\$id"
                if (Test-Path -LiteralPath $d) { return }
                $zip = Join-Path $Tmp "$id.zip"
                if (Telecharger "https://ambientcg.com/get?file=${id}_$($Res.ToUpper())-JPG.zip" $zip) {
                    $x = Join-Path $Tmp $id
                    Extraire $zip $x
                    New-Item -ItemType Directory -Force -Path $d | Out-Null
                    Get-ChildItem -LiteralPath $x -File |
                        Where-Object { $_.Name -match '_(Color|NormalGL|Roughness|AmbientOcclusion|Displacement|Metalness|Opacity)\.(jpg|png)$' } |
                        Move-Item -Destination $d
                    Remove-Item -LiteralPath $x, $zip -Recurse -Force
                }
            }
        }
        $i = 0; $l = Premiers $AmbientCgHdri
        foreach ($id in $l) {
            $i++
            Etape "[$i/$($l.Count)] ciel HDRI $id" {
                $cible = Join-Path $Ext "ambientCG\HDRI\${id}_4K_HDR.exr"
                if (Test-Path -LiteralPath $cible) { return }
                $zip = Join-Path $Tmp "${id}_4K.zip"
                if (Telecharger "https://ambientcg.com/get?file=${id}_4K.zip" $zip) {
                    $x = Join-Path $Tmp $id
                    Extraire $zip $x
                    New-Item -ItemType Directory -Force -Path (Split-Path $cible) | Out-Null
                    Get-ChildItem -LiteralPath $x -File -Filter '*.exr' | Select-Object -First 1 | Move-Item -Destination $cible
                    Remove-Item -LiteralPath $x, $zip -Recurse -Force
                }
            }
        }
    }

    # ---------------------------------------------------------------- 4 et 5. fichiers directs (NASA, espace)

    function Fichiers-Directs($liste) {
        $i = 0; $l = Premiers $liste
        foreach ($e in $l) {
            $i++
            $dossier, $nom, $url = $e -split '\|', 3
            Etape "[$i/$($l.Count)] $nom" {
                $d = Join-Path $Projet $dossier
                if ($nom -like '*.zip') {
                    $cible = Join-Path $d ([IO.Path]::GetFileNameWithoutExtension($nom))
                    if (Test-Path -LiteralPath $cible) { return }
                    $zip = Join-Path $Tmp $nom
                    if (Telecharger $url $zip) { Extraire $zip $cible; Remove-Item -LiteralPath $zip -Force }
                } else {
                    [void](Telecharger $url (Join-Path $d $nom))
                }
            }
        }
    }

    if ($choisis -contains '4') { Titre 'NASA 3D Resources (domaine public)'; Fichiers-Directs $Nasa }
    if ($choisis -contains '5') { Titre "Espace : etoiles, planetes, Lune, references"; Fichiers-Directs $Espace }

    # ---------------------------------------------------------------- 6. sons Kenney

    if ($choisis -contains '6') {
        Titre 'Sons Kenney (CC0)'
        $i = 0; $l = Premiers $Kenney
        foreach ($p in $l) {
            $i++
            Etape "[$i/$($l.Count)] $p" {
                $d = Join-Path $Ext "Sons\Kenney\$p"
                if (Test-Path -LiteralPath $d) { return }
                $page = Invoke-WebRequest -Uri "https://kenney.nl/assets/$p" -UseBasicParsing
                $m = [regex]::Match($page.Content, 'https://kenney\.nl/media/pages/assets/[^"''\s]+\.zip')
                if (-not $m.Success) { throw "lien du zip introuvable sur https://kenney.nl/assets/$p" }
                $zip = Join-Path $Tmp "$p.zip"
                if (Telecharger $m.Value $zip) { Extraire $zip $d; Remove-Item -LiteralPath $zip -Force }
            }
        }
        # Armes du jeu : Kenney Blaster Kit, seulement les .glb et leur texture (pas de FBX/OBJ en double)
        Etape '[+] armes : blaster-kit' {
            $d = Join-Path $Ext 'Modeles\Kenney\blaster-kit'
            if (Test-Path -LiteralPath $d) { return }
            $zip = Join-Path $Tmp 'blaster-kit.zip'
            $tmpd = Join-Path $Tmp 'blaster-kit'
            $lien = $null
            try {
                $page = Invoke-WebRequest -Uri 'https://kenney.nl/assets/blaster-kit' -UseBasicParsing
                $m = [regex]::Match($page.Content, 'https://kenney\.nl/media/pages/assets/[^"''\s]+\.zip')
                if ($m.Success) { $lien = $m.Value }
            } catch { }
            if (-not $lien -or -not (Telecharger $lien $zip)) {
                # repli : copies CC0 rangees dans le depot du jeu (Kenney autorise la redistribution)
                Write-Host '    site Kenney indisponible : copies du depot du jeu'
                $depot = 'https://raw.githubusercontent.com/madhiphares-eng/mon-projet/echos-ether/ressources/kenney-blaster-kit'
                foreach ($f in 'blaster-a.glb', 'blaster-d.glb', 'blaster-e.glb', 'blaster-l.glb', 'grenade-a.glb', 'grenade-b.glb', 'LICENCE.txt') {
                    [void](Telecharger "$depot/$f" (Join-Path $d $f))
                }
                [void](Telecharger 'https://media.githubusercontent.com/media/madhiphares-eng/mon-projet/echos-ether/ressources/kenney-blaster-kit/Textures/colormap.png' (Join-Path $d 'Textures\colormap.png'))
                return
            }
            if (Test-Path -LiteralPath $zip) {
                Extraire $zip $tmpd
                New-Item -ItemType Directory -Force -Path $d | Out-Null
                Copy-Item -Path (Join-Path $tmpd 'Models\GLB format\*') -Destination $d -Recurse -Force
                $lic = Join-Path $tmpd 'License.txt'
                if (Test-Path -LiteralPath $lic) { Copy-Item -LiteralPath $lic -Destination (Join-Path $d 'LICENCE.txt') -Force }
                Remove-Item -LiteralPath $tmpd -Recurse -Force
                Remove-Item -LiteralPath $zip -Force
            }
        }
    }

    # ---------------------------------------------------------------- 3. Sketchfab (le plus long, en dernier)

    if ($choisis -contains '3') {
        Titre 'Sketchfab (CC0, CC BY, CC BY-SA)'
        $i = 0; $l = Premiers $Sketchfab
        foreach ($e in $l) {
            $i++
            $cat, $uid, $nom = $e -split '\|', 3
            Etape "[$i/$($l.Count)] $cat/$nom" {
                $d = Join-Path $Ext "Sketchfab\$cat\$nom"
                $licence = Join-Path $d 'LICENCE.txt'
                if (Test-Path -LiteralPath $licence) { return }
                # La licence est relue a chaque fois : si l'auteur l'a changee, le modele est ignore.
                $info = Api "https://api.sketchfab.com/v3/models/$uid"
                if (-not $info.isDownloadable -or @('cc0', 'by', 'by-sa') -notcontains $info.license.slug) {
                    Write-Host "    ignore : licence actuelle '$($info.license.slug)'" -ForegroundColor Yellow
                    return
                }
                $dl = Api "https://api.sketchfab.com/v3/models/$uid/download" $Auth
                $fichier = if ($dl.glb) { $dl.glb } else { $dl.gltf }
                if (-not $fichier) { throw 'aucun format glTF propose' }
                if ($fichier.size -gt $MaxSketchfabMo * 1MB) {
                    Write-Host ("    trop lourd ({0:N0} Mo) : ajoute a la liste a telecharger a la main" -f ($fichier.size / 1MB)) -ForegroundColor Yellow
                    [void]$Etat.AlaMain.Add(('{0} ({1:N0} Mo, Sketchfab) : {2}' -f $info.name, ($fichier.size / 1MB), $info.viewerUrl))
                    return
                }
                $ok = $false
                if ($dl.glb) {
                    $ok = Telecharger $fichier.url (Join-Path $d "$nom.glb")
                } else {
                    $zip = Join-Path $Tmp "$uid.zip"
                    if (Telecharger $fichier.url $zip) { Extraire $zip $d; Remove-Item -LiteralPath $zip -Force; $ok = $true }
                }
                if ($ok) {
                    $credit = "`"$($info.name)`" par $($info.user.displayName) (sketchfab.com/$($info.user.username)), $($info.license.label), $($info.viewerUrl)"
                    [IO.File]::WriteAllText($licence, $credit + "`r`n", (New-Object Text.UTF8Encoding $false))
                }
                Start-Sleep -Milliseconds 300
            }
        }
    }

    # ---------------------------------------------------------------- credits, liste a la main, bilan

    $credits = New-Object System.Collections.ArrayList
    [void]$credits.Add("Credits des assets externes d'Echos d'Ether (a reprendre dans l'ecran des credits du jeu).")
    [void]$credits.Add('Poly Haven, ambientCG, Kenney : CC0, credit non obligatoire mais apprecie.')
    [void]$credits.Add('')
    $dossierSf = Join-Path $Ext 'Sketchfab'
    if (Test-Path -LiteralPath $dossierSf) {
        Get-ChildItem -LiteralPath $dossierSf -Recurse -Filter 'LICENCE.txt' | Sort-Object FullName | ForEach-Object {
            [void]$credits.Add([IO.File]::ReadAllText($_.FullName).Trim())
        }
    }
    foreach ($c in $CreditsFixes) { [void]$credits.Add($c) }
    [IO.File]::WriteAllText((Join-Path $Projet 'CREDITS-ASSETS.txt'), ($credits -join "`r`n") + "`r`n", (New-Object Text.UTF8Encoding $false))

    $main = @($AlaMain) + @($Etat.AlaMain)
    [IO.File]::WriteAllText((Join-Path $Projet 'A-TELECHARGER-A-LA-MAIN.txt'), ($main -join "`r`n") + "`r`n", (New-Object Text.UTF8Encoding $false))

    if (Test-Path -LiteralPath $Ext) {
        [IO.File]::WriteAllText((Join-Path $Ext 'LISEZMOI.txt'), ($LisezMoi -join "`r`n") + "`r`n", (New-Object Text.UTF8Encoding $false))
    }
    $erreurs = Join-Path $Projet 'telechargement-erreurs.txt'
    if ($Etat.Erreurs.Count -gt 0) { [IO.File]::WriteAllText($erreurs, ($Etat.Erreurs -join "`r`n") + "`r`n") }
    elseif (Test-Path -LiteralPath $erreurs) { Remove-Item -LiteralPath $erreurs -Force }
    Remove-Item -LiteralPath $Tmp -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host ''
    Write-Host ("[OK] Termine : {0:N0} Mo telecharges dans {1}" -f ($Etat.Octets / 1MB), $Ext) -ForegroundColor Green
    if ($Etat.Erreurs.Count -gt 0) {
        Write-Host "$($Etat.Erreurs.Count) echec(s), detail dans $erreurs : relance le script pour reessayer." -ForegroundColor Yellow
    }
    Write-Host "Credits a citer : $(Join-Path $Projet 'CREDITS-ASSETS.txt')"
    Write-Host "Assets a prendre a la main (Unity Asset Store, sons, cartes geantes) : $(Join-Path $Projet 'A-TELECHARGER-A-LA-MAIN.txt')"
    Write-Host "Ouvre Unity : le premier import peut prendre de 30 min a plus d'une heure."
    if ($env:OS -eq 'Windows_NT' -and (Test-Path -LiteralPath $Ext)) { explorer $Ext }
}
