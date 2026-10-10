using UnityEngine;

namespace GlossaryDemo.EnemyAI
{
    // 머리 위 이름표(TextMeshPro)가 항상 카메라를 보게 함
    public class AIFaceCamera : MonoBehaviour
    {
        private Camera cam;

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam != null) transform.rotation = cam.transform.rotation;
        }
    }
}
