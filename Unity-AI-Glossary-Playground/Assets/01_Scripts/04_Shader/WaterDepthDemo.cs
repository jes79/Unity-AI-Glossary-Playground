using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0417 물 셰이더 (Depth)
    // 카메라 Depth Texture 로 "수면 아래 바닥까지 거리"를 구해 얕은 곳은 밝은 청록, 깊은 곳은 짙은 남색으로
    // D: Depth 사용을 끄면 물 전체가 같은 색 → 깊이 정보가 얼마나 중요한지 비교
    // 떠다니는 상자가 수면을 오르내리면 닿는 경계에 거품이 생김 (거품도 Depth 차이로 계산)
    public class WaterDepthDemo : MonoBehaviour
    {
        [SerializeField] private Renderer water;
        [SerializeField] private Transform floatingBox;
        [SerializeField, Range(0.1f, 10f)] private float depthMaxDistance = 2.5f;
        [SerializeField, Range(0f, 2f)] private float foamDistance = 0.35f;
        [SerializeField] private bool useDepth = true;

        private Material mat;
        private Vector3 boxStart;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(water.sharedMaterial, water);
            if (floatingBox != null) boxStart = floatingBox.position;
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.D)) ToggleDepth();
            if (floatingBox != null)
            {
                floatingBox.position = boxStart + Vector3.up * (Mathf.Sin(Time.time * 0.8f) * 0.6f);
                floatingBox.rotation = Quaternion.Euler(Mathf.Sin(Time.time * 0.6f) * 10f, Time.time * 15f, 0f);
            }
        }

        public void SetDepthMax(float v) { depthMaxDistance = v; Apply(); }
        public void SetFoam(float v) { foamDistance = v; Apply(); }
        public void ToggleDepth() { useDepth = !useDepth; Apply(); }

        private void Apply()
        {
            if (mat == null) return;
            mat.SetFloat("_DepthMaxDistance", depthMaxDistance);
            mat.SetFloat("_FoamDistance", foamDistance);
            mat.SetFloat("_UseDepth", useDepth ? 1f : 0f);
            GlossaryInfoPanel.Status = $"Depth 사용 {ShaderDemoUtil.OnOff(useDepth)}   최대 깊이 {depthMaxDistance:0.0}m (이보다 깊으면 Deep 색)   거품 거리 {foamDistance:0.00}m\n" +
                                       "물 깊이 = 바닥 깊이(Depth Texture) - 수면 깊이";
        }
    }
}
