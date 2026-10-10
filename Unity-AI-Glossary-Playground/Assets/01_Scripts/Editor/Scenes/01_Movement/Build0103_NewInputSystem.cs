using System.IO;
using GlossaryDemo.Movement;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0103_NewInputSystem
{
    [GlossaryScene("0103")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0103_NewInputSystem")]
    public static void Build()
    {
        var ctx = NewScene3D("0103",
            "Move : WASD / 방향키 / 왼쪽 스틱\nJump : Space / 패드 A(South)   Attack : 마우스 왼쪽 / J / 패드 X(West)\nTab : Player 액션 맵 끄기/켜기\n오른쪽 로그에 어떤 장치가 어떤 액션을 발동했는지 표시됩니다",
            true, 24f);
        ctx.Camera.transform.position = new Vector3(0f, 11f, -13f);
        ctx.Camera.transform.rotation = Quaternion.Euler(42f, 0f, 0f);

        for (int i = 0; i < 5; i++)
            Prim(PrimitiveType.Cube, $"Block_{i}", new Vector3(-6f + i * 3f, 0.25f + i * 0.15f, 5f), new Vector3(2f, 0.5f + i * 0.3f, 2f), new Color32(110, 120, 150, 255));

        var player = PlayerCapsule(new Vector3(0f, 1f, 0f));
        var rb = player.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 공격 때만 보이는 칼 (피벗을 돌려서 휘두름)
        var pivot = new GameObject("SwordPivot").transform;
        pivot.SetParent(player.transform, false);
        var blade = Prim(PrimitiveType.Cube, "Sword", new Vector3(0f, 0f, 1f), new Vector3(0.15f, 0.15f, 1.3f), new Color32(255, 220, 120, 255), pivot);
        Object.DestroyImmediate(blade.GetComponent<Collider>());
        pivot.gameObject.SetActive(false);

        ctx.Canvas = CreateCanvas("DemoCanvas");
        var log = CreateText("EventLog", ctx.Canvas.transform, new Vector2(-30f, -30f), new Vector2(620f, 420f), "<b>입력 이벤트</b>", 22f, TextColor, TextAlignmentOptions.TopLeft);
        log.rectTransform.pivot = new Vector2(1f, 1f);
        AnchorTopRight(log.rectTransform);
        var bindings = CreateText("Bindings", ctx.Canvas.transform, new Vector2(-30f, 30f), new Vector2(760f, 160f), "", 20f, SubTextColor, TextAlignmentOptions.BottomLeft);
        bindings.rectTransform.pivot = new Vector2(1f, 0f);
        AnchorBottomRight(bindings.rectTransform);

        var input = player.AddComponent<InputActionsPlayer>();
        Set(input, "sword", pivot);
        Set(input, "logText", log);
        Set(input, "bindingText", bindings);

        ExportActionsJson();
        Save(ctx);
    }

    // 코드로 만든 액션 구성을 .inputactions 파일로도 저장 → 에디터에서 열어 Input Actions 창으로 구조를 볼 수 있음
    private static void ExportActionsJson()
    {
        string folder = $"{GeneratedFolder}/{Sub}";
        EnsureFolder(folder);
        string path = $"{folder}/0103_PlayerControls.inputactions";
        var asset = InputActionsPlayer.CreateActions();
        File.WriteAllText(path, asset.ToJson());
        Object.DestroyImmediate(asset);
        AssetDatabase.ImportAsset(path);
    }
}
