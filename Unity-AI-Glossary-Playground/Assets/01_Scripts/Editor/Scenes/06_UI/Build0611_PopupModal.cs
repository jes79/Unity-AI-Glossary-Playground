using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0611_PopupModal
{
    [GlossaryScene("0611")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0611_PopupModal")]
    public static void Build()
    {
        var ctx = NewSceneUI("0611",
            "Q 또는 버튼 : '정말 나가시겠습니까?' 팝업   (삭제·구매도 같은 팝업 재사용)\n팝업에서 Enter = 확인, ESC = 취소\n" +
            "M : 모달 차단막 OFF → 팝업이 떠 있어도 뒤 버튼이 눌리는 문제 확인");
        var root = ctx.Canvas.transform;

        var items = Body(root, "ItemText", new Vector2(230f, 330f), new Vector2(900f, 50f), "", 30f, TextAlignmentOptions.Center, TitleColor);
        var actions = ButtonColumn(root, new Vector2(230f, 220f), 420f, 76f, 26f, "게임 나가기 (Q)", "아이템 삭제", "물약 구매");

        // 팝업 뒤에 깔린 버튼 (모달 확인용)
        var back = CreateButton("BackgroundButton", root, new Vector2(230f, -290f), new Vector2(560f, 90f), "뒤 버튼 (모달이면 안 눌림)", new Color32(90, 95, 115, 255), 26f);
        var counter = Body(root, "BackgroundCounter", new Vector2(230f, -365f), new Vector2(560f, 40f), "뒤 버튼 클릭 수: 0", 22f, TextAlignmentOptions.Center);

        InfoBox(root, "UsageInfo", new Vector2(760f, 120f), new Vector2(340f, 420f), "재사용 방법",
            "팝업 프리팹 하나 +\n<color=#9BE89B>Show(제목, 메시지,\n  확인 콜백, 취소 콜백)</color>\n\n" +
            "• 메시지와 할 일(Action)만\n  바꿔서 어디서나 호출\n" +
            "• 화면 전체를 덮는\n  반투명 <b>Blocker</b> 이미지가\n  뒤쪽 클릭을 막음 = 모달\n" +
            "• 열 때 OutBack 스케일\n  (Time.unscaledDeltaTime)", 19f);

        var modalBtn = CreateButtonRow(root, 60f, "모달 차단막 ON (M)")[0];

        // ── 팝업 (전체 화면 루트 + Blocker + 창) ──
        var popupRoot = CreateRect("ConfirmPopup", root, Vector2.zero, Vector2.zero);
        Stretch(popupRoot);
        var group = popupRoot.gameObject.AddComponent<CanvasGroup>();
        var blocker = CreateImage("Blocker", popupRoot, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.6f));
        Stretch(blocker.rectTransform);
        var window = Panel(popupRoot, "Window", new Vector2(100f, 0f), new Vector2(720f, 380f), new Color32(40, 44, 58, 255));
        var title = CreateText("Title", window.transform, new Vector2(0f, 130f), new Vector2(660f, 60f), "제목", 36f, TitleColor);
        title.fontStyle = FontStyles.Bold;
        var message = CreateText("Message", window.transform, new Vector2(0f, 20f), new Vector2(660f, 140f), "메시지", 28f, Color.white);
        message.richText = true;
        var confirm = CreateButton("ConfirmButton", window.transform, new Vector2(-150f, -120f), new Vector2(240f, 72f), "확인", new Color32(220, 90, 80, 255));
        var cancel = CreateButton("CancelButton", window.transform, new Vector2(150f, -120f), new Vector2(240f, 72f), "취소", new Color32(80, 88, 110, 255));

        var popup = popupRoot.gameObject.AddComponent<ConfirmPopup>();
        Set(popup, "group", group);
        Set(popup, "blocker", blocker);
        Set(popup, "window", window.rectTransform);
        Set(popup, "titleText", title);
        Set(popup, "messageText", message);
        Set(popup, "confirmLabel", LabelOf(confirm));
        Set(popup, "cancelLabel", LabelOf(cancel));
        OnClick(confirm, popup.Confirm);
        OnClick(cancel, popup.Cancel);

        var demo = new GameObject("PopupDemo").AddComponent<PopupDemo>();
        Set(demo, "popup", popup);
        Set(demo, "itemText", items);
        Set(demo, "backgroundCounter", counter);
        Set(demo, "modalLabel", LabelOf(modalBtn));
        OnClick(actions[0], demo.AskQuit);
        OnClick(actions[1], demo.AskDelete);
        OnClick(actions[2], demo.AskBuy);
        OnClick(back, demo.BackgroundClick);
        OnClick(modalBtn, demo.ToggleModal);

        Save(ctx);
    }
}
