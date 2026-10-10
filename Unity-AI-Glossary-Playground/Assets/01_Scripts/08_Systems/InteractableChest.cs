using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 상자: 한 번 열면 골드를 주고, 이후에는 상호작용 불가 (R 로 다시 닫기)
    public class InteractableChest : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform lid;
        [SerializeField] private int gold = 50;

        private bool opened;
        private float angle;

        public string Prompt => "상자 열기";
        public bool CanInteract => !opened; // 열린 상자는 표시가 뜨지 않음
        public Vector3 PromptPosition => transform.position + Vector3.up * 1.6f;

        public void Interact(Interactor who)
        {
            opened = true;
            who.Gold += gold;
            DamagePopup.Spawn(transform.position + Vector3.up * 1.5f, $"+{gold} G", new Color(1f, 0.85f, 0.3f), 6f);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(UnityEngine.InputSystem.Key.R)) opened = false;
            angle = Mathf.MoveTowards(angle, opened ? -110f : 0f, 300f * Time.deltaTime);
            lid.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }
}
