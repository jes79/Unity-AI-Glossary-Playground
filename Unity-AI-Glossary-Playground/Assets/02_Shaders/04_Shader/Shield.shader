// 0418 포스필드 / 쉴드 (Depth Intersection)
// 쉴드 표면의 시야 깊이와 그 뒤에 이미 그려진 물체의 깊이(Depth Texture)를 비교
//   차이가 0 에 가까움 = 쉴드가 물체(바닥, 벽, 통과하는 상자)와 닿는 부분 → 밝게 빛남
// + Fresnel 외곽 + 육각형 패턴 + 클릭 지점에서 퍼지는 파문(_HitPos)
Shader "Glossary/Shader/Shield"
{
    Properties
    {
        [HDR] _ShieldColor("Shield Color", Color) = (0.3, 0.9, 2.5, 1)
        _Alpha("Alpha", Range(0, 1)) = 0.6
        _FresnelPower("Fresnel Power", Range(0.5, 8)) = 3
        _IntersectWidth("Intersection Width", Range(0.01, 2)) = 0.35
        [Toggle] _UseIntersection("Use Intersection", Float) = 1
        _HexDensity("Hex Density", Float) = 16
        _HexStrength("Hex Strength", Range(0, 1)) = 0.25
        _HitPos("Hit Position (xyz) / Time (w)", Vector) = (0, -100, 0, -100)
        _HitSpeed("Hit Ripple Speed", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _ShieldColor;
            float _Alpha;
            float _FresnelPower;
            float _IntersectWidth;
            float _UseIntersection;
            float _HexDensity;
            float _HexStrength;
            float4 _HitPos;
            float _HitSpeed;
        CBUFFER_END

        // 육각형 격자에서 가장 가까운 칸 가장자리까지 거리 (0 = 경계선)
        float HexEdgeDistance(float2 p)
        {
            const float2 s = float2(1.0, 1.7320508);
            float4 hC = floor(float4(p, p - float2(0.5, 1.0)) / s.xyxy) + 0.5;
            float4 h = float4(p - hC.xy * s, p - (hC.zw + 0.5) * s);
            float2 q = dot(h.xy, h.xy) < dot(h.zw, h.zw) ? h.xy : h.zw;
            q = abs(q);
            return 0.5 - max(dot(q, s * 0.5), q.x);
        }
        ENDHLSL

        Pass
        {
            Name "Shield"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One   // 가산: 빛나는 막
            ZWrite Off
            Cull Off             // 안쪽 면도 그려서 바닥과 닿는 뒤쪽 경계도 보이게

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
                float3 positionOS : TEXCOORD1;
                half3 normalWS    : TEXCOORD2;
                float eyeDepth    : TEXCOORD3;
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
                output.eyeDepth = -pos.positionVS.z;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // ── 1) Depth Intersection: 뒤에 있는 물체와의 깊이 차이 ──
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float sceneEyeDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float diff = sceneEyeDepth - input.eyeDepth;
                half intersect = 1.0 - saturate(diff / _IntersectWidth);
                intersect = intersect * intersect * intersect * _UseIntersection;

                // ── 2) Fresnel 외곽 (Cull Off 라 안쪽 면은 abs 로) ──
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half fresnel = pow(1.0 - abs(dot(normalWS, viewDirWS)), _FresnelPower);

                // ── 3) 육각형 패턴 (구 좌표 UV) + 위로 흐르는 밝기 ──
                float3 p = normalize(input.positionOS);
                float2 sphereUV = float2(atan2(p.z, p.x) / (2.0 * PI), asin(clamp(p.y, -1.0, 1.0)) / PI);
                float hex = 1.0 - smoothstep(0.0, 0.06, HexEdgeDistance(sphereUV * float2(_HexDensity * 2.0, _HexDensity)));
                float pulse = 0.5 + 0.5 * sin(input.positionWS.y * 3.0 - _Time.y * 2.0);
                half pattern = hex * _HexStrength * (0.4 + 0.6 * pulse);

                // ── 4) 클릭 파문: 맞은 지점에서 반지름이 점점 커지는 고리 ──
                float age = _Time.y - _HitPos.w;
                float dist = distance(input.positionWS, _HitPos.xyz);
                half ring = (1.0 - saturate(abs(dist - age * _HitSpeed) / 0.25)) * saturate(1.0 - age / 1.2);

                half alpha = saturate((0.06 + fresnel * 0.8 + intersect + pattern + ring) * _Alpha);
                return half4(_ShieldColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
