using GlossaryDemo.Movement;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;

// 01_Movement 빌더 공용 헬퍼 (GlossaryKit 에 없는 것만)
public static class MovementBuildUtil
{
    public const string Sub = "Movement"; // Assets/09_Generated/Movement

    // 월드 공간 글자 (billboard = true 면 항상 카메라를 향함)
    public static TextMeshPro Label3D(string text, Vector3 pos, float fontSize, Color color, Transform parent = null, bool billboard = true)
    {
        var go = new GameObject($"Label_{text.Split('\n')[0]}");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font = GlossaryKit.Font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(10f, 2f);
        if (billboard) go.AddComponent<WorldLabelBillboard>();
        return tmp;
    }

    // 2D 발판 (Ground 레이어 + BoxCollider2D)
    public static SpriteRenderer Platform2D(string name, Vector2 pos, Vector2 size, Color? color = null)
    {
        var p = CreateSpriteObject(name, SquareSprite, pos, size, color ?? GroundColor);
        p.gameObject.layer = LayerMask.NameToLayer("Ground");
        p.gameObject.AddComponent<BoxCollider2D>();
        return p;
    }

    // 2D 플레이어 (Rigidbody2D + BoxCollider2D, 마찰 0 재질 → 벽에 붙어 멈추는 현상 방지)
    public static SpriteRenderer Player2D(Vector2 pos, float gravityScale = 3f)
    {
        var player = CreateSpriteObject("Player", SquareSprite, pos, new Vector2(0.8f, 1f), PlayerColor, order: 1);
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        var rb = player.gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = player.gameObject.AddComponent<BoxCollider2D>();
        col.sharedMaterial = NoFriction2D;
        return player;
    }

    public static PhysicsMaterial2D NoFriction2D
    {
        get
        {
            string path = $"{GeneratedFolder}/{Sub}/NoFriction2D.physicsMaterial2D";
            var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (m != null) return m;
            return SaveAsset(new PhysicsMaterial2D("NoFriction2D") { friction = 0f, bounciness = 0f }, Sub, "NoFriction2D.physicsMaterial2D");
        }
    }

    // 3D 플레이어 캡슐 (색 + 앞쪽 표시용 코). 컴포넌트는 호출한 쪽에서 붙임
    public static GameObject PlayerCapsule(Vector3 pos)
    {
        var player = Prim(PrimitiveType.Capsule, "Player", pos, Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.45f), new Vector3(0.3f, 0.15f, 0.3f), Color.white, player.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        return player;
    }

    // 1 유닛 격자 무늬 스프라이트 (카메라 이동이 눈에 잘 보이게 하는 배경)
    public static Sprite GridSprite => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteFolder}/MovementGrid.png") ??
        CreateSprite("MovementGrid", 64, 64, (u, v) =>
        {
            bool line = u < 0.04f || v < 0.04f;
            return line ? new Color(1f, 1f, 1f, 0.35f) : new Color(1f, 1f, 1f, 0.06f);
        }, 64f);

    public static SpriteRenderer GridBackground(Vector2 center, Vector2 size)
    {
        var sr = CreateSpriteObject("GridBackground", GridSprite, new Vector3(center.x, center.y, 1f), Vector2.one, Color.white, order: -10);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        return sr;
    }

    // 고리 모양 스프라이트 (록온 표시 등)
    public static Sprite RingSprite => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteFolder}/MovementRing.png") ??
        CreateSprite("MovementRing", 128, 128, (u, v) =>
        {
            float d = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
            float a = Mathf.Clamp01((1f - Mathf.Abs(d - 0.82f) / 0.12f) * 4f);
            return new Color(1f, 1f, 1f, a);
        }, 128f);

    // 투명 머티리얼 (URP Lit Transparent)
    public static Material TransparentMat(string name, Color color)
    {
        var m = LitMat(name, color);
        MakeTransparent(m);
        return m;
    }

    // 화면(UI) 위 사각형 테두리. 앵커를 0~1 화면 비율로 바꾸면 그 영역을 표시
    public static RectTransform ScreenOutline(string name, Transform canvas, Color color, float thickness = 3f)
    {
        var root = CreateRect(name, canvas, Vector2.zero, Vector2.zero);
        Stretch(root);
        void Edge(string n, Vector2 aMin, Vector2 aMax, Vector2 size)
        {
            var img = CreateImage(n, root, Vector2.zero, size, color);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
        }
        Edge("Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, thickness));
        Edge("Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness));
        Edge("Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness, 0f));
        Edge("Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(thickness, 0f));
        return root;
    }

    public static void SetVector2(Object target, string field, Vector2 value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).vector2Value = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
