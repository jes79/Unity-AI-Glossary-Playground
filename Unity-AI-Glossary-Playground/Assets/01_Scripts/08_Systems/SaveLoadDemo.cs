using System;
using System.Collections.Generic;
using System.IO;
using GlossaryDemo.EnemyAI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Systems
{
    // 저장할 데이터 묶음 (JsonUtility 는 [Serializable] 클래스의 public 필드를 JSON 으로 바꿈)
    [Serializable]
    public class SaveData
    {
        public string savedAt;
        public Vector3 position;
        public int hp;
        public List<SavedItem> inventory = new List<SavedItem>();
    }

    // 아이템은 에셋 참조 대신 "id 문자열 + 개수"로 저장 (에셋 참조는 JSON 에 안정적으로 못 담음)
    [Serializable]
    public class SavedItem
    {
        public string id;
        public int count;
    }

    // 세이브 / 로드: 플레이어 위치·체력·인벤토리를 JSON 파일로 저장 (Application.persistentDataPath)
    public class SaveLoadDemo : MonoBehaviour
    {
        [SerializeField] private string fileName = "save_0812.json";
        [SerializeField] private AITopDownPlayer player;
        [SerializeField] private ItemData[] itemDatabase;   // id → ItemData 찾기용
        [SerializeField] private int maxHp = 100;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private TextMeshProUGUI jsonText;
        [SerializeField] private TextMeshProUGUI pathText;

        private int hp = 100;
        private Inventory inventory;
        private string log = "";

        private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

        private void Awake() => inventory = new Inventory(20);

        private void Start()
        {
            if (pathText != null) pathText.text = $"저장 위치: {SavePath}";
            ShowFile();
            Refresh("WASD 로 움직이고 상태를 바꾼 뒤 F5 로 저장, F9 로 불러오기");
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.H)) { hp = Mathf.Max(0, hp - 10); Refresh("HP -10"); }
            if (DemoInput.KeyDown(Key.J)) { hp = Mathf.Min(maxHp, hp + 10); Refresh("HP +10"); }
            for (int i = 0; i < itemDatabase.Length && i < 3; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) { inventory.Add(itemDatabase[i], i == 2 ? 1 : 5); Refresh($"{itemDatabase[i].itemName} 획득"); }
            if (DemoInput.KeyDown(Key.F5)) Save();
            if (DemoInput.KeyDown(Key.F9)) Load();
            if (DemoInput.KeyDown(Key.Delete)) DeleteSave();

            if (stateText != null)
                stateText.text = $"<b>현재 상태</b>\n위치 ({player.transform.position.x:0.0}, {player.transform.position.z:0.0})\nHP {hp}/{maxHp}\n인벤토리: {InventorySummary()}";
        }

        public void Save()
        {
            var data = new SaveData
            {
                savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                position = player.transform.position,
                hp = hp,
            };
            foreach (var s in inventory.slots)
                if (s != null && !s.IsEmpty) data.inventory.Add(new SavedItem { id = s.item.id, count = s.count });

            string json = JsonUtility.ToJson(data, true); // true = 보기 좋게 줄바꿈
            File.WriteAllText(SavePath, json);
            ShowFile();
            Refresh($"<color=#7CFC8A>저장 완료</color> ({json.Length} 글자)");
        }

        public void Load()
        {
            if (!File.Exists(SavePath)) { Refresh("<color=#FF7A7A>저장 파일이 없음</color>"); return; }
            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                player.Teleport(data.position);
                hp = data.hp;
                inventory.Clear();
                foreach (var si in data.inventory)
                {
                    var item = Array.Find(itemDatabase, d => d.id == si.id); // id 로 에셋을 다시 찾음
                    if (item != null) inventory.Add(item, si.count);
                }
                Refresh($"<color=#7CFC8A>불러오기 완료</color> (저장 시각 {data.savedAt})");
            }
            catch (Exception e)
            {
                Refresh($"<color=#FF7A7A>불러오기 실패: {e.Message}</color>"); // 파일이 깨졌을 때 대비
            }
        }

        public void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            ShowFile();
            Refresh("저장 파일 삭제");
        }

        private void ShowFile()
        {
            if (jsonText == null) return;
            jsonText.text = File.Exists(SavePath) ? File.ReadAllText(SavePath) : "(저장 파일 없음)";
        }

        private string InventorySummary()
        {
            var parts = new List<string>();
            foreach (var s in inventory.slots)
                if (s != null && !s.IsEmpty) parts.Add($"{s.item.itemName}×{s.count}");
            return parts.Count == 0 ? "비어 있음" : string.Join(", ", parts);
        }

        private void Refresh(string msg)
        {
            log = msg;
            GlossaryInfoPanel.Status = log;
        }
    }
}
