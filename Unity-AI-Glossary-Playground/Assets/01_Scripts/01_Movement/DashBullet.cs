using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 0108 데모용 총알: 직선 이동, 플레이어와 거리가 가까우면 맞음. 무적(i-frame)이면 그대로 통과
    public class DashBullet : MonoBehaviour
    {
        private DashPlayer target;
        private Vector2 velocity;
        private float life = 5f;
        private bool touched; // 한 총알은 한 번만 판정

        public void Init(DashPlayer player, Vector2 v)
        {
            target = player;
            velocity = v;
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }

            if (touched || target == null) return;
            if (Vector2.Distance(transform.position, target.transform.position) < 0.55f)
            {
                touched = true;
                target.OnBulletTouch(out bool damaged);
                if (damaged) Destroy(gameObject); // 맞으면 사라지고, 회피하면 몸을 통과해 계속 날아감
            }
        }
    }
}
