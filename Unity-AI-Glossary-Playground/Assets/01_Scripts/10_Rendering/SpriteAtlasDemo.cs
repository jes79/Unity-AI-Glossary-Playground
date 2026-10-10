using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GlossaryDemo.Rendering
{
    // UI 아이콘 48개: 개별 텍스처 16장 vs Sprite Atlas 1장
    // 같은 그림 두 벌을 만들어 한 벌만 Atlas 에 넣었음 → 버튼으로 바꿔 끼우며 비교
    // 아이콘을 살짝 겹쳐 배치 → 텍스처가 다르면 Canvas 가 순서를 바꿔 묶을 수 없어 Draw Call 이 늘어남
    public class SpriteAtlasDemo : MonoBehaviour
    {
        [SerializeField] private SpriteAtlas atlas;
        [SerializeField] private Sprite[] looseSprites;   // Atlas 에 없는 개별 PNG
        [SerializeField] private Sprite[] atlasSprites;   // Atlas 에 들어 있는 PNG (같은 그림)
        [SerializeField] private Image[] icons;
        [SerializeField] private string packerModeInfo;   // 빌더가 EditorSettings.spritePackerMode 를 기록
        [SerializeField] private bool useAtlas = true;

        private ProfilerRecorder batches, setPass;
        private readonly HashSet<Texture> textures = new HashSet<Texture>();

        private void OnEnable()
        {
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        }

        private void OnDisable()
        {
            batches.Dispose();
            setPass.Dispose();
        }

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) Toggle();

            // 지금 아이콘들이 실제로 쓰는 텍스처 수 (Atlas 로 묶였으면 1장)
            textures.Clear();
            foreach (var img in icons) if (img.sprite != null) textures.Add(img.sprite.texture);
            Sprite sample = atlasSprites.Length > 0 ? atlasSprites[0] : null;
            bool packed = sample != null && sample.packed;

            GlossaryInfoPanel.Status =
                $"지금: <b>{(useAtlas ? "Sprite Atlas 의 스프라이트" : "개별 텍스처 스프라이트")}</b>   아이콘 {icons.Length}개\n" +
                $"사용 중인 텍스처 수: <b>{textures.Count}</b>장   Batches {(batches.Valid ? batches.LastValue.ToString() : "N/A")}  SetPass {(setPass.Valid ? setPass.LastValue.ToString() : "N/A")}\n" +
                $"Atlas '{(atlas != null ? atlas.name : "-")}' 스프라이트 {(atlas != null ? atlas.spriteCount : 0)}개   packed = {(packed ? "<color=#8F8>true</color>" : "<color=#FC6>false</color>")}\n" +
                (packed ? "" : $"<color=#FC6>{packerModeInfo}</color>");
        }

        public void Toggle()
        {
            useAtlas = !useAtlas;
            Apply();
        }

        private void Apply()
        {
            var set = useAtlas ? atlasSprites : looseSprites;
            for (int i = 0; i < icons.Length; i++) icons[i].sprite = set[i % set.Length];
        }

        // 런타임에 이름으로 Atlas 에서 꺼내는 방법 (복제본을 반환하므로 한 번만 꺼내서 캐시)
        public Sprite GetFromAtlas(string spriteName) => atlas != null ? atlas.GetSprite(spriteName) : null;
    }
}
