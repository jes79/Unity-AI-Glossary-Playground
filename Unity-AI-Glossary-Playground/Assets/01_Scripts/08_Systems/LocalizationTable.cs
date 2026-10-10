using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    public enum Language { Korean, English }

    // 문자열 테이블 (키 → 언어별 문장). Unity Localization 패키지의 "String Table Collection" 을 단순화한 대체 구현
    [CreateAssetMenu(menuName = "Glossary/Localization Table", fileName = "StringTable")]
    public class LocalizationTable : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string key;
            [TextArea] public string korean;
            [TextArea] public string english;
        }

        public List<Entry> entries = new List<Entry>();

        private Dictionary<string, Entry> lookup;

        public string Get(string key, Language lang)
        {
            if (lookup == null || lookup.Count != entries.Count)
            {
                lookup = new Dictionary<string, Entry>();
                foreach (var e in entries) lookup[e.key] = e;
            }
            if (!lookup.TryGetValue(key, out var entry)) return $"#{key}#"; // 빠진 키는 눈에 띄게
            string s = lang == Language.Korean ? entry.korean : entry.english;
            return string.IsNullOrEmpty(s) ? entry.korean : s;           // 번역이 없으면 기본 언어로 대체(Fallback)
        }
    }

    // 현재 언어를 들고 있는 정적 관리자. 언어가 바뀌면 이벤트로 모든 LocalizedText 가 갱신됨
    public static class Localization
    {
        public const string PrefKey = "Glossary0819_Language";
        public static Language Current { get; private set; } = Language.Korean;
        public static LocalizationTable Table { get; set; }
        public static event Action Changed;

        public static void SetLanguage(Language lang)
        {
            Current = lang;
            PlayerPrefs.SetInt(PrefKey, (int)lang); // 다음 실행에도 유지
            Changed?.Invoke();
        }

        public static string Get(string key) => Table != null ? Table.Get(key, Current) : key;

        // "{0}" 같은 자리 표시자에 값 끼워 넣기 (Smart String 의 간단 버전)
        public static string Format(string key, params object[] args) => string.Format(Get(key), args);
    }
}
