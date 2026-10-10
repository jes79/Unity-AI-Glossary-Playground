using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Color over Lifetime / Size over Lifetime: 입자 하나가 태어나서 죽을 때까지(0~1) 색과 크기를 곡선대로 바꿈
    // 왼쪽: 두 모듈 OFF(기준)   오른쪽: 노랑→빨강→투명 + 점점 작아짐
    public class OverLifetimeDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem target;
        [SerializeField] private bool colorOverLifetime = true;
        [SerializeField] private bool sizeOverLifetime = true;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.C)) ToggleColor();
            if (DemoInput.KeyDown(Key.S)) ToggleSize();
            GlossaryInfoPanel.Status = $"오른쪽  Color over Lifetime {FxUtil.OnOff(colorOverLifetime)}  (노랑 → 빨강 → 투명)\n" +
                                       $"        Size over Lifetime {FxUtil.OnOff(sizeOverLifetime)}  (1 → 0 곡선)";
        }

        public void ToggleColor() { colorOverLifetime = !colorOverLifetime; Apply(); }
        public void ToggleSize() { sizeOverLifetime = !sizeOverLifetime; Apply(); }

        private void Apply()
        {
            var col = target.colorOverLifetime;
            col.enabled = colorOverLifetime;
            var size = target.sizeOverLifetime;
            size.enabled = sizeOverLifetime;
        }
    }
}
