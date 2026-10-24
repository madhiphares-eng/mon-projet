// Lueur additive pour HDRP : bulles de temps, onde du scanner, faisceau de balise, réacteurs.
// La couleur est écrite telle quelle (indépendante de l'exposition) : 1 = blanc, au-delà le bloom s'en charge.
//   _Couleur   : teinte (HDR)
//   _Intensite : multiplicateur global (pour les fondus)
//   _Fresnel   : 0 = uniforme, 2 à 4 = surtout sur les bords (aspect de bulle)
//   _Base      : part visible au centre quand le fresnel est fort
//   _Bandes    : ondulations qui défilent (bulles), 0 = aucune
Shader "EchosEther/Lueur"
{
    Properties
    {
        [HDR] _Couleur("Couleur", Color) = (0.4, 1.4, 1.3, 1)
        _Intensite("Intensité", Float) = 1
        _Fresnel("Fresnel", Range(0, 8)) = 3
        _Base("Base", Range(0, 1)) = 0.05
        _Bandes("Bandes", Range(0, 4)) = 0
        _VitesseBandes("Vitesse des bandes", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5
            #pragma only_renderers d3d11 vulkan metal

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            float4 _Couleur;
            float _Intensite;
            float _Fresnel;
            float _Base;
            float _Bandes;
            float _VitesseBandes;

            struct Attributs { float3 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionRWS : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            Varyings Vert(Attributs entree)
            {
                Varyings sortie;
                sortie.positionRWS = TransformObjectToWorld(entree.positionOS);   // relatif à la caméra en HDRP
                sortie.positionCS = TransformWorldToHClip(sortie.positionRWS);
                sortie.normalWS = TransformObjectToWorldNormal(entree.normalOS);
                return sortie;
            }

            float4 Frag(Varyings entree) : SV_Target
            {
                float3 vue = GetWorldSpaceNormalizeViewDir(entree.positionRWS);
                float bord = pow(1.0 - abs(dot(normalize(entree.normalWS), vue)), max(_Fresnel, 0.001));
                float masque = _Fresnel > 0.001 ? saturate(bord + _Base) : 1.0;
                if (_Bandes > 0.0)
                {
                    float3 mondeAbs = GetAbsolutePositionWS(entree.positionRWS);
                    float onde = 0.5 + 0.5 * sin(dot(mondeAbs, float3(0.31, 0.9, 0.22)) * _Bandes - _Time.y * _VitesseBandes);
                    masque *= 0.75 + 0.5 * onde;
                }
                return float4(_Couleur.rgb * masque * _Intensite, 1.0);
            }
            ENDHLSL
        }
    }
}
