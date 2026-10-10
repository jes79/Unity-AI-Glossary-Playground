using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.AnimationDemo
{
    // Animation Event 데모: 공격 클립 12프레임(30fps → 0.4초)에 들어 있는 이벤트가 OnAttackHit() 을 호출 → 데미지 판정
    // 비교: '버튼 누르는 즉시 판정' 모드에서는 칼을 휘두르기도 전에 허수아비가 맞음 (타이밍 어긋남)
    // Animation Event 는 Animator 와 같은 GameObject 의 스크립트 함수를 이름으로 찾아 호출함
    [RequireComponent(typeof(Animator))]
    public class AnimationEventDemo : MonoBehaviour
    {
        [Header("판정")]
        [SerializeField] private bool useAnimationEvent = true;
        [SerializeField] private int hitFrame = 12;            // 빌더가 클립에 넣은 이벤트 프레임 (표시용)
        [SerializeField] private float clipFrameRate = 30f;
        [SerializeField] private float hitRange = 1.8f;
        [SerializeField] private int damage = 10;

        [Header("참조")]
        [SerializeField] private Transform target;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Image progressFill;            // 공격 진행 막대
        [SerializeField] private RectTransform hitMarker;       // 이벤트 위치 표시

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackState = Animator.StringToHash("Attack");

        private Animator animator;
        private int hp = 100;
        private string log = "";
        private Color baseColor;
        private Vector3 targetHome;
        private float lastHitProgress = -1f;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (targetRenderer != null) baseColor = targetRenderer.material.color;
            if (target != null) targetHome = target.position;
        }

        private void Update()
        {
            if (DemoInput.LeftClick || DemoInput.JumpPressed)
            {
                animator.SetTrigger(AttackHash);
                if (!useAnimationEvent) DoHit("즉시 판정"); // 잘못된 예: 칼이 아직 뒤에 있는데 맞음
            }
            if (DemoInput.KeyDown(Key.E)) useAnimationEvent = !useAnimationEvent;
            if (DemoInput.KeyDown(Key.R)) hp = 100;

            // 공격 진행 막대 + 이벤트 지점 표시
            var info = animator.GetCurrentAnimatorStateInfo(0);
            float progress = info.shortNameHash == AttackState ? Mathf.Clamp01(info.normalizedTime) : 0f;
            if (progressFill != null) progressFill.fillAmount = progress;

            float eventTime = hitFrame / clipFrameRate;
            GlossaryInfoPanel.Status =
                $"판정 방식: {(useAnimationEvent ? $"Animation Event ({hitFrame}프레임 = {eventTime:0.00}초)" : "버튼 즉시 (비교용)")}\n" +
                $"허수아비 HP: {hp}   마지막 판정 진행도: {(lastHitProgress < 0f ? "-" : lastHitProgress.ToString("0.00"))}\n{log}";
        }

        // ── Animation Event 가 호출하는 함수 (클립의 이벤트 functionName 과 이름이 같아야 함) ──
        public void OnAttackHit()
        {
            if (!useAnimationEvent) return;
            DoHit($"Animation Event ({hitFrame}프레임)");
        }

        private void DoHit(string source)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            lastHitProgress = info.shortNameHash == AttackState ? info.normalizedTime : 0f;
            if (target == null) return;

            // 앞쪽 거리 판정
            Vector3 to = target.position - transform.position;
            to.y = 0f;
            if (to.magnitude <= hitRange && Vector3.Dot(transform.forward, to.normalized) > 0.3f)
            {
                hp = Mathf.Max(0, hp - damage);
                log = $"<color=#FF8080>명중!</color> {source} → -{damage}";
                StopAllCoroutines();
                StartCoroutine(HitReaction());
            }
            else log = $"빗나감 ({source})";
        }

        private IEnumerator HitReaction()
        {
            if (targetRenderer != null) targetRenderer.material.color = Color.white;
            Vector3 push = transform.forward * 0.25f;
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                float k = 1f - t / 0.25f;
                target.position = targetHome + push * k;
                target.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 60f) * 8f * k);
                yield return null;
            }
            target.position = targetHome;
            target.rotation = Quaternion.identity;
            if (targetRenderer != null) targetRenderer.material.color = baseColor;
        }

        // 이벤트 마커를 진행 막대 위 비율 위치에 배치 (빌더에서 호출되지 않아도 런타임에 맞춤)
        private void Start()
        {
            if (hitMarker == null || progressFill == null) return;
            var clips = animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.animationClips : null;
            float length = 0.8f;
            if (clips != null)
                foreach (var c in clips)
                    if (c.name.EndsWith("_Attack")) length = c.length;
            float ratio = hitFrame / clipFrameRate / length;
            var parent = (RectTransform)progressFill.transform.parent;
            hitMarker.anchoredPosition = new Vector2((ratio - 0.5f) * parent.rect.width, hitMarker.anchoredPosition.y);
        }
    }
}
