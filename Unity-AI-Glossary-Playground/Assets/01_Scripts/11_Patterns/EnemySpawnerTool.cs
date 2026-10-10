using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // Custom Editor 데모의 대상 컴포넌트
    // Inspector 의 "적 10마리 생성" 버튼(EnemySpawnerToolEditor)이 이 컴포넌트의 설정값을 읽어 에디터에서 적을 배치함
    // 플레이 중에는 화면 버튼/키로 같은 기능을 런타임 코드로 실행
    public class EnemySpawnerTool : MonoBehaviour
    {
        [Header("생성 설정")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField, Range(1, 50)] private int spawnCount = 10;
        [SerializeField, Range(1f, 15f)] private float radius = 6f;
        [Tooltip("같은 값이면 매번 같은 배치 (Undo/Redo 비교가 쉬움)")]
        [SerializeField] private int seed = 1110;
        [SerializeField] private Transform container;

        public GameObject EnemyPrefab => enemyPrefab;
        public int SpawnCount { get => spawnCount; set => spawnCount = Mathf.Clamp(value, 1, 50); }
        public float Radius { get => radius; set => radius = Mathf.Clamp(value, 1f, 15f); }
        public Transform Container => container != null ? container : transform;

        // 원 둘레에 고르게 + 약간의 흔들림 (에디터 버튼과 런타임이 같은 위치 계산을 공유)
        public List<Vector3> GetSpawnPositions(int salt = 0)
        {
            var rng = new System.Random(seed + salt);
            var list = new List<Vector3>();
            for (int i = 0; i < spawnCount; i++)
            {
                float a = (i / (float)spawnCount) * Mathf.PI * 2f + (float)(rng.NextDouble() - 0.5) * 0.3f;
                float r = radius * (0.75f + (float)rng.NextDouble() * 0.25f);
                list.Add(transform.position + new Vector3(Mathf.Cos(a) * r, 1f, Mathf.Sin(a) * r));
            }
            return list;
        }

        public int CountSpawned() => Container.childCount;

        // ── 플레이 중(런타임) 버전 ──
        private int runtimeSalt;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.G)) SpawnRuntime();
            if (DemoInput.KeyDown(Key.X)) ClearRuntime();
            GlossaryInfoPanel.Status = $"생성된 적 {CountSpawned()}마리   spawnCount {spawnCount}   radius {radius:0.0}\n" +
                                       "에디터(플레이 전)에서는 Inspector 버튼 / Tools > Glossary > Patterns > Enemy Spawner 창 사용";
        }

        public void SpawnRuntime()
        {
            runtimeSalt++;
            foreach (var p in GetSpawnPositions(runtimeSalt))
                Instantiate(enemyPrefab, p, Quaternion.LookRotation(transform.position - new Vector3(p.x, transform.position.y, p.z)), Container);
        }

        public void ClearRuntime()
        {
            for (int i = Container.childCount - 1; i >= 0; i--) Destroy(Container.GetChild(i).gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.3f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
