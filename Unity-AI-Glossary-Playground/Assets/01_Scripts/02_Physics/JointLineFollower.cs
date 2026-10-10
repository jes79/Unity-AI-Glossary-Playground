using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 두 Transform 사이에 선을 그림 (SpringJoint 용수철 표시)
    [RequireComponent(typeof(LineRenderer))]
    public class JointLineFollower : MonoBehaviour
    {
        [SerializeField] private Transform a, b;
        private LineRenderer lr;

        public void Init(Transform from, Transform to)
        {
            a = from;
            b = to;
        }

        private void Awake() => lr = GetComponent<LineRenderer>();

        private void LateUpdate()
        {
            if (a == null || b == null) return;
            lr.SetPosition(0, a.position);
            lr.SetPosition(1, b.position);
        }
    }
}
