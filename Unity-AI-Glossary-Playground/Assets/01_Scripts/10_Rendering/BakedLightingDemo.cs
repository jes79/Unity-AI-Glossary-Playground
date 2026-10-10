using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // 실시간(Realtime) 조명 vs 베이크(Baked) 조명 비교
    // - 건물(움직이지 않는 Static 오브젝트)을 비추는 "가로등" 라이트들은 Mode = Baked
    //   → 굽고 나면 빛이 라이트맵 텍스처에 저장되어 런타임 계산 비용이 0 에 가까움
    // - 공(움직이는 오브젝트)은 라이트맵을 못 받으므로 Light Probe 로 구운 빛을 받음
    // - 비교용 Realtime 라이트 묶음을 켜고 끄며 프레임 시간을 비교
    // 라이트맵 굽기는 에디터 작업이라 메뉴 Tools/Glossary/Rendering/Bake Lighting for 1003 으로 실행
    public class BakedLightingDemo : MonoBehaviour
    {
        [SerializeField] private Light[] bakedLights;     // Mode = Baked (굽기 대상)
        [SerializeField] private Light[] realtimeLights;  // Mode = Realtime (비교용)
        [SerializeField] private Transform movingBall;    // 동적 오브젝트 (Light Probe 로 빛을 받음)

        private float smoothMs;
        private readonly StringBuilder sb = new StringBuilder(512);

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) ToggleRealtime();

            if (movingBall != null)
                movingBall.position = new Vector3(Mathf.Sin(Time.time * 0.6f) * 7f, 0.6f, -1.5f);

            smoothMs = Mathf.Lerp(smoothMs, Time.unscaledDeltaTime * 1000f, 0.05f);

            bool baked = bakedLights.Length > 0 && bakedLights[0].bakingOutput.isBaked;
            int lightmaps = LightmapSettings.lightmaps != null ? LightmapSettings.lightmaps.Length : 0;
            sb.Clear();
            sb.Append("라이트맵 텍스처: ").Append(lightmaps).Append("장   ");
            sb.Append(baked ? "<color=#8F8>굽기 완료</color>" : "<color=#FC6>아직 안 구움 → Baked 라이트도 실시간처럼 계산 중</color>").Append('\n');
            sb.Append("Baked 라이트 ").Append(bakedLights.Length).Append("개: ");
            if (bakedLights.Length > 0) sb.Append(bakedLights[0].bakingOutput.lightmapBakeType);
            sb.Append("\nRealtime 라이트 ").Append(realtimeLights.Length).Append("개: ")
              .Append(realtimeLights.Length > 0 && realtimeLights[0].enabled ? "ON" : "OFF");
            sb.Append("\n프레임: ").Append(smoothMs.ToString("0.00")).Append(" ms");
            GlossaryInfoPanel.Status = sb.ToString();
        }

        public void ToggleRealtime()
        {
            foreach (var l in realtimeLights) l.enabled = !l.enabled;
        }
    }
}
