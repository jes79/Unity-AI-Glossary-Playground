using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Effects
{
    // 대화 중 Depth of Field: DOF 만 들어 있는 "대화용 Volume" 의 weight 를 0 ↔ 1 로 부드럽게 바꿈
    // → 대화가 시작되면 배경이 서서히 흐려지고, 끝나면 다시 선명해짐 (Volume 블렌딩)
    // Gaussian: 시작/끝 거리로 단순하게 흐림(가벼움)   Bokeh: 초점 거리·조리개로 실제 렌즈처럼(무거움)
    public class DialogueDofDemo : MonoBehaviour
    {
        [SerializeField] private Volume dialogueVolume;
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Transform npc;
        [SerializeField] private float blendTime = 0.35f;
        [SerializeField] private string[] lines =
        {
            "촌장: 어서 오게, 여행자여.",
            "촌장: 대화하는 동안 배경이 흐려진 게 보이나?",
            "촌장: 이게 Depth of Field 라네. 말이 끝나면 다시 선명해질 걸세.",
        };

        private DepthOfField dof;
        private Camera cam;
        private int lineIndex = -1;

        private bool Talking => lineIndex >= 0;

        private void Start()
        {
            cam = Camera.main;
            dialogueVolume.profile.TryGet(out dof);
            dialogueVolume.weight = 0f;
            dialogueBox.SetActive(false);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.E) || DemoInput.KeyDown(Key.Space)) Advance();
            if (DemoInput.KeyDown(Key.M)) ToggleMode();

            // 대화 중이면 weight → 1, 아니면 → 0
            dialogueVolume.weight = Mathf.MoveTowards(dialogueVolume.weight, Talking ? 1f : 0f, Time.deltaTime / blendTime);

            // Bokeh 초점은 항상 NPC 까지의 거리
            float dist = Vector3.Distance(cam.transform.position, npc.position);
            dof.focusDistance.value = dist;
            dof.gaussianStart.value = dist + 1.5f;
            dof.gaussianEnd.value = dist + 10f;

            GlossaryInfoPanel.Status = $"대화 {(Talking ? $"중 ({lineIndex + 1}/{lines.Length})" : "아님")}   대화 Volume weight {dialogueVolume.weight:0.00}\n" +
                                       $"DOF 모드 {dof.mode.value}   초점 거리 {dist:0.0}m (NPC)";
        }

        public void Advance()
        {
            lineIndex++;
            if (lineIndex >= lines.Length) lineIndex = -1;
            dialogueBox.SetActive(Talking);
            if (Talking) dialogueText.text = lines[lineIndex] + "   <size=70%><color=#9AA3B5>[E / Space]</color></size>";
        }

        public void ToggleMode() =>
            dof.mode.value = dof.mode.value == DepthOfFieldMode.Gaussian ? DepthOfFieldMode.Bokeh : DepthOfFieldMode.Gaussian;
    }
}
