using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // 에디터 전용 작업(Git, 빌드, 폰트 에셋 생성 등)을 씬 안에서 "단계별 페이지"로 보여 주는 패널
    // ← / → 키 또는 이전/다음 버튼으로 넘김
    public class GuidePages : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI pageText;
        [SerializeField] private string[] titles;
        [SerializeField, TextArea(3, 12)] private string[] bodies;
        [SerializeField] private bool arrowKeys = true;

        private int page;

        public int Page => page;
        public int Count => titles?.Length ?? 0;

        private void Start() => Show(0);

        private void Update()
        {
            if (!arrowKeys) return;
            if (DemoInput.KeyDown(Key.RightArrow)) Next();
            if (DemoInput.KeyDown(Key.LeftArrow)) Prev();
        }

        public void Next() => Show(Mathf.Min(page + 1, Count - 1));
        public void Prev() => Show(Mathf.Max(page - 1, 0));

        public void Show(int index)
        {
            if (Count == 0) return;
            page = Mathf.Clamp(index, 0, Count - 1);
            titleText.text = titles[page];
            bodyText.text = page < bodies.Length ? bodies[page] : "";
            if (pageText != null) pageText.text = $"{page + 1} / {Count}   (← / →)";
        }
    }
}
