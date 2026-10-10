using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // GameManager 상태에 반응하는 미니 게임 (떨어지는 블록 피하기)
    // - Playing 일 때만 블록 생성/이동, 플레이어 조작
    // - 상태가 바뀌면 이벤트로 알림을 받아 화면(오버레이)을 교체
    public class DodgeGame : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Sprite blockSprite;
        [SerializeField] private float playerSpeed = 8f;
        [SerializeField] private float spawnInterval = 0.45f;
        [SerializeField] private float fallSpeed = 5f;
        [SerializeField] private float halfWidth = 5f;
        [SerializeField] private float centerX = 3f;

        [Header("상태별 화면")]
        [SerializeField] private GameObject readyScreen;
        [SerializeField] private GameObject pausedScreen;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI hudText;

        private readonly List<Transform> blocks = new List<Transform>();
        private readonly List<string> history = new List<string>();
        private float spawnTimer;
        private GameManager gm;

        private void Start()
        {
            gm = GameManager.Instance;
            gm.OnStateChanged += HandleStateChanged; // 상태 변화 구독
            HandleStateChanged(gm.State, gm.State);
        }

        private void OnDestroy()
        {
            if (gm != null) gm.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState prev, GameState next)
        {
            if (prev != next)
            {
                history.Add($"{prev}→{next}");
                if (history.Count > 4) history.RemoveAt(0);
            }
            readyScreen.SetActive(next == GameState.Ready);
            pausedScreen.SetActive(next == GameState.Paused);
            gameOverScreen.SetActive(next == GameState.GameOver);

            if (next == GameState.Ready) ClearBlocks();
            if (next == GameState.GameOver)
                gameOverText.text = $"GAME OVER\n<size=60%>버틴 시간 {gm.Score:0.0}초   최고 {gm.BestScore:0.0}초\nR : 다시 (Ready 로)</size>";
        }

        private void Update()
        {
            hudText.text = $"상태: <b>{gm.State}</b>    시간 {gm.Score:0.0}초";
            GlossaryInfoPanel.Status = $"GameManager.State = <b>{gm.State}</b>   Time.timeScale = {Time.timeScale}\n최근 전환: {string.Join(", ", history)}";

            if (gm.State != GameState.Playing) return; // Playing 이 아니면 게임 로직 정지

            var p = player.position;
            p.x = Mathf.Clamp(p.x + DemoInput.Move.x * playerSpeed * Time.deltaTime, centerX - halfWidth, centerX + halfWidth);
            player.position = p;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = Mathf.Max(0.15f, spawnInterval - gm.Score * 0.01f); // 시간이 갈수록 빨라짐
                SpawnBlock();
            }

            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                var b = blocks[i];
                b.position += Vector3.down * (fallSpeed + gm.Score * 0.1f) * Time.deltaTime;
                if (Mathf.Abs(b.position.x - p.x) < 0.65f && Mathf.Abs(b.position.y - p.y) < 0.65f)
                {
                    gm.GameOver(); // 충돌 → 게임 오버 상태로
                    return;
                }
                if (b.position.y < -6f)
                {
                    Destroy(b.gameObject);
                    blocks.RemoveAt(i);
                }
            }
        }

        private void SpawnBlock()
        {
            var go = new GameObject("Block");
            go.transform.position = new Vector3(Random.Range(centerX - halfWidth, centerX + halfWidth), 6f, 0f);
            go.transform.localScale = Vector3.one * 0.7f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = blockSprite;
            sr.color = new Color(1f, 0.4f, 0.35f);
            blocks.Add(go.transform);
        }

        private void ClearBlocks()
        {
            foreach (var b in blocks) if (b != null) Destroy(b.gameObject);
            blocks.Clear();
        }
    }
}
