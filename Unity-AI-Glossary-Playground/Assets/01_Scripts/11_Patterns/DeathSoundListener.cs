using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 리스너 2: 사운드 — 죽으면 효과음 재생
    public class DeathSoundListener : DeathListenerBase
    {
        [SerializeField] private AudioSource source;

        protected override void OnPlayerDied() => source.Play();
    }
}
