using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 2D 카메라 Dead Zone / Damping (CinemachinePositionComposer)
    // - Dead Zone: 화면 가운데 이 사각형 안에서 대상이 움직이면 카메라는 가만히 있음
    // - Damping: 대상이 Dead Zone 을 벗어나면 damping 초 정도에 걸쳐 부드럽게 따라감 (0 이면 즉시)
    // 화면 위 노란 사각형이 현재 Dead Zone 크기
    public class DeadZoneDemo : MonoBehaviour
    {
        [SerializeField] private CinemachinePositionComposer composer;
        [SerializeField] private RectTransform deadZoneOutline; // 화면 비율 앵커로 Dead Zone 표시

        [Header("요청값")]
        [SerializeField] private Vector2 deadZoneSize = new Vector2(0.3f, 0.3f); // 화면 비율 (0~1)
        [SerializeField, Range(0f, 3f)] private float damping = 0.5f;

        private bool deadZoneOn = true;
        private bool dampingOn = true;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Z)) { deadZoneOn = !deadZoneOn; Apply(); }
            if (DemoInput.KeyDown(Key.X)) { dampingOn = !dampingOn; Apply(); }
            if (DemoInput.KeyDown(Key.Equals) || DemoInput.KeyDown(Key.NumpadPlus)) { deadZoneSize = Vector2.Min(deadZoneSize + Vector2.one * 0.05f, Vector2.one * 0.8f); Apply(); }
            if (DemoInput.KeyDown(Key.Minus) || DemoInput.KeyDown(Key.NumpadMinus)) { deadZoneSize = Vector2.Max(deadZoneSize - Vector2.one * 0.05f, Vector2.one * 0.05f); Apply(); }

            GlossaryInfoPanel.Status = $"Dead Zone: {(deadZoneOn ? $"ON ({deadZoneSize.x:0.00} × {deadZoneSize.y:0.00} 화면 비율)" : "OFF")}\n" +
                                       $"Damping: {(dampingOn ? $"{damping:0.0}" : "0 (즉시)")}";
        }

        private void Apply()
        {
            if (composer == null) return;
            // Composition 은 구조체라 복사해서 고친 뒤 다시 넣어야 함
            var comp = composer.Composition;
            comp.DeadZone.Enabled = deadZoneOn;
            comp.DeadZone.Size = deadZoneSize;
            composer.Composition = comp;

            float d = dampingOn ? damping : 0f;
            composer.Damping = new Vector3(d, d, 0f);

            if (deadZoneOutline != null)
            {
                deadZoneOutline.gameObject.SetActive(deadZoneOn);
                Vector2 center = new Vector2(0.5f, 0.5f) + comp.ScreenPosition; // ScreenPosition 0 = 화면 가운데
                deadZoneOutline.anchorMin = center - deadZoneSize * 0.5f;
                deadZoneOutline.anchorMax = center + deadZoneSize * 0.5f;
                deadZoneOutline.offsetMin = deadZoneOutline.offsetMax = Vector2.zero;
            }
        }
    }
}
