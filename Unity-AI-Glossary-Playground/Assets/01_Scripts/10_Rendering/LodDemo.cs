using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // LOD Group 3단계 데모: 카메라가 멀어질수록 건물이 LOD0(자세히) → LOD1 → LOD2(상자) → Culled 로 바뀜
    // 빌더가 코드로 LODGroup.SetLODs 를 호출해 만든 건물들
    // C 키: 단계별 색칠(LOD0 초록 / LOD1 노랑 / LOD2 빨강)로 지금 어떤 단계인지 눈으로 확인
    public class LodDemo : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LODGroup[] buildings;
        [SerializeField] private Material[] debugMaterials;   // 단계별 색 (0,1,2)
        [SerializeField] private float minDistance = 8f;
        [SerializeField] private float maxDistance = 140f;
        [SerializeField] private float distance = 15f;

        private bool colorize;
        private int forced = -1;
        private Material[][][] originalMats; // [건물][LOD][렌더러]

        private void Start()
        {
            originalMats = new Material[buildings.Length][][];
            for (int b = 0; b < buildings.Length; b++)
            {
                var lods = buildings[b].GetLODs();
                originalMats[b] = new Material[lods.Length][];
                for (int l = 0; l < lods.Length; l++)
                {
                    originalMats[b][l] = new Material[lods[l].renderers.Length];
                    for (int r = 0; r < lods[l].renderers.Length; r++) originalMats[b][l][r] = lods[l].renderers[r].sharedMaterial;
                }
            }
        }

        private void Update()
        {
            distance += -DemoInput.Move.y * 40f * Time.deltaTime - DemoInput.Scroll * 0.05f; // W/S 또는 휠
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            cam.transform.position = new Vector3(0f, 4f + distance * 0.15f, -distance);
            cam.transform.LookAt(new Vector3(0f, 3f, 0f));

            if (DemoInput.KeyDown(Key.C)) ToggleColorize();
            if (DemoInput.KeyDown(Key.Digit0)) ForceLod(0);
            if (DemoInput.KeyDown(Key.Digit1)) ForceLod(1);
            if (DemoInput.KeyDown(Key.Digit2)) ForceLod(2);
            if (DemoInput.KeyDown(Key.F)) ForceLod(-1);

            // 화면 높이 대비 건물 크기(= LOD 전환 기준값) 계산
            var g = buildings[0];
            float size = g.size * Mathf.Max(g.transform.lossyScale.x, g.transform.lossyScale.y);
            float d = Vector3.Distance(cam.transform.position, g.transform.TransformPoint(g.localReferencePoint));
            float relHeight = size / (2f * d * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad)) * QualitySettings.lodBias;
            GlossaryInfoPanel.Status =
                $"카메라 거리 {distance:0}m   화면 높이 비율 {relHeight * 100f:0}%\n" +
                $"가운데 건물 현재 단계: <b>{CurrentLod(g)}</b>   (LOD0 ≥60%, LOD1 ≥25%, LOD2 ≥8%, 그 아래 Culled)\n" +
                $"강제 LOD: {(forced < 0 ? "자동" : "LOD" + forced)}   색칠: {(colorize ? "ON" : "OFF")}   LOD Bias {QualitySettings.lodBias:0.0}";
        }

        // 어느 단계 렌더러가 실제로 보이는지로 현재 LOD 를 알아냄 (LODGroup 은 현재 단계를 직접 알려주지 않음)
        private static string CurrentLod(LODGroup g)
        {
            var lods = g.GetLODs();
            for (int i = 0; i < lods.Length; i++)
                foreach (var r in lods[i].renderers)
                    if (r != null && r.isVisible) return "LOD" + i;
            return "Culled (안 그림)";
        }

        public void ToggleColorize()
        {
            colorize = !colorize;
            for (int b = 0; b < buildings.Length; b++)
            {
                var lods = buildings[b].GetLODs();
                for (int l = 0; l < lods.Length; l++)
                    for (int r = 0; r < lods[l].renderers.Length; r++)
                        lods[l].renderers[r].sharedMaterial = colorize ? debugMaterials[Mathf.Min(l, debugMaterials.Length - 1)] : originalMats[b][l][r];
            }
        }

        // 테스트용 강제 단계 (-1 = 자동)
        public void ForceLod(int level)
        {
            forced = level;
            foreach (var g in buildings) g.ForceLOD(level);
        }

        public void Near() => distance = 15f;
        public void Far() => distance = 90f;
    }
}
