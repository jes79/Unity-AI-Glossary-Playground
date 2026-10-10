using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 아이템 카드 UI: 연결된 ItemData(ScriptableObject) 의 값을 읽어 그대로 표시
    // 카드 6장이 모두 같은 스크립트 → 데이터 에셋만 바꿔 끼우면 다른 아이템이 됨
    public class ItemCardView : MonoBehaviour
    {
        [SerializeField] private ItemData data;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private ScriptableObjectDemo owner;

        public ItemData Data => data;

        private void Start() => Refresh();

        public void Refresh()
        {
            if (data == null) return;
            if (icon != null) icon.sprite = data.icon;
            if (nameText != null)
            {
                nameText.text = data.itemName;
                nameText.color = data.RarityColor;
            }
            if (infoText != null)
                infoText.text = $"<color=#9AA3B5>{data.type} · {data.rarity}</color>\n" +
                                $"가격 <color=#FFD54A>{data.price} G</color>   최대 스택 {data.maxStack}\n" +
                                $"{data.EffectText}\n<size=80%><color=#B8BECC>{data.description}</color></size>";
        }

        public void Use() => owner.UseItem(data);
    }
}
