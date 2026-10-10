using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GlossaryDemo.Effects
{
    // 화면 전환 데모: 같은 씬을 다시 로드하되, 로드할 때마다 "구역 A(숲)" ↔ "구역 B(동굴)" 을 번갈아 보여 줌
    // (실제 게임에선 LoadScene("Dungeon") 처럼 다른 씬 이름을 넘기면 됨)
    public class ScreenTransitionDemo : MonoBehaviour
    {
        [SerializeField] private float halfDuration = 0.5f; // 아웃 0.5초 + 인 0.5초
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private GameObject areaA;
        [SerializeField] private GameObject areaB;
        [SerializeField] private Camera cam;
        [SerializeField] private Color areaABackground = new Color(0.55f, 0.75f, 0.95f);
        [SerializeField] private Color areaBBackground = new Color(0.08f, 0.06f, 0.12f);

        // static: 씬을 다시 로드해도 값이 유지됨 → 다음 구역을 기억
        private static int area;
        private static int loads;

        private void Start()
        {
            areaA.SetActive(area == 0);
            areaB.SetActive(area == 1);
            cam.backgroundColor = area == 0 ? areaABackground : areaBBackground;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) GoFade();
            if (DemoInput.KeyDown(Key.Digit2)) GoWipe();

            GlossaryInfoPanel.Status = $"현재 구역: {(areaA.activeSelf ? "A — 숲" : "B — 동굴")}   씬 로드 {loads}회\n" +
                                       $"전환: 아웃 {halfDuration:0.0}초 → LoadSceneAsync → 인 {halfDuration:0.0}초 {(ScreenFader.Busy ? "<color=#FFD24D>(전환 중)</color>" : "")}";
        }

        public void GoFade() => Go(ScreenFader.Style.Fade);
        public void GoWipe() => Go(ScreenFader.Style.Wipe);

        private void Go(ScreenFader.Style style)
        {
            if (ScreenFader.Busy) return;
            area = 1 - area;
            loads++;
            ScreenFader.LoadScene(SceneManager.GetActiveScene().name, style, halfDuration, fadeColor);
        }
    }
}
