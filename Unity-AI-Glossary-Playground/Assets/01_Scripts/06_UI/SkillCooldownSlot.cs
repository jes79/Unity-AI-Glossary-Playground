using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 스킬 쿨타임 UI 한 칸
    // 아이콘 위에 어두운 Image(Type = Filled, Fill Method = Radial 360)를 덮고 fillAmount 를 남은 시간 비율로 줄임
    public class SkillCooldownSlot : MonoBehaviour
    {
        [SerializeField] private string skillName = "스킬";
        [SerializeField, Min(0.1f)] private float cooldown = 3f;
        [SerializeField] private Image cooldownOverlay; // Filled Radial360, Origin Top, Clockwise
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private RectTransform iconRoot;

        private float remaining;

        public string SkillName => skillName;
        public float Remaining => remaining;
        public float Cooldown => cooldown;
        public bool Ready => remaining <= 0f;

        // true: 남은 시간만큼 어둡게 덮고 점점 걷힘 / false: 0 에서 차오름
        public static bool ShowRemaining = true;

        private void Start() => Refresh();

        public void Use()
        {
            if (!Ready)
            {
                StartCoroutine(Punch(0.92f)); // 아직 쿨타임: 살짝 눌리는 반응만
                return;
            }
            remaining = cooldown;
            StartCoroutine(Punch(1.15f));
        }

        public void SetFillMethod(Image.FillMethod method, int origin)
        {
            cooldownOverlay.fillMethod = method;
            cooldownOverlay.fillOrigin = origin;
            Refresh();
        }

        private void Update()
        {
            if (remaining > 0f)
            {
                remaining = Mathf.Max(0f, remaining - Time.deltaTime);
                if (remaining <= 0f) StartCoroutine(Punch(1.2f)); // 다시 사용 가능 알림
            }
            Refresh();
        }

        private void Refresh()
        {
            float ratio = remaining / cooldown;
            cooldownOverlay.fillAmount = ShowRemaining ? ratio : (Ready ? 0f : 1f - ratio);
            timeText.text = Ready ? "" : remaining >= 1f ? $"{remaining:0}" : $"{remaining:0.0}";
        }

        private IEnumerator Punch(float scale)
        {
            yield return Ease.Run(0.25f, Ease.OutQuad, t =>
            {
                float s = Mathf.Lerp(scale, 1f, t);
                iconRoot.localScale = new Vector3(s, s, 1f);
            });
        }
    }
}
