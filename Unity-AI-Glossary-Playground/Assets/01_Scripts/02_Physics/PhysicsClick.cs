using UnityEngine.EventSystems;

namespace GlossaryDemo.PhysicsDemo
{
    // UI(정보 패널 버튼 등) 위를 클릭한 것은 월드 클릭으로 치지 않음
    public static class PhysicsClick
    {
        private static bool OverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        public static bool LeftWorld => DemoInput.LeftClick && !OverUI;
        public static bool RightWorld => DemoInput.RightClick && !OverUI;
    }
}
