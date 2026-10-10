// 0414 툰 셰이딩 (Cel Shading) - URP 메인 라이트 기준 명암을 _Steps 단계로 끊음
// 일반 조명:  밝기 = saturate(dot(N, L))            → 부드러운 그라데이션
// 툰 조명:    밝기 = floor(밝기 × 단계수) / (단계수-1) → 계단처럼 끊긴 명암 (기본 3단계)
Shader "Glossary/Shader/Toon"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 0.6, 0.35, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _ShadowColor("Shadow Color", Color) = (0.35, 0.3, 0.5, 1)
        _Steps("Steps", Range(2, 6)) = 3
        [Toggle] _Banding("Banding (Toon)", Float) = 1
        _SpecularSize("Specular Size", Range(0, 0.2)) = 0.04
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
            half4 _ShadowColor;
            float _Steps;
            float _Banding;
            float _SpecularSize;
        CBUFFER_END

        #include "GlossaryShaderPasses.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardToon"
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

                // URP 메인 라이트 (방향, 색, 그림자)
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half lightAmount = saturate(dot(normalWS, mainLight.direction)) * mainLight.shadowAttenuation;

                // 핵심: 밝기를 _Steps 단계로 끊음 (3단계 → 0, 0.5, 1)
                half steps = max(_Steps, 2.0);
                half band = min(floor(lightAmount * steps), steps - 1.0) / (steps - 1.0);
                half shade = lerp(lightAmount, band, _Banding);

                half3 color = albedo * lerp(_ShadowColor.rgb, mainLight.color, shade);

                // 만화풍 하이라이트: 반사 방향 근처를 딱 끊어서 흰 점으로
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half spec = saturate(dot(normalWS, halfDir));
                half specMask = step(1.0 - _SpecularSize, spec) * mainLight.shadowAttenuation;
                specMask = lerp(pow(spec, 64) * 0.6, specMask, _Banding);
                color += specMask * mainLight.color * 0.6;
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
