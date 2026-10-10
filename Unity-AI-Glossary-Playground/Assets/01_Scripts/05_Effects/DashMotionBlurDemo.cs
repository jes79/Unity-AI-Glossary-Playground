using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Effects
{
    // Motion Blur: 카메라가 빠르게 움직일 때 생기는 잔상 (URP 는 Camera Only = 카메라 움직임 기준)
    // 대시하는 동안만 intensity 를 올리고, 평소에는 0 → 걷는 동안엔 화면이 선명함
    public class DashMotionBlurDemo : MonoBehaviour
    {
        public enum BlurMode { DashOnly, Always, Off }

        [SerializeField] private Volume volume;
        [SerializeField] private Transform player;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private BlurMode mode = BlurMode.DashOnly;

        [Header("이동 / 대시")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float dashSpeed = 32f;
        [SerializeField] private float dashTime = 0.22f;
        [SerializeField] private float dashCooldown = 0.4f;
        [SerializeField, Range(0f, 1f)] private float dashBlurIntensity = 1f;

        private MotionBlur blur;
        private Vector3 camOffset, lastDir = Vector3.forward;
        private float dashTimer, cooldown;

        private bool Dashing => dashTimer > 0f;

        private void Start()
        {
            volume.profile.TryGet(out blur);
            camOffset = cameraTransform.position - player.position;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.M)) CycleMode();
            if ((DemoInput.KeyDown(Key.LeftShift) || DemoInput.KeyDown(Key.Space)) && cooldown <= 0f) Dash();

            Vector2 mv = DemoInput.Move;
            Vector3 dir = new Vector3(mv.x, 0f, mv.y);
            if (dir.sqrMagnitude > 0.01f) lastDir = dir.normalized;

            dashTimer -= Time.deltaTime;
            cooldown -= Time.deltaTime;
            Vector3 velocity = Dashing ? lastDir * dashSpeed : dir * walkSpeed;
            player.position += velocity * Time.deltaTime;
            player.position = new Vector3(Mathf.Clamp(player.position.x, -45f, 45f), player.position.y, Mathf.Clamp(player.position.z, -45f, 45f));

            // 카메라는 고정 오프셋으로 딱 붙어 따라감 → 대시 때 카메라가 순간적으로 빨리 움직여 블러가 생김
            cameraTransform.position = player.position + camOffset;

            float target = mode switch
            {
                BlurMode.Always => dashBlurIntensity,
                BlurMode.Off => 0f,
                _ => Dashing ? dashBlurIntensity : 0f,
            };
            blur.intensity.value = Mathf.MoveTowards(blur.intensity.value, target, Time.deltaTime * 8f);

            GlossaryInfoPanel.Status = $"모드 {(mode == BlurMode.DashOnly ? "대시 때만" : mode == BlurMode.Always ? "항상" : "끔")}   " +
                                       $"{(Dashing ? "<color=#FFD24D>대시 중!</color>" : "걷는 중")}\nMotion Blur Intensity {blur.intensity.value:0.00}";
        }

        public void Dash()
        {
            dashTimer = dashTime;
            cooldown = dashTime + dashCooldown;
        }

        public void CycleMode() => mode = (BlurMode)(((int)mode + 1) % 3);
    }
}
