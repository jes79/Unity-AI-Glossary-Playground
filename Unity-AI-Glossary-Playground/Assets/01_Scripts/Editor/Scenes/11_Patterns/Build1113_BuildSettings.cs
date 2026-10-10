using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1113_BuildSettings
{
    [GlossaryScene("1113")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1113_BuildSettings")]
    public static void Build()
    {
        var ctx = NewSceneUI("1113",
            "위 탭 / ← → : PC · Android · WebGL → itch.io 순서 안내\n왼쪽 아래 = 지금 실행 환경 (런타임 API), 오른쪽 아래 = 씬을 만들 때 읽은 에디터 빌드 설정\n" +
            "메뉴 Tools > Glossary > Patterns > Build > Print Build Settings / Open Build Profiles Window");
        var root = ctx.Canvas.transform;

        var guide = PatternsBuildUtil.CreateGuide(root, "BuildGuide", new Vector2(520f, 110f), new Vector2(860f, 600f),
            ("PC (Windows) 빌드",
             "1. <b>File > Build Profiles</b> (Unity 6) 를 열고 <b>Windows</b> 선택\n" +
             "2. <b>Scene List</b> 에 넣을 씬 확인 — 0번 씬이 처음 실행됨 (이 프로젝트: 0000_Hub)\n" +
             "3. <b>Player Settings</b>: Company / Product Name, 아이콘, 해상도(Fullscreen Mode)\n" +
             "4. <b>Build</b> → 빈 폴더(예: Builds/Windows) 선택\n" +
             "5. 결과: 실행 파일(.exe) + <color=#FFD54F>_Data 폴더 + UnityPlayer.dll</color> → 셋을 함께 압축해서 배포\n" +
             "6. Development Build 체크 시 화면 왼쪽 아래에 로그·프로파일러 연결 가능"),
            ("Android 1 — 준비",
             "1. Unity Hub > Installs > 버전 옆 톱니바퀴 > <b>Add modules</b> > <b>Android Build Support</b> (SDK·NDK·OpenJDK 포함)\n" +
             "2. Build Profiles 에서 <b>Android</b> 선택 → <b>Switch Platform</b> (에셋 재임포트로 시간이 걸림)\n" +
             "3. 휴대폰: 개발자 옵션 > USB 디버깅 ON → USB 연결\n" +
             "4. Run Device 에서 폰 선택 → <b>Build And Run</b>"),
            ("Android 2 — Player Settings",
             "• Package Name: <color=#9BE89B>com.회사.게임</color> (스토어에서 유일, 나중에 못 바꿈)\n" +
             "• Minimum API Level / Target API Level (스토어 요구 버전 확인)\n" +
             "• Scripting Backend: <b>IL2CPP</b>, Target Architectures: <b>ARM64</b> (Play 스토어 필수)\n" +
             "• Publishing Settings: <b>Keystore</b> 생성 → 비밀번호 잘 보관 (잃어버리면 업데이트 불가)\n" +
             "• 스토어 업로드는 <b>Build App Bundle (.aab)</b> 체크, 테스트는 .apk"),
            ("WebGL 1 — 준비",
             "1. Unity Hub > Add modules > <b>Web Build Support</b>\n" +
             "2. Build Profiles 에서 <b>Web</b>(WebGL) 선택 → <b>Switch Platform</b>\n" +
             "3. Scene List 확인\n" +
             "4. 주의: 웹은 스레드·파일 접근이 제한되고, 첫 로딩이 큼 → 텍스처·사운드 압축 확인\n" +
             "5. 한글 폰트: 이 프로젝트처럼 동적 폰트면 원본 ttf 가 빌드에 포함됨 → 용량·<color=#FFB74D>라이선스</color> 확인"),
            ("WebGL 2 — Player Settings",
             "• Resolution and Presentation: Default Canvas <b>960 x 600</b> (itch.io 임베드 크기와 맞춤)\n" +
             "• WebGL Template: Default 또는 Minimal\n" +
             "• Publishing Settings:\n" +
             "  - Compression Format: <b>Gzip</b> (또는 Brotli)\n" +
             "  - <b>Decompression Fallback 체크</b> — itch.io 는 서버 압축 헤더를 못 바꾸므로 이게 없으면\n" +
             "    'Unable to parse Build/xxx.framework.js.gz' 오류가 남 (또는 Compression Disabled)\n" +
             "• Data Caching 체크 → 두 번째 방문부터 빠름"),
            ("WebGL 3 — 빌드",
             "1. <b>Build</b> → 빈 폴더 선택 (예: Builds/WebGL)\n" +
             "2. 결과 폴더 구조:\n" +
             "   <color=#9BE89B>index.html</color>\n   <color=#9BE89B>Build/</color>  (.data .framework.js .loader.js .wasm)\n   <color=#9BE89B>TemplateData/</color>\n" +
             "3. index.html 을 그냥 더블클릭하면 브라우저 보안 때문에 안 열림 →\n" +
             "   <b>Build And Run</b> 으로 로컬 서버에서 테스트"),
            ("itch.io 1 — 업로드",
             "1. 결과 폴더 <b>안의 내용물</b>을 zip 으로 압축 (<color=#FFB74D>index.html 이 zip 최상위</color>에 있어야 함)\n" +
             "2. itch.io 로그인 > Dashboard > <b>Create new project</b>\n" +
             "3. Kind of project: <b>HTML</b>\n" +
             "4. Uploads: zip 업로드 → <b>This file will be played in the browser</b> 체크\n" +
             "5. 파일 크기 제한 확인 (zip 안 개별 파일 크기 제한도 있음)"),
            ("itch.io 2 — 공개",
             "1. Embed options: Viewport <b>960 x 600</b> (빌드 캔버스와 같게)\n" +
             "2. <b>Fullscreen button</b> 체크, 모바일 대응이면 Mobile friendly\n" +
             "3. 제목·설명·스크린샷·태그 입력\n" +
             "4. Visibility: 먼저 <b>Draft</b> 로 저장 → View page 로 직접 플레이 테스트\n" +
             "5. 문제 없으면 <b>Public</b> 으로 바꾸고 저장 → 링크 공유\n" +
             "6. 업데이트: 새 zip 을 올리고 예전 파일은 삭제(또는 butler 도구로 자동 업로드)"));

        // 탭 (해당 페이지로 이동)
        var tabs = ButtonRow(root, new Vector2(520f, 455f), 200f, 56f, 22f, "PC", "Android", "WebGL", "itch.io");
        int[] tabPages = { 0, 1, 3, 6 };
        for (int i = 0; i < tabs.Length; i++) OnClick(tabs[i], guide.Show, tabPages[i]);

        // 런타임 정보 + 에디터 스냅샷
        var runtime = InfoBox(root, "RuntimeInfo", new Vector2(-560f, -300f), new Vector2(760f, 420f), "지금 실행 환경 (런타임 API)", "", 19f);
        var snapshot = InfoBox(root, "EditorSnapshot", new Vector2(520f, -385f), new Vector2(860f, 250f), "에디터 빌드 설정 (씬 빌드 시점, BuildSettingsTools.Summary)", "", 18f);

        var display = new GameObject("BuildInfoDisplay").AddComponent<BuildInfoDisplay>();
        Set(display, "runtimeText", runtime);
        Set(display, "editorSnapshotText", snapshot);
        SetString(display, "editorSnapshot", BuildSettingsTools.Summary());

        Save(ctx);
    }
}
