using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlossaryDemo.Patterns
{
    // 싱글톤 GameManager
    // - static Instance 로 어디서나 GameManager.Instance.AddScore() 처럼 접근
    // - Awake 에서 이미 Instance 가 있으면 자기 자신을 파괴 → 항상 하나만 존재
    // - DontDestroyOnLoad → 씬이 바뀌어도 파괴되지 않아 점수가 유지됨
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // 데모 비교 토글 (static 이라 씬을 다시 불러와도 값이 남음)
        public static bool UseDontDestroyOnLoad = true;

        // 화면 로그 (씬이 바뀌어도 남도록 static)
        public static readonly List<string> Log = new();

        private static int createdCount;

        [SerializeField] private int score;
        private int number;

        public int Score => score;
        public int Number => number;

        // 에디터에서 Domain Reload 를 끈 경우에도 static 값이 초기화되도록
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            UseDontDestroyOnLoad = true;
            createdCount = 0;
            Log.Clear();
        }

        private void Awake()
        {
            createdCount++;
            if (Instance != null && Instance != this)
            {
                // 씬을 다시 불러오면 씬에 배치된 GameManager 가 또 생기지만, 기존 것이 있으면 새 것을 지움
                AddLog($"새 GameManager #{createdCount} 감지 → 이미 #{Instance.number} 가 있어서 <color=#FF8A65>Destroy</color>");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            number = createdCount;
            if (UseDontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            AddLog($"GameManager #{number} 이 Instance 가 됨 (DontDestroyOnLoad {(UseDontDestroyOnLoad ? "ON" : "OFF")})");
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        public void AddScore(int amount)
        {
            score += amount;
        }

        // DontDestroyOnLoad 를 끄면 현재 씬으로 되돌려서, 다음 씬 로드 때 같이 파괴되게 함
        public void SetPersistent(bool persistent)
        {
            UseDontDestroyOnLoad = persistent;
            if (persistent) DontDestroyOnLoad(gameObject);
            else SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            AddLog($"DontDestroyOnLoad {(persistent ? "ON" : "OFF")}");
        }

        // DontDestroyOnLoad OFF 상태에서 씬을 다시 불러오기 직전에 호출: 이 씬과 함께 사라짐을 명확히 함
        public void ReleaseWithScene()
        {
            AddLog($"GameManager #{number} 는 씬과 함께 파괴됨 (점수 {score} 사라짐)");
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Instance = null;
            Destroy(gameObject);
        }

        // 데모 정리: 이 데모 씬이 아닌 곳(허브 등)으로 가면 스스로 제거
        private void OnActiveSceneChanged(Scene from, Scene to)
        {
            if (to.name != "1101_Singleton") Destroy(gameObject);
        }

        public static void AddLog(string message)
        {
            Log.Add(message);
            if (Log.Count > 6) Log.RemoveAt(0);
        }
    }
}
