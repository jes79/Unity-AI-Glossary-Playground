using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 스킬 / 쿨다운: Q, W, E 스킬마다 다른 재사용 대기시간
    // 사용하면 남은 시간 = 쿨다운, 매 프레임 줄어들고 0 이 되어야 다시 사용 가능
    public class SkillCooldownDemo : MonoBehaviour
    {
        [System.Serializable]
        public class Skill
        {
            public string name;
            public Key key;
            public float cooldown;
            public Color color = Color.white;
            public Image cooldownOverlay;   // 위에서부터 걷히는 어두운 덮개 (Radial fill)
            public TextMeshProUGUI timerText;
            [HideInInspector] public float remaining;
            [HideInInspector] public int uses;
        }

        [SerializeField] private Skill[] skills;
        [SerializeField, Range(0f, 0.8f)] private float cooldownReduction = 0.5f; // C 키: 쿨감 적용 비교
        [SerializeField] private bool useReduction;
        [SerializeField] private Transform hero;
        [SerializeField] private Transform dummy;
        [SerializeField] private Sprite effectSprite;
        [SerializeField] private TextMeshProUGUI messageText;

        private float messageTimer;
        private Vector3 heroHome;

        private void Start() => heroHome = hero.position;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.C)) useReduction = !useReduction;
            for (int i = 0; i < skills.Length; i++)
                if (DemoInput.KeyDown(skills[i].key)) TryUse(i);

            foreach (var s in skills)
            {
                s.remaining = Mathf.Max(0f, s.remaining - Time.deltaTime); // 쿨다운 진행
                float total = EffectiveCooldown(s);
                if (s.cooldownOverlay != null) s.cooldownOverlay.fillAmount = total > 0f ? s.remaining / total : 0f;
                if (s.timerText != null) s.timerText.text = s.remaining > 0f ? s.remaining.ToString(s.remaining < 1f ? "0.0" : "0") : "";
            }

            messageTimer -= Time.deltaTime;
            if (messageText != null) messageText.alpha = Mathf.Clamp01(messageTimer);

            var sb = new System.Text.StringBuilder();
            sb.Append($"쿨다운 감소: {(useReduction ? $"<color=#7CFC8A>ON (-{cooldownReduction * 100f:0}%)</color>" : "OFF")}\n");
            foreach (var s in skills)
                sb.Append($"{s.key} {s.name}: 쿨 {EffectiveCooldown(s):0.0}초  {(s.remaining > 0f ? $"<color=#FF9A7A>대기 {s.remaining:0.0}</color>" : "<color=#7CFC8A>사용 가능</color>")}  사용 {s.uses}회\n");
            GlossaryInfoPanel.Status = sb.ToString().TrimEnd();
        }

        private float EffectiveCooldown(Skill s) => s.cooldown * (useReduction ? 1f - cooldownReduction : 1f);

        public void TryUse(int index)
        {
            var s = skills[index];
            if (s.remaining > 0f)
            {
                ShowMessage($"{s.name} 쿨다운 중! ({s.remaining:0.0}초 남음)");
                return;
            }
            s.remaining = EffectiveCooldown(s); // 사용 → 쿨다운 시작
            s.uses++;
            switch (index)
            {
                case 0: StartCoroutine(Fireball(s.color)); break;
                case 1: StartCoroutine(Dash()); break;
                default: StartCoroutine(Nova(s.color)); break;
            }
        }

        private void ShowMessage(string msg)
        {
            if (messageText == null) return;
            messageText.text = msg;
            messageTimer = 1.2f;
        }

        private SpriteRenderer Fx(Vector3 pos, float size, Color color)
        {
            var go = new GameObject("SkillFx");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * size;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = effectSprite;
            sr.color = color;
            sr.sortingOrder = 5;
            return sr;
        }

        // Q: 화염구 - 앞으로 날아가 허수아비에 맞음
        private IEnumerator Fireball(Color color)
        {
            var fx = Fx(hero.position + Vector3.right * 0.6f, 0.6f, color);
            while (fx.transform.position.x < dummy.position.x)
            {
                fx.transform.position += Vector3.right * 14f * Time.deltaTime;
                yield return null;
            }
            DamagePopup.Spawn(dummy.position + Vector3.up, "-30", color, 6f);
            Destroy(fx.gameObject);
        }

        // W: 대시 - 빠르게 앞으로 갔다가 돌아옴
        private IEnumerator Dash()
        {
            Vector3 target = heroHome + Vector3.right * 4f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.15f)
            {
                hero.position = Vector3.Lerp(heroHome, target, t);
                Fx(hero.position, 0.5f, new Color(0.5f, 0.8f, 1f, 0.4f)).gameObject.AddComponent<FadeOut>();
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.4f)
            {
                hero.position = Vector3.Lerp(target, heroHome, t);
                yield return null;
            }
            hero.position = heroHome;
        }

        // E: 광역기 - 원이 크게 퍼짐
        private IEnumerator Nova(Color color)
        {
            var fx = Fx(hero.position, 0.5f, color);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
            {
                fx.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 12f, t);
                fx.color = new Color(color.r, color.g, color.b, 1f - t);
                yield return null;
            }
            DamagePopup.Spawn(dummy.position + Vector3.up * 1.5f, "-120", color, 8f);
            Destroy(fx.gameObject);
        }
    }

    // 잔상용: 점점 투명해지다가 사라짐
    public class FadeOut : MonoBehaviour
    {
        private SpriteRenderer sr;
        private float t;
        private void Awake() => sr = GetComponent<SpriteRenderer>();
        private void Update()
        {
            t += Time.deltaTime / 0.3f;
            var c = sr.color;
            c.a = Mathf.Lerp(0.4f, 0f, t);
            sr.color = c;
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
