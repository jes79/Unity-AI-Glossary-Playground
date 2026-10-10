using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Systems
{
    // 한국어 / 영어 전환 데모. 버튼 또는 L 키로 언어를 바꾸면 LocalizedText 가 모두 갱신됨
    [DefaultExecutionOrder(-50)]
    public class LocalizationDemo : MonoBehaviour
    {
        [SerializeField] private LocalizationTable table;
        [SerializeField] private TextMeshProUGUI goldText;   // 자리 표시자({0}) 예시
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private int gold = 1250;

        private void Awake()
        {
            Localization.Table = table;
            // 저장된 언어 불러오기 (없으면 한국어)
            Localization.SetLanguage((Language)PlayerPrefs.GetInt(Localization.PrefKey, 0));
        }

        private void OnEnable() => Localization.Changed += RefreshDynamic;
        private void OnDisable() => Localization.Changed -= RefreshDynamic;
        private void Start() => RefreshDynamic();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.L)) Toggle();
            if (DemoInput.KeyDown(Key.G)) { gold += 100; RefreshDynamic(); }

            GlossaryInfoPanel.Status = $"현재 언어: <b>{Localization.Current}</b>   테이블 키 {table.entries.Count}개\n" +
                                       $"Get(\"menu_start\") = \"{Localization.Get("menu_start")}\"";
        }

        public void SetKorean() => Localization.SetLanguage(Language.Korean);
        public void SetEnglish() => Localization.SetLanguage(Language.English);
        public void Toggle() => Localization.SetLanguage(Localization.Current == Language.Korean ? Language.English : Language.Korean);

        // 숫자·날짜처럼 바뀌는 값이 들어간 문장은 코드에서 Format
        private void RefreshDynamic()
        {
            if (goldText != null) goldText.text = Localization.Format("gold_amount", gold.ToString("N0"));
            if (dateText != null)
            {
                string date;
                try
                {
                    // 날짜 형식도 언어(문화권)마다 다름: 2026년 10월 10일 / Saturday, October 10, 2026
                    var culture = new System.Globalization.CultureInfo(Localization.Current == Language.Korean ? "ko-KR" : "en-US");
                    date = System.DateTime.Now.ToString("D", culture);
                }
                catch (System.Exception)
                {
                    date = System.DateTime.Now.ToString("yyyy-MM-dd");
                }
                dateText.text = Localization.Format("today", date);
            }
        }
    }
}
