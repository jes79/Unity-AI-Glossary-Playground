using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Patterns
{
    // 상태 머신을 가진 플레이어 (Rigidbody2D 횡스크롤)
    // 현재 상태 객체의 Tick 만 호출하고, 상태 전환은 ChangeState 로만 함 (Exit → Enter 순서 보장)
    [RequireComponent(typeof(Rigidbody2D))]
    public class StatePatternPlayer : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 11f;
        [SerializeField] private float attackDuration = 0.35f;
        [SerializeField] private LayerMask groundMask;

        [Header("표시")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private GameObject slash;
        [SerializeField] private Image[] stateNodes;          // Idle, Run, Jump, Attack 순서
        [SerializeField] private TextMeshProUGUI historyText;

        // 상태 인스턴스 (한 번 만들어 재사용)
        public readonly IPlayerState Idle = new IdleState();
        public readonly IPlayerState Run = new RunState();
        public readonly IPlayerState Jump = new JumpState();
        public readonly IPlayerState Attack = new AttackState();

        private Rigidbody2D rb;
        private IPlayerState current;
        private float facing = 1f;
        private Vector2 baseScale;
        private readonly List<string> history = new();

        public float InputX { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool Grounded { get; private set; }
        public float VelocityY => rb.linearVelocity.y;
        public float AttackDuration => attackDuration;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            baseScale = transform.localScale;
        }

        private void Start()
        {
            slash.SetActive(false);
            ChangeState(Idle);
        }

        private void Update()
        {
            // 입력 수집 (상태들은 이 값만 읽음)
            InputX = DemoInput.Move.x;
            JumpPressed = DemoInput.JumpPressed;
            AttackPressed = DemoInput.KeyDown(Key.J);
            Grounded = CheckGround();

            current.Tick(this);

            if (transform.position.y < -8f) { transform.position = new Vector3(0f, 0f, 0f); rb.linearVelocity = Vector2.zero; }

            GlossaryInfoPanel.Status = $"현재 상태: <b>{current.Name}</b>   땅 위 {Grounded}   입력 x {InputX:0}\n" +
                                       "상태마다 클래스 하나 (IdleState, RunState, JumpState, AttackState)";
        }

        public void ChangeState(IPlayerState next)
        {
            if (current == next) return;
            string from = current?.Name ?? "시작";
            current?.Exit(this);
            current = next;
            current.Enter(this);

            history.Insert(0, $"{from} → <b>{next.Name}</b>");
            if (history.Count > 8) history.RemoveAt(history.Count - 1);
            if (historyText != null) historyText.text = "전환 기록\n" + string.Join("\n", history);
            HighlightNode();
        }

        // ── 상태들이 호출하는 동작 ──
        public void Move(float x)
        {
            rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);
            if (Mathf.Abs(x) > 0.1f) facing = Mathf.Sign(x);
        }

        public void DoJump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

        public void SetLook(Color color, Vector2 squash)
        {
            body.color = color;
            transform.localScale = new Vector3(baseScale.x * squash.x, baseScale.y * squash.y, 1f);
        }

        public void ShowSlash(bool show)
        {
            slash.SetActive(show);
            if (show) slash.transform.localPosition = new Vector3(0.9f * facing, 0f, 0f);
        }

        private bool CheckGround()
        {
            Vector2 feet = (Vector2)transform.position + Vector2.down * (transform.localScale.y * 0.5f);
            return Physics2D.OverlapBox(feet, new Vector2(transform.localScale.x * 0.8f, 0.1f), 0f, groundMask) != null;
        }

        private void HighlightNode()
        {
            IPlayerState[] order = { Idle, Run, Jump, Attack };
            for (int i = 0; i < stateNodes.Length && i < order.Length; i++)
                stateNodes[i].color = order[i] == current ? new Color(1f, 0.8f, 0.3f) : new Color32(52, 58, 76, 255);
        }
    }
}
