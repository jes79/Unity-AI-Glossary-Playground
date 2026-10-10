using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Effects
{
    // 데칼: 벽·바닥 표면에 "투영해서" 붙이는 자국. 총알 자국을 남기고 5초 뒤 서서히 사라지게 함
    // 방식 A — URP Decal Projector: 렌더러에 Decal Renderer Feature 가 있어야 보임.
    //   이 씬은 공유 렌더러를 건드리지 않으려고, 빌더가 만든 복제 파이프라인(Decal 기능 포함)으로
    //   씬에 있는 동안만 QualitySettings.renderPipeline 을 바꿨다가 씬을 나갈 때 되돌림.
    // 방식 B — 대체 구현: 표면 법선 방향으로 살짝 띄운 Quad. 곡면·모서리에선 삐져나오지만 어디서나 동작
    public class DecalDemo : MonoBehaviour
    {
        public enum Mode { DecalProjector, QuadFallback }

        [SerializeField] private RenderPipelineAsset decalPipeline;   // Decal Renderer Feature 가 들어간 복제 파이프라인
        [SerializeField] private Material decalMaterial;              // Shader Graphs/Decal
        [SerializeField] private Material quadMaterial;               // 대체 구현용 반투명 Unlit
        [SerializeField] private Mode mode = Mode.DecalProjector;

        [Header("총알 자국")]
        [SerializeField] private float lifetime = 5f;     // 이 시간이 지나면
        [SerializeField] private float fadeTime = 0.6f;   // 이만큼 서서히 사라짐
        [SerializeField] private float size = 0.35f;
        [SerializeField] private int maxDecals = 60;

        private class Mark { public GameObject go; public DecalProjector projector; public Renderer quad; public float age; }

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private readonly List<Mark> marks = new List<Mark>();
        private RenderPipelineAsset previousPipeline;
        private bool swapped;
        private MaterialPropertyBlock mpb;
        private Camera cam;

        private bool ProjectorAvailable => swapped && decalMaterial != null;

        private void Awake()
        {
            cam = Camera.main;
            mpb = new MaterialPropertyBlock();
            if (decalPipeline != null)
            {
                previousPipeline = QualitySettings.renderPipeline;
                QualitySettings.renderPipeline = decalPipeline; // 이 씬에서만 Decal 기능이 있는 렌더러 사용
                swapped = true;
            }
            if (!ProjectorAvailable) mode = Mode.QuadFallback;
        }

        private void OnDestroy()
        {
            if (swapped) QualitySettings.renderPipeline = previousPipeline; // 반드시 되돌림
            swapped = false;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.M)) ToggleMode();
            if (DemoInput.KeyDown(Key.Space)) ShootRandom();
            if (DemoInput.KeyDown(Key.C)) ClearAll();
            if (DemoInput.LeftClick && !FxUtil.PointerOverUI && Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f))
                Place(hit.point, hit.normal);

            for (int i = marks.Count - 1; i >= 0; i--)
            {
                var mk = marks[i];
                mk.age += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01((mk.age - lifetime) / fadeTime);
                if (alpha <= 0f) { Destroy(mk.go); marks.RemoveAt(i); continue; }
                if (mk.projector != null) mk.projector.fadeFactor = alpha; // Decal Projector 자체의 투명도
                else if (mk.quad != null)
                {
                    mpb.SetColor(BaseColorId, new Color(1f, 1f, 1f, alpha));
                    mk.quad.SetPropertyBlock(mpb);
                }
            }

            GlossaryInfoPanel.Status = $"방식: {(mode == Mode.DecalProjector ? "URP Decal Projector" : "Quad 대체 구현")}" +
                                       $"{(ProjectorAvailable ? "" : "  (Decal 파이프라인 없음 → Quad 만 가능)")}\n" +
                                       $"자국 {marks.Count}개 — {lifetime:0}초 후 {fadeTime:0.0}초 동안 사라짐";
        }

        public void ToggleMode()
        {
            if (!ProjectorAvailable) { mode = Mode.QuadFallback; return; }
            mode = mode == Mode.DecalProjector ? Mode.QuadFallback : Mode.DecalProjector;
        }

        public void ShootRandom()
        {
            var origin = cam.transform.position;
            var dir = (cam.transform.forward + cam.transform.right * Random.Range(-0.35f, 0.35f) + cam.transform.up * Random.Range(-0.25f, 0.2f)).normalized;
            if (Physics.Raycast(origin, dir, out var hit, 100f)) Place(hit.point, hit.normal);
        }

        public void ClearAll()
        {
            foreach (var mk in marks) Destroy(mk.go);
            marks.Clear();
        }

        private void Place(Vector3 point, Vector3 normal)
        {
            if (marks.Count >= maxDecals) { Destroy(marks[0].go); marks.RemoveAt(0); }
            // 표면 안쪽(-normal)을 바라보게 + 랜덤 회전으로 매번 다른 모양
            var rot = Quaternion.LookRotation(-normal) * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            var mark = new Mark();

            if (mode == Mode.DecalProjector && ProjectorAvailable)
            {
                mark.go = new GameObject("BulletHole_Decal");
                mark.go.transform.SetPositionAndRotation(point, rot);
                var dp = mark.go.AddComponent<DecalProjector>();
                dp.material = decalMaterial;
                dp.size = new Vector3(size, size, 0.3f); // z = 투영 깊이
                dp.pivot = Vector3.zero;                 // 맞은 점을 중심으로 앞뒤 0.15 범위에 투영
                mark.projector = dp;
            }
            else
            {
                mark.go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                mark.go.name = "BulletHole_Quad";
                Destroy(mark.go.GetComponent<Collider>());
                mark.go.transform.SetPositionAndRotation(point + normal * 0.01f, rot); // Z-fighting 방지로 1cm 띄움
                mark.go.transform.localScale = Vector3.one * size;
                mark.quad = mark.go.GetComponent<Renderer>();
                mark.quad.sharedMaterial = quadMaterial;
                mark.quad.shadowCastingMode = ShadowCastingMode.Off;
            }
            marks.Add(mark);
        }
    }
}
