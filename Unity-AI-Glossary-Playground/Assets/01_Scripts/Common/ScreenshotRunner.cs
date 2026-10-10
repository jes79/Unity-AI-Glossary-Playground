using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlossaryDemo
{
    // README 용 스크린샷 자동 촬영
    // 플레이어를 "-glossaryShots <폴더> [-glossaryShotList 0000_Hub,0105_CoyoteTime,...]" 인자로 실행하면
    // 씬을 차례로 열어 PNG 로 저장하고 종료함 (목록이 없으면 Build Settings 의 모든 씬)
    // 인자가 없으면 아무 일도 하지 않음 (일반 실행에 영향 없음)
    public class ScreenshotRunner : MonoBehaviour
    {
        private float waitSeconds = 3f; // 씬 로드 후 연출이 자리 잡을 때까지 대기 (-glossaryShotWait 로 변경)

        private string outputDir;
        private readonly List<string> scenes = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string dir = null, list = null, wait = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-glossaryShots") dir = args[i + 1];
                if (args[i] == "-glossaryShotList") list = args[i + 1];
                if (args[i] == "-glossaryShotWait") wait = args[i + 1];
            }
            if (dir == null) return;

            var go = new GameObject("ScreenshotRunner");
            DontDestroyOnLoad(go);
            var runner = go.AddComponent<ScreenshotRunner>();
            runner.outputDir = dir;
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
            foreach (string sceneName in scenes)
            {
                if (!Application.CanStreamedLevelBeLoaded(sceneName)) continue;

                Debug.Log($"[Shot] {sceneName}"); // Player.log 에서 씬별 오류를 구분하기 위한 표시
                Time.timeScale = 1f;
                SceneManager.LoadScene(sceneName);
                yield return new WaitForSecondsRealtime(waitSeconds);
                yield return new WaitForEndOfFrame();

                ScreenCapture.CaptureScreenshot(Path.Combine(outputDir, sceneName + ".png"));
                yield return null;
                yield return null;
            }
            Application.Quit();
        }
    }
}
