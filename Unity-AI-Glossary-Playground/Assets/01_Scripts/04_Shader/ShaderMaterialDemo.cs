using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0401 Shader / Material
    // - 셰이더(그리는 규칙) 1개로 색만 다른 머티리얼 3개를 만들고, 머티리얼마다 물체 2개씩 씀
    // - 머티리얼 값을 바꾸면 그 머티리얼을 쓰는 물체 2개가 "같이" 바뀜 (머티리얼 = 공유되는 값 묶음)
    // - 셰이더를 바꾸면 3개 머티리얼 모두 그리는 방식이 바뀜 (값 _BaseColor 는 그대로 유지)
    public class ShaderMaterialDemo : MonoBehaviour
    {
        [Header("같은 셰이더로 만든 머티리얼 3개 (빨강/초록/파랑)")]
        [SerializeField] private Material[] materials;
        [SerializeField] private Renderer[] renderers;

        [Header("셰이더 교체 비교")]
        [SerializeField] private Shader litShader;
        [SerializeField] private Shader unlitShader;

        private Material[] runtimeMats;
        private int selected;
        private bool useUnlit;
        private float bounce;

        private void Start()
        {
            // 에셋을 직접 고치지 않도록 실행용 복사본으로 교체 (공유 관계는 유지됨)
            runtimeMats = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
                runtimeMats[i] = ShaderDemoUtil.CloneShared(materials[i], renderers);
            UpdateStatus();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) SelectMaterial(0);
            if (DemoInput.KeyDown(Key.Digit2)) SelectMaterial(1);
            if (DemoInput.KeyDown(Key.Digit3)) SelectMaterial(2);
            if (DemoInput.KeyDown(Key.S)) ToggleShader();

            // 선택한 머티리얼을 쓰는 물체들을 살짝 튀게 해서 "누가 이 머티리얼을 공유하는지" 보여 줌
            bounce = Mathf.Max(0f, bounce - Time.deltaTime * 3f);
            foreach (var r in renderers)
            {
                bool isSelected = r.sharedMaterial == runtimeMats[selected];
                float s = 1f + (isSelected ? Mathf.Sin(bounce * Mathf.PI) * 0.25f : 0f);
                r.transform.localScale = Vector3.one * s;
            }
        }

        public void SelectMaterial(int index)
        {
            selected = Mathf.Clamp(index, 0, runtimeMats.Length - 1);
            bounce = 1f;
            UpdateStatus();
        }

        // 슬라이더: 선택한 머티리얼의 색(Hue)만 바꿈 → 같은 머티리얼을 쓰는 2개가 같이 바뀜
        public void SetHue(float hue)
        {
            if (runtimeMats == null) return;
            var mat = runtimeMats[selected];
            Color.RGBToHSV(mat.GetColor("_BaseColor"), out _, out float s, out float v);
            mat.SetColor("_BaseColor", Color.HSVToRGB(hue, Mathf.Max(s, 0.6f), Mathf.Max(v, 0.8f)));
            UpdateStatus();
        }

        // 셰이더 교체: 3개 머티리얼 전부 다른 규칙(조명 有/無)으로 그려짐
        public void ToggleShader()
        {
            useUnlit = !useUnlit;
            foreach (var m in runtimeMats) m.shader = useUnlit ? unlitShader : litShader;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (runtimeMats == null) return;
            var c = runtimeMats[selected].GetColor("_BaseColor");
            GlossaryInfoPanel.Status =
                $"셰이더: <b>{runtimeMats[0].shader.name}</b> (3개 머티리얼이 공유)\n" +
                $"선택 머티리얼: <b>{materials[selected].name}</b>  _BaseColor = #{ColorUtility.ToHtmlStringRGB(c)}\n" +
                "→ 이 머티리얼을 쓰는 물체 2개가 함께 바뀜";
        }
    }
}
