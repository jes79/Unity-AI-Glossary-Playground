using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // 2D 본 애니메이션 대체 데모 (2D Animation 패키지 미설치)
    // - 부위별 SpriteRenderer 를 본(Transform) 계층 아래에 붙이고, 클립은 본의 회전만 키프레임
    // - 2D Animation 패키지는 한 장의 그림을 메시로 만들어 본에 '가중치'로 붙이므로 관절이 휘어지지만,
    //   여기서는 부위가 딱딱하게 붙어 있는 '컷아웃(cut-out)' 방식 → 원리(본 회전이 자식을 끌고 감)는 같음
    public class Bone2DDemo : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject[] boneGizmos;
        [SerializeField] private SpriteRenderer[] skinParts;
        [SerializeField] private string[] states = { "Walk", "Wave" };
        [SerializeField] private Transform[] watchBones;

        private bool showBones = true;
        private bool showSkin = true;
        private int stateIndex;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) Play(0);
            if (DemoInput.KeyDown(Key.Digit2)) Play(1);
            if (DemoInput.KeyDown(Key.B))
            {
                showBones = !showBones;
                foreach (var g in boneGizmos) g.SetActive(showBones);
            }
            if (DemoInput.KeyDown(Key.H))
            {
                showSkin = !showSkin;
                foreach (var s in skinParts) s.enabled = showSkin;
            }
            if (DemoInput.KeyDown(Key.P)) animator.speed = animator.speed > 0f ? 0f : 1f;

            var sb = new System.Text.StringBuilder();
            sb.Append($"재생: {states[stateIndex]}   본 표시 {(showBones ? "ON" : "OFF")}   그림 {(showSkin ? "ON" : "OFF")}\n");
            foreach (var b in watchBones)
                if (b != null) sb.Append($"{b.name} {Mathf.DeltaAngle(0f, b.localEulerAngles.z):+0;-0}°  ");
            GlossaryInfoPanel.Status = sb.ToString();
        }

        private void Play(int i)
        {
            stateIndex = i;
            animator.speed = 1f;
            animator.CrossFade(states[i], 0.2f);
        }
    }
}
