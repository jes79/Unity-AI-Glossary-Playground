using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // 히트스톱: 공격이 맞는 순간 Time.timeScale = 0 으로 hitStopDuration(0.05초) 동안 멈춤
    // - 멈춘 동안에는 Time.deltaTime 이 0 이므로 실제 시간(Time.unscaledTime)으로 끝나는 시점을 잼
    // - 판정 시점은 공격 클립의 Animation Event(OnAttackHit)
    [RequireComponent(typeof(Animator))]
    public class HitStopDemo : MonoBehaviour
    {
        [Header("히트스톱")]
        [SerializeField] private bool useHitStop = true;
        [SerializeField, Range(0f, 0.5f)] private float hitStopDuration = 0.05f;

        [Header("연출")]
        [SerializeField] private Transform enemy;
        [SerializeField] private Renderer[] enemyRenderers;
        [SerializeField] private Material flashMaterial;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float shakeAmount = 0.12f;

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly float[] Presets = { 0.05f, 0.1f, 0.2f, 0.4f };

        private Animator animator;
        private float stopEndTime = -1f;   // unscaledTime 기준
        private float shakeEndTime = -1f;
        private Vector3 camHome, enemyHome;
        private Material[] enemyMaterials;
        private float knock;
        private int hits;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (cameraTransform != null) camHome = cameraTransform.position;
            if (enemy != null) enemyHome = enemy.position;
            enemyMaterials = new Material[enemyRenderers.Length];
            for (int i = 0; i < enemyRenderers.Length; i++) enemyMaterials[i] = enemyRenderers[i].sharedMaterial;
        }

        private void OnDisable() => Time.timeScale = 1f;

        private void Update()
        {
            if (DemoInput.LeftClick || DemoInput.JumpPressed) animator.SetTrigger(AttackHash);
            if (DemoInput.KeyDown(Key.H)) useHitStop = !useHitStop;
            for (int i = 0; i < Presets.Length; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) hitStopDuration = Presets[i];

            // ── 히트스톱 종료 검사 (실제 시간 기준) ──
            bool stopped = stopEndTime > 0f && Time.unscaledTime < stopEndTime;
            if (stopEndTime > 0f && !stopped)
            {
                Time.timeScale = 1f;
                stopEndTime = -1f;
                SetFlash(false);
            }

            // 카메라 흔들림 (멈춘 동안에도 보이도록 unscaled)
            if (cameraTransform != null)
            {
                bool shaking = Time.unscaledTime < shakeEndTime;
                cameraTransform.position = camHome + (shaking ? (Vector3)Random.insideUnitCircle * shakeAmount : Vector3.zero);
            }

            // 적 넉백 복귀 (게임 시간 기준 → 히트스톱 중에는 멈춰 있음)
            knock = Mathf.MoveTowards(knock, 0f, Time.deltaTime * 1.5f);
            if (enemy != null) enemy.position = enemyHome + transform.forward * knock;

            GlossaryInfoPanel.Status = $"히트스톱: {(useHitStop ? $"ON  {hitStopDuration:0.00}초" : "OFF")}   Time.timeScale = {Time.timeScale:0}\n" +
                                       $"{(stopped ? "<color=#FF8080>■ 정지 중</color>" : "")}  명중 {hits}회";
        }

        // 공격 클립 Animation Event
        public void OnAttackHit()
        {
            hits++;
            knock = 0.5f;
            shakeEndTime = Time.unscaledTime + 0.12f + (useHitStop ? hitStopDuration : 0f);
            if (!useHitStop)
            {
                // 히트스톱이 없으면 번쩍임만 아주 짧게
                SetFlash(true);
                Invoke(nameof(EndFlash), 0.05f);
                return;
            }
            SetFlash(true);
            Time.timeScale = 0f;                                  // 전체 정지
            stopEndTime = Time.unscaledTime + hitStopDuration;    // 0.05초 뒤 복구
        }

        private void EndFlash() => SetFlash(false);

        private void SetFlash(bool on)
        {
            for (int i = 0; i < enemyRenderers.Length; i++)
                enemyRenderers[i].sharedMaterial = on && flashMaterial != null ? flashMaterial : enemyMaterials[i];
        }
    }
}
