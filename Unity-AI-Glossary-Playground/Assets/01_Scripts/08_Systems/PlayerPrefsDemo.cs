using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // PlayerPrefs: 키-값으로 간단한 설정을 저장 (int / float / string)
    // - 사운드 볼륨(float) 과 최고 점수(int) 를 저장 → 플레이를 껐다 켜도 유지
    // - 큰 데이터(인벤토리 등)는 JSON 파일 저장(0812)을 사용
    public class PlayerPrefsDemo : MonoBehaviour
    {
        public const string VolumeKey = "Glossary0813_Volume";
        public const string HighScoreKey = "Glossary0813_HighScore";

        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private TextMeshProUGUI gameText;
        [SerializeField] private TextMeshProUGUI prefsText;
        [SerializeField] private float roundTime = 5f;

        private bool playing;
        private float timer;
        private int score;
        private string log = "";

        private void Start()
        {
            // 불러오기: 저장된 값이 없으면 기본값(두 번째 인자) 사용
            float vol = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
            volumeSlider.SetValueWithoutNotify(vol);
            AudioListener.volume = vol;
            log = PlayerPrefs.HasKey(VolumeKey) ? "저장된 볼륨을 불러왔습니다" : "저장된 값 없음 → 기본값 0.8";
            Refresh();
        }

        // 슬라이더 값이 바뀔 때 (OnValueChanged)
        public void OnVolumeChanged(float v)
        {
            AudioListener.volume = v;
            PlayerPrefs.SetFloat(VolumeKey, v);
            PlayerPrefs.Save(); // 즉시 디스크에 기록 (안 불러도 정상 종료 시 저장됨)
            log = $"SetFloat(\"{VolumeKey}\", {v:0.00})";
            Refresh();
        }

        public void StartRound()
        {
            playing = true;
            timer = roundTime;
            score = 0;
        }

        private void Update()
        {
            if (!playing)
            {
                if (DemoInput.KeyDown(UnityEngine.InputSystem.Key.Enter)) StartRound();
                gameText.text = $"<size=70%>Enter 또는 [게임 시작] → {roundTime:0}초 동안 Space 연타!</size>\n마지막 점수 {score}   최고 점수 <color=#FFD54A>{PlayerPrefs.GetInt(HighScoreKey, 0)}</color>";
                return;
            }

            if (DemoInput.JumpPressed) score++;
            timer -= Time.deltaTime;
            gameText.text = $"<size=140%><b>{score}</b></size>\n남은 시간 {Mathf.Max(0f, timer):0.0}초";
            if (timer <= 0f) EndRound();
        }

        private void EndRound()
        {
            playing = false;
            int best = PlayerPrefs.GetInt(HighScoreKey, 0);
            if (score > best)
            {
                PlayerPrefs.SetInt(HighScoreKey, score); // 최고 점수 갱신 시에만 저장
                PlayerPrefs.Save();
                log = $"<color=#7CFC8A>최고 점수 갱신! SetInt(\"{HighScoreKey}\", {score})</color>";
            }
            else log = $"점수 {score} (최고 {best} 못 넘음 → 저장 안 함)";
            Refresh();
        }

        public void ResetPrefs()
        {
            PlayerPrefs.DeleteKey(VolumeKey);
            PlayerPrefs.DeleteKey(HighScoreKey);
            PlayerPrefs.Save();
            volumeSlider.SetValueWithoutNotify(0.8f);
            AudioListener.volume = 0.8f;
            log = "DeleteKey 로 두 값을 삭제";
            Refresh();
        }

        private void Refresh()
        {
            volumeText.text = $"볼륨 {volumeSlider.value * 100f:0}%";
            prefsText.text =
                "<b>현재 PlayerPrefs</b>\n" +
                $"{VolumeKey} = {(PlayerPrefs.HasKey(VolumeKey) ? PlayerPrefs.GetFloat(VolumeKey).ToString("0.00") : "<color=#888>(없음)</color>")}\n" +
                $"{HighScoreKey} = {(PlayerPrefs.HasKey(HighScoreKey) ? PlayerPrefs.GetInt(HighScoreKey).ToString() : "<color=#888>(없음)</color>")}\n\n" +
                "<size=80%><color=#9AA3B5>Windows 저장 위치: 레지스트리\nHKCU\\Software\\[회사명]\\[제품명]\n(에디터는 HKCU\\Software\\Unity\\UnityEditor\\...)</color></size>";
            GlossaryInfoPanel.Status = log;
        }
    }
}
