// 0415 홀로그램: 반투명(가산 블렌딩) + Fresnel 외곽 + 위로 흐르는 스캔라인 + 깜빡임 + 가끔 지직거림(글리치)
// 스캔라인 = frac(월드 Y × 밀도 - 시간 × 속도) : 시간이 흐르면 줄무늬가 위로 이동
Shader "Glossary/Shader/Hologram"
{
    Properties
    {
        [HDR] _HoloColor("Hologram Color", Color) = (0.3, 1.4, 2.2, 1)
        _Alpha("Alpha", Range(0, 1)) = 0.8
        _ScanDensity("Scanline Density (lines / m)", Float) = 18
        _ScanSpeed("Scanline Speed", Float) = 1.2
        _ScanStrength("Scanline Strength", Range(0, 1)) = 0.6
        _FresnelPower("Fresnel Power", Range(0.5, 8)) = 2
        [Toggle] _Flicker("Flicker", Float) = 1
        _GlitchStrength("Glitch Strength", Range(0, 0.2)) = 0.04
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _HoloColor;
            float _Alpha;
            float _ScanDensity;
            float _ScanSpeed;
            float _ScanStrength;
            float _FresnelPower;
            float _Flicker;
            float _GlitchStrength;
        CBUFFER_END

        #include "GlossaryShaderPasses.hlsl"
        ENDHLSL

        Pass
        {
            Name "Hologram"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One   // 가산: 뒤 배경에 빛을 더함 (어두운 곳에서 빛나 보임)
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS    : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // 글리치: 가끔(노이즈가 0.75 넘을 때) 가로 띠 단위로 옆으로 밀림
                float t = _Time.y;
                float gate = step(0.75, GlossaryValueNoise(float2(t * 4.0, 1.7)));
                float band = GlossaryHash21(float2(floor(positionWS.y * 12.0), floor(t * 20.0)));
                positionWS.x += (band - 0.5) * 2.0 * _GlitchStrength * gate;

                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float t = _Time.y;
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                // 외곽(Fresnel)이 더 진하게
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);

                // 스캔라인: 가는 줄무늬가 위로 흐름
                float s = frac(input.positionWS.y * _ScanDensity - t * _ScanSpeed);
                half lines = smoothstep(0.5, 1.0, s);
                // 큰 밝은 띠가 천천히 위로 훑고 지나감
                float sweep = frac(input.positionWS.y * 0.35 - t * _ScanSpeed * 0.25);
                half sweepBand = smoothstep(0.9, 1.0, sweep);

                // 깜빡임 (빠른 노이즈)
                half flicker = lerp(1.0, 0.65 + 0.35 * GlossaryValueNoise(float2(t * 18.0, 0.0)), _Flicker);

                half alpha = (0.18 + fresnel * 0.8 + lines * _ScanStrength * 0.5 + sweepBand * 0.6) * _Alpha * flicker;
                return half4(_HoloColor.rgb, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
