using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0115_CameraViewTypes
{
    [GlossaryScene("0115")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0115_CameraViewTypes")]
    public static void Build()
    {
        var ctx = NewScene3D("0115",
            "1 탑다운  2 쿼터뷰  3 횡스크롤  4 FPS  5 TPS (아래 버튼도 가능)\n쿼터뷰: 바닥 클릭 = 그 위치로 이동   그 밖: WASD 이동, Space 점프\nFPS/TPS: 마우스로 시선 (Esc 커서 풀기, 클릭 다시 잠금)\n같은 장면이 시점에 따라 얼마나 다르게 보이는지 비교하세요",
            true, 60f);

        int ground = LayerMask.NameToLayer("Ground");
        // 횡스크롤용 z=0 줄 위의 발판
        for (int i = 0; i < 5; i++)
        {
            var b = Prim(PrimitiveType.Cube, $"SideStep_{i}", new Vector3(4f + i * 3f, 0.4f + i * 0.5f, 0f), new Vector3(2f, 0.8f + i, 2f), new Color32(90, 160, 110, 255));
            b.layer = ground;
        }
        // 주변 오브젝트 (나무 대용 기둥, 집 대용 상자)
        for (int i = 0; i < 10; i++)
        {
            var rnd = new System.Random(i * 31 + 7);
            var pos = new Vector3((float)rnd.NextDouble() * 40f - 20f, 0f, (float)rnd.NextDouble() * 30f + 4f);
            Prim(PrimitiveType.Cylinder, $"Tree_{i}", pos + Vector3.up * 1.5f, new Vector3(0.6f, 1.5f, 0.6f), new Color32(120, 90, 60, 255));
            var leaf = Prim(PrimitiveType.Sphere, "Leaves", pos + Vector3.up * 3.5f, Vector3.one * 2.2f, new Color32(70, 150, 80, 255));
            Object.DestroyImmediate(leaf.GetComponent<Collider>());
        }
        Prim(PrimitiveType.Cube, "House", new Vector3(-8f, 1.5f, -6f), new Vector3(4f, 3f, 4f), new Color32(200, 170, 130, 255));
        var roof = Prim(PrimitiveType.Cube, "Roof", new Vector3(-8f, 3.6f, -6f), new Vector3(3f, 3f, 3f), new Color32(170, 70, 60, 255));
        roof.transform.rotation = Quaternion.Euler(0f, 45f, 45f);
        Object.DestroyImmediate(roof.GetComponent<Collider>());

        var player = PlayerCapsule(new Vector3(0f, 1.1f, 0f));
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>().center = Vector3.zero;

        var marker = Prim(PrimitiveType.Cylinder, "ClickMarker", Vector3.zero, new Vector3(0.6f, 0.02f, 0.6f), new Color32(255, 220, 90, 255));
        Object.DestroyImmediate(marker.GetComponent<Collider>());
        marker.SetActive(false);

        var view = player.AddComponent<ViewTypesController>();
        Set(view, "cam", ctx.Camera);
        SetLayerMask(view, "groundMask", LayerMask.GetMask("Ground"));
        Set(view, "clickMarker", marker.transform);
        SetArray(view, "bodyRenderers", player.GetComponentsInChildren<Renderer>());

        ctx.Canvas = CreateCanvas("DemoCanvas");
        var buttons = CreateButtonRow(ctx.Canvas.transform, 60f, "1 탑다운", "2 쿼터뷰", "3 횡스크롤", "4 FPS", "5 TPS");
        for (int i = 0; i < buttons.Length; i++) OnClick(buttons[i], view.SetView, i);

        // 화면 가운데 조준점 (FPS/TPS 에서 의미 있음)
        var cross = CreateText("Crosshair", ctx.Canvas.transform, Vector2.zero, new Vector2(40f, 40f), "+", 36f, new Color(1f, 1f, 1f, 0.6f));
        cross.rectTransform.anchorMin = cross.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        Save(ctx);
    }
}
