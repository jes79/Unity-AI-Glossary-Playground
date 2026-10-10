// 0402 Shader Graph 데모: Shader Graph 노드 레시피와 "똑같은 계산"을 손으로 쓴 HLSL
// _Step 값(1~4)에 따라 레시피의 몇 단계까지 연결된 상태인지 보여 줌
//   1) Color 속성 → Base Color
//   2) Time → Sine → Remap(-1..1 → 0..1) → Lerp(ColorA, ColorB) → Base Color
//   3) Fresnel Effect(Power) × RimColor → Emission
//   4) Tiling And Offset(Time 스크롤) → Sample Texture 2D(노이즈) × Fresnel → Emission
Shader "Glossary/Shader/GraphEquivalent"
{
    Properties
    {
        _ColorA("Color A", Color) = (0.2, 0.45, 1, 1)
        _ColorB("Color B", Color) = (0.75, 0.3, 1, 1)
        _PulseSpeed("Pulse Speed", Float) = 2
        _FresnelPower("Fresnel Power", Range(0.5, 8)) = 3
        [HDR] _RimColor("Rim Color", Color) = (0.4, 1.6, 3, 1)
        _NoiseTex("Noise", 2D) = "white" {}
        _ScrollSpeed("Scroll Speed", Vector) = (0, 0.3, 0, 0)
        _Step("Recipe Step", Range(1, 4)) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "GlossaryShaderCommon.hlsl"

        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);

        CBUFFER_START(UnityPerMaterial)
            half4 _ColorA;
            half4 _ColorB;
            float _PulseSpeed;
            float _FresnelPower;
            half4 _RimColor;
            float4 _NoiseTex_ST;
            float4 _ScrollSpeed;
            float _Step;
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

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                // 1) Color 속성 → Base Color
                half3 baseColor = _ColorA.rgb;

                // 2) Time → Sine → Remap → Lerp
                if (_Step >= 2)
                {
                    float sine = sin(_Time.y * _PulseSpeed);   // Sine 노드 (-1 ~ 1)
                    float t = sine * 0.5 + 0.5;                 // Remap 노드 (0 ~ 1)
                    baseColor = lerp(_ColorA.rgb, _ColorB.rgb, t); // Lerp 노드
                }

                half3 emission = 0;
                // 3) Fresnel Effect = pow(1 - saturate(dot(N, V)), Power)
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                if (_Step >= 3) emission = fresnel * _RimColor.rgb;

                // 4) Tiling And Offset(Time × 속도) → Sample Texture 2D → Multiply
                if (_Step >= 4)
                {
                    float2 uv = input.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw + _Time.y * _ScrollSpeed.xy;
                    half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv).r;
                    emission = _RimColor.rgb * (fresnel + noise * noise * 0.6);
                }

                // Lit Master Stack 처럼: 조명 받은 Base Color + Emission
                half3 color = GlossarySimpleLighting(baseColor, input.positionWS, normalWS) + emission;
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
