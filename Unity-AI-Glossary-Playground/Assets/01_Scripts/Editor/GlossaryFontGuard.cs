using TMPro;
using UnityEditor;
using UnityEngine;

// 프로젝트를 열 때 한글 폰트가 쓸 수 있는 상태인지 확인하고, 끊겨 있으면 바로 고친다.
//
// 왜 필요한가: 데모가 쓰는 GlossaryFont 는 동적(Dynamic) TMP 폰트라서 원본 ttf 파일이 있어야
// 글자(글리프)를 만들어 낸다. 원본 malgun.ttf 는 재배포 라이선스 때문에 git 에 올리지 않으므로,
// 저장소를 새로 받은 사람의 프로젝트에서는 폰트 에셋만 있고 원본이 없는 상태가 된다.
// 이때 TMP 는 글리프를 하나도 못 만들고 '에러 없이 글자만 안 보이는' 상태가 되어
// 모든 씬의 정보 패널(용어·조작법·상태)이 빈 칸으로 보인다.
[InitializeOnLoad]
public static class GlossaryFontGuard
{
    private const string FontAssetPath = "Assets/Resources/Fonts/GlossaryFont.asset";

    static GlossaryFontGuard()
    {
        // 에디터가 켜진 직후에는 AssetDatabase 가 아직 준비되지 않았을 수 있어 한 프레임 뒤에 확인
        EditorApplication.delayCall += Check;
    }

    private static void Check()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null)
        {
            GlossaryKit.EnsureKoreanFont();
            return;
        }
        if (fontAsset.sourceFontFile != null) return;

        Debug.Log("[Glossary] 한글 폰트 원본(ttf)이 없어 글자가 안 보이는 상태입니다. 시스템 폰트에서 다시 가져옵니다…");
        var repaired = GlossaryKit.EnsureKoreanFont();
        if (repaired == null || repaired.sourceFontFile == null)
            Debug.LogWarning("[Glossary] 한글 폰트를 고치지 못했습니다. 한글 ttf 를 Assets/06_Fonts 에 넣고 " +
                             "Tools > Glossary > Setup Korean Font 를 실행하세요");
    }
}
