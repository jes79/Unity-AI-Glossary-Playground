using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 레버: 당기면 연결된 조명이 켜지고 꺼짐
    public class LeverInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform handle;
        [SerializeField] private Light targetLight;
        [SerializeField] private Renderer lampRenderer;
        [SerializeField] private Material lampOn;
        [SerializeField] private Material lampOff;

        private bool on;

        public string Prompt => on ? "레버 내리기 (조명 끄기)" : "레버 올리기 (조명 켜기)";

        private void Start() => Apply();

        public void Interact(InteractorPlayer player)
        {
            on = !on;
            Apply();
            player.ShowMessage(on ? "철컥! 조명이 켜졌다" : "철컥! 조명이 꺼졌다");
        }

        private void Apply()
        {
            handle.localRotation = Quaternion.Euler(on ? -35f : 35f, 0f, 0f);
            targetLight.enabled = on;
            lampRenderer.sharedMaterial = on ? lampOn : lampOff;
        }
    }
}
