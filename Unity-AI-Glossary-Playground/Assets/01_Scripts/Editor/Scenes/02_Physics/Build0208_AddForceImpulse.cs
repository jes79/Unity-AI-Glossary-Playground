using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0208_AddForceImpulse
{
    [GlossaryScene("0208")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0208_AddForceImpulse")]
    public static void Build()
    {
        var ctx = NewScene3D("0208",
            "왼쪽 클릭 : 그 자리에서 AddExplosionForce (오른쪽 상자 더미를 날려 보세요)\nSpace : 왼쪽 4개 레인에 같은 값으로 AddForce 1번 (ForceMode 만 다름)   H(누르기) : 계속 주기\nR : 전부 제자리로\n각 레인의 앞 상자는 질량 1, 뒤 상자는 질량 5 — 질량에 따라 달라지는지 보세요",
            true, 40f);
        ctx.Camera.transform.position = new Vector3(0f, 15f, -16f);
        ctx.Camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        // ForceMode 비교 레인
        ForceMode[] modes = { ForceMode.Force, ForceMode.Acceleration, ForceMode.Impulse, ForceMode.VelocityChange };
        var laneData = new (ForceMode mode, Rigidbody[] bodies)[modes.Length];
        for (int i = 0; i < modes.Length; i++)
        {
            float x = -11f + i * 2.6f;
            var light = Prim(PrimitiveType.Cube, $"{modes[i]}_m1", new Vector3(x - 0.6f, 0.4f, -7f), Vector3.one * 0.8f, new Color32(120, 200, 255, 255));
            var heavy = Prim(PrimitiveType.Cube, $"{modes[i]}_m5", new Vector3(x + 0.6f, 0.4f, -7f), Vector3.one * 0.8f, new Color32(60, 110, 200, 255));
            var rb1 = light.AddComponent<Rigidbody>(); rb1.mass = 1f;
            var rb5 = heavy.AddComponent<Rigidbody>(); rb5.mass = 5f;
            laneData[i] = (modes[i], new[] { rb1, rb5 });
            Label(modes[i].ToString(), new Vector3(x, 1.6f, -7.5f), 4.5f, Color.white);
            // 레인 표시선
            var lane = Prim(PrimitiveType.Cube, $"Lane_{i}", new Vector3(x, 0.01f, 3f), new Vector3(2.2f, 0.02f, 20f), new Color32(60, 66, 80, 255));
            Object.DestroyImmediate(lane.GetComponent<Collider>());
        }

        // 폭발용 상자 더미 (피라미드)
        int n = 0;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4 - y; x++)
                for (int z = 0; z < 4 - y; z++)
                {
                    var c = Prim(PrimitiveType.Cube, $"Crate_{n++}", new Vector3(5f + x + y * 0.5f, 0.5f + y, 2f + z + y * 0.5f), Vector3.one * 0.95f,
                        Color.HSVToRGB(0.08f + y * 0.05f, 0.55f, 0.85f));
                    c.AddComponent<Rigidbody>().mass = 1f;
                }

        var visual = Prim(PrimitiveType.Sphere, "ExplosionVisual", Vector3.zero, Vector3.one, Color.white);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<Renderer>().sharedMaterial = TransparentMat("ForceExplosion", new Color(1f, 0.6f, 0.2f, 0.3f));
        visual.SetActive(false);

        var demo = new GameObject("ForceDemo").AddComponent<ForceDemo>();
        var so = new SerializedObject(demo);
        var lanes = so.FindProperty("lanes");
        lanes.arraySize = laneData.Length;
        for (int i = 0; i < laneData.Length; i++)
        {
            var el = lanes.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("mode").intValue = (int)laneData[i].mode; // enum 은 intValue 로 실제 값 지정
            var bodies = el.FindPropertyRelative("bodies");
            bodies.arraySize = laneData[i].bodies.Length;
            for (int b = 0; b < laneData[i].bodies.Length; b++) bodies.GetArrayElementAtIndex(b).objectReferenceValue = laneData[i].bodies[b];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        SetLayerMask(demo, "groundMask", LayerMask.GetMask("Ground"));
        Set(demo, "explosionVisual", visual.transform);
        Save(ctx);
    }
}
