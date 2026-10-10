using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Patterns
{
    // 버전 관리 개념을 화면에서 흉내 내는 미니 Git (실제 Git 을 실행하지 않음)
    // 수정 → add/commit → 커밋 기록이 쌓이고, checkout 으로 예전 시점으로 돌아가 볼 수 있음
    // .gitignore 에 걸리는 파일(Library/ 등)은 add 해도 무시되는 것도 보여 줌
    public class MiniGitSimulator : MonoBehaviour
    {
        [SerializeField] private RectTransform timeline;   // 커밋 동그라미가 놓일 영역
        [SerializeField] private RectTransform headMarker;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Sprite nodeSprite;
        [SerializeField] private float spacing = 110f;

        private readonly List<string> commits = new();
        private readonly List<RectTransform> nodes = new();
        private readonly List<string> changed = new();
        private int head = -1;
        private int fileCounter;
        private string message = "";

        private static readonly string[] SampleFiles = { "Player.cs", "Enemy.prefab", "Stage1.unity", "UI.cs", "Boss.mat", "Sound.wav" };

        private void Start()
        {
            AddCommit("첫 커밋 (프로젝트 생성)");
            Refresh();
        }

        public void ModifyFile()
        {
            string f = SampleFiles[fileCounter++ % SampleFiles.Length];
            if (!changed.Contains(f)) changed.Add(f);
            message = $"{f} 수정됨 (아직 커밋 안 됨)";
            Refresh();
        }

        public void Commit()
        {
            if (head != commits.Count - 1)
            {
                message = "<color=#FF8A65>예전 커밋을 보는 중(detached HEAD)</color> — '최신으로' 돌아온 뒤 커밋하세요 (실제로는 새 브랜치를 만듦)";
            }
            else if (changed.Count == 0)
            {
                message = "바뀐 파일이 없어 커밋할 것이 없음 (nothing to commit)";
            }
            else
            {
                AddCommit($"{string.Join(", ", changed)} 수정");
                message = $"git add . → git commit  ({changed.Count}개 파일)";
                changed.Clear();
            }
            Refresh();
        }

        public void CheckoutPrevious()
        {
            if (head > 0) head--;
            message = $"git checkout → 커밋 #{head + 1} 시점으로 이동 (그때의 파일 상태)";
            Refresh();
        }

        public void CheckoutLatest()
        {
            head = commits.Count - 1;
            message = "git checkout main → 최신 커밋으로";
            Refresh();
        }

        // .gitignore 체험: Library 폴더를 add 해 보기
        public void TryAddLibrary()
        {
            message = "git add Library/ → <color=#9BE89B>.gitignore 의 [Ll]ibrary/ 규칙에 걸려 무시됨</color> (Unity 가 다시 만드는 캐시라 올리면 안 됨)";
            Refresh();
        }

        private void AddCommit(string msg)
        {
            commits.Add(msg);
            head = commits.Count - 1;

            // 커밋 동그라미 + 번호
            var go = new GameObject($"Commit_{commits.Count}", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(timeline, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(64f, 64f);
            rt.anchoredPosition = new Vector2(50f + (commits.Count - 1) * spacing, 0f);
            var img = go.GetComponent<Image>();
            img.sprite = nodeSprite;
            img.color = new Color(0.35f, 0.6f, 1f);

            var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            label.rectTransform.SetParent(rt, false);
            label.rectTransform.sizeDelta = new Vector2(64f, 64f);
            label.font = GlossaryInfoPanel.Font;
            label.fontSize = 26f;
            label.alignment = TextAlignmentOptions.Center;
            label.text = commits.Count.ToString();
            label.raycastTarget = false;
            nodes.Add(rt);

            // 오래된 것은 왼쪽으로 밀어 최근 8개만 보이게
            float overflow = Mathf.Max(0, commits.Count - 8) * spacing;
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].anchoredPosition = new Vector2(50f + i * spacing - overflow, 0f);
        }

        private void Refresh()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var img = nodes[i].GetComponent<Image>();
                img.color = i == head ? new Color(1f, 0.8f, 0.3f) : i > head ? new Color(0.35f, 0.4f, 0.5f) : new Color(0.35f, 0.6f, 1f);
                nodes[i].gameObject.SetActive(nodes[i].anchoredPosition.x >= 0f);
            }
            if (head >= 0)
            {
                headMarker.SetParent(nodes[head], false);
                headMarker.anchoredPosition = new Vector2(0f, 62f);
            }

            statusText.text = $"HEAD → 커밋 #{head + 1}: {commits[head]}\n" +
                              $"작업 중인 변경: {(changed.Count == 0 ? "없음 (clean)" : string.Join(", ", changed))}\n" +
                              $"<color=#FFD54F>{message}</color>";
            GlossaryInfoPanel.Status = $"커밋 {commits.Count}개   HEAD #{head + 1}   변경 {changed.Count}개 (미니 시뮬레이터 — 실제 Git 아님)";
        }
    }
}
