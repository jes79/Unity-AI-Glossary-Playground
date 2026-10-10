using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Emission: Rate over Time(초당 계속 뿜기) vs Burst(한 순간에 N개)
    // 왼쪽 분수는 Rate, 오른쪽 표적을 때리면 Burst 로 파편 20개가 한 번에 튐
    public class EmissionDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem rateSystem;
        [SerializeField] private ParticleSystem burstSystem;
        [SerializeField] private Transform target;

        [Header("Emission")]
        [SerializeField, Range(0f, 100f)] private float rateOverTime = 10f;
        [SerializeField, Range(1, 100)] private int burstCount = 20;

        private Camera cam;
        private float punch;
        private Vector3 targetScale;
        private int hits;

        private void Start()
        {
            cam = Camera.main;
            targetScale = target.localScale;
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.UpArrow)) SetRate(rateOverTime + 10f);
            if (DemoInput.KeyDown(Key.DownArrow)) SetRate(rateOverTime - 10f);
            if (DemoInput.KeyDown(Key.Space)) Hit(target.position + Vector3.back * 0.5f, Vector3.back);

            if (DemoInput.LeftClick && !FxUtil.PointerOverUI &&
                Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f) && hit.transform == target)
                Hit(hit.point, hit.normal);

            // 맞은 표적이 잠깐 커졌다 돌아옴
            punch = Mathf.MoveTowards(punch, 0f, Time.deltaTime * 4f);
            target.localScale = targetScale * (1f + punch * 0.25f);

            GlossaryInfoPanel.Status = $"왼쪽 Rate over Time: {rateOverTime:0}개/초 → 현재 {rateSystem.particleCount}개 살아 있음\n" +
                                       $"오른쪽 Burst: 한 번에 {burstCount}개 (맞힌 횟수 {hits})";
        }

        public void HitButton() => Hit(target.position + Vector3.back * 0.5f, Vector3.back);
        public void SetRate(float rate) { rateOverTime = Mathf.Clamp(rate, 0f, 100f); Apply(); }

        private void Hit(Vector3 point, Vector3 normal)
        {
            hits++;
            punch = 1f;
            burstSystem.transform.SetPositionAndRotation(point, Quaternion.LookRotation(normal));
            FxUtil.Replay(burstSystem); // Burst 는 재생 시작(0초)에 burstCount 개를 한꺼번에 방출
        }

        private void Apply()
        {
            var em = rateSystem.emission;
            em.rateOverTime = rateOverTime;
            var bem = burstSystem.emission;
            bem.SetBurst(0, new ParticleSystem.Burst(0f, (short)burstCount));
        }
    }
}
