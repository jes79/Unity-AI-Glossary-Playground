using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0402 Shader Graph
    // .shadergraph 파일은 에디터의 노드 창에서 만드는 에셋이라, 이 데모는
    //   (1) 노드 레시피를 단계별로 화면에 보여 주고
    //   (2) 같은 계산을 손으로 쓴 HLSL 셰이더(Glossary/Shader/GraphEquivalent)로 단계마다 실제 결과를 보여 줌
    // _Step 값으로 "레시피의 몇 단계까지 연결한 상태인지"를 셰이더에 알려 줌
    public class ShaderGraphRecipeDemo : MonoBehaviour
    {
        [SerializeField] private Renderer target;
        [SerializeField] private TextMeshProUGUI recipeText;
        [SerializeField, Range(1, 4)] private int step = 1;

        private Material mat;

        // 단계별: 제목, 노드 연결, 대응 HLSL
        private static readonly string[] Titles =
        {
            "1. Color → Base Color",
            "2. Time → Sine → Remap → Lerp",
            "3. Fresnel Effect → Emission",
            "4. Tiling And Offset → Sample Texture 2D",
        };

        private static readonly string[] Nodes =
        {
            "Blackboard [+] → Color 속성 'ColorA' 추가\n→ 그래프로 끌어다 놓고 Fragment 의 <b>Base Color</b> 에 연결 → [Save Asset]",
            "Color 'ColorB', Float 'PulseSpeed' 추가\n<b>Time</b>(Time) → <b>Multiply</b>(PulseSpeed) → <b>Sine</b> → <b>Remap</b>(In -1~1, Out 0~1)\n→ <b>Lerp</b>(A=ColorA, B=ColorB, T) → Base Color",
            "Float 'FresnelPower', Color 'RimColor'(Mode: HDR) 추가\n<b>Fresnel Effect</b>(Power=FresnelPower) → <b>Multiply</b>(RimColor)\n→ Fragment 의 <b>Emission</b> 에 연결",
            "Texture2D 'Noise', Vector2 'ScrollSpeed' 추가\n<b>Time</b> × ScrollSpeed → <b>Tiling And Offset</b>(Offset) → <b>Sample Texture 2D</b>(UV)\n→ R × Fresnel → <b>Add</b> → Emission",
        };

        private static readonly string[] Hlsl =
        {
            "baseColor = _ColorA;",
            "t = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;\nbaseColor = lerp(_ColorA, _ColorB, t);",
            "fresnel = pow(1 - saturate(dot(N, V)), _FresnelPower);\nemission = fresnel * _RimColor;",
            "uv = uv * tiling + _Time.y * _ScrollSpeed;\nnoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler, uv).r;\nemission = _RimColor * (fresnel + noise * noise * 0.6);",
        };

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(target.sharedMaterial, target);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.RightArrow) || DemoInput.KeyDown(Key.N)) NextStep();
            if (DemoInput.KeyDown(Key.LeftArrow) || DemoInput.KeyDown(Key.B)) PrevStep();
        }

        public void NextStep() { step = Mathf.Min(4, step + 1); Apply(); }
        public void PrevStep() { step = Mathf.Max(1, step - 1); Apply(); }

        private void Apply()
        {
            if (mat != null) mat.SetFloat("_Step", step);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<color=#FFD966><b>준비</b></color>  Project 창 우클릭 > Create > Shader Graph > URP > <b>Lit Shader Graph</b>");
            sb.AppendLine("→ 더블클릭해서 Shader Graph 창 열기 → 그래프 우클릭 > Create > Material\n");
            for (int i = 0; i < Titles.Length; i++)
            {
                bool done = i < step;
                bool current = i == step - 1;
                string color = current ? "#7FB2FF" : done ? "#9BE89B" : "#6A6F7A";
                sb.AppendLine($"<color={color}><b>{(done ? "■" : "□")} {Titles[i]}</b></color>");
                if (current)
                {
                    sb.AppendLine(Nodes[i]);
                    sb.AppendLine($"<color=#C8A2FF>HLSL 대응</color>\n<color=#D8D8D8>{Hlsl[i]}</color>");
                }
            }
            if (recipeText != null) recipeText.text = sb.ToString();

            GlossaryInfoPanel.Status = $"레시피 {step}/4 단계까지 연결된 결과를 표시 중\n←/→ (또는 B/N) 로 단계 이동";
        }
    }
}
