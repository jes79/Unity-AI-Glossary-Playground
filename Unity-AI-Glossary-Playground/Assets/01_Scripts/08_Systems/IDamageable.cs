using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // "맞을 수 있는 것"의 공통 규칙. 공격하는 쪽은 상대가 플레이어인지 적인지 상자인지 몰라도 됨
    //   if (hit.collider.TryGetComponent(out IDamageable d)) d.TakeDamage(10, point);
    public interface IDamageable
    {
        bool IsAlive { get; }
        string Summary { get; } // 상태 표시용
        void TakeDamage(int amount, Vector3 hitPoint);
    }

    // 데미지 숫자 팝업 (위로 떠오르며 사라짐)
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshPro text;
        private float life;

        public static void Spawn(Vector3 pos, string msg, Color color, float size = 6f)
        {
            var go = new GameObject("DamagePopup");
            go.transform.position = pos;
            var cam = Camera.main;
            if (cam != null) go.transform.rotation = cam.transform.rotation;
            var p = go.AddComponent<DamagePopup>();
            p.text = go.AddComponent<TextMeshPro>();
            p.text.font = GlossaryInfoPanel.Font;
            p.text.text = msg;
            p.text.fontSize = size;
            p.text.color = color;
            p.text.alignment = TextAlignmentOptions.Center;
            p.text.fontStyle = FontStyles.Bold;
            p.text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private void Update()
        {
            life += Time.deltaTime;
            transform.position += Vector3.up * Time.deltaTime * 1.5f;
            var c = text.color;
            c.a = 1f - life / 0.9f;
            text.color = c;
            if (life > 0.9f) Destroy(gameObject);
        }
    }
}
