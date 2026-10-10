using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // Gizmos 데모 제어: 런타임 선 표시 토글 + 상태 표시
    public class GizmosDemo : MonoBehaviour
    {
        [SerializeField] private EnemyVisionGizmos[] enemies;
        [SerializeField] private TextMeshProUGUI toggleLabel;

        private void Start() => RefreshLabel();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.G)) ToggleRuntimeLines();

            int seen = 0, attack = 0;
            foreach (var e in enemies)
            {
                if (e.CanSee) seen++;
                if (e.InAttack) attack++;
            }
            GlossaryInfoPanel.Status = $"나를 본 적 {seen}   공격 범위 안 {attack}\n" +
                                       $"LineRenderer 표시 {(EnemyVisionGizmos.ShowRuntimeLines ? "ON" : "OFF → Game 뷰 'Gizmos' 버튼을 켜야 보임")}";
        }

        public void ToggleRuntimeLines()
        {
            EnemyVisionGizmos.ShowRuntimeLines = !EnemyVisionGizmos.ShowRuntimeLines;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (toggleLabel != null) toggleLabel.text = EnemyVisionGizmos.ShowRuntimeLines ? "LineRenderer 선 ON (G)" : "LineRenderer 선 OFF (G)";
        }

        private void OnDestroy() => EnemyVisionGizmos.ShowRuntimeLines = true;
    }
}
