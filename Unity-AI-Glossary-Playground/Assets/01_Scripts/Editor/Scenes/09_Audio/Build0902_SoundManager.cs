using GlossaryDemo.AudioDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0902_SoundManager
{
    [GlossaryScene("0902")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0902_SoundManager")]
    public static void Build()
    {
        var ctx = NewScene2D("0902", "1 ~ 4 또는 아래 버튼 : 각 오브젝트가 SoundManager.Play(\"이름\") 호출\n'없는 이름' 버튼 : 등록 안 된 이름 → 경고만 내고 멈추지 않음\n빠르게 눌러 여러 소리가 동시에 나도 끊기지 않는지(보이스 풀) 확인");

        var hit = CreateToneClip("0902_Hit", 180f, 0.15f, Wave.Square, 0.4f, 60f, 0.002f, 0.1f);
        var coin = CreateToneClip("0902_Coin", 988f, 0.3f, Wave.Square, 0.25f, 1319f, 0.002f, 0.2f);
        var jump = CreateToneClip("0902_Jump", 250f, 0.3f, Wave.Sine, 0.5f, 700f, 0.005f, 0.1f);
        var boom = CreateToneClip("0902_Explosion", 80f, 0.9f, Wave.Noise, 0.5f, attack: 0.005f, release: 0.8f);

        // ── 사운드 매니저 (씬에 하나) ──
        var managerGo = new GameObject("SoundManager");
        var manager = managerGo.AddComponent<SoundManager>();
        var entries = new (string name, AudioClip clip, float vol, float pitch)[]
        {
            ("Hit", hit, 0.9f, 0.05f), ("Coin", coin, 0.7f, 0f), ("Jump", jump, 0.8f, 0.05f), ("Explosion", boom, 1f, 0.1f),
        };
        var so = new SerializedObject(manager);
        var arr = so.FindProperty("sounds");
        arr.arraySize = entries.Length;
        for (int i = 0; i < entries.Length; i++)
        {
            var e = arr.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = entries[i].name;
            e.FindPropertyRelative("clip").objectReferenceValue = entries[i].clip;
            e.FindPropertyRelative("volume").floatValue = entries[i].vol;
            e.FindPropertyRelative("pitchRandom").floatValue = entries[i].pitch;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        // ── 소리를 부르는 쪽: 서로 모르는 네 오브젝트 ──
        var canvas = CreateCanvas("Canvas");
        string[] names = { "Hit", "Coin", "Jump", "Explosion" };
        string[] labels = { "적 (Hit)", "코인 (Coin)", "플레이어 (Jump)", "폭탄 (Explosion)" };
        Color[] colors = { EnemyColor, new Color(1f, 0.85f, 0.2f), PlayerColor, new Color(0.6f, 0.6f, 0.65f) };
        var buttons = CreateButtonRow(canvas.transform, 70f, "1  Hit", "2  Coin", "3  Jump", "4  Explosion", "없는 이름");
        SoundManagerDemo first = null;
        for (int i = 0; i < names.Length; i++)
        {
            float x = -2.2f + i * 3f; // 왼쪽 위 정보 패널을 피해 오른쪽으로
            var sr = CreateSpriteObject(labels[i], i == 1 ? CircleSprite : SquareSprite, new Vector3(x, 0.5f), new Vector2(1.4f, 1.4f), colors[i]);
            var caller = sr.gameObject.AddComponent<SoundManagerDemo>();
            SetString(caller, "soundName", names[i]);
            SetInt(caller, "key", (int)(Key.Digit1 + i));
            Set(caller, "visual", sr);
            SetBool(caller, "showStatus", i == 0);
            OnClick(buttons[i], caller.PlaySound);
            if (first == null) first = caller;

            var label = CreateText($"Label_{i}", canvas.transform, Vector2.zero, new Vector2(300f, 40f), labels[i], 26f, TextColor);
            label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchoredPosition = new Vector2(x * 100f, -60f); // 2D 카메라: 1 유닛 = 100px
        }
        OnClick(buttons[4], first.PlayByName, "Missing");

        ExplainPanel(canvas, "사운드 매니저 구조",
            "<color=#9FD0FF>public class SoundManager : MonoBehaviour {\n" +
            "  public static SoundManager Instance;\n" +
            "  [SerializeField] Sound[] sounds; // 이름 + 클립\n" +
            "  void Awake() {\n" +
            "    if (Instance != null) { Destroy(gameObject); return; }\n" +
            "    Instance = this; DontDestroyOnLoad(gameObject);\n  }\n" +
            "  public static void Play(string name) { … }\n}</color>\n\n" +
            "• 이름→클립 딕셔너리, AudioSource 풀 8개 돌려쓰기\n" +
            "• 호출 쪽: <b>SoundManager.Play(\"Hit\");</b> 한 줄\n" +
            "• 오타는 경고 로그 (enum 이나 const string 으로 막을 수도)", 580f, 400f);

        Save(ctx);
    }
}
