using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // 웨이브 스포너: 웨이브 n 의 적 수 = baseCount + (n-1) × increasePerWave (기본 5, 10, 15 ...)
    // 스폰 지점에서 spawnInterval 간격으로 하나씩 생성 → 모두 처치되면 휴식 후 다음 웨이브
    public class WaveSpawner : MonoBehaviour
    {
        [Header("웨이브")]
        [SerializeField] private int baseCount = 5;
        [SerializeField] private int increasePerWave = 5;   // 웨이브마다 5마리씩 증가
        [SerializeField] private float spawnInterval = 0.35f;
        [SerializeField] private float timeBetweenWaves = 3f;

        [Header("참조")]
        [SerializeField] private WaveEnemy enemyPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform baseTarget;       // 적이 향하는 기지
        [SerializeField] private TextMeshProUGUI bannerText;

        [Header("기지 포탑")]
        [SerializeField] private float towerRange = 10f;
        [SerializeField] private float towerFireInterval = 0.22f;
        [SerializeField] private LineRenderer shotLine;

        private readonly List<WaveEnemy> alive = new List<WaveEnemy>();
        private int wave;
        private int spawnedThisWave;
        private int countThisWave;
        private int kills;
        private int baseHp = 20;
        private float fireTimer;
        private float shotTimer;
        private string phase = "";
        private Coroutine loop;

        public int CountForWave(int n) => baseCount + (n - 1) * increasePerWave;

        private void Start() => loop = StartCoroutine(WaveLoop());

        private IEnumerator WaveLoop()
        {
            while (true)
            {
                wave++;
                countThisWave = CountForWave(wave);
                spawnedThisWave = 0;
                phase = "스폰 중";
                Banner($"웨이브 {wave}  —  적 {countThisWave}마리");

                for (int i = 0; i < countThisWave; i++)
                {
                    var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    var e = Instantiate(enemyPrefab, p.position + new Vector3(Random.Range(-1f, 1f), 0.4f, Random.Range(-1f, 1f)), Quaternion.identity);
                    e.Init(baseTarget, this, 1.6f + wave * 0.1f);
                    alive.Add(e);
                    spawnedThisWave++;
                    yield return new WaitForSeconds(spawnInterval);
                }

                phase = "전투 중";
                while (alive.Count > 0) yield return null;

                phase = "휴식";
                Banner($"웨이브 {wave} 클리어!  다음 웨이브: 적 {CountForWave(wave + 1)}마리 (+{increasePerWave})");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.K)) KillAll();
            if (DemoInput.KeyDown(Key.R)) Restart();

            TowerFire();

            if (bannerText != null)
                bannerText.alpha = Mathf.MoveTowards(bannerText.alpha, 0f, Time.deltaTime * 0.4f);

            GlossaryInfoPanel.Status = $"웨이브 {wave} ({phase})   생성 {spawnedThisWave}/{countThisWave}   남은 적 {alive.Count}\n" +
                                       $"웨이브별 적 수: {CountForWave(1)}, {CountForWave(2)}, {CountForWave(3)}, {CountForWave(4)}, {CountForWave(5)} ...\n" +
                                       $"처치 {kills}   기지 HP {baseHp}";
        }

        // 기지 포탑이 사거리 안 가장 가까운 적을 자동 사격
        private void TowerFire()
        {
            fireTimer -= Time.deltaTime;
            shotTimer -= Time.deltaTime;
            if (shotLine != null) shotLine.enabled = shotTimer > 0f;
            if (fireTimer > 0f) return;

            WaveEnemy best = null;
            float bestDist = towerRange;
            foreach (var e in alive)
            {
                float d = Vector3.Distance(e.transform.position, baseTarget.position);
                if (d < bestDist) { bestDist = d; best = e; }
            }
            if (best == null) return;
            fireTimer = towerFireInterval;
            if (shotLine != null)
            {
                shotLine.positionCount = 2;
                shotLine.SetPosition(0, baseTarget.position + Vector3.up * 2f);
                shotLine.SetPosition(1, best.transform.position);
                shotTimer = 0.06f;
            }
            Remove(best, true);
        }

        public void ReachedBase(WaveEnemy e)
        {
            baseHp = Mathf.Max(0, baseHp - 1);
            Remove(e, false);
        }

        private void Remove(WaveEnemy e, bool killed)
        {
            alive.Remove(e);
            if (killed) kills++;
            Destroy(e.gameObject);
        }

        private void KillAll()
        {
            foreach (var e in alive.ToArray()) Remove(e, true);
        }

        private void Restart()
        {
            if (loop != null) StopCoroutine(loop);
            foreach (var e in alive.ToArray()) Remove(e, false);
            wave = 0;
            kills = 0;
            baseHp = 20;
            loop = StartCoroutine(WaveLoop());
        }

        private void Banner(string text)
        {
            if (bannerText == null) return;
            bannerText.text = text;
            bannerText.alpha = 1f;
        }
    }
}
