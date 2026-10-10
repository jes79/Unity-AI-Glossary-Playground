using System;
using System.Collections;
using UnityEngine;

namespace GlossaryDemo
{
    // DOTween 없이 쓰는 최소 이징 함수 모음 (t: 0~1)
    public static class Ease
    {
        public static float Linear(float t) => t;
        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
        public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;

        public static float OutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public static float OutElastic(float t)
        {
            if (t <= 0f || t >= 1f) return t;
            const float c4 = 2f * Mathf.PI / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        public static float OutBounce(float t)
        {
            const float n1 = 7.5625f, d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }

        // 코루틴용: duration 동안 eased t(0~1)를 onUpdate 로 넘김
        // unscaled = true 면 Time.timeScale 영향을 받지 않음 (일시정지·히트스톱 중 UI 연출)
        public static IEnumerator Run(float duration, Func<float, float> ease, Action<float> onUpdate, bool unscaled = false)
        {
            float time = 0f;
            while (time < duration)
            {
                time += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                onUpdate(ease(Mathf.Clamp01(time / duration)));
                yield return null;
            }
            onUpdate(ease(1f));
        }
    }
}
