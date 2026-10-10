using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AudioDemo
{
    // AudioSource(스피커) + AudioClip(소리 파일) 데모: 점프할 때 효과음 재생
    // - PlayOneShot : 이미 재생 중인 소리를 끊지 않고 겹쳐서 재생 → 연타해도 소리가 다 들림 (효과음에 적합)
    // - Play        : source.clip 을 처음부터 다시 재생 → 연타하면 앞 소리가 끊김 (BGM·하나만 나야 하는 소리에 적합)
    public class JumpSoundDemo : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip landClip;
        [SerializeField] private bool usePlayOneShot = true;
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

        [Header("점프")]
        [SerializeField] private float jumpVelocity = 9f;
        [SerializeField] private float gravity = 25f;
        [SerializeField] private float groundY = -2.5f;

        private float vy;
        private bool grounded = true;
        private int playCount;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.T)) usePlayOneShot = !usePlayOneShot;

            // 공중에서도 점프 가능 (연타로 소리 겹침 비교)
            if (DemoInput.JumpPressed)
            {
                vy = jumpVelocity;
                grounded = false;
                PlayJump();
            }

            if (!grounded)
            {
                vy -= gravity * Time.deltaTime;
                var p = transform.position + Vector3.up * (vy * Time.deltaTime);
                if (p.y > 3f) { p.y = 3f; vy = Mathf.Min(vy, 0f); } // 연타해도 화면 밖으로 안 나가게
                if (p.y <= groundY)
                {
                    p.y = groundY;
                    grounded = true;
                    vy = 0f;
                    if (landClip != null) source.PlayOneShot(landClip, volume * 0.6f);
                }
                transform.position = p;
            }

            float scaleY = grounded ? 1f : Mathf.Clamp(1f + vy * 0.02f, 0.85f, 1.2f);
            transform.localScale = new Vector3(1f / scaleY, scaleY, 1f);

            GlossaryInfoPanel.Status = $"재생 방식: {(usePlayOneShot ? "PlayOneShot (겹쳐 재생)" : "Play (앞 소리 끊고 다시)")}\n" +
                                       $"재생 횟수 {playCount}   source.isPlaying = {source.isPlaying}";
        }

        private void PlayJump()
        {
            playCount++;
            if (usePlayOneShot)
            {
                source.PlayOneShot(jumpClip, volume); // 클립과 볼륨을 넘겨 한 번 재생
            }
            else
            {
                source.clip = jumpClip;
                source.volume = volume;
                source.Play();
            }
        }
    }
}
