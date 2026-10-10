using UnityEngine;

namespace GlossaryDemo.EnemyAI
{
    // 웨이브 적: 기지를 향해 걸어감. 기지에 닿으면 스포너에 알림
    public class WaveEnemy : MonoBehaviour
    {
        private Transform target;
        private WaveSpawner spawner;
        private float speed;

        public void Init(Transform target, WaveSpawner spawner, float speed)
        {
            this.target = target;
            this.spawner = spawner;
            this.speed = speed;
        }

        private void Update()
        {
            if (target == null) return;
            Vector3 p = target.position;
            p.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, p, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, p) < 1.2f) spawner.ReachedBase(this);
        }
    }
}
