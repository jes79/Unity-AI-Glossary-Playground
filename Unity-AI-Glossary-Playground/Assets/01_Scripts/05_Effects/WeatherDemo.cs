using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // 날씨: 넓은 맵 전체에 비를 뿌리면 입자가 너무 많아지므로, 카메라 주변 상자 영역에서만 뿌리고 그 영역이 카메라를 따라다님
    // 핵심: 파티클은 Simulation Space = World → 따라다니는 건 "생성 위치"뿐, 이미 떨어지는 빗방울은 카메라와 같이 끌려가지 않음
    public class WeatherDemo : MonoBehaviour
    {
        [SerializeField] private Transform cameraRig;
        [SerializeField] private ParticleSystem rain;
        [SerializeField] private ParticleSystem snow;
        [SerializeField] private float height = 10f;   // 카메라 위 생성 높이
        [SerializeField] private float lookAhead = 6f; // 카메라 앞쪽으로 당겨 화면 안에 더 많이 내리게
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private bool follow = true;
        [SerializeField] private bool isSnow;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) ToggleWeather();
            if (DemoInput.KeyDown(Key.F)) ToggleFollow();

            Vector2 mv = DemoInput.Move;
            cameraRig.position += new Vector3(mv.x, 0f, mv.y) * moveSpeed * Time.deltaTime;

            GlossaryInfoPanel.Status = $"{(isSnow ? "눈" : "비")}   카메라 따라가기 {FxUtil.OnOff(follow)}   입자 {Active.particleCount}개\n" +
                                       $"카메라 위치 ({cameraRig.position.x:0}, {cameraRig.position.z:0}) — 따라가기 OFF 로 멀리 가면 비 영역을 벗어남";
        }

        private void LateUpdate()
        {
            if (!follow) return;
            // 회전은 무시하고 위치만: 카메라 바로 위 + 앞쪽
            Vector3 fwd = cameraRig.forward;
            fwd.y = 0f;
            Vector3 p = cameraRig.position + fwd.normalized * lookAhead;
            p.y = height;
            rain.transform.position = p;
            snow.transform.position = p;
        }

        private ParticleSystem Active => isSnow ? snow : rain;

        public void ToggleWeather() { isSnow = !isSnow; Apply(); }
        public void ToggleFollow() => follow = !follow;

        private void Apply()
        {
            if (isSnow) { rain.Stop(true, ParticleSystemStopBehavior.StopEmitting); snow.Play(true); }
            else { snow.Stop(true, ParticleSystemStopBehavior.StopEmitting); rain.Play(true); }
        }
    }
}
