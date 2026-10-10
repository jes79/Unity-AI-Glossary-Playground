using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Effects
{
    // Volume / Post Processing 기본
    // Global Volume(어디서나 적용) + Volume Profile(어떤 효과를 얼마나) + 카메라의 Post Processing 체크
    // 스크립트에서는 volume.profile.TryGet(out Bloom b) 로 Override 를 꺼내 값을 바꿈
    public class VolumeBasicsDemo : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private Camera targetCamera;

        private Bloom bloom;
        private ColorAdjustments color;
        private bool volumeOn = true;

        private void Start()
        {
            // volume.profile: 실행 중 복사본(에셋 원본을 건드리지 않음). sharedProfile 은 원본
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out color);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.V)) ToggleVolume();
            if (DemoInput.KeyDown(Key.B)) ToggleBloom();
            if (DemoInput.KeyDown(Key.C)) ToggleColor();
            if (DemoInput.KeyDown(Key.P)) ToggleCameraPP();

            var camData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            GlossaryInfoPanel.Status = $"V Global Volume {FxUtil.OnOff(volumeOn)}   P 카메라 Post Processing {FxUtil.OnOff(camData.renderPostProcessing)}\n" +
                                       $"B Bloom {FxUtil.OnOff(bloom.active)} (Intensity {bloom.intensity.value:0.0})   " +
                                       $"C Color Adjustments {FxUtil.OnOff(color.active)} (Saturation {color.saturation.value:0}, Exposure {color.postExposure.value:0.0})";
        }

        public void ToggleVolume() { volumeOn = !volumeOn; volume.weight = volumeOn ? 1f : 0f; }
        public void ToggleBloom() => bloom.active = !bloom.active;
        public void ToggleColor() => color.active = !color.active;

        // 카메라의 Post Processing 체크를 끄면 Volume 이 있어도 아무 효과도 안 보임 (흔한 실수)
        public void ToggleCameraPP()
        {
            var data = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = !data.renderPostProcessing;
        }

        public void SetBloomIntensity(float v) => bloom.intensity.value = v;
        public void SetSaturation(float v) => color.saturation.value = v;
        public void SetExposure(float v) => color.postExposure.value = v;
    }
}
