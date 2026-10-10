using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // 파티클 머티리얼 블렌딩
    // Alpha Blend: 결과 = 입자색×α + 배경×(1-α)  → 겹쳐도 입자 색 이상 밝아지지 않음, 밝은 배경 위에서도 보임
    // Additive   : 결과 = 입자색×α + 배경        → 겹칠수록 밝아져 빛처럼 보임, 흰 배경 위에선 사라짐
    public class BlendModeDemo : MonoBehaviour
    {
        public enum Layout { Split, AllAlpha, AllAdditive }

        [SerializeField] private ParticleSystemRenderer left;
        [SerializeField] private ParticleSystemRenderer right;
        [SerializeField] private Material alphaMaterial;
        [SerializeField] private Material additiveMaterial;
        [SerializeField] private Layout layout = Layout.Split;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) SetLayout(0);
            if (DemoInput.KeyDown(Key.Digit2)) SetLayout(1);
            if (DemoInput.KeyDown(Key.Digit3)) SetLayout(2);
            string Name(Material m) => m == additiveMaterial ? "Additive" : "Alpha Blend";
            GlossaryInfoPanel.Status = $"왼쪽 {Name(left.sharedMaterial)}   |   오른쪽 {Name(right.sharedMaterial)}\n" +
                                       "어두운 배경: Additive 가 겹친 곳이 하얗게 빛남 / 흰 배경: Additive 는 거의 안 보임";
        }

        public void SetLayout(int i) { layout = (Layout)i; Apply(); }

        private void Apply()
        {
            left.sharedMaterial = layout == Layout.AllAdditive ? additiveMaterial : alphaMaterial;
            right.sharedMaterial = layout == Layout.AllAlpha ? alphaMaterial : additiveMaterial;
        }
    }
}
