using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // 보스 AI 를 비헤이비어 트리로 구성. 체력 50% 이하이면 "분노 패턴" 가지가 우선 선택됨
    // 매 프레임 루트부터 다시 평가 → 조건이 바뀌면 즉시 다른 가지로 넘어감 (반응형)
    public class BossBehaviorTree : MonoBehaviour
    {
        [Header("보스")]
        [SerializeField] private float maxHp = 500f;
        [SerializeField, Range(0f, 1f)] private float rageThreshold = 0.5f; // 체력 50% 이하 → 패턴 변경
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float rageChaseSpeed = 5.5f;
        [SerializeField] private float detectRange = 14f;
        [SerializeField] private float meleeRange = 3f;

        [Header("참조")]
        [SerializeField] private AITopDownPlayer player;
        [SerializeField] private Renderer body;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private AIBullet bulletPrefab;

        private float hp;
        private BTNode root;
        private float meleeCooldown, burstCooldown;
        private Vector3 spawnPos;
        private float baseY;

        // 시간이 걸리는 액션 실행용 상태
        private string runningId;
        private int runningFrame;
        private float runningTime;

        private bool IsRage => hp <= maxHp * rageThreshold;

        private void Awake()
        {
            hp = maxHp;
            spawnPos = transform.position;
            baseY = transform.position.y;
            root = BuildTree();
        }

        // ───── 트리 구성 ─────
        private BTNode BuildTree()
        {
            return new BTSelector("Root (Selector)",
                new BTSequence("사망",
                    new BTCondition("HP ≤ 0", () => hp <= 0f),
                    new BTAction("쓰러져 있기", Dead)),
                new BTSequence("분노 패턴 (HP ≤ 50%)",
                    new BTCondition("HP ≤ 50%", () => IsRage),
                    new BTSelector("분노 행동",
                        new BTSequence("탄막",
                            new BTCondition("탄막 쿨다운 끝", () => burstCooldown <= 0f),
                            new BTAction("12방향 탄막 발사", Burst)),
                        new BTSequence("회전 베기",
                            new BTCondition("거리 < 4m", () => Dist() < 4f),
                            new BTAction("회전 베기", Spin)),
                        new BTAction("빠른 추격", () => Chase(rageChaseSpeed)))),
                new BTSequence("근접 공격",
                    new BTCondition("거리 < 3m", () => Dist() < meleeRange),
                    new BTCondition("공격 쿨다운 끝", () => meleeCooldown <= 0f),
                    new BTAction("내려찍기", Slam)),
                new BTSequence("추격",
                    new BTCondition("거리 < 14m", () => Dist() < detectRange),
                    new BTAction("걸어서 추격", () => Chase(chaseSpeed))),
                new BTAction("대기", () => BTState.Running));
        }

        private void Update()
        {
            meleeCooldown -= Time.deltaTime;
            burstCooldown -= Time.deltaTime;

            if (DemoInput.JumpPressed && Dist() < 4f) TakeDamage(maxHp * 0.1f);
            if (DemoInput.KeyDown(Key.H)) TakeDamage(maxHp * 0.1f);
            if (DemoInput.KeyDown(Key.R)) ResetBoss();

            root.ClearVisit();
            root.Tick();

            if (body != null) body.material.color = hp <= 0f ? Color.gray : IsRage ? new Color(0.65f, 0.2f, 0.95f) : new Color(0.85f, 0.3f, 0.25f);
            if (label != null) label.text = $"BOSS  HP {hp:0}/{maxHp:0}{(IsRage ? "\n<color=#D08CFF>분노!</color>" : "")}";

            var sb = new StringBuilder();
            sb.Append($"보스 HP {hp:0}/{maxHp:0} ({hp / maxHp * 100f:0}%)   플레이어 HP {player.Hp:0}\n");
            root.Dump(sb);
            GlossaryInfoPanel.Status = sb.ToString();
        }

        private void TakeDamage(float amount)
        {
            if (hp <= 0f) return;
            hp = Mathf.Max(0f, hp - amount);
        }

        private void ResetBoss()
        {
            hp = maxHp;
            transform.position = spawnPos;
            transform.rotation = Quaternion.identity;
            meleeCooldown = burstCooldown = 0f;
            runningId = null;
        }

        // ───── 액션 ─────

        // duration 동안 Running, 끝나면 Success. 한 프레임이라도 평가가 끊기면 처음부터 다시 시작
        private BTState RunTimed(string id, float duration, Action<float> onUpdate, Action onDone)
        {
            if (runningId != id || runningFrame != Time.frameCount - 1)
            {
                runningId = id;
                runningTime = 0f;
            }
            runningFrame = Time.frameCount;
            runningTime += Time.deltaTime;
            float t = Mathf.Clamp01(runningTime / duration);
            onUpdate?.Invoke(t);
            if (t >= 1f)
            {
                runningId = null;
                onDone?.Invoke();
                return BTState.Success;
            }
            return BTState.Running;
        }

        private BTState Dead()
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, 90f), Time.deltaTime * 5f);
            return BTState.Running;
        }

        private BTState Chase(float speed)
        {
            Vector3 target = player.transform.position;
            target.y = transform.position.y;
            Face(target);
            if (Dist() > meleeRange * 0.7f)
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            return BTState.Running;
        }

        // 내려찍기: 0.8초 동안 뛰어올랐다 떨어지며 3.5m 안에 피해
        private BTState Slam()
        {
            return RunTimed("slam", 0.8f,
                t => SetHeight(Mathf.Sin(t * Mathf.PI) * 2f),
                () =>
                {
                    SetHeight(0f);
                    if (Dist() < 3.5f) player.TakeDamage(20f);
                    meleeCooldown = 1.5f;
                });
        }

        // 회전 베기: 1초 동안 두 바퀴 돌고 4m 안에 피해
        private BTState Spin()
        {
            return RunTimed("spin", 1f,
                t => transform.Rotate(0f, 720f * Time.deltaTime, 0f),
                () => { if (Dist() < 4.5f) player.TakeDamage(15f); });
        }

        // 12방향 탄막: 0.5초 기모은 뒤 발사, 쿨다운 3초
        private BTState Burst()
        {
            return RunTimed("burst", 0.5f,
                t => transform.localScale = Vector3.one * 2f * (1f + 0.2f * t),
                () =>
                {
                    transform.localScale = Vector3.one * 2f;
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * 30f * Mathf.Deg2Rad;
                        var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                        AIBullet.Spawn(bulletPrefab, transform.position + dir * 1.2f, dir * 7f, player.transform, b => player.TakeDamage(8f), 3f);
                    }
                    burstCooldown = 3f;
                });
        }

        private void SetHeight(float h)
        {
            var p = transform.position;
            p.y = baseY + h;
            transform.position = p;
        }

        private float Dist()
        {
            Vector3 d = player.transform.position - transform.position;
            d.y = 0f;
            return d.magnitude;
        }

        private void Face(Vector3 target)
        {
            Vector3 d = target - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * 8f);
        }
    }
}
