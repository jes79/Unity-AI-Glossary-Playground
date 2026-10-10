using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 총알: SphereCollider + Rigidbody. 무언가에 부딪히면(OnCollisionEnter) 맞은 Collider 종류를 알리고 사라짐
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class ColliderBullet : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 4f;

        public static System.Action<Collider, Vector3> OnHit; // 데모 UI 가 결과를 표시하려고 구독

        private void Start() => Destroy(gameObject, lifeTime);

        private void OnCollisionEnter(Collision collision)
        {
            // collision.collider = 맞은 상대의 Collider (BoxCollider / SphereCollider / CapsuleCollider / MeshCollider)
            OnHit?.Invoke(collision.collider, collision.GetContact(0).point);
            Destroy(gameObject); // 벽에 맞으면 사라짐
        }
    }
}
