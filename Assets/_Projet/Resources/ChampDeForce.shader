// Champ de force pour HDRP (cellules, portes du Colosse) : des hexagones qui respirent, un balayage, un cadre,
// et des ondes là où il est touché. Additif. Le maillage est un plan : x de -largeur/2 à +largeur/2, y de 0 à hauteur.
//   _Impacts[8] : xy = point touché (repère local), w = instant de l'impact (_Temps), w < 0 = libre
//   _Temps      : l'horloge du champ (donnée par le code, pour que les impacts restent synchrones)
Shader "EchosEther/ChampDeForce"
{
    Properties
    {
        [HDR] _Couleur("Couleur", Color) = (0.35, 0.85, 1.2, 1)
        _Intensite("Intensité", Float) = 1
        _Taille("Taille (largeur, hauteur)", Vector) = (2, 2.6, 0, 0)
        _Temps("Temps", Float) = 0
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
            float4 _Taille;
            float _Temps;
            float4 _Impacts[8];

            struct Attributs { float3 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionRWS : TEXCOORD0; float3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; float2 local : TEXCOORD3; };

            float Hasard(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
            float2 Modulo(float2 x, float2 y) { return x - y * floor(x / y); }
            float DistanceHexa(float2 p) { p = abs(p); return max(dot(p, normalize(float2(1.0, 1.732))), p.x); }

            Varyings Vert(Attributs entree)
            {
                Varyings sortie;
                sortie.positionRWS = TransformObjectToWorld(entree.positionOS);
                sortie.positionCS = TransformWorldToHClip(sortie.positionRWS);
                sortie.normalWS = TransformObjectToWorldNormal(entree.normalOS);
                sortie.uv = entree.uv;
                sortie.local = entree.positionOS.xy;
                return sortie;
            }

            float4 Frag(Varyings entree) : SV_Target
            {
                float2 p = entree.uv * _Taille.xy;
                // la grille d'hexagones
                float2 r = float2(1.0, 1.732), h = r * 0.5, q = p * 5.0;
                float2 a = Modulo(q, r) - h, b = Modulo(q - h, r) - h;
                float2 gv = dot(a, a) < dot(b, b) ? a : b;
                float trait = smoothstep(0.06, 0.0, 0.5 - DistanceHexa(gv));
                float pulse = 0.5 + 0.5 * sin(_Temps * 2.0 + Hasard(q - gv) * 6.283);
                float balayage = pow(0.5 + 0.5 * sin(p.y * 3.0 - _Temps * 2.5), 12.0);
                float lignes = 0.5 + 0.5 * sin(p.y * 160.0 + _Temps * 8.0);
                float2 bords = min(entree.uv, 1.0 - entree.uv) * _Taille.xy;
                float cadre = smoothstep(0.08, 0.0, min(bords.x, bords.y));
                // les ondes des impacts
                float onde = 0.0;
                [unroll] for (int i = 0; i < 8; i++)
                {
                    float4 im = _Impacts[i];
                    float age = _Temps - im.w;
                    if (im.w < 0.0 || age > 1.2) continue;
                    float d = length(entree.local - im.xy);
                    onde += smoothstep(0.12, 0.0, abs(d - age * 2.4)) * exp(-age * 3.0) * 2.0 + exp(-d * 6.0) * exp(-age * 8.0) * 2.5;
                }
                float3 vue = GetWorldSpaceNormalizeViewDir(entree.positionRWS);
                float face = abs(dot(normalize(entree.normalWS), vue));
                float masque = (trait * (0.35 + 0.4 * pulse) + 0.06 + balayage * 0.35 + cadre * 0.9) * (0.85 + 0.15 * lignes) + onde;
                masque *= _Intensite * lerp(1.4, 1.0, face) * (0.92 + 0.08 * Hasard(float2(floor(_Temps * 30.0), 0.0)));
                return float4(_Couleur.rgb * masque, 1.0);
            }
            ENDHLSL
        }
    }
}
