using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // 주변에서 IInteractable 을 찾아 E 키로 상호작용하는 플레이어
    // 대상이 문인지 상자인지 몰라도 됨 → GetComponent<IInteractable>() 하나로 처리
    [RequireComponent(typeof(CharacterController))]
    public class InteractorPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float interactRadius = 2f;
        [SerializeField] private LayerMask interactableMask;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Transform marker; // 현재 대상 위에 뜨는 표시

        private CharacterController cc;
        private readonly Collider[] hits = new Collider[16];
        private IInteractable current;
        private Component currentComponent;
        private float messageTimer;

        public int Coins { get; set; }

        private void Awake() => cc = GetComponent<CharacterController>();

        private void Update()
        {
            Vector2 input = DemoInput.Move;
            Vector3 move = new Vector3(input.x, 0f, input.y);
            if (move.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(move);
            cc.Move((move * moveSpeed + Vector3.down * 5f) * Time.deltaTime);

            FindNearest();

            if (current != null && DemoInput.KeyDown(Key.E))
                current.Interact(this); // 어떤 종류든 같은 호출

            promptText.text = current != null ? $"[E] {current.Prompt}" : "";
            if (marker != null)
            {
                marker.gameObject.SetActive(current != null);
                if (currentComponent != null)
                    marker.position = currentComponent.transform.position + Vector3.up * (2.2f + Mathf.Sin(Time.time * 4f) * 0.1f);
            }

            if (messageTimer > 0f && (messageTimer -= Time.deltaTime) <= 0f) messageText.text = "";

            GlossaryInfoPanel.Status = $"가까운 대상: {(currentComponent != null ? $"{currentComponent.GetType().Name} (IInteractable)" : "-")}\n" +
                                       $"코인 {Coins}개";
        }

        private void FindNearest()
        {
            current = null;
            currentComponent = null;
            float best = float.MaxValue;
            int count = Physics.OverlapSphereNonAlloc(transform.position, interactRadius, hits, interactableMask);
            for (int i = 0; i < count; i++)
            {
                // 인터페이스로 꺼내기: 구체 클래스 이름을 몰라도 됨
                var interactable = hits[i].GetComponentInParent<IInteractable>();
                if (interactable == null) continue;
                float d = (hits[i].transform.position - transform.position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    current = interactable;
                    currentComponent = (Component)interactable;
                }
            }
        }

        // 상호작용 대상들이 결과를 알릴 때 사용
        public void ShowMessage(string message)
        {
            messageText.text = message;
            messageTimer = 2.5f;
        }
    }
}
