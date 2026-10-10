using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0412 아웃라인 - 마우스를 올린 아이템에만 노란 외곽선
    // 매 프레임 마우스 위치로 레이를 쏴서 맞은 아이템의 _OutlineWidth 를 MaterialPropertyBlock 으로 켬
    // (아이템들이 머티리얼을 공유해도 하나만 외곽선이 생김)
    public class OutlineHoverDemo : MonoBehaviour
    {
        private static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");

        [SerializeField] private Renderer[] items;
        [SerializeField, Range(0.005f, 0.15f)] private float outlineWidth = 0.04f;
        [SerializeField] private Color outlineColor = new Color(1f, 0.85f, 0.1f);
        [SerializeField] private bool showAll;

        private MaterialPropertyBlock block;
        private Renderer hovered;
        private Camera cam;

        private void Start()
        {
            block = new MaterialPropertyBlock();
            cam = Camera.main;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.A)) ToggleShowAll();

            // 마우스 아래 아이템 찾기
            hovered = null;
            if (Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f))
            {
                var r = hit.collider.GetComponent<Renderer>();
                if (System.Array.IndexOf(items, r) >= 0) hovered = r;
            }

            foreach (var r in items)
            {
                bool on = showAll || r == hovered;
                r.GetPropertyBlock(block);
                block.SetFloat(WidthId, on ? outlineWidth : 0f);
                block.SetColor(ColorId, outlineColor);
                r.SetPropertyBlock(block);
            }

            GlossaryInfoPanel.Status = $"마우스 아래: <b>{(hovered != null ? hovered.name : "없음")}</b>   두께 {outlineWidth:0.000}m   전체 표시 {ShaderDemoUtil.OnOff(showAll)}";
        }

        public void SetWidth(float value) => outlineWidth = value;
        public void ToggleShowAll() => showAll = !showAll;
    }
}
