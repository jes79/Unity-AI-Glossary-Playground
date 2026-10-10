using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GlossaryDemo.Rendering
{
    // 2D 탑다운 Y축 정렬: 화면에서 아래쪽(y 가 작은) 물체가 앞에 그려지게
    // 같은 Sorting Layer("Characters") 안에서 순서를 정하는 세 가지 방법을 비교
    //  0) OFF            : 플레이어 Order in Layer 를 고정(항상 위) → 나무 뒤로 가도 가려지지 않음 (잘못된 예)
    //  1) 카메라 Custom Axis: camera.transparencySortMode = CustomAxis, axis = (0,1,0)
    //                      → 같은 Layer/Order 끼리는 y 가 클수록 먼저(뒤에) 그림. 코드 0줄 (권장)
    //  2) 코드로 Order    : 매 프레임 sortingOrder = -y*100 (오래된 방식, 어디서나 확실하게 동작)
    // 나무/캐릭터는 여러 스프라이트(줄기+잎, 몸+머리)라서 SortingGroup 으로 묶어 한 덩어리로 정렬
    // SortingGroup 의 기준점 = 루트 위치 → 루트를 "발 밑"에 둠
    public class YSortDemo : MonoBehaviour
    {
        public enum Mode { Off, CameraCustomAxis, CodeSortingOrder }

        [SerializeField] private Camera cam;
        [SerializeField] private Transform player;
        [SerializeField] private SortingGroup[] sortables;   // 플레이어 포함, 정렬 대상 전부
        [SerializeField] private SortingGroup playerGroup;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private Mode mode = Mode.CameraCustomAxis;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Y)) NextMode();
            Vector2 m = DemoInput.Move;
            player.position += (Vector3)(m * (moveSpeed * Time.deltaTime));

            if (mode == Mode.CodeSortingOrder)
                foreach (var g in sortables) g.sortingOrder = Mathf.RoundToInt(-g.transform.position.y * 100f);

            GlossaryInfoPanel.Status =
                $"방식: <b>{Label(mode)}</b>\n" +
                $"카메라 Transparency Sort: {cam.transparencySortMode}  축 {cam.transparencySortAxis}\n" +
                $"플레이어 y {player.position.y:0.00}  Layer '{playerGroup.sortingLayerName}'  Order {playerGroup.sortingOrder}";
        }

        private static string Label(Mode m) => m switch
        {
            Mode.Off => "OFF (플레이어 Order 고정 → 항상 위에 그려짐)",
            Mode.CameraCustomAxis => "카메라 Custom Axis (0,1,0)",
            _ => "코드: sortingOrder = -y × 100",
        };

        public void NextMode()
        {
            mode = (Mode)(((int)mode + 1) % 3);
            Apply();
        }

        private void Apply()
        {
            foreach (var g in sortables) g.sortingOrder = 0;
            if (mode == Mode.CameraCustomAxis)
            {
                cam.transparencySortMode = TransparencySortMode.CustomAxis;
                cam.transparencySortAxis = Vector3.up; // y 가 큰 것(화면 위쪽)이 더 "멀리" = 먼저 그림
            }
            else
            {
                cam.transparencySortMode = TransparencySortMode.Default;
                if (mode == Mode.Off) playerGroup.sortingOrder = 10; // 잘못된 예: 항상 나무보다 위
            }
        }
    }
}
