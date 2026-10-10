using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 두 지점 사이를 왕복하는 단순 이동 (가시 함정, 움직이는 발판 등)
    public class PingPongMover : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(3f, 0f, 0f);
        [SerializeField] private float speed = 0.5f;
        [SerializeField] private float phase;
        [SerializeField] private float spin;

        private Vector3 start;

        private void Awake() => start = transform.position;

        private void Update()
        {
            float t = Mathf.PingPong(Time.time * speed + phase, 1f);
            transform.position = start + offset * Mathf.SmoothStep(0f, 1f, t);
            if (spin != 0f) transform.Rotate(0f, 0f, spin * Time.deltaTime);
        }
    }
}
