using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Patterns
{
    // 화면 슬라이더 ↔ InspectorAttributeDemo 값 양방향 연결
    // (빌드에는 Inspector 가 없으므로 같은 값을 화면에서도 조절. Inspector 에서 바꾸면 슬라이더도 따라 움직임)
    public class InspectorAttributeUI : MonoBehaviour
    {
        [SerializeField] private InspectorAttributeDemo target;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private Slider radiusSlider;
        [SerializeField] private Slider bobSlider;
        [SerializeField] private Slider sizeSlider;

        private void Update()
        {
            speedSlider.SetValueWithoutNotify(target.OrbitSpeed);
            radiusSlider.SetValueWithoutNotify(target.OrbitRadius);
            bobSlider.SetValueWithoutNotify(target.BobHeight);
            sizeSlider.SetValueWithoutNotify(target.Size);
        }

        public void SetSpeed(float v) => target.OrbitSpeed = v;
        public void SetRadius(float v) => target.OrbitRadius = v;
        public void SetBob(float v) => target.BobHeight = v;
        public void SetSize(float v) => target.Size = v;
    }
}
