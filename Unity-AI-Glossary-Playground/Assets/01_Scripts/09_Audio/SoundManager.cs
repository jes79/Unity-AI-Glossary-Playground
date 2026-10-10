using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlossaryDemo.AudioDemo
{
    // 사운드 매니저: 어디서든 SoundManager.Play("Hit") 한 줄로 효과음 재생
    // - 이름 → 클립 표(Inspector 에서 등록)
    // - AudioSource 풀(voices 개)을 돌려 쓰므로 여러 소리가 동시에 나도 끊기지 않음
    // - 싱글턴: static Instance, 씬 이동 시에도 유지(DontDestroyOnLoad), 중복 생성되면 새 것을 파괴
    public class SoundManager : MonoBehaviour
    {
        [Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0f, 0.3f)] public float pitchRandom = 0f;
        }

        [SerializeField] private Sound[] sounds;
        [SerializeField, Range(1, 32)] private int voices = 8;
        [SerializeField] private bool persistAcrossScenes = false; // 실제 게임에선 true 권장 (데모 허브 왕복 때문에 기본 false)

        public static SoundManager Instance { get; private set; }

        private readonly Dictionary<string, Sound> table = new Dictionary<string, Sound>();
        private AudioSource[] pool;
        private int next;

        public static string LastPlayed { get; private set; } = "-";
        public static int BusyVoices
        {
            get
            {
                if (Instance == null) return 0;
                int n = 0;
                foreach (var s in Instance.pool) if (s.isPlaying) n++;
                return n;
            }
        }
        public int VoiceCount => voices;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // 이미 있으면 중복 제거
                return;
            }
            Instance = this;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);

            foreach (var s in sounds)
                if (s != null && !string.IsNullOrEmpty(s.name)) table[s.name] = s;

            pool = new AudioSource[voices];
            for (int i = 0; i < voices; i++)
            {
                pool[i] = gameObject.AddComponent<AudioSource>();
                pool[i].playOnAwake = false;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── 어디서든 호출하는 정적 진입점 ──
        public static void Play(string name)
        {
            if (Instance == null)
            {
                Debug.LogWarning($"[SoundManager] 씬에 SoundManager 가 없음 (Play \"{name}\")");
                return;
            }
            Instance.PlayInternal(name);
        }

        private void PlayInternal(string name)
        {
            if (!table.TryGetValue(name, out var s) || s.clip == null)
            {
                Debug.LogWarning($"[SoundManager] 등록되지 않은 소리: \"{name}\"");
                LastPlayed = $"<color=#FF8080>\"{name}\" 없음 (경고 로그)</color>";
                return;
            }

            // 쉬고 있는 AudioSource 를 찾고, 다 바쁘면 가장 오래된 것을 재사용(라운드 로빈)
            AudioSource src = null;
            for (int i = 0; i < pool.Length; i++)
            {
                var c = pool[(next + i) % pool.Length];
                if (!c.isPlaying) { src = c; break; }
            }
            if (src == null) src = pool[next];
            next = (Array.IndexOf(pool, src) + 1) % pool.Length;

            src.clip = s.clip;
            src.volume = s.volume;
            src.pitch = 1f + UnityEngine.Random.Range(-s.pitchRandom, s.pitchRandom);
            src.Play();
            LastPlayed = $"\"{name}\"";
        }
    }
}
