using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1007_RenderTexture
{
    [GlossaryScene("1007")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1007_RenderTexture")]
    public static void Build()
    {
        var ctx = NewScene3D("1007", "C : CCTV 채널 전환 (CAM 1 / CAM 2)   R : 저해상도 Render Texture 로 교체\n" +
                                     "벽 너머 금고실을 직접 볼 수는 없지만 모니터로는 보임\n" +
                                     "= 다른 카메라 화면을 텍스처로 받아 모니터에 붙인 것", true, 50f);
        ctx.Camera.transform.position = new Vector3(0f, 2.2f, -4f);
        ctx.Camera.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        // 1) Render Texture 에셋 (CCTV 카메라 출력 대상)
        var rt = new RenderTexture(512, 288, 16) { name = "CCTV1007" };
        SaveAsset(rt, Sub, "CCTV1007.renderTexture");

        // 2) 경비실: 책상 + 모니터 (모니터 화면 머티리얼에 Render Texture)
        Prim(PrimitiveType.Cube, "Desk", new Vector3(0f, 0.5f, 2f), new Vector3(4f, 1f, 1.5f), new Color(0.45f, 0.32f, 0.22f));
        Prim(PrimitiveType.Cube, "Monitor Frame", new Vector3(0f, 2.3f, 2.6f), new Vector3(3.4f, 2f, 0.15f), new Color(0.12f, 0.12f, 0.14f));
        var screen = Prim(PrimitiveType.Quad, "Monitor Screen", new Vector3(0f, 2.3f, 2.51f), new Vector3(3.2f, 1.8f, 1f), Color.white);
        var screenMat = UnlitMat("RT1007_MonitorScreen", Color.white);
        screenMat.SetTexture("_BaseMap", rt);
        screen.GetComponent<Renderer>().sharedMaterial = screenMat;
        Prim(PrimitiveType.Cube, "Wall (blocks view)", new Vector3(0f, 2.5f, 6f), new Vector3(30f, 5f, 0.5f), new Color(0.6f, 0.62f, 0.66f));

        // 3) 금고실 (벽 너머): 금고 + 소품 + 돌아다니는 침입자
        Prim(PrimitiveType.Cube, "Vault", new Vector3(0f, 1.5f, 19f), new Vector3(4f, 3f, 2f), new Color(0.75f, 0.65f, 0.25f));
        for (int i = 0; i < 6; i++)
            Prim(PrimitiveType.Cube, $"Crate_{i}", new Vector3(-8f + i * 3.2f, 0.5f, 10f + (i % 2) * 7f), Vector3.one, new Color(0.55f, 0.4f, 0.25f));
        var intruder = Prim(PrimitiveType.Capsule, "Intruder", new Vector3(0f, 1f, 14f), Vector3.one, EnemyColor);
        var lamp = new GameObject("Vault Lamp").AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.range = 15f;
        lamp.intensity = 4f;
        lamp.color = new Color(1f, 0.9f, 0.7f);
        lamp.transform.position = new Vector3(0f, 4f, 14f);

        // 4) CCTV 카메라 2대: targetTexture 를 쓰는 카메라는 화면이 아니라 텍스처에 그림
        var cams = new Camera[2];
        Vector3[] camPos = { new Vector3(-9f, 4.5f, 8f), new Vector3(9f, 4.5f, 21f) };
        for (int i = 0; i < 2; i++)
        {
            var body = Prim(PrimitiveType.Cube, $"CCTV Body {i + 1}", camPos[i], new Vector3(0.4f, 0.4f, 0.8f), new Color(0.2f, 0.2f, 0.22f));
            Object.DestroyImmediate(body.GetComponent<Collider>());
            var camGo = new GameObject($"CCTV Camera {i + 1}");
            camGo.transform.position = camPos[i];
            camGo.transform.LookAt(new Vector3(0f, 0.5f, 15f));
            body.transform.rotation = camGo.transform.rotation;
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            cam.fieldOfView = 60f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.05f);
            cam.targetTexture = i == 0 ? rt : null;
            cam.enabled = i == 0;
            cams[i] = cam;
        }

        var monitor = new GameObject("CctvMonitor").AddComponent<CctvMonitor>();
        SetArray(monitor, "cctvCameras", cams);
        Set(monitor, "renderTexture", rt);
        Set(monitor, "monitorScreen", screen.GetComponent<Renderer>());
        Set(monitor, "intruder", intruder.transform);

        var b = CreateButtonRow(canvas.transform, 60f, "C 채널 전환", "R 해상도");
        OnClick(b[0], monitor.NextChannel);
        OnClick(b[1], monitor.ToggleResolution);

        HowToPanel(canvas.transform, "Render Texture 만드는 법",
            "1. Project 창 Create > <b>Render Texture</b> (예: 512x288)\n" +
            "2. CCTV 위치에 Camera 추가 → <b>Output Texture</b>(Target Texture) 에 1번 지정\n" +
            "3. 모니터용 머티리얼 (URP/Unlit) → Base Map 에 1번 지정\n" +
            "4. 모니터 Quad 에 머티리얼 적용\n\n" +
            "<color=#9BE89B>팁</color>\n· 해상도가 클수록 선명하지만 비용↑\n" +
            "· 안 보이는 CCTV 카메라는 꺼 두기\n· 미니맵, 거울, 포털도 같은 원리",
            520f, 360f);

        Save(ctx);
    }
}
