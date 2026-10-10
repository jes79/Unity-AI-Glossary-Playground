using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1108_InspectorAttributes
{
    [GlossaryScene("1108")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1108_InspectorAttributes")]
    public static void Build()
    {
        var ctx = NewScene3D("1108",
            "플레이하면 'AttributeDemoCube' 가 자동 선택됩니다 → Inspector 에서 Header 로 묶인 값과 Range 슬라이더 확인\n" +
            "Inspector 나 오른쪽 화면 슬라이더로 값 변경 (둘이 연동)\n컴포넌트 ⋮ 메뉴 > '기본값으로 되돌리기' (ContextMenu)", true, 16f);
        PatternsBuildUtil.LookAt(ctx.Camera, new Vector3(0f, 7f, -9f), new Vector3(0f, 0.5f, 0f));

        var cube = Prim(PrimitiveType.Cube, "AttributeDemoCube", new Vector3(0f, 0.5f, 0f), Vector3.one, Color.white);
        var demo = cube.AddComponent<InspectorAttributeDemo>();
        Set(demo, "body", cube.GetComponent<Renderer>());
        var pivot = Prim(PrimitiveType.Cylinder, "OrbitCenter", new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.02f, 0.3f), new Color(1f, 0.85f, 0.3f));
        Object.DestroyImmediate(pivot.GetComponent<Collider>());

        var canvas = CreateCanvas("UI Canvas");
        var root = canvas.transform;
        Body(root, "SliderTitle", new Vector2(720f, 420f), new Vector2(400f, 40f), "화면 슬라이더 (= Inspector 값)", 22f, TextAlignmentOptions.Center, TitleColor);
        var speed = CreateSlider("Speed", root, new Vector2(720f, 340f), 360f, 0f, 180f, 45f, "[Range(0,180)] orbitSpeed");
        var radius = CreateSlider("Radius", root, new Vector2(720f, 250f), 360f, 0.5f, 5f, 2.5f, "[Range(0.5,5)] orbitRadius");
        var bob = CreateSlider("Bob", root, new Vector2(720f, 160f), 360f, 0f, 2f, 0.5f, "[Range(0,2)] bobHeight");
        var size = CreateSlider("Size", root, new Vector2(720f, 70f), 360f, 0.3f, 2f, 1f, "[Range(0.3,2)] size");
        var reset = CreateButton("Reset", root, new Vector2(720f, -20f), new Vector2(360f, 60f), "기본값으로 (ContextMenu)", null, 22f);

        InfoBox(root, "CodeInfo", new Vector2(-590f, -250f), new Vector2(720f, 540f), "코드",
            "<color=#9BE89B>[Header(\"이동\")]</color>\n" +
            "<color=#9BE89B>[Tooltip(\"설명\")]</color>\n" +
            "<color=#9BE89B>[SerializeField, Range(0f, 180f)]</color> private float orbitSpeed = 45f;\n" +
            "<color=#9BE89B>[SerializeField, Min(0.1f)]</color> private float bobPeriod = 1f;\n" +
            "<color=#9BE89B>[Space(12)] [Header(\"외형\")]</color>\n" +
            "<color=#9BE89B>[SerializeField, TextArea(2, 4)]</color> private string memo;\n" +
            "<color=#9BE89B>[field: SerializeField]</color> public int Level { get; private set; }\n\n" +
            "public float publicField;   <color=#FF8A65>// 보이긴 하지만 어디서나 수정 가능 → 비권장</color>\n" +
            "<color=#9BE89B>[HideInInspector]</color> public int hiddenValue;   // public 인데 숨김\n" +
            "private float notShown;   // 안 보임\n\n" +
            "<color=#9BE89B>void OnValidate()</color> { 값 검증 }   <color=#9BE89B>[ContextMenu(\"…\")]</color> 메뉴 추가\n" +
            "<b>핵심</b>: 바깥에서 바꿀 필요 없는 값은 <b>private + [SerializeField]</b>", 18f);

        var ui = new GameObject("InspectorAttributeUI").AddComponent<InspectorAttributeUI>();
        Set(ui, "target", demo);
        Set(ui, "speedSlider", speed);
        Set(ui, "radiusSlider", radius);
        Set(ui, "bobSlider", bob);
        Set(ui, "sizeSlider", size);
        OnValueChanged(speed, ui.SetSpeed);
        OnValueChanged(radius, ui.SetRadius);
        OnValueChanged(bob, ui.SetBob);
        OnValueChanged(size, ui.SetSize);
        OnClick(reset, demo.ResetFromButton);

        Save(ctx);
    }
}
