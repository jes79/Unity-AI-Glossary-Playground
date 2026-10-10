using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // FSM(유한 상태 기계) 적: Idle → Patrol → Chase → Attack → Dead
    // 한 번에 하나의 상태만 실행하고, 각 상태 안에서 "전환 조건"을 검사해 다음 상태로 바꿈
    public class FsmEnemy : MonoBehaviour
    {
        public enum State { Idle, Patrol, Chase, Attack, Dead }

        [Header("전환 거리")]
        [SerializeField] private float chaseRange = 10f;   // 이 안에 들어오면 추격
        [SerializeField] private float attackRange = 2f;   // 이 안이면 공격
        [SerializeField] private float loseRange = 12f;    // 이보다 멀어지면 추격 포기

        [Header("행동 값")]
        [SerializeField] private float idleTime = 2f;
        [SerializeField] private float patrolSpeed = 2.5f;
        [SerializeField] private float chaseSpeed = 4.5f;
        [SerializeField] private float attackInterval = 1f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float reviveTime = 4f;

        [Header("참조")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private AITopDownPlayer player;
        [SerializeField] private Renderer body;
        [SerializeField] private TextMeshPro label;

        private State state = State.Idle;
        private float stateTime;
        private float hp;
        private int patrolIndex;
        private float attackTimer;
        private Vector3 spawnPos;
        private readonly List<string> history = new List<string>();

        public State Current => state;

        private void Awake()
        {
            hp = maxHp;
            spawnPos = transform.position;
            EnterState(State.Idle);
        }

        private void Update()
        {
            stateTime += Time.deltaTime;
            float dist = DistanceToPlayer();

            // ───── 상태별 행동 + 전환 조건 ─────
            switch (state)
            {
                case State.Idle:
                    if (dist < chaseRange) ChangeState(State.Chase);
                    else if (stateTime > idleTime) ChangeState(State.Patrol);
                    break;

                case State.Patrol:
                    if (patrolPoints != null && patrolPoints.Length > 0)
                    {
                        Vector3 target = patrolPoints[patrolIndex].position;
                        if (MoveTo(target, patrolSpeed))
                        {
                            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                            ChangeState(State.Idle); // 지점에 도착하면 잠깐 쉼
                            break;
                        }
                    }
                    if (dist < chaseRange) ChangeState(State.Chase);
                    break;

                case State.Chase:
                    MoveTo(player.transform.position, chaseSpeed, attackRange * 0.8f);
                    if (dist <= attackRange) ChangeState(State.Attack);
                    else if (dist > loseRange) ChangeState(State.Patrol);
                    break;

                case State.Attack:
                    Face(player.transform.position);
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0f)
                    {
                        attackTimer = attackInterval;
                        player.TakeDamage(attackDamage);
                        transform.localScale = Vector3.one * 1.35f; // 공격 모션 대신 크기 펀치
                    }
                    if (dist > attackRange + 0.5f) ChangeState(State.Chase);
                    break;

                case State.Dead:
                    if (stateTime > reviveTime)
                    {
                        hp = maxHp;
                        transform.position = spawnPos;
                        transform.rotation = Quaternion.identity;
                        ChangeState(State.Idle);
                    }
                    break;
            }

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 10f);

            // 플레이어의 공격 (테스트용 입력)
            if (state != State.Dead)
            {
                if (DemoInput.JumpPressed && dist <= 3f) TakeDamage(25f);
                if (DemoInput.KeyDown(Key.K)) TakeDamage(maxHp);
            }

            if (label != null) label.text = $"{state}\nHP {hp:0}";

            GlossaryInfoPanel.Status = $"현재 상태: <b>{state}</b> ({stateTime:0.0}초)   플레이어 거리 {dist:0.0}m\n" +
                                       $"적 HP {hp:0}/{maxHp:0}   플레이어 HP {player.Hp:0}\n" +
                                       $"최근 전환: {string.Join(" → ", history)}";
        }

        public void TakeDamage(float amount)
        {
            if (state == State.Dead) return;
            hp -= amount;
            transform.localScale = Vector3.one * 0.8f;
            if (hp <= 0f)
            {
                hp = 0f;
                ChangeState(State.Dead); // 어느 상태에서든 Dead 로 갈 수 있는 "전역 전환"
            }
            else if (state == State.Idle || state == State.Patrol)
            {
                ChangeState(State.Chase); // 맞으면 바로 추격
            }
        }

        private void ChangeState(State next)
        {
            if (next == state) return;
            history.Add(next.ToString());
            if (history.Count > 6) history.RemoveAt(0);
            ExitState(state);
            state = next;
            EnterState(next);
        }

        // 상태에 들어갈 때 한 번 실행 (OnEnter)
        private void EnterState(State s)
        {
            stateTime = 0f;
            if (s == State.Attack) attackTimer = 0.3f;
            if (s == State.Dead) transform.rotation = Quaternion.Euler(0f, 0f, 90f); // 쓰러짐
            if (body != null) body.material.color = StateColor(s);
        }

        // 상태에서 나갈 때 한 번 실행 (OnExit)
        private void ExitState(State s) { }

        private static Color StateColor(State s) => s switch
        {
            State.Idle => new Color(0.6f, 0.6f, 0.65f),
            State.Patrol => new Color(0.3f, 0.8f, 0.4f),
            State.Chase => new Color(1f, 0.65f, 0.1f),
            State.Attack => new Color(0.95f, 0.2f, 0.2f),
            _ => new Color(0.2f, 0.2f, 0.2f),
        };

        private float DistanceToPlayer()
        {
            if (player == null) return float.MaxValue;
            Vector3 d = player.transform.position - transform.position;
            d.y = 0f;
            return d.magnitude;
        }

        // 목표로 이동. 도착하면 true
        private bool MoveTo(Vector3 target, float speed, float stopDistance = 0.1f)
        {
            target.y = transform.position.y;
            Vector3 d = target - transform.position;
            if (d.magnitude <= stopDistance) return true;
            Face(target);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            return false;
        }

        private void Face(Vector3 target)
        {
            Vector3 d = target - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * 10f);
        }
    }
}
