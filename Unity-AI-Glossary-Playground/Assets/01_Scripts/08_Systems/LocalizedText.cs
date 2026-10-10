using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 텍스트에 "키"만 적어 두면 현재 언어의 문장으로 바꿔 줌 (Localization 패키지의 LocalizeStringEvent 역할)
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;
        private TMP_Text text;

        public string Key => key;

        private void Awake() => text = GetComponent<TMP_Text>();
        private void OnEnable() { Localization.Changed += Refresh; Refresh(); }
        private void OnDisable() => Localization.Changed -= Refresh;

        public void Refresh()
        {
            if (text == null) text = GetComponent<TMP_Text>();
            text.text = Localization.Get(key);
        }
    }
}
