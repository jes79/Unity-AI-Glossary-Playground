using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0712_PredictiveAim
{
    [GlossaryScene("0712")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0712_PredictiveAim")]
    public static void Build()
    {
        var ctx = NewScene3D("0712", "P : 예측 조준 ↔ 현재 위치 조준   C : 자동 원 운동 ON/OFF (OFF면 WASD 직접 이동)\nR : 명중 기록 초기화\n노란 원 = 포탑이 계산한 '만날 지점'. 두 방식의 명중률을 비교해 보세요");
        TopCamera(ctx);

        var player = Player(new Vector3(8f, 0f, 0f), 6f);

        var turret = Prim(PrimitiveType.Cylinder, "Turret", new Vector3(0f, 0.5f, 0f), new Vector3(2f, 0.5f, 2f), new Color32(120, 125, 140, 255));
        var head = new GameObject("Head").transform;
        head.SetParent(turret.transform, false);
        head.localPosition = new Vector3(0f, 2f, 0f);
        head.localScale = new Vector3(0.5f, 2f, 0.5f); // 부모 스케일 상쇄
        var dome = Prim(PrimitiveType.Sphere, "Dome", Vector3.zero, Vector3.one * 1.4f, EnemyColor, head);
        var barrel = Prim(PrimitiveType.Cube, "Barrel", new Vector3(0f, 0f, 1f), new Vector3(0.3f, 0.3f, 1.6f), new Color32(60, 60, 70, 255), head);
        Object.DestroyImmediate(dome.GetComponent<Collider>());
        Object.DestroyImmediate(barrel.GetComponent<Collider>());
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(head, false);
        muzzle.localPosition = new Vector3(0f, 0f, 1.9f);

        var marker = Prim(PrimitiveType.Cylinder, "AimMarker", new Vector3(0f, 0.05f, 0f), new Vector3(0.9f, 0.02f, 0.9f), new Color32(255, 220, 60, 255));
        Object.DestroyImmediate(marker.GetComponent<Collider>());

        Ring(turret.transform, 8f, new Color(0.4f, 0.7f, 1f), "CircleRoute");

        var t = turret.AddComponent<PredictiveTurret>();
        Set(t, "target", player);
        Set(t, "head", head);
        Set(t, "muzzle", muzzle);
        Set(t, "aimMarker", marker.transform);
        Set(t, "bulletPrefab", BulletPrefab("TurretBullet_0712", new Color(1f, 0.6f, 0.2f), 0.35f));
        SetFloat(t, "bulletSpeed", 14f);

        Save(ctx);
    }
}
