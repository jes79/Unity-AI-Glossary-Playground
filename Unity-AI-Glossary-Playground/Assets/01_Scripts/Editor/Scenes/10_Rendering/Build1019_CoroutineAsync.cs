using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1019_CoroutineAsync
{
    [GlossaryScene("1019")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1019_CoroutineAsync")]
    public static void Build()
    {
        var ctx = NewScene3D("1019", "Space / [시작] : 두 문 모두 3초 카운트다운 시작\n" +
                                     "C : 기다리는 도중 취소   R : 문 닫기(리셋)\n" +
                                     "왼쪽 = 코루틴, 오른쪽 = async/await. 기다리는 동안 공이 계속 도는지(안 멈춤) 관찰", true, 30f);
        ctx.Camera.transform.position = new Vector3(0f, 5f, -11f);
        ctx.Camera.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        var frameColor = new Color(0.35f, 0.35f, 0.4f);
        Transform Door(string name, float x, Color color, out TMPro.TextMeshPro label)
        {
            Prim(PrimitiveType.Cube, name + " Frame L", new Vector3(x - 1.6f, 1.75f, 0f), new Vector3(0.4f, 3.5f, 0.6f), frameColor);
            Prim(PrimitiveType.Cube, name + " Frame R", new Vector3(x + 1.6f, 1.75f, 0f), new Vector3(0.4f, 3.5f, 0.6f), frameColor);
            Prim(PrimitiveType.Cube, name + " Frame Top", new Vector3(x, 3.7f, 0f), new Vector3(3.6f, 0.4f, 0.6f), frameColor);
            var door = Prim(PrimitiveType.Cube, name, new Vector3(x, 1.5f, 0f), new Vector3(2.8f, 3f, 0.2f), color);
            label = WorldLabel(name, new Vector3(x, 5.4f, -0.6f), 5f);
            return door.transform;
        }
        var cDoor = Door("Coroutine Door", -3.5f, new Color(0.55f, 0.65f, 0.95f), out var cLabel);
        var aDoor = Door("Async Door", 3.5f, new Color(0.95f, 0.65f, 0.45f), out var aLabel);

        // 문 뒤 보상 + 계속 움직이는 공 (메인 스레드가 멈추지 않음을 보여 줌)
        Prim(PrimitiveType.Sphere, "Treasure L", new Vector3(-3.5f, 0.6f, 1.5f), Vector3.one * 1.2f, new Color(1f, 0.85f, 0.3f));
        Prim(PrimitiveType.Sphere, "Treasure R", new Vector3(3.5f, 0.6f, 1.5f), Vector3.one * 1.2f, new Color(1f, 0.85f, 0.3f));
        var spinnerRoot = new GameObject("Spinner (Animator 없이 회전)");
        spinnerRoot.transform.position = new Vector3(0f, 0.5f, -3f);
        Prim(PrimitiveType.Sphere, "Ball", new Vector3(1.2f, 0f, 0f), Vector3.one * 0.5f, PlayerColor, spinnerRoot.transform);
        spinnerRoot.AddComponent<SpinWhileWaiting>();

        var demo = new GameObject("DelayedDoorDemo").AddComponent<DelayedDoorDemo>();
        Set(demo, "coroutineDoor", cDoor);
        Set(demo, "asyncDoor", aDoor);
        Set(demo, "coroutineLabel", cLabel);
        Set(demo, "asyncLabel", aLabel);

        var b = CreateButtonRow(canvas.transform, 60f, "시작 (3초)", "취소", "리셋");
        OnClick(b[0], demo.StartBoth);
        OnClick(b[1], demo.CancelBoth);
        OnClick(b[2], demo.ResetDoors);

        HowToPanel(canvas.transform, "두 방법 코드 비교",
            "<color=#9BE89B>코루틴</color>\n<noparse>IEnumerator OpenLater() {\n  yield return new WaitForSeconds(3f);\n  OpenDoor();\n}\nStartCoroutine(OpenLater());   // StopCoroutine 으로 취소</noparse>\n\n" +
            "<color=#FFB070>async / await</color> (Unity 6 Awaitable)\n<noparse>async Awaitable OpenLater(CancellationToken t) {\n  await Awaitable.WaitForSecondsAsync(3f, t);\n  OpenDoor();\n}\n_ = OpenLater(destroyCancellationToken);</noparse>\n\n" +
            "· 코루틴: MonoBehaviour 필요, 오브젝트가 꺼지면 자동 중단\n" +
            "· async: 반환값·try/catch 가 자연스러움, 취소는 토큰으로\n" +
            "· Task.Delay 는 Time.timeScale·일시정지를 무시 → Awaitable 권장",
            600f, 520f);

        Save(ctx);
    }
}
