// 0417 물 셰이더 (Depth 기반 색 + 거품 + 물결)
// 핵심: 카메라 Depth Texture 에서 "물 아래 바닥까지 거리"를 읽음
//   물 깊이 = 바닥의 시야 깊이(SampleSceneDepth) - 수면의 시야 깊이
//   얕으면(깊이≈0) _ShallowColor, 깊을수록 _DeepColor. 아주 얕은 물가에는 거품
// 카메라(또는 URP 에셋)에서 Depth Texture 가 켜져 있어야 동작함
Shader "Glossary/Shader/Water"
{
    Properties
    {
        _ShallowColor("Shallow Color", Color) = (0.35, 0.95, 0.9, 0.45)
        _DeepColor("Deep Color", Color) = (0.02, 0.12, 0.35, 0.95)
        _DepthMaxDistance("Depth Max Distance", Range(0.1, 10)) = 2.5
        [Toggle] _UseDepth("Use Depth", Float) = 1
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamDistance("Foam Distance", Range(0, 2)) = 0.35
        _NoiseTex("Noise", 2D) = "gray" {}
        _WaveHeight("Wave Height", Range(0, 0.5)) = 0.08
        _WaveSpeed("Wave Speed", Range(0, 5)) = 1.2
        _NormalStrength("Ripple Normal Strength", Range(0, 2)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);

        CBUFFER_START(UnityPerMaterial)
            half4 _ShallowColor;
            half4 _DeepColor;
            float _DepthMaxDistance;
            float _UseDepth;
            half4 _FoamColor;
            float _FoamDistance;
            float4 _NoiseTex_ST;
            float _WaveHeight;
            float _WaveSpeed;
            float _NormalStrength;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Water"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float eyeDepth    : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                // 출렁이는 물결 (두 방향 sin 합)
                float t = _Time.y * _WaveSpeed;
                positionWS.y += (sin(positionWS.x * 0.8 + t) + sin(positionWS.z * 1.1 + t * 1.3)) * 0.5 * _WaveHeight;
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.eyeDepth = -TransformWorldToView(positionWS).z; // 수면의 시야 깊이(카메라에서 거리)
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float t = _Time.y * _WaveSpeed;

                // ── 1) 물 깊이: 바닥 깊이 - 수면 깊이 ──
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float sceneEyeDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float waterDepth = max(sceneEyeDepth - input.eyeDepth, 0.0);
                float depth01 = saturate(waterDepth / _DepthMaxDistance);
                depth01 = lerp(0.5, depth01, _UseDepth); // Depth OFF 비교용: 어디나 같은 색

                half4 color = lerp(_ShallowColor, _DeepColor, depth01);

                // ── 2) 잔물결 법선 + 하이라이트 ──
                float2 uv = input.positionWS.xz * _NoiseTex_ST.xy;
                half n1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv * 0.35 + float2(t * 0.03, t * 0.02)).r;
                half n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv * 0.5 - float2(t * 0.02, t * 0.035)).r;
                half3 normalWS = normalize(half3((n1 - 0.5) * _NormalStrength, 1.0, (n2 - 0.5) * _NormalStrength));
                Light mainLight = GetMainLight();
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), 180.0) * 1.5;
                half diffuse = saturate(dot(normalWS, mainLight.direction)) * 0.5 + 0.5;
                color.rgb = color.rgb * (mainLight.color * diffuse + GlossaryAmbient(normalWS) * 0.5) + spec * mainLight.color;

                // ── 3) 거품: 바닥/물체와 아주 가까운 곳(얕은 곳)에 노이즈 모양으로 ──
                half foamNoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv * 1.6 + float2(t * 0.05, 0)).r;
                half foamCutoff = saturate(waterDepth / max(_FoamDistance, 0.001)) * 0.85;
                half foam = smoothstep(foamCutoff - 0.05, foamCutoff + 0.05, foamNoise) * _UseDepth * step(0.0001, _FoamDistance);
                color.rgb = lerp(color.rgb, _FoamColor.rgb, foam);
                color.a = saturate(max(color.a, foam));
                return color;
            }
            ENDHLSL
        }
    }
}
