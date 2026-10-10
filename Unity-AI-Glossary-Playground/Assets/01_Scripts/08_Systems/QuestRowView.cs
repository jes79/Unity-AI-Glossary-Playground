using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 퀘스트 한 줄 UI (제목, 설명, 진행 막대, 버튼)
    public class QuestRowView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonLabel;
        [SerializeField] private Image background;

        private QuestManager owner;
        private int index;

        public void Bind(QuestManager owner, int index)
        {
            this.owner = owner;
            this.index = index;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => owner.OnRowButton(index));
        }

        public void Refresh()
        {
            var p = owner.Get(index);
            titleText.text = p.data.title;
            descText.text = $"{p.data.description}\n<color=#FFD54A>보상 {p.data.rewardGold} G · {p.data.rewardXp} XP</color>";
            progressFill.fillAmount = (float)p.count / p.data.requiredCount;
            progressText.text = $"{p.count} / {p.data.requiredCount}";

            (string label, Color color, bool interactable) = p.state switch
            {
                QuestManager.QuestState.Available => ("수락", new Color(0.35f, 0.55f, 1f), true),
                QuestManager.QuestState.InProgress => ("진행 중", new Color(0.4f, 0.4f, 0.45f), false),
                QuestManager.QuestState.Completed => ("보상 받기", new Color(0.3f, 0.75f, 0.4f), true),
                _ => ("완료 (다시)", new Color(0.55f, 0.45f, 0.2f), true),
            };
            buttonLabel.text = label;
            button.image.color = color;
            button.interactable = interactable;
            if (background != null)
                background.color = p.state == QuestManager.QuestState.Completed ? new Color32(45, 70, 50, 255) : new Color32(45, 49, 61, 255);
        }
    }
}
