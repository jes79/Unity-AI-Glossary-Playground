using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // [SerializeField] / [Header] / [Range] 등 Inspector 정리용 속성 모음
    // 플레이 중 Hierarchy 에서 이 오브젝트를 선택하고 Inspector 를 보면서 값을 바꿔 보세요 (화면 슬라이더와 연동됨)
    public class InspectorAttributeDemo : MonoBehaviour
    {
        [Header("이동")]
        [Tooltip("원을 그리며 도는 속도 (도/초). 마우스를 올리면 이 설명이 뜸")]
        [SerializeField, Range(0f, 180f)] private float orbitSpeed = 45f;   // private 이지만 Inspector 에 보임
        [SerializeField, Range(0.5f, 5f)] private float orbitRadius = 2.5f;

        [Header("통통 튀기")]
        [SerializeField, Range(0f, 2f)] private float bobHeight = 0.5f;
        [SerializeField, Min(0.1f)] private float bobPeriod = 1f;           // 0.1 보다 작게 입력하면 0.1 로 고정

        [Space(12)]
        [Header("외형")]
        [SerializeField] private Color bodyColor = new Color(0.35f, 0.65f, 1f);
        [SerializeField, Range(0.3f, 2f)] private float size = 1f;
        [SerializeField, TextArea(2, 4)] private string memo = "여러 줄 메모는 TextArea 로.\n줄바꿈이 됩니다.";

        [Header("자동 프로퍼티 노출")]
        [field: SerializeField] public int Level { get; private set; } = 1; // 프로퍼티의 숨은 필드를 노출

        // ── 비교용 ──
        public float publicField = 1f;          // public: 보이지만 다른 스크립트가 마음대로 바꿀 수 있음 (권장 X)
        [HideInInspector] public int hiddenValue = 7; // public 이지만 Inspector 에서 숨김
        private float notShown = 3f;             // 그냥 private: Inspector 에 안 보임

        [SerializeField] private Renderer body;

        private float angle;
        private Vector3 center;
        private MaterialPropertyBlock block;

        public float OrbitSpeed { get => orbitSpeed; set => orbitSpeed = Mathf.Clamp(value, 0f, 180f); }
        public float OrbitRadius { get => orbitRadius; set => orbitRadius = Mathf.Clamp(value, 0.5f, 5f); }
        public float BobHeight { get => bobHeight; set => bobHeight = Mathf.Clamp(value, 0f, 2f); }
        public float Size { get => size; set => size = Mathf.Clamp(value, 0.3f, 2f); }

        private void Awake()
        {
            center = transform.position;
            block = new MaterialPropertyBlock();
        }

        private void Start()
        {
#if UNITY_EDITOR
            // 에디터에서 실행하면 이 오브젝트를 자동 선택 → Inspector 에 Header/Range 가 바로 보임
            UnityEditor.Selection.activeGameObject = gameObject;
#endif
        }

        private void Update()
        {
            angle += orbitSpeed * Time.deltaTime;
            float rad = angle * Mathf.Deg2Rad;
            float bob = Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI / bobPeriod)) * bobHeight;
            transform.position = center + new Vector3(Mathf.Cos(rad) * orbitRadius, bob, Mathf.Sin(rad) * orbitRadius);
            transform.localScale = Vector3.one * size;

            block.SetColor("_BaseColor", bodyColor);
            body.SetPropertyBlock(block);

            GlossaryInfoPanel.Status = $"orbitSpeed {orbitSpeed:0}   orbitRadius {orbitRadius:0.0}   bobHeight {bobHeight:0.00}   size {size:0.00}\n" +
                                       $"Level {Level}   publicField {publicField}   (hiddenValue {hiddenValue}, notShown {notShown} 은 Inspector 에 없음)";
        }

        // Inspector 에서 값이 바뀔 때마다 호출 (에디터 전용). 값 검증에 사용
        private void OnValidate()
        {
            bobPeriod = Mathf.Max(0.1f, bobPeriod);
        }

        // 컴포넌트 ⋮ 메뉴(또는 우클릭)에 항목 추가
        [ContextMenu("기본값으로 되돌리기")]
        private void ResetValues()
        {
            orbitSpeed = 45f;
            orbitRadius = 2.5f;
            bobHeight = 0.5f;
            bobPeriod = 1f;
            size = 1f;
            bodyColor = new Color(0.35f, 0.65f, 1f);
        }

        public void ResetFromButton() => ResetValues();
    }
}
