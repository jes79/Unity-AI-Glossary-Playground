using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0211_HitboxHurtbox
{
    [GlossaryScene("0211")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0211_HitboxHurtbox")]
    public static void Build()
    {
        var ctx = NewScene3D("0211",
            "Space / 클릭 : 검 휘두르기 (0.4초)\nH : 히트박스 '0.1초만' ↔ '휘두르는 내내'   T : 슬로 모션\n빨간 상자 = 히트박스(켜졌을 때만 진하게), 초록 = 허트박스\n0.1초만 켜면 정면 허수아비만 맞고, 내내 켜면 양옆까지 맞습니다. 아래 타임라인의 빨간 구간이 활성 시간",
            true, 24f);
        ctx.Camera.transform.position = new Vector3(0f, 9f, -7f);
        ctx.Camera.transform.rotation = Quaternion.Euler(52f, 0f, 0f);

        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, -1f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");

        // 검: 피벗을 돌려 휘두름. 검날 끝 쪽에 히트박스 상자
        var pivot = new GameObject("SwordPivot").transform;
        pivot.SetParent(player.transform, false);
        pivot.localPosition = new Vector3(0f, 0.1f, 0f);
        var blade = Prim(PrimitiveType.Cube, "Blade", new Vector3(0f, 0f, 1.3f), new Vector3(0.12f, 0.12f, 1.9f), new Color32(220, 225, 235, 255), pivot);
        Object.DestroyImmediate(blade.GetComponent<Collider>());
        var hitbox = Prim(PrimitiveType.Cube, "Hitbox", new Vector3(0f, 0f, 1.6f), new Vector3(0.5f, 0.6f, 1.4f), Color.white, pivot);
        Object.DestroyImmediate(hitbox.GetComponent<Collider>()); // 판정은 OverlapBox 로 직접 함 (Collider 불필요)
        hitbox.GetComponent<Renderer>().sharedMaterial = TransparentMat("HitboxRed", new Color(1f, 0.2f, 0.2f, 0.5f));
        pivot.gameObject.SetActive(false);

        // 허수아비 + 허트박스 (Hitbox 레이어의 트리거 캡슐)
        int hurtLayer = LayerMask.NameToLayer("Hitbox");
        var hurtMat = TransparentMat("HurtboxGreen", new Color(0.3f, 1f, 0.4f, 0.25f));
        float[] angles = { -55f, 0f, 55f };
        string[] names = { "왼쪽 허수아비", "정면 허수아비", "오른쪽 허수아비" };
        for (int i = 0; i < angles.Length; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, angles[i], 0f) * Vector3.forward;
            var dummy = Prim(PrimitiveType.Capsule, names[i], new Vector3(0f, 1f, -1f) + dir * 2.3f, new Vector3(0.7f, 1f, 0.7f), new Color32(190, 160, 110, 255));
            AddHealth(dummy, 300f, 1.4f);
            var hurt = Prim(PrimitiveType.Capsule, "Hurtbox", Vector3.zero, new Vector3(1.25f, 1.05f, 1.25f), Color.white, dummy.transform);
            hurt.layer = hurtLayer;
            hurt.GetComponent<CapsuleCollider>().isTrigger = true;
            hurt.GetComponent<Renderer>().sharedMaterial = hurtMat;
        }

        // 타임라인 UI (0 ~ 0.4초, 빨간 구간 = 히트박스 활성)
        ctx.Canvas = CreateCanvas("DemoCanvas");
        var bar = CreateImage("Timeline", ctx.Canvas.transform, new Vector2(0f, 70f), new Vector2(900f, 34f), new Color(0.15f, 0.17f, 0.22f, 0.95f));
        AnchorBottom(bar.rectTransform);
        var window = CreateImage("ActiveWindow", bar.transform, Vector2.zero, Vector2.zero, new Color(1f, 0.25f, 0.25f, 0.85f));
        var cursor = CreateImage("Cursor", bar.transform, Vector2.zero, new Vector2(6f, 0f), Color.white);
        CreateText("Start", bar.transform, new Vector2(-470f, 0f), new Vector2(80f, 30f), "0초", 18f, SubTextColor);
        CreateText("End", bar.transform, new Vector2(480f, 0f), new Vector2(80f, 30f), "0.4초", 18f, SubTextColor);
        CreateText("Caption", bar.transform, new Vector2(0f, 36f), new Vector2(600f, 30f), "휘두르기 타임라인 (빨강 = 히트박스 ON)", 20f, TextColor);

        var demo = player.AddComponent<SwordHitboxDemo>();
        Set(demo, "swordPivot", pivot);
        Set(demo, "hitbox", hitbox.transform);
        Set(demo, "hitboxRenderer", hitbox.GetComponent<Renderer>());
        SetLayerMask(demo, "hurtboxMask", 1 << hurtLayer);
        Set(demo, "timelineWindow", window.rectTransform);
        Set(demo, "timelineCursor", cursor.rectTransform);
        Save(ctx);
    }
}
