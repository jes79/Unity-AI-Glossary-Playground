using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // Joint 데모 관리
    // - HingeJoint 문: 경첩(세로 축) 하나로 문틀에 연결. 플레이어가 밀면 열림
    //     useLimits(각도 제한 0~100°) / useSpring(놓으면 스스로 닫힘, 도어 클로저) 를 키로 전환
    // - SpringJoint 추: 천장과 용수철로 연결, 클릭해서 당기면 출렁임
    // - 밧줄: 작은 마디들을 HingeJoint 로 줄줄이 연결
    // 마우스 클릭: 클릭한 Rigidbody 를 화면 안쪽으로 밀기
    public class JointDemo : MonoBehaviour
    {
        [SerializeField] private HingeJoint door;
        [SerializeField] private float pushImpulse = 6f;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.L)) door.useLimits = !door.useLimits;
            if (DemoInput.KeyDown(Key.C)) door.useSpring = !door.useSpring;

            if (PhysicsClick.LeftWorld)
            {
                Ray ray = DemoInput.MouseRay();
                if (Physics.Raycast(ray, out var hit, 200f) && hit.rigidbody != null && !hit.rigidbody.isKinematic)
                    hit.rigidbody.AddForceAtPosition(ray.direction * pushImpulse, hit.point, ForceMode.Impulse);
            }

            GlossaryInfoPanel.Status = $"문 각도: {door.angle:0}°   각도 제한(L): {(door.useLimits ? $"ON {door.limits.min:0}~{door.limits.max:0}°" : "OFF")}\n" +
                                       $"자동 닫힘 Spring(C): {(door.useSpring ? "ON" : "OFF")}";
        }
    }
}
