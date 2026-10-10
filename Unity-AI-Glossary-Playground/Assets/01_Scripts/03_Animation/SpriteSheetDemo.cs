using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // 스프라이트 시트 애니메이션 데모
    // - 한 장의 텍스처(8프레임 가로 띠)를 8개 스프라이트로 자르고(Slice), AnimationClip 이 SpriteRenderer.sprite 를 프레임마다 교체
    // - 위쪽에 잘린 8프레임을 나열하고 지금 보이는 프레임을 노란 테두리로 표시
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class SpriteSheetDemo : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private Transform[] frameSlots;    // 위쪽 프레임 목록의 각 칸
        [SerializeField] private Transform highlight;
        [SerializeField] private float clipFrameRate = 12f;  // 클립에 구워진 fps
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float limitX = 7f;

        private Animator animator;
        private SpriteRenderer sr;
        private float fps = 12f;
        private bool alwaysPlay;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            float x = DemoInput.Move.x;
            if (Mathf.Abs(x) > 0.01f)
            {
                sr.flipX = x < 0f; // 왼쪽으로 갈 때 좌우 반전 (같은 시트 재사용)
                var p = transform.position + Vector3.right * (x * moveSpeed * Time.deltaTime);
                p.x = Mathf.Clamp(p.x, -limitX, limitX);
                transform.position = p;
            }

            if (DemoInput.KeyDown(Key.Digit1)) fps = 6f;
            if (DemoInput.KeyDown(Key.Digit2)) fps = 12f;
            if (DemoInput.KeyDown(Key.Digit3)) fps = 24f;
            if (DemoInput.KeyDown(Key.Space)) alwaysPlay = !alwaysPlay;

            // 멈춰 있으면 애니메이션도 멈춤 (Animator.speed 로 재생 속도 = fps 조절)
            bool moving = Mathf.Abs(x) > 0.01f || alwaysPlay;
            animator.speed = moving ? fps / clipFrameRate : 0f;

            int index = System.Array.IndexOf(frames, sr.sprite);
            if (highlight != null && index >= 0 && index < frameSlots.Length) highlight.position = frameSlots[index].position;

            GlossaryInfoPanel.Status = $"현재 프레임: {index + 1} / {frames.Length}  ({(sr.sprite != null ? sr.sprite.name : "-")})\n" +
                                       $"재생 속도: {fps:0} fps   {(moving ? "재생 중" : "정지 (이동하면 재생)")}";
        }
    }
}
