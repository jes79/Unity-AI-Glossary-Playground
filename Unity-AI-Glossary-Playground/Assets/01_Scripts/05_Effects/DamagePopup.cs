using TMPro;
using UnityEngine;

namespace GlossaryDemo.Effects
{
    // 데미지 숫자 하나: 튀어나오며 커졌다(OutBack) → 위로 떠오르며 감속 → 마지막에 투명해지며 사라짐
    // 월드 공간 TextMeshPro + 항상 카메라를 바라보는 빌보드
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshPro text;
        private Transform cam;
        private Vector3 velocity;
        private float age, lifetime = 0.9f, scale = 1f;
        private Color color;

        public static DamagePopup Create(Vector3 position, int damage, bool critical, float criticalScale)
        {
            var go = new GameObject(critical ? "CritPopup" : "DamagePopup");
            go.transform.position = position;
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = GlossaryInfoPanel.Font;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);
            tmp.fontSize = 6f;
            tmp.sortingOrder = 10;

            var popup = go.AddComponent<DamagePopup>();
            popup.text = tmp;
            popup.cam = Camera.main != null ? Camera.main.transform : null;
            // 치명타: 크고 빨갛게 + 느낌표, 조금 더 오래
            popup.color = critical ? new Color(1f, 0.25f, 0.2f) : Color.white;
            popup.scale = critical ? criticalScale : 1f;
            popup.lifetime = critical ? 1.1f : 0.9f;
            tmp.text = critical ? $"{damage}!" : damage.ToString();
            tmp.color = popup.color;
            // 살짝 옆으로 퍼지게: 여러 숫자가 겹치지 않음
            popup.velocity = new Vector3(Random.Range(-0.8f, 0.8f), critical ? 3.2f : 2.6f, 0f);
            go.transform.localScale = Vector3.zero;
            return popup;
        }

        private void LateUpdate()
        {
            age += Time.deltaTime;
            float t = age / lifetime;
            if (t >= 1f) { Destroy(gameObject); return; }

            // 1) 처음 0.15초: 0 → 목표 크기로 튀어나옴
            float pop = Ease.OutBack(Mathf.Clamp01(age / 0.15f));
            transform.localScale = Vector3.one * scale * pop;

            // 2) 위로 떠오르며 감속
            transform.position += velocity * Time.deltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, 1f - Mathf.Exp(-4f * Time.deltaTime));

            // 3) 마지막 40% 동안 투명해짐
            var c = color;
            c.a = 1f - Mathf.Clamp01((t - 0.6f) / 0.4f);
            text.color = c;

            if (cam != null) transform.rotation = cam.rotation; // 빌보드
        }
    }
}
