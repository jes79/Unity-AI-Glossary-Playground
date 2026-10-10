using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // 폭발 = 5개 레이어를 같은 순간에 재생
    // 1 섬광(아주 짧고 큼)  2 화염 구(가산, 주황)  3 연기(알파, 오래 남음)  4 파편(중력+충돌)  5 충격파 링(바닥에 퍼짐)
    public class ExplosionDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] layers = new ParticleSystem[5];
        [SerializeField] private bool[] layerOn = { true, true, true, true, true };
        [SerializeField] private Light flashLight;
        [SerializeField] private EffectsCameraShake cameraShake;
        [SerializeField] private AudioSource boomSound;

        private static readonly string[] Names = { "섬광", "화염 구", "연기", "파편", "충격파 링" };
        private Camera cam;
        private float lightTimer;

        private void Start() => cam = Camera.main;

        private void Update()
        {
            for (int i = 0; i < 5; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) ToggleLayer(i);

            if (DemoInput.KeyDown(Key.Space)) ExplodeCenter();
            if (DemoInput.LeftClick && !FxUtil.PointerOverUI && FxUtil.MouseOnPlane(cam, 0f, out var p)) Explode(p);

            lightTimer -= Time.deltaTime;
            flashLight.intensity = Mathf.Max(0f, lightTimer / 0.3f) * 12f;

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 5; i++) sb.Append($"{i + 1} {Names[i]} {FxUtil.OnOff(layerOn[i])}   ");
            GlossaryInfoPanel.Status = sb.ToString();
        }

        public void ToggleLayer(int i) => layerOn[i] = !layerOn[i];
        public void ExplodeCenter() => Explode(new Vector3(1f, 0f, 0f));

        public void Explode(Vector3 groundPoint)
        {
            transform.position = groundPoint + Vector3.up * 0.8f;
            for (int i = 0; i < layers.Length; i++)
                if (layerOn[i]) FxUtil.Replay(layers[i]);

            if (layerOn[0])
            {
                lightTimer = 0.3f; // 섬광 레이어와 함께 주변을 비추는 순간 조명
                if (cameraShake != null) cameraShake.Shake(0.35f, 0.25f);
            }
            if (boomSound != null) boomSound.Play();
        }
    }
}
