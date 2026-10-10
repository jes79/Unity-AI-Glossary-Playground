using System.Collections;
using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 리스너 3: 카메라 — 죽으면 화면 흔들림
    public class CameraShakeListener : DeathListenerBase
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private float strength = 0.3f;

        protected override void OnPlayerDied() => StartCoroutine(Shake());

        private IEnumerator Shake()
        {
            Vector3 origin = cameraTransform.localPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                cameraTransform.localPosition = origin + (Vector3)Random.insideUnitCircle * strength * (1f - t / duration);
                yield return null;
            }
            cameraTransform.localPosition = origin;
        }
    }
}
