namespace GlossaryDemo.Patterns
{
    // 상호작용 대상의 공통 약속(인터페이스)
    // 문·상자·NPC·레버가 서로 다른 클래스여도 플레이어는 IInteractable 하나로 똑같이 다룸
    public interface IInteractable
    {
        string Prompt { get; }                  // "문 열기" 같은 안내 문구
        void Interact(InteractorPlayer player); // E 키를 눌렀을 때 할 일
    }
}
