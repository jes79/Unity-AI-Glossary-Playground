using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 월드 공간 글자(TextMeshPro)가 항상 카메라 쪽을 보도록 회전 (카메라가 움직이는 데모용)
    public class WorldLabelBillboard : MonoBehaviour
    {
        private Camera cam;

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            // 카메라와 같은 방향을 보게 하면 글자가 뒤집히지 않음
            transform.rotation = cam.transform.rotation;
        }
    }
}
