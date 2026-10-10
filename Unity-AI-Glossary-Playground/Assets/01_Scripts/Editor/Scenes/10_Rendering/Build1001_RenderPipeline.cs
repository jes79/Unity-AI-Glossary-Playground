using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1001_RenderPipeline
{
    [GlossaryScene("1001")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1001_RenderPipeline")]
    public static void Build()
    {
        var ctx = NewScene3D("1001", "Space / [고치기] : 분홍 머티리얼 → URP Lit 교체\n" +
                                     "왼쪽 줄은 Built-in 'Standard' 셰이더라 URP 에서 분홍색으로 보임\n" +
                                     "오른쪽 줄은 처음부터 URP/Lit. 상태창에서 현재 파이프라인 확인");
        ctx.Camera.transform.position = new Vector3(0f, 4f, -9f);
        ctx.Camera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        Color[] colors = { new Color(0.9f, 0.4f, 0.3f), new Color(0.3f, 0.7f, 0.4f), new Color(0.35f, 0.5f, 0.95f) };
        PrimitiveType[] shapes = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Capsule };

        // 왼쪽: Built-in Standard (분홍) / 오른쪽: URP Lit (정상)
        var builtin = BuiltinStandardMat("RP1001_BuiltinStandard", colors[0]);
        var urpLit = LitMat("RP1001_UrpLit", colors[0], 0.5f);
        var broken = new Renderer[3];
        for (int i = 0; i < 3; i++)
        {
            var b = Prim(shapes[i], $"Builtin_{shapes[i]}", new Vector3(-4.5f + i * 1.6f, 1f, 0f), Vector3.one, colors[i]);
            broken[i] = b.GetComponent<Renderer>();
            broken[i].sharedMaterial = builtin;
            var g = Prim(shapes[i], $"URP_{shapes[i]}", new Vector3(1.3f + i * 1.6f, 1f, 0f), Vector3.one, colors[i]);
            g.GetComponent<Renderer>().sharedMaterial = LitMat($"RP1001_UrpLit_{i}", colors[i], 0.5f);
        }
        WorldLabel("Built-in Standard 머티리얼", new Vector3(-2.9f, 2.4f, 0f), 3.5f);
        WorldLabel("URP / Lit 머티리얼", new Vector3(2.9f, 2.4f, 0f), 3.5f);

        var info = new GameObject("RenderPipelineInfo").AddComponent<RenderPipelineInfo>();
        SetArray(info, "brokenRenderers", broken);
        Set(info, "builtinMaterial", builtin);
        Set(info, "urpLitMaterial", urpLit);

        var buttons = CreateButtonRow(canvas.transform, 60f, "고치기 (URP Lit)", "다시 분홍으로");
        OnClick(buttons[0], info.Fix);
        OnClick(buttons[1], info.Break);

        HowToPanel(canvas.transform, "Built-in → URP 전환 + 분홍색 고치기",
            "1. Package Manager 에서 <b>Universal RP</b> 설치\n" +
            "2. Project 창 Create > Rendering > <b>URP Asset (with Universal Renderer)</b>\n" +
            "3. Project Settings > <b>Graphics</b> > Default Render Pipeline 에 URP 에셋 지정\n" +
            "   (Quality 탭의 레벨별 Render Pipeline 도 확인 — Quality 가 우선)\n" +
            "4. 이제 Built-in 셰이더 머티리얼이 <color=#F6F>분홍색</color> = 이 파이프라인이 그릴 수 없는 셰이더\n" +
            "5. Window > Rendering > <b>Render Pipeline Converter</b>\n" +
            "   → Built-in to URP 선택, <b>Material Upgrade</b> 체크 → Initialize And Convert\n" +
            "6. 직접 만든 셰이더는 자동 변환 안 됨 → Shader Graph 나 URP HLSL 로 다시 작성\n" +
            "7. 남은 분홍색: 머티리얼 선택 > Shader 를 Universal Render Pipeline/Lit 으로\n\n" +
            "<color=#9BE89B>이 씬의 [고치기]</color> = 5번 컨버터가 하는 일\n(셰이더 교체 + _Color → _BaseColor 복사)",
            600f, 500f);

        Save(ctx);
    }
}
