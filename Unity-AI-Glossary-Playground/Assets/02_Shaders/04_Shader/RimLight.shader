// 0411 림 라이트 (Fresnel)
// Fresnel = 1 - dot(N, V) : 표면이 카메라를 정면으로 볼수록 0, 옆으로 꺾일수록(외곽) 1
// 이 값을 _RimPower 로 거듭제곱해 가장자리만 남기고 하늘색(_RimColor)으로 더함
Shader "Glossary/Shader/RimLight"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.35, 0.3, 0.45, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _RimColor("Rim Color", Color) = (0.45, 0.85, 1.6, 1)
        _RimPower("Rim Power", Range(0.5, 10)) = 3
        _RimIntensity("Rim Intensity", Range(0, 4)) = 1.5
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
            half4 _RimColor;
            float _RimPower;
            float _RimIntensity;
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
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 color = GlossarySimpleLighting(albedo, input.positionWS, normalWS);

                // Fresnel 림: 외곽일수록 1
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower);
                color += fresnel * _RimIntensity * _RimColor.rgb;
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
