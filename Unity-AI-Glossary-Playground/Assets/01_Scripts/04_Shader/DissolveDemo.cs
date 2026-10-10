using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0410 디졸브
    // "죽으면 1초 동안 디졸브되고 경계가 주황색으로 빛나게" → _Dissolve 를 1초 동안 0 → 1 로 올림
    // 왼쪽: 비교용으로 그냥 SetActive(false) 로 사라지는 캐릭터
    public class DissolveDemo : MonoBehaviour
    {
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        [SerializeField] private GameObject instantCharacter;
        [SerializeField] private Renderer[] dissolveRenderers;

        [Header("디졸브")]
        [SerializeField, Range(0.1f, 3f)] private float duration = 1f;
        [SerializeField, Range(0.01f, 0.3f)] private float edgeWidth = 0.08f;
        [SerializeField] private Color edgeColor = new Color(1f, 0.35f, 0.05f);
        [SerializeField] private float edgeHdrIntensity = 2f; // 색 × 2^2
        [SerializeField] private bool bottomToTop;
        [SerializeField] private bool autoRevive = true;

        private Material mat;
        private float amount;
        private Coroutine running;
        private bool dead;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(dissolveRenderers[0].sharedMaterial, dissolveRenderers);

            // 아래→위 모드에 쓸 높이 범위 = 캐릭터 전체 경계 상자
            var b = dissolveRenderers[0].bounds;
            foreach (var r in dissolveRenderers) b.Encapsulate(r.bounds);
            mat.SetFloat("_HeightMin", b.min.y);
            mat.SetFloat("_HeightMax", b.max.y);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) Die();
            if (DemoInput.KeyDown(Key.R)) Revive();
            if (DemoInput.KeyDown(Key.H)) ToggleBottomToTop();
            GlossaryInfoPanel.Status =
                $"_Dissolve = <b>{amount:0.00}</b>   시간 {duration:0.0}초   경계 두께 {edgeWidth:0.00}\n" +
                $"아래→위 {ShaderDemoUtil.OnOff(bottomToTop)}   자동 부활 {ShaderDemoUtil.OnOff(autoRevive)}";
        }

        public void Die()
        {
            if (dead) return;
            dead = true;
            if (instantCharacter != null) instantCharacter.SetActive(false); // 비교: 한 프레임에 사라짐
            Play(0f, 1f, duration, autoRevive);
        }

        public void Revive()
        {
            dead = false;
            if (instantCharacter != null) instantCharacter.SetActive(true);
            Play(amount, 0f, 0.5f, false);
        }

        public void SetDuration(float value) => duration = value;
        public void SetEdgeWidth(float value) { edgeWidth = value; Apply(); }
        public void ToggleBottomToTop() { bottomToTop = !bottomToTop; Apply(); }

        private void Play(float from, float to, float time, bool reviveAfter)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Animate(from, to, time, reviveAfter));
        }

        private IEnumerator Animate(float from, float to, float time, bool reviveAfter)
        {
            // Ease.Run: time 초 동안 0→1 진행도를 넘겨 줌 → _Dissolve 값으로 사용
            yield return Ease.Run(time, Ease.Linear, t =>
            {
                amount = Mathf.Lerp(from, to, t);
                Apply();
            });
            if (reviveAfter)
            {
                yield return new WaitForSeconds(1f);
                Revive();
            }
        }

        private void Apply()
        {
            if (mat == null) return;
            mat.SetFloat(DissolveId, amount);
            mat.SetFloat("_EdgeWidth", edgeWidth);
            mat.SetColor("_EdgeColor", edgeColor * Mathf.Pow(2f, edgeHdrIntensity));
            mat.SetFloat("_HeightMode", bottomToTop ? 1f : 0f);
        }
    }
}
