using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo
{
    // README 용 스크린샷 자동 촬영 + 정보 패널 가림 점검
    // 플레이어를 "-glossaryShots <폴더> [-glossaryShotList 0000_Hub,0105_CoyoteTime,...]" 인자로 실행하면
    // 씬을 차례로 열어 PNG 로 저장하고 종료함 (목록이 없으면 Build Settings 의 모든 씬)
    // "-glossaryOverlapCheck" 를 함께 주면 정보 패널이 데모 UI 를 가리는 씬을 Player.log 에 적음
    // 강의 모드 화면: 목록 항목을 "씬@단계" 로 쓰면(예: 0105_CoyoteTime@Demo) 그 단계를 띄워 "씬_단계.png" 로 저장
    //   단계 이름 = LectureStep (Opening, SelfIntro, Intro, Concept, Demo, Prompt). "-glossaryLecture" 를 주면 허브 등 모든 항목을 강의 모드로 촬영
    // 인자가 없으면 아무 일도 하지 않음 (일반 실행에 영향 없음)
    public class ScreenshotRunner : MonoBehaviour
    {
        private float waitSeconds = 3f; // 씬 로드 후 연출이 자리 잡을 때까지 대기 (-glossaryShotWait 로 변경)
        private bool checkOverlap;
        private bool capture = true;
        private bool lecture;

        private string outputDir;
        private readonly List<string> scenes = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string dir = null, list = null, wait = null;
            bool overlap = false, noShots = false, lecture = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-glossaryOverlapCheck") overlap = true;
                if (args[i] == "-glossaryNoCapture") noShots = true;
                if (args[i] == "-glossaryLecture") lecture = true;
                if (i >= args.Length - 1) continue;
                if (args[i] == "-glossaryShots") dir = args[i + 1];
                if (args[i] == "-glossaryShotList") list = args[i + 1];
                if (args[i] == "-glossaryShotWait") wait = args[i + 1];
            }
            if (dir == null) return;

            // 창이 포커스를 잃으면 플레이어가 멈춰서 캡처가 중간에 서 버린다.
            // 자동 촬영일 때만 백그라운드 실행을 켠다 (일반 빌드 설정은 그대로)
            Application.runInBackground = true;

            var go = new GameObject("ScreenshotRunner");
            DontDestroyOnLoad(go);
            var runner = go.AddComponent<ScreenshotRunner>();
            runner.outputDir = dir;
            runner.checkOverlap = overlap;
            runner.capture = !noShots;
            runner.lecture = lecture;
            if (wait != null && float.TryParse(wait, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float w)) runner.waitSeconds = w;
            if (list != null)
            {
                runner.scenes.AddRange(list.Split(','));
            }
            else
            {
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                    runner.scenes.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
            }
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(outputDir);
            foreach (string item in scenes)
            {
                // "씬@단계" → 강의 모드로 그 단계를 띄움
                string[] parts = item.Split('@');
                string sceneName = parts[0];
                LectureStep? step = null;
                if (parts.Length > 1 && System.Enum.TryParse(parts[1], true, out LectureStep parsed)) step = parsed;
                bool lectureShot = lecture || parts.Length > 1;
                string shotName = parts.Length > 1 ? $"{sceneName}_{parts[1]}" : sceneName;

                if (!Application.CanStreamedLevelBeLoaded(sceneName)) continue;

                Debug.Log($"[Shot] {item}"); // Player.log 에서 씬별 오류를 구분하기 위한 표시
                if (lectureShot) LectureMode.Prepare(step);
                else LectureMode.SetEnabled(false);
                Time.timeScale = 1f;
                SceneManager.LoadScene(sceneName);
                yield return new WaitForSecondsRealtime(waitSeconds);

                // 말풍선 글자가 다 나온 상태로 찍기 (레이아웃이 다시 잡히도록 한 프레임 더 기다림)
                if (LectureOverlay.Current != null)
                {
                    LectureOverlay.Current.SkipTyping();
                    yield return null;
                }
                yield return new WaitForEndOfFrame();

                if (checkOverlap) ReportOverlap(shotName);
                if (capture) ScreenCapture.CaptureScreenshot(Path.Combine(outputDir, shotName + ".png"));
                yield return null;
                yield return null;
            }
            Application.Quit();
        }

        // 정보 패널(강의 모드면 강의 UI)이 데모 UI 를 얼마나 가리는지 화면 좌표로 재서 Player.log 에 적는다.
        // 패널 높이는 글자 길이에 따라 달라지므로 실행 중에만 알 수 있다.
        private static void ReportOverlap(string sceneName)
        {
            var info = FindAnyObjectByType<GlossaryInfoPanel>();
            if (info == null) return;
            var panelRt = info.transform.Find("GlossaryInfoCanvas/Panel") as RectTransform;
            if (panelRt != null && panelRt.gameObject.activeInHierarchy)
            {
                Rect panel = ScreenRect(panelRt, null);
                Debug.Log($"[PanelSize] {sceneName} h={panel.height:F0} (화면 {Screen.height})");
                ReportCover("PanelOverlap", sceneName, panel, info.transform);
            }

            // 강의 UI (진행 막대·초상화·말풍선·키 안내)는 정보 패널 오브젝트 아래에 있으므로 같은 기준으로 잰다
            var lecture = LectureOverlay.Current;
            if (lecture == null) return;
            foreach (var rt in lecture.CoverRects)
                ReportCover("LectureOverlap", $"{sceneName} [{rt.name}]", ScreenRect(rt, null), info.transform);
        }

        private static void ReportCover(string tag, string sceneName, Rect panel, Transform self)
        {
            foreach (var graphic in FindObjectsByType<Graphic>(FindObjectsSortMode.None))
            {
                if (!graphic.isActiveAndEnabled) continue;
                if (graphic.transform.IsChildOf(self)) continue; // 패널·강의 UI 자신
                var canvas = graphic.canvas;
                if (canvas == null || canvas.renderMode == RenderMode.WorldSpace) continue;

                Rect r = ScreenRect((RectTransform)graphic.transform, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera);
                if (r.width < 4f || r.height < 4f) continue;

                float ox = Mathf.Min(panel.xMax, r.xMax) - Mathf.Max(panel.xMin, r.xMin);
                float oy = Mathf.Min(panel.yMax, r.yMax) - Mathf.Max(panel.yMin, r.yMin);
                if (ox <= 0f || oy <= 0f) continue;

                float ratio = (ox * oy) / (r.width * r.height);
                if (ratio < 0.15f) continue; // 모서리만 살짝 스치는 것은 넘어감
                Debug.LogWarning($"[{tag}] {sceneName} {HierarchyPath(graphic.transform)} {ratio * 100f:F0}% 가림");
            }
        }

        private static Rect ScreenRect(RectTransform rt, Camera cam)
        {
            var c = new Vector3[4];
            rt.GetWorldCorners(c);
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector2 p = cam == null ? (Vector2)c[i] : RectTransformUtility.WorldToScreenPoint(cam, c[i]);
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
            }
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static string HierarchyPath(Transform t)
        {
            string s = t.name;
            for (var p = t.parent; p != null; p = p.parent) s = p.name + "/" + s;
            return s;
        }
    }
}
