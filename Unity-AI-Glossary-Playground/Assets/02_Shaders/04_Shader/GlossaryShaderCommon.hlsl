#ifndef GLOSSARY_SHADER_COMMON_INCLUDED
#define GLOSSARY_SHADER_COMMON_INCLUDED

// 04_Shader 데모 셰이더 공통 함수 (간단 조명, 노이즈)
// Core.hlsl 과 Lighting.hlsl 다음에 include
// 그림자/깊이 보조 패스는 GlossaryShaderPasses.hlsl (CBUFFER 다음에 include)

// ───────────── 간단한 조명 (메인 라이트 램버트 + 그림자 + 환경광) ─────────────
half3 GlossaryAmbient(half3 normalWS)
{
    // 라이트를 굽지 않은 씬에서도 너무 어둡지 않게 최소 환경광을 보장
    return max(SampleSH(normalWS), half3(0.12, 0.13, 0.16));
}

half3 GlossarySimpleLighting(half3 albedo, float3 positionWS, half3 normalWS)
{
    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    Light mainLight = GetMainLight(shadowCoord);
    half ndl = saturate(dot(normalWS, mainLight.direction));
    half3 direct = mainLight.color * (ndl * mainLight.shadowAttenuation * mainLight.distanceAttenuation);
    return albedo * (direct + GlossaryAmbient(normalWS));
}

// ───────────── 텍스처 없이 쓰는 간단한 노이즈 ─────────────
float GlossaryHash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float GlossaryValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = GlossaryHash21(i);
    float b = GlossaryHash21(i + float2(1, 0));
    float c = GlossaryHash21(i + float2(0, 1));
    float d = GlossaryHash21(i + float2(1, 1));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

#endif
