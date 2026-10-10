using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Systems
{
    // 플레이어 쪽: 반경 안의 IInteractable 중 가장 가까운 것을 골라 "E 상호작용" 표시 → E 로 실행
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private float radius = 2.2f;
        [SerializeField] private LayerMask interactableMask;
        [SerializeField] private RectTransform promptUI;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private GameObject dialogPanel;

        private IInteractable current;
        private Camera cam;
        private readonly Collider[] hits = new Collider[16];

        public int Gold { get; set; }
        public string LastAction { get; private set; } = "물체에 가까이 가 보세요";

        private void Start() => cam = Camera.main;

        private void Update()
        {
            current = FindNearest();

            if (current != null)
            {
                promptUI.gameObject.SetActive(true);
                promptText.text = $"<color=#FFD54A>[E]</color> {current.Prompt}";
                promptUI.position = cam.WorldToScreenPoint(current.PromptPosition); // 월드 위치 → 화면 위치
                if (DemoInput.KeyDown(Key.E))
                {
                    current.Interact(this);
                    LastAction = $"{((Component)current).name} → {current.Prompt}";
                }
            }
            else
            {
                promptUI.gameObject.SetActive(false);
                if (dialogPanel != null && dialogPanel.activeSelf) dialogPanel.SetActive(false); // 멀어지면 대화창 닫기
            }

            GlossaryInfoPanel.Status = $"가까운 대상: {(current != null ? ((Component)current).name : "없음")}   골드 {Gold}\n마지막 상호작용: {LastAction}";
        }

        private IInteractable FindNearest()
        {
            int n = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, interactableMask);
            IInteractable best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                var it = hits[i].GetComponentInParent<IInteractable>();
                if (it == null || !it.CanInteract) continue;
                float d = (hits[i].ClosestPoint(transform.position) - transform.position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = it; }
            }
            return best;
        }

        public void ShowDialog(string text)
        {
            if (dialogPanel == null) return;
            dialogPanel.SetActive(text != null);
            if (text != null) dialogText.text = text;
        }
    }
}
