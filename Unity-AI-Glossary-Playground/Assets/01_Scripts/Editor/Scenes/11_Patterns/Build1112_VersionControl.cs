using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1112_VersionControl
{
    [GlossaryScene("1112")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1112_VersionControl")]
    public static void Build()
    {
        var ctx = NewSceneUI("1112",
            "← / → : Git 설정 단계 페이지   아래 버튼 : 미니 Git 체험 (수정 → 커밋 → 되돌아가기)\n" +
            "왼쪽 아래 = 이 프로젝트의 실제 .gitignore 핵심 줄 (씬을 빌드할 때 파일에서 읽어 옴)\n" +
            "메뉴 Tools > Glossary > Patterns > Git > Check Version Control Setup : 설정 검사");
        var root = ctx.Canvas.transform;

        // ── 빌드 시점 검사 결과 (에디터 API 로 읽은 값) ──
        var checklist = string.Join("\n", GitSetupChecker.Checklist());
        InfoBox(root, "Checklist", new Vector2(-90f, 250f), new Vector2(540f, 330f), "이 프로젝트 검사 결과", checklist, 19f);

        // ── 실제 .gitignore 하이라이트 ──
        InfoBox(root, "GitignoreHighlights", new Vector2(-545f, -290f), new Vector2(790f, 470f), ".gitignore 핵심 줄 (실제 파일)",
            GitSetupChecker.GitignoreHighlights(), 18f);

        // ── 단계별 안내 ──
        PatternsBuildUtil.CreateGuide(root, "GitGuide", new Vector2(560f, 230f), new Vector2(760f, 560f),
            ("1. 저장소 만들기",
             "• 프로젝트 폴더에서 <color=#9BE89B>git init</color> (또는 GitHub Desktop 으로 Add Local Repository)\n" +
             "• Unity 설정 확인: <b>Project Settings > Editor</b>\n" +
             "  - Version Control Mode = <b>Visible Meta Files</b>\n" +
             "  - Asset Serialization = <b>Force Text</b> (씬·프리팹이 글자로 저장 → 비교·병합 가능)\n" +
             "• <b>.meta 파일은 반드시 같이 커밋</b> (에셋 GUID 가 들어 있어 빠지면 참조가 끊김)"),
            (".gitignore (유니티용)",
             "• GitHub 의 <b>Unity.gitignore</b> 템플릿을 저장소 최상위에 <color=#9BE89B>.gitignore</color> 로 저장\n" +
             "• 핵심: <color=#9BE89B>[Ll]ibrary/  [Tt]emp/  [Oo]bj/  [Bb]uild/  [Ll]ogs/  [Uu]ser[Ss]ettings/</color>\n" +
             "  → Unity 가 다시 만드는 폴더라 올리면 용량만 커지고 충돌이 잦아짐\n" +
             "• <color=#9BE89B>*.csproj  *.sln  .vs/</color> → IDE 가 자동 생성\n" +
             "• 이미 올라간 파일은 무시되지 않음 → <color=#9BE89B>git rm -r --cached Library</color>\n" +
             "• Unity 프로젝트가 하위 폴더면 .gitignore 경로도 맞춰야 함 (이 저장소는 `**/[Ll]ibrary/` 처럼 ** 사용)"),
            ("Git LFS (큰 파일)",
             "• 이미지·사운드·3D 모델 같은 큰 바이너리는 Git 기록을 무겁게 함 → <b>Git LFS</b> 로 따로 저장\n" +
             "• <color=#9BE89B>git lfs install</color>\n" +
             "• <color=#9BE89B>git lfs track \"*.psd\" \"*.png\" \"*.wav\" \"*.mp3\" \"*.fbx\" \"*.ttf\"</color>\n" +
             "• 위 명령이 .gitattributes 에 다음처럼 기록:\n" +
             "  <color=#9BE89B>*.psd filter=lfs diff=lfs merge=lfs -text</color>\n" +
             "• <color=#9BE89B>git add .gitattributes</color> 를 <b>다른 파일보다 먼저</b> 커밋\n" +
             "• GitHub 무료 LFS 용량·대역폭 제한 확인"),
            ("일상 작업 흐름",
             "• <color=#9BE89B>git status</color>  무엇이 바뀌었나\n" +
             "• <color=#9BE89B>git add .</color>  →  <color=#9BE89B>git commit -m \"점프 버퍼 추가\"</color>\n" +
             "• <color=#9BE89B>git push</color>  원격(GitHub)에 백업\n" +
             "• 기능마다 브랜치: <color=#9BE89B>git switch -c feature/inventory</color>\n" +
             "• 커밋은 작게, 메시지는 '무엇을 왜'\n" +
             "• 아래 미니 Git 버튼으로 수정 → 커밋 → 예전 시점 보기를 체험해 보세요"),
            ("씬·프리팹 충돌 줄이기",
             "• 같은 씬을 두 사람이 동시에 고치면 충돌이 잦음 → 작업을 프리팹 단위로 나누기\n" +
             "• Unity 의 <b>UnityYAMLMerge</b>(Smart Merge)를 merge tool 로 등록하면 YAML 병합이 쉬워짐\n" +
             "• 커밋 전 Unity 에서 <b>File > Save Project</b> (변경이 디스크에 써졌는지)\n" +
             "• Library 를 지운 뒤 다시 열어도 잘 열리는지 = 커밋이 완전한지 확인하는 방법"));

        // ── 미니 Git 시뮬레이터 (대체 구현) ──
        var gitPanel = Panel(root, "MiniGit", new Vector2(430f, -330f), new Vector2(960f, 360f), DarkPanel);
        CreateText("Title", gitPanel.transform, new Vector2(0f, 152f), new Vector2(920f, 36f), "미니 Git (개념 체험용 시뮬레이터)", 22f, TitleColor, TextAlignmentOptions.Left).fontStyle = FontStyles.Bold;
        var timeline = CreateRect("Timeline", gitPanel.transform, new Vector2(0f, 60f), new Vector2(920f, 110f));
        var line = CreateImage("Line", timeline, Vector2.zero, new Vector2(900f, 4f), new Color(1f, 1f, 1f, 0.2f));
        line.raycastTarget = false;
        var head = Panel(timeline, "HEAD", Vector2.zero, new Vector2(80f, 30f), new Color32(220, 90, 80, 255));
        CreateText("Text", head.transform, Vector2.zero, new Vector2(80f, 30f), "HEAD", 18f, Color.white).fontStyle = FontStyles.Bold;
        var status = CreateText("Status", gitPanel.transform, new Vector2(0f, -40f), new Vector2(920f, 90f), "", 19f, TextColor, TextAlignmentOptions.TopLeft);
        status.richText = true;
        var btns = ButtonRow(gitPanel.transform, new Vector2(0f, -135f), 170f, 54f, 19f, "파일 수정", "add + commit", "이전 커밋으로", "최신으로", "Library 추가?");

        var sim = gitPanel.gameObject.AddComponent<MiniGitSimulator>();
        Set(sim, "timeline", timeline);
        Set(sim, "headMarker", head.rectTransform);
        Set(sim, "statusText", status);
        Set(sim, "nodeSprite", CircleSprite);
        OnClick(btns[0], sim.ModifyFile);
        OnClick(btns[1], sim.Commit);
        OnClick(btns[2], sim.CheckoutPrevious);
        OnClick(btns[3], sim.CheckoutLatest);
        OnClick(btns[4], sim.TryAddLibrary);

        Save(ctx);
    }
}
