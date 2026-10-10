using UnityEngine;
using UnityEngine.EventSystems;

namespace GlossaryDemo.Effects
{
    // 이펙트 데모들이 같이 쓰는 작은 도우미
    public static class FxUtil
    {
        // 파티클을 처음부터 다시 재생 (Burst 를 매번 새로 터뜨릴 때)
        public static void Replay(ParticleSystem ps)
        {
            if (ps == null) return;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }

        // 켜져 있으면 방출만 멈추고(남은 입자는 자연스럽게 사라짐), 꺼져 있으면 다시 재생
        public static bool ToggleEmit(ParticleSystem ps)
        {
            if (ps == null) return false;
            if (ps.isEmitting) { ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); return false; }
            ps.Play(true);
            return true;
        }

        public static string OnOff(bool on) => on ? "<color=#8EF58E>ON</color>" : "<color=#FF8080>OFF</color>";

        // 마우스가 UI(버튼·슬라이더) 위에 있으면 월드 클릭으로 처리하지 않음
        public static bool PointerOverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // 화면 클릭 → 높이 y 인 수평면과 만나는 점
        public static bool MouseOnPlane(Camera cam, float y, out Vector3 point)
        {
            point = Vector3.zero;
            if (cam == null) return false;
            Ray ray = DemoInput.MouseRay(cam);
            var plane = new Plane(Vector3.up, new Vector3(0f, y, 0f));
            if (!plane.Raycast(ray, out float dist)) return false;
            point = ray.GetPoint(dist);
            return true;
        }
    }
}
