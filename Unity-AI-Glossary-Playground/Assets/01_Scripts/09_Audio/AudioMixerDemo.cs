using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.AudioDemo
{
    // 설정창 볼륨 슬라이더 ↔ 믹서 그룹 연결
    // 슬라이더 값(0.0001 ~ 1, 선형)을 Log10 × 20 으로 데시벨로 바꿔 SetFloat("BGMVolume", dB) 호출
    // → 실제 Audio Mixer 코드와 한 글자도 다르지 않게 사용 (mixer 타입만 AudioMixer 로 바꾸면 됨)
    public class AudioMixerDemo : MonoBehaviour
    {
        [SerializeField] private SimpleAudioMixer mixer;
        [SerializeField] private string[] exposedParams = { "MasterVolume", "BGMVolume", "SFXVolume" };
        [SerializeField] private string[] groupNames = { "Master", "BGM", "SFX" };
        [SerializeField] private Slider[] sliders;
        [SerializeField] private TextMeshProUGUI[] dbLabels;
        [SerializeField] private Image[] meters;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip sfxClip;
        [SerializeField] private float sfxInterval = 0.75f;

        private bool autoSfx = true;
        private float sfxTimer;

        private void Start()
        {
            for (int i = 0; i < sliders.Length; i++)
            {
                int index = i;
                // 저장된 설정 불러오기 (설정창의 기본 패턴)
                float saved = PlayerPrefs.GetFloat("Glossary0903_" + exposedParams[i], 0.8f);
                sliders[i].SetValueWithoutNotify(saved);
                SetVolume(index, saved);
                sliders[i].onValueChanged.AddListener(v => SetVolume(index, v));
            }
        }

        public void SetVolume(int index, float linear)
        {
            float db = SimpleAudioMixer.LinearToDb(linear); // Mathf.Log10(v) * 20
            mixer.SetFloat(exposedParams[index], db);
            PlayerPrefs.SetFloat("Glossary0903_" + exposedParams[index], linear);
        }

        public void ToggleMute(int index)
        {
            var g = mixer.Find(groupNames[index]);
            if (g != null) g.mute = !g.mute;
        }

        public void PlaySfx()
        {
            if (sfxSource != null && sfxClip != null) sfxSource.PlayOneShot(sfxClip);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) PlaySfx();
            if (DemoInput.KeyDown(Key.A)) autoSfx = !autoSfx;
            if (DemoInput.KeyDown(Key.M)) ToggleMute(0);

            if (autoSfx)
            {
                sfxTimer += Time.deltaTime;
                if (sfxTimer >= sfxInterval)
                {
                    sfxTimer = 0f;
                    PlaySfx();
                }
            }

            for (int i = 0; i < groupNames.Length; i++)
            {
                var g = mixer.Find(groupNames[i]);
                if (g == null) continue;
                if (i < dbLabels.Length) dbLabels[i].text = g.mute ? "MUTE" : $"{g.volumeDb:0.0} dB";
                if (i < meters.Length) meters[i].fillAmount = mixer.GetGain(groupNames[i]);
            }

            GlossaryInfoPanel.Status = $"최종 볼륨  BGM = {mixer.GetGain("BGM"):0.00}   SFX = {mixer.GetGain("SFX"):0.00}\n" +
                                       $"(그룹 볼륨 × Master 볼륨)   자동 효과음 {(autoSfx ? "ON" : "OFF")}";
        }
    }
}
