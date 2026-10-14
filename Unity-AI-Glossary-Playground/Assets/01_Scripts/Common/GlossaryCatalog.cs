using UnityEngine;

namespace GlossaryDemo
{
    // 용어집 한 줄(= 데모 씬 하나)의 정보
    public readonly struct GlossaryEntry
    {
        public readonly string Id;        // "0105" (분야 2자리 + 순번 2자리)
        public readonly int Category;     // 1 ~ 11
        public readonly string Sub;       // 소분류 (없으면 빈 문자열)
        public readonly string Term;      // 용어
        public readonly string Summary;   // 쉽게 말하면
        public readonly string Prompt;    // 이렇게 요청하세요
        public readonly string Scene;     // 씬 이름 "0105_CoyoteTime"

        public GlossaryEntry(string id, int category, string sub, string term, string summary, string prompt, string scene)
        {
            Id = id;
            Category = category;
            Sub = sub;
            Term = term;
            Summary = summary;
            Prompt = prompt;
            Scene = scene;
        }

        public string CategoryName => GlossaryCatalog.Categories[Category - 1];
        public string Folder => GlossaryCatalog.CategoryFolders[Category - 1];
        public string ScenePath => $"Assets/00_Scenes/{Folder}/{Scene}.unity";

        // 개념 설명과 확인 포인트 (= 테스트 방법). 본문은 GlossaryCatalog.Guide.cs
        public string Concept => GlossaryCatalog.ConceptOf(Id);
        public string Check => GlossaryCatalog.CheckOf(Id);
    }

    public static partial class GlossaryCatalog
    {
        public const string HubScene = "0000_Hub";
        public const string HubScenePath = "Assets/00_Scenes/0000_Hub.unity";

        public static bool TryFind(string id, out GlossaryEntry entry)
        {
            foreach (var e in Entries)
            {
                if (e.Id == id)
                {
                    entry = e;
                    return true;
                }
            }
            entry = default;
            return false;
        }

        public static GlossaryEntry Find(string id)
        {
            if (TryFind(id, out var entry)) return entry;
            Debug.LogError($"[GlossaryCatalog] id {id} 없음");
            return default;
        }
    }
}
