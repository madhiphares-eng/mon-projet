// Volume lumineux additif pour HDRP : flamme d'un propulseur, rayon d'éther, faisceau d'un gyrophare.
// Le maillage (EffetsColosse.Tronc) porte en uv.y sa position le long de l'axe, de 0 (début) à 1 (fin).
//   _Couleur, _CouleurCoeur : le bord et le cœur (HDR)
//   _Coeur      : 0 = uniforme ; 2 à 4 = surtout au centre, vu de face (l'inverse d'un fresnel)
//   _FonduBas, _FonduHaut : la part de la longueur où il s'efface, au début et à la fin
//   _Bruit      : turbulences (0 à 1), qui défilent à la vitesse _Defilement
//   _Bandes     : des bandes qui défilent le long de l'axe (diamants du jet, rayon), 0 = aucune
Shader "EchosEther/Volume"
{
    Properties
    {
        [HDR] _Couleur("Couleur", Color) = (0.4, 0.7, 1.4, 1)
        [HDR] _CouleurCoeur("Couleur du cœur", Color) = (1.4, 1.5, 1.6, 1)
        _Intensite("Intensité", Float) = 1
        _Coeur("Cœur", Range(0, 8)) = 1.6
        _FonduBas("Fondu au début", Range(0.001, 1)) = 0.5
        _FonduHaut("Fondu à la fin", Range(0.001, 1)) = 0.05
        _Bruit("Bruit", Range(0, 1)) = 0.8
        _Defilement("Défilement", Float) = 16
        _Bandes("Bandes", Float) = 0
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
            float4 _CouleurCoeur;
            float _Intensite;
            float _Coeur;
            float _FonduBas;
            float _FonduHaut;
            float _Bruit;
            float _Defilement;
            float _Bandes;

            struct Attributs { float3 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionRWS : TEXCOORD0; float3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; };

            float Hasard(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
            float Bruit(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hasard(i), Hasard(i + float2(1, 0)), f.x), lerp(Hasard(i + float2(0, 1)), Hasard(i + float2(1, 1)), f.x), f.y);
            }

            Varyings Vert(Attributs entree)
            {
                Varyings sortie;
                sortie.positionRWS = TransformObjectToWorld(entree.positionOS);
                sortie.positionCS = TransformWorldToHClip(sortie.positionRWS);
                sortie.normalWS = TransformObjectToWorldNormal(entree.normalOS);
                sortie.uv = entree.uv;
                return sortie;
            }

            float4 Frag(Varyings entree) : SV_Target
            {
                float3 vue = GetWorldSpaceNormalizeViewDir(entree.positionRWS);
                float face = abs(dot(normalize(entree.normalWS), vue));
                float le_long = entree.uv.y;
                float t = _Time.y * _Defilement;
                float n = Bruit(float2(entree.uv.x * 9.0, le_long * 7.0 - t)) * 0.6 + Bruit(float2(entree.uv.x * 21.0, le_long * 15.0 - t * 1.9)) * 0.4;
                float masque = pow(face, max(_Coeur, 0.001));
                masque *= smoothstep(0.0, _FonduBas, le_long) * (1.0 - smoothstep(1.0 - _FonduHaut, 1.0, le_long));
                masque *= lerp(1.0, 0.55 + 0.6 * n, _Bruit);
                if (_Bandes > 0.0) masque *= 0.8 + 0.2 * sin(le_long * _Bandes - _Time.y * 4.0);
                float3 c = lerp(_Couleur.rgb, _CouleurCoeur.rgb, pow(face, 3.0));
                return float4(c * masque * _Intensite, 1.0);
            }
            ENDHLSL
        }
    }
}
