using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0615_Minimap
{
    [GlossaryScene("0615")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0615_Minimap")]
    public static void Build()
    {
        var ctx = NewScene3D("0615",
            "WASD : 이동 (오른쪽 위 미니맵에서 파란 화살표가 나)\nR : 미니맵 회전(플레이어 방향) ↔ 북쪽 고정   C : 원형 ↔ 사각형\n" +
            "Z/X·휠 : 줌   M : 큰 지도   빨간 점 = 적, 회색 = 장애물", true, 44f);

        int minimapLayer = LayerMask.NameToLayer("Minimap");
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");

        // 메인 카메라는 Minimap 레이어(아이콘)를 그리지 않음
        ctx.Camera.cullingMask = ~(1 << minimapLayer);

        // ── 아이콘용 머티리얼 (Unlit + 알파 클립) ──
        var arrowTex = CreateTexture("Minimap_Arrow", 64, 64, (u, v) =>
        {
            // 위(+v)를 가리키는 삼각형
            float halfWidth = (1f - v) * 0.45f;
            bool inside = v > 0.08f && v < 0.95f && Mathf.Abs(u - 0.5f) < halfWidth;
            return inside ? Color.white : new Color(1f, 1f, 1f, 0f);
        }, false, TextureWrapMode.Clamp);
        var playerIconMat = IconMat("MinimapIcon_Player", new Color(0.3f, 0.7f, 1f), arrowTex);
        var enemyIconMat = IconMat("MinimapIcon_Enemy", new Color(1f, 0.3f, 0.3f), CircleSprite.texture);
        var obstacleIconMat = IconMat("MinimapIcon_Obstacle", new Color(0.75f, 0.75f, 0.8f), null);

        // ── 장애물 ──
        var rng = new System.Random(615);
        var obstacleMat = ColorMat(new Color(0.55f, 0.57f, 0.62f));
        for (int i = 0; i < 16; i++)
        {
            var pos = new Vector3(rng.Next(-18, 19), 0f, rng.Next(-18, 19));
            if (pos.magnitude < 4f) pos += new Vector3(6f, 0f, 6f);
            var size = new Vector3(1.5f + (float)rng.NextDouble() * 3f, 1f + (float)rng.NextDouble() * 2f, 1.5f + (float)rng.NextDouble() * 3f);
            var box = Prim(PrimitiveType.Cube, $"Obstacle_{i:00}", pos + Vector3.up * size.y * 0.5f, size, Color.gray);
            box.GetComponent<Renderer>().sharedMaterial = obstacleMat;
            box.layer = obstacleLayer;
            box.isStatic = true;
            // 장애물 아이콘: 위에서 본 크기 그대로 평면 사각형
            Icon(box.transform, obstacleIconMat, minimapLayer, new Vector3(1f, 1f, 1f), true);
        }

        // 바닥 위 "호수" 표시 (Ground 레이어라 미니맵에도 보임)
        var lake = Prim(PrimitiveType.Cube, "Lake", new Vector3(-10f, 0.01f, 10f), new Vector3(8f, 0.02f, 6f), new Color(0.25f, 0.5f, 0.85f));
        lake.layer = LayerMask.NameToLayer("Ground");
        Object.DestroyImmediate(lake.GetComponent<Collider>());

        // ── 플레이어 ──
        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, 0f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        Object.DestroyImmediate(player.GetComponent<Collider>());
        var cc = player.AddComponent<CharacterController>();
        cc.center = Vector3.zero;
        cc.height = 2f;
        cc.radius = 0.5f;
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.white, player.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        player.AddComponent<MinimapPlayerMover>();
        Icon(player.transform, playerIconMat, minimapLayer, new Vector3(2.4f, 2.4f, 1f), false);

        // ── 적 ──
        for (int i = 0; i < 6; i++)
        {
            var enemy = Prim(PrimitiveType.Sphere, $"Enemy_{i}", new Vector3(rng.Next(-15, 16), 0.6f, rng.Next(-15, 16)), Vector3.one * 1.2f, EnemyColor);
            enemy.layer = LayerMask.NameToLayer("Enemy");
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            var w = enemy.AddComponent<MinimapWanderer>();
            SetFloat(w, "speed", 2f + i * 0.3f);
            Icon(enemy.transform, enemyIconMat, minimapLayer, new Vector3(1.5f, 1.5f, 1f), false);
        }

        // ── Render Texture + 탑뷰 카메라 ──
        var rt = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32) { name = "MinimapRT" };
        rt = SaveAsset(rt, "06_UI", "MinimapRT.renderTexture");

        var mmGo = new GameObject("Minimap Camera (Top View → Render Texture)");
        var mm = mmGo.AddComponent<Camera>();
        mmGo.AddComponent<UniversalAdditionalCameraData>();
        mm.orthographic = true;
        mm.orthographicSize = 14f;
        mm.clearFlags = CameraClearFlags.SolidColor;
        mm.backgroundColor = new Color(0.05f, 0.07f, 0.1f);
        mm.cullingMask = LayerMask.GetMask("Ground", "Obstacle", "Minimap");
        mm.targetTexture = rt;
        mm.nearClipPlane = 0.3f;
        mm.farClipPlane = 60f;
        mm.depth = -2f;
        mmGo.transform.SetPositionAndRotation(new Vector3(0f, 30f, 0f), Quaternion.Euler(90f, 0f, 0f));

        // ── UI: RawImage 로 Render Texture 표시 ──
        var canvas = CreateCanvas("UI Canvas");
        var frame = CreateRect("MinimapFrame", canvas.transform, Vector2.zero, new Vector2(340f, 340f));
        Anchor(frame, Vector2.one, Vector2.one, new Vector2(-40f, -40f));
        frame.pivot = new Vector2(0.5f, 0.5f);
        frame.anchoredPosition = new Vector2(-210f, -210f);
        var border = CreateImage("Border", frame, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.15f));
        Stretch(border.rectTransform, -6f);
        var maskImg = CreateImage("Mask", frame, Vector2.zero, Vector2.zero, Color.white, CircleSprite);
        Stretch(maskImg.rectTransform);
        maskImg.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        var rawRt = CreateRect("MinimapView (RawImage)", maskImg.transform, Vector2.zero, Vector2.zero);
        Stretch(rawRt);
        var raw = rawRt.gameObject.AddComponent<RawImage>();
        raw.texture = rt;
        raw.raycastTarget = false;

        var northPivot = CreateRect("NorthPivot", frame, Vector2.zero, Vector2.zero);
        Stretch(northPivot);
        var north = CreateImage("North", northPivot, Vector2.zero, new Vector2(44f, 44f), new Color32(220, 70, 70, 255), CircleSprite);
        Anchor(north.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero);
        CreateText("N", north.transform, Vector2.zero, new Vector2(44f, 44f), "N", 26f, Color.white).fontStyle = FontStyles.Bold;

        var option = CreateText("OptionLabel", canvas.transform, Vector2.zero, new Vector2(820f, 36f), "", 20f, TextColor, TextAlignmentOptions.Right);
        Anchor(option.rectTransform, Vector2.one, Vector2.one, new Vector2(-40f, -400f));

        var buttons = CreateButtonRow(canvas.transform, 60f, "회전 (R)", "모양 (C)", "큰 지도 (M)");

        var ctrl = new GameObject("MinimapController").AddComponent<MinimapController>();
        Set(ctrl, "minimapCamera", mm);
        Set(ctrl, "mainCamera", ctx.Camera);
        Set(ctrl, "player", player.transform);
        Set(ctrl, "minimapFrame", frame);
        Set(ctrl, "maskImage", maskImg);
        Set(ctrl, "circleSprite", CircleSprite);
        Set(ctrl, "squareSprite", WhiteSprite);
        Set(ctrl, "northArrow", northPivot);
        Set(ctrl, "optionLabel", option);
        OnClick(buttons[0], ctrl.ToggleRotate);
        OnClick(buttons[1], ctrl.ToggleShape);
        OnClick(buttons[2], ctrl.ToggleBigMap);

        Save(ctx);
    }

    // 위를 향한 평면 아이콘 (Minimap 레이어). worldFlat = true 면 부모의 x/z 크기를 그대로 덮음
    private static void Icon(Transform parent, Material mat, int layer, Vector3 scale, bool worldFlat)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "MinimapIcon";
        Object.DestroyImmediate(q.GetComponent<Collider>());
        q.layer = layer;
        q.transform.SetParent(parent, false);
        q.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // 앞면이 위(+Y), 텍스처 위쪽이 부모의 정면(+Z)
        if (worldFlat)
        {
            q.transform.localPosition = new Vector3(0f, 0.55f, 0f); // 상자 윗면 바로 위 (부모 스케일 적용)
            q.transform.localScale = scale;
        }
        else
        {
            q.transform.localPosition = new Vector3(0f, 3f, 0f);
            q.transform.localScale = new Vector3(scale.x / parent.localScale.x, scale.y / parent.localScale.z, 1f);
        }
        var r = q.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
    }

    private static Material IconMat(string name, Color color, Texture tex)
    {
        return CreateMaterial(name, "Universal Render Pipeline/Unlit", m =>
        {
            m.SetColor("_BaseColor", color);
            m.SetTexture("_BaseMap", tex);
            if (tex != null)
            {
                m.SetFloat("_AlphaClip", 1f);
                m.SetFloat("_Cutoff", 0.5f);
                m.EnableKeyword("_ALPHATEST_ON");
            }
        });
    }
}
