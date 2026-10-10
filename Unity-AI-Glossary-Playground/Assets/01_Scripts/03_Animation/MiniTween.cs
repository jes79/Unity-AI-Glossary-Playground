using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.AnimationDemo
{
    // DOTween 없이 쓰는 아주 작은 트윈 러너
    // - 매 프레임 진행도 t(0~1)를 이징 함수에 통과시켜 값을 보간
    // - 같은 대상의 트윈을 새로 걸면 이전 것을 정지(Kill) → 연타해도 크기가 누적되지 않음 (DOTween 의 DOKill 과 같은 역할)
    // DOTween 대응: PunchScale ↔ transform.DOPunchScale(...), ScaleTo ↔ DOScale, MoveTo ↔ DOMove/DOAnchorPos, ColorTo ↔ DOColor
    public class MiniTween : MonoBehaviour
    {
        private class Tween
        {
            public object target;
            public string key;
            public float duration, time;
            public bool unscaled;
            public Func<float, float> ease;
            public Action<float> apply;
            public Action onComplete;
        }

        private static MiniTween runner;
        private readonly List<Tween> tweens = new List<Tween>();

        public static int ActiveCount => runner != null ? runner.tweens.Count : 0;

        private static MiniTween Runner
        {
            get
            {
                if (runner == null) runner = new GameObject("[MiniTween]").AddComponent<MiniTween>();
                return runner;
            }
        }

        private void Update()
        {
            for (int i = tweens.Count - 1; i >= 0; i--)
            {
                var tw = tweens[i];
                if (tw.target is UnityEngine.Object o && o == null) { tweens.RemoveAt(i); continue; }
                tw.time += tw.unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(tw.time / tw.duration);
                tw.apply(tw.ease(t));
                if (t >= 1f)
                {
                    tweens.RemoveAt(i);
                    tw.onComplete?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            if (runner == this) runner = null;
        }

        // 공통 진입점: target+key 가 같은 기존 트윈은 지움
        public static void To(object target, string key, float duration, Func<float, float> ease, Action<float> apply, Action onComplete = null, bool unscaled = false)
        {
            Kill(target, key);
            Runner.tweens.Add(new Tween { target = target, key = key, duration = Mathf.Max(0.0001f, duration), ease = ease ?? Ease.Linear, apply = apply, onComplete = onComplete, unscaled = unscaled });
        }

        public static void Kill(object target, string key = null)
        {
            if (runner == null) return;
            runner.tweens.RemoveAll(t => t.target == target && (key == null || t.key == key));
        }

        // ── DOTween 스타일 단축 함수 ──

        // 펀치: 원래 크기에서 punch 만큼 커졌다가 진동하며 원래 크기로 (DOPunchScale(punch, duration, vibrato, elasticity))
        public static void PunchScale(Transform t, Vector3 punch, float duration, int vibrato = 1, float elasticity = 0.5f)
        {
            Vector3 baseScale = GetBaseScale(t);
            To(t, "scale", duration, Ease.Linear, k =>
            {
                // vibrato 1 : sin(0~π) → 한 번 커졌다가 원래대로
                // vibrato N : 점점 줄어드는 진동 (elasticity 가 작을수록 반대 방향으로 덜 줄어듦)
                int n = Mathf.Max(1, vibrato);
                float wave = Mathf.Sin(k * Mathf.PI * (2 * n - 1));
                if (n > 1) wave *= 1f - k;
                if (wave < 0f) wave *= elasticity;
                t.localScale = baseScale + punch * wave;
            }, () => t.localScale = baseScale);
        }

        public static void ScaleTo(Transform t, Vector3 to, float duration, Func<float, float> ease = null)
        {
            Vector3 from = t.localScale;
            To(t, "scale", duration, ease ?? Ease.OutQuad, k => t.localScale = Vector3.LerpUnclamped(from, to, k));
        }

        public static void MoveTo(RectTransform t, Vector2 to, float duration, Func<float, float> ease = null)
        {
            Vector2 from = t.anchoredPosition;
            To(t, "move", duration, ease ?? Ease.OutQuad, k => t.anchoredPosition = Vector2.LerpUnclamped(from, to, k));
        }

        public static void ColorTo(Graphic g, Color to, float duration, Func<float, float> ease = null)
        {
            Color from = g.color;
            To(g, "color", duration, ease ?? Ease.Linear, k => g.color = Color.LerpUnclamped(from, to, k));
        }

        // 펀치가 진행 중일 때 다시 걸면 커진 크기를 기준으로 삼지 않도록 원래 크기를 기억
        private static readonly Dictionary<Transform, Vector3> baseScales = new Dictionary<Transform, Vector3>();

        private static Vector3 GetBaseScale(Transform t)
        {
            if (!baseScales.TryGetValue(t, out var s) || t == null)
            {
                s = t.localScale;
                baseScales[t] = s;
            }
            return s;
        }
    }
}
