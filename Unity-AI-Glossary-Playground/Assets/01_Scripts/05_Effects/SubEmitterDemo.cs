using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Sub Emitter: 부모 입자에 사건(Birth / Collision / Death …)이 생기면 그 위치에서 자식 파티클 시스템을 터뜨림
    // 불꽃놀이: 로켓 입자가 수명을 다해 죽는 순간(Death) 자식 Burst 가 터짐
    public class SubEmitterDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem rockets;
        [SerializeField] private bool useSubEmitter = true;
        [SerializeField] private bool autoLaunch = true;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) Launch();
            if (DemoInput.KeyDown(Key.S)) ToggleSubEmitter();
            if (DemoInput.KeyDown(Key.A)) ToggleAuto();
            GlossaryInfoPanel.Status = $"Sub Emitter (Death → 폭죽 Burst) {FxUtil.OnOff(useSubEmitter)}   자동 발사 {FxUtil.OnOff(autoLaunch)}\n" +
                                       $"로켓 {rockets.particleCount}개 비행 중 — OFF 면 로켓이 그냥 사라짐";
        }

        public void Launch() => rockets.Emit(1); // 로켓 하나 추가 (Shape/Main 설정 그대로 사용)
        public void ToggleSubEmitter() { useSubEmitter = !useSubEmitter; Apply(); }
        public void ToggleAuto() { autoLaunch = !autoLaunch; Apply(); }

        private void Apply()
        {
            var sub = rockets.subEmitters;
            sub.enabled = useSubEmitter;
            var em = rockets.emission;
            em.rateOverTime = autoLaunch ? 1.2f : 0f;
        }
    }
}
