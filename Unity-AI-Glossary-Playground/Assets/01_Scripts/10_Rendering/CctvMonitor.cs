using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // CCTV: 보안 카메라가 찍은 화면을 Render Texture 에 그리고, 그 텍스처를 모니터 머티리얼에 붙임
    // Camera.targetTexture = renderTexture  → 이 카메라는 화면 대신 텍스처에 그림
    // 모니터 머티리얼 _BaseMap = renderTexture → 텍스처가 모니터 표면에 보임
    public class CctvMonitor : MonoBehaviour
    {
        [SerializeField] private Camera[] cctvCameras;       // 채널 (C 키로 전환)
        [SerializeField] private RenderTexture renderTexture; // 빌더가 만든 에셋 (512x288)
        [SerializeField] private Renderer monitorScreen;
        [SerializeField] private float sweepAngle = 35f;     // 좌우로 고개를 돌리는 각도
        [SerializeField] private float sweepSpeed = 0.4f;
        [SerializeField] private Transform intruder;           // 감시 구역을 돌아다니는 "침입자"

        private int channel;
        private Quaternion[] baseRotations;
        private RenderTexture lowRes;  // R 키: 저해상도(128x72) 텍스처로 교체해 차이 보기
        private bool useLow;

        private void Start()
        {
            baseRotations = new Quaternion[cctvCameras.Length];
            for (int i = 0; i < cctvCameras.Length; i++) baseRotations[i] = cctvCameras[i].transform.rotation;
            lowRes = new RenderTexture(128, 72, 16) { name = "CCTV_Low", filterMode = FilterMode.Point };
            SetChannel(0);
        }

        private void OnDestroy()
        {
            if (lowRes != null) lowRes.Release();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.C)) NextChannel();
            if (DemoInput.KeyDown(Key.R)) ToggleResolution();

            // 감시 카메라처럼 좌우로 천천히 회전
            for (int i = 0; i < cctvCameras.Length; i++)
            {
                float yaw = Mathf.Sin(Time.time * sweepSpeed + i) * sweepAngle;
                cctvCameras[i].transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * baseRotations[i];
            }

            if (intruder != null)
                intruder.position = new Vector3(Mathf.Sin(Time.time * 0.5f) * 6f, 1f, 14f + Mathf.Sin(Time.time * 0.8f) * 3f);

            var rt = Current;
            GlossaryInfoPanel.Status = $"채널 CAM {channel + 1}/{cctvCameras.Length}  ({cctvCameras[channel].name})\n" +
                                       $"Render Texture: {rt.width}x{rt.height}  {(useLow ? "저해상도(Point 필터)" : "기본")}\n" +
                                       "켜진 CCTV 카메라만 렌더링 → 카메라 수만큼 그리기 비용이 듦";
        }

        private RenderTexture Current => useLow ? lowRes : renderTexture;

        public void NextChannel() => SetChannel((channel + 1) % cctvCameras.Length);

        public void ToggleResolution()
        {
            useLow = !useLow;
            SetChannel(channel);
        }

        private void SetChannel(int c)
        {
            channel = c;
            for (int i = 0; i < cctvCameras.Length; i++)
            {
                bool on = i == channel;
                cctvCameras[i].targetTexture = on ? Current : null;
                cctvCameras[i].enabled = on; // 보이지 않는 채널은 꺼서 비용 절약
            }
            // 모니터 머티리얼 인스턴스에 텍스처 연결 (URP Unlit 의 메인 텍스처 = _BaseMap)
            monitorScreen.material.SetTexture("_BaseMap", Current);
        }
    }
}
