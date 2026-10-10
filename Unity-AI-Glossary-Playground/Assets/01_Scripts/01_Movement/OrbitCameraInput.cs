using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // Cinemachine 3인칭 궤도 카메라 입력
    // CinemachineOrbitalFollow 의 HorizontalAxis(좌우 각도) / VerticalAxis(위아래 각도) / RadialAxis(거리 배율)를
    // 마우스로 직접 돌림. (실무에서는 같은 오브젝트에 CinemachineInputAxisController 를 붙이면
    //  Input System 액션과 자동 연결되지만, 여기서는 원리를 보이려고 값을 코드로 바꿈)
    public class OrbitCameraInput : MonoBehaviour
    {
        [SerializeField] private CinemachineOrbitalFollow orbital;
        [SerializeField] private CinemachineCamera orbitCamera;    // 3인칭 궤도 카메라
        [SerializeField] private CinemachineCamera overviewCamera; // 비교용 고정 전경 카메라
        [SerializeField] private CinemachineBrain brain;
        [SerializeField] private float sensitivity = 0.15f;
        [SerializeField] private bool requireRightMouse = true; // 우클릭 드래그 중에만 회전 (UI 클릭과 충돌 방지)

        private void Update()
        {
            if (orbital != null && (!requireRightMouse || DemoInput.RightHeld))
            {
                Vector2 d = DemoInput.MouseDelta * sensitivity;
                // 좌우: 각도에 더함 (Wrap 이라 -180~180 을 넘으면 반대편으로 이어짐)
                orbital.HorizontalAxis.Value = Mathf.Repeat(orbital.HorizontalAxis.Value + d.x + 180f, 360f) - 180f;
                // 위아래: 범위(Range) 안으로 제한
                orbital.VerticalAxis.Value = Mathf.Clamp(orbital.VerticalAxis.Value - d.y, orbital.VerticalAxis.Range.x, orbital.VerticalAxis.Range.y);
            }

            if (orbital != null)
            {
                float scroll = DemoInput.Scroll;
                if (Mathf.Abs(scroll) > 0.01f)
                    orbital.RadialAxis.Value = Mathf.Clamp(orbital.RadialAxis.Value - Mathf.Sign(scroll) * 0.1f, orbital.RadialAxis.Range.x, orbital.RadialAxis.Range.y);
            }

            // C: Priority 가 높은 카메라가 "라이브" → Brain 이 블렌드하며 전환
            if (DemoInput.KeyDown(Key.C) && orbitCamera != null && overviewCamera != null)
            {
                bool toOverview = orbitCamera.Priority.Value >= overviewCamera.Priority.Value;
                orbitCamera.Priority = toOverview ? 0 : 10;
                overviewCamera.Priority = toOverview ? 10 : 0;
            }

            string live = brain != null && brain.ActiveVirtualCamera != null ? brain.ActiveVirtualCamera.Name : "-";
            GlossaryInfoPanel.Status = $"라이브 카메라: {live}{(brain != null && brain.IsBlending ? " (블렌드 중)" : "")}\n" +
                                       (orbital != null
                                           ? $"Horizontal {orbital.HorizontalAxis.Value:0}°  Vertical {orbital.VerticalAxis.Value:0}°  Radial ×{orbital.RadialAxis.Value:0.0}"
                                           : "");
        }
    }
}
