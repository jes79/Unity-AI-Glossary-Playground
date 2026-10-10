using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlossaryDemo.AudioDemo
{
    // Audio Mixer 를 흉내 낸 스크립트 믹서 (Audio Mixer 에셋은 공개 API 로 코드 생성이 어려워 대체 구현)
    // - 그룹 트리: Master ─┬─ BGM
    //                      └─ SFX
    // - 각 그룹 볼륨은 데시벨(dB, -80 ~ 0). 최종 볼륨 = 자기 그룹 × 부모 그룹 … (dB 는 더하기, 선형은 곱하기)
    // - SetFloat("BGMVolume", dB) 는 실제 AudioMixer.SetFloat(노출 파라미터, dB) 와 같은 모양
    public class SimpleAudioMixer : MonoBehaviour
    {
        [Serializable]
        public class Group
        {
            public string name;
            public string parent;           // 비어 있으면 최상위(Master)
            public string exposedParameter; // "MasterVolume" 처럼 코드에서 부를 이름
            [Range(-80f, 20f)] public float volumeDb;
            public bool mute;
        }

        [SerializeField] private Group[] groups =
        {
            new Group { name = "Master", parent = "", exposedParameter = "MasterVolume" },
            new Group { name = "BGM", parent = "Master", exposedParameter = "BGMVolume" },
            new Group { name = "SFX", parent = "Master", exposedParameter = "SFXVolume" },
        };

        private readonly List<MixerRoutedSource> sources = new List<MixerRoutedSource>();

        public static SimpleAudioMixer Instance { get; private set; }
        public IReadOnlyList<Group> Groups => groups;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(MixerRoutedSource s) { if (!sources.Contains(s)) sources.Add(s); }
        public void Unregister(MixerRoutedSource s) => sources.Remove(s);

        // ── 노출 파라미터 방식 (AudioMixer.SetFloat / GetFloat 와 같은 사용법) ──
        public bool SetFloat(string exposedName, float db)
        {
            var g = Array.Find(groups, x => x.exposedParameter == exposedName);
            if (g == null) return false;
            g.volumeDb = Mathf.Clamp(db, -80f, 20f);
            return true;
        }

        public bool GetFloat(string exposedName, out float db)
        {
            var g = Array.Find(groups, x => x.exposedParameter == exposedName);
            db = g != null ? g.volumeDb : 0f;
            return g != null;
        }

        public void SetMute(string groupName, bool mute)
        {
            var g = Find(groupName);
            if (g != null) g.mute = mute;
        }

        public Group Find(string groupName) => Array.Find(groups, x => x.name == groupName);

        // 그룹의 최종 선형 볼륨 (부모까지 곱함)
        public float GetGain(string groupName)
        {
            float gain = 1f;
            var g = Find(groupName);
            int guard = 0;
            while (g != null && guard++ < 16)
            {
                gain *= g.mute ? 0f : DbToLinear(g.volumeDb);
                g = string.IsNullOrEmpty(g.parent) ? null : Find(g.parent);
            }
            return gain;
        }

        public static float DbToLinear(float db) => db <= -80f ? 0f : Mathf.Pow(10f, db / 20f);
        public static float LinearToDb(float v) => v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f; // 슬라이더(0~1) → dB

        private void LateUpdate()
        {
            foreach (var s in sources) s.ApplyGain(GetGain(s.GroupName));
        }
    }
}
