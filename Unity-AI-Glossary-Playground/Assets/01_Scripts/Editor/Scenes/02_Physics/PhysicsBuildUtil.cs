using GlossaryDemo.PhysicsDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;

// 02_Physics 빌더 공용 헬퍼
public static class PhysicsBuildUtil
{
    public const string Sub = "Physics"; // Assets/09_Generated/Physics

    public static TextMeshPro Label(string text, Vector3 pos, float fontSize, Color color, Transform parent = null)
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
        go.AddComponent<PhysicsBillboard>();
        return tmp;
    }

    // Rigidbody 플레이어 캡슐 (PhysicsDemoPlayer 포함)
    public static PhysicsDemoPlayer Player(Vector3 pos)
    {
        var go = Prim(PrimitiveType.Capsule, "Player", pos, Vector3.one, PlayerColor);
        go.layer = LayerMask.NameToLayer("Player");
        go.tag = "Player";
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.45f), new Vector3(0.3f, 0.15f, 0.3f), Color.white, go.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        return go.AddComponent<PhysicsDemoPlayer>();
    }

    // 체력 + 머리 위 HP 표시
    public static DemoHealth AddHealth(GameObject go, float maxHp, float labelHeight = 1.4f, bool respawn = true)
    {
        var h = go.AddComponent<DemoHealth>();
        SetFloat(h, "maxHp", maxHp);
        Set(h, "body", go.GetComponent<Renderer>());
        SetBool(h, "respawn", respawn);
        var label = Label($"HP {maxHp:0}", new Vector3(0f, labelHeight, 0f), 4f, Color.white, go.transform);
        // 부모 스케일 영향 제거
        Vector3 s = go.transform.lossyScale;
        label.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
        Set(h, "hpLabel", label);
        return h;
    }

    public static Material TransparentMat(string name, Color color)
    {
        var m = LitMat(name, color);
        MakeTransparent(m);
        return m;
    }

    public static Material LineMat => ParticleMat("PhysicsLine", Color.white, false, WhiteSprite.texture);

    public static LineRenderer Line(string name, float width, Color color)
    {
        var lr = new GameObject(name).AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = width;
        lr.sharedMaterial = LineMat;
        lr.startColor = lr.endColor = color;
        lr.useWorldSpace = true;
        return lr;
    }

    public static PhysicsMaterial PhysicsMat(string name, float bounciness, float friction,
        PhysicsMaterialCombine bounceCombine = PhysicsMaterialCombine.Average, PhysicsMaterialCombine frictionCombine = PhysicsMaterialCombine.Average)
    {
        var m = new PhysicsMaterial(name)
        {
            bounciness = bounciness,
            dynamicFriction = friction,
            staticFriction = friction,
            bounceCombine = bounceCombine,
            frictionCombine = frictionCombine
        };
        return SaveAsset(m, Sub, $"{name}.asset");
    }

    // 오른쪽 위 로그용 텍스트 (데모 캔버스가 없으면 만듦)
    public static TextMeshProUGUI RightPanelText(SceneContext ctx, string name, string text, Vector2 size, float y = -30f)
    {
        if (ctx.Canvas == null) ctx.Canvas = CreateCanvas("DemoCanvas");
        var bg = CreateImage(name + "_Bg", ctx.Canvas.transform, new Vector2(-24f, y), size, new Color(0.07f, 0.08f, 0.1f, 0.8f));
        bg.rectTransform.pivot = new Vector2(1f, 1f);
        AnchorTopRight(bg.rectTransform);
        bg.raycastTarget = false;
        var t = CreateText(name, bg.transform, Vector2.zero, Vector2.zero, text, 20f, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(t.rectTransform, 14f);
        t.richText = true;
        return t;
    }
}
