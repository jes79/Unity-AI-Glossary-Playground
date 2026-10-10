using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.AnimationDemo
{
    // Blend Tree 데모: 같은 Speed 값을 두 캐릭터에 넣고 비교
    // - 왼쪽: 1D Blend Tree (Idle 0 / Walk 2 / Run 5) → 사이 값에서는 두 클립을 비율대로 섞음
    // - 오른쪽: 상태 3개를 Speed 문턱값으로 딱딱 전환 → 문턱 근처에서 동작이 '툭' 바뀜
    public class BlendTreeDemo : MonoBehaviour
    {
        [SerializeField] private Animator blendTree;
        [SerializeField] private Animator hardSwitch;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float[] thresholds = { 0f, 2f, 5f }; // Idle, Walk, Run

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private float speed;

        private void Start()
        {
            if (speedSlider != null) speedSlider.onValueChanged.AddListener(v => speed = v);
        }

        private void Update()
        {
            // W/S 로 속도 조절 (슬라이더와 동기화)
            float input = DemoInput.Move.y;
            if (Mathf.Abs(input) > 0.01f)
            {
                speed = Mathf.Clamp(speed + input * 3f * Time.deltaTime, 0f, maxSpeed);
                if (speedSlider != null) speedSlider.SetValueWithoutNotify(speed);
            }
            if (DemoInput.KeyDown(Key.Digit1)) SetSpeed(0f);
            if (DemoInput.KeyDown(Key.Digit2)) SetSpeed(2f);
            if (DemoInput.KeyDown(Key.Digit3)) SetSpeed(3.5f);
            if (DemoInput.KeyDown(Key.Digit4)) SetSpeed(5f);

            blendTree.SetFloat(SpeedHash, speed);
            hardSwitch.SetFloat(SpeedHash, speed);

            GlossaryInfoPanel.Status = $"Speed = {speed:0.00}\n블렌드 비율  {Weights(speed)}";
        }

        public void SetSpeed(float v)
        {
            speed = v;
            if (speedSlider != null) speedSlider.SetValueWithoutNotify(v);
        }

        // 1D Blend Tree 가 내부에서 하는 계산: 양옆 문턱값 사이에서 선형 보간
        private string Weights(float s)
        {
            string[] names = { "Idle", "Walk", "Run" };
            var w = new float[3];
            if (s <= thresholds[0]) w[0] = 1f;
            else if (s >= thresholds[2]) w[2] = 1f;
            else
            {
                int i = s < thresholds[1] ? 0 : 1;
                float t = Mathf.InverseLerp(thresholds[i], thresholds[i + 1], s);
                w[i] = 1f - t;
                w[i + 1] = t;
            }
            return $"{names[0]} {w[0] * 100f:0}%  {names[1]} {w[1] * 100f:0}%  {names[2]} {w[2] * 100f:0}%";
        }
    }
}
