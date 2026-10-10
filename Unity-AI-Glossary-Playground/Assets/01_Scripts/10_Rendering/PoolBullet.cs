using System;
using UnityEngine;

namespace GlossaryDemo.Rendering
{
    // 앞으로 날아가다 lifeTime 이 지나면 "끝났다"고 알리는 총알
    // 끝났을 때 무엇을 할지(Destroy 할지, 풀에 반납할지)는 쏜 쪽(BulletPoolingDemo)이 정함
    public class PoolBullet : MonoBehaviour
    {
        [SerializeField] private float speed = 14f;
        [SerializeField] private float lifeTime = 1.5f;

        private float age;
        private Action<PoolBullet> onExpired;

        // 풀에서 꺼낼 때마다(또는 새로 만들 때) 상태 초기화 — 풀링에서 가장 중요한 부분
        public void Launch(Vector3 pos, Vector3 dir, Action<PoolBullet> expired)
        {
            transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir));
            age = 0f;
            onExpired = expired;
        }

        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
            age += Time.deltaTime;
            if (age >= lifeTime) onExpired?.Invoke(this);
        }
    }
}
