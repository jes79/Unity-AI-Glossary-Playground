using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // New Input System 정식 사용 예
    // InputActionAsset → InputActionMap("Player") → InputAction(Move / Jump / Attack) 를 코드로 만들고
    // 키보드·마우스·게임패드 바인딩을 한 액션에 함께 묶음. 게임 코드는 "어떤 키"가 아니라 "어떤 액션"만 봄
    // (실무에서는 .inputactions 에셋을 에디터로 만들고 Generate C# Class 또는 PlayerInput 컴포넌트를 씀.
    //  빌더가 같은 내용을 Assets/09_Generated/Movement/0103_PlayerControls.inputactions 로도 저장해 둠)
    [RequireComponent(typeof(Rigidbody))]
    public class InputActionsPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 6f;
        [SerializeField] private Transform sword;          // 공격 시 휘두를 막대
        [SerializeField] private TextMeshProUGUI logText;  // 오른쪽 이벤트 로그
        [SerializeField] private TextMeshProUGUI bindingText;

        private InputActionAsset actions;
        private InputActionMap playerMap;
        private InputAction moveAction, jumpAction, attackAction;

        private Rigidbody rb;
        private Vector2 move;
        private bool jumpRequested;
        private int jumpCount, attackCount;
        private string lastDevice = "-";
        private readonly Queue<string> log = new Queue<string>();
        private Coroutine swing;

        // 액션 에셋을 코드로 구성. 에디터 빌더도 이 메서드로 .inputactions JSON 을 저장함
        public static InputActionAsset CreateActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "PlayerControls";
            var map = asset.AddActionMap("Player");

            // Move: Value(Vector2). WASD·방향키는 2DVector 합성(Composite) 바인딩, 패드는 왼쪽 스틱
            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");

            // Jump / Attack: Button. 한 액션에 여러 장치 바인딩
            var jump = map.AddAction("Jump", InputActionType.Button);
            jump.AddBinding("<Keyboard>/space");
            jump.AddBinding("<Gamepad>/buttonSouth"); // A(Xbox) / ✕(PS)

            var attack = map.AddAction("Attack", InputActionType.Button);
            attack.AddBinding("<Mouse>/leftButton");
            attack.AddBinding("<Keyboard>/j");
            attack.AddBinding("<Gamepad>/buttonWest"); // X(Xbox) / □(PS)
            return asset;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            actions = CreateActions();
            playerMap = actions.FindActionMap("Player");
            moveAction = playerMap.FindAction("Move");
            jumpAction = playerMap.FindAction("Jump");
            attackAction = playerMap.FindAction("Attack");

            // 버튼형 액션은 performed 이벤트로 받음 (눌린 순간 1번)
            jumpAction.performed += OnJump;
            attackAction.performed += OnAttack;
            actions.Enable(); // 활성화해야 입력을 받음

            if (bindingText != null)
                bindingText.text = "<b>바인딩</b>\n" +
                                   $"Move   : {moveAction.GetBindingDisplayString()}\n" +
                                   $"Jump   : {jumpAction.GetBindingDisplayString()}\n" +
                                   $"Attack : {attackAction.GetBindingDisplayString()}";
        }

        private void OnDisable()
        {
            jumpAction.performed -= OnJump;
            attackAction.performed -= OnAttack;
            actions.Disable();
            Destroy(actions);
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            jumpCount++;
            jumpRequested = true;
            Log($"Jump  ← {ctx.control.device.displayName} {ctx.control.displayName}");
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            attackCount++;
            Log($"Attack ← {ctx.control.device.displayName} {ctx.control.displayName}");
            if (swing != null) StopCoroutine(swing);
            swing = StartCoroutine(Swing());
        }

        private void Update()
        {
            // 값형 액션은 매 프레임 ReadValue 로 읽음
            move = moveAction.ReadValue<Vector2>();
            if (moveAction.activeControl != null) lastDevice = moveAction.activeControl.device.displayName;

            // Tab: 액션 맵 끄기/켜기 (메뉴가 열리면 Player 맵을 끄는 식으로 사용)
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (playerMap.enabled) playerMap.Disable(); else playerMap.Enable();
                Log(playerMap.enabled ? "Player 맵 Enable" : "Player 맵 Disable (입력 무시됨)");
            }

            if (transform.position.y < -10f) { rb.position = new Vector3(0f, 1f, 0f); rb.linearVelocity = Vector3.zero; }

            string pad = Gamepad.current != null ? Gamepad.current.displayName : "연결 안 됨";
            GlossaryInfoPanel.Status = $"Player 맵: {(playerMap.enabled ? "ON" : "OFF")}   게임패드: {pad}\n" +
                                       $"Move 값: ({move.x:0.00}, {move.y:0.00})   마지막 이동 장치: {lastDevice}\n" +
                                       $"Jump {jumpCount}회   Attack {attackCount}회";
        }

        private void FixedUpdate()
        {
            Vector3 v = new Vector3(move.x, 0f, move.y) * moveSpeed; // 스틱을 살짝 밀면 느리게 (아날로그 값 그대로)
            rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
            rb.angularVelocity = Vector3.zero;
            if (v.sqrMagnitude > 0.1f) rb.MoveRotation(Quaternion.LookRotation(new Vector3(v.x, 0f, v.z)));

            if (jumpRequested)
            {
                jumpRequested = false;
                if (Physics.Raycast(transform.position, Vector3.down, 1.1f, ~LayerMask.GetMask("Player")))
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
            }
        }

        private IEnumerator Swing()
        {
            if (sword == null) yield break;
            sword.gameObject.SetActive(true);
            yield return Ease.Run(0.2f, Ease.OutCubic, t => sword.localRotation = Quaternion.Euler(0f, Mathf.Lerp(-80f, 80f, t), 0f));
            sword.gameObject.SetActive(false);
        }

        private void Log(string line)
        {
            log.Enqueue($"{Time.time:0.0}s  {line}");
            while (log.Count > 10) log.Dequeue();
            if (logText != null) logText.text = "<b>입력 이벤트</b>\n" + string.Join("\n", log);
        }
    }
}
