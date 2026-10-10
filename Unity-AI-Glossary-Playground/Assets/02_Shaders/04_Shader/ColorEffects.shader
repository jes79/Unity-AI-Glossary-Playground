// 0420 그레이스케일 / 색상 반전 / 얼음(Freeze) 틴트
// 흑백  : 밝기 = dot(색, (0.299, 0.587, 0.114))  ← 사람 눈이 초록을 가장 밝게 느끼는 가중치
// 반전  : 1 - 색
// 얼음  : 흑백으로 바꾼 밝기에 파란색(_FreezeColor)을 곱하고, 외곽(Fresnel)에 하얀 서리 빛을 더함
Shader "Glossary/Shader/ColorEffects"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Grayscale("Grayscale", Range(0, 1)) = 0
        _Invert("Invert", Range(0, 1)) = 0
        _Freeze("Freeze", Range(0, 1)) = 0
        _FreezeColor("Freeze Tint", Color) = (0.45, 0.75, 1, 1)
        [HDR] _FreezeRimColor("Freeze Rim", Color) = (0.8, 1.2, 1.6, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float _Grayscale;
            float _Invert;
            float _Freeze;
            half4 _FreezeColor;
            half4 _FreezeRimColor;
        CBUFFER_END

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
                half3 normalWS    : TEXCOORD2;
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
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normalWS = normalize(input.normalWS);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 color = saturate(GlossarySimpleLighting(albedo, input.positionWS, normalWS));

                // 흑백
                half gray = dot(color, half3(0.299, 0.587, 0.114));
                color = lerp(color, gray.xxx, _Grayscale);

                // 반전
                color = lerp(color, 1.0 - color, _Invert);

                // 얼음: 파란 계열 틴트 + 외곽 서리 빛
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), 3.0);
                half3 frozen = (0.25 + gray * 1.1) * _FreezeColor.rgb + fresnel * _FreezeRimColor.rgb;
                color = lerp(color, frozen, _Freeze);
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
