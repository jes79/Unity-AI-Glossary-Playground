using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // Kinematic Rigidbody 를 MoveRotation 으로 회전 (회전 망치 등). 물리적으로 다른 물체를 밀어냄
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicSpinner : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 90f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        private void FixedUpdate() =>
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, degreesPerSecond * Time.fixedDeltaTime, 0f));
    }
}
