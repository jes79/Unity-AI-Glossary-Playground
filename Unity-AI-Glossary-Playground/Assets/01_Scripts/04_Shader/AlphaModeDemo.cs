using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0407 Alpha / Transparent / Cutout
    // 같은 나뭇잎 텍스처(알파 채널 = 잎 모양)를 세 방식으로 그림
    //   Opaque     : 알파 무시 → 사각형 그대로 보임
    //   Transparent: 알파만큼 반투명하게 섞음 → 가장자리가 부드럽지만 정렬 문제·비용이 있음
    //   Cutout     : 알파가 Cutoff 보다 작으면 아예 버림(Alpha Clipping) → 경계가 딱 끊기고 불투명처럼 싸게 그려짐
    // 가운데 나무의 잎은 M 키로 세 방식을 돌려 보며 비교 (요청 예문: Cutout 으로 바꾸기)
    public class AlphaModeDemo : MonoBehaviour
    {
        public enum Mode { Opaque, Transparent, Cutout }

        [SerializeField] private Renderer[] treeLeaves;
        [SerializeField] private Material opaqueMat;
        [SerializeField] private Material transparentMat;
        [SerializeField] private Material cutoutMat;
        [SerializeField] private Mode treeMode = Mode.Opaque;
        [SerializeField, Range(0f, 1f)] private float cutoff = 0.5f;

        private Material runtimeCutout;

        private void Start()
        {
            // Cutoff 슬라이더가 에셋을 고치지 않도록 컷아웃 머티리얼만 실행용 복사본 사용 (예시 쿼드 포함 전체)
            var all = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            runtimeCutout = ShaderDemoUtil.CloneShared(cutoutMat, all);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.M)) NextMode();
            if (DemoInput.KeyDown(Key.C)) SetMode((int)Mode.Cutout);
        }

        public void NextMode() => SetMode(((int)treeMode + 1) % 3);

        public void SetMode(int mode)
        {
            treeMode = (Mode)mode;
            Apply();
        }

        public void SetCutoff(float value)
        {
            cutoff = value;
            Apply();
        }

        private void Apply()
        {
            if (runtimeCutout != null) runtimeCutout.SetFloat("_Cutoff", cutoff);
            var mat = treeMode switch
            {
                Mode.Transparent => transparentMat,
                Mode.Cutout => runtimeCutout,
                _ => opaqueMat,
            };
            foreach (var r in treeLeaves) r.sharedMaterial = mat;

            string desc = treeMode switch
            {
                Mode.Transparent => "Transparent: 반투명 섞기 (Surface Type = Transparent)",
                Mode.Cutout => "Cutout: Alpha Clipping 체크 + Threshold(Cutoff)",
                _ => "Opaque: 알파 무시 → 사각형이 그대로 보임",
            };
            GlossaryInfoPanel.Status = $"나무 잎 방식: <b>{treeMode}</b>\n{desc}\nAlpha Cutoff = {cutoff:0.00} (Cutout 에만 적용)";
        }
    }
}
