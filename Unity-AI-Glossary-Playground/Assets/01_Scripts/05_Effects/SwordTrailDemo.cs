using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Trail Renderer: 움직이는 오브젝트가 지나간 자리를 time 초 동안 띠로 남김 → 검기
    // 파티클 Trails 모듈: 입자 하나하나에 꼬리를 붙임 → 오른쪽 혜성
    public class SwordTrailDemo : MonoBehaviour
    {
        [SerializeField] private Transform swordPivot;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem comets;

        [Header("휘두르기")]
        [SerializeField] private float swingDuration = 0.18f;
        [SerializeField] private float swingArc = 220f;
        [SerializeField, Range(0.05f, 1f)] private float trailTime = 0.25f;

        private bool swinging;
        private float side = 1f; // 왼→오 / 오→왼 번갈아

        private void Start()
        {
            trail.time = trailTime;
            swordPivot.localRotation = Quaternion.Euler(0f, 0f, swingArc * 0.5f * side);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space) || (DemoInput.LeftClick && !FxUtil.PointerOverUI)) Swing();
            if (DemoInput.KeyDown(Key.T)) ToggleTrail();
            if (DemoInput.KeyDown(Key.P)) ToggleParticleTrails();
            if (DemoInput.KeyDown(Key.UpArrow)) SetTrailTime(trailTime + 0.05f);
            if (DemoInput.KeyDown(Key.DownArrow)) SetTrailTime(trailTime - 0.05f);

            GlossaryInfoPanel.Status = $"Trail Renderer {FxUtil.OnOff(trail.emitting)}  time {trailTime:0.00}초 (꼬리가 남는 시간)\n" +
                                       $"파티클 Trails 모듈 {FxUtil.OnOff(comets.trails.enabled)}";
        }

        public void Swing()
        {
            if (!swinging) StartCoroutine(SwingRoutine());
        }

        public void ToggleTrail()
        {
            trail.emitting = !trail.emitting;
            trail.Clear();
        }

        public void ToggleParticleTrails()
        {
            var t = comets.trails;
            t.enabled = !t.enabled;
        }

        public void SetTrailTime(float t)
        {
            trailTime = Mathf.Clamp(t, 0.05f, 1f);
            trail.time = trailTime;
        }

        private IEnumerator SwingRoutine()
        {
            swinging = true;
            float from = swingArc * 0.5f * side, to = -from;
            yield return Ease.Run(swingDuration, Ease.OutCubic, k =>
                swordPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(from, to, k)));
            side = -side;
            swinging = false;
        }
    }
}
