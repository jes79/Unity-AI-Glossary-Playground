using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 상자: 한 번 열면 코인 획득, 이후엔 빈 상자
    public class ChestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform lid;
        [SerializeField] private int coins = 10;

        private bool opened;

        public string Prompt => opened ? "빈 상자 살펴보기" : "상자 열기";

        public void Interact(InteractorPlayer player)
        {
            if (opened)
            {
                player.ShowMessage("상자는 비어 있다");
                return;
            }
            opened = true;
            player.Coins += coins;
            player.ShowMessage($"코인 {coins}개를 얻었다!");
        }

        private void Update()
        {
            var target = Quaternion.Euler(opened ? 110f : 0f, 0f, 0f);
            lid.localRotation = Quaternion.Slerp(lid.localRotation, target, 6f * Time.deltaTime);
        }
    }
}
