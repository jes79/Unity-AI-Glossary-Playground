using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.AnimationDemo
{
    // Animator 의 현재 상태(파랑)와 전환 중인 다음 상태(주황)를 상자 색으로 보여 주는 UI
    // Animator 창을 열지 않고도 상태 머신이 어떻게 움직이는지 게임 화면에서 확인하기 위함
    public class AnimatorStateDisplay : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private int layer;
        [SerializeField] private string[] stateNames;
        [SerializeField] private Image[] boxes;

        private static readonly Color Idle = new Color32(45, 49, 61, 255);
        private static readonly Color Current = new Color32(88, 140, 255, 255);
        private static readonly Color Next = new Color32(255, 160, 60, 255);

        public string CurrentStateName { get; private set; } = "";

        private void Update()
        {
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;
            var cur = animator.GetCurrentAnimatorStateInfo(layer);
            var next = animator.GetNextAnimatorStateInfo(layer);
            bool inTransition = animator.IsInTransition(layer);
            for (int i = 0; i < stateNames.Length && i < boxes.Length; i++)
            {
                Color c = Idle;
                if (cur.IsName(stateNames[i]))
                {
                    c = Current;
                    CurrentStateName = stateNames[i];
                }
                else if (inTransition && next.IsName(stateNames[i])) c = Next;
                boxes[i].color = c;
            }
        }
    }
}
