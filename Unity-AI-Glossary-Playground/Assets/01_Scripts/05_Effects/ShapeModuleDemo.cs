using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Shape 모듈: 입자가 "어디서, 어느 방향으로" 나오는지 정하는 모양
    // Cone(원뿔: 각도로 퍼짐) / Sphere(사방) / Edge(선분) / Box(상자 부피)
    public class ShapeModuleDemo : MonoBehaviour
    {
        public enum ShapeKind { Cone, Sphere, Edge, Box }

        [SerializeField] private ParticleSystem ps;
        [SerializeField] private ShapeKind kind = ShapeKind.Cone;
        [SerializeField, Range(0f, 90f)] private float coneAngle = 30f;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) SetKind(0);
            if (DemoInput.KeyDown(Key.Digit2)) SetKind(1);
            if (DemoInput.KeyDown(Key.Digit3)) SetKind(2);
            if (DemoInput.KeyDown(Key.Digit4)) SetKind(3);

            string desc = kind switch
            {
                ShapeKind.Cone => $"Cone: 반지름 0.3 원판에서 {coneAngle:0}° 벌어지며 위로 → 굴뚝 연기",
                ShapeKind.Sphere => "Sphere: 구 표면에서 바깥 모든 방향으로 → 폭발, 오라",
                ShapeKind.Edge => "Edge: 선분 위 아무 곳에서나 → 칼날, 바닥 틈새 연기",
                _ => "Box: 상자 부피 안에서 → 비/눈 영역, 먼지",
            };
            GlossaryInfoPanel.Status = $"Shape = {kind}\n{desc}";
        }

        public void SetKind(int k) { kind = (ShapeKind)k; Apply(); }
        public void SetConeAngle(float a) { coneAngle = a; Apply(); }

        private void Apply()
        {
            var shape = ps.shape;
            shape.rotation = new Vector3(-90f, 0f, 0f); // 모양의 +Z 를 위(+Y)로 돌림
            shape.scale = Vector3.one;
            switch (kind)
            {
                case ShapeKind.Cone:
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = coneAngle;
                    shape.radius = 0.3f;
                    break;
                case ShapeKind.Sphere:
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 0.5f;
                    break;
                case ShapeKind.Edge:
                    shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
                    shape.radius = 2f; // Edge 는 radius 가 선분 반 길이
                    shape.rotation = Vector3.zero;
                    break;
                default:
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(3f, 3f, 0.5f);
                    break;
            }
        }
    }
}
