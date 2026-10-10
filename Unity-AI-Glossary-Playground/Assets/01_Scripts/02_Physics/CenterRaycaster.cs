using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 카메라 화면 중앙에서 Raycast
    // - ViewportPointToRay(0.5, 0.5) = 화면 정중앙에서 카메라가 보는 방향으로 나가는 광선
    // - 클릭(또는 Space)하면 맞은 오브젝트 이름을 Debug.Log 로 출력 (Console 창 확인) + 화면 로그
    // - L 키: LayerMask 로 Obstacle 레이어(유리창)를 무시하고 뒤쪽을 맞힘
    public class CenterRaycaster : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float maxDistance = 50f;
        [SerializeField] private float lookSpeed = 0.15f;
        [SerializeField] private Transform hitMarker;
        [SerializeField] private LineRenderer rayLine;
        [SerializeField] private Transform gunTip;
        [SerializeField] private TextMeshProUGUI aimText;
        [SerializeField] private TextMeshProUGUI logText;

        private bool ignoreGlass;
        private float yaw, pitch;
        private readonly Queue<string> log = new Queue<string>();
        private float lineTimer;

        private void Start()
        {
            yaw = cam.transform.eulerAngles.y;
            pitch = Mathf.DeltaAngle(0f, cam.transform.eulerAngles.x);
        }

        private void Update()
        {
            Look();
            if (DemoInput.KeyDown(Key.L)) ignoreGlass = !ignoreGlass;

            int mask = ignoreGlass ? ~LayerMask.GetMask("Obstacle") : ~0; // ~ = 비트 반전 → "Obstacle 만 빼고 전부"
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // ★ 핵심 한 줄: 맞으면 true, 맞은 정보는 hit 에 담김
            bool didHit = Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask, QueryTriggerInteraction.Ignore);

            if (aimText != null)
                aimText.text = didHit ? $"{hit.collider.name}  ({hit.distance:0.0} m)" : "(없음)";
            if (hitMarker != null)
            {
                hitMarker.gameObject.SetActive(didHit);
                if (didHit) { hitMarker.position = hit.point; hitMarker.rotation = Quaternion.LookRotation(hit.normal); }
            }

            if (PhysicsClick.LeftWorld || DemoInput.JumpPressed)
            {
                string msg = didHit ? $"Raycast 맞음: {hit.collider.name}" : "Raycast: 아무것도 안 맞음";
                Debug.Log(msg); // 요청 예문: 맞은 오브젝트 이름 출력
                log.Enqueue($"{Time.time:0.0}s  {msg}");
                while (log.Count > 8) log.Dequeue();
                if (logText != null) logText.text = "<b>Debug.Log 출력</b>\n" + string.Join("\n", log);
                if (rayLine != null && gunTip != null)
                {
                    rayLine.SetPosition(0, gunTip.position);
                    rayLine.SetPosition(1, didHit ? hit.point : ray.GetPoint(maxDistance));
                    lineTimer = 0.1f;
                }
            }
            lineTimer -= Time.deltaTime;
            if (rayLine != null) rayLine.enabled = lineTimer > 0f;

            GlossaryInfoPanel.Status = $"LayerMask: {(ignoreGlass ? "Obstacle(유리) 제외" : "전부")}   최대 거리: {maxDistance} m\n" +
                                       (didHit ? $"맞은 점: {hit.point:F1}   법선: {hit.normal:F1}" : "맞은 것 없음");
        }

        // 오른쪽 버튼 드래그 또는 방향키로 둘러보기
        private void Look()
        {
            Vector2 d = DemoInput.RightHeld ? DemoInput.MouseDelta * lookSpeed : Vector2.zero;
            var kb = Keyboard.current;
            if (kb != null)
            {
                float k = 90f * Time.deltaTime;
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) d.x -= k;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) d.x += k;
                if (kb.upArrowKey.isPressed || kb.wKey.isPressed) d.y += k;
                if (kb.downArrowKey.isPressed || kb.sKey.isPressed) d.y -= k;
            }
            yaw += d.x;
            pitch = Mathf.Clamp(pitch - d.y, -60f, 60f);
            cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
