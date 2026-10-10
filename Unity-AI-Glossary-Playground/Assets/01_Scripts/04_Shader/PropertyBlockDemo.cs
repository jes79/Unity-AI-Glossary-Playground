using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0409 MaterialPropertyBlock
    // 적 100마리가 머티리얼 "하나"를 공유한 채로, 렌더러마다 MaterialPropertyBlock 으로 _BaseColor 만 다르게 넣음
    //   → 머티리얼 복제가 0개 (메모리·에셋 그대로)
    // 비교 모드(M): renderer.material 로 색을 바꾸면 접근하는 순간 머티리얼이 렌더러마다 복제됨 → 100개 생성
    // 참고: URP 에서 MPB 를 쓴 렌더러는 SRP Batcher 대상에서 빠짐. 색만 다르게 할 땐 GPU Instancing 과 함께 쓰거나,
    //       많은 수라면 셰이더 쪽에서 인스턴스 ID 로 색을 고르는 방법도 있음
    public class PropertyBlockDemo : MonoBehaviour
    {
        public enum Mode { PropertyBlock, MaterialCopy }

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer[] enemies;
        [SerializeField] private Material sharedMaterial;
        [SerializeField] private Mode mode = Mode.PropertyBlock;
        [SerializeField] private bool animateColors = true;

        private MaterialPropertyBlock block;
        private Color[] colors;
        private readonly List<Material> copies = new List<Material>();
        private int setCallsThisFrame;

        private void Start()
        {
            block = new MaterialPropertyBlock();
            colors = new Color[enemies.Length];
            Randomize();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.M)) ToggleMode();
            if (DemoInput.KeyDown(Key.R)) Randomize();
            if (DemoInput.KeyDown(Key.Space)) ToggleAnimate();

            setCallsThisFrame = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                Color c = colors[i];
                if (animateColors)
                {
                    // 위치에 따라 밝기가 물결치게 → 매 프레임 100개의 값을 바꿔도 머티리얼은 그대로 1개
                    var p = enemies[i].transform.position;
                    float wave = 0.65f + 0.35f * Mathf.Sin(Time.time * 3f + p.x * 0.8f + p.z * 0.5f);
                    c *= wave;
                    c.a = 1f;
                }
                ApplyColor(i, c);
            }

            var unique = new HashSet<Material>();
            foreach (var r in enemies) unique.Add(r.sharedMaterial);
            GlossaryInfoPanel.Status =
                $"방식: <b>{(mode == Mode.PropertyBlock ? "MaterialPropertyBlock" : "renderer.material (복제)")}</b>\n" +
                $"사용 중인 머티리얼 수: <b>{unique.Count}</b>  (적 {enemies.Length}마리)   만들어진 복제본: {copies.Count}\n" +
                $"이번 프레임 색 변경 {setCallsThisFrame}회   색 물결 {ShaderDemoUtil.OnOff(animateColors)}";
        }

        private void ApplyColor(int i, Color c)
        {
            var r = enemies[i];
            if (mode == Mode.PropertyBlock)
            {
                // 핵심: 렌더러에 "덮어쓸 값 묶음"만 전달. 머티리얼 에셋은 공유 그대로
                r.GetPropertyBlock(block);
                block.SetColor(BaseColorId, c);
                r.SetPropertyBlock(block);
            }
            else
            {
                // renderer.material 을 처음 읽는 순간 이 렌더러 전용 복제본이 생김
                bool wasShared = r.sharedMaterial == sharedMaterial;
                r.material.SetColor(BaseColorId, c);
                if (wasShared) copies.Add(r.sharedMaterial);
            }
            setCallsThisFrame++;
        }

        public void Randomize()
        {
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.HSVToRGB(Random.value, Random.Range(0.55f, 0.9f), Random.Range(0.75f, 1f));
        }

        public void ToggleAnimate() => animateColors = !animateColors;

        public void ToggleMode()
        {
            mode = mode == Mode.PropertyBlock ? Mode.MaterialCopy : Mode.PropertyBlock;
            if (mode == Mode.PropertyBlock)
            {
                // 복제본 정리 후 공유 머티리얼로 되돌림
                foreach (var r in enemies) r.sharedMaterial = sharedMaterial;
                foreach (var m in copies) Destroy(m);
                copies.Clear();
            }
            else
            {
                // MPB 를 지워야 복제 머티리얼 색이 그대로 보임
                foreach (var r in enemies) r.SetPropertyBlock(null);
            }
        }

        private void OnDestroy()
        {
            foreach (var m in copies) if (m != null) Destroy(m);
        }
    }
}
