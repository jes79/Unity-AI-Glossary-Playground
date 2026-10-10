using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // "횃불 주변만 밝은 2D 조명" 대체 구현
    // 이 프로젝트는 URP Universal(3D) Renderer 를 쓰므로 Light2D(URP 2D Renderer 전용)가 동작하지 않음.
    // 공유 파이프라인 에셋을 고치지 않고 같은 효과를 내는 고전적인 방법:
    //  1) Light 카메라가 "빛 스프라이트"(부드러운 원)만 Render Texture 에 더하기로 그림. 배경색 = 환경광(어두움)
    //  2) 화면을 덮는 Quad 가 그 텍스처를 장면 색에 곱함 → 빛이 없는 곳은 어둡고 횃불 주변만 밝음
    // URP 2D Renderer 의 Light2D(Global + Point) 와 결과가 비슷함 (설명 패널 참고)
    public class TorchLight2DFallback : MonoBehaviour
    {
        [SerializeField] private Camera mainCam;
        [SerializeField] private Camera lightCam;          // TransparentFX 레이어(빛 스프라이트)만 그림
        [SerializeField] private Renderer overlay;          // 화면 덮개 Quad (Multiply)
        [SerializeField] private Transform playerTorch;     // 마우스를 따라다니는 횃불
        [SerializeField] private GameObject torchTemplate;  // 클릭으로 설치할 횃불 원본
        [SerializeField] private Color ambient = new Color(0.08f, 0.08f, 0.16f);
        [SerializeField] private int maxPlacedTorches = 8;
        [SerializeField, Range(1, 4)] private int downSample = 2; // 라이트 맵 해상도 = 화면 / downSample

        private RenderTexture lightMap;
        private Material overlayMat;
        private readonly List<GameObject> placed = new List<GameObject>();
        private readonly List<Transform> glows = new List<Transform>();
        private bool lightingOn = true;
        private int lastW, lastH;

        private void Start()
        {
            overlayMat = overlay.material; // 인스턴스
            CollectGlows();
            lightCam.backgroundColor = ambient;
            Resize();
        }

        private void OnDestroy()
        {
            if (lightMap != null) lightMap.Release();
        }

        private void Update()
        {
            if (Screen.width != lastW || Screen.height != lastH) Resize();

            Vector2 mouse = DemoInput.MouseWorld2D(mainCam);
            playerTorch.position = new Vector3(mouse.x, mouse.y, 0f);

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (DemoInput.LeftClick && !overUI) PlaceTorch(mouse);
            if (DemoInput.KeyDown(Key.L)) ToggleLighting();
            if (DemoInput.KeyDown(Key.C)) ClearTorches();

            // 횃불 깜빡임: 빛 스프라이트 크기를 펄린 노이즈로 흔듦
            for (int i = 0; i < glows.Count; i++)
            {
                if (glows[i] == null) continue;
                float n = Mathf.PerlinNoise(Time.time * 3f, i * 7.1f);
                float s = Mathf.Lerp(0.9f, 1.1f, n);
                glows[i].localScale = new Vector3(s, s, 1f) * GlowBaseScale(glows[i]);
            }

            GlossaryInfoPanel.Status =
                $"2D 조명: {(lightingOn ? "<color=#8F8>ON</color>" : "<color=#F88>OFF (전체가 밝음)</color>")}   설치한 횃불 {placed.Count}/{maxPlacedTorches}\n" +
                $"환경광 #{ColorUtility.ToHtmlStringRGB(lightCam.backgroundColor)}   라이트 맵 {lastW / downSample}x{lastH / downSample}\n" +
                "방식: Render Texture 라이트 맵 x Multiply (Light2D 대체)";
        }

        private readonly Dictionary<Transform, float> baseScales = new Dictionary<Transform, float>();
        private float GlowBaseScale(Transform t) => baseScales.TryGetValue(t, out float s) ? s : 1f;

        private void CollectGlows()
        {
            glows.Clear();
            int fx = LayerMask.NameToLayer("TransparentFX");
            foreach (var sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                if (sr.gameObject.layer == fx) Register(sr.transform);
        }

        private void Register(Transform glow)
        {
            if (glows.Contains(glow)) return;
            glows.Add(glow);
            // 부모 스케일 기준 원래 크기 기억 (깜빡임은 이 값 기준으로 ±10%)
            baseScales[glow] = glow.localScale.x;
        }

        // 화면 크기에 맞춰 라이트 맵 RT 와 덮개 Quad 크기를 다시 맞춤
        private void Resize()
        {
            lastW = Mathf.Max(1, Screen.width);
            lastH = Mathf.Max(1, Screen.height);
            if (lightMap != null) { lightCam.targetTexture = null; lightMap.Release(); }
            // HDR 포맷: 빛이 겹치면 1 을 넘어 더 밝아질 수 있음
            lightMap = new RenderTexture(lastW / downSample, lastH / downSample, 0, RenderTextureFormat.DefaultHDR) { name = "LightMap2D" };
            lightCam.targetTexture = lightMap;
            overlayMat.SetTexture("_LightTex", lightMap);

            float h = mainCam.orthographicSize * 2f;
            overlay.transform.localScale = new Vector3(h * mainCam.aspect, h, 1f);
        }

        public void ToggleLighting()
        {
            lightingOn = !lightingOn;
            overlay.enabled = lightingOn;
            lightCam.enabled = lightingOn; // 끄면 라이트 맵 렌더링 비용도 없음
        }

        // 슬라이더(0~1): 환경광 밝기
        public void SetAmbient(float v)
        {
            v = Mathf.Clamp01(v);
            lightCam.backgroundColor = Color.Lerp(new Color(0.01f, 0.01f, 0.03f), new Color(0.35f, 0.35f, 0.5f), v);
        }

        public void PlaceTorch(Vector2 pos)
        {
            if (placed.Count >= maxPlacedTorches)
            {
                Destroy(placed[0]);
                placed.RemoveAt(0);
            }
            var t = Instantiate(torchTemplate, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            t.SetActive(true);
            placed.Add(t);
            foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>())
                if (sr.gameObject.layer == LayerMask.NameToLayer("TransparentFX")) Register(sr.transform);
        }

        public void ClearTorches()
        {
            foreach (var t in placed) Destroy(t);
            placed.Clear();
            glows.RemoveAll(g => g == null);
        }
    }
}
