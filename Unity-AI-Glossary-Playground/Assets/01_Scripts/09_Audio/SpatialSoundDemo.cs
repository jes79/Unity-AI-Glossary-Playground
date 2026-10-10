using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AudioDemo
{
    // 3D 사운드 데모: 모닥불 AudioSource 를 Spatial Blend 1(3D) + Linear Rolloff, Max Distance 10m 로 설정
    // → 플레이어(AudioListener)가 10m 밖으로 나가면 소리가 완전히 사라짐. 좌우 위치에 따라 왼쪽/오른쪽 스피커로 나뉨(패닝)
    // 비교: 2D(Spatial Blend 0)는 거리와 상관없이 같은 크기 / Logarithmic 은 10m 에서도 완전히 0 이 되지 않음
    public class SpatialSoundDemo : MonoBehaviour
    {
        [SerializeField] private AudioSource fireSource;
        [SerializeField] private Light fireLight;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 14f, -9f);
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float maxDistance = 10f;

        private bool is3D = true;
        private bool linear = true;

        private void Start() => Apply();

        private void Apply()
        {
            fireSource.spatialBlend = is3D ? 1f : 0f;              // 0 = 2D, 1 = 3D
            fireSource.rolloffMode = linear ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic;
            fireSource.minDistance = 1f;                            // 이 거리 안에서는 최대 볼륨
            fireSource.maxDistance = maxDistance;                   // Linear: 여기서 0 이 됨
        }

        private void Update()
        {
            Vector2 m = DemoInput.Move;
            transform.position += new Vector3(m.x, 0f, m.y) * (moveSpeed * Time.deltaTime);
            if (m.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(m.x, 0f, m.y));
            if (cameraTransform != null)
            {
                cameraTransform.position = transform.position + cameraOffset;
                cameraTransform.LookAt(transform.position);
            }

            if (DemoInput.KeyDown(Key.Digit1)) { is3D = !is3D; Apply(); }
            if (DemoInput.KeyDown(Key.Digit2)) { linear = !linear; Apply(); }

            if (fireLight != null) fireLight.intensity = 6f + Mathf.PerlinNoise(Time.time * 8f, 0f) * 4f;

            float d = Vector3.Distance(transform.position, fireSource.transform.position);
            float vol = Attenuation(d);
            GlossaryInfoPanel.Status = $"모닥불까지 거리 {d:0.0} m   예상 볼륨 {vol * 100f:0}%\n" +
                                       $"Spatial Blend {(is3D ? "1 (3D)" : "0 (2D)")}   Rolloff {(linear ? "Linear" : "Logarithmic")}   Max {maxDistance:0} m";
        }

        // 거리 감쇠 공식 (Unity 기본 커브와 같은 계산) — 화면 표시용
        private float Attenuation(float d)
        {
            if (!is3D) return 1f;
            float min = fireSource.minDistance, max = fireSource.maxDistance;
            if (d <= min) return 1f;
            if (linear) return Mathf.Clamp01(1f - (d - min) / (max - min));
            return Mathf.Clamp01(min / d); // Logarithmic: 1/거리 → 멀어도 완전히 0 이 되지 않음
        }
    }
}
