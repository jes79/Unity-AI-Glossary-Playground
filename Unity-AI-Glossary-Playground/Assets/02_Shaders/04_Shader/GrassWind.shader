// 0416 버텍스 애니메이션 (Vertex Displacement) - 바람에 흔들리는 풀
// 버텍스 셰이더에서 정점 위치를 sin(시간 + 위치) 로 옆으로 밀어 흔듦
// uv.y(뿌리 0 → 끝 1)의 제곱을 곱해서 뿌리는 고정되고 끝으로 갈수록 크게 휨
Shader "Glossary/Shader/GrassWind"
{
    Properties
    {
        _BottomColor("Bottom Color", Color) = (0.08, 0.25, 0.08, 1)
        _TopColor("Top Color", Color) = (0.55, 0.85, 0.3, 1)
        _WindStrength("Wind Strength", Range(0, 1)) = 0.35
        _WindSpeed("Wind Speed", Range(0, 8)) = 2
        _WindFrequency("Wind Frequency", Range(0, 2)) = 0.6
        _WindDirection("Wind Direction (XZ)", Vector) = (1, 0, 0.3, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Cull Off // 얇은 풀잎은 앞뒤 모두 보이게

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BottomColor;
            half4 _TopColor;
            float _WindStrength;
            float _WindSpeed;
            float _WindFrequency;
            float4 _WindDirection;
        CBUFFER_END

        // 바람 변위 계산 (본 패스, 그림자, 깊이 패스가 모두 같은 함수를 써야 그림자도 같이 흔들림)
        float3 GrassDisplace(float3 positionWS, float2 uv)
        {
            float3 dir = normalize(float3(_WindDirection.x, 0, _WindDirection.z) + float3(0.0001, 0, 0));
            // 위치마다 위상이 달라서 물결처럼 퍼져 나가는 흔들림
            float phase = _Time.y * _WindSpeed + dot(positionWS.xz, dir.xz) * _WindFrequency * 3.0;
            float wave = sin(phase) * 0.7 + sin(phase * 2.3 + 1.3) * 0.3;
            // 큰 돌풍: 느리게 움직이는 노이즈
            float gust = GlossaryValueNoise(positionWS.xz * 0.2 - dir.xz * _Time.y * _WindSpeed * 0.3);
            float bend = uv.y * uv.y; // 뿌리 고정, 끝일수록 크게
            positionWS += dir * (wave * 0.5 + gust * 0.8) * _WindStrength * bend;
            positionWS.y -= _WindStrength * bend * gust * 0.15; // 휘면 살짝 낮아짐
            return positionWS;
        }

        #define GLOSSARY_VERTEX_DISPLACE(posWS, posOS, uv) GrassDisplace(posWS, uv)
        #include "GlossaryShaderPasses.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardGrass"
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
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                positionWS = GrassDisplace(positionWS, input.uv); // ← 버텍스 변위 핵심
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normalWS = normalize(input.normalWS) * IS_FRONT_VFACE(frontFace, 1.0, -1.0);
                half3 albedo = lerp(_BottomColor.rgb, _TopColor.rgb, input.uv.y);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                // 풀은 반투과 느낌을 위해 Half-Lambert (어두운 면도 너무 검지 않게)
                half ndl = dot(normalWS, mainLight.direction) * 0.5 + 0.5;
                half3 color = albedo * (mainLight.color * ndl * mainLight.shadowAttenuation + GlossaryAmbient(half3(0, 1, 0)));
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
