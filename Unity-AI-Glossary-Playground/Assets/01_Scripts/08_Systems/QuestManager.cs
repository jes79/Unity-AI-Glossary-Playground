using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 퀘스트 진행 관리
    // - 게임 쪽은 "무엇을 했다"만 알림: QuestManager.Report("slime")
    // - 매니저가 수락된 퀘스트 중 targetId 가 같은 것의 진행도를 올림 → 목표 달성 시 보상 받기 가능
    public class QuestManager : MonoBehaviour
    {
        public enum QuestState { Available, InProgress, Completed, Rewarded }

        public class Progress
        {
            public QuestData data;
            public QuestState state;
            public int count;
        }

        [SerializeField] private QuestData[] quests;        // ← 데이터만 추가하면 목록에 나타남
        [SerializeField] private QuestRowView rowTemplate;  // 비활성 템플릿을 복제해 줄을 만듦
        [SerializeField] private TextMeshProUGUI walletText;
        [SerializeField] private TextMeshProUGUI logText;

        private static QuestManager instance;
        private readonly List<Progress> list = new List<Progress>();
        private readonly List<QuestRowView> rows = new List<QuestRowView>();
        private int gold, xp;

        public static event Action<string> OnReported;

        private void Awake()
        {
            instance = this;
            for (int i = 0; i < quests.Length; i++)
            {
                list.Add(new Progress { data = quests[i], state = QuestState.Available });
                var row = Instantiate(rowTemplate, rowTemplate.transform.parent);
                row.gameObject.SetActive(true);
                var rt = (RectTransform)row.transform;
                rt.anchoredPosition = ((RectTransform)rowTemplate.transform).anchoredPosition + new Vector2(0f, -i * 150f);
                row.Bind(this, i);
                rows.Add(row);
            }
        }

        private void OnDestroy() { if (instance == this) instance = null; }

        private void Start() => Refresh("퀘스트를 수락하고 몬스터를 클릭해 처치하세요");

        // 게임 어디서든 호출: "slime 을 처치했다", "mushroom 을 채집했다"
        public static void Report(string targetId)
        {
            OnReported?.Invoke(targetId);
            if (instance != null) instance.Handle(targetId);
        }

        private void Handle(string targetId)
        {
            string msg = $"{targetId} 처치/채집";
            foreach (var p in list)
            {
                if (p.state != QuestState.InProgress || p.data.targetId != targetId) continue;
                p.count = Mathf.Min(p.data.requiredCount, p.count + 1);
                msg = $"'{p.data.title}' 진행도 {p.count}/{p.data.requiredCount}";
                if (p.count >= p.data.requiredCount)
                {
                    p.state = QuestState.Completed;
                    msg = $"<color=#7CFC8A>'{p.data.title}' 목표 달성! 보상을 받으세요</color>";
                }
            }
            Refresh(msg);
        }

        public Progress Get(int index) => list[index];

        // 줄의 버튼: 수락 → (진행 중) → 보상 받기
        public void OnRowButton(int index)
        {
            var p = list[index];
            switch (p.state)
            {
                case QuestState.Available:
                    p.state = QuestState.InProgress;
                    p.count = 0;
                    Refresh($"'{p.data.title}' 수락");
                    break;
                case QuestState.Completed:
                    p.state = QuestState.Rewarded;
                    gold += p.data.rewardGold;
                    xp += p.data.rewardXp;
                    Refresh($"보상: {p.data.rewardGold} G, {p.data.rewardXp} XP");
                    break;
                case QuestState.Rewarded:
                    p.state = QuestState.Available; // 데모용: 다시 받을 수 있게
                    Refresh($"'{p.data.title}' 다시 받을 수 있음");
                    break;
            }
        }

        private void Refresh(string msg)
        {
            foreach (var r in rows) r.Refresh();
            if (walletText != null) walletText.text = $"골드 {gold} G   경험치 {xp} XP";
            if (logText != null) logText.text = msg;
            int active = list.FindAll(p => p.state == QuestState.InProgress).Count;
            GlossaryInfoPanel.Status = $"퀘스트 {list.Count}개 (데이터 에셋)   진행 중 {active}개\n{msg}";
        }
    }
}
