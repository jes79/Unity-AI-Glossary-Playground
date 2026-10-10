#ifndef GLOSSARY_SHADER_PASSES_INCLUDED
#define GLOSSARY_SHADER_PASSES_INCLUDED

// 04_Shader 데모 셰이더의 ShadowCaster / DepthOnly 패스 공용 코드
// Core.hlsl, Lighting.hlsl, GlossaryShaderCommon.hlsl, UnityPerMaterial CBUFFER 다음에 include
//
// 선택 훅 (include 전에 #define 하면 그림자/깊이 패스에도 같은 처리가 적용됨)
//   GLOSSARY_VERTEX_DISPLACE(posWS, posOS, uv) : 버텍스 이동 결과 월드 위치를 돌려주는 식 (풀 흔들림 등)
//   GLOSSARY_DEPTH_CLIP(input)                 : clip() 을 호출하는 문장 (디졸브 등 구멍 뚫는 셰이더)

#ifndef GLOSSARY_VERTEX_DISPLACE
#define GLOSSARY_VERTEX_DISPLACE(posWS, posOS, uv) (posWS)
#endif

#ifndef GLOSSARY_DEPTH_CLIP
#define GLOSSARY_DEPTH_CLIP(input)
#endif

// 메인 라이트 방향 (URP 가 그림자 패스를 그릴 때 설정하는 전역 값)
float3 _LightDirection;

struct GlossaryDepthAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 uv         : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GlossaryDepthVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv         : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 positionOS : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

GlossaryDepthVaryings GlossaryShadowVert(GlossaryDepthAttributes input)
{
    GlossaryDepthVaryings output = (GlossaryDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    positionWS = GLOSSARY_VERTEX_DISPLACE(positionWS, input.positionOS.xyz, input.uv);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
    output.positionCS = ApplyShadowClamping(positionCS);
    output.uv = input.uv;
    output.positionWS = positionWS;
    output.positionOS = input.positionOS.xyz;
    return output;
}

GlossaryDepthVaryings GlossaryDepthVert(GlossaryDepthAttributes input)
{
    GlossaryDepthVaryings output = (GlossaryDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    positionWS = GLOSSARY_VERTEX_DISPLACE(positionWS, input.positionOS.xyz, input.uv);
    output.positionCS = TransformWorldToHClip(positionWS);
    output.uv = input.uv;
    output.positionWS = positionWS;
    output.positionOS = input.positionOS.xyz;
    return output;
}

half4 GlossaryDepthFrag(GlossaryDepthVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    GLOSSARY_DEPTH_CLIP(input);
    return half4(input.positionCS.z, 0, 0, 0);
}

#endif
