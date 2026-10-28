using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlossaryDemo
{
    // 강의 플레이어(Builds/LecturePlayer/Glossary.exe) 시작 처리
    // - 명령줄 인자
    //     -glossaryStart 0105 -glossaryLecture : 허브를 거치지 않고 그 용어 씬을 강의 모드로 열기 (① 소개부터, 분야 첫 용어면 분야 소개부터)
    //     -glossaryStart 0105                  : 강의 모드 없이 그 용어 씬만 열기
    //     인자 없음                            : 지금처럼 허브
    //   없는 id 를 주면 허브를 열고 허브 아래쪽에 알림을 한 줄 띄움
    //   -glossaryShots(자동 촬영)와 함께 주면 이 인자는 무시함 (촬영은 ScreenshotRunner 가 씬을 직접 고름)
    // - 강의 플레이어 빌드에서만: 테두리 없는 창 전체화면(현재 모니터 해상도)으로 시작
    //   (Tools > Glossary > Build Lecture Player 가 GLOSSARY_LECTURE_PLAYER 정의를 붙여 빌드함)
    public static class LectureLaunch
    {
        public const string StartArg = "-glossaryStart";
        public const string LectureArg = "-glossaryLecture";

#if GLOSSARY_LECTURE_PLAYER
        public const bool IsLecturePlayer = true;
#else
        public const bool IsLecturePlayer = false;
#endif

        // 첫 씬이 뜨기 전에 창 모양부터 정함
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetupWindow()
        {
            if (!IsLecturePlayer || Application.isEditor) return;

            // -screen-width / -screen-fullscreen 처럼 창 크기를 직접 준 실행은 그대로 둔다
            foreach (string arg in System.Environment.GetCommandLineArgs())
                if (arg.StartsWith("-screen-") || arg == "-popupwindow" || arg == "-window-mode") return;

            // 해상도·창 모드는 레지스트리에 남아 다음 실행에 다시 쓰이므로(스크린샷 플레이어와 같은 키) 매번 맞춰 둔다
            var display = Screen.mainWindowDisplayInfo;
            int width = display.width > 0 ? display.width : Screen.currentResolution.width;
            int height = display.height > 0 ? display.height : Screen.currentResolution.height;
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        }

        // 첫 씬(허브)이 열린 뒤 인자를 보고 용어 씬으로 넘어감
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HandleArgs()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string id = null;
            bool lecture = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-glossaryShots") return; // 자동 촬영 실행
                if (args[i] == LectureArg) lecture = true;
                if (args[i] == StartArg && i + 1 < args.Length) id = args[i + 1].Trim();
            }
            if (id == null) return;

            if (!GlossaryCatalog.TryFind(id, out var entry) || !Application.CanStreamedLevelBeLoaded(entry.Scene))
            {
                Debug.LogWarning($"[LectureLaunch] {StartArg} {id}: 없는 용어 → 허브");
                HubMenu.Notice = $"'{id}' 번 용어 씬이 없어 허브를 열었습니다. 용어 번호는 4자리입니다 (예: 0105).";
                if (SceneManager.GetActiveScene().name != GlossaryCatalog.HubScene) LectureMode.GoHub();
                return;
            }

            Debug.Log($"[LectureLaunch] {entry.Id} {entry.Term} 열기 (강의 모드 {(lecture ? "켬" : "끔")})");
            if (lecture)
            {
                LectureMode.StartFromTerm(entry);
            }
            else
            {
                LectureMode.SetEnabled(false);
                SceneManager.LoadScene(entry.Scene);
            }
        }
    }
}
