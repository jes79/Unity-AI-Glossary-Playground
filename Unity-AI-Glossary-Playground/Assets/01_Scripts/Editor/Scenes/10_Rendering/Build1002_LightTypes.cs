using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1002_LightTypes
{
    [GlossaryScene("1002")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1002_LightTypes")]
    public static void Build()
    {
        var ctx = NewScene3D("1002", "WASD : 이동   마우스 : 비출 방향   <b>F : 손전등 ON/OFF</b>\n" +
                                     "1 / 2 / 3 : Directional / Point / Spot 켜기·끄기\n" +
                                     "1 로 태양을 끄면 Point·Spot 빛 모양이 잘 보임", true, 26f);
        ctx.Camera.transform.position = new Vector3(0f, 13f, -11f);
        ctx.Camera.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        // 해질녘 느낌으로 태양을 약하게 → 다른 조명이 눈에 띄게
        ctx.Light.intensity = 0.35f;
        ctx.Light.color = new Color(1f, 0.85f, 0.7f);
        RenderSettings.ambientLight = new Color(0.12f, 0.12f, 0.16f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // 소품 (빛이 닿는 모양을 보기 위함)
        for (int i = 0; i < 10; i++)
        {
            float a = i * Mathf.PI * 2f / 10f;
            Prim(i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Cylinder, $"Prop_{i}",
                new Vector3(Mathf.Cos(a) * 8f, 0.75f, Mathf.Sin(a) * 6f), new Vector3(1f, 1.5f, 1f), new Color(0.75f, 0.75f, 0.8f));
        }

        // Point Light 3개: 가로등 전구
        Color[] bulbColors = { new Color(1f, 0.6f, 0.3f), new Color(0.4f, 0.8f, 1f), new Color(0.6f, 1f, 0.5f) };
        var points = new Light[3];
        for (int i = 0; i < 3; i++)
        {
            var pos = new Vector3(-7f + i * 7f, 0f, 4f);
            Prim(PrimitiveType.Cylinder, $"Lamppost_{i}", pos + new Vector3(0f, 1.5f, 0f), new Vector3(0.15f, 1.5f, 0.15f), new Color(0.3f, 0.3f, 0.35f));
            var bulb = Prim(PrimitiveType.Sphere, $"Bulb_{i}", pos + new Vector3(0f, 3.1f, 0f), Vector3.one * 0.35f, bulbColors[i]);
            Object.DestroyImmediate(bulb.GetComponent<Collider>());
            var l = new GameObject($"Point Light_{i}").AddComponent<Light>();
            l.type = LightType.Point;
            l.color = bulbColors[i];
            l.range = 7f;
            l.intensity = 6f;
            l.transform.position = pos + new Vector3(0f, 2.6f, 0f);
            points[i] = l;
        }

        // Spot Light 2개: 무대 조명 (원뿔 모양으로 바닥을 비춤)
        var spots = new Light[2];
        for (int i = 0; i < 2; i++)
        {
            var l = new GameObject($"Spot Light_{i}").AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = i == 0 ? new Color(1f, 0.4f, 0.8f) : new Color(1f, 1f, 0.5f);
            l.range = 14f;
            l.spotAngle = 40f;
            l.intensity = 25f;
            l.shadows = LightShadows.Soft;
            l.transform.position = new Vector3(i == 0 ? -6f : 6f, 7f, -4f);
            l.transform.rotation = Quaternion.LookRotation(new Vector3(i == 0 ? -6f : 6f, 0f, -4f) - l.transform.position + new Vector3(0f, 0f, 0.01f));
            spots[i] = l;
        }

        // 플레이어 + 손전등 (자식 Spot Light)
        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, -2f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        var nose = Prim(PrimitiveType.Cube, "Flashlight Body", new Vector3(0.3f, 0.2f, 0.5f), new Vector3(0.15f, 0.15f, 0.4f), new Color(0.2f, 0.2f, 0.2f), player.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        var flash = new GameObject("Flashlight (Spot)").AddComponent<Light>();
        flash.transform.SetParent(player.transform, false);
        flash.transform.localPosition = new Vector3(0.3f, 0.3f, 0.7f);
        flash.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
        flash.type = LightType.Spot;
        flash.spotAngle = 45f;
        flash.innerSpotAngle = 20f;
        flash.range = 15f;
        flash.intensity = 30f;
        flash.color = new Color(1f, 0.97f, 0.85f);
        flash.shadows = LightShadows.Soft;

        var fp = player.AddComponent<FlashlightPlayer>();
        Set(fp, "flashlight", flash);

        var demo = new GameObject("LightTypesDemo").AddComponent<LightTypesDemo>();
        Set(demo, "directional", ctx.Light);
        SetArray(demo, "points", points);
        SetArray(demo, "spots", spots);
        Set(demo, "player", fp);

        var b = CreateButtonRow(canvas.transform, 60f, "1 Directional", "2 Point", "3 Spot", "F 손전등");
        OnClick(b[0], demo.ToggleDirectional);
        OnClick(b[1], demo.TogglePoints);
        OnClick(b[2], demo.ToggleSpots);
        OnClick(b[3], fp.ToggleFlashlight);

        Save(ctx);
    }
}
