using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // 월드 스페이스 UI 데모 제어: 카메라 공전, 클릭으로 적 피해, 빌보드 ON/OFF
    public class WorldSpaceUIDemo : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private Vector3 orbitCenter = Vector3.zero;
        [SerializeField] private float orbitDistance = 13f;
        [SerializeField] private float orbitHeight = 6f;
        [SerializeField] private float orbitSpeed = 70f;
        [SerializeField] private bool autoOrbit = true;
        [SerializeField] private float clickDamage = 20f;
        [SerializeField] private TextMeshProUGUI billboardLabel;

        private float yaw;

        private void Start() => RefreshLabel();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.B)) ToggleBillboard();
            if (DemoInput.KeyDown(Key.R)) autoOrbit = !autoOrbit;

            float input = (DemoInput.KeyHeld(Key.E) ? 1f : 0f) - (DemoInput.KeyHeld(Key.Q) ? 1f : 0f);
            if (autoOrbit && Mathf.Approximately(input, 0f)) input = 0.25f;
            yaw += input * orbitSpeed * Time.deltaTime;

            Vector3 offset = Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, orbitHeight, -orbitDistance);
            cam.transform.position = orbitCenter + offset;
            cam.transform.LookAt(orbitCenter + Vector3.up);

            if (DemoInput.LeftClick && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                if (Physics.Raycast(DemoInput.MouseRay(cam), out var hit, 100f))
                {
                    var enemy = hit.collider.GetComponentInParent<WorldUIEnemy>();
                    if (enemy != null) enemy.Damage(clickDamage);
                }
            }

            GlossaryInfoPanel.Status = $"빌보드(카메라 바라보기): {(WorldHealthBar.Billboard ? "ON" : "<color=#FF6060>OFF</color>")}   카메라 자동 회전: {(autoOrbit ? "ON" : "OFF")}\n" +
                                       "체력바 Canvas: Render Mode = World Space, 크기 1.6 x 0.2 유닛 (scale 0.01)";
        }

        public void ToggleBillboard()
        {
            WorldHealthBar.Billboard = !WorldHealthBar.Billboard;
            RefreshLabel();
        }

        public void ToggleAutoOrbit() => autoOrbit = !autoOrbit;

        private void RefreshLabel()
        {
            if (billboardLabel != null) billboardLabel.text = WorldHealthBar.Billboard ? "빌보드 ON (B)" : "빌보드 OFF (B)";
        }

        private void OnDestroy() => WorldHealthBar.Billboard = true;
    }
}
