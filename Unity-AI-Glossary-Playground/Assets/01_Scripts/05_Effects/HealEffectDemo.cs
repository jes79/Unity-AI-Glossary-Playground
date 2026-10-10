using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Effects
{
    // 힐 이펙트: 초록 입자가 몸 둘레 원에서 생겨나 Orbital Velocity(공전) + 위쪽 속도로 나선을 그리며 올라감
    // S 로 공전을 끄면 그냥 곧게 올라가는 것과 비교 가능
    public class HealEffectDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem spiral;
        [SerializeField] private ParticleSystem crosses;
        [SerializeField] private Image hpFill;
        [SerializeField] private Renderer body;

        [Header("힐")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int healAmount = 25;
        [SerializeField] private float orbitalSpeed = 4f; // 라디안/초
        [SerializeField] private bool useSpiral = true;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock mpb;
        private Color baseColor;
        private int hp = 35;
        private float shownHp, glow;

        private void Start()
        {
            mpb = new MaterialPropertyBlock();
            baseColor = body.sharedMaterial.GetColor(BaseColorId);
            shownHp = hp;
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.H) || DemoInput.KeyDown(Key.Space)) Heal();
            if (DemoInput.KeyDown(Key.D)) Damage();
            if (DemoInput.KeyDown(Key.S)) ToggleSpiral();

            // HP 바는 부드럽게 따라감, 몸은 초록빛으로 잠깐 물듦
            shownHp = Mathf.MoveTowards(shownHp, hp, maxHp * Time.deltaTime);
            hpFill.fillAmount = shownHp / maxHp;
            glow = Mathf.MoveTowards(glow, 0f, Time.deltaTime * 1.5f);
            mpb.SetColor(BaseColorId, Color.Lerp(baseColor, new Color(0.4f, 1f, 0.5f), glow));
            body.SetPropertyBlock(mpb);

            GlossaryInfoPanel.Status = $"HP {hp} / {maxHp}   나선(Orbital Y {orbitalSpeed:0}rad/s) {FxUtil.OnOff(useSpiral)}";
        }

        public void Heal()
        {
            hp = Mathf.Min(maxHp, hp + healAmount);
            glow = 1f;
            FxUtil.Replay(spiral);
            FxUtil.Replay(crosses);
        }

        public void Damage() => hp = Mathf.Max(1, hp - 20);
        public void ToggleSpiral() { useSpiral = !useSpiral; Apply(); }

        private void Apply()
        {
            var vel = spiral.velocityOverLifetime;
            vel.orbitalY = useSpiral ? orbitalSpeed : 0f;
        }
    }
}
