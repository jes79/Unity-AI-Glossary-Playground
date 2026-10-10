using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AudioDemo
{
    // 피치 랜덤: 발소리를 낼 때마다 pitch 를 0.9 ~ 1.1 사이 랜덤으로 → 같은 클립이 반복돼도 기계적으로 들리지 않음
    // 비교: OFF 면 매번 pitch 1.0 → '똑같은 소리 반복' 느낌
    public class PitchVariationDemo : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private bool usePitchVariation = true;
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;
        [SerializeField] private float stepInterval = 0.35f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float limitX = 7f;
        [SerializeField] private Transform body;
        [SerializeField] private SpriteRenderer[] historyBars;

        private float stepTimer;
        private bool autoWalk;
        private float dir = 1f;
        private int stepCount;
        private float lastPitch = 1f;
        private readonly float[] history = new float[16];
        private int historyIndex;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.P)) usePitchVariation = !usePitchVariation;
            if (DemoInput.KeyDown(Key.Space)) autoWalk = !autoWalk;

            float x = DemoInput.Move.x;
            if (autoWalk && Mathf.Abs(x) < 0.01f)
            {
                if (Mathf.Abs(transform.position.x) > limitX - 0.5f) dir = -Mathf.Sign(transform.position.x);
                x = dir;
            }

            bool moving = Mathf.Abs(x) > 0.01f;
            if (moving)
            {
                var p = transform.position + Vector3.right * (x * moveSpeed * Time.deltaTime);
                p.x = Mathf.Clamp(p.x, -limitX, limitX);
                transform.position = p;
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepInterval)
                {
                    stepTimer -= stepInterval;
                    PlayFootstep();
                }
                // 걸을 때 몸이 통통 튀게
                if (body != null) body.localPosition = new Vector3(0f, 0.5f + Mathf.Abs(Mathf.Sin(stepTimer / stepInterval * Mathf.PI)) * 0.12f, 0f);
            }
            else stepTimer = stepInterval * 0.9f; // 멈췄다 출발하면 곧바로 첫 발소리

            UpdateHistory();
            GlossaryInfoPanel.Status = $"피치 랜덤: {(usePitchVariation ? $"ON ({minPitch:0.0} ~ {maxPitch:0.0})" : "OFF (항상 1.0)")}   자동 걷기 {(autoWalk ? "ON" : "OFF")}\n" +
                                       $"발소리 {stepCount}회   마지막 pitch = {lastPitch:0.000}";
        }

        private void PlayFootstep()
        {
            stepCount++;
            // 핵심: 재생 직전에 pitch 를 랜덤으로
            lastPitch = usePitchVariation ? Random.Range(minPitch, maxPitch) : 1f;
            source.pitch = lastPitch;
            var clip = footsteps[usePitchVariation ? Random.Range(0, footsteps.Length) : 0]; // 클립도 여러 개 중 랜덤
            source.PlayOneShot(clip);

            history[historyIndex] = lastPitch;
            historyIndex = (historyIndex + 1) % history.Length;
        }

        // 최근 16번의 피치를 막대 높이로 표시 (1.0 이 가운데 높이)
        private void UpdateHistory()
        {
            if (historyBars == null) return;
            for (int i = 0; i < historyBars.Length && i < history.Length; i++)
            {
                float v = history[(historyIndex + i) % history.Length];
                float h = v <= 0f ? 0.02f : Mathf.Clamp((v - 0.85f) * 10f, 0.05f, 3f);
                // 막대 스프라이트의 부모(피벗이 아래쪽)를 늘려야 바닥 기준으로 자람
                var t = historyBars[i].transform.parent != null ? historyBars[i].transform.parent : historyBars[i].transform;
                t.localScale = new Vector3(t.localScale.x, h, 1f);
                historyBars[i].color = v <= 0f ? new Color(1f, 1f, 1f, 0.1f) : Color.Lerp(new Color(0.4f, 0.6f, 1f), new Color(1f, 0.6f, 0.3f), Mathf.InverseLerp(0.9f, 1.1f, v));
            }
        }
    }
}
