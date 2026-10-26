using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GlossaryDemo
{
    // 강의 모드 화면: 위쪽 진행 막대 + 오른쪽 아래 담당자(픽셀아트 캐릭터·이름표·말풍선) + 아래쪽 키 안내
    // GlossaryInfoPanel 이 강의 모드일 때 같은 오브젝트에 붙임 → 씬 182개를 따로 고치지 않아도 됨
    // 대사는 카탈로그(용어·쉽게 말하면·개념·확인 포인트·요청 예문)와 씬의 조작법으로 조합함
    public class LectureOverlay : MonoBehaviour
    {
        public const float ProgressHeight = 88f; // 진행 막대 높이 (③ 단계 조작 카드가 이 아래에 붙음)

        private const float Margin = 24f;
        private const float CharSlotWidth = 132f;   // 캐릭터 그림의 가장 넓은 폭 (상태마다 폭이 달라도 말풍선이 흔들리지 않게)
        private const float CharBottom = 8f;        // 캐릭터가 서 있는 높이
        private const float BubbleBigWidth = 960f, BubbleSmallWidth = 440f;
        private const float BigBubbleY = 110f;      // 큰 말풍선 아래 (데모 버튼 줄 CreateButtonRow(y 60, 높이 70) 위)
        private const float CharsPerSecond = 45f;   // 말풍선 글자가 나오는 속도

        private static readonly Color BubbleColor = new Color32(250, 250, 252, 255); // 불투명 (뒤 글자가 비치지 않게)
        private static readonly Color InkColor = new Color32(32, 34, 44, 255);

        public static LectureOverlay Current { get; private set; }

        private GlossaryInfoPanel panel;
        private GlossaryEntry entry;
        private LecturePresenter presenter;
        private LectureStep[] steps;

        private GameObject canvasGo;
        private TextMeshProUGUI progressTitle, progressSteps;
        private RectTransform progressFill, progressRect;
        private RectTransform characterRect, bubble, bubbleTail, keyGuideRect, cardsRow;
        private RawImage character;
        private RawImage fallback;                // 그림이 없을 때 보이는 원 + 글자
        private TextMeshProUGUI fallbackLetter;
        private Image nameTagImage;
        private TextMeshProUGUI nameTagText;
        private float characterScale = 2f;
        private float bobTime;
        private TextMeshProUGUI bubbleText, continueMark;
        private GameObject promptBlock;
        private CanvasGroup promptGroup;
        private TextMeshProUGUI badText, goodText;
        private readonly TextMeshProUGUI[] elementNotes = new TextMeshProUGUI[4];
        private readonly Image[] elementBgs = new Image[4];

        private float typed;
        private int totalChars;
        private bool typing;
        private bool bubbleHidden;

        // 화면을 가리는 강의 UI (ScreenshotRunner 의 가림 점검용)
        public IEnumerable<RectTransform> CoverRects
        {
            get
            {
                yield return progressRect;
                yield return characterRect;
                if (bubble.gameObject.activeSelf) yield return bubble;
                yield return keyGuideRect;
            }
        }

        public LectureStep CurrentStep => steps[LectureMode.StepIndex];

        public static LectureOverlay Create(GlossaryInfoPanel panel, GlossaryEntry entry)
        {
            var overlay = panel.gameObject.AddComponent<LectureOverlay>();
            overlay.panel = panel;
            overlay.entry = entry;
            overlay.presenter = LecturePresenters.For(entry.Category);
            overlay.steps = LectureMode.BeginEntry(entry);
            overlay.Build();
            overlay.ShowStep();
            Current = overlay;
            return overlay;
        }

        // 강의 모드를 끌 때: 강의 UI 를 지우고 정보 패널을 원래대로
        public void Close()
        {
            panel.SetLectureView(GlossaryInfoPanel.LectureView.Off);
            if (canvasGo != null) Destroy(canvasGo);
            Destroy(this);
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        // ───── 입력 ─────

        private void Update()
        {
            UpdateTyping();
            UpdateBob();

            if (LectureMode.TypingInInputField) return;
            if (DemoInput.KeyDown(LectureMode.NextKey) || DemoInput.KeyDown(LectureMode.NextKeyAlt)) Next();
            else if (DemoInput.KeyDown(LectureMode.PrevKey) || DemoInput.KeyDown(LectureMode.PrevKeyAlt)) Prev();
            else if (DemoInput.KeyDown(LectureMode.NextTermKey)) GoNeighbor(+1, false);
            else if (DemoInput.KeyDown(LectureMode.PrevTermKey)) GoNeighbor(-1, false);
            else if (DemoInput.KeyDown(LectureMode.HubKey)) GoHub();
            else if (DemoInput.KeyDown(LectureMode.BubbleKey)) ToggleBubble();
        }

        // 다음: 글자가 나오는 중이면 먼저 전부 보여 주고, 다 나왔으면 다음 단계 / 다음 용어
        public void Next()
        {
            if (typing)
            {
                SkipTyping();
                return;
            }
            if (LectureMode.StepIndex < steps.Length - 1)
            {
                LectureMode.StepIndex++;
                ShowStep();
            }
            else if (!GoNeighbor(+1, false))
            {
                GoHub(); // 마지막 용어까지 끝나면 허브로
            }
        }

        public void Prev()
        {
            if (LectureMode.StepIndex > 0)
            {
                LectureMode.StepIndex--;
                ShowStep();
            }
            else
            {
                GoNeighbor(-1, true); // 이전 용어의 마지막 단계(④)로
            }
        }

        private bool GoNeighbor(int dir, bool atLastStep)
        {
            if (!LectureMode.TryNeighbor(entry.Id, dir, out var next)) return false;
            if (!CanLeaveScene()) return true;
            LectureMode.Go(next, atLastStep);
            return true;
        }

        private void GoHub()
        {
            if (CanLeaveScene()) LectureMode.GoHub();
        }

        // 데모가 비동기 로드의 활성화를 기다리는 중이면(0616) 다른 씬으로 가지 않고 말풍선으로 알려 줌
        private bool CanLeaveScene()
        {
            if (!LectureMode.SceneChangeLocked) return true;
            bubbleHidden = false;
            bubble.gameObject.SetActive(true);
            StartTyping("잠깐만요! 데모가 다음 씬을 불러 놓고 기다리는 중이에요.\n강의 키가 아닌 아무 키나 눌러 로딩을 끝낸 다음 넘어갈게요.");
            return false;
        }

        private void ToggleBubble()
        {
            bubbleHidden = !bubbleHidden;
            bubble.gameObject.SetActive(!bubbleHidden);
        }

        // ───── 단계 표시 ─────

        private void ShowStep()
        {
            var step = CurrentStep;
            if (step >= LectureStep.Demo) LectureMode.DemoReached = true; // ③ 이후로는 시간 제한 데모도 진행
            bool compact = step == LectureStep.Demo; // ③ 직접 보기: 말풍선과 캐릭터를 줄여 데모가 잘 보이게

            // ③ 에서는 정보 패널을 '조작법 + 확인 포인트 + 실시간 상태' 카드로, 나머지 단계에서는 숨김
            panel.SetLectureView(compact ? GlossaryInfoPanel.LectureView.Card : GlossaryInfoPanel.LectureView.Hidden);

            // 강의 여는 인사는 디렉터, 나머지는 분야 담당자
            var who = step == LectureStep.Opening ? LecturePresenters.Director : presenter;
            nameTagText.text = who.Role;
            nameTagImage.color = who.Accent;

            // 픽셀아트는 정수 배로만 키움 (2배 = 높이 360, ③ 은 1배)
            characterScale = compact ? 1f : 2f;
            SetCharacter(who, StateFor(step));
            float slot = CharSlotWidth * characterScale;

            if (compact)
            {
                // 캐릭터 머리 위에 작은 말풍선, 꼬리는 아래(캐릭터) 방향
                Place(bubble, new Vector2(1f, 0f), new Vector2(-Margin, CharBottom + characterRect.sizeDelta.y + 24f), new Vector2(BubbleSmallWidth, 0f));
                bubbleTail.anchorMin = bubbleTail.anchorMax = new Vector2(1f, 0f);
                bubbleTail.anchoredPosition = new Vector2(-slot * 0.5f, 0f);
                bubbleText.fontSize = 22f;
            }
            else
            {
                // 캐릭터 왼쪽에 큰 말풍선, 꼬리는 오른쪽(캐릭터) 방향
                Place(bubble, new Vector2(1f, 0f), new Vector2(-(Margin + slot + 12f), BigBubbleY), new Vector2(BubbleBigWidth, 0f));
                bubbleTail.anchorMin = bubbleTail.anchorMax = new Vector2(1f, 0f);
                bubbleTail.anchoredPosition = new Vector2(0f, 70f);
                bubbleText.fontSize = 27f;
            }

            // ④ 와 디렉터 인사에서 '좋은 요청의 4요소' 블록을 씀 (나쁜/좋은 요청 카드는 ④ 에서만)
            promptBlock.SetActive(step == LectureStep.Prompt || step == LectureStep.Opening);
            cardsRow.gameObject.SetActive(step == LectureStep.Prompt);
            if (step == LectureStep.Prompt) FillPromptBlock();
            else if (step == LectureStep.Opening) FillOpeningElements();

            bubbleHidden = false;
            bubble.gameObject.SetActive(true);
            StartTyping(LineFor(step));
            UpdateProgress();
        }

        // 단계별 대사: 카탈로그 문장에 담당자 말투의 앞뒤 문장을 붙인다
        private string LineFor(LectureStep step)
        {
            int index = LectureMode.IndexInCategory(entry);
            int count = LectureMode.CountInCategory(entry.Category);
            string term = $"<b><color=#C0571A>{entry.Term}</color></b>";

            switch (step)
            {
                case LectureStep.Opening:
                    return "안녕하세요, 디렉터예요! 오늘은 개발실 팀원들이 예제 씬을 직접 돌려 보면서 게임 용어를 하나씩 설명할 거예요.\n" +
                           "용어를 알면 AI에게 원하는 걸 정확하게 부탁할 수 있어요. 좋은 요청에는 아래 네 가지가 들어가요.\n" +
                           "예) '<b>플레이어 점프</b>에 <b>코요테 타임</b>을 <b>0.1초</b> 넣어줘. <b>Unity 6 · 2D</b> 프로젝트야.'";

                case LectureStep.SelfIntro:
                    // 같은 담당자가 여러 분야를 맡으므로 분야가 바뀔 때마다 '이 분야에서 회사가 하는 일'을 소개
                    string hello = LectureMode.SamePresenterAsBefore
                        ? $"또 저예요, {presenter.Role} 담당!"
                        : $"안녕하세요! {presenter.Role} 담당이에요.";
                    return $"{hello}\n" +
                           $"이번엔 '{entry.CategoryName}' 분야예요. {LecturePresenters.CategoryJob(entry.Category)}\n" +
                           $"이 분야 용어는 모두 <b>{count}개</b>, {term}부터 시작할게요!";

                case LectureStep.Intro:
                    string opener = LectureMode.CurrentHasSelfIntro
                        ? (index == 1 ? "첫 번째 용어부터 갈게요." : "그럼 바로 시작해 볼게요.")
                        : presenter.NextLine;
                    string summary = string.IsNullOrEmpty(entry.Summary) ? "" : $"\n쉽게 말하면 <b>'{entry.Summary}'</b>, 이런 뜻이에요.";
                    return $"{opener}\n이번 용어는 {term}!{summary}";

                case LectureStep.Concept:
                    return $"조금 더 자세히 볼게요.\n{entry.Concept}";

                case LectureStep.Demo:
                    return presenter.DemoLine;

                case LectureStep.Prompt:
                    return "이제 AI에게 부탁해 볼까요? 왼쪽처럼 막연하게 말하면 AI가 무엇을 얼마나 만들지 몰라 엉뚱한 결과가 나와요. " +
                           "오른쪽 예문에서 출발해서, 아래 4요소 중 <b>빠진 칸을 채워</b> 내 게임에 맞게 고쳐 보세요.";
            }
            return "";
        }

        // 단계 → 캐릭터 그림 상태
        private static string StateFor(LectureStep step)
        {
            switch (step)
            {
                case LectureStep.Intro: return "rest";    // 기본
                case LectureStep.Concept: return "work";  // 키보드 들고 생각
                case LectureStep.Prompt: return "ask";    // 손 들고 물음표
                default: return "review";                 // 인사·분야 소개·직접 보기: 반짝이며 웃음
            }
        }

        // 캐릭터 그림을 원본 비율 그대로 정수 배 확대해서 오른쪽 아래에 세움
        private void SetCharacter(LecturePresenter who, string state)
        {
            var tex = LecturePresenters.Sprite(who, state);
            character.enabled = tex != null;
            fallback.gameObject.SetActive(tex == null);
            fallback.color = who.Accent;
            if (tex != null)
            {
                character.texture = tex;
                characterRect.sizeDelta = new Vector2(tex.width, tex.height) * characterScale;
            }
            else
            {
                characterRect.sizeDelta = new Vector2(90f, 90f) * characterScale;
                fallbackLetter.text = who.Role.Substring(0, 1);
                fallbackLetter.fontSize = 40f * characterScale;
            }
            characterRect.anchorMin = characterRect.anchorMax = new Vector2(1f, 0f);
            characterRect.pivot = new Vector2(0.5f, 0f); // 발끝 가운데 기준
            characterRect.anchoredPosition = new Vector2(-(Margin + CharSlotWidth * characterScale * 0.5f), CharBottom);
        }

        // 말하는 동안(글자가 나오는 중) 캐릭터가 살짝 통통 튐. 픽셀이 뭉개지지 않게 그림 1픽셀 단위로 움직임
        private void UpdateBob()
        {
            float hop = 0f;
            if (typing)
            {
                bobTime += Time.unscaledDeltaTime;
                hop = Mathf.Round(Mathf.Abs(Mathf.Sin(bobTime * 9f)) * 3f) * characterScale;
            }
            else
            {
                bobTime = 0f;
            }
            characterRect.anchoredPosition = new Vector2(characterRect.anchoredPosition.x, CharBottom + hop);
        }

        // 디렉터 인사: 4요소를 예시 문장으로 짚음
        private void FillOpeningElements()
        {
            SetElement(0, "무엇에? 플레이어 점프", true);
            SetElement(1, "정확한 이름: 코요테 타임", true);
            SetElement(2, "얼마나? 0.1초", true);
            SetElement(3, "어디서? Unity 6 · 2D", true);
        }

        // ④ 막연한 요청 / 구체적인 요청 / 좋은 요청의 4요소
        // 4요소 칸: 예문에 들어 있으면 초록 "들어 있음", 빠졌으면 주황 "빠짐 → 예: …" (학생이 채워 넣을 거리)
        private void FillPromptBlock()
        {
            badText.text = $"\"{LectureRequestExamples.Vague(entry)}\"";
            goodText.text = entry.Prompt;

            string prompt = entry.Prompt ?? "";
            string target = FindTarget(prompt);
            string termFound = FindTerm(entry.Term, prompt);
            string number = FindNumber(prompt);
            string env = FindEnvironment(prompt);
            bool is2D = Camera.main != null && Camera.main.orthographic;

            SetElement(0, target != null ? $"'{target}' 들어 있음" : $"빠짐 → 예: {LectureRequestExamples.TargetExample(entry.Category)}", target != null);
            SetElement(1, termFound != null ? $"'{termFound}' 들어 있음" : $"빠짐 → 예: '{ShortTerm(entry.Term)}'", termFound != null);
            SetElement(2, number != null ? $"'{number}' 들어 있음" : $"빠짐 → 예: {LectureRequestExamples.NumberExample(entry.Category)}", number != null);
            SetElement(3, env != null ? $"'{env}' 들어 있음" : $"빠짐 → 예: Unity 6 · URP · {(is2D ? "2D" : "3D")}", env != null);
        }

        private void SetElement(int i, string note, bool found)
        {
            elementNotes[i].text = note;
            elementBgs[i].color = found ? new Color32(214, 242, 220, 255)  // 들어 있음: 초록
                                        : new Color32(255, 236, 204, 255); // 빠짐: 주황 (채울 거리)
        }

        // "가속/감속 (Acceleration)" → "가속/감속"
        private static string ShortTerm(string term)
        {
            int p = term.IndexOf(" (", System.StringComparison.Ordinal);
            return p > 0 ? term.Substring(0, p) : term;
        }

        // 요청 대상(무엇에 적용할지)으로 자주 쓰는 말. '적' 한 글자는 '적용' 같은 낱말과 헷갈려 조사까지 붙여 찾음
        private static readonly string[] TargetWords =
        {
            "플레이어", "캐릭터", "몬스터", "보스", "NPC", "적이", "적을", "적에", "적의", "적과", "적 ", "총알", "아이템", "버튼",
            "카메라", "상자", "코인", "포탑", "무기", "갑옷", "나무", "건물", "바위", "풀이", "물고기", "체력바", "인벤토리", "상점",
            "스킬", "퀘스트", "발소리", "BGM", "효과음", "모닥불", "던전", "GameManager", "PlayerController", "UI",
        };

        private static string FindTarget(string prompt)
        {
            foreach (string w in TargetWords)
                if (prompt.Contains(w)) return w.Trim();
            return null;
        }

        // 용어(또는 괄호 안 영문, '/' 로 나뉜 각 부분)가 요청 예문에 들어 있는지
        private static string FindTerm(string term, string prompt)
        {
            string flat = prompt.Replace(" ", "").ToLowerInvariant();
            foreach (string part in term.Split('(', ')', '/', ','))
            {
                string p = part.Trim();
                if (p.Length < 2) continue;
                if (flat.Contains(p.Replace(" ", "").ToLowerInvariant())) return p;
            }
            return null;
        }

        // 숫자 + 단위. 영문 이름 속 숫자(Vector3, Rigidbody2D)와 환경 표기(2D/3D, Unity 6)는 수치로 치지 않음
        private static readonly Regex NumberRegex = new Regex(@"(?<![A-Za-z\d.])\d+(\.\d+)?(?![\dDd])\s?(초|도|ms|m|%|만 개|개|배|프레임|px|fps|번|칸|분|명|마리|그루|kg|cm|단계|층|가지|레벨|씩)?");

        private static string FindNumber(string prompt)
        {
            foreach (Match m in NumberRegex.Matches(prompt))
            {
                if (m.Index >= 6 && prompt.Substring(m.Index - 6, 6) == "Unity ") continue;
                // '3인칭' 처럼 낱말의 일부인 숫자는 수치가 아님
                if (string.CompareOrdinal(prompt, m.Index + m.Length, "인칭", 0, 2) == 0) continue;
                return m.Value.Trim();
            }
            return null;
        }

        // 내 환경 표기. 다른 낱말 속 글자("NPC" 의 PC, "AudioSource" 의 ios)는 빼고 낱말 단위로 찾음
        private static readonly Regex EnvironmentRegex =
            new Regex(@"(?<![A-Za-z])(Unity 6|URP|HDRP|2D|3D|Input System|모바일|PC|Android|iOS|WebGL)(?![A-Za-z])");

        private static string FindEnvironment(string prompt)
        {
            var m = EnvironmentRegex.Match(prompt);
            return m.Success ? m.Value : null;
        }

        private void UpdateProgress()
        {
            int index = LectureMode.IndexInCategory(entry);
            int count = LectureMode.CountInCategory(entry.Category);
            progressTitle.text = $"{entry.Category:D2} {entry.CategoryName}  ›  <color=#FFD479>{entry.Id} {entry.Term}</color>  <color=#A8B0C0>({index}/{count})</color>";

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < steps.Length; i++)
            {
                if (i > 0) sb.Append("<color=#5A6070>  ›  </color>");
                string label = StepLabel(steps[i]);
                if (i == LectureMode.StepIndex) sb.Append("<b><color=#FFFFFF>").Append(label).Append("</color></b>");
                else sb.Append("<color=#7A8294>").Append(label).Append("</color>");
            }
            progressSteps.text = sb.ToString();

            // 분야 안 진행률 (이 용어의 단계까지 반영)
            float t = (index - 1 + (LectureMode.StepIndex + 1f) / steps.Length) / Mathf.Max(1, count);
            progressFill.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);
        }

        private static string StepLabel(LectureStep step)
        {
            switch (step)
            {
                case LectureStep.Opening: return "시작";
                case LectureStep.SelfIntro: return "분야 소개";
                case LectureStep.Intro: return "① 소개";
                case LectureStep.Concept: return "② 개념";
                case LectureStep.Demo: return "③ 직접 보기";
                default: return "④ AI에게 요청";
            }
        }

        // ───── 한 글자씩 나오는 효과 ─────

        private void StartTyping(string text)
        {
            bubbleText.text = text;
            bubbleText.maxVisibleCharacters = 0;
            bubbleText.ForceMeshUpdate(); // 글자 수(태그 제외)를 바로 알기 위해 지금 한 번 계산
            totalChars = bubbleText.textInfo.characterCount;
            typed = 0f;
            typing = true;
            continueMark.gameObject.SetActive(false);
            promptGroup.alpha = 0f; // ④ 카드는 대사가 끝난 뒤에 보여 줌 (자리는 미리 잡아 둠)
        }

        private void UpdateTyping()
        {
            if (!typing) return;
            typed += Time.unscaledDeltaTime * CharsPerSecond; // 일시정지(timeScale 0) 데모에서도 진행
            bubbleText.maxVisibleCharacters = (int)typed;
            if ((int)typed >= totalChars) SkipTyping();
        }

        public void SkipTyping()
        {
            typing = false;
            bubbleText.maxVisibleCharacters = 99999;
            continueMark.gameObject.SetActive(true);
            promptGroup.alpha = 1f;
        }

        // ───── UI 생성 ─────

        private void Build()
        {
            canvasGo = new GameObject("LectureCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1001; // 정보 패널(1000) 위
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var root = canvasGo.transform;

            BuildProgress(root);
            BuildCharacter(root);
            BuildBubble(root);
            BuildKeyGuide(root);
        }

        // 위쪽 진행 막대: 정보 패널 자리(왼쪽 위)를 그대로 써서 데모 UI 를 가리지 않음
        private void BuildProgress(Transform root)
        {
            progressRect = NewRect("Progress", root);
            Place(progressRect, new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(GlossaryInfoPanel.PanelWidth, ProgressHeight));
            progressRect.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.1f, 0.9f);

            progressTitle = NewText("Title", progressRect, "", 22f, Color.white);
            progressTitle.enableAutoSizing = true;
            progressTitle.fontSizeMin = 14f;
            progressTitle.fontSizeMax = 22f;
            progressTitle.textWrappingMode = TextWrappingModes.NoWrap;
            progressTitle.overflowMode = TextOverflowModes.Truncate;
            Fill(progressTitle.rectTransform, new Vector2(16f, -10f), new Vector2(-16f, -40f));

            progressSteps = NewText("Steps", progressRect, "", 18f, Color.white);
            progressSteps.enableAutoSizing = true;
            progressSteps.fontSizeMin = 13f;
            progressSteps.fontSizeMax = 18f;
            progressSteps.textWrappingMode = TextWrappingModes.NoWrap;
            Fill(progressSteps.rectTransform, new Vector2(16f, -44f), new Vector2(-16f, -70f));

            var bar = NewRect("Bar", progressRect);
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.offsetMin = new Vector2(16f, 8f);
            bar.offsetMax = new Vector2(-16f, 14f);
            bar.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

            progressFill = NewRect("Fill", bar);
            progressFill.anchorMin = Vector2.zero;
            progressFill.anchorMax = new Vector2(0f, 1f);
            progressFill.offsetMin = progressFill.offsetMax = Vector2.zero;
            progressFill.gameObject.AddComponent<Image>().color = presenter.Accent;
        }

        // 오른쪽 아래에 서 있는 캐릭터 (프레임 없이 투명 배경 그림 그대로. 그림이 없으면 직군 첫 글자를 원 안에)
        private void BuildCharacter(Transform root)
        {
            characterRect = NewRect("Character", root);
            character = characterRect.gameObject.AddComponent<RawImage>();
            character.raycastTarget = false;

            fallback = NewRect("Fallback", characterRect).gameObject.AddComponent<RawImage>();
            fallback.texture = CircleTexture();
            fallback.raycastTarget = false;
            Fill(fallback.rectTransform, Vector2.zero, Vector2.zero, true);
            fallbackLetter = NewText("Letter", fallback.transform, "", 80f, InkColor);
            fallbackLetter.alignment = TextAlignmentOptions.Center;
            fallbackLetter.fontStyle = FontStyles.Bold;
            Fill(fallbackLetter.rectTransform, Vector2.zero, Vector2.zero, true);
        }

        private void BuildBubble(Transform root)
        {
            bubble = NewRect("Bubble", root);
            var bg = bubble.gameObject.AddComponent<Image>();
            bg.color = BubbleColor;
            var layout = bubble.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 32, 30);
            layout.spacing = 14f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            bubble.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 말풍선 클릭 = 다음 (선택 상태로 남으면 Space/Enter 가 버튼을 누르므로 바로 선택 해제)
            var btn = bubble.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            btn.onClick.AddListener(() =>
            {
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                Next();
            });

            // 꼬리: 45도 돌린 사각형을 말풍선 가장자리에 반쯤 걸쳐 둠
            bubbleTail = NewRect("Tail", bubble);
            bubbleTail.sizeDelta = new Vector2(30f, 30f);
            bubbleTail.localRotation = Quaternion.Euler(0f, 0f, 45f);
            bubbleTail.gameObject.AddComponent<Image>().color = BubbleColor;
            bubbleTail.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            // 이름표: 말풍선 위 테두리에 걸친 탭
            var tag = NewRect("NameTag", bubble);
            nameTagImage = tag.gameObject.AddComponent<Image>();
            tag.anchorMin = tag.anchorMax = new Vector2(0f, 1f);
            tag.pivot = new Vector2(0f, 0.5f);
            tag.anchoredPosition = new Vector2(22f, 0f);
            // 색과 글자는 단계마다 ShowStep 에서 (디렉터 / 분야 담당자)
            var tl = tag.gameObject.AddComponent<HorizontalLayoutGroup>();
            tl.padding = new RectOffset(16, 16, 5, 5);
            tl.childControlWidth = tl.childControlHeight = true;
            var tf = tag.gameObject.AddComponent<ContentSizeFitter>();
            tf.horizontalFit = tf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            tag.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            nameTagText = NewText("Label", tag, "", 21f, InkColor);
            nameTagText.fontStyle = FontStyles.Bold;
            nameTagText.textWrappingMode = TextWrappingModes.NoWrap;

            bubbleText = NewText("Line", bubble, "", 27f, InkColor);
            bubbleText.lineSpacing = 8f;

            BuildPromptBlock(bubble);

            continueMark = NewText("Continue", bubble, "PgDn ▶", 16f, new Color(0.45f, 0.48f, 0.56f));
            continueMark.alignment = TextAlignmentOptions.BottomRight;
            continueMark.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var cr = continueMark.rectTransform;
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(1f, 0f);
            cr.anchoredPosition = new Vector2(-14f, 6f);
            cr.sizeDelta = new Vector2(120f, 24f);
        }

        // ④ 단계 블록: [막연한 요청] [구체적인 요청] + 좋은 요청의 4요소 (용어가 들어 있는지는 4요소 칸에서만 표시)
        private void BuildPromptBlock(Transform parent)
        {
            var block = NewRect("PromptBlock", parent);
            promptBlock = block.gameObject;
            promptGroup = promptBlock.AddComponent<CanvasGroup>();
            var vl = promptBlock.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 12f;
            vl.childControlWidth = vl.childControlHeight = true;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;

            cardsRow = NewRect("Cards", block);
            var cards = cardsRow;
            var hl = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 16f;
            hl.childControlWidth = hl.childControlHeight = true;
            hl.childForceExpandWidth = hl.childForceExpandHeight = true;

            badText = Card(cards, "막연한 요청", new Color32(190, 50, 50, 255), new Color32(253, 232, 232, 255));
            goodText = Card(cards, "구체적인 요청", new Color32(30, 130, 70, 255), new Color32(224, 246, 230, 255));

            var title = NewText("ElementsTitle", block, "좋은 요청의 4요소", 19f, InkColor);
            title.fontStyle = FontStyles.Bold;

            var row = NewRect("Elements", block);
            var rl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rl.spacing = 10f;
            rl.childControlWidth = rl.childControlHeight = true;
            rl.childForceExpandWidth = rl.childForceExpandHeight = true;

            string[] names = { "대상", "기능 용어", "조건·수치", "내 환경" };
            for (int i = 0; i < names.Length; i++)
            {
                var chip = NewRect("Element" + i, row);
                EqualWidth(chip);
                elementBgs[i] = chip.gameObject.AddComponent<Image>();
                var cl = chip.gameObject.AddComponent<VerticalLayoutGroup>();
                cl.padding = new RectOffset(12, 12, 8, 8);
                cl.spacing = 2f;
                cl.childControlWidth = cl.childControlHeight = true;
                cl.childForceExpandWidth = true;
                cl.childForceExpandHeight = false;
                NewText("Name", chip, $"<b>{names[i]}</b>", 18f, InkColor);
                elementNotes[i] = NewText("Note", chip, "", 15f, new Color(0.3f, 0.32f, 0.38f));
            }
        }

        private static TextMeshProUGUI Card(Transform parent, string header, Color headerColor, Color bg)
        {
            var card = NewRect(header, parent);
            EqualWidth(card);
            card.gameObject.AddComponent<Image>().color = bg;
            var vl = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(16, 16, 10, 12);
            vl.spacing = 4f;
            vl.childControlWidth = vl.childControlHeight = true;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            var h = NewText("Header", card, header, 18f, headerColor);
            h.fontStyle = FontStyles.Bold;
            return NewText("Body", card, "", 21f, InkColor);
        }

        // 아래쪽 키 안내 (작게)
        private void BuildKeyGuide(Transform root)
        {
            keyGuideRect = NewRect("KeyGuide", root);
            keyGuideRect.anchorMin = keyGuideRect.anchorMax = keyGuideRect.pivot = Vector2.zero;
            keyGuideRect.anchoredPosition = new Vector2(16f, 0f); // 맨 아래 (데모 버튼 줄은 y 25 부터)
            keyGuideRect.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var hl = keyGuideRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(12, 12, 2, 2);
            hl.childControlWidth = hl.childControlHeight = true;
            var fit = keyGuideRect.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var text = NewText("Text", keyGuideRect, LectureMode.KeyGuide, 15f, new Color(0.82f, 0.85f, 0.9f));
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        // 가로 줄 안의 칸들을 글자 길이와 상관없이 같은 너비로
        private static void EqualWidth(RectTransform rt)
        {
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 1f;
            le.flexibleWidth = 1f;
        }

        // ───── UI 헬퍼 ─────

        private static Texture2D circleTexture;

        // 이름 첫 글자를 넣을 원 (가장자리를 부드럽게)
        private static Texture2D CircleTexture()
        {
            if (circleTexture != null) return circleTexture;
            const int size = 128;
            circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    byte a = (byte)(Mathf.Clamp01(r - d) * 255f);
                    px[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            circleTexture.SetPixels32(px);
            circleTexture.Apply();
            return circleTexture;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color)
        {
            var tmp = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = GlossaryInfoPanel.Font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.richText = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        // stretch = false: 부모 위쪽에 붙은 가로 띠 (y 는 위에서부터의 거리, 음수)
        // stretch = true : 부모 전체를 채움
        // 두 경우 모두 topLeft = 왼쪽·위 여백, bottomRight = 오른쪽·아래 여백
        private static void Fill(RectTransform rt, Vector2 topLeft, Vector2 bottomRight, bool stretch = false)
        {
            if (stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(topLeft.x, bottomRight.y);
                rt.offsetMax = new Vector2(bottomRight.x, topLeft.y);
                return;
            }
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(topLeft.x, bottomRight.y);
            rt.offsetMax = new Vector2(bottomRight.x, topLeft.y);
        }
    }
}
