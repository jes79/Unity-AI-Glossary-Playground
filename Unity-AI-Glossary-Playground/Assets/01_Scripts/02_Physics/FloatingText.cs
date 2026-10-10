using TMPro;
using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 떠오르며 사라지는 글자 (데미지 숫자, "+10" 등). FloatingText.Spawn(위치, 글자, 색)
    public class FloatingText : MonoBehaviour
    {
        private TextMeshPro tmp;
        private float life = 0.9f, age;
        private Color color;

        public static void Spawn(Vector3 position, string text, Color color, float size = 5f)
        {
            var go = new GameObject("FloatingText");
            go.transform.position = position;
            var t = go.AddComponent<TextMeshPro>();
            t.font = GlossaryInfoPanel.Font;
            t.text = text;
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.rectTransform.sizeDelta = new Vector2(8f, 2f);
            t.color = color;
            t.fontStyle = FontStyles.Bold;
            var ft = go.AddComponent<FloatingText>();
            ft.tmp = t;
            ft.color = color;
            go.AddComponent<PhysicsBillboard>();
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += Vector3.up * 1.5f * Time.deltaTime;
            if (tmp != null) tmp.color = new Color(color.r, color.g, color.b, 1f - age / life);
            if (age >= life) Destroy(gameObject);
        }
    }
}
