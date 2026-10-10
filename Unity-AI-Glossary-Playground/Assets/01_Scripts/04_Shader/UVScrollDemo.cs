using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0408 UV / Tiling / Offset
    // UV     : 메쉬 각 점이 텍스처의 어느 좌표(0~1)를 읽을지
    // Tiling : UV 에 곱하는 값 → 2 면 텍스처가 2번 반복
    // Offset : UV 에 더하는 값 → 매 프레임 늘리면 텍스처가 미끄러져 "흐르는 물"이 됨
    // 실제 셰이더 계산: uv = 원래UV × Tiling + Offset  (URP 머티리얼의 _BaseMap_ST 값)
    public class UVScrollDemo : MonoBehaviour
    {
        [SerializeField] private Renderer river;
        [SerializeField] private Renderer waterfall;
        [SerializeField] private Renderer checkerCube;

        [Header("Tiling / Offset")]
        [SerializeField] private Vector2 tiling = new Vector2(2f, 4f);
        [SerializeField] private float flowSpeed = 0.4f; // 초당 UV 이동량
        [SerializeField] private bool flowing = true;

        private Material riverMat, fallMat, checkerMat;
        private Vector2 offset;
        private float fallOffset;

        private void Start()
        {
            riverMat = ShaderDemoUtil.CloneShared(river.sharedMaterial, river);
            fallMat = ShaderDemoUtil.CloneShared(waterfall.sharedMaterial, waterfall);
            checkerMat = ShaderDemoUtil.CloneShared(checkerCube.sharedMaterial, checkerCube);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) ToggleFlow();

            // 핵심: Offset 을 시간에 따라 늘림 → 텍스처가 계속 흘러감 (1 을 넘어도 Repeat 랩 모드라 이어짐)
            if (flowing)
            {
                offset.y = Mathf.Repeat(offset.y - flowSpeed * Time.deltaTime, 1f);
                fallOffset = Mathf.Repeat(fallOffset - flowSpeed * 2.5f * Time.deltaTime, 1f);
            }

            riverMat.SetTextureScale("_BaseMap", tiling);
            riverMat.SetTextureOffset("_BaseMap", offset);
            // 폭포는 같은 원리로 2.5배 빠르게 (따로 누적해야 1 을 넘을 때 끊기지 않음)
            fallMat.SetTextureScale("_BaseMap", new Vector2(tiling.x, tiling.y * 0.5f));
            fallMat.SetTextureOffset("_BaseMap", new Vector2(0f, fallOffset));
            // UV 확인용 큐브에도 같은 값을 넣어 숫자 변화가 눈에 보이게
            checkerMat.SetTextureScale("_BaseMap", tiling * 0.5f);
            checkerMat.SetTextureOffset("_BaseMap", offset);

            GlossaryInfoPanel.Status =
                $"Tiling ({tiling.x:0.0}, {tiling.y:0.0})   Offset ({offset.x:0.00}, {offset.y:0.00})   흐름 {ShaderDemoUtil.OnOff(flowing)}\n" +
                $"속도 {flowSpeed:0.00} UV/초   셰이더 계산: uv × Tiling + Offset";
        }

        public void SetTilingX(float value) => tiling.x = value;
        public void SetTilingY(float value) => tiling.y = value;
        public void SetFlowSpeed(float value) => flowSpeed = value;
        public void ToggleFlow() => flowing = !flowing;
    }
}
