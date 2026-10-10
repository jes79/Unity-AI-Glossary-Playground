using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // World Space Canvas 체력바: 적의 자식으로 머리 위에 붙어 있고, LateUpdate 에서 카메라를 바라보게(빌보드) 회전
    [RequireComponent(typeof(Canvas))]
    public class WorldHealthBar : MonoBehaviour
    {
        // 데모 비교 토글: 모든 체력바에 공통 적용
        public static bool Billboard = true;

        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float smoothSpeed = 4f;

        private Camera cam;
        private float target = 1f;

        private void Awake()
        {
            cam = Camera.main;
            GetComponent<Canvas>().worldCamera = cam;
        }

        public void SetRatio(float ratio, string text = null)
        {
            target = Mathf.Clamp01(ratio);
            if (text != null && label != null) label.text = text;
        }

        // 이동·회전이 끝난 뒤(LateUpdate)에 카메라 방향으로 맞춰야 떨림이 없음
        private void LateUpdate()
        {
            fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, target, smoothSpeed * Time.deltaTime);

            if (Billboard && cam != null)
                transform.rotation = cam.transform.rotation; // 카메라와 같은 방향 = 항상 정면이 보임
            else
                transform.localRotation = Quaternion.identity; // 끄면 적의 회전을 그대로 따라감 (옆에서 보면 납작해짐)
        }
    }
}
