using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace GlossaryDemo.Rendering
{
    // 스테이지별 리소스를 "필요할 때만" 불러오고, 다른 스테이지로 가면 내려놓는 데모
    // Addressables 패키지가 없어서 같은 흐름을 Resources.LoadAsync 로 보여 줌
    //   Addressables:  Addressables.LoadAssetAsync<GameObject>("stage1")  →  Addressables.Release(handle)
    //   이 데모:       Resources.LoadAsync<GameObject>(path)                →  Destroy + Resources.UnloadUnusedAssets()
    public class StageResourceLoader : MonoBehaviour
    {
        [SerializeField] private string resourceFolder = "Glossary1018";   // Resources/Glossary1018/Stage1 ...
        [SerializeField] private string[] stageNames = { "Stage1_Forest", "Stage2_Desert", "Stage3_Ice" };
        [SerializeField] private Image progressFill;
        [SerializeField] private float minimumLoadingTime = 0.6f;          // 로딩 화면이 너무 빨리 사라지지 않게 (연출용)

        private GameObject currentInstance;
        private int currentStage = -1;
        private bool loading;
        private float lastLoadMs;
        private string log = "";

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) LoadStage(0);
            if (DemoInput.KeyDown(Key.Digit2)) LoadStage(1);
            if (DemoInput.KeyDown(Key.Digit3)) LoadStage(2);
            if (DemoInput.KeyDown(Key.U)) Unload();

            GlossaryInfoPanel.Status =
                $"현재 스테이지: <b>{(currentStage < 0 ? "없음 (아무것도 메모리에 없음)" : stageNames[currentStage])}</b>{(loading ? "  로딩 중..." : "")}\n" +
                $"마지막 로드 {lastLoadMs:0} ms   전체 할당 메모리 {Profiler.GetTotalAllocatedMemoryLong() / 1048576f:0.0} MB\n{log}";
        }

        public void LoadStage(int index)
        {
            if (loading || index == currentStage) return;
            StartCoroutine(LoadRoutine(index));
        }

        private IEnumerator LoadRoutine(int index)
        {
            loading = true;
            Unload(); // 이전 스테이지 리소스부터 내려놓기
            float start = Time.realtimeSinceStartup;

            // 비동기 로드: 로딩 중에도 게임이 멈추지 않음
            ResourceRequest req = Resources.LoadAsync<GameObject>($"{resourceFolder}/{stageNames[index]}");
            while (!req.isDone || Time.realtimeSinceStartup - start < minimumLoadingTime)
            {
                float p = Mathf.Min(req.progress, (Time.realtimeSinceStartup - start) / minimumLoadingTime);
                if (progressFill != null) progressFill.fillAmount = p;
                yield return null;
            }
            if (progressFill != null) progressFill.fillAmount = 1f;

            lastLoadMs = (Time.realtimeSinceStartup - start) * 1000f;
            var prefab = req.asset as GameObject;
            if (prefab == null)
            {
                log = $"<color=#F88>불러오기 실패: Resources/{resourceFolder}/{stageNames[index]}</color>";
                loading = false;
                yield break;
            }
            currentInstance = Instantiate(prefab);
            currentStage = index;
            log = $"로드 완료: {prefab.name}";
            loading = false;
        }

        // 언로드: 인스턴스 파괴 → 참조가 없어진 에셋(메시·머티리얼·텍스처)을 메모리에서 해제
        public void Unload()
        {
            if (currentInstance != null) Destroy(currentInstance);
            currentInstance = null;
            if (currentStage >= 0) log = $"언로드: {stageNames[currentStage]}";
            currentStage = -1;
            if (progressFill != null) progressFill.fillAmount = 0f;
            Resources.UnloadUnusedAssets(); // Addressables 에서는 Addressables.Release(handle)
        }
    }
}
