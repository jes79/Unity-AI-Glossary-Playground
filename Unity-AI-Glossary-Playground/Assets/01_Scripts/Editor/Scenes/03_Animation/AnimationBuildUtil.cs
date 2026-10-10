using System.Collections.Generic;
using GlossaryDemo.AnimationDemo;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;

// 03_Animation 빌더 공용 헬퍼
// - 프리미티브로 만든 간단한 캐릭터(제네릭 리그)
// - AnimationClip 을 코드로 키프레임 작성 (AnimationUtility.SetEditorCurve)
// - AnimatorController 생성, 상태 다이어그램 UI, 설명 패널
public static class AnimationBuildUtil
{
    public const string Sub = "Animation";
    public static string Folder => $"{GeneratedFolder}/{Sub}";

    // 리그 본 경로 (Animator 루트 기준). 클립의 커브는 이 경로로 본을 찾음
    public const string Hips = "Hips";
    public const string Chest = "Hips/Chest";
    public const string Head = "Hips/Chest/Head";
    public const string ArmL = "Hips/Chest/ArmL";
    public const string ArmR = "Hips/Chest/ArmR";
    public const string LegL = "Hips/LegL";
    public const string LegR = "Hips/LegR";

    public static readonly Color SkinColor = new Color32(255, 214, 170, 255);

    public class Rig
    {
        public GameObject Root;
        public Animator Animator;
        public Transform Hips, Chest, Head, ArmL, ArmR, LegL, LegR;
        public Transform Sword; // withSword = true 일 때만
    }

    // 캐릭터는 +Z 를 바라봄. 엉덩이(Hips) 높이 1, 발바닥이 y = 0
    public static Rig CreateRig(string name, Vector3 pos, Color color, Transform parent = null, bool withSword = false, float yaw = 0f)
    {
        var root = new GameObject(name);
        if (parent != null) root.transform.SetParent(parent, false);
        root.transform.localPosition = pos;
        root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        var r = new Rig { Root = root };
        Color dark = color * 0.75f;
        dark.a = 1f;

        r.Hips = Bone("Hips", root.transform, new Vector3(0f, 1f, 0f));
        Visual(PrimitiveType.Cube, r.Hips, new Vector3(0f, 0.02f, 0f), new Vector3(0.5f, 0.22f, 0.3f), dark);

        r.Chest = Bone("Chest", r.Hips, new Vector3(0f, 0.12f, 0f));
        Visual(PrimitiveType.Cube, r.Chest, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.32f), color);

        r.Head = Bone("Head", r.Chest, new Vector3(0f, 0.62f, 0f));
        Visual(PrimitiveType.Sphere, r.Head, new Vector3(0f, 0.2f, 0f), Vector3.one * 0.4f, SkinColor);
        // 바라보는 방향 표시용 눈(바이저)
        Visual(PrimitiveType.Cube, r.Head, new Vector3(0f, 0.24f, 0.17f), new Vector3(0.28f, 0.08f, 0.1f), new Color(0.1f, 0.1f, 0.15f));

        r.ArmL = Bone("ArmL", r.Chest, new Vector3(-0.39f, 0.56f, 0f));
        Visual(PrimitiveType.Cube, r.ArmL, new Vector3(0f, -0.3f, 0f), new Vector3(0.16f, 0.62f, 0.16f), SkinColor);
        r.ArmR = Bone("ArmR", r.Chest, new Vector3(0.39f, 0.56f, 0f));
        Visual(PrimitiveType.Cube, r.ArmR, new Vector3(0f, -0.3f, 0f), new Vector3(0.16f, 0.62f, 0.16f), SkinColor);

        r.LegL = Bone("LegL", r.Hips, new Vector3(-0.14f, -0.05f, 0f));
        Visual(PrimitiveType.Cube, r.LegL, new Vector3(0f, -0.475f, 0f), new Vector3(0.2f, 0.95f, 0.2f), dark);
        r.LegR = Bone("LegR", r.Hips, new Vector3(0.14f, -0.05f, 0f));
        Visual(PrimitiveType.Cube, r.LegR, new Vector3(0f, -0.475f, 0f), new Vector3(0.2f, 0.95f, 0.2f), dark);

        if (withSword)
        {
            // 오른손 끝에서 아래(팔 방향)로 뻗은 칼
            r.Sword = Bone("Sword", r.ArmR, new Vector3(0f, -0.6f, 0f));
            Visual(PrimitiveType.Cube, r.Sword, new Vector3(0f, -0.45f, 0f), new Vector3(0.06f, 0.9f, 0.14f), new Color(0.85f, 0.88f, 0.95f));
        }

