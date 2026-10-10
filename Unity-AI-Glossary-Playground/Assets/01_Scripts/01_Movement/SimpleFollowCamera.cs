using UnityEngine;

namespace GlossaryDemo.Movement
{
    // Cinemachine 없이 쓰는 가장 단순한 3D 추적 카메라: 대상 + 고정 오프셋 위치에서 대상을 바라봄
    public class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -10f);
        [SerializeField, Range(0f, 1f)] private float smoothTime = 0.15f;
        [SerializeField] private float lookHeight = 1f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.rotation = Quaternion.LookRotation(target.position + Vector3.up * lookHeight - transform.position);
        }
    }
}
