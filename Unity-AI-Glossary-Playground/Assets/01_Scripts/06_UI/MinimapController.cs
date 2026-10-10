using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 미니맵 = 위에서 내려다보는 직교 카메라가 Render Texture 에 그리고, UI 의 RawImage 가 그 텍스처를 표시
    // - 미니맵 카메라 Culling Mask: Ground + Obstacle + Minimap(아이콘)  → 실제 캐릭터 대신 아이콘만 보임
    // - 메인 카메라 Culling Mask: Minimap 레이어 제외 → 아이콘은 메인 화면에 안 보임
    public class MinimapController : MonoBehaviour
    {
        [Header("카메라")]
        [SerializeField] private Camera minimapCamera;  // targetTexture = Render Texture
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform player;
        [SerializeField] private float height = 30f;
        [SerializeField] private Vector3 mainCameraOffset = new Vector3(0f, 13f, -10f);

        [Header("미니맵 설정")]
        [SerializeField, Range(5f, 40f)] private float zoom = 14f;   // orthographicSize (반지름처럼 생각)
        [SerializeField] private bool rotateWithPlayer;
        [SerializeField] private bool circleShape = true;

        [Header("UI")]
        [SerializeField] private RectTransform minimapFrame;
        [SerializeField] private Image maskImage;         // Mask 가 붙은 이미지 (스프라이트 모양대로 잘림)
        [SerializeField] private Sprite circleSprite;
        [SerializeField] private Sprite squareSprite;
        [SerializeField] private RectTransform northArrow;
        [SerializeField] private TextMeshProUGUI optionLabel;

        private bool bigMap;
        private Vector2 smallSize, smallPos, smallAnchor;

        private void Start()
        {
            smallSize = minimapFrame.sizeDelta;
            smallPos = minimapFrame.anchoredPosition;
            smallAnchor = minimapFrame.anchorMin;
            ApplyShape();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) ToggleRotate();
            if (DemoInput.KeyDown(Key.C)) ToggleShape();
            if (DemoInput.KeyDown(Key.M)) ToggleBigMap();
            if (DemoInput.KeyHeld(Key.Z)) zoom = Mathf.Max(5f, zoom - 15f * Time.deltaTime);
            if (DemoInput.KeyHeld(Key.X)) zoom = Mathf.Min(40f, zoom + 15f * Time.deltaTime);
            float scroll = DemoInput.Scroll;
            if (Mathf.Abs(scroll) > 0.01f) zoom = Mathf.Clamp(zoom - Mathf.Sign(scroll) * 1.5f, 5f, 40f);

            GlossaryInfoPanel.Status = $"미니맵 orthographicSize {zoom:0.0}   회전: {(rotateWithPlayer ? "플레이어 방향" : "북쪽 고정")}   모양: {(circleShape ? "원" : "사각형")}\n" +
                                       $"Render Texture {minimapCamera.targetTexture.width}x{minimapCamera.targetTexture.height}   큰 지도: {(bigMap ? "ON" : "OFF")}";
        }

        private void LateUpdate()
        {
            // 미니맵 카메라는 플레이어 머리 위에서 수직으로 내려다봄
            minimapCamera.transform.position = player.position + Vector3.up * height;
            float yaw = rotateWithPlayer ? player.eulerAngles.y : 0f;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
            minimapCamera.orthographicSize = bigMap ? zoom * 2f : zoom;

            // 북쪽 표시: 지도가 돌면 북쪽 화살표도 반대로 돔
            if (northArrow != null) northArrow.localRotation = Quaternion.Euler(0f, 0f, yaw);

            mainCamera.transform.position = player.position + mainCameraOffset;
            mainCamera.transform.LookAt(player.position);
        }

        public void ToggleRotate()
        {
            rotateWithPlayer = !rotateWithPlayer;
            RefreshLabel();
        }

        public void ToggleShape()
        {
            circleShape = !circleShape;
            ApplyShape();
        }

        // 큰 지도: 같은 Render Texture 를 화면 가운데 크게 표시 + 더 넓게 찍음
        public void ToggleBigMap()
        {
            bigMap = !bigMap;
            if (bigMap)
            {
                minimapFrame.anchorMin = minimapFrame.anchorMax = new Vector2(0.5f, 0.5f);
                minimapFrame.anchoredPosition = new Vector2(150f, 0f);
                minimapFrame.sizeDelta = new Vector2(760f, 760f);
            }
            else
            {
                minimapFrame.anchorMin = minimapFrame.anchorMax = smallAnchor;
                minimapFrame.anchoredPosition = smallPos;
                minimapFrame.sizeDelta = smallSize;
            }
        }

        private void ApplyShape()
        {
            maskImage.sprite = circleShape ? circleSprite : squareSprite;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (optionLabel != null)
                optionLabel.text = $"R : {(rotateWithPlayer ? "플레이어 방향 회전" : "북쪽 고정")}   C : {(circleShape ? "원형" : "사각형")}   Z/X·휠 : 줌   M : 큰 지도";
        }
    }
}
