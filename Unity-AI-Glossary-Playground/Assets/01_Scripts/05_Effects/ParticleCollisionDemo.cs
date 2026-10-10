using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Collision 모듈: 입자가 월드 콜라이더(바닥·상자)에 닿으면 튕기거나 죽게 함
    // 닿는 순간 Sub Emitter(Collision)로 물튀김을 터뜨림
    public class ParticleCollisionDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem rain;
        [SerializeField] private bool useCollision = true;
        [SerializeField] private bool bounceMode;
        [SerializeField, Range(0f, 2000f)] private float rate = 600f;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.C)) ToggleCollision();
            if (DemoInput.KeyDown(Key.B)) ToggleBounce();
            if (DemoInput.KeyDown(Key.UpArrow)) { rate = Mathf.Min(2000f, rate + 200f); Apply(); }
            if (DemoInput.KeyDown(Key.DownArrow)) { rate = Mathf.Max(0f, rate - 200f); Apply(); }

            GlossaryInfoPanel.Status = $"Collision {FxUtil.OnOff(useCollision)}   " +
                                       $"{(bounceMode ? "Bounce 0.4 / Lifetime Loss 0 (튕김)" : "Bounce 0 / Lifetime Loss 1 (닿으면 소멸 + 물튀김)")}\n" +
                                       $"비 {rate:0}개/초, 빗방울 {rain.particleCount}개";
        }

        public void ToggleCollision() { useCollision = !useCollision; Apply(); }
        public void ToggleBounce() { bounceMode = !bounceMode; Apply(); }

        private void Apply()
        {
            var col = rain.collision;
            col.enabled = useCollision;
            col.bounce = bounceMode ? 0.4f : 0f;
            col.lifetimeLoss = bounceMode ? 0f : 1f;
            var sub = rain.subEmitters;
            sub.enabled = useCollision && !bounceMode;
            var em = rain.emission;
            em.rateOverTime = rate;
        }
    }
}
