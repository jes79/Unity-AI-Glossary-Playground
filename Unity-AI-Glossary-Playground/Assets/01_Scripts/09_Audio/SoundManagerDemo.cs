using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AudioDemo
{
    // 사운드 매니저 사용 측: 서로 다른 오브젝트(적, 코인, 플레이어…)가 참조 없이 SoundManager.Play("이름") 만 호출
    public class SoundManagerDemo : MonoBehaviour
    {
        [SerializeField] private string soundName = "Hit";
        [SerializeField] private Key key = Key.Digit1;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private bool showStatus;

        private float flash;
        private Color baseColor;

        private void Awake()
        {
            if (visual != null) baseColor = visual.color;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(key)) PlaySound();

            if (visual != null)
            {
                flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 4f);
                visual.color = Color.Lerp(baseColor, Color.white, flash);
                visual.transform.localScale = Vector3.one * (1.4f + flash * 0.3f);
            }

            if (showStatus && SoundManager.Instance != null)
                GlossaryInfoPanel.Status = $"마지막 호출: SoundManager.Play({SoundManager.LastPlayed})\n" +
                                           $"사용 중인 AudioSource: {SoundManager.BusyVoices} / {SoundManager.Instance.VoiceCount}";
        }

        // 버튼에서도 호출
        public void PlaySound()
        {
            SoundManager.Play(soundName); // ← 이 한 줄이 핵심. AudioSource/AudioClip 참조가 필요 없음
            flash = 1f;
        }

        public void PlayByName(string name) => SoundManager.Play(name);
    }
}
