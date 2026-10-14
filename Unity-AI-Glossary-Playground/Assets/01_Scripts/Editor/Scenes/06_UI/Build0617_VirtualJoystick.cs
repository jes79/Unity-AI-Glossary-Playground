using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0617_VirtualJoystick
{
    [GlossaryScene("0617")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0617_VirtualJoystick")]
    public static void Build()
    {
        var ctx = NewScene3D("0617",
            "왼쪽 아래 조이스틱을 마우스로 끌기 (터치와 같은 포인터 이벤트) → 기울인 만큼 속도\n오른쪽 아래 점프 버튼 : 누르는 순간 점프\n" +
            "F : 고정 ↔ 플로팅 (누른 곳에 조이스틱이 나타남)   키보드 WASD/Space 도 동작", true, 30f);

        int groundLayer = LayerMask.NameToLayer("Ground");
        Vector3[] boxes = { new(4f, 0.5f, 3f), new(7f, 1f, 6f), new(-5f, 0.75f, 5f), new(-3f, 0.5f, -4f), new(5f, 1.5f, -5f) };
        Vector3[] sizes = { new(3f, 1f, 3f), new(3f, 2f, 3f), new(4f, 1.5f, 2f), new(2f, 1f, 2f), new(3f, 3f, 3f) };
        for (int i = 0; i < boxes.Length; i++)
        {
            var b = Prim(PrimitiveType.Cube, $"Platform_{i}", boxes[i], sizes[i], new Color(0.45f, 0.5f, 0.62f));
            b.layer = groundLayer;
        }

        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, 0f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.white, player.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        var rb = player.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // ── UI ──
        var canvas = CreateCanvas("Mobile UI Canvas");
        var root = canvas.transform;

        // 터치 영역 (투명 Image: alpha 0 이어도 Raycast Target 이면 포인터 이벤트를 받음)
        var area = CreateImage("JoystickTouchArea", root, Vector2.zero, new Vector2(760f, 560f), new Color(1f, 1f, 1f, 0.03f));
        Anchor(area.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero);
        CreateText("AreaHint", area.transform, new Vector2(0f, 90f), new Vector2(700f, 36f), "터치 영역 (플로팅 모드에선 여기 아무 곳이나)", 20f, new Color(1f, 1f, 1f, 0.3f));

        var bg = CreateImage("Background", area.transform, new Vector2(-150f, -90f), new Vector2(260f, 260f), new Color(1f, 1f, 1f, 0.22f), CircleSprite);
        bg.raycastTarget = false;
        var handle = CreateImage("Handle", bg.transform, Vector2.zero, new Vector2(110f, 110f), new Color(1f, 1f, 1f, 0.7f), CircleSprite);
        handle.raycastTarget = false;

        var joystick = area.gameObject.AddComponent<VirtualJoystick>();
        Set(joystick, "background", bg.rectTransform);
        Set(joystick, "handle", handle.rectTransform);

        // 점프 버튼
        var jumpImg = CreateImage("JumpButton", root, Vector2.zero, new Vector2(200f, 200f), new Color(1f, 1f, 1f, 0.35f), CircleSprite);
        Anchor(jumpImg.rectTransform, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-200f, 200f));
        CreateText("Label", jumpImg.transform, Vector2.zero, new Vector2(200f, 80f), "점프", 44f, Color.white).fontStyle = FontStyles.Bold;
        var jump = jumpImg.gameObject.AddComponent<JumpButton>();
        Set(jump, "image", jumpImg);

        var modeBtn = CreateButton("ModeToggle", root, Vector2.zero, new Vector2(300f, 70f), "조이스틱: 고정 (F)", new Color32(70, 80, 105, 255), 24f);
        Anchor((RectTransform)modeBtn.transform, Vector2.one, Vector2.one, new Vector2(-30f, -24f));

        var ctrl = player.AddComponent<JoystickPlayer>();
        Set(ctrl, "joystick", joystick);
        Set(ctrl, "jumpButton", jump);
        Set(ctrl, "cameraTransform", ctx.Camera.transform);
        Set(ctrl, "modeLabel", LabelOf(modeBtn));
        SetLayerMask(ctrl, "groundMask", LayerMask.GetMask("Ground"));
        OnClick(modeBtn, ctrl.ToggleJoystickMode);

        Save(ctx);
    }
}
