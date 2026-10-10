using GlossaryDemo;
using GlossaryDemo.EnemyAI;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static GlossaryKit;

// 07_EnemyAI 빌더 공통 헬퍼 (플레이어 캡슐, 머리 위 라벨, 범위 원, 벽, NavMesh 굽기, 탄 프리팹)
public static class EnemyAIBuildUtil
{
    public const string Sub = "EnemyAI";

    // 위에서 비스듬히 내려다보는 카메라 (AI 데모는 전체 판을 보는 게 중요)
    public static void TopCamera(GlossaryKit.SceneContext ctx, float height = 20f, float back = -15f, float pitch = 55f, float x = 2f)
    {
        ctx.Camera.transform.position = new Vector3(x, height, back);
        ctx.Camera.transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public static AITopDownPlayer Player(Vector3 pos, float speed = 6f)
    {
        var go = Prim(PrimitiveType.Capsule, "Player", pos + Vector3.up, Vector3.one, PlayerColor);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.layer = LayerMask.NameToLayer("Player");
        go.tag = "Player";
        var cc = go.AddComponent<CharacterController>();
        cc.radius = 0.5f;
        cc.height = 2f;
        // 앞쪽 표시 (방향 확인용)
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.45f), new Vector3(0.3f, 0.15f, 0.3f), Color.white, go.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        var p = go.AddComponent<AITopDownPlayer>();
        SetFloat(p, "moveSpeed", speed);
        Set(p, "body", go.GetComponent<Renderer>());
        Label(go.transform, "플레이어", 1.6f, 3f);
        return p;
    }

    // 머리 위 3D 텍스트 (카메라를 향함)
    public static TextMeshPro Label(Transform parent, string text, float y, float size = 4f, Color? color = null)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, y, 0f);
        // 부모 스케일 영향을 지움
        Vector3 s = parent.lossyScale;
        go.transform.localScale = new Vector3(1f / Mathf.Max(0.01f, s.x), 1f / Mathf.Max(0.01f, s.y), 1f / Mathf.Max(0.01f, s.z));
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font = GlossaryKit.Font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color ?? Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(8f, 2f);
        go.AddComponent<AIFaceCamera>();
        return tmp;
    }

    // 바닥에 그리는 원 (탐지 거리, 공격 거리 표시)
    public static LineRenderer Ring(Transform parent, float radius, Color color, string name = "Ring", int segments = 64, float width = 0.08f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        Vector3 s = parent.lossyScale;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius / s.x, (0.05f - parent.position.y) / s.y, Mathf.Sin(a) * radius / s.z));
        }
        lr.startWidth = lr.endWidth = width;
        lr.sharedMaterial = LineMat(color);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return lr;
    }

    public static LineRenderer Line(string name, Color color, float width = 0.1f, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.startWidth = lr.endWidth = width;
        lr.sharedMaterial = LineMat(color);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return lr;
    }

    public static Material LineMat(Color color) => UnlitMat($"AI_Line_{ColorUtility.ToHtmlStringRGBA(color)}", color);

    public static Material TransparentUnlit(string name, Color color)
    {
        var m = UnlitMat(name, color);
        MakeTransparent(m);
        return m;
    }

    public static GameObject Wall(string name, Vector3 center, Vector3 size, Transform parent = null, string layer = "Obstacle")
    {
        var go = Prim(PrimitiveType.Cube, name, center, size, new Color32(110, 116, 132, 255), parent);
        go.layer = LayerMask.NameToLayer(layer);
        go.isStatic = true;
        return go;
    }

    // 바닥 오브젝트에 NavMeshSurface 를 붙이고 굽기 → NavMeshData 를 에셋으로 저장해 씬에서 참조
    // Player / Enemy / 움직이는 장애물(excludeLayers) 은 굽기에서 뺌
    public static NavMeshSurface BakeNavMesh(GameObject surfaceObject, string id, params string[] excludeLayers)
    {
        var surface = surfaceObject.GetComponent<NavMeshSurface>() ?? surfaceObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        int mask = ~(LayerMask.GetMask("Player", "Enemy", "UI") | (excludeLayers.Length > 0 ? LayerMask.GetMask(excludeLayers) : 0));
        surface.layerMask = mask;
        surface.BuildNavMesh();

        var data = surface.navMeshData;
        if (data != null)
        {
            data.name = $"{id}_NavMesh";
            SaveAsset(data, Sub, $"{id}_NavMesh.asset");
            surface.navMeshData = data;
            EditorUtility.SetDirty(surface);
        }
        else
        {
            Debug.LogWarning($"[{id}] NavMesh 굽기 실패");
        }
        return surface;
    }

    // 작은 구 모양 탄 프리팹
    public static AIBullet BulletPrefab(string name, Color color, float size = 0.35f)
    {
        var go = Prim(PrimitiveType.Sphere, name, Vector3.zero, Vector3.one * size, color);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = UnlitMat($"AI_Bullet_{ColorUtility.ToHtmlStringRGBA(color)}", color);
        go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        go.AddComponent<AIBullet>();
        return SavePrefab(go, Sub, name).GetComponent<AIBullet>();
    }

    // 오른쪽 위 큰 안내 텍스트 (UI)
    public static TextMeshProUGUI BigText(GlossaryKit.SceneContext ctx, Vector2 pos, string text, float size = 40f)
    {
        if (ctx.Canvas == null) ctx.Canvas = CreateCanvas("Canvas");
        var t = CreateText("BigText", ctx.Canvas.transform, pos, new Vector2(1000f, 80f), text, size, Color.white);
        AnchorTop(t.rectTransform);
        t.fontStyle = FontStyles.Bold;
        return t;
    }
}
