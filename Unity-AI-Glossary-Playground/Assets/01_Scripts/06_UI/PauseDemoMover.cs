using UnityEngine;

namespace GlossaryDemo.UIDemo
{
    // Time.deltaTime 으로 움직이는 오브젝트: timeScale 이 0 이면 멈춤
    public class PauseDemoMover : MonoBehaviour
    {
        [SerializeField] private float amplitude = 3f;
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float spin = 90f;

        private Vector3 origin;
        private float t;

        private void Start() => origin = transform.position;

        private void Update()
        {
            t += Time.deltaTime; // Time.time 대신 누적 → timeScale 0 이면 증가하지 않음
            transform.position = origin + Vector3.right * Mathf.Sin(t * speed) * amplitude;
            transform.Rotate(0f, 0f, spin * Time.deltaTime);
        }
    }
}
