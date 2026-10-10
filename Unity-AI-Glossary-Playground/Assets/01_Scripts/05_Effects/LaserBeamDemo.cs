using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Line Renderer 레이저: Raycast 로 맞은 지점까지 선을 긋고, 그 지점에서 스파크 파티클을 계속 뿜음
    // R: 반사 모드 — 맞은 면에서 Vector3.Reflect 로 꺾어 최대 maxBounces 번 이어 그림 (positionCount 증가)
    public class LaserBeamDemo : MonoBehaviour
    {
        [SerializeField] private Transform turret;
        [SerializeField] private Transform muzzle;
        [SerializeField] private LineRenderer line;
        [SerializeField] private ParticleSystem sparks;
        [SerializeField] private Light hitLight;
        [SerializeField] private Transform[] movers;

        [Header("레이저")]
        [SerializeField] private float maxDistance = 40f;
        [SerializeField] private float width = 0.12f;
        [SerializeField] private bool reflect;
        [SerializeField, Range(1, 8)] private int maxBounces = 4;
        [SerializeField] private bool holdToFire = true;

        private Camera cam;
        private Vector3[] moverBase;
        private bool firing, toggled;
        private Vector3 aimPoint;

        private void Start()
        {
            cam = Camera.main;
            moverBase = new Vector3[movers.Length];
            for (int i = 0; i < movers.Length; i++) moverBase[i] = movers[i].position;
            aimPoint = turret.position + turret.forward * 10f;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) ToggleReflect();
            if (DemoInput.KeyDown(Key.Space)) ToggleFire();

            // 움직이는 장애물
            for (int i = 0; i < movers.Length; i++)
                movers[i].position = moverBase[i] + Vector3.forward * Mathf.Sin(Time.time * (0.8f + i * 0.3f)) * 2.5f;

            // 포탑이 마우스 지점을 바라봄 (수평으로만)
            if (FxUtil.MouseOnPlane(cam, muzzle.position.y, out var p)) aimPoint = p;
            Vector3 flat = aimPoint - turret.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f) turret.rotation = Quaternion.LookRotation(flat);

            firing = toggled || (holdToFire && DemoInput.LeftHeld && !FxUtil.PointerOverUI);
            line.enabled = firing;
            hitLight.enabled = false;
            var em = sparks.emission;
            em.enabled = false;
            if (firing) DrawBeam();

            GlossaryInfoPanel.Status = $"발사 {FxUtil.OnOff(firing)}   반사 {FxUtil.OnOff(reflect)} (최대 {maxBounces}회)   Line Renderer 점 {line.positionCount}개\n" +
                                       "폭이 미세하게 떨려서 에너지가 흐르는 느낌을 줌";
        }

        public void ToggleReflect() => reflect = !reflect;
        public void ToggleFire() => toggled = !toggled;

        private void DrawBeam()
        {
            Vector3 origin = muzzle.position;
            Vector3 dir = muzzle.forward;
            int count = 1;
            line.positionCount = maxBounces + 2;
            line.SetPosition(0, origin);

            for (int bounce = 0; bounce <= maxBounces; bounce++)
            {
                if (Physics.Raycast(origin, dir, out var hit, maxDistance))
                {
                    line.SetPosition(count++, hit.point);
                    // 마지막으로 맞은 지점에 스파크 + 빛
                    sparks.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(Vector3.Reflect(dir, hit.normal)));
                    hitLight.transform.position = hit.point + hit.normal * 0.3f;
                    if (!reflect || bounce == maxBounces) { SetHitFx(true); break; }
                    origin = hit.point + hit.normal * 0.01f;
                    dir = Vector3.Reflect(dir, hit.normal);
                }
                else
                {
                    line.SetPosition(count++, origin + dir * maxDistance);
                    SetHitFx(false);
                    break;
                }
            }
            line.positionCount = count;
            line.widthMultiplier = width * (0.85f + 0.3f * Mathf.PerlinNoise(Time.time * 30f, 0f));
        }

        private void SetHitFx(bool on)
        {
            var em = sparks.emission;
            em.enabled = on;
            hitLight.enabled = on;
        }
    }
}
