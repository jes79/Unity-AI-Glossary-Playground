using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 피격 시 카메라 쉐이크 (Cinemachine Impulse)
    // - CinemachineImpulseSource.GenerateImpulseWithForce(force) 로 "충격 신호"를 보내면
    // - CinemachineCamera 에 붙은 CinemachineImpulseListener 가 신호를 받아 카메라를 흔듦
    // - 흔들림 길이 = ImpulseDefinition.ImpulseDuration (요청값 0.2초)
    public class ScreenShakeDemo : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private Transform player;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Transform enemy;

        [Header("쉐이크")]
        [SerializeField] private bool shakeEnabled = true;
        [SerializeField, Range(0.05f, 1f)] private float shakeDuration = 0.2f;
        [SerializeField, Range(0f, 3f)] private float force = 0.6f;
        [SerializeField] private float attackInterval = 2.5f;

        private static readonly CinemachineImpulseDefinition.ImpulseShapes[] Shapes =
        {
            CinemachineImpulseDefinition.ImpulseShapes.Bump, CinemachineImpulseDefinition.ImpulseShapes.Recoil,
            CinemachineImpulseDefinition.ImpulseShapes.Explosion, CinemachineImpulseDefinition.ImpulseShapes.Rumble
        };
        private int shapeIndex;
        private int hitCount;
        private Vector3 enemyHome;

        private void Start()
        {
            enemyHome = enemy.position;
            StartCoroutine(EnemyLoop());
        }

        private void Update()
        {
            if (DemoInput.JumpPressed) Hit();
            if (DemoInput.KeyDown(Key.Digit1)) shakeEnabled = !shakeEnabled;
            if (DemoInput.KeyDown(Key.UpArrow)) force = Mathf.Min(3f, force + 0.2f);
            if (DemoInput.KeyDown(Key.DownArrow)) force = Mathf.Max(0f, force - 0.2f);
            if (DemoInput.KeyDown(Key.S)) shapeIndex = (shapeIndex + 1) % Shapes.Length;

            GlossaryInfoPanel.Status = $"카메라 쉐이크: {(shakeEnabled ? "ON" : "OFF")}   피격 {hitCount}회\n" +
                                       $"세기(force): {force:0.0}   길이: {shakeDuration:0.00}초   모양: {Shapes[shapeIndex]}";
        }

        // 피격 처리: 플레이어 붉게 깜빡 + Impulse 발생
        public void Hit()
        {
            hitCount++;
            StartCoroutine(Flash());
            if (!shakeEnabled || impulseSource == null) return;

            var def = impulseSource.ImpulseDefinition;
            def.ImpulseDuration = shakeDuration;
            def.ImpulseShape = Shapes[shapeIndex];
            impulseSource.GenerateImpulseWithForce(force); // DefaultVelocity 방향 × force 만큼 흔듦
        }

        private IEnumerator EnemyLoop()
        {
            var wait = new WaitForSeconds(attackInterval);
            while (true)
            {
                yield return wait;
                // 적이 플레이어에게 돌진 → 닿는 순간 Hit → 제자리로 복귀
                Vector3 target = Vector3.Lerp(enemyHome, player.position, 0.82f);
                yield return Ease.Run(0.18f, Ease.InQuad, t => enemy.position = Vector3.Lerp(enemyHome, target, t));
                Hit();
                yield return Ease.Run(0.5f, Ease.OutCubic, t => enemy.position = Vector3.Lerp(target, enemyHome, t));
            }
        }

        private IEnumerator Flash()
        {
            if (playerRenderer == null) yield break;
            var mat = playerRenderer.material;
            mat.SetColor("_BaseColor", new Color(1f, 0.25f, 0.25f));
            yield return new WaitForSeconds(0.12f);
            mat.SetColor("_BaseColor", new Color(0.31f, 0.67f, 1f));
        }
    }
}
