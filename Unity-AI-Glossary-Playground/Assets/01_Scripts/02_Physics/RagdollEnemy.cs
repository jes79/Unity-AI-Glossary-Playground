using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 래그돌 전환
    // 살아 있을 때 : Animator 가 뼈(Transform)를 움직이고, 뼈마다 붙은 Rigidbody 는 isKinematic = true (물리 영향 X)
    // 죽을 때      : ① Animator 끄기  ② 모든 Rigidbody isKinematic = false  ③ 맞은 부위에 힘 → 관절(CharacterJoint)로 이어진 몸이 축 늘어짐
    // disableAnimatorOnDeath 를 끄면(A 키) Animator 가 계속 뼈를 덮어써서 물리와 싸우는 "나쁜 예"를 볼 수 있음
    public class RagdollEnemy : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody[] bones;
        [SerializeField] private Transform pelvis;
        [SerializeField] private float walkSpeed = 1.2f;
        [SerializeField] private float turnSpeed = 30f;

        public static bool DisableAnimatorOnDeath = true;

        private Vector3[] localPos;
        private Quaternion[] localRot;
        private Vector3 pelvisLocalPos;

        public bool IsDead { get; private set; }

        private void Awake()
        {
            localPos = new Vector3[bones.Length];
            localRot = new Quaternion[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                localPos[i] = bones[i].transform.localPosition;
                localRot[i] = bones[i].transform.localRotation;
            }
            pelvisLocalPos = pelvis.localPosition;
            SetRagdoll(false);
        }

        private void Update()
        {
            if (IsDead) return;
            // 살아 있는 동안 원을 그리며 걸어 다님 (걷기 동작은 Animator 가 재생)
            transform.position += transform.forward * walkSpeed * Time.deltaTime;
            transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f);
        }

        public void Die(Rigidbody hitBone, Vector3 force, Vector3 point)
        {
            if (IsDead) return;
            IsDead = true;
            SetRagdoll(true);
            if (hitBone != null) hitBone.AddForceAtPosition(force, point, ForceMode.Impulse);
        }

        public void Revive()
        {
            if (!IsDead) return;
            // 쓰러진 위치에서 일어남: 루트를 골반 아래 바닥으로 옮기고 뼈 자세를 원래대로
            Vector3 p = pelvis.position;
            transform.position = new Vector3(p.x, 0f, p.z);
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].transform.localPosition = localPos[i];
                bones[i].transform.localRotation = localRot[i];
            }
            pelvis.localPosition = pelvisLocalPos;
            IsDead = false;
            SetRagdoll(false);
        }

        private void SetRagdoll(bool on)
        {
            // ① 애니메이터: 래그돌이면 끔 (나쁜 예 모드에서는 계속 켜 둠)
            if (animator != null) animator.enabled = !on || !DisableAnimatorOnDeath;
            // ② 물리: 래그돌이면 중력·충돌로 움직이게
            foreach (var rb in bones)
            {
                if (rb == null) continue;
                // 속도 초기화는 아직 동적(non-kinematic)일 때만: kinematic 바디에 속도를 넣으면 경고가 남
                if (!on && !rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                rb.isKinematic = !on;
            }
        }
    }
}
