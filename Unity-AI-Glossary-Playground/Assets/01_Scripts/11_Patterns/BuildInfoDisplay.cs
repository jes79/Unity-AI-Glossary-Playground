using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlossaryDemo.Patterns
{
    // 빌드 설정 데모: 지금 실행 중인 환경(플랫폼·버전·Build Settings 의 씬 목록)을 런타임 API 로 표시
    // WebGL 로 빌드해서 열면 Platform 이 WebGLPlayer 로 바뀌는 것을 확인할 수 있음
    public class BuildInfoDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI runtimeText;
        [SerializeField] private TextMeshProUGUI editorSnapshotText;
        [SerializeField, TextArea(4, 10)] private string editorSnapshot; // 씬 빌드 시점의 에디터 설정 (빌더가 기록)

        private void Start()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Application.platform : <b>{Application.platform}</b>");
            sb.AppendLine($"isEditor : {Application.isEditor}   isMobile : {Application.isMobilePlatform}");
            sb.AppendLine($"productName : {Application.productName}   version : {Application.version}");
            sb.AppendLine($"unityVersion : {Application.unityVersion}");
            sb.AppendLine($"그래픽 API : {SystemInfo.graphicsDeviceType}");
            int count = SceneManager.sceneCountInBuildSettings;
            sb.AppendLine($"Build Settings 의 씬 : {count}개   (지금 씬 index {SceneManager.GetActiveScene().buildIndex})");
            for (int i = 0; i < Mathf.Min(count, 4); i++)
                sb.AppendLine($"   {i}: {System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i))}");
            if (count > 4) sb.Append("   …");
            runtimeText.text = sb.ToString();

            editorSnapshotText.text = string.IsNullOrEmpty(editorSnapshot) ? "(없음)" : editorSnapshot;
        }

        private void Update()
        {
            GlossaryInfoPanel.Status = $"실행 플랫폼 {Application.platform}   화면 {Screen.width}x{Screen.height}   {(Application.isEditor ? "에디터에서 실행 중" : "빌드에서 실행 중")}";
        }
    }
}
