using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.EnemyAI
{
    // 보스 패턴 / 페이즈
    // - 패턴 3개(부채꼴 탄막, 돌진, 점프 충격파)를 랜덤으로 반복 (같은 패턴 연속 금지)
    // - 체력 30% 이하 → 2페이즈: 연출 후 색·속도·탄 수가 강화됨
    public class BossPhaseController : MonoBehaviour
    {
        [Header("보스")]
        [SerializeField] private float maxHp = 1000f;
        [SerializeField, Range(0f, 1f)] private float phase2Threshold = 0.3f; // 30% 이하 2페이즈
        [SerializeField] private float idleBetweenPatterns = 1.2f;

        [Header("참조")]
        [SerializeField] private AITopDownPlayer player;
        [SerializeField] private Renderer body;
        [SerializeField] private AIBullet bossBulletPrefab;
        [SerializeField] private AIBullet playerBulletPrefab;
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI bannerText;
        [SerializeField] private LineRenderer telegraph;   // 돌진 예고선
        [SerializeField] private Transform shockRing;      // 충격파 범위 표시

        private float hp;
        private int phase = 1;
        private string pattern = "-";
        private int lastPattern = -1;
        private bool transitioning;
        private float baseY;
        private float shootTimer;
        private Coroutine brain;

        private readonly string[] patternNames = { "부채꼴 탄막", "돌진", "점프 충격파" };

        private void Start()
        {
            hp = maxHp;
            baseY = transform.position.y;
            if (telegraph != null) telegraph.enabled = false;
            if (shockRing != null) shockRing.gameObject.SetActive(false);
            brain = StartCoroutine(Brain());
        }

        private IEnumerator Brain()
        {
            yield return new WaitForSeconds(1f);
            while (hp > 0f)
            {
                // 2페이즈 진입 검사 (패턴 사이에서만 전환 → 패턴이 중간에 끊기지 않음)
                if (phase == 1 && hp <= maxHp * phase2Threshold)
                    yield return EnterPhase2();

                int next;
                do next = Random.Range(0, 3); while (next == lastPattern); // 같은 패턴 연속 금지
                lastPattern = next;
                pattern = patternNames[next];

                switch (next)
                {
                    case 0: yield return FanShot(); break;
                    case 1: yield return Charge(); break;
                    default: yield return JumpShockwave(); break;
                }

                pattern = "대기";
                yield return new WaitForSeconds(phase == 1 ? idleBetweenPatterns : idleBetweenPatterns * 0.5f);
            }
            pattern = "격파됨";
            Banner("보스 격파!  R : 다시 하기");
        }

        private IEnumerator EnterPhase2()
        {
            transitioning = true;
            phase = 2;
            pattern = "페이즈 전환";
            Banner("2 페이즈!");
            float t = 0f;
            while (t < 1.5f)
            {
                t += Time.deltaTime;
                transform.Rotate(0f, 600f * Time.deltaTime, 0f);
                transform.localScale = Vector3.one * Mathf.Lerp(2f, 2.6f, t / 1.5f);
                yield return null;
            }
            transitioning = false;
        }

        // 패턴 1: 플레이어 쪽 부채꼴 탄막 (1페이즈 5발×3회, 2페이즈 9발×5회)
        private IEnumerator FanShot()
        {
            int volleys = phase == 1 ? 3 : 5;
            int count = phase == 1 ? 5 : 9;
            float spread = phase == 1 ? 50f : 80f;
            for (int v = 0; v < volleys; v++)
            {
                Vector3 to = Flat(player.transform.position - transform.position).normalized;
                for (int i = 0; i < count; i++)
                {
                    float a = -spread * 0.5f + spread * i / (count - 1);
                    Vector3 dir = Quaternion.Euler(0f, a, 0f) * to;
                    AIBullet.Spawn(bossBulletPrefab, transform.position + dir * 1.5f, dir * (phase == 1 ? 8f : 10f), player.transform, b => player.TakeDamage(6f), 4f);
                }
                yield return new WaitForSeconds(phase == 1 ? 0.5f : 0.3f);
            }
        }

        // 패턴 2: 예고선 → 돌진 (2페이즈는 2연속)
        private IEnumerator Charge()
        {
            int times = phase == 1 ? 1 : 2;
            for (int n = 0; n < times; n++)
            {
                Vector3 start = transform.position;
                Vector3 dir = Flat(player.transform.position - start).normalized;
                Vector3 end = start + dir * 14f;
                end.x = Mathf.Clamp(end.x, -13f, 13f);
                end.z = Mathf.Clamp(end.z, -13f, 13f);

                if (telegraph != null)
                {
                    telegraph.enabled = true;
                    telegraph.positionCount = 2;
                    telegraph.SetPosition(0, start + Vector3.down * (baseY - 0.1f));
                    telegraph.SetPosition(1, end + Vector3.down * (baseY - 0.1f));
                }
                yield return new WaitForSeconds(phase == 1 ? 0.8f : 0.5f);
                if (telegraph != null) telegraph.enabled = false;

                float speed = phase == 1 ? 18f : 24f;
                bool hit = false;
                while ((transform.position - end).sqrMagnitude > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, end, speed * Time.deltaTime);
                    if (!hit && Flat(player.transform.position - transform.position).magnitude < 1.8f)
                    {
                        hit = true;
                        player.TakeDamage(20f);
                    }
                    yield return null;
                }
                yield return new WaitForSeconds(0.3f);
            }
        }

        // 패턴 3: 점프 → 착지 충격파 (원형 탄 + 범위 피해)
        private IEnumerator JumpShockwave()
        {
            Vector3 from = transform.position;
            Vector3 to = Flat(player.transform.position) + Vector3.up * baseY;
            if (shockRing != null)
            {
                shockRing.gameObject.SetActive(true);
                shockRing.position = new Vector3(to.x, 0.05f, to.z);
            }
            float t = 0f, dur = phase == 1 ? 1f : 0.7f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                transform.position = Vector3.Lerp(from, to, t) + Vector3.up * (5f * 4f * t * (1f - t));
                yield return null;
            }
            transform.position = to;
            if (shockRing != null) shockRing.gameObject.SetActive(false);
            if (Flat(player.transform.position - transform.position).magnitude < 4f) player.TakeDamage(25f);

            int count = phase == 1 ? 16 : 28;
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2f / count;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                AIBullet.Spawn(bossBulletPrefab, transform.position + dir * 1.5f, dir * 7f, player.transform, b => player.TakeDamage(6f), 4f);
            }
            yield return new WaitForSeconds(0.4f);
        }

        private void Update()
        {
            // 플레이어 사격: Space 누르고 있으면 보스 방향으로 자동 발사
            shootTimer -= Time.deltaTime;
            if (DemoInput.JumpHeld && shootTimer <= 0f && hp > 0f)
            {
                shootTimer = 0.15f;
                Vector3 dir = Flat(transform.position - player.transform.position).normalized;
                AIBullet.Spawn(playerBulletPrefab, player.transform.position + dir, dir * 20f, transform, b => TakeDamage(10f), 2f);
            }
            if (DemoInput.KeyDown(Key.H)) TakeDamage(maxHp * 0.1f);
            if (DemoInput.KeyDown(Key.R)) Restart();

            if (body != null)
                body.material.color = hp <= 0f ? Color.gray : phase == 2 ? new Color(0.6f, 0.15f, 0.9f) : new Color(0.85f, 0.35f, 0.25f);
            if (hpFill != null)
            {
                hpFill.fillAmount = hp / maxHp;
                hpFill.color = phase == 2 ? new Color(0.7f, 0.3f, 1f) : new Color(0.9f, 0.25f, 0.25f);
            }
            if (hpText != null) hpText.text = $"BOSS  {hp:0} / {maxHp:0}   (페이즈 {phase})";
            if (bannerText != null) bannerText.alpha = Mathf.MoveTowards(bannerText.alpha, 0f, Time.deltaTime * 0.5f);

            GlossaryInfoPanel.Status = $"페이즈 {phase}{(transitioning ? " (전환 연출 중)" : "")}   현재 패턴: <b>{pattern}</b>\n" +
                                       $"보스 HP {hp / maxHp * 100f:0}%  (2페이즈 기준 {phase2Threshold * 100f:0}%)\n" +
                                       $"플레이어 HP {player.Hp:0}   사망 {player.DeathCount}회";
        }

        private void TakeDamage(float amount)
        {
            if (hp <= 0f || transitioning) return;
            hp = Mathf.Max(0f, hp - amount);
        }

        private void Restart()
        {
            if (brain != null) StopCoroutine(brain);
            hp = maxHp;
            phase = 1;
            lastPattern = -1;
            transitioning = false;
            transform.localScale = Vector3.one * 2f;
            transform.position = new Vector3(0f, baseY, 4f);
            if (telegraph != null) telegraph.enabled = false;
            if (shockRing != null) shockRing.gameObject.SetActive(false);
            brain = StartCoroutine(Brain());
        }

        private void Banner(string text)
        {
            if (bannerText == null) return;
            bannerText.text = text;
            bannerText.alpha = 1f;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
