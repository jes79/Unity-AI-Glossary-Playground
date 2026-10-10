using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Texture Sheet Animation: 한 장의 텍스처를 4x4 칸으로 나눠 입자 수명 동안 0→15번 칸을 차례로 보여 줌
    // OFF 로 하면 시트 전체(16칸)가 한 입자에 통째로 찍히는 것을 볼 수 있음
    public class TextureSheetDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem explosion;
        [SerializeField] private bool useSheetAnimation = true;
        [SerializeField, Range(1, 3)] private int cycles = 1;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space) || (DemoInput.LeftClick && !FxUtil.PointerOverUI)) Explode();
            if (DemoInput.KeyDown(Key.A)) ToggleSheet();
            if (DemoInput.KeyDown(Key.UpArrow)) { cycles = Mathf.Min(3, cycles + 1); Apply(); }
            if (DemoInput.KeyDown(Key.DownArrow)) { cycles = Mathf.Max(1, cycles - 1); Apply(); }

            float life = explosion.main.startLifetime.constant;
            GlossaryInfoPanel.Status = $"Texture Sheet Animation {FxUtil.OnOff(useSheetAnimation)}   Tiles 4 x 4 = 16프레임, Cycles {cycles}\n" +
                                       $"수명 {life:0.0}초 동안 {16 * cycles}프레임 → 약 {16 * cycles / life:0}fps";
        }

        public void Explode() => explosion.Emit(1);
        public void ToggleSheet() { useSheetAnimation = !useSheetAnimation; Apply(); }

        private void Apply()
        {
            var tsa = explosion.textureSheetAnimation;
            tsa.enabled = useSheetAnimation;
            tsa.numTilesX = 4;
            tsa.numTilesY = 4;
            tsa.animation = ParticleSystemAnimationType.WholeSheet;
            tsa.cycleCount = cycles;
        }
    }
}
