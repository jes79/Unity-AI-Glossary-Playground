// 1009 2D 조명 대체 구현 - 화면을 덮는 Quad 에 "라이트 맵" 텍스처를 곱하기(Multiply)로 얹음
// 결과 = 장면 색 x 빛 색  → 빛이 없는 곳은 어둡고(환경광), 횃불 주변만 밝음
Shader "Glossary/10_Rendering/LightMapMultiply2D"
{
    Properties
    {
        [MainTexture] _LightTex ("Light Map (Render Texture)", 2D) = "white" {}
        _Boost ("Boost", Float) = 1.3
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend DstColor Zero
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_LightTex);
            SAMPLER(sampler_LightTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _LightTex_ST;
                float _Boost;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 light = SAMPLE_TEXTURE2D(_LightTex, sampler_LightTex, input.uv).rgb * _Boost;
                return half4(light, 1);
            }
            ENDHLSL
        }
    }
}
