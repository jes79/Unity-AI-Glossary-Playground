using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 문: 열고 닫기 (경첩을 중심으로 회전)
    public class DoorInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform hinge;
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float speed = 4f;

        private bool open;

        public string Prompt => open ? "문 닫기" : "문 열기";

        public void Interact(InteractorPlayer player)
        {
            open = !open;
            player.ShowMessage(open ? "문이 열렸다" : "문이 닫혔다");
        }

        private void Update()
        {
            var target = Quaternion.Euler(0f, open ? openAngle : 0f, 0f);
            hinge.localRotation = Quaternion.Slerp(hinge.localRotation, target, speed * Time.deltaTime);
        }
    }
}
