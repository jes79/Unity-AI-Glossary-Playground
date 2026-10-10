using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 시점 종류 비교: 한 캐릭터를 5가지 시점으로 조작
    // 1 탑다운   : 바로 위에서 내려다봄, WASD = 화면 상하좌우
    // 2 쿼터뷰   : 45도 비스듬히 위에서 봄(아이소메트릭 느낌), 마우스 클릭한 바닥 위치로 이동 (요청 예문)
    // 3 횡스크롤 : 옆에서 봄, 좌우 이동 + 점프만 (깊이 z 는 0 으로 고정)
    // 4 FPS      : 눈 위치 카메라, 마우스로 시선, WASD = 바라보는 방향 기준
    // 5 TPS      : 어깨 뒤 카메라, 마우스로 카메라 회전, 캐릭터는 이동 방향을 봄
    [RequireComponent(typeof(CharacterController))]
    public class ViewTypesController : MonoBehaviour
    {
        public enum ViewType { TopDown, Quarter, SideScroll, FPS, TPS }

        [SerializeField] private ViewType view = ViewType.Quarter;
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask groundMask;     // 쿼터뷰 클릭 이동용 바닥
        [SerializeField] private Transform clickMarker;    // 클릭 지점 표시
        [SerializeField] private Renderer[] bodyRenderers; // FPS 에서는 자기 몸을 숨김

        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float mouseSensitivity = 0.12f;

        private CharacterController cc;
        private float verticalVelocity;
        private float yaw, pitch = 15f;     // FPS/TPS 시선 각도
        private Vector3? clickTarget;       // 쿼터뷰 이동 목표
        private Vector3 camVelocity;

        private static readonly string[] Names = { "탑다운 (Top-down)", "쿼터뷰 (Quarter view)", "횡스크롤 (Side-scroll)", "1인칭 (FPS)", "3인칭 (TPS)" };
        private static readonly string[] Tips =
        {
            "바로 위에서 내려다봄. 전략·슈팅(트윈스틱)에 많이 씀",
            "45° 비스듬히 위. 바닥을 클릭하면 그 위치로 이동 (디아블로식)",
            "옆에서 봄. 좌우 + 점프만, 깊이는 고정 (플랫포머)",
            "눈 위치 카메라. 마우스로 시선 (Esc : 커서 풀기)",
            "어깨 뒤 카메라. 마우스로 카메라 회전 (Esc : 커서 풀기)"
        };

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
        }

        private void Start() => SetView((int)view);

        // 버튼에서도 호출 (0~4)
        public void SetView(int index)
        {
            view = (ViewType)index;
            clickTarget = null;
            if (clickMarker != null) clickMarker.gameObject.SetActive(false);

            bool mouseLook = view == ViewType.FPS || view == ViewType.TPS;
            Cursor.lockState = mouseLook ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !mouseLook;
            foreach (var r in bodyRenderers) if (r != null) r.enabled = view != ViewType.FPS;

            cam.orthographic = view == ViewType.SideScroll;
            cam.orthographicSize = 6f;
            cam.fieldOfView = view == ViewType.Quarter ? 30f : 60f;
            if (view == ViewType.SideScroll)
            {
                // 횡스크롤로 바꾸면 깊이를 0 으로 정렬
                cc.enabled = false;
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                cc.enabled = true;
            }
            SnapCamera();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) SetView(0);
            if (DemoInput.KeyDown(Key.Digit2)) SetView(1);
            if (DemoInput.KeyDown(Key.Digit3)) SetView(2);
            if (DemoInput.KeyDown(Key.Digit4)) SetView(3);
            if (DemoInput.KeyDown(Key.Digit5)) SetView(4);
            if (DemoInput.KeyDown(Key.Escape)) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

            if (view == ViewType.FPS || view == ViewType.TPS)
            {
                if (DemoInput.LeftClick && Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Vector2 d = DemoInput.MouseDelta * mouseSensitivity;
                    yaw += d.x;
                    pitch = Mathf.Clamp(pitch - d.y, -70f, 80f);
                }
            }

            Vector3 move = GetMove();
            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (DemoInput.JumpPressed && cc.isGrounded && view != ViewType.Quarter) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalVelocity += gravity * Time.deltaTime;
            cc.Move((move * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);

            // 회전: FPS 는 시선(yaw)대로, 나머지는 이동 방향을 봄
            if (view == ViewType.FPS) transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            else if (move.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(move.x, 0f, move.z)), 720f * Time.deltaTime);

            if (transform.position.y < -10f) { cc.enabled = false; transform.position = Vector3.up; cc.enabled = true; }

            GlossaryInfoPanel.Status = $"현재 시점: {(int)view + 1}. {Names[(int)view]}\n{Tips[(int)view]}";
        }

        private Vector3 GetMove()
        {
            Vector2 input = DemoInput.Move;
            switch (view)
            {
                case ViewType.TopDown:
                    return new Vector3(input.x, 0f, input.y); // 화면 위 = 월드 +z

                case ViewType.Quarter:
                    // 클릭: 마우스 레이를 바닥에 쏴서 목표 지점 저장
                    bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(); // 버튼 클릭은 이동으로 치지 않음
                    if (DemoInput.LeftClick && !overUI && Physics.Raycast(cam.ScreenPointToRay(DemoInput.MousePosition), out var hit, 200f, groundMask))
                    {
                        clickTarget = hit.point;
                        if (clickMarker != null) { clickMarker.gameObject.SetActive(true); clickMarker.position = hit.point + Vector3.up * 0.05f; }
                    }
                    if (input.sqrMagnitude > 0.01f)
                    {
                        clickTarget = null; // 키보드 입력이 있으면 클릭 이동 취소
                        return Quaternion.Euler(0f, 45f, 0f) * new Vector3(input.x, 0f, input.y); // 카메라가 45도 돌아가 있으므로 입력도 돌림
                    }
                    if (clickTarget.HasValue)
                    {
                        Vector3 to = clickTarget.Value - transform.position;
                        to.y = 0f;
                        if (to.magnitude < 0.15f) { clickTarget = null; if (clickMarker != null) clickMarker.gameObject.SetActive(false); return Vector3.zero; }
                        return Vector3.ClampMagnitude(to, 1f).normalized;
                    }
                    return Vector3.zero;

                case ViewType.SideScroll:
                    // 깊이(z)를 0 쪽으로 계속 당겨서 한 줄 위에서만 움직이게 함
                    return new Vector3(input.x, 0f, -transform.position.z * 5f / moveSpeed);

                default: // FPS, TPS: 시선 yaw 기준
                    Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
                    return yawRot * new Vector3(input.x, 0f, input.y);
            }
        }

        private void LateUpdate()
        {
            GetCameraPose(out Vector3 pos, out Quaternion rot);
            bool snap = view == ViewType.FPS; // FPS 는 지연 없이 머리에 붙어야 멀미가 없음
            cam.transform.position = snap ? pos : Vector3.SmoothDamp(cam.transform.position, pos, ref camVelocity, 0.08f);
            cam.transform.rotation = snap ? rot : Quaternion.Slerp(cam.transform.rotation, rot, 1f - Mathf.Exp(-15f * Time.deltaTime));
        }

        private void SnapCamera()
        {
            GetCameraPose(out Vector3 pos, out Quaternion rot);
            cam.transform.SetPositionAndRotation(pos, rot);
            camVelocity = Vector3.zero;
        }

        private void GetCameraPose(out Vector3 pos, out Quaternion rot)
        {
            Vector3 p = transform.position;
            switch (view)
            {
                case ViewType.TopDown:
                    rot = Quaternion.Euler(90f, 0f, 0f);
                    pos = p + Vector3.up * 20f;
                    break;
                case ViewType.Quarter:
                    rot = Quaternion.Euler(35f, 45f, 0f);           // 위에서 35도, 옆으로 45도 → 쿼터뷰
                    pos = p - rot * Vector3.forward * 30f;          // 멀리서 좁은 FOV(30) → 원근 왜곡이 적은 쿼터뷰 느낌
                    break;
                case ViewType.SideScroll:
                    rot = Quaternion.identity;
                    pos = new Vector3(p.x, p.y + 1.5f, -15f);
                    break;
                case ViewType.FPS:
                    rot = Quaternion.Euler(pitch, yaw, 0f);
                    pos = p + Vector3.up * 0.7f;                    // 눈 높이
                    break;
                default: // TPS
                    rot = Quaternion.Euler(pitch, yaw, 0f);
                    pos = p + Vector3.up * 1.4f + rot * new Vector3(0.7f, 0f, -5f); // 오른쪽 어깨 뒤
                    break;
            }
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
