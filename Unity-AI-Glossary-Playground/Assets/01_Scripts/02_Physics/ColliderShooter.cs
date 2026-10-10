using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 0201 데모: 클릭한 곳을 향해 총알 프리팹(SphereCollider 달림)을 발사
    // V 키로 Collider 모양을 보이는 반투명 덮개(colliderViews)를 켜고 끔
    public class ColliderShooter : MonoBehaviour
    {
        [SerializeField] private Rigidbody bulletPrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float bulletSpeed = 25f;
        [SerializeField] private GameObject[] colliderViews;

        private string lastHit = "-";
        private int shots, hits;
        private bool showColliders = true;

        private void OnEnable() => ColliderBullet.OnHit += HandleHit;
        private void OnDisable() => ColliderBullet.OnHit -= HandleHit;

        private void Update()
        {
            if (DemoInput.LeftClick && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = DemoInput.MouseRay();
                Vector3 aim = Physics.Raycast(ray, out var hit, 200f, ~LayerMask.GetMask("PlayerBullet"), QueryTriggerInteraction.Ignore) ? hit.point : ray.GetPoint(40f);
                Fire((aim - muzzle.position).normalized);
            }
            if (DemoInput.KeyDown(Key.V))
            {
                showColliders = !showColliders;
                foreach (var v in colliderViews) if (v != null) v.SetActive(showColliders);
            }

            GlossaryInfoPanel.Status = $"발사 {shots}   충돌 {hits}\n마지막으로 맞은 것: {lastHit}";
        }

        private void Fire(Vector3 dir)
        {
            shots++;
            var b = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir));
            b.linearVelocity = dir * bulletSpeed;
        }

        private void HandleHit(Collider col, Vector3 point)
        {
            hits++;
            lastHit = $"{col.name}  ({col.GetType().Name})";
            FloatingText.Spawn(point + Vector3.up * 0.3f, col.GetType().Name, new Color(1f, 0.9f, 0.5f), 3.5f);
        }
    }
}
