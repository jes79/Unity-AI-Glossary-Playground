using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // 세 가지 라이트 종류를 하나씩 켜고 꺼서 비교
    // Directional = 태양(위치 무관, 방향만), Point = 전구(한 점에서 사방), Spot = 손전등/무대 조명(원뿔)
    public class LightTypesDemo : MonoBehaviour
    {
        [SerializeField] private Light directional;
        [SerializeField] private Light[] points;
        [SerializeField] private Light[] spots;
        [SerializeField] private FlashlightPlayer player;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) ToggleDirectional();
            if (DemoInput.KeyDown(Key.Digit2)) TogglePoints();
            if (DemoInput.KeyDown(Key.Digit3)) ToggleSpots();

            GlossaryInfoPanel.Status =
                $"1 Directional(태양): {OnOff(directional.enabled)}  강도 {directional.intensity:0.0}\n" +
                $"2 Point(전구) x{points.Length}: {OnOff(points.Length > 0 && points[0].enabled)}  범위 {(points.Length > 0 ? points[0].range : 0):0}m\n" +
                $"3 Spot(무대 조명) x{spots.Length}: {OnOff(spots.Length > 0 && spots[0].enabled)}  각도 {(spots.Length > 0 ? spots[0].spotAngle : 0):0}°\n" +
                $"F 손전등(Spot): {OnOff(player != null && player.IsOn)}";
        }

        private static string OnOff(bool on) => on ? "<color=#8F8>ON</color>" : "<color=#F88>OFF</color>";

        public void ToggleDirectional() => directional.enabled = !directional.enabled;
        public void TogglePoints() { foreach (var l in points) l.enabled = !l.enabled; }
        public void ToggleSpots() { foreach (var l in spots) l.enabled = !l.enabled; }
    }
}
