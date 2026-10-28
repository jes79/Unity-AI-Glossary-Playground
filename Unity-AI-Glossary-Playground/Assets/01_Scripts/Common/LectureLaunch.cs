using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlossaryDemo
{
    // 강의 플레이어(Builds/LecturePlayer/Glossary.exe) 시작 처리 — 쉐이더 랩의 LectureLaunch 와 같은 방식
    // - 명령줄 인자 (OMP Studio 앱의 ▶ 예제 실행이 이렇게 실행한다)
    //     -glossaryStart 0105 -glossaryLecture : 그 용어 씬부터 강의 모드 (분야 첫 용어면 분야 소개부터, 아니면 ① 소개부터)
    //     -glossaryStart 0105                  : 강의 없이 그 용어 씬만 연다
    //     인자 없음                            : 지금처럼 허브부터
    //   없는 번호를 주면 허브를 그대로 둔다 (Player.log 에 경고)
    // - 강의 플레이어 빌드(GLOSSARY_LECTURE_PLAYER 정의)에서만
    //   · 테두리 없는 창 전체화면(현재 모니터 해상도)으로 시작, 앱이 포커스를 가져가도 멈추지 않음
    //   · F10 = 강의 플레이어 끝내기 (Esc 는 일시정지·팝업 데모가 쓰므로 비워 둔다)
    // - 스크린샷 인자(-glossaryShots)와 함께 쓰지 않는다: 그 실행은 ScreenshotRunner 가 씬을 직접 고른다
    public static class LectureLaunch
    {
        public const string StartArg = "-glossaryStart";
        public const string LectureArg = "-glossaryLecture";

        // 상수(const) 대신 속성으로 둬서 '실행되지 않는 코드' 경고가 나지 않게 함
#if GLOSSARY_LECTURE_PLAYER
        public static bool IsLecturePlayer => true;
#else
        public static bool IsLecturePlayer => false;
#endif

        // 첫 씬이 뜨기 전에 창 모양부터 정함
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetupWindow()
        {
            if (!IsLecturePlayer || Application.isEditor) return;
            Application.runInBackground = true;

            // -screen-width / -screen-fullscreen 처럼 창 크기를 직접 준 실행은 그대로 둔다
            foreach (string arg in System.Environment.GetCommandLineArgs())
                if (arg.StartsWith("-screen-") || arg == "-popupwindow" || arg == "-window-mode") return;

            // 해상도·창 모드는 레지스트리에 남아 다음 실행에 다시 쓰이므로 매번 맞춰 둔다
            var display = Screen.mainWindowDisplayInfo;
            int width = display.width > 0 ? display.width : Screen.currentResolution.width;
            int height = display.height > 0 ? display.height : Screen.currentResolution.height;
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        }

        // 첫 씬(허브)이 열린 뒤 인자를 보고 그 용어 씬으로 옮긴다. 앱이 켜질 때 한 번만 불린다
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HandleArgs()
        {
            if (IsLecturePlayer && !Application.isEditor)
            {
                var keys = new GameObject("LectureLaunchKeys");
                Object.DontDestroyOnLoad(keys);
                keys.AddComponent<QuitKeyWatcher>();
            }

            string[] args = System.Environment.GetCommandLineArgs();
            string id = null;
            bool lecture = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-glossaryShots") return;
                if (args[i] == LectureArg) lecture = true;
                if (args[i] == StartArg && i + 1 < args.Length) id = args[i + 1].Trim();
            }
            if (id == null) return;

            if (!TryFind(id, out var entry))
            {
                Debug.LogWarning($"[LectureLaunch] {StartArg} {id}: 없는 용어 → 허브");
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(entry.Scene))
            {
                Debug.LogWarning($"[LectureLaunch] 씬 '{entry.Scene}' 이 빌드에 없음 → 허브 (Tools > Glossary > Build Lecture Player 로 다시 빌드)");
                return;
            }
            Debug.Log($"[LectureLaunch] {entry.Id} 열기 (강의 모드 {(lecture ? "켬" : "끔")})");
            if (lecture) LectureMode.StartFromTerm(entry);
            else SceneManager.LoadScene(entry.Scene);
        }

        private static bool TryFind(string id, out GlossaryEntry entry)
        {
            foreach (var e in GlossaryCatalog.Entries)
            {
                if (e.Id != id) continue;
                entry = e;
                return true;
            }
            entry = default;
            return false;
        }

        // 강의 플레이어 끝내기 키 (씬이 바뀌어도 남는다)
        private sealed class QuitKeyWatcher : MonoBehaviour
        {
            private void Update()
            {
                if (DemoInput.KeyDown(LectureMode.QuitKey)) Application.Quit();
            }
        }
    }
}
