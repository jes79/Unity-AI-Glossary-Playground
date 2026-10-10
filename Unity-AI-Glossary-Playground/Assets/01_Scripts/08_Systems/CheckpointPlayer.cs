using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Systems
{
    // 체크포인트 / 리스폰: 깃발(Checkpoint 태그)을 지나면 그 위치를 저장하고, 죽으면 마지막 깃발에서 부활
    [RequireComponent(typeof(Rigidbody2D))]
    public class CheckpointPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 12f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float deathY = -7f;
        [SerializeField] private float respawnDelay = 0.7f;
        [SerializeField] private bool useCheckpoints = true;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Camera cam;
        [SerializeField] private Sprite flagActiveSprite;

        private Rigidbody2D rb;
        private Vector3 startPos;
        private Vector3 respawnPos;          // 마지막으로 저장된 리스폰 위치
        private Transform lastFlag;
        private bool dead;
        private int deaths;
        private int flagIndex;
        private float camY;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            startPos = respawnPos = transform.position;
            if (cam == null) cam = Camera.main;
            camY = cam.transform.position.y;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.C)) useCheckpoints = !useCheckpoints;
            if (DemoInput.KeyDown(Key.K)) Die();

            if (!dead)
            {
                bool grounded = Physics2D.OverlapBox((Vector2)transform.position + Vector2.down * 0.5f, new Vector2(0.7f, 0.1f), 0f, groundMask) != null;
                if (DemoInput.JumpPressed && grounded) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                if (transform.position.y < deathY) Die();
            }

            // 카메라가 x 축으로 따라감
            var cp = cam.transform.position;
            cp.x = Mathf.Lerp(cp.x, transform.position.x + 3f, Time.deltaTime * 5f);
            cp.y = camY;
            cam.transform.position = cp;

            Vector3 target = useCheckpoints ? respawnPos : startPos;
            GlossaryInfoPanel.Status =
                $"체크포인트 사용: {(useCheckpoints ? "ON" : "<color=#FF7A7A>OFF (항상 처음부터)</color>")}\n" +
                $"마지막 깃발: {(lastFlag != null ? lastFlag.name : "없음 (시작 지점)")}   리스폰 위치 ({target.x:0.0}, {target.y:0.0})\n" +
                $"사망 {deaths}회{(dead ? "   <color=#FF7A7A>리스폰 중...</color>" : "")}";
        }

        private void FixedUpdate()
        {
            if (!dead) rb.linearVelocity = new Vector2(DemoInput.Move.x * moveSpeed, rb.linearVelocity.y);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (dead) return;
            if (other.CompareTag("Checkpoint")) Activate(other.transform);
            else if (other.CompareTag("Enemy")) Die();
        }

        // 깃발 통과 → 리스폰 위치 갱신 (이미 지난 깃발보다 뒤쪽 깃발은 무시)
        private void Activate(Transform flag)
        {
            if (flag == lastFlag || (lastFlag != null && flag.position.x < lastFlag.position.x)) return;
            lastFlag = flag;
            flagIndex++;
            respawnPos = flag.position + Vector3.up * 0.5f;
            var sr = flag.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (flagActiveSprite != null) sr.sprite = flagActiveSprite;
                sr.color = new Color(0.4f, 1f, 0.5f);
            }
            DamagePopup.Spawn(flag.position + Vector3.up * 1.5f, "체크포인트!", new Color(0.5f, 1f, 0.6f), 5f);
        }

        public void Die()
        {
            if (dead) return;
            StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            dead = true;
            deaths++;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            for (float t = 0f; t < respawnDelay; t += Time.deltaTime)
            {
                sprite.color = Color.Lerp(Color.red, new Color(1f, 0f, 0f, 0f), t / respawnDelay);
                yield return null;
            }
            transform.position = useCheckpoints ? respawnPos : startPos; // 마지막 체크포인트에서 부활
            rb.simulated = true;
            sprite.color = new Color(0.3f, 0.65f, 1f);
            dead = false;
        }
    }
}
