using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 탑다운 2D 단순 이동 (WASD, 위치 직접 이동). 카메라 데모에서 "따라갈 대상" 역할
    public class SimpleMover2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private Rect bounds = new Rect(-28f, -18f, 56f, 36f);

        private void Update()
        {
            Vector3 p = transform.position + (Vector3)(DemoInput.Move * moveSpeed * Time.deltaTime);
            p.x = Mathf.Clamp(p.x, bounds.xMin, bounds.xMax);
            p.y = Mathf.Clamp(p.y, bounds.yMin, bounds.yMax);
            transform.position = p;
        }
    }
}
