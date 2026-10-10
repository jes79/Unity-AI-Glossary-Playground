using System.Collections.Generic;
using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1016_SpriteAtlas
{
    private const int IconCount = 16;

    [GlossaryScene("1016")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1016_SpriteAtlas")]
    public static void Build()
    {
        var ctx = NewSceneUI("1016", "Space / [전환] : 개별 텍스처 ↔ Sprite Atlas 스프라이트\n" +
                                     "같은 그림인데 '사용 중인 텍스처 수'와 Batches 가 크게 달라지는 것을 관찰\n" +
                                     "(아이콘을 일부러 겹쳐 놓아 텍스처가 다르면 묶일 수 없게 함)");
        var canvas = ctx.Canvas.transform;

        // 같은 그림 두 벌: Loose(개별) / Atlas(아틀라스에 넣을 것)
        var loose = new Sprite[IconCount];
        var inAtlas = new Sprite[IconCount];
        var atlasTextures = new List<Object>();
        for (int i = 0; i < IconCount; i++)
        {
            int k = i;
            loose[i] = CreateSprite($"Loose1016_Icon_{i:00}", 64, 64, (u, v) => IconPixel(k, u, v), 64f);
            inAtlas[i] = CreateSprite($"Atlas1016_Icon_{i:00}", 64, 64, (u, v) => IconPixel(k, u, v), 64f);
            atlasTextures.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(inAtlas[i])));
        }

        // Sprite Atlas 에셋 생성 + 아이콘 텍스처 추가 + 미리 패킹
        var atlas = new SpriteAtlas();
        atlas.SetPackingSettings(new SpriteAtlasPackingSettings { blockOffset = 1, padding = 2, enableRotation = false, enableTightPacking = false });
        atlas.SetTextureSettings(new SpriteAtlasTextureSettings { readable = false, generateMipMaps = false, sRGB = true, filterMode = FilterMode.Bilinear });
        atlas.SetIncludeInBuild(true);
        // 압축 포맷이 없는 환경(배치 모드 등)에서도 패킹되도록 비압축 RGBA32 로 지정
        atlas.SetPlatformSettings(new TextureImporterPlatformSettings
        {
            name = "DefaultTexturePlatform",
            format = TextureImporterFormat.RGBA32,
            textureCompression = TextureImporterCompression.Uncompressed,
            maxTextureSize = 2048,
        });
        SaveAsset(atlas, Sub, "Icons1016.spriteatlas");
        atlas.Add(atlasTextures.ToArray());
        EditorUtility.SetDirty(atlas);
        AssetDatabase.SaveAssets();
        SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);

        // 아이콘 그리드 8x6 (살짝 겹침)
        var gridBg = CreateImage("Grid Background", canvas, new Vector2(-30f, -40f), new Vector2(640f, 520f), PanelColor, RoundedSprite, true);
        var icons = new List<Image>();
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 8; x++)
            {
                var img = CreateImage($"Icon_{y}_{x}", gridBg.transform, new Vector2(-252f + x * 72f, 180f - y * 72f), new Vector2(84f, 84f), Color.white, inAtlas[(x + y * 8) % IconCount]);
                img.raycastTarget = false;
                icons.Add(img);
            }

        var demo = new GameObject("SpriteAtlasDemo").AddComponent<SpriteAtlasDemo>();
        Set(demo, "atlas", atlas);
        SetArray(demo, "looseSprites", loose);
        SetArray(demo, "atlasSprites", inAtlas);
        SetArray(demo, "icons", icons.ToArray());
        var mode = EditorSettings.spritePackerMode;
        SetString(demo, "packerModeInfo", mode == SpritePackerMode.Disabled
            ? "Sprite Packer Mode = Disabled → Play 중 Atlas 미적용. Project Settings > Editor > Sprite Packer > Mode 를 'Sprite Atlas V1 - Always Enabled' 로"
            : $"Sprite Packer Mode = {mode}. 아직 packed=false 면 Atlas 에셋 선택 > Pack Preview");

        var b = CreateButtonRow(canvas, 60f, "전환 (Atlas ↔ 개별)");
        OnClick(b[0], demo.Toggle);

        HowToPanel(canvas, "UI 아이콘을 Sprite Atlas 로 묶기",
            "1. Project 창 Create > 2D > <b>Sprite Atlas</b>\n" +
            "2. Objects for Packing 에 아이콘 <b>폴더</b>를 드래그\n" +
            "3. Padding 2, Allow Rotation / Tight Packing 끄기 (UI)\n" +
            "4. <b>Pack Preview</b> 로 한 장으로 묶인 모습 확인\n" +
            "5. Project Settings > Editor > <b>Sprite Packer Mode</b>\n   = Sprite Atlas V1/V2 - Always Enabled (Play 중 적용)\n" +
            "6. Image 컴포넌트는 그대로 원래 스프라이트 사용 → 자동으로 Atlas 텍스처 사용\n\n" +
            "<color=#9BE89B>확인</color>: Frame Debugger 에서 UI 배치 수, Stats 의 Batches\n" +
            "<color=#9BE89B>코드</color>: atlas.GetSprite(\"이름\") 로 꺼내기\n(이 씬은 빌더가 SpriteAtlasExtensions.Add 로 생성)",
            560f, 500f);

        Save(ctx);
    }

    // 아이콘 그림: 모양 4종 x 색 4종
    private static Color IconPixel(int index, float u, float v)
    {
        Color[] palette = { new Color(1f, 0.45f, 0.4f), new Color(0.4f, 0.8f, 0.45f), new Color(0.4f, 0.6f, 1f), new Color(1f, 0.82f, 0.3f) };
        Color c = palette[index % 4];
        float x = u - 0.5f, y = v - 0.5f;
        float r = Mathf.Sqrt(x * x + y * y);
        bool inside;
        switch (index / 4)
        {
            case 0: inside = r < 0.42f; break;                                         // 원
            case 1: inside = Mathf.Abs(x) + Mathf.Abs(y) < 0.45f; break;               // 마름모
            case 2: inside = r < 0.44f && r > 0.26f; break;                            // 고리
            default:                                                                   // 별
                float a = Mathf.Atan2(y, x);
                inside = r < 0.25f + 0.17f * Mathf.Cos(a * 5f);
                break;
        }
        bool border = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) > 0.47f;
        if (border) return new Color(0.1f, 0.1f, 0.12f, 1f);
        return inside ? c : new Color(0.2f, 0.22f, 0.28f, 1f);
    }
}
