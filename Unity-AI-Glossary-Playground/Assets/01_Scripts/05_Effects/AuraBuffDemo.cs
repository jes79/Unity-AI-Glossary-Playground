using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // 오라 / 버프: 버프가 유지되는 동안 계속(루프) 재생되는 이펙트
    // 발밑 마법진(회전하는 Quad) + 위로 오르는 입자 + 은은한 빛. 캐릭터 자식이라 따라다님
    public class AuraBuffDemo : MonoBehaviour
    {
        [SerializeField] private Transform magicCircle;
        [SerializeField] private ParticleSystem risingParticles;
        [SerializeField] private Light auraLight;

        [Header("버프")]
        [SerializeField] private float buffDuration = 10f;
        [SerializeField] private float circleSpinSpeed = 45f; // 도/초
        [SerializeField] private float moveSpeed = 5f;

        private float buffTimer;
        private float appear; // 0~1 등장/퇴장 연출
        private Vector3 circleScale;

        private void Start()
        {
            circleScale = magicCircle.localScale;
            SetBuff(true);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.B) || DemoInput.KeyDown(Key.Space)) ToggleBuff();

            Vector2 mv = DemoInput.Move;
            transform.position += new Vector3(mv.x, 0f, mv.y) * moveSpeed * Time.deltaTime;

            bool active = buffTimer > 0f;
            if (active)
            {
                buffTimer -= Time.deltaTime;
                if (buffTimer <= 0f) SetBuff(false); // 지속 시간 끝 → 자동 해제
            }

            // 마법진: 켜질 때 OutBack 으로 튀어나오고, 꺼질 때 줄어듦. 항상 Y 축 회전
            appear = Mathf.MoveTowards(appear, active ? 1f : 0f, Time.deltaTime * 3f);
            float s = active ? Ease.OutBack(appear) : appear;
            magicCircle.localScale = circleScale * s;
            magicCircle.gameObject.SetActive(appear > 0.001f);
            magicCircle.Rotate(0f, circleSpinSpeed * Time.deltaTime, 0f, Space.World);
            auraLight.intensity = 2.5f * appear * (0.85f + 0.15f * Mathf.Sin(Time.time * 4f));

            GlossaryInfoPanel.Status = active
                ? $"버프 {FxUtil.OnOff(true)}  남은 시간 {buffTimer:0.0}초 / {buffDuration:0}초   (WASD 로 움직여도 따라옴)"
                : $"버프 {FxUtil.OnOff(false)}  — B 로 다시 걸기";
        }

        public void ToggleBuff() => SetBuff(buffTimer <= 0f);

        private void SetBuff(bool on)
        {
            buffTimer = on ? buffDuration : 0f;
            if (on) risingParticles.Play(true);
            else risingParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting); // 남은 입자는 자연스럽게 사라짐
        }
    }
}
