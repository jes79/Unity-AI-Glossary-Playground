using UnityEngine;

namespace GlossaryDemo.Effects
{
    // 작은 카메라 쉐이크. 매 프레임 "지난 프레임에 더한 흔들림"을 빼고 새 흔들림을 더하므로
    // 카메라를 움직이는 다른 스크립트와 같이 써도 원래 위치가 틀어지지 않음
    // 히트스톱(timeScale = 0) 중에도 흔들리도록 unscaledDeltaTime 사용
    public class EffectsCameraShake : MonoBehaviour
    {
        private Vector3 offset;
        private float timer, duration, amplitude;

        public void Shake(float shakeDuration, float shakeAmplitude)
        {
            duration = Mathf.Max(0.01f, shakeDuration);
            timer = duration;
            amplitude = shakeAmplitude;
        }

        private void LateUpdate()
        {
            transform.localPosition -= offset;
            offset = Vector3.zero;
            if (timer <= 0f) return;

            timer -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(timer / duration); // 시간이 지날수록 약해짐
            offset = Random.insideUnitSphere * amplitude * k;
            transform.localPosition += offset;
        }

        private void OnDisable()
        {
            transform.localPosition -= offset;
            offset = Vector3.zero;
        }
    }
}
