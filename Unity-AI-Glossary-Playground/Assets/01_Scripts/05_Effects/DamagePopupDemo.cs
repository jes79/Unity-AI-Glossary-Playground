using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // 표적을 클릭하면 맞은 지점에 데미지 숫자 생성. criticalChance 확률로 치명타(크고 빨갛게)
    public class DamagePopupDemo : MonoBehaviour
    {
        [SerializeField] private Collider[] targets;
        [SerializeField] private ParticleSystem hitSpark;

        [Header("데미지")]
        [SerializeField] private int minDamage = 12;
        [SerializeField] private int maxDamage = 30;
        [SerializeField, Range(0f, 1f)] private float criticalChance = 0.2f;
        [SerializeField] private float criticalMultiplier = 2f;
        [SerializeField] private float criticalScale = 1.6f;
        [SerializeField] private bool autoAttack;

        private Camera cam;
        private int hits, crits;
        private float autoTimer;

        private void Start() => cam = Camera.main;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) HitRandom();
            if (DemoInput.KeyDown(Key.A)) ToggleAuto();
            if (DemoInput.KeyDown(Key.C)) ForceCritical();

            if (DemoInput.LeftClick && !FxUtil.PointerOverUI && Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f) &&
                System.Array.IndexOf(targets, hit.collider) >= 0)
                Hit(hit.point + hit.normal * 0.3f, Random.value < criticalChance);

            if (autoAttack && (autoTimer -= Time.deltaTime) <= 0f) { autoTimer = 0.15f; HitRandom(); }

            GlossaryInfoPanel.Status = $"치명타 확률 {criticalChance * 100f:0}% (×{criticalMultiplier}, 크기 ×{criticalScale})   자동 공격 {FxUtil.OnOff(autoAttack)}\n" +
                                       $"타격 {hits}회 / 치명타 {crits}회";
        }

        public void HitRandom()
        {
            var t = targets[Random.Range(0, targets.Length)];
            Vector3 p = t.bounds.center + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0f, 0.6f), -t.bounds.extents.z - 0.3f);
            Hit(p, Random.value < criticalChance);
        }

        public void ForceCritical()
        {
            var t = targets[Random.Range(0, targets.Length)];
            Hit(t.bounds.center + new Vector3(0f, 0.4f, -t.bounds.extents.z - 0.3f), true);
        }

        public void ToggleAuto() => autoAttack = !autoAttack;

        private void Hit(Vector3 point, bool critical)
        {
            hits++;
            if (critical) crits++;
            int dmg = Random.Range(minDamage, maxDamage + 1);
            if (critical) dmg = Mathf.RoundToInt(dmg * criticalMultiplier);
            DamagePopup.Create(point, dmg, critical, criticalScale);
            if (hitSpark != null)
            {
                hitSpark.transform.position = point;
                hitSpark.Emit(critical ? 16 : 6);
            }
        }
    }
}
