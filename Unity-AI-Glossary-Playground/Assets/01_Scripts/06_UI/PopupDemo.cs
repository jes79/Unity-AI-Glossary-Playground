using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // 같은 ConfirmPopup 하나를 여러 상황(나가기, 삭제, 구매)에서 메시지·콜백만 바꿔 재사용
    public class PopupDemo : MonoBehaviour
    {
        [SerializeField] private ConfirmPopup popup;
        [SerializeField] private TextMeshProUGUI itemText;
        [SerializeField] private TextMeshProUGUI backgroundCounter;
        [SerializeField] private TextMeshProUGUI modalLabel;

        private readonly List<string> log = new();
        private int swords = 3;
        private int gold = 1000;
        private int backgroundClicks;

        private void Start()
        {
            Refresh();
            ApplyModalLabel();
        }

        private void Update()
        {
            if (!popup.IsOpen && DemoInput.KeyDown(Key.Q)) AskQuit();
            if (DemoInput.KeyDown(Key.M)) ToggleModal();

            GlossaryInfoPanel.Status = $"팝업 {(popup.IsOpen ? "열림" : "닫힘")}   모달 {(popup.Modal ? "ON" : "<color=#FF6060>OFF</color>")}\n" + string.Join("\n", log);
        }

        public void AskQuit()
        {
            popup.Show("게임 종료", "정말 나가시겠습니까?\n저장하지 않은 진행은 사라집니다.",
                () => Log("확인 → 나가기 처리 (데모라 실제로 나가지 않음)"),
                () => Log("취소 → 게임 계속"),
                "나가기", "취소");
        }

        public void AskDelete()
        {
            if (swords <= 0) { Log("삭제할 검이 없음"); return; }
            popup.Show("아이템 삭제", "<color=#FFB74D>전설의 검</color> 1개를 삭제할까요?\n되돌릴 수 없습니다.",
                () => { swords--; Refresh(); Log("확인 → 전설의 검 삭제"); },
                () => Log("취소 → 삭제 안 함"),
                "삭제", "취소");
        }

        public void AskBuy()
        {
            popup.Show("구매 확인", "체력 물약을 <color=#FFD54F>300 G</color>에 구매할까요?",
                () =>
                {
                    if (gold < 300) { Log("골드 부족"); return; }
                    gold -= 300; Refresh(); Log("확인 → 물약 구매");
                },
                () => Log("취소 → 구매 안 함"),
                "구매", "닫기");
        }

        // 팝업 뒤에 있는 버튼: 모달이면 Blocker 에 막혀 눌리지 않음
        public void BackgroundClick()
        {
            backgroundClicks++;
            backgroundCounter.text = $"뒤 버튼 클릭 수: {backgroundClicks}";
            if (popup.IsOpen) Log("<color=#FF6060>팝업이 떠 있는데 뒤 버튼이 눌림!</color> (모달 OFF)");
        }

        public void ToggleModal()
        {
            popup.SetModal(!popup.Modal);
            ApplyModalLabel();
        }

        private void ApplyModalLabel()
        {
            if (modalLabel != null) modalLabel.text = popup.Modal ? "모달 차단막 ON (M)" : "모달 차단막 OFF (M)";
        }

        private void Refresh() => itemText.text = $"전설의 검 x{swords}     골드 {gold:N0} G";

        private void Log(string message)
        {
            log.Add(message);
            if (log.Count > 4) log.RemoveAt(0);
        }
    }
}
