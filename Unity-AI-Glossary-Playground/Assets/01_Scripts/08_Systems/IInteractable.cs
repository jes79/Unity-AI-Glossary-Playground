using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 상호작용 가능한 모든 것(문, 상자, NPC ...)의 공통 규칙
    // 플레이어(Interactor)는 가까운 IInteractable 을 찾아 Prompt 를 띄우고, E 를 누르면 Interact() 만 호출
    public interface IInteractable
    {
        string Prompt { get; }              // "문 열기", "상자 열기", "대화하기"
        bool CanInteract { get; }
        Vector3 PromptPosition { get; }     // 안내 표시를 띄울 위치
        void Interact(Interactor who);
    }
}
