using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // NPC: 말을 걸 때마다 다음 대사
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "주민";
        [SerializeField, TextArea] private string[] lines = { "안녕!", "날씨가 좋네." };

        private int index;

        public string Prompt => $"{npcName}와 대화";

        public void Interact(InteractorPlayer player)
        {
            player.ShowMessage($"{npcName}: {lines[index]}");
            index = (index + 1) % lines.Length;
            // 말할 때 플레이어 쪽을 봄
            Vector3 to = player.transform.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(to);
        }
    }
}
