using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // Occlusion Culling 개념 시각화 (레이캐스트 기반 간이 판정)
    // 진짜 Occlusion Culling 은 에디터에서 미리 구운 데이터로 GPU 에 보내기 전에 "벽에 가려진 것"을 뺌.
    // 여기서는 카메라에서 각 소품의 여러 점으로 레이를 쏴서 전부 벽에 막히면 "가려짐"으로 판정해
    //  - 위에서 내려다보는 미니맵 카메라에 초록(보임) / 빨강(가려짐) / 회색(화면 밖 = Frustum Culling)으로 표시
    //  - O 키: 가려진 물체의 렌더러를 실제로 꺼서 "안 그리기"를 흉내
    public class OcclusionVisualizer : MonoBehaviour
    {
        [SerializeField] private Camera playerCam;
        [SerializeField] private Renderer[] props;             // 판정 대상 (Occludee)
        [SerializeField] private LayerMask occluderMask;       // 벽 (Occluder)
        [SerializeField] private Material visibleMat, occludedMat, outOfViewMat;
        [SerializeField] private Renderer[] markers;           // 미니맵용 표시 (props 와 같은 순서, 미니맵 카메라만 봄)
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float turnSpeed = 100f;

        private bool simulateCulling;
        private readonly Plane[] planes = new Plane[6];
        private readonly List<Vector3> samplePoints = new List<Vector3>(9);

        private void Update()
        {
            if (DemoInput.KeyDown(Key.O)) ToggleSimulate();

            // WASD: 전진·회전 (CharacterController 없이 벽 통과 방지만 간단히)
            Vector2 m = DemoInput.Move;
            var t = playerCam.transform;
            t.Rotate(0f, m.x * turnSpeed * Time.deltaTime, 0f, Space.World);
            Vector3 fwd = t.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 step = fwd * (m.y * moveSpeed * Time.deltaTime);
            if (!Physics.SphereCast(t.position, 0.3f, step.normalized, out _, step.magnitude + 0.3f, occluderMask))
                t.position += step;

            GeometryUtility.CalculateFrustumPlanes(playerCam, planes);
            int visible = 0, occluded = 0, outside = 0;
            for (int i = 0; i < props.Length; i++)
            {
                var r = props[i];
                Bounds b = r.bounds;
                State s;
                if (!GeometryUtility.TestPlanesAABB(planes, b)) s = State.OutOfView;          // 화면 밖 (Frustum Culling 은 항상 자동)
                else s = AnyPointVisible(t.position, b) ? State.Visible : State.Occluded;      // 벽 뒤?

                if (s == State.Visible) visible++; else if (s == State.Occluded) occluded++; else outside++;
                markers[i].sharedMaterial = s == State.Visible ? visibleMat : s == State.Occluded ? occludedMat : outOfViewMat;
                r.enabled = !simulateCulling || s == State.Visible;
            }

            GlossaryInfoPanel.Status =
                $"소품 {props.Length}개 중  <color=#8F8>보임 {visible}</color>  <color=#F77>벽에 가려짐 {occluded}</color>  <color=#AAA>화면 밖 {outside}</color>\n" +
                $"가려진 것 안 그리기(O): {(simulateCulling ? "ON → 실제로 그리는 수 " + visible : "OFF → 화면 안의 " + (visible + occluded) + "개 모두 그림")}\n" +
                $"카메라 useOcclusionCulling = {playerCam.useOcclusionCulling} (구운 데이터가 있으면 Unity 가 자동 적용)";
        }

        private enum State { Visible, Occluded, OutOfView }

        // 바운드의 중심 + 8 꼭짓점 중 하나라도 벽에 안 막히면 보임
        private bool AnyPointVisible(Vector3 eye, Bounds b)
        {
            samplePoints.Clear();
            samplePoints.Add(b.center);
            Vector3 e = b.extents * 0.9f;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        samplePoints.Add(b.center + Vector3.Scale(e, new Vector3(x, y, z)));
            foreach (var p in samplePoints)
            {
                Vector3 d = p - eye;
                if (!Physics.Raycast(eye, d.normalized, d.magnitude, occluderMask)) return true;
            }
            return false;
        }

        public void ToggleSimulate()
        {
            simulateCulling = !simulateCulling;
            if (!simulateCulling) foreach (var r in props) r.enabled = true;
        }
    }
}
