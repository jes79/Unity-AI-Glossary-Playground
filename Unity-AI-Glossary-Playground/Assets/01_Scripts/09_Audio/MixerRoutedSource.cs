using UnityEngine;

namespace GlossaryDemo.AudioDemo
{
    // AudioSource 의 Output 을 믹서 그룹에 연결하는 역할 (실제 Audio Mixer 에선 AudioSource.outputAudioMixerGroup)
    [RequireComponent(typeof(AudioSource))]
    public class MixerRoutedSource : MonoBehaviour
    {
        [SerializeField] private string groupName = "SFX";
        [SerializeField, Range(0f, 1f)] private float baseVolume = 1f;

        private AudioSource source;
        public string GroupName => groupName;

        private void Awake() => source = GetComponent<AudioSource>();

        private void Start()
        {
            if (SimpleAudioMixer.Instance != null) SimpleAudioMixer.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (SimpleAudioMixer.Instance != null) SimpleAudioMixer.Instance.Unregister(this);
        }

        public void ApplyGain(float gain) => source.volume = baseVolume * gain;
    }
}
