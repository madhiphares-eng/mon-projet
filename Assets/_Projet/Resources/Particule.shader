// Particules pour HDRP (étincelles, vapeur, lueurs, éclairs de tir) : texture × couleur de la particule.
// Deux mélanges, choisis par le code (EffetsColosse.Particules) :
//   additif (One One) : la couleur est prémultipliée par l'alpha, le bloom fait le reste ;
//   fondu (SrcAlpha OneMinusSrcAlpha) : pour la fumée et la vapeur.
// Comme Lueur, la couleur est écrite telle quelle (indépendante de l'exposition) : 1 = blanc.
//   _Douceur : particules molles, en mètres (elles s'effacent en touchant une paroi) ; 0 = aucune
Shader "EchosEther/Particule"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        [HDR] _Teinte("Teinte", Color) = (1, 1, 1, 1)
        _Intensite("Intensité", Float) = 1
        _Douceur("Douceur", Float) = 0.3
        [HideInInspector] _SrcBlend("Mélange source", Float) = 1
        [HideInInspector] _DstBlend("Mélange destination", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            Blend [_SrcBlend] [_DstBlend]
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Teinte;
            float _Intensite;
            float _Douceur;
            float _DstBlend;

            struct Attributs { float3 positionOS : POSITION; float4 couleur : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 couleur : COLOR; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributs entree)
            {
                Varyings sortie;
                float3 positionRWS = TransformObjectToWorld(entree.positionOS);
                sortie.positionCS = TransformWorldToHClip(positionRWS);
                sortie.couleur = entree.couleur;
                sortie.uv = entree.uv;
                return sortie;
            }

            float4 Frag(Varyings entree) : SV_Target
            {
                float4 t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, entree.uv) * entree.couleur * _Teinte;
                if (_Douceur > 0.0)
                {
                    float scene = LinearEyeDepth(LoadCameraDepth(uint2(entree.positionCS.xy)), _ZBufferParams);
                    float moi = LinearEyeDepth(entree.positionCS.z, _ZBufferParams);
                    t.a *= saturate((scene - moi) / _Douceur);
                }
                float additif = _DstBlend == 1.0 ? 1.0 : 0.0;
                return float4(t.rgb * _Intensite * lerp(1.0, t.a, additif), t.a);
            }
            ENDHLSL
        }
    }
}
