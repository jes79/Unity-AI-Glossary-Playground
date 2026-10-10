using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 원형 범위 검사 두 가지
    // 1) OverlapSphere : "이 점에서 반경 r 안에 있는 Collider 전부" → 폭발 범위 데미지 (왼쪽 클릭)
    // 2) SphereCast    : "굵기 r 인 공을 한 방향으로 굴려서 처음 맞는 것" → 굵은 레이저 (오른쪽 클릭)
    // 둘 다 layerMask 로 Enemy 레이어만 검사 → 범위 안의 상자(Obstacle)는 무시됨
    public class AreaQueryDemo : MonoBehaviour
    {
        [Header("폭발 (OverlapSphere)")]
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float explosionDamage = 60f;   // 중심에서 최대, 가장자리로 갈수록 줄어듦
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private Transform explosionVisual;     // 반투명 구

        [Header("SphereCast")]
        [SerializeField] private Transform gun;
        [SerializeField] private float castRadius = 0.6f;
        [SerializeField] private float castDamage = 25f;
        [SerializeField] private Transform castVisual;
        [SerializeField] private LineRenderer castLine;

        [SerializeField] private LayerMask groundMask;

        private string last = "-";
        private bool useSphereCast = true;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) useSphereCast = !useSphereCast;
            if (PhysicsClick.LeftWorld && AimPoint(out Vector3 p)) Explode(p);
            if (PhysicsClick.RightWorld && AimPoint(out Vector3 q)) Cast(q);

            GlossaryInfoPanel.Status = $"폭발 반경 {explosionRadius} m (OverlapSphere)   오른쪽 클릭: {(useSphereCast ? $"SphereCast 반지름 {castRadius}" : "Raycast (가는 선)")}\n{last}";
        }

        private bool AimPoint(out Vector3 point)
        {
            point = default;
            if (!Physics.Raycast(DemoInput.MouseRay(), out var hit, 200f, groundMask)) return false;
            point = hit.point;
            return true;
        }

        private void Explode(Vector3 center)
        {
            // ★ 반경 안의 Enemy 레이어 Collider 를 전부 찾음 (자주 부르면 OverlapSphereNonAlloc 으로 배열 재사용)
            Collider[] hits = Physics.OverlapSphere(center, explosionRadius, enemyMask, QueryTriggerInteraction.Ignore);
            int damaged = 0;
            foreach (var col in hits)
            {
                var health = col.GetComponentInParent<DemoHealth>();
                if (health == null || health.IsDead) continue;
                float dist = Vector3.Distance(center, col.ClosestPoint(center));
                float falloff = 1f - Mathf.Clamp01(dist / explosionRadius); // 가까울수록 1
                health.TakeDamage(Mathf.Max(5f, explosionDamage * falloff));
                damaged++;
            }
            last = $"폭발! OverlapSphere 로 찾은 적: {hits.Length}명 → 데미지 {damaged}명";
            if (explosionVisual != null) StartCoroutine(ShowSphere(explosionVisual, center, explosionRadius));
        }

        private void Cast(Vector3 target)
        {
            Vector3 origin = gun.position;
            Vector3 dir = target + Vector3.up * 1f - origin;
            dir.y = 0f;
            dir.Normalize();
            const float maxDist = 30f;

            bool didHit = useSphereCast
                ? Physics.SphereCast(origin, castRadius, dir, out RaycastHit hit, maxDist, enemyMask, QueryTriggerInteraction.Ignore)
                : Physics.Raycast(origin, dir, out hit, maxDist, enemyMask, QueryTriggerInteraction.Ignore);

            Vector3 end = didHit ? origin + dir * hit.distance : origin + dir * maxDist;
            if (castLine != null)
            {
                castLine.widthMultiplier = useSphereCast ? castRadius * 2f : 0.05f;
                castLine.SetPosition(0, origin);
                castLine.SetPosition(1, end);
                StartCoroutine(HideLine());
            }
            if (useSphereCast && castVisual != null) StartCoroutine(ShowSphere(castVisual, end, castRadius));

            if (didHit)
            {
                var health = hit.collider.GetComponentInParent<DemoHealth>();
                if (health != null) health.TakeDamage(castDamage);
                last = $"{(useSphereCast ? "SphereCast" : "Raycast")} 맞음: {hit.collider.name} ({hit.distance:0.0} m)";
            }
            else last = $"{(useSphereCast ? "SphereCast" : "Raycast")} 빗나감";
        }

        private IEnumerator HideLine()
        {
            castLine.enabled = true;
            yield return new WaitForSeconds(0.25f);
            castLine.enabled = false;
        }

        private static IEnumerator ShowSphere(Transform visual, Vector3 pos, float radius)
        {
            visual.gameObject.SetActive(true);
            visual.position = pos;
            var mat = visual.GetComponent<Renderer>().material;
            Color c = mat.GetColor("_BaseColor");
            yield return Ease.Run(0.4f, Ease.OutCubic, t =>
            {
                visual.localScale = Vector3.one * radius * 2f * Mathf.Lerp(0.3f, 1f, t);
                mat.SetColor("_BaseColor", new Color(c.r, c.g, c.b, 0.45f * (1f - t * 0.6f)));
            });
            yield return new WaitForSeconds(0.2f);
            visual.gameObject.SetActive(false);
            mat.SetColor("_BaseColor", c);
        }
    }
}
