using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0404 Albedo(Base Map) / Normal Map
    // 왼쪽 벽: Albedo 만 → 벽돌 "그림"이라 빛이 움직여도 평평함
    // 오른쪽 벽: Albedo + Normal Map → 픽셀마다 법선이 달라 빛이 움직이면 줄눈 요철에 음영이 생김
    // Normal Map 은 실제 모양(폴리곤)은 그대로 두고 "빛 계산용 방향"만 바꾸는 텍스처
    public class NormalMapDemo : MonoBehaviour
    {
        [SerializeField] private Renderer normalWall;
        [SerializeField] private Texture albedo;
        [SerializeField] private Light orbitLight;
        [SerializeField] private Vector3 orbitCenter = new Vector3(0f, 1.6f, -1.2f);
        [SerializeField] private float orbitRadius = 2.2f;

        [Header("Normal Map")]
        [SerializeField, Range(0f, 2f)] private float bumpScale = 1f;
        [SerializeField] private bool useNormalMap = true;
        [SerializeField] private bool useAlbedo = true;
        [SerializeField] private float orbitSpeed = 1f;

        private Material mat;
        private float angle;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(normalWall.sharedMaterial, normalWall);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.N)) ToggleNormalMap();
            if (DemoInput.KeyDown(Key.A)) ToggleAlbedo();

            // 점광원이 벽 앞을 원을 그리며 돌아서 빛 방향이 계속 바뀜
            angle += orbitSpeed * Time.deltaTime;
            if (orbitLight != null)
                orbitLight.transform.position = orbitCenter + new Vector3(Mathf.Cos(angle) * orbitRadius * 1.6f, Mathf.Sin(angle) * orbitRadius * 0.6f, 0f);
        }

        public void SetBumpScale(float value) { bumpScale = value; Apply(); }
        public void SetOrbitSpeed(float value) => orbitSpeed = value;
        public void ToggleNormalMap() { useNormalMap = !useNormalMap; Apply(); }
        public void ToggleAlbedo() { useAlbedo = !useAlbedo; Apply(); }

        private void Apply()
        {
            if (mat == null) return;
            // _BumpScale 0 = 법선을 전혀 안 꺾음 (평평) → Normal Map 끈 것과 같음
            mat.SetFloat("_BumpScale", useNormalMap ? bumpScale : 0f);
            mat.SetTexture("_BaseMap", useAlbedo ? albedo : Texture2D.whiteTexture);
            GlossaryInfoPanel.Status =
                $"오른쪽 벽  Albedo {ShaderDemoUtil.OnOff(useAlbedo)}   Normal Map {ShaderDemoUtil.OnOff(useNormalMap)}   Bump Scale {bumpScale:0.00}\n" +
                "Albedo 를 끄면 흰 벽에 요철 음영만 남는 것을 확인";
        }
    }
}
