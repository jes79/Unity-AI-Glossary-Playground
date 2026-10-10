using UnityEngine;

namespace GlossaryDemo.UIDemo
{
    // 미니맵 데모용 간단 이동: WASD 로 이동, 진행 방향으로 몸을 돌림 (CharacterController 로 장애물 충돌)
    [RequireComponent(typeof(CharacterController))]
    public class MinimapPlayerMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float turnSpeed = 540f;

        private CharacterController cc;

        private void Awake() => cc = GetComponent<CharacterController>();

        private void Update()
        {
            Vector2 input = DemoInput.Move;
            Vector3 move = new Vector3(input.x, 0f, input.y);
            if (move.sqrMagnitude > 0.01f)
            {
                var target = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
            }
            cc.Move((move * moveSpeed + Vector3.down * 5f) * Time.deltaTime);
        }
    }
}
