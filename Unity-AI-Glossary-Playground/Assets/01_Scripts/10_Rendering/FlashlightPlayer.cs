using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // 플레이어가 들고 다니는 Spot Light 손전등. F 키로 켜고 끔
    // WASD 이동, 마우스 방향으로 몸을 돌림(손전등이 그쪽을 비춤)
    public class FlashlightPlayer : MonoBehaviour
    {
        [SerializeField] private Light flashlight;           // 자식 Spot Light
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Key toggleKey = Key.F;

        private Camera cam;

        private void Awake() => cam = Camera.main;

        private void Update()
        {
            Vector2 m = DemoInput.Move;
            transform.position += new Vector3(m.x, 0f, m.y) * (moveSpeed * Time.deltaTime);

            // 마우스가 가리키는 바닥 지점을 바라봄
            var plane = new Plane(Vector3.up, transform.position);
            Ray ray = DemoInput.MouseRay(cam);
            if (plane.Raycast(ray, out float d))
            {
                Vector3 dir = ray.GetPoint(d) - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);
            }

            if (DemoInput.KeyDown(toggleKey)) ToggleFlashlight();
        }

        public void ToggleFlashlight() => flashlight.enabled = !flashlight.enabled;
        public bool IsOn => flashlight != null && flashlight.enabled;
    }
}
