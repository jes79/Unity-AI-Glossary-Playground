using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1102_EventObserver
{
    [GlossaryScene("1102")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1102_EventObserver")]
    public static void Build()
    {
        var ctx = NewScene2D("1102",
            "H : 피해 25 (4번 맞으면 사망 → OnPlayerDied 방송)   R : 부활\n1~4 또는 카드 클릭 : 그 리스너 구독 ON/OFF\n" +
            "구독을 끈 리스너는 반응하지 않음 — 플레이어 코드는 하나도 안 바뀜");

        // ── 플레이어 (Subject) ──
        var body = CreateSpriteObject("Player", SquareSprite, new Vector3(-0.3f, 0.3f, 0f), new Vector2(1.4f, 1.4f), new Color(0.3f, 0.65f, 1f), order: 1);
        var subject = body.gameObject.AddComponent<PlayerHealthSubject>();
        Set(subject, "body", body);

        // UnityEvent 로 연결할 사망 파티클
        var psGo = new GameObject("DeathParticles (UnityEvent 연결)");
        psGo.transform.position = body.transform.position;
        var ps = psGo.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.5f;
        main.startLifetime = 0.9f;
        main.startSpeed = 5f;
        main.startSize = 0.25f;
        main.startColor = new Color(0.4f, 0.75f, 1f);
        main.gravityModifier = 0.6f;
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.4f;
        var psr = psGo.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = ParticleMat("Particle_1102_Death", Color.white);
        psr.sortingOrder = 5;
        // 인스펙터의 UnityEvent 칸에 끌어다 놓은 것과 같은 영구 리스너
        UnityEventTools.AddPersistentListener(subject.OnPlayerDiedUnity, ps.Play);

        // ── UI ──
        var canvas = CreateCanvas("UI Canvas");
        var root = canvas.transform;

        var hpBg = Panel(root, "HPBar", new Vector2(-30f, -130f), new Vector2(420f, 34f), new Color(0.1f, 0.1f, 0.12f, 1f));
        var hpFill = Filled(hpBg.transform, "Fill", Vector2.zero, Vector2.zero, new Color(0.9f, 0.3f, 0.3f), Image.FillMethod.Horizontal);
        Stretch(hpFill.rectTransform, 5f);
        var gameOver = CreateText("GameOver", root, new Vector2(-30f, 230f), new Vector2(800f, 120f), "GAME OVER", 96f, new Color(1f, 0.35f, 0.35f));
        gameOver.fontStyle = FontStyles.Bold;

        Body(root, "ListenersTitle", new Vector2(690f, 400f), new Vector2(420f, 40f), "OnPlayerDied 리스너 (Observer)", 24f, TextAlignmentOptions.Center, TitleColor);

        var sfx = CreateToneClip("SFX_1102_Death", 440f, 0.6f, Wave.Saw, 0.35f, 110f);
        var src = new GameObject("DeathSound").AddComponent<AudioSource>();
        src.clip = sfx;
        src.playOnAwake = false;

        var listeners = new DeathListenerBase[4];
        var (c1, s1, b1) = Card(root, 0, "1  UI : GAME OVER 표시");
        var ui = c1.gameObject.AddComponent<GameOverUIListener>();
        Set(ui, "gameOverText", gameOver.gameObject);
        listeners[0] = ui;

        var (c2, s2, b2) = Card(root, 1, "2  사운드 : 효과음");
        var snd = c2.gameObject.AddComponent<DeathSoundListener>();
        Set(snd, "source", src);
        listeners[1] = snd;

        var (c3, s3, b3) = Card(root, 2, "3  카메라 : 흔들림");
        var shake = c3.gameObject.AddComponent<CameraShakeListener>();
        Set(shake, "cameraTransform", ctx.Camera.transform);
        listeners[2] = shake;

        var (c4, s4, b4) = Card(root, 3, "4  통계 : 사망 횟수");
        var counter = CreateText("Counter", c4.transform, new Vector2(110f, -18f), new Vector2(160f, 30f), "사망 0회", 20f, Color.white, TextAlignmentOptions.Right);
        var cnt = c4.gameObject.AddComponent<DeathCounterListener>();
        Set(cnt, "counterText", counter);
        listeners[3] = cnt;

        var cards = new[] { (c1, s1, b1), (c2, s2, b2), (c3, s3, b3), (c4, s4, b4) };
        for (int i = 0; i < 4; i++)
        {
            Set(listeners[i], "subject", subject);
            Set(listeners[i], "card", cards[i].Item1);
            Set(listeners[i], "cardState", cards[i].Item2);
            OnClick(cards[i].Item3, listeners[i].ToggleSubscribed);
        }

        // UnityEvent 카드 (정적 설명)
        var ue = Panel(root, "Card_UnityEvent", new Vector2(690f, 330f - 4 * 110f), new Vector2(420f, 96f), new Color32(45, 52, 70, 255));
        CreateText("Title", ue.transform, new Vector2(0f, 16f), new Vector2(390f, 34f), "UnityEvent → 파티클 Play()", 22f, Color.white, TextAlignmentOptions.Left);
        CreateText("State", ue.transform, new Vector2(0f, -18f), new Vector2(390f, 30f), "<color=#9BE89B>인스펙터에서 연결 (코드 없이)</color>", 18f, Color.white, TextAlignmentOptions.Left);

        InfoBox(root, "CodeInfo", new Vector2(150f, -350f), new Vector2(760f, 230f), "코드",
            "<color=#9BE89B>public event Action OnPlayerDied;</color>   // 방송하는 쪽\n" +
            "<color=#9BE89B>OnPlayerDied?.Invoke();</color>   // 죽을 때 한 줄\n" +
            "<color=#9BE89B>void OnEnable()  => player.OnPlayerDied += Show;\nvoid OnDisable() => player.OnPlayerDied -= Show;</color>  // 듣는 쪽\n" +
            "UnityEvent 는 인스펙터에서 연결, C# event 는 코드에서 연결 (더 빠르고 안전)", 18f);

        var buttons = ButtonRow(root, new Vector2(-30f, -60f), 200f, 60f, 22f, "피해 25 (H)", "부활 (R)");

        var demo = new GameObject("EventObserverDemo").AddComponent<EventObserverDemo>();
        Set(demo, "subject", subject);
        SetArray(demo, "listeners", listeners);
        Set(demo, "hpFill", hpFill);
        OnClick(buttons[0], demo.Damage);
        OnClick(buttons[1], demo.Revive);

        Save(ctx);
    }

    private static (Image card, TextMeshProUGUI state, Button button) Card(Transform root, int index, string title)
    {
        var card = Panel(root, $"Card_{index + 1}", new Vector2(690f, 330f - index * 110f), new Vector2(420f, 96f), new Color32(52, 58, 76, 255));
        var btn = card.gameObject.AddComponent<Button>();
        btn.targetGraphic = card;
        CreateText("Title", card.transform, new Vector2(0f, 16f), new Vector2(390f, 34f), title, 22f, Color.white, TextAlignmentOptions.Left);
        var state = CreateText("State", card.transform, new Vector2(0f, -18f), new Vector2(390f, 30f), "구독 중", 18f, Color.white, TextAlignmentOptions.Left);
        state.richText = true;
        return (card, state, btn);
    }
}
