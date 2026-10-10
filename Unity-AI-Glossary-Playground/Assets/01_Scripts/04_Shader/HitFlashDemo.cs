using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0413 피격 플래시
    // 맞은 적의 렌더러에만 MaterialPropertyBlock 으로 _FlashAmount = 1 → flashDuration(0.1초) 동안 0 으로
    // 5마리가 머티리얼 하나를 공유해도 맞은 한 마리만 하얗게 번쩍임
    public class HitFlashDemo : MonoBehaviour
    {
        private static readonly int FlashId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] private Transform[] enemies;
        [SerializeField] private AudioSource hitSound;

        [Header("Hit Flash")]
        [SerializeField, Range(0.02f, 0.5f)] private float flashDuration = 0.1f;
        [SerializeField] private bool flashEnabled = true;
        [SerializeField] private bool fadeOut; // false = 0.1초 동안 꽉 찬 흰색 후 뚝 끊김, true = 서서히 빠짐

        private MaterialPropertyBlock block;
        private Coroutine[] running;
        private Vector3[] baseScales;
        private int hits;
        private Camera cam;

        private void Start()
        {
            block = new MaterialPropertyBlock();
            running = new Coroutine[enemies.Length];
            baseScales = new Vector3[enemies.Length];
            for (int i = 0; i < enemies.Length; i++) baseScales[i] = enemies[i].localScale;
            cam = Camera.main;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) Hit(Random.Range(0, enemies.Length));
            if (DemoInput.KeyDown(Key.F)) ToggleFlash();
            if (DemoInput.KeyDown(Key.G)) fadeOut = !fadeOut;

            if (DemoInput.LeftClick && Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f))
            {
                for (int i = 0; i < enemies.Length; i++)
                    if (hit.transform.IsChildOf(enemies[i])) Hit(i);
            }

            GlossaryInfoPanel.Status = $"Hit Flash {ShaderDemoUtil.OnOff(flashEnabled)}   지속 {flashDuration:0.00}초   방식: {(fadeOut ? "서서히 빠짐" : "딱 끊김")}\n맞힌 횟수 {hits}";
        }

        public void HitRandom() => Hit(Random.Range(0, enemies.Length));
        public void SetDuration(float value) => flashDuration = value;
        public void ToggleFlash() => flashEnabled = !flashEnabled;

        public void Hit(int index)
        {
            hits++;
            if (hitSound != null) hitSound.Play();
            if (running[index] != null) StopCoroutine(running[index]);
            running[index] = StartCoroutine(FlashRoutine(index));
        }

        private IEnumerator FlashRoutine(int index)
        {
            var renderers = enemies[index].GetComponentsInChildren<Renderer>();
            float t = 0f;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                float k = t / flashDuration;
                float flash = flashEnabled ? (fadeOut ? 1f - k : 1f) : 0f;
                SetFlash(renderers, flash);
                // 살짝 찌그러지는 연출 (플래시 OFF 와 비교할 때도 맞은 건 알 수 있게)
                enemies[index].localScale = Vector3.Scale(baseScales[index], new Vector3(1f + 0.15f * (1f - k), 1f - 0.12f * (1f - k), 1f + 0.15f * (1f - k)));
                yield return null;
            }
            SetFlash(renderers, 0f);
            enemies[index].localScale = baseScales[index];
            running[index] = null;
        }

        private void SetFlash(Renderer[] renderers, float amount)
        {
            foreach (var r in renderers)
            {
                r.GetPropertyBlock(block);
                block.SetFloat(FlashId, amount);
                r.SetPropertyBlock(block);
            }
        }
    }
}
