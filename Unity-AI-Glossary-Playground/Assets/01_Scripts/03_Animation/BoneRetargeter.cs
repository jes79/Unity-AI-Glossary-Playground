using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // Humanoid 리타게팅의 핵심 아이디어를 코드로 흉내 낸 대체 구현
    // - Humanoid Avatar 는 "이 본이 Hips / LeftUpperArm …" 이라는 공통 이름표(HumanBodyBones)를 붙여 두고
    //   애니메이션을 본 이름·길이와 상관없는 '근육 값'으로 저장해서 다른 체형에 다시 입힘
    // - 여기서는 공통 이름표 → (원본 본, 대상 본) 매핑 표를 두고, 매 프레임 원본의 회전 변화량을 대상 본에 복사
    //   엉덩이 높이 이동은 다리 길이 비율로 스케일 → 짧은 다리 캐릭터도 땅에 발이 붙음
    public class BoneRetargeter : MonoBehaviour
    {
        [Serializable]
        public struct BoneMap
        {
            public string humanName;   // 공통 이름표 (HumanBodyBones 역할)
            public Transform source;
            public Transform target;
        }

        [SerializeField] private bool retarget = true;
        [SerializeField] private BoneMap[] bones;
        [SerializeField] private Transform sourceHips;
        [SerializeField] private Transform targetHips;
        [SerializeField] private float sourceLegLength = 1f;
        [SerializeField] private float targetLegLength = 0.6f;
        [SerializeField] private Animator sourceAnimator;
        [SerializeField] private string[] clipStates = { "Dance", "Run" };

        private Quaternion[] sourceRest, targetRest;
        private Vector3 sourceHipsRest, targetHipsRest;
        private int clipIndex;

        private void Awake()
        {
            // 시작 자세(T포즈 역할)를 기억해 두고, 여기서 얼마나 돌았는지(변화량)만 옮김
            sourceRest = new Quaternion[bones.Length];
            targetRest = new Quaternion[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                sourceRest[i] = bones[i].source.localRotation;
                targetRest[i] = bones[i].target.localRotation;
            }
            sourceHipsRest = sourceHips.localPosition;
            targetHipsRest = targetHips.localPosition;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.M))
            {
                retarget = !retarget;
                if (!retarget) ResetTarget();
            }
            if (DemoInput.KeyDown(Key.Digit1)) Play(0);
            if (DemoInput.KeyDown(Key.Digit2)) Play(1);

            GlossaryInfoPanel.Status = $"리타게팅: {(retarget ? "ON (매핑 표로 본 회전 복사)" : "OFF (같은 클립을 경로 그대로 재생 → 본 이름이 달라 안 움직임)")}\n" +
                                       $"재생 중: {clipStates[clipIndex]}   다리 길이 비율 {targetLegLength / sourceLegLength:0.00}";
        }

        private void Play(int i)
        {
            clipIndex = Mathf.Clamp(i, 0, clipStates.Length - 1);
            if (sourceAnimator != null) sourceAnimator.CrossFade(clipStates[clipIndex], 0.2f);
        }

        // Animator 가 원본 포즈를 계산한 뒤(LateUpdate) 복사
        private void LateUpdate()
        {
            if (!retarget) return;
            for (int i = 0; i < bones.Length; i++)
            {
                Quaternion delta = Quaternion.Inverse(sourceRest[i]) * bones[i].source.localRotation;
                bones[i].target.localRotation = targetRest[i] * delta;
            }
            // 엉덩이 이동량은 다리 길이 비율로 줄이거나 늘림 (Humanoid 의 Body 위치 정규화와 같은 역할)
            float ratio = targetLegLength / Mathf.Max(0.01f, sourceLegLength);
            targetHips.localPosition = targetHipsRest + (sourceHips.localPosition - sourceHipsRest) * ratio;
        }

        private void ResetTarget()
        {
            for (int i = 0; i < bones.Length; i++) bones[i].target.localRotation = targetRest[i];
            targetHips.localPosition = targetHipsRest;
        }
    }
}
