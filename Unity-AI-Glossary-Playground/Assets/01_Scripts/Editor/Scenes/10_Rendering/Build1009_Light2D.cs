using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static GlossaryKit;
using static RenderingBuildUtil;

// URP 2D Light(Light2D)는 Renderer2D 가 필요하지만, 이 프로젝트의 공유 URP 에셋(PC_RPAsset)은 Universal Renderer 하나만 등록되어 있음.
// 카메라가 다른 렌더러를 쓰려면 파이프라인 에셋의 Renderer List 에 추가해야 하므로(공유 설정 수정 금지),
// 여기서는 "Render Texture 라이트 맵 + Multiply 덮개" 대체 구현으로 같은 모습을 만들고 패널에 URP 2D 설정법을 설명함
public static class Build1009_Light2D
{
    [GlossaryScene("1009")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1009_Light2D")]
    public static void Build()
    {
        var ctx = NewScene2D("1009", "마우스 : 손에 든 횃불 이동   클릭 : 횃불 설치 (최대 8)   C : 설치한 횃불 지우기\n" +
                                     "L : 2D 조명 ON/OFF (어둠 덮개)   슬라이더 : 환경광 밝기\n" +
                                     "어두운 던전에서 횃불 주변만 밝아지는 것을 관찰");
        ctx.Canvas = CreateCanvas("Canvas");
        int fx = LayerMask.NameToLayer("TransparentFX"); // 빛 스프라이트 전용 레이어 (메인 카메라는 안 그림)

        var lightAdd = CreateMaterial("L2D1009_LightAdditive", "Glossary/10_Rendering/LightAdditive2D", m => m.SetFloat("_Intensity", 1f));
        var overlayMat = CreateMaterial("L2D1009_LightMapMultiply", "Glossary/10_Rendering/LightMapMultiply2D", m => m.SetFloat("_Boost", 1.3f));

        // ── 던전 바닥/벽 (일반 스프라이트) ──
        for (int x = -10; x <= 10; x++)
            for (int y = -6; y <= 6; y++)
            {
                bool wall = Mathf.Abs(x) == 10 || Mathf.Abs(y) == 6 || (x == 3 && y > -2) || (y == 1 && x < -3);
                var c = wall ? new Color(0.45f, 0.42f, 0.5f) : ((x + y) % 2 == 0 ? new Color(0.62f, 0.55f, 0.45f) : new Color(0.56f, 0.5f, 0.42f));
                var tile = CreateSpriteObject($"Tile_{x}_{y}", SquareSprite, new Vector3(x, y), new Vector2(0.97f, 0.97f), c);
                tile.sortingLayerName = wall ? "Default" : "Background";
            }
        Color[] propColors = { new Color(0.8f, 0.6f, 0.2f), new Color(0.3f, 0.6f, 0.9f), new Color(0.9f, 0.3f, 0.3f), new Color(0.4f, 0.85f, 0.4f) };
        for (int i = 0; i < 10; i++)
        {
            var p = CreateSpriteObject($"Prop_{i}", i % 2 == 0 ? SquareSprite : CircleSprite,
                new Vector3((i * 5.3f % 17f) - 8.5f, (i * 3.1f % 9f) - 4.5f), Vector2.one * 0.7f, propColors[i % 4], null, 2);
            p.sortingLayerName = "Characters";
        }

        // ── 횃불 원본: 막대 + 불꽃(메인 카메라) + 빛 스프라이트(TransparentFX, 라이트 카메라) ──
        GameObject MakeTorch(string name, Vector3 pos, float radius, Color glowColor)
        {
            var root = new GameObject(name);
            root.transform.position = pos;
            var stick = CreateSpriteObject("Stick", SquareSprite, new Vector3(0f, -0.25f), new Vector2(0.12f, 0.5f), new Color(0.4f, 0.25f, 0.12f), root.transform, 5);
            stick.sortingLayerName = "Characters";
            var flame = CreateSpriteObject("Flame", CircleSprite, new Vector3(0f, 0.1f), new Vector2(0.3f, 0.4f), new Color(1f, 0.75f, 0.2f), root.transform, 6);
            flame.sortingLayerName = "Characters";
            var glow = CreateSpriteObject("Glow (Light)", SoftCircleSprite, Vector3.zero, new Vector2(radius * 2f, radius * 2f), glowColor, root.transform);
            glow.gameObject.layer = fx;
            glow.sharedMaterial = lightAdd;
            return root;
        }

        var torchColor = new Color(1f, 0.6f, 0.25f);
        MakeTorch("Wall Torch 1", new Vector3(-7f, 3f), 3f, torchColor);
        MakeTorch("Wall Torch 2", new Vector3(6f, -3.5f), 3f, torchColor);
        MakeTorch("Blue Crystal", new Vector3(7f, 4f), 2.5f, new Color(0.3f, 0.6f, 1f));
        var template = MakeTorch("Torch Template (클릭 설치용 원본)", new Vector3(0f, 0f), 2.5f, torchColor);
        template.SetActive(false);
        var hand = MakeTorch("Player Torch", Vector3.zero, 3.5f, new Color(1f, 0.85f, 0.6f));

        // ── 카메라 구성 ──
        ctx.Camera.cullingMask = ~(1 << fx); // 메인 카메라는 빛 스프라이트를 그리지 않음
        var lightCamGo = new GameObject("Light Camera (→ Render Texture)");
        lightCamGo.transform.SetParent(ctx.Camera.transform, false);
        var lightCam = lightCamGo.AddComponent<Camera>();
        lightCamGo.AddComponent<UniversalAdditionalCameraData>();
        lightCam.orthographic = true;
        lightCam.orthographicSize = ctx.Camera.orthographicSize;
        lightCam.cullingMask = 1 << fx;
        lightCam.clearFlags = CameraClearFlags.SolidColor;
        lightCam.backgroundColor = new Color(0.08f, 0.08f, 0.16f); // 환경광
        lightCam.nearClipPlane = 0.1f;
        lightCam.farClipPlane = 50f;
        lightCam.depth = -10; // 메인 카메라보다 먼저

        // 화면 덮개 Quad: 라이트 맵 x 장면 (가장 위 정렬)
        var overlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
        overlay.name = "Lighting Overlay (Multiply)";
        Object.DestroyImmediate(overlay.GetComponent<Collider>());
        overlay.transform.SetParent(ctx.Camera.transform, false);
        overlay.transform.localPosition = new Vector3(0f, 0f, 1f);
        overlay.transform.localScale = new Vector3(19.2f, 10.8f, 1f);
        var overlayRenderer = overlay.GetComponent<MeshRenderer>();
        overlayRenderer.sharedMaterial = overlayMat;
        overlayRenderer.sortingLayerName = "Foreground";
        overlayRenderer.sortingOrder = 1000;
        overlayRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var demo = new GameObject("TorchLight2DFallback").AddComponent<TorchLight2DFallback>();
        Set(demo, "mainCam", ctx.Camera);
        Set(demo, "lightCam", lightCam);
        Set(demo, "overlay", overlayRenderer);
        Set(demo, "playerTorch", hand.transform);
        Set(demo, "torchTemplate", template);

        var b = CreateButtonRow(ctx.Canvas.transform, 60f, "L 조명 ON/OFF", "C 횃불 지우기");
        OnClick(b[0], demo.ToggleLighting);
        OnClick(b[1], demo.ClearTorches);
        var slider = CreateSlider("Ambient Slider", ctx.Canvas.transform, new Vector2(-220f, 180f), 360f, 0f, 1f, 0.25f, "환경광 밝기");
        AnchorBottomRight(slider.GetComponent<RectTransform>());
        OnValueChanged(slider, demo.SetAmbient);

        HowToPanel(ctx.Canvas.transform, "진짜 URP 2D Light 로 하는 법",
            "이 프로젝트 카메라는 <b>Universal(3D) Renderer</b> → Light2D 가 안 보임.\n" +
            "그래서 같은 원리(빛 텍스처 x 장면)로 대체 구현함.\n\n" +
            "1. Create > Rendering > URP > <b>2D Renderer</b>\n" +
            "2. URP Asset > Renderer List 에 추가\n   (또는 2D 전용 URP Asset 을 만들어 Graphics 에 지정)\n" +
            "3. Camera > Rendering > <b>Renderer</b> = 2D Renderer\n" +
            "4. 스프라이트 머티리얼 = <b>Sprite-Lit-Default</b>\n" +
            "5. Light 2D (<b>Global</b>) Intensity 0.1 → 어두운 던전\n" +
            "6. 횃불마다 Light 2D (<b>Point</b>) Radius 3, 주황색\n" +
            "7. 깜빡임: intensity / pointLightOuterRadius 를 노이즈로\n\n" +
            "<color=#9BE89B>대체 구현 구조</color>  Light Camera → RT(라이트 맵)\n→ 화면 Quad 가 Multiply 로 덮음",
            580f, 520f);

        Save(ctx);
    }
}
