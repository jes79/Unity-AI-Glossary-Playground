using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0817_Interaction
{
    [GlossaryScene("0817")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0817_Interaction")]
    public static void Build()
    {
        var ctx = NewScene3D("0817", "WASD : 이동   E : 상호작용   R : 상자 다시 닫기\n문 · 상자 · NPC 에 가까이 가면 'E 상호작용' 표시가 뜸\n셋 다 IInteractable 인터페이스 → 플레이어 코드는 하나 (Interactor)\n문은 열고 닫기, 상자는 한 번만 열림(골드), NPC 는 대사 넘기기");
        EnemyAIBuildUtil.TopCamera(ctx, 16f, -12f, 52f, 1f);
        int interactable = LayerMask.NameToLayer("Interactable");
        var wallColor = new Color32(110, 116, 132, 255);

        // ── 문 (벽 사이) ──
        Prim(PrimitiveType.Cube, "Wall_L", new Vector3(1.5f, 1.5f, 6f), new Vector3(7f, 3f, 0.4f), wallColor);
        Prim(PrimitiveType.Cube, "Wall_R", new Vector3(10.5f, 1.5f, 6f), new Vector3(7f, 3f, 0.4f), wallColor);
        var door = new GameObject("Door (IInteractable)");
        door.transform.position = new Vector3(6f, 0f, 6f);
        door.layer = interactable;
        var trigger = door.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.25f, 0f);
        trigger.size = new Vector3(2.5f, 2.5f, 2.5f);
        var hinge = new GameObject("Hinge").transform;
        hinge.SetParent(door.transform, false);
        hinge.localPosition = new Vector3(-1f, 0f, 0f);
        Prim(PrimitiveType.Cube, "DoorPanel", new Vector3(1f, 1.25f, 0f), new Vector3(2f, 2.5f, 0.2f), new Color32(150, 100, 60, 255), hinge);
        var doorComp = door.AddComponent<InteractableDoor>();
        Set(doorComp, "hinge", hinge);

        // ── 상자 ──
        var chest = new GameObject("Chest (IInteractable)");
        chest.transform.position = new Vector3(-5f, 0f, 1f);
        chest.layer = interactable;
        var ct = chest.AddComponent<BoxCollider>();
        ct.isTrigger = true;
        ct.center = new Vector3(0f, 0.5f, 0f);
        ct.size = new Vector3(2f, 1.2f, 1.6f);
        Prim(PrimitiveType.Cube, "Base", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 0.9f), new Color32(160, 110, 50, 255), chest.transform);
        var lidHinge = new GameObject("LidHinge").transform;
        lidHinge.SetParent(chest.transform, false);
        lidHinge.localPosition = new Vector3(0f, 0.8f, -0.45f);
        var lid = Prim(PrimitiveType.Cube, "Lid", new Vector3(0f, 0.12f, 0.45f), new Vector3(1.45f, 0.25f, 0.95f), new Color32(190, 140, 60, 255), lidHinge);
        Object.DestroyImmediate(lid.GetComponent<Collider>());
        var chestComp = chest.AddComponent<InteractableChest>();
        Set(chestComp, "lid", lidHinge);

        // ── NPC ──
        var npc = Prim(PrimitiveType.Capsule, "NPC 촌장 (IInteractable)", new Vector3(5f, 1f, -3f), Vector3.one, new Color32(90, 200, 120, 255));
        npc.layer = interactable;
        EnemyAIBuildUtil.Label(npc.transform, "촌장", 1.5f, 3.5f, new Color(0.6f, 1f, 0.7f));
        npc.AddComponent<InteractableNpc>();

        var player = EnemyAIBuildUtil.Player(new Vector3(-1f, 0f, -6f));

        // ── UI: 상호작용 표시 + 대화창 ──
        ctx.Canvas = CreateCanvas("Canvas");
        var root = ctx.Canvas.transform;
        var prompt = Panel(root, "InteractPrompt", Vector2.zero, new Vector2(240f, 54f), new Color32(15, 17, 22, 220));
        var promptText = Label(prompt.transform, "Text", Vector2.zero, new Vector2(230f, 50f), "E 상호작용", 24f, TextAlignmentOptions.Center);
        prompt.gameObject.SetActive(false);
        var dialog = Panel(root, "DialogPanel", new Vector2(CenterX, 110f), new Vector2(1100f, 160f), new Color32(15, 17, 22, 235));
        AnchorBottom(dialog.rectTransform);
        var dialogText = Label(dialog.transform, "Text", Vector2.zero, new Vector2(1050f, 140f), "", 26f, TextAlignmentOptions.Left);
        dialog.gameObject.SetActive(false);

        var interactor = player.gameObject.AddComponent<Interactor>();
        SetLayerMask(interactor, "interactableMask", LayerMask.GetMask("Interactable"));
        Set(interactor, "promptUI", prompt.rectTransform);
        Set(interactor, "promptText", promptText);
        Set(interactor, "dialogText", dialogText);
        Set(interactor, "dialogPanel", dialog.gameObject);

        Save(ctx);
    }
}
