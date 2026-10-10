using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0418 포스필드 / 쉴드 (Depth Intersection)
    // 쉴드 셰이더가 Depth Texture 와 비교해 "다른 물체와 맞닿은 선"을 밝게 그림
    //   - 바닥과 만나는 원, 쉴드를 뚫고 지나가는 상자 단면, 박혀 있는 기둥 둘레가 빛남
    // 클릭: 쉴드에 맞은 지점에서 파문(_HitPos.xyz, 맞은 시각 _HitPos.w)이 퍼짐
    public class ShieldDemo : MonoBehaviour
    {
        private static readonly int HitPosId = Shader.PropertyToID("_HitPos");

        [SerializeField] private Renderer shield;
        [SerializeField] private Collider shieldCollider;
        [SerializeField] private Transform movingBox;
        [SerializeField, Range(0.01f, 2f)] private float intersectWidth = 0.35f;
        [SerializeField] private bool useIntersection = true;

        private Material mat;
        private Camera cam;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(shield.sharedMaterial, shield);
            cam = Camera.main;
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.I)) ToggleIntersection();

            // 상자가 쉴드 안팎을 드나듦 → 닿는 단면이 움직이며 빛남
            if (movingBox != null)
                movingBox.position = new Vector3(Mathf.Sin(Time.time * 0.7f) * 4.5f, 1f, -0.5f);

            if (DemoInput.LeftClick && shieldCollider.Raycast(DemoInput.MouseRay(cam), out var hit, 100f))
                mat.SetVector(HitPosId, new Vector4(hit.point.x, hit.point.y, hit.point.z, Time.timeSinceLevelLoad));
        }

        public void SetWidth(float v) { intersectWidth = v; Apply(); }
        public void ToggleIntersection() { useIntersection = !useIntersection; Apply(); }

        private void Apply()
        {
            if (mat == null) return;
            mat.SetFloat("_IntersectWidth", intersectWidth);
            mat.SetFloat("_UseIntersection", useIntersection ? 1f : 0f);
            GlossaryInfoPanel.Status = $"Depth Intersection {ShaderDemoUtil.OnOff(useIntersection)}   경계 폭 {intersectWidth:0.00}m\n" +
                                       "깊이 차이 = 뒤 물체 깊이 - 쉴드 표면 깊이 → 0 에 가까울수록 밝게";
        }
    }
}
