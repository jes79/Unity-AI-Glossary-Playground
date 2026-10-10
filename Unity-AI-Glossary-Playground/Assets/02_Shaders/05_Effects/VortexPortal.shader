// 0522 VFX Graph 대체 구현: 입자 위치를 CPU 가 아니라 정점 셰이더(GPU)에서 매 프레임 계산
// 메시에는 입자마다 사각형 4정점 + 씨앗값(uv1)만 들어 있고, 움직임은 전부 시간 위상(_SpinPhase, _PullPhase) 기반 수식
// uv0 = 사각형 모서리(-1~1), uv1 = (반지름 위상 0~1, 시작 각도, 랜덤 0~1, 입자 번호)
Shader "Glossary/05_Effects/VortexPortal"
{
    Properties
    {
        _ColorInner ("Inner Color", Color) = (1, 0.85, 1, 1)
        _ColorOuter ("Outer Color", Color) = (0.35, 0.3, 1, 1)
        _Radius ("Radius", Float) = 4
        _SpinPhase ("Spin Phase (script)", Float) = 0
        _Twist ("Spiral Twist", Float) = 7
        _PullPhase ("Pull Phase (script)", Float) = 0
        _Depth ("Funnel Depth", Float) = 2.5
        _Size ("Particle Size", Float) = 0.05
        _VisibleCount ("Visible Count", Float) = 50000
        _Intensity ("Intensity", Float) = 1.2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "VortexUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorInner;
                half4 _ColorOuter;
                float _Radius;
                float _SpinPhase;
                float _Twist;
                float _PullPhase;
                float _Depth;
                float _Size;
                float _VisibleCount;
                float _Intensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 corner     : TEXCOORD0;
                float4 seed       : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 corner     : TEXCOORD0;
                half4 color       : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                // _SpinPhase, _PullPhase: 스크립트가 속도×deltaTime 을 누적해 넘김 (속도를 바꿔도 위치가 튀지 않음)

                // 1) 반지름: 바깥(1) → 안쪽(0)으로 빨려 들어가고, 0 에 닿으면 다시 바깥에서 시작 (frac)
                float r01 = frac(v.seed.x - _PullPhase * (0.6 + 0.8 * v.seed.z));
                float radius = _Radius * (0.12 + 0.88 * r01);

                // 2) 각도: 전체 회전 + 안쪽일수록 더 꼬인 나선 팔 (안으로 흘러들며 저절로 더 빨리 도는 것처럼 보임)
                float angle = v.seed.y + _SpinPhase + _Twist * (1.0 - r01);

                // 3) 깔때기: 중심으로 갈수록 화면 안쪽(+Z)으로 깊어짐, 약간의 두께
                float depth = _Depth * (1.0 - r01) * (1.0 - r01) + (v.seed.z - 0.5) * 0.25 * r01;
                float3 centerOS = float3(cos(angle) * radius, sin(angle) * radius, depth);

                // 4) 빌보드: 뷰 공간에서 모서리 오프셋을 더해 항상 카메라를 향하는 사각형
                float3 centerWS = TransformObjectToWorld(centerOS);
                float3 centerVS = TransformWorldToView(centerWS);
                float visible = step(v.seed.w, _VisibleCount - 0.5); // 개수 조절: 번호가 넘는 입자는 크기 0
                float size = _Size * (0.6 + 0.8 * v.seed.z) * visible;
                centerVS.xy += v.corner * size;
                o.positionCS = TransformWViewToHClip(centerVS);

                // 5) 색: 안쪽은 밝은 분홍, 바깥은 파랑. 가장자리와 중심에서 서서히 페이드
                float fade = smoothstep(1.0, 0.85, r01) * smoothstep(0.0, 0.08, r01);
                half4 col = lerp(_ColorInner, _ColorOuter, r01);
                o.color = col * fade * _Intensity;
                o.corner = v.corner;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float d = length(i.corner);
                half a = saturate(1.0 - d);
                a *= a;
                return half4(i.color.rgb * a, 0);
            }
            ENDHLSL
        }
    }
}
