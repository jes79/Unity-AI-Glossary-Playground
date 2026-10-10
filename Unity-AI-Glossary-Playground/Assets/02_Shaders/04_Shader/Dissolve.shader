// 0410 디졸브 셰이더
// 노이즈 값이 _Dissolve 보다 작은 픽셀을 clip() 으로 버려서 "타들어가며 사라지는" 모양을 만듦
// 사라지기 직전 경계(_EdgeWidth)에는 HDR 주황색(_EdgeColor)을 더해 빛나게 함 (Bloom 과 같이 보면 더 잘 보임)
// _HeightMode = 1 이면 노이즈에 높이(아래→위) 값을 섞어 발밑부터 위로 사라짐
Shader "Glossary/Shader/Dissolve"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.8, 0.8, 0.85, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _NoiseTex("Noise", 2D) = "gray" {}
        _NoiseScale("Noise Scale", Float) = 1.2
        _Dissolve("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth("Edge Width", Range(0.005, 0.3)) = 0.08
        [HDR] _EdgeColor("Edge Color", Color) = (4, 1.4, 0.2, 1)
        [Toggle] _HeightMode("Bottom To Top", Float) = 0
        _HeightMin("Height Min (World Y)", Float) = 0
        _HeightMax("Height Max (World Y)", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float4 _NoiseTex_ST;
            float _NoiseScale;
            float _Dissolve;
            float _EdgeWidth;
            half4 _EdgeColor;
            float _HeightMode;
            float _HeightMin;
            float _HeightMax;
        CBUFFER_END

        // 디졸브 판정 값 (0~1). 오브젝트 공간 좌표로 노이즈를 읽어서 물체가 움직여도 무늬가 따라감
        float DissolveField(float3 positionWS, float3 positionOS)
        {
            float3 p = positionOS * _NoiseScale;
            float n1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, p.xy).r;
            float n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, p.zy + 0.37).r;
            float noise = (n1 + n2) * 0.5;

            // 아래→위 모드: 높이 0(발) ~ 1(머리) 값을 노이즈와 섞음 → 낮은 곳부터 먼저 사라짐
            float h = saturate((positionWS.y - _HeightMin) / max(_HeightMax - _HeightMin, 0.001));
            return lerp(noise, lerp(h, noise, 0.25), _HeightMode);
        }

        // _Dissolve 0 → 아무것도 안 사라지고 경계도 없음, 1 → 전부 사라짐
        float DissolveThreshold()
        {
            return lerp(-_EdgeWidth - 0.01, 1.01, _Dissolve);
        }

        // 그림자/깊이 패스에도 같은 구멍을 뚫음
        #define GLOSSARY_DEPTH_CLIP(input) clip(DissolveField(input.positionWS, input.positionOS) - DissolveThreshold())

        #include "GlossaryShaderPasses.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                half3 normalWS    : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.positionOS = input.positionOS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float field = DissolveField(input.positionWS, input.positionOS);
                float diff = field - DissolveThreshold();
                clip(diff); // 핵심: 기준보다 작은 픽셀은 그리지 않음

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 color = GlossarySimpleLighting(albedo, input.positionWS, normalize(input.normalWS));

                // 경계: 사라지기 직전 구간(diff < _EdgeWidth)을 HDR 색으로 빛나게
                half edge = 1.0 - saturate(diff / _EdgeWidth);
                color = lerp(color, _EdgeColor.rgb, edge);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex GlossaryShadowVert
            #pragma fragment GlossaryDepthFrag
            #pragma multi_compile_instancing
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex GlossaryDepthVert
            #pragma fragment GlossaryDepthFrag
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
}
