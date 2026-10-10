using System.Collections;
using TMPro;
using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 물리 데모 공용 체력: 데미지를 받으면 붉게 깜빡이고 숫자를 띄움. 0 이 되면 잠깐 사라졌다 다시 나타남
    public class DemoHealth : MonoBehaviour
    {
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private Renderer body;
        [SerializeField] private TextMeshPro hpLabel;      // 머리 위 HP 표시 (선택)
        [SerializeField] private float respawnDelay = 2f;
        [SerializeField] private bool respawn = true;

        private float hp;
        private Color baseColor;
        private Coroutine flash;

        public float Hp => hp;
        public float MaxHp => maxHp;
        public bool IsDead => hp <= 0f;

        private void Awake()
        {
            hp = maxHp;
            if (body == null) body = GetComponentInChildren<Renderer>();
            if (body != null) baseColor = body.sharedMaterial.GetColor("_BaseColor");
            UpdateLabel();
        }

        public void TakeDamage(float amount, bool showNumber = true)
        {
            if (IsDead) return;
            hp = Mathf.Max(0f, hp - amount);
            if (showNumber) FloatingText.Spawn(transform.position + Vector3.up * 1.5f, $"-{amount:0}", new Color(1f, 0.45f, 0.35f));
            if (flash != null) StopCoroutine(flash);
            if (isActiveAndEnabled) flash = StartCoroutine(Flash());
            UpdateLabel();
            if (IsDead && respawn) StartCoroutine(Respawn());
        }

        public void ResetHp()
        {
            hp = maxHp;
            UpdateLabel();
        }

        private IEnumerator Flash()
        {
            if (body == null) yield break;
            body.material.SetColor("_BaseColor", new Color(1f, 0.25f, 0.2f));
            yield return new WaitForSeconds(0.12f);
            body.material.SetColor("_BaseColor", IsDead ? baseColor * 0.4f : baseColor);
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(0.3f);
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            yield return new WaitForSeconds(respawnDelay);
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = true;
            if (body != null) body.material.SetColor("_BaseColor", baseColor);
            ResetHp();
        }

        private void UpdateLabel()
        {
            if (hpLabel != null) hpLabel.text = $"HP {hp:0}";
        }
    }
}
