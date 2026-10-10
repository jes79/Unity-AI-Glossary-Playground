using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 록온 카메라
    // - 평소: 플레이어 뒤쪽 위에서 따라감
    // - 록온 중: 플레이어 "뒤"를 적 반대편으로 잡고, 플레이어와 적 사이(적 쪽으로 치우친 점)를 바라봄
    //   → 플레이어와 적이 항상 한 화면에 들어옴
    public class LockOnCamera : MonoBehaviour
    {
        [SerializeField] private TargetLockPlayer player;
        [SerializeField] private float distance = 7f;
        [SerializeField] private float height = 4f;
        [SerializeField, Range(0f, 1f)] private float lookBias = 0.6f; // 0 = 플레이어, 1 = 적
        [SerializeField] private float smoothTime = 0.25f;

        private Vector3 velocity;
        private Vector3 lookPoint;
        private Vector3 freeBack = new Vector3(0f, 0f, -1f); // 록온 해제 후에도 마지막 방향 유지

        private void Start() => lookPoint = player.transform.position;

        private void LateUpdate()
        {
            if (player == null) return;
            Vector3 p = player.transform.position;
            Vector3 targetLook;
            Transform enemy = player.CurrentTarget;

            if (enemy != null)
            {
                Vector3 toEnemy = Vector3.ProjectOnPlane(enemy.position - p, Vector3.up).normalized;
                freeBack = -toEnemy;
                targetLook = Vector3.Lerp(p, enemy.position, lookBias) + Vector3.up;
            }
            else
            {
                targetLook = p + Vector3.up;
            }

            Vector3 desired = p + freeBack * distance + Vector3.up * height;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            lookPoint = Vector3.Lerp(lookPoint, targetLook, 1f - Mathf.Exp(-8f * Time.deltaTime));
            transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);
        }
    }
}
