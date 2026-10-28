using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GlossaryDemo
{
    // 강의 모드 한 용어 안의 진행 단계
    public enum LectureStep
    {
        Opening,   // 디렉터의 강의 여는 인사 (허브에서 강의를 시작했을 때 첫 용어 앞에 한 번)
        SelfIntro, // ⓪ 분야 소개 (분야가 바뀔 때마다 그 분야 담당자가)
        Intro,     // ① 소개: 용어 이름 + 쉽게 말하면
        Concept,   // ② 개념
        Demo,      // ③ 직접 보기 (말풍선을 줄이고 데모 조작)
        Prompt,    // ④ AI에게 이렇게 요청하기
    }

    // 강의 모드 상태 (static 이라 씬이 바뀌어도 유지됨)
    // - 허브의 [강의 모드로 시작] 또는 F4 로 켜고 끔
    // - 실제 화면(담당자·말풍선·진행 막대)은 각 씬의 GlossaryInfoPanel 이 LectureOverlay 를 붙여서 그림
    public static class LectureMode
    {
        // ───── 강의 키 ─────
        // 데모 스크립트가 쓰는 키(방향키·WASD·Space·숫자·글자 대부분·Enter·Esc·Shift·Tab·F5·F9 등)와 겹치지 않는 키만 고름.
        // 프레젠터 리모컨은 보통 PageDown/PageUp 을 보내므로 그대로 넘기기에 쓸 수 있다.
        public const Key NextKey = Key.PageDown;        // 다음 단계 (마지막 단계면 다음 용어 씬)
        public const Key NextKeyAlt = Key.RightBracket; // ]
        public const Key PrevKey = Key.PageUp;          // 이전 단계 (첫 단계면 이전 용어)
        public const Key PrevKeyAlt = Key.LeftBracket;  // [
        public const Key NextTermKey = Key.End;         // 다음 용어로 바로
        public const Key PrevTermKey = Key.Home;        // 이전 용어로 바로
        public const Key HubKey = Key.Backspace;        // 허브로
        public const Key BubbleKey = Key.F3;            // 말풍선 숨기기/보이기
        public const Key ToggleKey = Key.F4;            // 강의 모드 켜기/끄기
        public const Key QuitKey = Key.F10;             // 강의 플레이어 끝내기 (강의 플레이어 빌드에서만, LectureLaunch)

        private const string BaseKeyGuide =
            "PgDn / ]  다음     PgUp / [  이전     End  다음 용어     Home  이전 용어     F3  말풍선     Backspace  허브     F4  강의 모드 끄기";
        public static string KeyGuide => LectureLaunch.IsLecturePlayer ? BaseKeyGuide + "     F10  끝내기" : BaseKeyGuide;

        private static readonly Key[] AllKeys =
            { NextKey, NextKeyAlt, PrevKey, PrevKeyAlt, NextTermKey, PrevTermKey, HubKey, BubbleKey, ToggleKey };

        // 이번 프레임에 강의 키를 눌렀는가 ("아무 키나 누르세요" 같은 데모 입력에서 강의 키를 빼는 데 사용)
        public static bool LectureKeyPressedThisFrame
        {
            get
            {
                foreach (var key in AllKeys)
                    if (DemoInput.KeyDown(key)) return true;
                return false;
            }
        }

        public static bool Enabled { get; private set; }

        // 지금 진행 중인 용어와 그 안의 단계 번호. 같은 용어 씬을 다시 불러와도(씬 리로드 데모) 이어서 진행함
        public static string CurrentId { get; private set; }
        public static int StepIndex;
        public static bool CurrentHasSelfIntro { get; private set; }
        public static bool CurrentHasOpening { get; private set; }
        public static bool SamePresenterAsBefore { get; private set; } // 앞 분야와 담당자가 같으면 인사를 짧게
        public static bool CurrentIsDirectStart { get; private set; } // 명령줄로 바로 시작한 첫 용어 (① 소개를 인사로 시작)

        // 이 용어에서 ③ 직접 보기에 도달했는가. 시간 제한이 있는 데모(0816 게임 씬)는 이때부터 시계를 돌림
        public static bool DemoReached;
        public static bool DemoMayRun => !Enabled || DemoReached;

        // 데모가 씬 활성화를 기다리는 중(0616 allowSceneActivation = false)에는 강의가 다른 씬으로 넘어가면 안 됨
        // (대기 중인 비동기 로드 뒤에 LoadScene 이 줄을 서서 둘 다 멈춤)
        public static bool SceneChangeLocked;

        // 분야 소개를 마친 분야 번호 (0 = 아직 없음)
        private static int introducedCategory;
        private static bool pendingOpening;
        private static bool pendingDirectStart;

        // 다음 씬에서 시작할 단계 (용어가 바뀔 때 한 번만 사용)
        private static bool pendingLast;
        private static LectureStep? pendingStep;

        public static void SetEnabled(bool on)
        {
            Enabled = on;
            if (!on) CurrentId = null;
        }

        // 허브에서 고른 용어부터 강의 시작 (디렉터 인사 → 분야 소개부터)
        public static void StartAt(GlossaryEntry entry)
        {
            Enabled = true;
            introducedCategory = 0;
            Go(entry, false);
            pendingOpening = true; // Go 다음에 둬야 지워지지 않음
        }

        // 명령줄(-glossaryStart 0105 -glossaryLecture)로 한 용어부터 바로 강의 (LectureLaunch)
        // 디렉터 인사 없이 시작. 분야의 첫 용어면 분야 소개부터, 아니면 ① 소개부터
        public static void StartFromTerm(GlossaryEntry entry)
        {
            Enabled = true;
            introducedCategory = IndexInCategory(entry) == 1 ? 0 : entry.Category;
            Go(entry, false);
            pendingDirectStart = true; // Go 다음에 둬야 지워지지 않음
        }

        // 스크린샷 등에서 특정 단계를 바로 띄울 때 사용
        public static void Prepare(LectureStep? step)
        {
            Enabled = true;
            CurrentId = null;
            introducedCategory = 0;
            pendingStep = step;
            pendingLast = false;
            pendingOpening = step == LectureStep.Opening;
        }

        // 용어 씬으로 이동. atLastStep = 이전 용어로 거꾸로 갈 때 마지막 단계부터
        public static void Go(GlossaryEntry entry, bool atLastStep)
        {
            CurrentId = null; // 새 용어로 취급 → 씬이 열리면 단계를 처음(또는 끝)부터 계산
            pendingLast = atLastStep;
            pendingStep = null;
            pendingOpening = false;
            pendingDirectStart = false;
            Time.timeScale = 1f; // 일시정지 데모에서 넘어가도 멈춰 있지 않게
            if (Application.CanStreamedLevelBeLoaded(entry.Scene))
                SceneManager.LoadScene(entry.Scene);
            else
                Debug.LogWarning($"[Lecture] 씬 '{entry.Scene}' 이 Build Settings 에 없음 (Tools > Glossary > Build All Scenes 실행)");
        }

        public static void GoHub()
        {
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(GlossaryCatalog.HubScene))
                SceneManager.LoadScene(GlossaryCatalog.HubScene);
        }

        // 씬이 열렸을 때 LectureOverlay 가 호출: 이 용어의 단계 목록과 시작 단계를 정함
        public static LectureStep[] BeginEntry(GlossaryEntry entry)
        {
            bool resume = CurrentId == entry.Id;
            if (!resume)
            {
                // 분야의 첫 용어이거나, 다른 분야에서 넘어왔으면 담당자가 먼저 분야 소개
                CurrentHasSelfIntro = IndexInCategory(entry) == 1 || introducedCategory != entry.Category;
                SamePresenterAsBefore = introducedCategory > 0 &&
                                        LecturePresenters.For(introducedCategory).Key == LecturePresenters.For(entry.Category).Key;
                introducedCategory = entry.Category;
                CurrentHasOpening = pendingOpening;
                pendingOpening = false;
                CurrentIsDirectStart = pendingDirectStart;
                pendingDirectStart = false;
                DemoReached = false;
            }

            var steps = StepsFor(entry, CurrentHasOpening, CurrentHasSelfIntro);
            if (!resume)
            {
                CurrentId = entry.Id;
                StepIndex = pendingLast ? steps.Length - 1 : 0;
                if (pendingStep.HasValue)
                {
                    int found = System.Array.IndexOf(steps, pendingStep.Value);
                    if (found >= 0) StepIndex = found;
                }
                pendingLast = false;
                pendingStep = null;
            }
            StepIndex = Mathf.Clamp(StepIndex, 0, steps.Length - 1);
            return steps;
        }

        private static LectureStep[] StepsFor(GlossaryEntry entry, bool opening, bool selfIntro)
        {
            var list = new System.Collections.Generic.List<LectureStep>(6);
            if (opening) list.Add(LectureStep.Opening);
            if (selfIntro) list.Add(LectureStep.SelfIntro);
            list.Add(LectureStep.Intro);
            if (!string.IsNullOrEmpty(entry.Concept)) list.Add(LectureStep.Concept);
            list.Add(LectureStep.Demo);
            list.Add(LectureStep.Prompt);
            return list.ToArray();
        }

        // ───── 카탈로그 순서 계산 ─────

        public static bool TryNeighbor(string id, int dir, out GlossaryEntry entry)
        {
            var all = GlossaryCatalog.Entries;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Id != id) continue;
                int j = i + dir;
                if (j >= 0 && j < all.Length)
                {
                    entry = all[j];
                    return true;
                }
                break;
            }
            entry = default;
            return false;
        }

        public static bool TryFirstOf(int category, out GlossaryEntry entry)
        {
            foreach (var e in GlossaryCatalog.Entries)
            {
                if (e.Category != category) continue;
                entry = e;
                return true;
            }
            entry = default;
            return false;
        }

        // 분야 안에서 몇 번째 용어인가 (1부터)
        public static int IndexInCategory(GlossaryEntry entry)
        {
            int n = 0;
            foreach (var e in GlossaryCatalog.Entries)
            {
                if (e.Category != entry.Category) continue;
                n++;
                if (e.Id == entry.Id) return n;
            }
            return 0;
        }

        public static int CountInCategory(int category)
        {
            int n = 0;
            foreach (var e in GlossaryCatalog.Entries) if (e.Category == category) n++;
            return n;
        }

        // ───── 입력 ─────

        // 입력창(TMP_InputField)에 글자를 치는 중에는 강의 키를 무시 (0604 등)
        public static bool TypingInInputField
        {
            get
            {
                var es = EventSystem.current;
                var go = es != null ? es.currentSelectedGameObject : null;
                if (go == null) return false;
                var input = go.GetComponent<TMP_InputField>();
                return input != null && input.isFocused;
            }
        }
    }
}
