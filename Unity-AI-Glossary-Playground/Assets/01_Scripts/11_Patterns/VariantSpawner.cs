using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // 기본 / 빠른 / 강한 적 프리팹을 각 줄에서 소환
    // 코드에서는 셋 다 똑같이 Instantiate(prefab) — 차이는 프리팹(Variant)에 들어 있음
    public class VariantSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs;      // 0 Base, 1 Fast(Variant), 2 Strong(Variant)
        [SerializeField] private Transform[] lanes;
        [SerializeField] private bool autoSpawn = true;
        [SerializeField] private float interval = 2.5f;
        [SerializeField] private TextMeshProUGUI autoLabel;

        private float timer;
        private int next;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) Spawn(0);
            if (DemoInput.KeyDown(Key.Digit2)) Spawn(1);
            if (DemoInput.KeyDown(Key.Digit3)) Spawn(2);
            if (DemoInput.KeyDown(Key.T)) ToggleAuto();

            if (autoSpawn && (timer -= Time.deltaTime) <= 0f)
            {
                timer = interval;
                Spawn(next);
                next = (next + 1) % prefabs.Length;
            }

            GlossaryInfoPanel.Status = $"자동 소환 {(autoSpawn ? "ON" : "OFF")}   현재 적 {FindObjectsByType<VariantEnemy>(FindObjectsSortMode.None).Length}마리\n" +
                                       "Project 창 Assets/09_Generated/11_Patterns 에서 프리팹 3개를 열어 비교해 보세요";
        }

        public void Spawn(int index)
        {
            if (index < 0 || index >= prefabs.Length) return;
            var lane = lanes[Mathf.Min(index, lanes.Length - 1)];
            Instantiate(prefabs[index], lane.position, Quaternion.Euler(0f, 90f, 0f));
        }

        public void ToggleAuto()
        {
            autoSpawn = !autoSpawn;
            if (autoLabel != null) autoLabel.text = autoSpawn ? "자동 소환 ON (T)" : "자동 소환 OFF (T)";
        }
    }
}
