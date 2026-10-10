using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 문: 경첩(hinge)을 기준으로 90도 회전하며 열고 닫힘
    public class InteractableDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform hinge;
        [SerializeField] private float openAngle = 100f;

        private bool open;
        private float angle;

        public string Prompt => open ? "문 닫기" : "문 열기";
        public bool CanInteract => true;
        public Vector3 PromptPosition => transform.position + Vector3.up * 2.8f;

        public void Interact(Interactor who) => open = !open;

        private void Update()
        {
            angle = Mathf.MoveTowards(angle, open ? openAngle : 0f, 240f * Time.deltaTime);
            hinge.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
