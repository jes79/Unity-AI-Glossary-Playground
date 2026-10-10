// 0419 트라이플래너 매핑 + 위쪽에만 이끼
// UV 대신 월드 좌표를 X/Y/Z 세 방향에서 투영해 텍스처를 3번 읽고, 법선 방향으로 섞음
//   → UV 가 없거나 늘어난 메쉬(바위, 지형)에도 텍스처가 고르게 입혀짐
// 이끼 마스크 = 월드 법선의 Y(위를 향하는 정도). 바위를 돌려도 이끼는 항상 "위쪽" 면에만 남음
Shader "Glossary/Shader/TriplanarMoss"
{
    Properties
    {
        _RockTex("Rock Texture", 2D) = "gray" {}
        _RockTint("Rock Tint", Color) = (1, 1, 1, 1)
        _MossTex("Moss Texture", 2D) = "white" {}
        _MossTint("Moss Tint", Color) = (1, 1, 1, 1)
        _Tiling("World Tiling", Float) = 0.6
        _BlendSharpness("Triplanar Blend Sharpness", Range(1, 16)) = 4
        _MossAmount("Moss Amount", Range(0, 1)) = 0.5
        _MossSharpness("Moss Edge Sharpness", Range(0.01, 0.5)) = 0.12
        [Toggle] _UseTriplanar("Use Triplanar (off = mesh UV)", Float) = 1
        [Toggle] _UseMoss("Use Moss", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        TEXTURE2D(_RockTex);
        SAMPLER(sampler_RockTex);
        TEXTURE2D(_MossTex);
        SAMPLER(sampler_MossTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _RockTex_ST;
            half4 _RockTint;
            float4 _MossTex_ST;
            half4 _MossTint;
            float _Tiling;
            float _BlendSharpness;
            float _MossAmount;
            float _MossSharpness;
            float _UseTriplanar;
            float _UseMoss;
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
                output.uv = input.uv;
                return output;
            }

            // 트라이플래너 샘플: 세 평면(YZ, XZ, XY)에서 읽어 법선 가중치로 섞기
            half3 SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float3 positionWS, float3 weights)
            {
                float3 p = positionWS * _Tiling;
                half3 cx = SAMPLE_TEXTURE2D(tex, samp, p.zy).rgb; // X 방향에서 본 면
                half3 cy = SAMPLE_TEXTURE2D(tex, samp, p.xz).rgb; // 위/아래에서 본 면
                half3 cz = SAMPLE_TEXTURE2D(tex, samp, p.xy).rgb; // Z 방향에서 본 면
                return cx * weights.x + cy * weights.y + cz * weights.z;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normalWS = normalize(input.normalWS);

                // 법선이 많이 향한 축일수록 그 평면 투영을 더 많이 씀 (Sharpness 로 경계를 날카롭게)
                float3 weights = pow(abs(normalWS), _BlendSharpness);
                weights /= max(weights.x + weights.y + weights.z, 1e-4);

                half3 rock;
                half3 moss;
                if (_UseTriplanar > 0.5)
                {
                    rock = SampleTriplanar(TEXTURE2D_ARGS(_RockTex, sampler_RockTex), input.positionWS, weights);
                    moss = SampleTriplanar(TEXTURE2D_ARGS(_MossTex, sampler_MossTex), input.positionWS, weights);
                }
                else
                {
                    // 비교용: 메쉬 UV 그대로 → 늘어나고 이음새가 보임
                    float2 uv = input.uv * _Tiling * 4.0;
                    rock = SAMPLE_TEXTURE2D(_RockTex, sampler_RockTex, uv).rgb;
                    moss = SAMPLE_TEXTURE2D(_MossTex, sampler_MossTex, uv).rgb;
                }
                rock *= _RockTint.rgb;
                moss *= _MossTint.rgb;

                // 이끼 마스크: 위를 향하는 면(normal.y)일수록 1. 노이즈로 경계를 울퉁불퉁하게
                half noise = GlossaryValueNoise(input.positionWS.xz * 3.0 + input.positionWS.y * 2.0);
                half up = normalWS.y + (noise - 0.5) * 0.25;
                half threshold = 1.0 - _MossAmount * 1.6;
                half mossMask = smoothstep(threshold - _MossSharpness, threshold + _MossSharpness, up) * _UseMoss;

                half3 albedo = lerp(rock, moss, mossMask);
                half3 color = GlossarySimpleLighting(albedo, input.positionWS, normalWS);
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
