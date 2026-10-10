using UnityEngine;

namespace GlossaryDemo.UIDemo
{
    // 미니맵에 빨간 점으로 보이는 적: 정해진 범위 안에서 무작위 지점으로 걸어 다님
    public class MinimapWanderer : MonoBehaviour
    {
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float range = 16f;

        private Vector3 target;

        private void Start() => PickTarget();

        private void Update()
        {
            Vector3 to = target - transform.position;
            to.y = 0f;
            if (to.magnitude < 0.3f) PickTarget();
            else
            {
                transform.rotation = Quaternion.LookRotation(to);
                transform.position += to.normalized * speed * Time.deltaTime;
            }
        }

        private void PickTarget() => target = new Vector3(Random.Range(-range, range), transform.position.y, Random.Range(-range, range));
    }
}
