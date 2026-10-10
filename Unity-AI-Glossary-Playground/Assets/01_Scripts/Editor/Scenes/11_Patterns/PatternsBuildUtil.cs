using GlossaryDemo;
using GlossaryDemo.Patterns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

// 11_Patterns 빌더 공용 헬퍼
public static class PatternsBuildUtil
{
    // 단계별 안내 패널 (제목 + 본문 + 페이지 표시 + 이전/다음 버튼). pages 는 (제목, 본문) 쌍
    public static GuidePages CreateGuide(Transform parent, string name, Vector2 pos, Vector2 size, params (string title, string body)[] pages)
    {
        var panel = Panel(parent, name, pos, size, DarkPanel);
        var title = CreateText("Title", panel.transform, Vector2.zero, Vector2.zero, "", 28f, TitleColor, TextAlignmentOptions.TopLeft);
        Stretch(title.rectTransform);
        title.rectTransform.offsetMin = new Vector2(24f, 20f);
        title.rectTransform.offsetMax = new Vector2(-24f, -16f);
        title.fontStyle = FontStyles.Bold;

        var body = CreateText("Body", panel.transform, Vector2.zero, Vector2.zero, "", 21f, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(body.rectTransform);
        body.rectTransform.offsetMin = new Vector2(24f, 78f);
        body.rectTransform.offsetMax = new Vector2(-24f, -62f);
        body.richText = true;

        var pageText = CreateText("Page", panel.transform, Vector2.zero, new Vector2(300f, 40f), "", 20f, SubTextColor);
        Anchor(pageText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f));

        var prev = CreateButton("PrevButton", panel.transform, Vector2.zero, new Vector2(130f, 50f), "◀ 이전", new Color32(70, 80, 105, 255), 22f);
        Anchor((RectTransform)prev.transform, Vector2.zero, Vector2.zero, new Vector2(20f, 16f));
        var next = CreateButton("NextButton", panel.transform, Vector2.zero, new Vector2(130f, 50f), "다음 ▶", null, 22f);
        Anchor((RectTransform)next.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 16f));

        var guide = panel.gameObject.AddComponent<GuidePages>();
        Set(guide, "titleText", title);
        Set(guide, "bodyText", body);
        Set(guide, "pageText", pageText);
        var titles = new string[pages.Length];
        var bodies = new string[pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            titles[i] = pages[i].title;
            bodies[i] = pages[i].body;
        }
        SetStrings(guide, "titles", titles);
        SetStrings(guide, "bodies", bodies);
        OnClick(prev, guide.Prev);
        OnClick(next, guide.Next);
        return guide;
    }

    // 3D 씬용: 위에서 비스듬히 보는 카메라 위치 지정
    public static void LookAt(Camera cam, Vector3 position, Vector3 target)
    {
        cam.transform.position = position;
        cam.transform.LookAt(target);
    }
}
