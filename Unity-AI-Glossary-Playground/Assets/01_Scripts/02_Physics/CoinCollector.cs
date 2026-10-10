using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 트리거로 코인 먹기
    // - 코인 Collider 의 Is Trigger = true → 몸이 통과하고, OnTriggerEnter 가 호출됨 → 점수 +10
    // - T 키로 Is Trigger 를 끄면 코인이 "벽"처럼 막히고 OnCollisionEnter 만 호출됨 (점수 없음)
    // 이벤트를 받으려면 둘 중 하나에 Rigidbody 가 있어야 함 (여기서는 플레이어)
    public class CoinCollector : MonoBehaviour
    {
        [SerializeField] private int scorePerCoin = 10;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private float respawnTime = 3f;

        private int score;
        private bool coinsAreTriggers = true;
        private string lastEvent = "-";
        private GameObject[] coins;

        private void Start() => coins = GameObject.FindGameObjectsWithTag("Coin"); // 꺼진 오브젝트는 못 찾으므로 시작할 때 저장

        private void Update()
        {
            if (DemoInput.KeyDown(Key.T))
            {
                coinsAreTriggers = !coinsAreTriggers;
                foreach (var coin in coins)
                    coin.GetComponent<Collider>().isTrigger = coinsAreTriggers;
            }

            if (scoreText != null) scoreText.text = $"점수 {score}";
            GlossaryInfoPanel.Status = $"코인 Is Trigger: {(coinsAreTriggers ? "ON (통과 + OnTriggerEnter)" : "OFF (막힘 + OnCollisionEnter)")}\n마지막 이벤트: {lastEvent}";
        }

        // 트리거 Collider 에 들어간 순간 1번
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Coin")) return;
            score += scorePerCoin;
            lastEvent = $"OnTriggerEnter({other.name}) → +{scorePerCoin}";
            FloatingText.Spawn(other.transform.position + Vector3.up, $"+{scorePerCoin}", new Color(1f, 0.85f, 0.3f));
            StartCoroutine(Respawn(other.gameObject));
        }

        // 일반(트리거 아님) Collider 와 부딪힌 순간 1번
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.CompareTag("Coin")) return;
            lastEvent = $"OnCollisionEnter({collision.collider.name}) → 부딪힘, 점수 없음";
            FloatingText.Spawn(collision.transform.position + Vector3.up, "쿵!", new Color(0.8f, 0.8f, 0.9f), 4f);
        }

        private IEnumerator Respawn(GameObject coin)
        {
            coin.SetActive(false);
            yield return new WaitForSeconds(respawnTime);
            coin.SetActive(true);
        }
    }
}
