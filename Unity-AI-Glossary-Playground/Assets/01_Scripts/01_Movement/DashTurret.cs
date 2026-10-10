using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 0108 데모용 포탑: interval 초마다 플레이어 쪽으로 총알을 쏨
    public class DashTurret : MonoBehaviour
    {
        [SerializeField] private DashPlayer target;
        [SerializeField] private Sprite bulletSprite;
        [SerializeField] private float interval = 1.2f;
        [SerializeField] private float bulletSpeed = 6f;
        [SerializeField] private float startDelay;

        private float timer;

        private void Start() => timer = startDelay;

        private void Update()
        {
            if (target == null) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = interval;

            var go = new GameObject("Bullet");
            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one * 0.35f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bulletSprite;
            sr.color = new Color(1f, 0.4f, 0.35f);
            sr.sortingOrder = 2;
            var bullet = go.AddComponent<DashBullet>();
            bullet.Init(target, ((Vector2)(target.transform.position - transform.position)).normalized * bulletSpeed);
        }
    }
}