        r.Animator = root.AddComponent<Animator>();
        return r;
    }

    public static Transform Bone(string name, Transform parent, Vector3 localPos)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = localPos;
        return t;
    }

    // 콜라이더 없는 프리미티브 (본의 겉모습)
    public static GameObject Visual(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Color color, string name = "Mesh")
    {
        var go = Prim(type, name, localPos, scale, color, parent);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    // ───────────────────────── 클립 작성 ─────────────────────────

    public class ClipBuilder
    {
        public readonly AnimationClip Clip;
        private readonly string fileName;

        public ClipBuilder(string name, float frameRate = 30f)
        {
            Clip = new AnimationClip { name = name, frameRate = frameRate };
            fileName = name;
        }

        // 회전(오일러) 키. localEulerAnglesRaw 는 360도 이상 값도 그대로 보간 (구르기 등)
        public ClipBuilder Rot(string path, float[] times, params Vector3[] eulers)
        {
            Curve(path, typeof(Transform), "localEulerAnglesRaw.x", times, Pick(eulers, 0));
            Curve(path, typeof(Transform), "localEulerAnglesRaw.y", times, Pick(eulers, 1));
            Curve(path, typeof(Transform), "localEulerAnglesRaw.z", times, Pick(eulers, 2));
            return this;
        }

        public ClipBuilder Pos(string path, float[] times, params Vector3[] positions)
        {
            Curve(path, typeof(Transform), "m_LocalPosition.x", times, Pick(positions, 0));
            Curve(path, typeof(Transform), "m_LocalPosition.y", times, Pick(positions, 1));
            Curve(path, typeof(Transform), "m_LocalPosition.z", times, Pick(positions, 2));
            return this;
        }

        public ClipBuilder Scale(string path, float[] times, params Vector3[] scales)
        {
            Curve(path, typeof(Transform), "m_LocalScale.x", times, Pick(scales, 0));
            Curve(path, typeof(Transform), "m_LocalScale.y", times, Pick(scales, 1));
            Curve(path, typeof(Transform), "m_LocalScale.z", times, Pick(scales, 2));
            return this;
        }

        public ClipBuilder Curve(string path, System.Type type, string property, float[] times, float[] values, bool linear = false)
        {
            var keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++) keys[i] = new Keyframe(times[i], values[i]);
            var curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                if (linear)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }
                else
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                }
            }
            AnimationUtility.SetEditorCurve(Clip, EditorCurveBinding.FloatCurve(path, type, property), curve);
            return this;
        }

        public ClipBuilder Loop(bool loop = true)
        {
            var s = AnimationUtility.GetAnimationClipSettings(Clip);
            s.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(Clip, s);
            return this;
        }

        public ClipBuilder Events(params AnimationEvent[] events)
        {
            AnimationUtility.SetAnimationEvents(Clip, events);
            return this;
        }

        public AnimationClip Save()
        {
            return SaveAsset(Clip, Sub, fileName + ".anim");
        }

        private static float[] Pick(Vector3[] v, int axis)
        {
            var r = new float[v.Length];
            for (int i = 0; i < v.Length; i++) r[i] = v[i][axis];
            return r;
        }
    }

    public static float[] T(params float[] times) => times;
    public static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

    // ───────────────────────── 공용 동작 클립 (씬 id 접두사로 파일을 분리) ─────────────────────────
    // 캐릭터 기준: 팔/다리 X 회전 음수 = 앞으로, 왼팔 Z 음수 / 오른팔 Z 양수 = 옆으로 벌림

    public static AnimationClip IdleClip(string prefix)
    {
        var t = T(0f, 0.5f, 1f);
        return new ClipBuilder($"{prefix}_Idle")
            .Pos(Hips, t, V(0, 1f, 0), V(0, 0.97f, 0), V(0, 1f, 0))
            .Rot(Chest, t, V(0, 0, 0), V(4, 0, 0), V(0, 0, 0))
            .Rot(Head, t, V(0, 0, 0), V(-4, 0, 0), V(0, 0, 0))
            .Rot(ArmL, t, V(0, 0, -6), V(0, 0, -10), V(0, 0, -6))
            .Rot(ArmR, t, V(0, 0, 6), V(0, 0, 10), V(0, 0, 6))
            .Rot(LegL, t, V(0, 0, 0), V(0, 0, 0), V(0, 0, 0))
            .Rot(LegR, t, V(0, 0, 0), V(0, 0, 0), V(0, 0, 0))
            .Loop().Save();
    }

    public static AnimationClip WalkClip(string prefix) => Cycle($"{prefix}_Walk", 1.0f, 25f, 20f, 0.03f, 3f);

    public static AnimationClip RunClip(string prefix) => Cycle($"{prefix}_Run", 0.8f, 50f, 50f, 0.08f, 12f);

    // 걷기/뛰기 사이클: 반 주기마다 다리·팔이 반대로 흔들리고 엉덩이가 두 번 출렁임
    private static AnimationClip Cycle(string name, float len, float legSwing, float armSwing, float bob, float lean)
    {
        var t = T(0f, len * 0.5f, len);
        var t4 = T(0f, len * 0.25f, len * 0.5f, len * 0.75f, len);
        return new ClipBuilder(name)
            .Pos(Hips, t4, V(0, 1f, 0), V(0, 1f - bob, 0), V(0, 1f, 0), V(0, 1f - bob, 0), V(0, 1f, 0))
            .Rot(Chest, t, V(lean, 6, 0), V(lean, -6, 0), V(lean, 6, 0))
            .Rot(Head, t, V(-lean * 0.5f, 0, 0), V(-lean * 0.5f, 0, 0), V(-lean * 0.5f, 0, 0))
            .Rot(LegL, t, V(-legSwing, 0, 0), V(legSwing, 0, 0), V(-legSwing, 0, 0))
            .Rot(LegR, t, V(legSwing, 0, 0), V(-legSwing, 0, 0), V(legSwing, 0, 0))
            .Rot(ArmL, t, V(armSwing, 0, -6), V(-armSwing, 0, -6), V(armSwing, 0, -6))
            .Rot(ArmR, t, V(-armSwing, 0, 6), V(armSwing, 0, 6), V(-armSwing, 0, 6))
            .Loop().Save();
    }

    // 점프 0.7초: 웅크림 → 위로 → 착지
    public static AnimationClip JumpClip(string prefix)
    {
        var t = T(0f, 0.1f, 0.35f, 0.6f, 0.7f);
        return new ClipBuilder($"{prefix}_Jump")
            .Pos(Hips, t, V(0, 1f, 0), V(0, 0.82f, 0), V(0, 1.9f, 0), V(0, 1f, 0), V(0, 1f, 0))
            .Rot(Chest, t, V(0, 0, 0), V(15, 0, 0), V(-5, 0, 0), V(10, 0, 0), V(0, 0, 0))
            .Rot(Head, t, V(0, 0, 0), V(0, 0, 0), V(0, 0, 0), V(0, 0, 0), V(0, 0, 0))
            .Rot(LegL, t, V(0, 0, 0), V(-20, 0, 0), V(-55, 0, 0), V(-10, 0, 0), V(0, 0, 0))
            .Rot(LegR, t, V(0, 0, 0), V(-20, 0, 0), V(-25, 0, 0), V(-10, 0, 0), V(0, 0, 0))
            .Rot(ArmL, t, V(0, 0, -6), V(30, 0, -10), V(0, 0, -150), V(0, 0, -40), V(0, 0, -6))
            .Rot(ArmR, t, V(0, 0, 6), V(30, 0, 10), V(0, 0, 150), V(0, 0, 40), V(0, 0, 6))
            .Save();
    }

    // 공격 0.8초 (30fps → 24프레임). 12프레임(0.4초)에 칼이 가장 앞으로 나옴
    public static AnimationClip AttackClip(string prefix, params AnimationEvent[] events)
    {
        var t = T(0f, 0.25f, 0.4f, 0.55f, 0.8f);
        var b = new ClipBuilder($"{prefix}_Attack")
            .Pos(Hips, t, V(0, 1f, 0), V(0, 0.95f, -0.05f), V(0, 0.92f, 0.15f), V(0, 0.93f, 0.12f), V(0, 1f, 0))
            .Rot(Chest, t, V(0, 0, 0), V(-5, -35, 0), V(15, 30, 0), V(12, 25, 0), V(0, 0, 0))
            .Rot(Head, t, V(0, 0, 0), V(0, 20, 0), V(0, -20, 0), V(0, -15, 0), V(0, 0, 0))
            .Rot(ArmR, t, V(0, 0, 6), V(-150, 0, 30), V(-80, 0, 0), V(-60, 0, 0), V(0, 0, 6))
            .Rot(ArmL, t, V(0, 0, -6), V(-30, 0, -20), V(20, 0, -15), V(20, 0, -10), V(0, 0, -6))
            .Rot(LegL, t, V(0, 0, 0), V(-15, 0, 0), V(-30, 0, 0), V(-30, 0, 0), V(0, 0, 0))
            .Rot(LegR, t, V(0, 0, 0), V(10, 0, 0), V(20, 0, 0), V(20, 0, 0), V(0, 0, 0));
        if (events.Length > 0) b.Events(events);
        return b.Save();
    }

    // ───────────────────────── 컨트롤러 ─────────────────────────

    public static AnimatorController NewController(string name)
    {
        EnsureFolder(Folder);
        string path = $"{Folder}/{name}.controller";
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) AssetDatabase.DeleteAsset(path);
        return AnimatorController.CreateAnimatorControllerAtPath(path);
    }

    public static AnimatorState AddState(AnimatorStateMachine sm, string name, Motion motion, Vector3 pos)
    {
        var s = sm.AddState(name, pos);
        s.motion = motion;
        return s;
    }

    // 조건 전환 (Has Exit Time 끔 = 조건이 맞으면 바로 전환)
    public static AnimatorStateTransition Go(AnimatorState from, AnimatorState to, float duration = 0.1f)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = duration;
        return t;
    }

    // 클립이 끝나면 돌아가는 전환 (Has Exit Time 켬)
    public static AnimatorStateTransition After(AnimatorState from, AnimatorState to, float exitTime = 0.95f, float duration = 0.1f)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime = exitTime;
        t.duration = duration;
        return t;
    }

    // ───────────────────────── UI ─────────────────────────

    public static Canvas EnsureCanvas(SceneContext ctx)
    {
        if (ctx.Canvas == null) ctx.Canvas = CreateCanvas("Canvas");
        return ctx.Canvas;
    }

    // 화면 오른쪽 위 설명 패널 (실제 툴/패키지 사용법 등)
    public static TextMeshProUGUI ExplainPanel(Canvas canvas, string title, string body, float width = 560f, float height = 420f, float y = -16f)
    {
        var bg = CreateImage("ExplainPanel", canvas.transform, Vector2.zero, new Vector2(width, height), new Color(0.07f, 0.08f, 0.1f, 0.86f));
        var rt = bg.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-16f, y);
        bg.raycastTarget = false;
        var t = CreateText("Title", bg.transform, Vector2.zero, Vector2.zero, title, 24f, new Color(1f, 0.86f, 0.4f), TextAlignmentOptions.TopLeft);
        t.rectTransform.anchorMin = new Vector2(0f, 1f);
        t.rectTransform.anchorMax = new Vector2(1f, 1f);
        t.rectTransform.pivot = new Vector2(0.5f, 1f);
        t.rectTransform.sizeDelta = new Vector2(-32f, 40f);
        t.rectTransform.anchoredPosition = new Vector2(0f, -12f);
        t.fontStyle = FontStyles.Bold;
        var b = CreateText("Body", bg.transform, Vector2.zero, Vector2.zero, body, 18f, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(b.rectTransform);
        b.rectTransform.offsetMin = new Vector2(16f, 12f);
        b.rectTransform.offsetMax = new Vector2(-16f, -56f);
        b.richText = true;
        return b;
    }

    // 상태 이름 상자들을 가로로 그리고 현재 상태를 강조하는 다이어그램 (화면 아래 가운데)
    public static AnimatorStateDisplay StateDiagram(Canvas canvas, Animator animator, float y, params string[] states)
    {
        return StateDiagram(canvas, animator, 0, y, 0f, states);
    }

    public static AnimatorStateDisplay StateDiagram(Canvas canvas, Animator animator, int layer, float y, float x, params string[] states)
    {
        const float w = 170f, gap = 46f;
        float total = states.Length * w + (states.Length - 1) * gap;
        var root = CreateRect($"StateDiagram_L{layer}", canvas.transform, new Vector2(x, y), new Vector2(total, 60f));
        AnchorBottom(root);
        var boxes = new List<Image>();
        for (int i = 0; i < states.Length; i++)
        {
            float px = -total * 0.5f + w * 0.5f + i * (w + gap);
            var img = CreateImage(states[i], root, new Vector2(px, 0f), new Vector2(w, 56f), PanelColor, RoundedSprite, true);
            img.raycastTarget = false;
            var label = CreateText("Label", img.transform, Vector2.zero, Vector2.zero, states[i], 24f, Color.white);
            Stretch(label.rectTransform);
            boxes.Add(img);
            if (i < states.Length - 1)
                CreateText("Arrow", root, new Vector2(px + w * 0.5f + gap * 0.5f, 0f), new Vector2(gap, 56f), "↔", 26f, SubTextColor);
        }
        var display = root.gameObject.AddComponent<AnimatorStateDisplay>();
        Set(display, "animator", animator);
        SetInt(display, "layer", layer);
        SetStrings(display, "stateNames", states);
        SetArray(display, "boxes", boxes.ToArray());
        return display;
    }

    public static TextMeshProUGUI Label(Canvas canvas, string text, Vector2 anchoredFromBottom, float size = 24f, float width = 600f)
    {
        var t = CreateText("Label", canvas.transform, anchoredFromBottom, new Vector2(width, 40f), text, size, TextColor);
        AnchorBottom(t.rectTransform);
        return t;
    }

    // 3D 씬 공용 카메라: 캐릭터를 옆/앞에서 보는 각도
    public static void FrameCamera(Camera cam, Vector3 pos, Vector3 lookAt)
    {
        cam.transform.position = pos;
        cam.transform.LookAt(lookAt);
    }
}
