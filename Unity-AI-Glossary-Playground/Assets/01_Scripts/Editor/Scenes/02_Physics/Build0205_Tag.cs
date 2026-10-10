using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0205_Tag
{
    [GlossaryScene("0205")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0205_Tag")]
    public static void Build()
    {
        var ctx = NewScene3D("0205",
            "마우스 왼쪽 클릭 : 클릭한 대상을 공격\nF : 태그 검사 ON/OFF\nON 이면 'Enemy' 태그가 붙은 빨간 적만 데미지를 받고, 상자·NPC·플레이어는 무시됩니다. 머리 위 글자가 각 오브젝트의 태그입니다",
            true, 26f);

        void Target(PrimitiveType type, string name, string tag, Vector3 pos, Vector3 scale, Color color)
        {
            var go = Prim(type, name, pos, scale, color);
            go.tag = tag;
            AddHealth(go, 100f, 1.4f);
            Label($"Tag: {tag}", new Vector3(pos.x, pos.y + scale.y * 0.5f + 1.6f, pos.z), 4.5f, tag == "Enemy" ? new Color(1f, 0.6f, 0.6f) : new Color(0.75f, 0.8f, 0.9f));
        }

        Target(PrimitiveType.Capsule, "Enemy_A", "Enemy", new Vector3(-6f, 1f, 3f), Vector3.one, EnemyColor);
        Target(PrimitiveType.Capsule, "Enemy_B", "Enemy", new Vector3(0f, 1f, 5f), Vector3.one, EnemyColor);
        Target(PrimitiveType.Capsule, "Enemy_C", "Enemy", new Vector3(6f, 1f, 3f), Vector3.one, EnemyColor);
        Target(PrimitiveType.Cube, "Crate", "Untagged", new Vector3(-3f, 0.5f, 0f), Vector3.one, new Color32(170, 130, 80, 255));
        Target(PrimitiveType.Capsule, "Villager (NPC)", "Interactable", new Vector3(3f, 1f, 0f), Vector3.one, new Color32(120, 200, 140, 255));
        Target(PrimitiveType.Capsule, "Player", "Player", new Vector3(0f, 1f, -3f), Vector3.one, PlayerColor);

        var gun = Prim(PrimitiveType.Cube, "Gun", new Vector3(0f, 2f, -7f), new Vector3(0.5f, 0.5f, 1f), new Color32(200, 200, 210, 255));
        Object.DestroyImmediate(gun.GetComponent<Collider>());
        var line = Line("ShotLine", 0.06f, new Color(1f, 0.9f, 0.5f));
        line.enabled = false;

        var demo = new GameObject("TagDamageDemo").AddComponent<TagDamageDemo>();
        Set(demo, "shotLine", line);
        Set(demo, "gun", gun.transform);
        Save(ctx);
    }
}
