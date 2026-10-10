using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo.Effects
{
    // 씬 전환 연출: 덮기(out) → 씬 로드 → 걷어내기(in)
    // 씬이 바뀌어도 살아 있어야 하므로 DontDestroyOnLoad 오브젝트로 만들고, 끝나면 스스로 사라짐
    // 시간은 unscaled 로 재서 일시정지(timeScale 0) 중에도 동작
    public class ScreenFader : MonoBehaviour
    {
        public enum Style { Fade, Wipe }

        public static bool Busy { get; private set; }

        private Image cover;

        public static void LoadScene(string sceneName, Style style, float halfDuration, Color color)
        {
            if (Busy) return;
            var go = new GameObject("ScreenFader");
            DontDestroyOnLoad(go);
            var fader = go.AddComponent<ScreenFader>();
            fader.Build(color);
            fader.StartCoroutine(fader.Run(sceneName, style, halfDuration));
        }

        private void Build(Color color)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000; // 정보 패널(1000)보다 위
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            var rt = new GameObject("Cover", typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            cover = rt.gameObject.AddComponent<Image>();
            // Filled 타입(와이프)은 스프라이트가 필요 → 흰 텍스처로 즉석 생성
            var tex = Texture2D.whiteTexture;
            cover.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            cover.color = color;
            cover.raycastTarget = true; // 전환 중 클릭 막기
        }

        private IEnumerator Run(string sceneName, Style style, float half)
        {
            Busy = true;

            // 1) 덮기: 페이드는 알파 0→1, 와이프는 왼쪽에서 오른쪽으로 채움
            SetCover(style, 0f, true);
            yield return Ease.Run(half, Ease.InOutQuad, k => SetCover(style, k, true), unscaled: true);

            // 2) 화면이 완전히 가려진 상태에서 씬 로드
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;
            yield return null; // 새 씬 첫 프레임이 그려질 때까지 한 프레임 대기

            // 3) 걷어내기: 페이드는 알파 1→0, 와이프는 왼쪽부터 비워져 오른쪽으로 빠져나감
            yield return Ease.Run(half, Ease.InOutQuad, k => SetCover(style, 1f - k, false), unscaled: true);

            Busy = false;
            Destroy(gameObject);
        }

        private void SetCover(Style style, float amount, bool entering)
        {
            if (style == Style.Fade)
            {
                cover.type = Image.Type.Simple;
                var c = cover.color;
                c.a = amount;
                cover.color = c;
            }
            else
            {
                var c = cover.color;
                c.a = 1f;
                cover.color = c;
                cover.type = Image.Type.Filled;
                cover.fillMethod = Image.FillMethod.Horizontal;
                cover.fillOrigin = entering ? (int)Image.OriginHorizontal.Left : (int)Image.OriginHorizontal.Right;
                cover.fillAmount = amount;
            }
        }

        private void OnDestroy()
        {
            if (Busy) Busy = false;
        }
    }
}
