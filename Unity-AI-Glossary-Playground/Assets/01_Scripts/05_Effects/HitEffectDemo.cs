using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // 타격 이펙트 = 여러 연출을 "같은 프레임에" 겹치는 것
    // 1 하얀 섬광(표적 번쩍 + 별 모양 플래시)  2 파편 Burst  3 히트스톱 0.05초  4 작은 카메라 쉐이크
    // 하나씩 꺼 보면 각 요소가 타격감에 얼마나 기여하는지 알 수 있음
    public class HitEffectDemo : MonoBehaviour
    {
        [SerializeField] private Renderer target;
        [SerializeField] private ParticleSystem flash;
        [SerializeField] private ParticleSystem debris;
        [SerializeField] private EffectsCameraShake cameraShake;
        [SerializeField] private AudioSource hitSound;

        [Header("요소 ON/OFF")]
        [SerializeField] private bool useFlash = true;
        [SerializeField] private bool useDebris = true;
        [SerializeField] private bool useHitStop = true;
        [SerializeField] private bool useShake = true;

        [Header("수치")]
        [SerializeField] private float flashDuration = 0.06f;
        [SerializeField] private float hitStopDuration = 0.05f;
        [SerializeField] private float shakeDuration = 0.12f;
        [SerializeField] private float shakeAmplitude = 0.12f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock mpb;
        private Color baseColor;
        private Camera cam;
        private Coroutine flashRoutine, stopRoutine;
        private Vector3 targetPos;

        private void Start()
        {
            cam = Camera.main;
            mpb = new MaterialPropertyBlock();
            baseColor = target.sharedMaterial.GetColor(BaseColorId);
            targetPos = target.transform.position;
        }

        private void OnDisable() => Time.timeScale = 1f;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) useFlash = !useFlash;
            if (DemoInput.KeyDown(Key.Digit2)) useDebris = !useDebris;
            if (DemoInput.KeyDown(Key.Digit3)) useHitStop = !useHitStop;
            if (DemoInput.KeyDown(Key.Digit4)) useShake = !useShake;
            if (DemoInput.KeyDown(Key.Space)) HitFront();
            if (DemoInput.LeftClick && !FxUtil.PointerOverUI && Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f) && hit.transform == target.transform)
                Hit(hit.point, hit.normal);

            // 넉백 후 제자리로 복귀
            target.transform.position = Vector3.Lerp(target.transform.position, targetPos, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));

            GlossaryInfoPanel.Status = $"1 섬광 {FxUtil.OnOff(useFlash)}   2 파편 Burst {FxUtil.OnOff(useDebris)}   " +
                                       $"3 히트스톱 {hitStopDuration:0.00}s {FxUtil.OnOff(useHitStop)}   4 쉐이크 {FxUtil.OnOff(useShake)}\n" +
                                       $"Time.timeScale = {Time.timeScale:0.0}";
        }

        public void HitFront() => Hit(targetPos + new Vector3(0f, 0.3f, -0.5f), Vector3.back);
        public void ToggleFlash() => useFlash = !useFlash;
        public void ToggleDebris() => useDebris = !useDebris;
        public void ToggleHitStop() => useHitStop = !useHitStop;
        public void ToggleShake() => useShake = !useShake;

        private void Hit(Vector3 point, Vector3 normal)
        {
            if (hitSound != null) hitSound.Play();
            target.transform.position = targetPos - normal * 0.15f; // 맞은 반대 방향으로 살짝 밀림

            if (useFlash)
            {
                flash.transform.position = point + normal * 0.05f;
                FxUtil.Replay(flash);
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(FlashTarget());
            }
            if (useDebris)
            {
                debris.transform.SetPositionAndRotation(point, Quaternion.LookRotation(normal));
                FxUtil.Replay(debris);
            }
            if (useShake && cameraShake != null) cameraShake.Shake(shakeDuration, shakeAmplitude);
            if (useHitStop)
            {
                if (stopRoutine != null) StopCoroutine(stopRoutine);
                stopRoutine = StartCoroutine(HitStop());
            }
        }

        // 표적 머티리얼 색을 잠깐 흰색으로 (MaterialPropertyBlock → 공유 머티리얼은 그대로)
        private IEnumerator FlashTarget()
        {
            mpb.SetColor(BaseColorId, Color.white * 3f);
            target.SetPropertyBlock(mpb);
            yield return new WaitForSecondsRealtime(flashDuration);
            mpb.SetColor(BaseColorId, baseColor);
            target.SetPropertyBlock(mpb);
        }

        // 히트스톱: 게임 시간을 멈췄다가 실제 시간(Realtime)으로 기다린 뒤 복구
        private IEnumerator HitStop()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = 1f;
        }
    }
}
