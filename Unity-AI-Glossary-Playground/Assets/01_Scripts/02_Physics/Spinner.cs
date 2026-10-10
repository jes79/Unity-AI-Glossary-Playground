using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 단순 회전 + 위아래 흔들림 (코인 등 장식용)
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private Vector3 degreesPerSecond = new Vector3(0f, 120f, 0f);
        [SerializeField] private float bobHeight = 0.15f;

        private Vector3 basePos;

        private void Start() => basePos = transform.localPosition;

        private void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.World);
            if (bobHeight > 0f) transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time * 3f) * bobHeight;
        }
    }
}
