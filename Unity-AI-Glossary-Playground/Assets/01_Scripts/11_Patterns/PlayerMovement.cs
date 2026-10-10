using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // [컴포넌트 분리 1] 이동만 담당
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 720f;

        private CharacterController cc;

        private void Awake() => cc = GetComponent<CharacterController>();

        private void Update()
        {
            Vector2 input = DemoInput.Move;
            Vector3 move = new Vector3(input.x, 0f, input.y);
            if (move.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(move), turnSpeed * Time.deltaTime);
            cc.Move((move * moveSpeed + Vector3.down * 5f) * Time.deltaTime);
        }
    }
}
