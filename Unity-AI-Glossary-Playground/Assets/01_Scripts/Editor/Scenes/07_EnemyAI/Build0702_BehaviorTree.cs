using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0702_BehaviorTree
{
    [GlossaryScene("0702")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0702_BehaviorTree")]
    public static void Build()
    {
        var ctx = NewScene3D("0702", "WASD : 이동   Space : 공격(4m 안, 10%)   H : 보스 체력 -10%   R : 보스 리셋\n아래 트리에서 이번 프레임에 평가된 노드가 색으로 표시됨\n(초록 Success · 노랑 Running · 빨강 Failure)\n체력 50% 이하가 되면 '분노 패턴' 가지가 선택되는 것을 관찰");
        TopCamera(ctx);

        var player = Player(new Vector3(0f, 0f, -8f));

        var boss = Prim(PrimitiveType.Capsule, "Boss", new Vector3(4f, 2f, 4f), Vector3.one * 2f, EnemyColor);
        Object.DestroyImmediate(boss.GetComponent<Collider>());
        boss.layer = LayerMask.NameToLayer("Enemy");
        var eye = Prim(PrimitiveType.Cube, "Eye", new Vector3(0f, 0.5f, 0.45f), new Vector3(0.6f, 0.12f, 0.2f), Color.black, boss.transform);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        Ring(boss.transform, 3f, new Color(1f, 0.4f, 0.3f), "MeleeRange");
        var label = Label(boss.transform, "BOSS", 1.3f, 5f);

        var bt = boss.AddComponent<BossBehaviorTree>();
        Set(bt, "player", player);
        Set(bt, "body", boss.GetComponent<Renderer>());
        Set(bt, "label", label);
        Set(bt, "bulletPrefab", BulletPrefab("BossBullet_0702", new Color(1f, 0.5f, 1f), 0.45f));

        Save(ctx);
    }
}
