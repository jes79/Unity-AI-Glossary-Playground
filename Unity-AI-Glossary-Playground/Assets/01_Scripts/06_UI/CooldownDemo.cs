using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 쿨타임 UI 데모: Q/W/E 로 스킬 사용, Fill Method 를 바꿔 비교
    public class CooldownDemo : MonoBehaviour
    {
        [SerializeField] private SkillCooldownSlot[] slots;
        [SerializeField] private TextMeshProUGUI methodLabel;

        private static readonly (Image.FillMethod method, int origin, string name)[] Methods =
        {
            (Image.FillMethod.Radial360, (int)Image.Origin360.Top, "Radial 360 (Origin Top) ← 요청 예문"),
            (Image.FillMethod.Radial90, (int)Image.Origin90.BottomLeft, "Radial 90"),
            (Image.FillMethod.Vertical, (int)Image.OriginVertical.Bottom, "Vertical (아래→위)"),
            (Image.FillMethod.Horizontal, (int)Image.OriginHorizontal.Left, "Horizontal (왼→오)"),
        };
        private int methodIndex;

        private void Start() => ApplyMethod();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Q)) UseSlot(0);
            if (DemoInput.KeyDown(Key.W)) UseSlot(1);
            if (DemoInput.KeyDown(Key.E)) UseSlot(2);
            if (DemoInput.KeyDown(Key.F)) NextFillMethod();
            if (DemoInput.KeyDown(Key.R)) ToggleDirection();

            var sb = new System.Text.StringBuilder();
            foreach (var s in slots)
                sb.Append($"{s.SkillName}: {(s.Ready ? "<color=#9BE89B>사용 가능</color>" : $"{s.Remaining:0.0}/{s.Cooldown:0.0}초")}   ");
            sb.Append($"\nfillAmount = 남은시간 / 쿨타임   표시: {(SkillCooldownSlot.ShowRemaining ? "남은 만큼 덮기" : "차오르기")}");
            GlossaryInfoPanel.Status = sb.ToString();
        }

        public void UseSlot(int index)
        {
            if (index >= 0 && index < slots.Length) slots[index].Use();
        }

        public void NextFillMethod()
        {
            methodIndex = (methodIndex + 1) % Methods.Length;
            ApplyMethod();
        }

        public void ToggleDirection() => SkillCooldownSlot.ShowRemaining = !SkillCooldownSlot.ShowRemaining;

        private void ApplyMethod()
        {
            var m = Methods[methodIndex];
            foreach (var s in slots) s.SetFillMethod(m.method, m.origin);
            methodLabel.text = $"Fill Method : {m.name}";
        }

        private void OnDestroy() => SkillCooldownSlot.ShowRemaining = true;
    }
}
