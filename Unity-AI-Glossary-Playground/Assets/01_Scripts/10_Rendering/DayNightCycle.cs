using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GlossaryDemo.Rendering
{
    // 낮 → 밤 사이클: 태양 회전 + Procedural Skybox 값 + 환경광(Ambient) + 안개 색을 시간에 따라 바꿈
    // Skybox = 하늘 배경, Ambient(환경광) = 그림자 속까지 채우는 전체 밝기
    public class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private Material skyboxTemplate;            // Skybox/Procedural 머티리얼 에셋
        [SerializeField, Range(0f, 24f)] private float timeOfDay = 9f;
        [SerializeField] private float dayLengthSeconds = 40f;        // 24시간이 몇 초에 지나가는지
        [SerializeField] private bool paused;

        [Header("색 (시간에 따라 보간)")]
        [SerializeField] private Gradient sunColor;
        [SerializeField] private Gradient ambientSky;
        [SerializeField] private Gradient ambientGround;
        [SerializeField] private Gradient skyTint;

        private Material sky;            // 에셋을 직접 바꾸지 않도록 복제본 사용
        private float probeTimer;

        private void Start()
        {
            sky = new Material(skyboxTemplate);
            RenderSettings.skybox = sky;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight; // 하늘/수평선/땅 3색 환경광
        }

        private void OnDestroy()
        {
            if (sky != null) Destroy(sky);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) paused = !paused;
            if (DemoInput.KeyHeld(Key.RightArrow)) timeOfDay += Time.deltaTime * 4f;
            if (DemoInput.KeyHeld(Key.LeftArrow)) timeOfDay -= Time.deltaTime * 4f;
            if (!paused) timeOfDay += 24f / dayLengthSeconds * Time.deltaTime;
            timeOfDay = Mathf.Repeat(timeOfDay, 24f);

            float t = timeOfDay / 24f;
            // 6시 = 해 뜸(0°), 12시 = 머리 위(90°), 18시 = 해 짐(180°)
            float sunAngle = (timeOfDay - 6f) / 24f * 360f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

            float daylight = Mathf.Clamp01(Mathf.Sin((timeOfDay - 6f) / 12f * Mathf.PI) * 1.5f + 0.1f);
            sun.color = sunColor.Evaluate(t);
            sun.intensity = Mathf.Lerp(0f, 1.3f, daylight);
            sun.shadowStrength = daylight;

            // 환경광 (그늘진 곳의 밝기)
            RenderSettings.ambientSkyColor = ambientSky.Evaluate(t);
            RenderSettings.ambientEquatorColor = Color.Lerp(ambientGround.Evaluate(t), ambientSky.Evaluate(t), 0.5f);
            RenderSettings.ambientGroundColor = ambientGround.Evaluate(t);
            RenderSettings.fogColor = ambientSky.Evaluate(t);

            // Procedural Skybox 파라미터
            sky.SetColor("_SkyTint", skyTint.Evaluate(t));
            sky.SetFloat("_Exposure", Mathf.Lerp(0.15f, 1.3f, daylight));
            sky.SetFloat("_AtmosphereThickness", Mathf.Lerp(0.6f, 1.2f, 1f - Mathf.Abs(daylight - 0.5f) * 2f) + 0.2f);

            // 반사(Reflection Probe/스카이 반사)도 가끔 갱신
            probeTimer -= Time.deltaTime;
            if (probeTimer <= 0f)
            {
                probeTimer = 0.5f;
                DynamicGI.UpdateEnvironment();
            }

            int h = (int)timeOfDay, m = (int)((timeOfDay - h) * 60f);
            string phase = timeOfDay < 5f || timeOfDay >= 20f ? "밤" : timeOfDay < 8f ? "새벽" : timeOfDay < 17f ? "낮" : "저녁";
            GlossaryInfoPanel.Status = $"시각 {h:00}:{m:00} ({phase}){(paused ? "  (일시정지)" : "")}\n" +
                                       $"태양 각도 {sunAngle:0}°  강도 {sun.intensity:0.00}\n" +
                                       $"환경광(하늘) #{ColorUtility.ToHtmlStringRGB(RenderSettings.ambientSkyColor)}  Exposure {sky.GetFloat("_Exposure"):0.00}";
        }

        public void SetTime(float hour) => timeOfDay = hour;
        public void TogglePause() => paused = !paused;
    }
}
