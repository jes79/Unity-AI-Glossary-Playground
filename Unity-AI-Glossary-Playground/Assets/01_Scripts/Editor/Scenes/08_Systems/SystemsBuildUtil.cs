using System;
using System.Collections.Generic;
using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;

// 08_Systems 빌더 공통 헬퍼: 아이템 데이터(ScriptableObject) 에셋 + 아이콘 생성, UI 조각
public static class SystemsBuildUtil
{
    public const string Sub = "Systems";

    // 데모 UI 를 놓을 영역의 중심 (왼쪽 위 정보 패널을 피해 오른쪽으로)
    public const float CenterX = 300f;

    // ───────────────────────── 아이템 에셋 ─────────────────────────

    private struct ItemDef
    {
        public string Id, Name, Desc;
        public ItemType Type;
        public ItemRarity Rarity;
        public int Price, Stack, Heal, Mana, Atk, Def;
        public Color Color;
        public Func<float, float, bool> Shape;
    }

    private static Func<float, float, bool> Flask => (u, v) =>
        new Vector2(u - 0.5f, v - 0.38f).magnitude < 0.3f || (Mathf.Abs(u - 0.5f) < 0.09f && v > 0.6f && v < 0.9f);
    private static Func<float, float, bool> Blob => (u, v) =>
        v > 0.12f && Mathf.Pow((u - 0.5f) / 0.4f, 2f) + Mathf.Pow((v - 0.35f) / 0.42f, 2f) < 1f;
    private static Func<float, float, bool> Sword => (u, v) =>
    {
        // 대각선 칼날 + 손잡이 + 가드
        float along = (u + v) * 0.7071f, across = (v - u) * 0.7071f;
        bool blade = along > 0.55f && along < 1.3f && Mathf.Abs(across) < 0.06f + (1.3f - along) * 0.02f;
        bool guard = Mathf.Abs(along - 0.52f) < 0.04f && Mathf.Abs(across) < 0.2f;
        bool grip = along > 0.25f && along < 0.5f && Mathf.Abs(across) < 0.04f;
        return blade || guard || grip;
    };
    private static Func<float, float, bool> Shield => (u, v) =>
    {
        if (v < 0.08f || v > 0.9f) return false;
        float halfW = v > 0.5f ? 0.36f : 0.36f * Mathf.Sqrt(Mathf.Max(0f, (v - 0.08f) / 0.42f));
        return Mathf.Abs(u - 0.5f) < halfW;
    };
    private static Func<float, float, bool> Gem => (u, v) => Mathf.Abs(u - 0.5f) * 1.2f + Mathf.Abs(v - 0.5f) < 0.4f;

    private static ItemDef[] Defs => new[]
    {
        new ItemDef { Id = "potion_hp", Name = "체력 포션", Desc = "마시면 HP 를 30 회복한다.", Type = ItemType.Consumable, Price = 50, Stack = 99, Heal = 30, Color = new Color(0.9f, 0.25f, 0.3f), Shape = Flask },
        new ItemDef { Id = "potion_mp", Name = "마나 포션", Desc = "마시면 MP 를 20 회복한다.", Type = ItemType.Consumable, Price = 40, Stack = 99, Mana = 20, Color = new Color(0.3f, 0.5f, 1f), Shape = Flask },
        new ItemDef { Id = "slime_jelly", Name = "슬라임 젤리", Desc = "슬라임이 떨어뜨린 말랑한 재료.", Type = ItemType.Material, Price = 5, Stack = 99, Color = new Color(0.4f, 0.85f, 0.4f), Shape = Blob },
        new ItemDef { Id = "sword_wood", Name = "나무 검", Desc = "초보자용 검.", Type = ItemType.Weapon, Price = 100, Stack = 1, Atk = 5, Color = new Color(0.7f, 0.5f, 0.3f), Shape = Sword },
        new ItemDef { Id = "sword_steel", Name = "강철 검", Desc = "단단한 강철로 만든 검.", Type = ItemType.Weapon, Rarity = ItemRarity.Rare, Price = 400, Stack = 1, Atk = 12, Color = new Color(0.8f, 0.85f, 0.95f), Shape = Sword },
        new ItemDef { Id = "sword_legend", Name = "전설의 검", Desc = "용을 베었다는 전설의 검.", Type = ItemType.Weapon, Rarity = ItemRarity.Legendary, Price = 2000, Stack = 1, Atk = 30, Color = new Color(1f, 0.78f, 0.25f), Shape = Sword },
        new ItemDef { Id = "armor_leather", Name = "가죽 갑옷", Desc = "가벼운 가죽 방어구.", Type = ItemType.Armor, Price = 120, Stack = 1, Def = 3, Color = new Color(0.6f, 0.4f, 0.25f), Shape = Shield },
        new ItemDef { Id = "armor_plate", Name = "판금 갑옷", Desc = "무겁지만 튼튼하다.", Type = ItemType.Armor, Rarity = ItemRarity.Rare, Price = 600, Stack = 1, Def = 10, Color = new Color(0.7f, 0.75f, 0.85f), Shape = Shield },
        new ItemDef { Id = "gem", Name = "빛나는 보석", Desc = "비싸게 팔 수 있는 보석.", Type = ItemType.Material, Rarity = ItemRarity.Legendary, Price = 1000, Stack = 99, Color = new Color(0.4f, 1f, 0.95f), Shape = Gem },
    };

