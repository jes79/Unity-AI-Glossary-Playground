using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.ShaderDemo
{
    // 0420 그레이스케일 / 색상 반전 + 얼음(Freeze) 틴트
    // - 얼음: 클릭한 캐릭터만(MaterialPropertyBlock) 파란색 계열로 틴트 + 움직임 정지, freezeDuration 뒤 녹음
    // - G / I: 셰이더의 흑백·반전 값을 모든 캐릭터에 (공유 머티리얼)
    // - S: 비교용으로 화면 전체 흑백 (포스트 프로세싱 Color Adjustments 의 Saturation -100)
    public class ColorEffectsDemo : MonoBehaviour
    {
        private static readonly int FreezeId = Shader.PropertyToID("_Freeze");

        [SerializeField] private Transform[] characters;
        [SerializeField] private Volume screenVolume;

        [Header("얼음")]
        [SerializeField] private float freezeDuration = 3f;
        [SerializeField] private float transitionTime = 0.25f;

        private Material mat;
        private MaterialPropertyBlock block;
        private Renderer[][] renderers;
        private ShaderDemoSpinner[] spinners;
        private float[] freezeTimer;
        private float[] freezeAmount;
        private bool grayscale, invert;
        private float grayValue, invertValue;
        private ColorAdjustments screenAdjust;
        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
            block = new MaterialPropertyBlock();
            renderers = new Renderer[characters.Length][];
            spinners = new ShaderDemoSpinner[characters.Length];
            freezeTimer = new float[characters.Length];
            freezeAmount = new float[characters.Length];

            var all = new System.Collections.Generic.List<Renderer>();
            for (int i = 0; i < characters.Length; i++)
            {
                renderers[i] = characters[i].GetComponentsInChildren<Renderer>();
                spinners[i] = characters[i].GetComponent<ShaderDemoSpinner>();
                all.AddRange(renderers[i]);
            }
            mat = ShaderDemoUtil.CloneShared(renderers[0][0].sharedMaterial, all.ToArray());

            if (screenVolume != null && screenVolume.profile.TryGet(out screenAdjust)) screenAdjust.active = false;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.G)) ToggleGrayscale();
            if (DemoInput.KeyDown(Key.I)) ToggleInvert();
            if (DemoInput.KeyDown(Key.F)) FreezeAll();
            if (DemoInput.KeyDown(Key.S)) ToggleScreenGrayscale();

            if (DemoInput.LeftClick && Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f))
                for (int i = 0; i < characters.Length; i++)
                    if (hit.transform.IsChildOf(characters[i])) Freeze(i);

            // 흑백/반전은 0↔1 사이를 부드럽게 전환
            float step = Time.deltaTime / transitionTime;
            grayValue = Mathf.MoveTowards(grayValue, grayscale ? 1f : 0f, step);
            invertValue = Mathf.MoveTowards(invertValue, invert ? 1f : 0f, step);
            mat.SetFloat("_Grayscale", grayValue);
            mat.SetFloat("_Invert", invertValue);

            int frozenCount = 0;
            for (int i = 0; i < characters.Length; i++)
            {
                freezeTimer[i] -= Time.deltaTime;
                bool frozen = freezeTimer[i] > 0f;
                if (frozen) frozenCount++;
                freezeAmount[i] = Mathf.MoveTowards(freezeAmount[i], frozen ? 1f : 0f, step);
                if (spinners[i] != null) spinners[i].Paused = frozen; // 얼면 움직임도 멈춤
                foreach (var r in renderers[i])
                {
                    r.GetPropertyBlock(block);
                    block.SetFloat(FreezeId, freezeAmount[i]);
                    r.SetPropertyBlock(block);
                }
            }

            GlossaryInfoPanel.Status =
                $"흑백 {ShaderDemoUtil.OnOff(grayscale)}   반전 {ShaderDemoUtil.OnOff(invert)}   화면 전체 흑백(S) {ShaderDemoUtil.OnOff(screenAdjust != null && screenAdjust.active)}\n" +
                $"얼어 있는 캐릭터 {frozenCount}명 ({freezeDuration:0}초 뒤 녹음)";
        }

        public void ToggleGrayscale() => grayscale = !grayscale;
        public void ToggleInvert() => invert = !invert;
        public void FreezeAll() { for (int i = 0; i < characters.Length; i++) Freeze(i); }
        public void Freeze(int index) => freezeTimer[index] = freezeDuration;

        public void ToggleScreenGrayscale()
        {
            if (screenAdjust != null) screenAdjust.active = !screenAdjust.active;
        }
    }
}
