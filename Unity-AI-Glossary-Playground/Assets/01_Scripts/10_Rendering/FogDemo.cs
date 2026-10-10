using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // 던전 Fog: 시야를 viewDistance(20m) 로 제한
    // Linear  : fogStart ~ fogEnd 사이에서 선형으로 짙어짐 → End = 20m 이면 20m 밖은 완전히 안개색
    // Exp/Exp2: 밀도(density)로 지정. 20m 에서 95% 가려지도록 밀도를 계산
    // 카메라 배경색도 안개색으로 맞춰야 먼 곳이 "끝없이 어둡게" 보임
    public class FogDemo : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float viewDistance = 20f;
        [SerializeField] private Color fogColor = new Color(0.06f, 0.06f, 0.08f);
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 90f;

        private void Start()
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.F)) ToggleFog();
            if (DemoInput.KeyDown(Key.M)) NextMode();
            if (DemoInput.KeyDown(Key.Equals) || DemoInput.KeyDown(Key.NumpadPlus)) ChangeDistance(5f);
            if (DemoInput.KeyDown(Key.Minus) || DemoInput.KeyDown(Key.NumpadMinus)) ChangeDistance(-5f);

            // W/S 전진·후진, A/D 회전 (던전 걷기)
            Vector2 m = DemoInput.Move;
            var t = cam.transform;
            t.Rotate(0f, m.x * turnSpeed * Time.deltaTime, 0f, Space.World);
            Vector3 fwd = t.forward; fwd.y = 0f;
            t.position += fwd.normalized * (m.y * moveSpeed * Time.deltaTime);

            GlossaryInfoPanel.Status =
                $"Fog: {(RenderSettings.fog ? "<color=#8F8>ON</color>" : "<color=#F88>OFF</color>")}   모드: <b>{RenderSettings.fogMode}</b>\n" +
                (RenderSettings.fogMode == FogMode.Linear
                    ? $"Start {RenderSettings.fogStartDistance:0}m → End {RenderSettings.fogEndDistance:0}m"
                    : $"Density {RenderSettings.fogDensity:0.000}  (≈{viewDistance:0}m 에서 95% 가려짐)") +
                $"\n시야 거리 설정: {viewDistance:0}m   (기둥 간격 5m)";
        }

        public void ToggleFog()
        {
            RenderSettings.fog = !RenderSettings.fog;
            cam.backgroundColor = RenderSettings.fog ? fogColor : new Color(0.3f, 0.35f, 0.45f);
        }

        public void NextMode()
        {
            RenderSettings.fogMode = RenderSettings.fogMode switch
            {
                FogMode.Linear => FogMode.Exponential,
                FogMode.Exponential => FogMode.ExponentialSquared,
                _ => FogMode.Linear,
            };
            Apply();
        }

        public void ChangeDistance(float delta)
        {
            viewDistance = Mathf.Clamp(viewDistance + delta, 5f, 80f);
            Apply();
        }

        private void Apply()
        {
            RenderSettings.fogColor = fogColor;
            if (RenderSettings.fog) cam.backgroundColor = fogColor;
            // Linear: 가까운 곳(20%)부터 서서히, viewDistance 에서 완전히 가려짐
            RenderSettings.fogStartDistance = viewDistance * 0.2f;
            RenderSettings.fogEndDistance = viewDistance;
            // Exp : e^(-d·x) = 0.05  →  d = 3 / x
            // Exp2: e^(-(d·x)²) = 0.05 → d = √3 / x
            RenderSettings.fogDensity = RenderSettings.fogMode == FogMode.Exponential ? 3f / viewDistance : 1.732f / viewDistance;
        }
    }
}
