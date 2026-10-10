using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0419 트라이플래너 + 위쪽에만 이끼
    // T: 트라이플래너 ↔ 메쉬 UV 비교 (UV 모드에서는 늘어난 절벽과 바위 이음새가 드러남)
    // 바위가 굴러도 이끼는 항상 월드 기준 "위쪽" 면에 다시 자리잡음 (월드 법선 Y 로 마스크를 만들기 때문)
    public class TriplanarDemo : MonoBehaviour
    {
        [SerializeField] private Renderer[] targets;
        [SerializeField] private ShaderDemoSpinner[] rollingRocks;
        [SerializeField, Range(0f, 1f)] private float mossAmount = 0.5f;
        [SerializeField] private float tiling = 0.6f;
        [SerializeField, Range(1f, 16f)] private float blendSharpness = 4f;
        [SerializeField] private bool useTriplanar = true;
        [SerializeField] private bool useMoss = true;
        [SerializeField] private bool rolling = true;

        private Material mat;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(targets[0].sharedMaterial, targets);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.T)) ToggleTriplanar();
            if (DemoInput.KeyDown(Key.M)) ToggleMoss();
            if (DemoInput.KeyDown(Key.Space)) ToggleRolling();
        }

        public void SetMossAmount(float v) { mossAmount = v; Apply(); }
        public void SetTiling(float v) { tiling = v; Apply(); }
        public void SetSharpness(float v) { blendSharpness = v; Apply(); }
        public void ToggleTriplanar() { useTriplanar = !useTriplanar; Apply(); }
        public void ToggleMoss() { useMoss = !useMoss; Apply(); }
        public void ToggleRolling()
        {
            rolling = !rolling;
            foreach (var s in rollingRocks) s.Paused = !rolling;
            Apply();
        }

        private void Apply()
        {
            if (mat == null) return;
            mat.SetFloat("_MossAmount", mossAmount);
            mat.SetFloat("_Tiling", tiling);
            mat.SetFloat("_BlendSharpness", blendSharpness);
            mat.SetFloat("_UseTriplanar", useTriplanar ? 1f : 0f);
            mat.SetFloat("_UseMoss", useMoss ? 1f : 0f);
            GlossaryInfoPanel.Status = $"매핑: <b>{(useTriplanar ? "트라이플래너 (월드 XYZ 투영)" : "메쉬 UV")}</b>   이끼 {ShaderDemoUtil.OnOff(useMoss)} 양 {mossAmount:0.00}\n" +
                                       $"Tiling {tiling:0.00}   경계 날카로움 {blendSharpness:0.0}   바위 회전 {ShaderDemoUtil.OnOff(rolling)}";
        }
    }
}
