using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.PhysicsDemo
{
    // 레이어 충돌 매트릭스 (Layer Collision Matrix)
    // 원래는 Project Settings > Physics > Layer Collision Matrix 의 체크박스로 설정함.
    // 이 데모는 같은 일을 코드(Physics.IgnoreLayerCollision)로 실행 중에 바꿔서 보여 줌.
    // ※ IgnoreLayerCollision 은 프로젝트 전체 설정을 바꾸므로 씬을 나갈 때 원래대로 되돌림
    public class LayerMatrixDemo : MonoBehaviour
    {
        [SerializeField] private string[] layerNames = { "Player", "Enemy", "PlayerBullet", "EnemyBullet" };
        [SerializeField] private Image[] cells;            // layerNames.Length × layerNames.Length (아래 삼각형만 사용)
        [SerializeField] private TextMeshProUGUI[] cellLabels;
        [SerializeField] private Rigidbody bulletPrefab;    // EnemyBullet 레이어 총알
        [SerializeField] private Transform[] turrets;
        [SerializeField] private Transform player;
        [SerializeField] private float fireInterval = 0.8f;
        [SerializeField] private float bulletSpeed = 9f;

        private int[] layers;
        private bool[,] original;
        private float timer;

        public static int FriendlyFireHits, PlayerHits;

        private void Awake()
        {
            int n = layerNames.Length;
            layers = new int[n];
            for (int i = 0; i < n; i++) layers[i] = LayerMask.NameToLayer(layerNames[i]);
            // 원래 설정 저장
            original = new bool[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    original[i, j] = Physics.GetIgnoreLayerCollision(layers[i], layers[j]);
            FriendlyFireHits = PlayerHits = 0;
        }

        private void OnDestroy()
        {
            // 프로젝트 설정 원상 복구 (다른 씬에 영향 주지 않게)
            int n = layerNames.Length;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    Physics.IgnoreLayerCollision(layers[i], layers[j], original[i, j]);
        }

        private void Update()
        {
            // M: 요청 예문의 핵심 설정 "EnemyBullet ↔ Enemy 충돌 끄기"
            if (DemoInput.KeyDown(Key.M)) Toggle(IndexOf("EnemyBullet") * layerNames.Length + IndexOf("Enemy"));

            timer -= Time.deltaTime;
            if (timer <= 0f && bulletPrefab != null && player != null)
            {
                timer = fireInterval;
                foreach (var t in turrets)
                {
                    Vector3 dir = (player.position - t.position);
                    dir.y = 0f;
                    var b = Instantiate(bulletPrefab, t.position, Quaternion.identity);
                    b.linearVelocity = dir.normalized * bulletSpeed;
                }
            }

            RefreshGrid();
            bool ignored = Physics.GetIgnoreLayerCollision(LayerMask.NameToLayer("EnemyBullet"), LayerMask.NameToLayer("Enemy"));
            GlossaryInfoPanel.Status = $"EnemyBullet ↔ Enemy 충돌: {(ignored ? "끔 (적 몸을 통과)" : "켬 (적이 아군 총알에 맞음)")}\n" +
                                       $"아군 오사 {FriendlyFireHits}회   플레이어 피격 {PlayerHits}회";
        }

        // 버튼에서 호출: index = row * n + col
        public void Toggle(int index)
        {
            int n = layerNames.Length;
            int a = layers[index / n], b = layers[index % n];
            bool ignore = !Physics.GetIgnoreLayerCollision(a, b);
            Physics.IgnoreLayerCollision(a, b, ignore); // 대칭이라 (a,b) 와 (b,a) 가 같이 바뀜
        }

        private int IndexOf(string layer) => System.Array.IndexOf(layerNames, layer);

        private void RefreshGrid()
        {
            int n = layerNames.Length;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    int k = i * n + j;
                    if (k >= cells.Length || cells[k] == null) continue;
                    bool collide = !Physics.GetIgnoreLayerCollision(layers[i], layers[j]);
                    cells[k].color = collide ? new Color(0.3f, 0.75f, 0.4f) : new Color(0.35f, 0.2f, 0.22f);
                    if (k < cellLabels.Length && cellLabels[k] != null) cellLabels[k].text = collide ? "충돌" : "무시";
                }
        }
    }
}