    // 아이템 데이터 에셋 생성/갱신 (Assets/09_Generated/Systems/Items/*.asset). 이미 있으면 값만 덮어씀
    public static Dictionary<string, ItemData> EnsureItems()
    {
        var result = new Dictionary<string, ItemData>();
        string folder = $"{GeneratedFolder}/{Sub}/Items";
        EnsureFolder(folder);
        foreach (var d in Defs)
        {
            string path = $"{folder}/{d.Id}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(item, path);
            }
            item.id = d.Id;
            item.itemName = d.Name;
            item.description = d.Desc;
            item.type = d.Type;
            item.rarity = d.Rarity;
            item.price = d.Price;
            item.maxStack = d.Stack;
            item.healAmount = d.Heal;
            item.manaAmount = d.Mana;
            item.attackBonus = d.Atk;
            item.defenseBonus = d.Def;
            item.icon = Icon($"Item_{d.Id}", d.Color, d.Shape);
            EditorUtility.SetDirty(item);
            result[d.Id] = item;
        }
        AssetDatabase.SaveAssets();
        return result;
    }

    // 모양 함수로 외곽선이 있는 아이콘 그리기 (64px)
    public static Sprite Icon(string name, Color color, Func<float, float, bool> shape)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteFolder}/{name}.png");
        if (existing != null) return existing;
        const float o = 0.035f;
        return CreateSprite(name, 64, 64, (u, v) =>
        {
            if (shape(u, v))
            {
                float shade = 0.75f + 0.35f * v;
                bool rim = !shape(u + o, v) || !shape(u - o, v) || !shape(u, v + o) || !shape(u, v - o);
                return rim ? Color.Lerp(color, Color.black, 0.45f) : new Color(color.r * shade, color.g * shade, color.b * shade, 1f);
            }
            bool outline = shape(u + o, v) || shape(u - o, v) || shape(u, v + o) || shape(u, v - o);
            return outline ? new Color(0.05f, 0.05f, 0.08f, 1f) : Color.clear;
        }, 64f);
    }

    public static void SetInts(UnityEngine.Object target, string field, params int[] values)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).intValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetFloats(UnityEngine.Object target, string field, params float[] values)
    {
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).floatValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ───────────────────────── UI 조각 ─────────────────────────

    public static Image Panel(Transform parent, string name, Vector2 pos, Vector2 size, Color? color = null)
    {
        var img = CreateImage(name, parent, pos, size, color ?? PanelColor, RoundedSprite, true);
        return img;
    }

    public static TextMeshProUGUI Label(Transform parent, string name, Vector2 pos, Vector2 size, string text, float fontSize = 24f,
        TextAlignmentOptions align = TextAlignmentOptions.Left, Color? color = null)
    {
        var t = CreateText(name, parent, pos, size, text, fontSize, color ?? TextColor, align);
        t.richText = true;
        return t;
    }

    // 가로 채움 막대 (배경 + Filled 이미지) → fill 이미지 반환
    public static Image Bar(Transform parent, string name, Vector2 pos, Vector2 size, Color color, float fill = 1f)
    {
        var bg = CreateImage(name, parent, pos, size, new Color(0f, 0f, 0f, 0.45f), WhiteSprite);
        var f = CreateImage("Fill", bg.transform, Vector2.zero, Vector2.zero, color, WhiteSprite);
        Stretch(f.rectTransform, 2f);
        f.type = Image.Type.Filled;
        f.fillMethod = Image.FillMethod.Horizontal;
        f.fillAmount = fill;
        return f;
    }

    public static Button SmallButton(Transform parent, string name, Vector2 pos, Vector2 size, string label, Color? color = null, float fontSize = 22f)
    {
        var b = CreateButton(name, parent, pos, size, label, color, fontSize);
        return b;
    }

    // 2D 씬용 바닥 발판 (SpriteRenderer + BoxCollider2D, Ground 레이어)
    public static GameObject Platform2D(string name, Vector3 pos, Vector2 size, Transform parent = null)
    {
        var sr = CreateSpriteObject(name, SquareSprite, pos, size, GroundColor, parent);
        sr.gameObject.layer = LayerMask.NameToLayer("Ground");
        sr.gameObject.AddComponent<BoxCollider2D>();
        return sr.gameObject;
    }
}
