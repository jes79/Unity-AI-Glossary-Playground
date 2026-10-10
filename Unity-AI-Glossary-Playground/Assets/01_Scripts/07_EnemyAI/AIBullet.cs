using System;
using UnityEngine;

namespace GlossaryDemo.EnemyAI
{
    // 단순 투사체: 직선 이동 + 대상과의 거리로 명중 판정 (물리 충돌 없이 가볍게)
    // 보스 탄막, 예측 조준 포탑 등에서 프리팹으로 사용
    public class AIBullet : MonoBehaviour
    {
        public Vector3 velocity;
        public float lifeTime = 3f;
        public float hitRadius = 0.6f;
        public Transform target;
        public Action<AIBullet> onHit;
        public Action<AIBullet> onExpire;

        private float age;

        public static AIBullet Spawn(AIBullet prefab, Vector3 pos, Vector3 velocity, Transform target, Action<AIBullet> onHit, float life = 3f)
        {
            var b = Instantiate(prefab, pos, Quaternion.LookRotation(velocity.sqrMagnitude > 0f ? velocity : Vector3.forward));
            b.velocity = velocity;
            b.target = target;
            b.onHit = onHit;
            b.lifeTime = life;
            return b;
        }

        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
            age += Time.deltaTime;

            if (target != null)
            {
                Vector3 d = target.position - transform.position;
                d.y = 0f;
                if (d.magnitude < hitRadius)
                {
                    onHit?.Invoke(this);
                    Destroy(gameObject);
                    return;
                }
            }

            if (age > lifeTime)
            {
                onExpire?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }
}
