using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.AudioDemo
{
    // BGM 크로스페이드: AudioSource 두 개를 두고, 하나는 볼륨 1→0, 다른 하나는 0→1 을 crossfadeTime(2초) 동안 동시에 진행
    // 비교: '즉시 교체'는 뚝 끊기고 바로 다음 곡이 시작됨
    public class CrossfadeDemo : MonoBehaviour
    {
        [SerializeField] private AudioSource peaceBgm;
        [SerializeField] private AudioSource battleBgm;
        [SerializeField] private float crossfadeTime = 2f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.8f;
        [SerializeField] private bool useCrossfade = true;
        [SerializeField] private bool equalPower;     // 등전력 곡선: 중간에 소리가 꺼지는 느낌을 줄임
        [SerializeField] private Image peaceMeter, battleMeter;

        private AudioSource from, to;
        private float fromStart, toStart;
        private float t = 1f;
        private bool inBattle;

        private void Start()
        {
            peaceBgm.volume = bgmVolume;
            battleBgm.volume = 0f;
            peaceBgm.Play();
        }

        public void StartBattle() { if (!inBattle) { inBattle = true; FadeTo(battleBgm, peaceBgm); } }
        public void EndBattle() { if (inBattle) { inBattle = false; FadeTo(peaceBgm, battleBgm); } }
        public void ToggleMode() => useCrossfade = !useCrossfade;

        private void FadeTo(AudioSource next, AudioSource current)
        {
            if (!useCrossfade)
            {
                current.Stop();
                current.volume = 0f;
                next.volume = bgmVolume;
                next.Play();
                t = 1f;
                return;
            }
            from = current;
            to = next;
            fromStart = current.volume; // 페이드 도중 다시 눌러도 현재 볼륨에서 이어감
            toStart = next.volume;
            if (!next.isPlaying) next.Play();
            t = 0f;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.B)) { if (inBattle) EndBattle(); else StartBattle(); }
            if (DemoInput.KeyDown(Key.C)) ToggleMode();
            if (DemoInput.KeyDown(Key.E)) equalPower = !equalPower;

            if (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / crossfadeTime);
                float outK = equalPower ? Mathf.Cos(t * Mathf.PI * 0.5f) : 1f - t;
                float inK = equalPower ? Mathf.Sin(t * Mathf.PI * 0.5f) : t;
                from.volume = fromStart * outK;
                to.volume = Mathf.Lerp(toStart, bgmVolume, inK);
                if (t >= 1f) from.Stop(); // 다 줄어든 곡은 정지
            }

            if (peaceMeter != null) peaceMeter.fillAmount = peaceBgm.volume / bgmVolume;
            if (battleMeter != null) battleMeter.fillAmount = battleBgm.volume / bgmVolume;

            GlossaryInfoPanel.Status = $"모드: {(useCrossfade ? $"크로스페이드 {crossfadeTime:0}초 ({(equalPower ? "등전력" : "선형")})" : "즉시 교체")}\n" +
                                       $"평화 {peaceBgm.volume:0.00}   전투 {battleBgm.volume:0.00}   {(t < 1f ? $"진행 {t * 100f:0}%" : "")}";
        }
    }
}
