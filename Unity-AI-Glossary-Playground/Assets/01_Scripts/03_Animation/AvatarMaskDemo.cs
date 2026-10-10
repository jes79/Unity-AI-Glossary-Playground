using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // Animation Layer / Avatar Mask 데모
    // - Base Layer : Run (전신)
    // - UpperBody 레이어 : Shoot (전신 클립이지만 Avatar Mask 로 상체 본만 덮어씀)
    // 왼쪽 캐릭터는 마스크 있음 → 뛰면서 상체만 사격 / 오른쪽은 마스크 없음 → 사격 자세가 다리까지 덮어 써서 멈춰 선 채 미끄러짐
    public class AvatarMaskDemo : MonoBehaviour
    {
        [SerializeField] private Animator withMask;
        [SerializeField] private Animator withoutMask;
        [SerializeField] private int upperLayer = 1;
        [SerializeField] private float blendSpeed = 8f;

        private float weight;
        private bool toggled;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.F)) toggled = !toggled;
            bool shooting = toggled || DemoInput.LeftHeld || DemoInput.JumpHeld;

            // 레이어 Weight 를 0 ↔ 1 로 부드럽게 → 상체 레이어를 켜고 끔
            weight = Mathf.MoveTowards(weight, shooting ? 1f : 0f, blendSpeed * Time.deltaTime);
            withMask.SetLayerWeight(upperLayer, weight);
            withoutMask.SetLayerWeight(upperLayer, weight);

            GlossaryInfoPanel.Status = $"UpperBody 레이어 Weight = {weight:0.00}  ({(shooting ? "사격 중" : "달리기만")})\n" +
                                       "왼쪽: Avatar Mask(상체만)   오른쪽: 마스크 없음";
        }
    }
}
