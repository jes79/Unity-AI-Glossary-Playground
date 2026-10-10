using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 퀘스트 데이터 (ScriptableObject). 새 퀘스트 = 이 에셋을 하나 더 만들어 QuestManager 목록에 넣기만 하면 됨
    [CreateAssetMenu(menuName = "Glossary/Quest Data", fileName = "NewQuest")]
    public class QuestData : ScriptableObject
    {
        public string questId = "quest";
        public string title = "슬라임 10마리 처치";
        [TextArea] public string description;

        [Header("목표")]
        public string targetId = "slime"; // 이 id 의 이벤트(처치/채집)가 오면 진행도 +1
        public int requiredCount = 10;

        [Header("보상")]
        public int rewardGold = 100;
        public int rewardXp = 50;
    }
}
