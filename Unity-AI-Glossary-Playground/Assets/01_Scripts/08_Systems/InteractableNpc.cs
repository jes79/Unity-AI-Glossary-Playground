using UnityEngine;

namespace GlossaryDemo.Systems
{
    // NPC: E 를 누를 때마다 다음 대사, 끝나면 대화창 닫기
    public class InteractableNpc : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "촌장";
        [SerializeField, TextArea] private string[] lines =
        {
            "어서 오게, 모험가여.",
            "이 마을의 문과 상자도 모두 E 키로 다룰 수 있다네.",
            "같은 키, 같은 표시… 하지만 하는 일은 저마다 다르지. 그게 인터페이스라네!",
        };

        private int index = -1;

        public string Prompt => index < 0 ? "대화하기" : "다음";
        public bool CanInteract => true;
        public Vector3 PromptPosition => transform.position + Vector3.up * 2.4f;

        public void Interact(Interactor who)
        {
            index++;
            if (index >= lines.Length)
            {
                index = -1;
                who.ShowDialog(null); // 대화 끝
                return;
            }
            who.ShowDialog($"<color=#FFD54A>{npcName}</color>\n{lines[index]}  <size=70%>({index + 1}/{lines.Length})</size>");
        }
    }
}
