

using UnityEngine;
using UnityEngine.SceneManagement;


namespace GlossaryDemo.Systems
{
    // 씬 사이에 값을 넘기는 가장 간단한 방법: static 필드 (씬이 바뀌어도 메모리에 남음)
    public static class SceneFlow
    {
        public const string TitleScene = "0816_SceneManagement";
        public const string GameScene = "0816_SceneManagement_Game";
        public const string ResultScene = "0816_SceneManagement_Result";
        public const string SceneFolder = "Assets/00_Scenes/08_Systems";

        public static int LastScore;   // 게임 씬 → 결과 씬으로 넘기는 점수
        public static int BestScore;
        public static int PlayCount;

        // 비동기 씬 로드. Build Settings 에 없으면(에디터에서 목록이 갱신된 경우) 에디터 전용 방법으로 로드
        public static AsyncOperation LoadAsync(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                return SceneManager.LoadSceneAsync(sceneName);
#if UNITY_EDITOR
            Debug.LogWarning($"[0816] '{sceneName}' 이 Build Settings 에 없어 에디터 전용 로드를 사용합니다 (Tools > Glossary > Build Scene > 08 Systems > 0816 다시 실행 권장)");
            return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                $"{SceneFolder}/{sceneName}.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Debug.LogError($"씬 '{sceneName}' 이 Build Settings 에 없음");
            return null;
#endif
        }
    }

}
