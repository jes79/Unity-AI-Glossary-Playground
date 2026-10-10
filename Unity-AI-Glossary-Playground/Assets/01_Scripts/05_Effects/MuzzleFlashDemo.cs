using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // 머즐 플래시: 총구에 0.05초만 보이는 섬광 입자 + 같은 시간 동안 켜지는 Point Light
    // 섬광만 있으면 "그림"이고, 빛이 주변 벽·바닥을 비춰야 "발사"처럼 느껴짐
    public class MuzzleFlashDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem flash;
        [SerializeField] private Light muzzleLight;
        [SerializeField] private Transform gun;
        [SerializeField] private AudioSource shotSound;

        [Header("머즐 플래시")]
        [SerializeField] private float flashDuration = 0.05f;
        [SerializeField] private float lightIntensity = 8f;
        [SerializeField] private float fireRate = 10f; // 누르고 있으면 초당 발사 수
        [SerializeField] private bool useFlash = true;
        [SerializeField] private bool useLight = true;

        private float lightTimer, cooldown;
        private Vector3 gunRestPos;
        private int shots;

        private void Start()
        {
            gunRestPos = gun.localPosition;
            muzzleLight.enabled = false;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.F)) ToggleFlash();
            if (DemoInput.KeyDown(Key.L)) ToggleLight();

            cooldown -= Time.deltaTime;
            bool trigger = DemoInput.KeyHeld(Key.Space) || (DemoInput.LeftHeld && !FxUtil.PointerOverUI);
            if (trigger && cooldown <= 0f) Fire();

            // 정확히 flashDuration 동안만 빛을 켬
            lightTimer -= Time.deltaTime;
            muzzleLight.enabled = useLight && lightTimer > 0f;

            // 반동: 뒤로 밀렸다가 복귀
            gun.localPosition = Vector3.Lerp(gun.localPosition, gunRestPos, 1f - Mathf.Exp(-20f * Time.deltaTime));

            GlossaryInfoPanel.Status = $"F 섬광 {FxUtil.OnOff(useFlash)}   L Point Light {FxUtil.OnOff(useLight)} (강도 {lightIntensity}, {flashDuration:0.00}초)\n" +
                                       $"발사 {shots}회 — 연사 {fireRate:0}발/초";
        }

        public void Fire()
        {
            cooldown = 1f / fireRate;
            shots++;
            if (useFlash) FxUtil.Replay(flash); // 랜덤 회전·크기라 매번 모양이 조금씩 다름
            lightTimer = flashDuration;
            muzzleLight.intensity = lightIntensity * Random.Range(0.8f, 1.2f);
            gun.localPosition = gunRestPos - gun.forward * 0.08f;
            if (shotSound != null) shotSound.PlayOneShot(shotSound.clip);
        }

        public void ToggleFlash() => useFlash = !useFlash;
        public void ToggleLight() => useLight = !useLight;
    }
}
