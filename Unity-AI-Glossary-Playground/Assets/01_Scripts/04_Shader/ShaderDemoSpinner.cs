using UnityEngine;

namespace GlossaryDemo.ShaderDemo
{
    // 셰이더 데모에서 물체를 천천히 돌리고(rotation) 위아래로 띄우는(bob) 단순 연출
    // Paused 로 멈출 수 있음 (얼음 상태, 회전 비교 등)
    public class ShaderDemoSpinner : MonoBehaviour
    {
        [SerializeField] private Vector3 degreesPerSecond = new Vector3(0f, 30f, 0f);
        [SerializeField] private bool worldSpace = true;
        [SerializeField] private float bobHeight;
        [SerializeField] private float bobSpeed = 1.5f;

        public bool Paused { get; set; }

        private Vector3 basePos;
        private float bobTime;

        private void Awake() => basePos = transform.localPosition;

        private void Update()
        {
            if (Paused) return;
            transform.Rotate(degreesPerSecond * Time.deltaTime, worldSpace ? Space.World : Space.Self);
            if (bobHeight > 0f)
            {
                bobTime += Time.deltaTime;
                transform.localPosition = basePos + Vector3.up * (Mathf.Sin(bobTime * bobSpeed) * bobHeight);
            }
        }
    }
}
