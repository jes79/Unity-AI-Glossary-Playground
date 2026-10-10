using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo
{
    // 데모용 간단 입력 래퍼 (New Input System 의 Keyboard/Mouse 를 직접 읽음)
    // 프로젝트는 New Input System 전용이라 구 Input.GetKey 는 쓰면 안 됨
    // "New Input System" 데모(0103)만 InputAction 을 정식으로 사용함
    public static class DemoInput
    {
        public static Vector2 Move
        {
            get
            {
                var kb = Keyboard.current;
                if (kb == null) return Vector2.zero;
                float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
                float y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
                var v = new Vector2(x, y);
                return v.sqrMagnitude > 1f ? v.normalized : v;
            }
        }

        public static bool JumpPressed => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        public static bool JumpHeld => Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        public static bool JumpReleased => Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame;

        public static bool KeyDown(Key key) => Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        public static bool KeyHeld(Key key) => Keyboard.current != null && Keyboard.current[key].isPressed;
        public static bool KeyUp(Key key) => Keyboard.current != null && Keyboard.current[key].wasReleasedThisFrame;

        public static bool LeftClick => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        public static bool LeftHeld => Mouse.current != null && Mouse.current.leftButton.isPressed;
        public static bool RightClick => Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        public static bool RightHeld => Mouse.current != null && Mouse.current.rightButton.isPressed;
        public static Vector2 MousePosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        public static Vector2 MouseDelta => Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        public static float Scroll => Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;

        // 마우스 위치에서 카메라 레이 (3D 클릭 데모용)
        public static Ray MouseRay(Camera cam = null)
        {
            cam = cam != null ? cam : Camera.main;
            return cam.ScreenPointToRay(MousePosition);
        }

        // 마우스 위치의 월드 좌표 (2D 직교 카메라용)
        public static Vector2 MouseWorld2D(Camera cam = null)
        {
            cam = cam != null ? cam : Camera.main;
            return cam.ScreenToWorldPoint(MousePosition);
        }
    }
}
