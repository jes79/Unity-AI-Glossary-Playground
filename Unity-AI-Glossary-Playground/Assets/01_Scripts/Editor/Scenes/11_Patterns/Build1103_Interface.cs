using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1103_Interface
{
    [GlossaryScene("1103")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1103_Interface")]
    public static void Build()
    {
        var ctx = NewScene3D("1103",
            "WASD : 이동   E : 상호작용\n문·상자·NPC·레버는 서로 다른 클래스지만 모두 IInteractable 을 구현\n" +
            "플레이어는 GetComponent<IInteractable>() 하나로 전부 처리", true, 24f);
        PatternsBuildUtil.LookAt(ctx.Camera, new Vector3(0f, 13f, -11f), new Vector3(0f, 0f, 0.5f));

        int interactable = LayerMask.NameToLayer("Interactable");
        var wood = ColorMat(new Color(0.55f, 0.36f, 0.2f));
        var metal = ColorMat(new Color(0.5f, 0.52f, 0.58f));

        // ── 문 ──
        var door = new GameObject("Door (DoorInteractable)");
        door.transform.position = new Vector3(-5f, 0f, 3.5f);
        Prim(PrimitiveType.Cube, "PostL", new Vector3(-1.15f, 1.4f, 0f), new Vector3(0.3f, 2.8f, 0.4f), Color.gray, door.transform).GetComponent<Renderer>().sharedMaterial = metal;
        Prim(PrimitiveType.Cube, "PostR", new Vector3(1.15f, 1.4f, 0f), new Vector3(0.3f, 2.8f, 0.4f), Color.gray, door.transform).GetComponent<Renderer>().sharedMaterial = metal;
        var hinge = new GameObject("Hinge").transform;
        hinge.SetParent(door.transform, false);
        hinge.localPosition = new Vector3(-1f, 0f, 0f);
        var panel = Prim(PrimitiveType.Cube, "DoorPanel", new Vector3(1f, 1.3f, 0f), new Vector3(2f, 2.6f, 0.15f), Color.white, hinge);
        panel.GetComponent<Renderer>().sharedMaterial = wood;
        panel.layer = interactable;
        var doorI = door.AddComponent<DoorInteractable>();
        Set(doorI, "hinge", hinge);

        // ── 상자 ──
        var chest = new GameObject("Chest (ChestInteractable)");
        chest.transform.position = new Vector3(5f, 0f, 3f);
        var cbase = Prim(PrimitiveType.Cube, "Base", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 1f), Color.white, chest.transform);
        cbase.GetComponent<Renderer>().sharedMaterial = wood;
        cbase.layer = interactable;
        var lidHinge = new GameObject("LidHinge").transform;
        lidHinge.SetParent(chest.transform, false);
        lidHinge.localPosition = new Vector3(0f, 0.8f, 0.5f);
        var lid = Prim(PrimitiveType.Cube, "Lid", new Vector3(0f, 0.12f, -0.5f), new Vector3(1.45f, 0.25f, 1.05f), new Color(0.75f, 0.6f, 0.2f), lidHinge);
        Object.DestroyImmediate(lid.GetComponent<Collider>());
        var chestI = chest.AddComponent<ChestInteractable>();
        Set(chestI, "lid", lidHinge);

        // ── NPC ──
        var npc = Prim(PrimitiveType.Capsule, "NPC (NpcInteractable)", new Vector3(-4.5f, 1f, -2.5f), Vector3.one, new Color(0.4f, 0.8f, 0.45f));
        npc.layer = interactable;
        var nnose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.white, npc.transform);
        Object.DestroyImmediate(nnose.GetComponent<Collider>());
        npc.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        var npcI = npc.AddComponent<NpcInteractable>();
        SetString(npcI, "npcName", "대장장이");
        SetStrings(npcI, "lines", "어서 오게! 무기를 고치러 왔나?", "인터페이스? 나도 IInteractable 이라네.", "문이든 상자든 나든, 자네는 E 하나로 부르면 되지.");

        // ── 레버 + 조명 ──
        var lever = new GameObject("Lever (LeverInteractable)");
        lever.transform.position = new Vector3(4.5f, 0f, -2.5f);
        var lbase = Prim(PrimitiveType.Cube, "Base", new Vector3(0f, 0.25f, 0f), new Vector3(0.8f, 0.5f, 0.6f), Color.white, lever.transform);
        lbase.GetComponent<Renderer>().sharedMaterial = metal;
        lbase.layer = interactable;
        var handle = new GameObject("HandlePivot").transform;
        handle.SetParent(lever.transform, false);
        handle.localPosition = new Vector3(0f, 0.5f, 0f);
        var stick = Prim(PrimitiveType.Cylinder, "Stick", new Vector3(0f, 0.5f, 0f), new Vector3(0.12f, 0.5f, 0.12f), new Color(0.85f, 0.2f, 0.2f), handle);
        Object.DestroyImmediate(stick.GetComponent<Collider>());

        var lampPost = Prim(PrimitiveType.Cylinder, "LampPost", new Vector3(7f, 1.2f, -4f), new Vector3(0.15f, 1.2f, 0.15f), Color.gray);
        lampPost.GetComponent<Renderer>().sharedMaterial = metal;
        var lamp = Prim(PrimitiveType.Sphere, "Lamp", new Vector3(7f, 2.6f, -4f), Vector3.one * 0.6f, Color.gray);
        Object.DestroyImmediate(lamp.GetComponent<Collider>());
        var lightGo = new GameObject("LampLight");
        lightGo.transform.position = new Vector3(7f, 2.6f, -4f);
        var pl = lightGo.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.color = new Color(1f, 0.85f, 0.45f);
        pl.intensity = 6f;
        pl.range = 9f;

        var leverI = lever.AddComponent<LeverInteractable>();
        Set(leverI, "handle", handle);
        Set(leverI, "targetLight", pl);
        Set(leverI, "lampRenderer", lamp.GetComponent<Renderer>());
        Set(leverI, "lampOn", UnlitMat("Unlit_1103_LampOn", new Color(1f, 0.9f, 0.5f)));
        Set(leverI, "lampOff", ColorMat(new Color(0.3f, 0.3f, 0.33f)));

        // ── 플레이어 ──
        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, 0f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        Object.DestroyImmediate(player.GetComponent<Collider>());
        var cc = player.AddComponent<CharacterController>();
        cc.center = Vector3.zero;
        var pnose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.white, player.transform);
        Object.DestroyImmediate(pnose.GetComponent<Collider>());

        var marker = Prim(PrimitiveType.Sphere, "TargetMarker", Vector3.zero, Vector3.one * 0.35f, new Color(1f, 0.85f, 0.2f));
        Object.DestroyImmediate(marker.GetComponent<Collider>());
        marker.GetComponent<Renderer>().sharedMaterial = UnlitMat("Unlit_1103_Marker", new Color(1f, 0.85f, 0.2f));

        // ── UI ──
        var canvas = CreateCanvas("UI Canvas");
        var prompt = CreateText("Prompt", canvas.transform, new Vector2(0f, -330f), new Vector2(900f, 60f), "", 40f, Color.white);
        prompt.fontStyle = FontStyles.Bold;
        var message = CreateText("Message", canvas.transform, new Vector2(0f, -400f), new Vector2(1400f, 50f), "", 30f, new Color(1f, 0.85f, 0.45f));
        InfoBox(canvas.transform, "CodeInfo", new Vector2(740f, 210f), new Vector2(400f, 480f), "인터페이스 코드",
            "<color=#9BE89B>public interface IInteractable\n{\n  string Prompt { get; }\n  void Interact(InteractorPlayer p);\n}</color>\n\n" +
            "<color=#9BE89B>class Door : MonoBehaviour,\n             IInteractable</color>\n\n" +
            "플레이어 쪽:\n<color=#9BE89B>var it = hit.GetComponent\n  <IInteractable>();\nit?.Interact(this);</color>\n\n" +
            "<color=#FF8A65>없으면</color>: if (door) … else if (chest) …\n새 종류마다 플레이어 코드 수정", 18f);

        var ip = player.AddComponent<InteractorPlayer>();
        SetLayerMask(ip, "interactableMask", LayerMask.GetMask("Interactable"));
        Set(ip, "promptText", prompt);
        Set(ip, "messageText", message);
        Set(ip, "marker", marker.transform);

        Save(ctx);
    }
}
