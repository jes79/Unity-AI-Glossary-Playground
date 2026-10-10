using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0416 버텍스 애니메이션 - 바람에 흔들리는 풀
    // 풀 메쉬 자체는 움직이지 않음(Transform 고정). 버텍스 셰이더가 매 프레임 정점 위치만 옮겨서 흔들림을 만듦
    // → 풀 수천 개를 스크립트 없이 GPU 에서 한 번에 흔들 수 있음
    public class GrassWindDemo : MonoBehaviour
    {
        [SerializeField] private Renderer grass;
        [SerializeField] private Transform windArrow;
        [SerializeField, Range(0f, 1f)] private float windStrength = 0.35f;
        [SerializeField, Range(0f, 8f)] private float windSpeed = 2f;
        [SerializeField, Range(0f, 2f)] private float windFrequency = 0.6f;
        [SerializeField, Range(0f, 360f)] private float windAngle = 15f;
        [SerializeField] private bool windOn = true;

        private Material mat;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(grass.sharedMaterial, grass);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) ToggleWind();
        }

        public void SetStrength(float v) { windStrength = v; Apply(); }
        public void SetSpeed(float v) { windSpeed = v; Apply(); }
        public void SetFrequency(float v) { windFrequency = v; Apply(); }
        public void SetAngle(float v) { windAngle = v; Apply(); }
        public void ToggleWind() { windOn = !windOn; Apply(); }

        private void Apply()
        {
            if (mat == null) return;
            var dir = Quaternion.Euler(0f, windAngle, 0f) * Vector3.right;
            mat.SetFloat("_WindStrength", windOn ? windStrength : 0f);
            mat.SetFloat("_WindSpeed", windSpeed);
            mat.SetFloat("_WindFrequency", windFrequency);
            mat.SetVector("_WindDirection", new Vector4(dir.x, 0f, dir.z, 0f));
            if (windArrow != null) windArrow.rotation = Quaternion.LookRotation(dir);
            GlossaryInfoPanel.Status = $"바람 {ShaderDemoUtil.OnOff(windOn)}   세기 {windStrength:0.00}   속도 {windSpeed:0.0}   물결 간격 {windFrequency:0.00}   방향 {windAngle:0}°\n" +
                                       "버텍스 셰이더: 위치 += 바람방향 × sin(시간 + 위치) × 세기 × (높이²)";
        }
    }
}
