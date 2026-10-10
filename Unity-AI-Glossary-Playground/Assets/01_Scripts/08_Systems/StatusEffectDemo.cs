using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 버프 / 디버프(상태이상) 시스템
    // - 독: 5초 동안 1초마다 데미지 3, 중첩 불가 (다시 걸면 지속시간만 갱신)
    // - 가속(버프): 5초 동안 이동속도 ×1.5   - 빙결: 2초 동안 이동 불가
    public class StatusEffectDemo : MonoBehaviour
    {
        public enum StackPolicy { Refresh, Ignore, Stack } // 갱신 / 무시 / 중첩(비교용)

        private class ActiveEffect
        {
            public string id, name;
            public float duration, remaining, tickTimer;
            public Color color;
        }

        [Header("독 (Poison)")]
        [SerializeField] private float poisonDuration = 5f;
        [SerializeField] private float poisonTickInterval = 1f;
        [SerializeField] private int poisonDamage = 3;
        [SerializeField] private StackPolicy poisonPolicy = StackPolicy.Refresh;

        [Header("버프 / 빙결")]
        [SerializeField] private float hasteDuration = 5f;
        [SerializeField] private float hasteMultiplier = 1.5f;
        [SerializeField] private float freezeDuration = 2f;

        [Header("대상")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private float baseSpeed = 2f;
        [SerializeField] private float moveRange = 4f;
        [SerializeField] private SpriteRenderer body;

        [Header("UI")]
        [SerializeField] private Image[] effectIcons;          // 효과 아이콘 칸 (radial fill)
        [SerializeField] private TextMeshProUGUI[] effectTexts;
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI logText;

        private readonly List<ActiveEffect> effects = new List<ActiveEffect>();
        private int hp;
        private float x;
        private int dir = 1;
        private Vector3 origin;
        private int totalPoisonDamage;
        private string log = "P : 독   H : 가속   F : 빙결";

        private void Awake()
        {
            hp = maxHp;
            origin = transform.position;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.P)) ApplyPoison();
            if (DemoInput.KeyDown(Key.H)) ApplyHaste();
            if (DemoInput.KeyDown(Key.F)) ApplyFreeze();
            if (DemoInput.KeyDown(Key.S)) { poisonPolicy = (StackPolicy)(((int)poisonPolicy + 1) % 3); log = $"독 재적용 규칙: {PolicyName(poisonPolicy)}"; }
            if (DemoInput.KeyDown(Key.R)) { hp = maxHp; totalPoisonDamage = 0; effects.Clear(); log = "초기화"; }

            TickEffects();
            Move();
            UpdateUI();
        }

        public void ApplyPoison()
        {
            var existing = effects.Find(e => e.id == "poison");
            if (existing != null && poisonPolicy == StackPolicy.Refresh)
            {
                existing.remaining = poisonDuration; // 중첩하지 않고 시간만 다시 5초로
                log = "독 재적용 → 중첩 없이 지속시간만 5초로 갱신";
                return;
            }
            if (existing != null && poisonPolicy == StackPolicy.Ignore)
            {
                log = "독 재적용 → 이미 걸려 있어서 무시";
                return;
            }
            effects.Add(new ActiveEffect { id = "poison", name = "독", duration = poisonDuration, remaining = poisonDuration, tickTimer = poisonTickInterval, color = new Color(0.55f, 0.9f, 0.3f) });
            int n = effects.FindAll(e => e.id == "poison").Count;
            log = n > 1 ? $"<color=#FF7A7A>독 중첩 ×{n} (비교용: 데미지가 {n}배)</color>" : "독 적용: 5초 동안 1초마다 3 데미지";
        }

        public void ApplyHaste() => ApplySimple("haste", "가속", hasteDuration, new Color(0.4f, 0.9f, 1f));
        public void ApplyFreeze() => ApplySimple("freeze", "빙결", freezeDuration, new Color(0.6f, 0.8f, 1f));

        private void ApplySimple(string id, string name, float duration, Color color)
        {
            var existing = effects.Find(e => e.id == id);
            if (existing != null) { existing.remaining = duration; log = $"{name} 갱신"; return; }
            effects.Add(new ActiveEffect { id = id, name = name, duration = duration, remaining = duration, color = color });
            log = $"{name} 적용 ({duration:0}초)";
        }

        private void TickEffects()
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                var e = effects[i];
                e.remaining -= Time.deltaTime;
                if (e.id == "poison")
                {
                    e.tickTimer -= Time.deltaTime;
                    if (e.tickTimer <= 0f)
                    {
                        e.tickTimer += poisonTickInterval; // 1초마다
                        hp = Mathf.Max(0, hp - poisonDamage);
                        totalPoisonDamage += poisonDamage;
                        DamagePopup.Spawn(transform.position + Vector3.up * 0.8f, $"-{poisonDamage}", new Color(0.6f, 1f, 0.3f), 5f);
                        if (hp == 0) hp = maxHp; // 데모가 끝나지 않게 다시 채움
                    }
                }
                if (e.remaining <= 0f) effects.RemoveAt(i); // 시간이 끝나면 제거 → 효과도 사라짐
            }
        }

        // 이동속도 = 기본 속도 × 버프 배율 (빙결이면 0)
        private float CurrentSpeed
        {
            get
            {
                if (effects.Exists(e => e.id == "freeze")) return 0f;
                float s = baseSpeed;
                if (effects.Exists(e => e.id == "haste")) s *= hasteMultiplier;
                return s;
            }
        }

        private void Move()
        {
            x += dir * CurrentSpeed * Time.deltaTime;
            if (x > moveRange) { x = moveRange; dir = -1; }
            if (x < -moveRange) { x = -moveRange; dir = 1; }
            transform.position = origin + new Vector3(x, Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.15f * (CurrentSpeed > 0f ? 1f : 0f), 0f);

            Color c = new Color(0.4f, 0.85f, 0.45f);
            if (effects.Exists(e => e.id == "poison")) c = Color.Lerp(c, new Color(0.6f, 0.2f, 0.8f), 0.5f + 0.2f * Mathf.Sin(Time.time * 8f));
            if (effects.Exists(e => e.id == "freeze")) c = new Color(0.6f, 0.85f, 1f);
            body.color = c;
        }

        private void UpdateUI()
        {
            for (int i = 0; i < effectIcons.Length; i++)
            {
                bool on = i < effects.Count;
                effectIcons[i].transform.parent.gameObject.SetActive(on);
                if (!on) continue;
                var e = effects[i];
                effectIcons[i].color = e.color;
                effectIcons[i].fillAmount = e.remaining / e.duration; // 남은 시간 비율
                effectTexts[i].text = $"{e.name}\n{e.remaining:0.0}s";
            }
            if (hpFill != null) hpFill.fillAmount = (float)hp / maxHp;
            if (logText != null) logText.text = log;

            GlossaryInfoPanel.Status = $"HP {hp}/{maxHp}   독으로 받은 총 데미지 {totalPoisonDamage}\n" +
                                       $"이동속도 {CurrentSpeed:0.0} (기본 {baseSpeed:0.0})   걸린 효과 {effects.Count}개\n" +
                                       $"독 재적용 규칙: <b>{PolicyName(poisonPolicy)}</b>";
        }

        private static string PolicyName(StackPolicy p) => p switch
        {
            StackPolicy.Refresh => "갱신 (중첩 불가, 시간만 초기화)",
            StackPolicy.Ignore => "무시 (중첩 불가, 새로 거는 것 무시)",
            _ => "중첩 허용 (비교용)",
        };
    }
}
