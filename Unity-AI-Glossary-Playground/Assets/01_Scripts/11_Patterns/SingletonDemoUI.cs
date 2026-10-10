using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GlossaryDemo.Patterns
{
    // 씬마다 새로 생기는 UI 쪽 스크립트. GameManager 를 직접 참조하지 않고 GameManager.Instance 로 접근
    // (버튼에 GameManager 를 직접 연결하면, 씬을 다시 불러왔을 때 파괴된 중복 오브젝트를 가리키게 됨)
    public class SingletonDemoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private TextMeshProUGUI ddolLabel;

        private static int sceneLoads;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => sceneLoads = 0;

        private void Start()
        {
            sceneLoads++;
            RefreshLabel();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.A)) AddScore();
            if (DemoInput.KeyDown(Key.R)) ReloadScene();
            if (DemoInput.KeyDown(Key.D)) ToggleDontDestroy();
            if (DemoInput.KeyDown(Key.C)) TryCreateDuplicate();

            var gm = GameManager.Instance;
            scoreText.text = gm != null ? $"SCORE {gm.Score}" : "GameManager 없음";
            infoText.text = gm != null
                ? $"GameManager.Instance = #{gm.Number}   소속 씬: {gm.gameObject.scene.name}\n이 씬을 불러온 횟수: {sceneLoads}"
                : "Instance 없음";
            logText.text = string.Join("\n", GameManager.Log);

            GlossaryInfoPanel.Status = $"DontDestroyOnLoad {(GameManager.UseDontDestroyOnLoad ? "ON → 다시 불러와도 점수 유지" : "<color=#FF6060>OFF → 다시 불러오면 점수 0</color>")}\n" +
                                       "Hierarchy 의 'DontDestroyOnLoad' 항목에 GameManager 가 옮겨 가는 것도 확인해 보세요";
        }

        public void AddScore()
        {
            if (GameManager.Instance != null) GameManager.Instance.AddScore(10);
        }

        public void ReloadScene()
        {
            GameManager.AddLog("— 씬 다시 불러오기 —");
            if (!GameManager.UseDontDestroyOnLoad && GameManager.Instance != null) GameManager.Instance.ReleaseWithScene();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ToggleDontDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetPersistent(!GameManager.UseDontDestroyOnLoad);
            RefreshLabel();
        }

        // 일부러 GameManager 를 하나 더 만들어 봄 → Awake 에서 스스로 파괴됨
        public void TryCreateDuplicate()
        {
            new GameObject("GameManager (중복 시도)").AddComponent<GameManager>();
        }

        private void RefreshLabel()
        {
            if (ddolLabel != null) ddolLabel.text = GameManager.UseDontDestroyOnLoad ? "DontDestroyOnLoad ON (D)" : "DontDestroyOnLoad OFF (D)";
        }
    }
}
