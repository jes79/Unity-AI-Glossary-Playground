using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 월드 글자가 카메라를 향하게 (물리 데모 공용)
    public class PhysicsBillboard : MonoBehaviour
    {
        private Camera cam;

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam != null) transform.rotation = cam.transform.rotation;
        }
    }
}
