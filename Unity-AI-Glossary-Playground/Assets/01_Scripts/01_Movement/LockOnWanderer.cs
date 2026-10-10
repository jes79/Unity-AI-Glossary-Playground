using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 0114 데모용 적: 시작 위치 주변을 천천히 배회 (록온 카메라가 따라가는 모습을 보이기 위함)
    public class LockOnWanderer : MonoBehaviour
    {
        [SerializeField] private float radius = 2.5f;
        [SerializeField] private float speed = 0.6f;

        private Vector3 home;
        private float phase;

        private void Start()
        {
            home = transform.position;
            phase = Random.value * 10f;
        }

        private void Update()
        {
            float t = Time.time * speed + phase;
            transform.position = home + new Vector3(Mathf.Sin(t) * radius, 0f, Mathf.Sin(t * 0.7f) * radius * 0.6f);
            transform.Rotate(0f, 60f * Time.deltaTime, 0f);
        }
    }
}
