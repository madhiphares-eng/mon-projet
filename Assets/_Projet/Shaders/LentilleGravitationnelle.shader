// Lentille gravitationnelle d'un trou noir — passe plein écran HDRP (Custom Pass).
// Paramètres globaux envoyés par EchosEther.Rendu.TrouNoir :
//   _TN_Position : position monde absolue du trou noir (xyz)
//   _TN_Params   : x = rayon de l'ombre (monde), y = force de déviation, z = intensité de l'anneau de photons
//   _TN_Couleur  : couleur de l'anneau de photons
Shader "FullScreen/LentilleGravitationnelle"
{
    HLSLINCLUDE

    #pragma vertex Vert
    #pragma target 4.5
    #pragma only_renderers d3d11 vulkan metal

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float4 _TN_Position;
    float4 _TN_Params;
    float4 _TN_Couleur;

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

        float rawDepth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, rawDepth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float2 uv = posInput.positionNDC.xy;
        float4 couleur = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);

        // HDRP rend en coordonnées relatives à la caméra.
        float3 positionRel = GetCameraRelativePositionWS(_TN_Position.xyz);
        float distance = length(positionRel);
        float rayonMonde = _TN_Params.x;

        // Trou noir derrière la caméra ou caméra à l'intérieur : rien à faire.
        float zVue = mul(UNITY_MATRIX_V, float4(positionRel, 1.0)).z;
        if (rayonMonde <= 0.0 || zVue > 0.0 || distance <= rayonMonde)
            return couleur;

        // Les objets plus proches que le trou noir (terrain, joueur) ne sont pas déformés.
        if (rawDepth != UNITY_RAW_FAR_CLIP_VALUE && LinearEyeDepth(rawDepth, _ZBufferParams) < distance)
            return couleur;

        // Centre et rayon apparent de l'ombre à l'écran (unités : hauteur d'écran).
        float2 centre = ComputeNormalizedDeviceCoordinates(positionRel, UNITY_MATRIX_VP);
        float aspect = _ScreenSize.x / _ScreenSize.y;
        float angle = asin(saturate(rayonMonde / distance));
        float rayon = tan(angle) * abs(UNITY_MATRIX_P[1][1]) * 0.5;

        float2 d = uv - centre;
        d.x *= aspect;
        float r = max(length(d), 1e-5);
        float2 direction = d / r;

        // Déviation en 1/r (lentille mince). L'anneau d'Einstein se forme à r = rayon * sqrt(force).
        float deviation = _TN_Params.y * rayon * rayon / r;
        float2 source = direction * (r - deviation);
        source.x /= aspect;
        float2 uvSource = saturate(centre + source);

        float3 ciel = CustomPassSampleCameraColor(uvSource, 0).rgb;

        // Anneau de photons : fine couronne lumineuse au bord de l'ombre.
        float ecart = (r - rayon * 1.03) / (rayon * 0.04);
        float anneau = _TN_Params.z * exp(-ecart * ecart);
        ciel += _TN_Couleur.rgb * anneau * GetCurrentExposureMultiplier();

        // Ombre : aucune lumière ne s'en échappe.
        float horsOmbre = smoothstep(rayon * 0.97, rayon, r);
        return float4(ciel * horsOmbre, 1.0);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
