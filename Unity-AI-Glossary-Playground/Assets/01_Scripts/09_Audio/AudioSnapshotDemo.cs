using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AudioDemo
{
    // 오디오 스냅샷 + 로우패스 데모
    // - 스냅샷 = '이 상황에서의 믹서 설정값 묶음' (Normal: 필터 열림 22000Hz / Paused: 800Hz 로 고음을 깎아 먹먹하게)
    // - TransitionTo(스냅샷, 시간) 으로 설정값을 부드럽게 이동 (실제 AudioMixerSnapshot.TransitionTo 와 같은 개념)
    // - 일시정지 중에는 Time.timeScale = 0 이므로 전환은 unscaledDeltaTime 으로 진행
    public class AudioSnapshotDemo : MonoBehaviour
    {
        [Serializable]
        public class Snapshot
        {
            public string name;
            public float cutoffHz = 22000f;
            [Range(1f, 10f)] public float resonance = 1f;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [SerializeField] private AudioSource bgm;
        [SerializeField] private AudioLowPassFilter lowPass;
        [SerializeField] private Snapshot normal = new Snapshot { name = "Normal", cutoffHz = 22000f, resonance = 1f, volume = 0.8f };
        [SerializeField] private Snapshot paused = new Snapshot { name = "Paused", cutoffHz = 800f, resonance = 1.2f, volume = 0.6f };
        [SerializeField] private float transitionTime = 0.4f;
        [SerializeField] private bool useLowPass = true;
        [SerializeField] private GameObject pauseOverlay;
        [SerializeField] private Transform bouncer; // 일시정지 확인용 (timeScale 영향을 받음)

        private Snapshot fromSnap, toSnap;
        private float t = 1f;
        private bool isPaused;
        private float cutoff, resonance, volume;

        private void Start()
        {
            Apply(normal, normal, 1f);
            toSnap = normal;
            if (pauseOverlay != null) pauseOverlay.SetActive(false);
        }

        private void OnDisable() => Time.timeScale = 1f;

        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            if (pauseOverlay != null) pauseOverlay.SetActive(isPaused);
            TransitionTo(isPaused ? paused : normal, transitionTime);
        }

        public void ToggleLowPass() => useLowPass = !useLowPass;

        // 현재 값에서 목표 스냅샷으로 time 초 동안 이동
        public void TransitionTo(Snapshot target, float time)
        {
            fromSnap = new Snapshot { name = "current", cutoffHz = cutoff, resonance = resonance, volume = volume };
            toSnap = target;
            t = time <= 0f ? 1f : 0f;
            transitionTime = Mathf.Max(0.0001f, time);
            if (t >= 1f) Apply(target, target, 1f);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Escape) || DemoInput.KeyDown(Key.P)) TogglePause();
            if (DemoInput.KeyDown(Key.F)) ToggleLowPass();

            if (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.unscaledDeltaTime / transitionTime); // 정지 중에도 진행
                Apply(fromSnap, toSnap, t);
            }

            lowPass.enabled = useLowPass;

            if (bouncer != null)
                bouncer.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 3f)) * 2f, 0f);

            GlossaryInfoPanel.Status = $"{(isPaused ? "<color=#FFB060>일시정지</color> (Time.timeScale = 0)" : "플레이 중")}   스냅샷 → {toSnap?.name}\n" +
                                       $"Low Pass Cutoff {cutoff:0} Hz   Volume {volume:0.00}   필터 {(useLowPass ? "ON" : "OFF (볼륨만 변화)")}";
        }

        private void Apply(Snapshot a, Snapshot b, float k)
        {
            // 주파수는 로그 스케일로 보간해야 귀에 고르게 들림 (22000 → 800 을 선형 보간하면 끝에서만 급변)
            cutoff = Mathf.Exp(Mathf.Lerp(Mathf.Log(a.cutoffHz), Mathf.Log(b.cutoffHz), k));
            resonance = Mathf.Lerp(a.resonance, b.resonance, k);
            volume = Mathf.Lerp(a.volume, b.volume, k);
            lowPass.cutoffFrequency = cutoff;
            lowPass.lowpassResonanceQ = resonance;
            bgm.volume = volume;
        }
    }
}
