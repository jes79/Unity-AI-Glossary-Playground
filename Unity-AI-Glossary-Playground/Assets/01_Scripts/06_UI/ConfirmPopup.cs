using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 재사용 가능한 확인/취소 팝업 (모달)
    // 사용: popup.Show("나가기", "정말 나가시겠습니까?", () => { 확인 시 할 일 }, () => { 취소 시 할 일 });
    // 뒤에 깔린 반투명 Blocker 이미지가 클릭을 막아서 팝업이 떠 있는 동안 다른 UI 를 누를 수 없음 (= 모달)
    public class ConfirmPopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image blocker;
        [SerializeField] private RectTransform window;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI confirmLabel;
        [SerializeField] private TextMeshProUGUI cancelLabel;
        [SerializeField] private float openDuration = 0.25f;

        private Action onConfirm;
        private Action onCancel;
        private bool modal = true;

        public bool IsOpen { get; private set; }
        public bool Modal => modal;

        private void Awake()
        {
            group.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void Show(string title, string message, Action confirm, Action cancel = null, string confirmText = "확인", string cancelText = "취소")
        {
            titleText.text = title;
            messageText.text = message;
            confirmLabel.text = confirmText;
            cancelLabel.text = cancelText;
            onConfirm = confirm;
            onCancel = cancel;

            gameObject.SetActive(true);
            transform.SetAsLastSibling(); // 다른 UI 보다 위에 그림
            IsOpen = true;
            StopAllCoroutines();
            StartCoroutine(Animate(true));
        }

        // 버튼 OnClick 에 연결
        public void Confirm() => Close(onConfirm);
        public void Cancel() => Close(onCancel);

        // 비교용: 모달 OFF 면 Blocker 가 클릭을 막지 않고 투명해짐
        public void SetModal(bool value)
        {
            modal = value;
            blocker.raycastTarget = value;
            blocker.color = new Color(0f, 0f, 0f, value ? 0.6f : 0f);
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (DemoInput.KeyDown(Key.Escape)) Cancel();
            if (DemoInput.KeyDown(Key.Enter)) Confirm();
        }

        private void Close(Action callback)
        {
            if (!IsOpen) return;
            IsOpen = false;
            StopAllCoroutines();
            StartCoroutine(Animate(false));
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            callback?.Invoke(); // 콜백은 한 번만 실행
        }

        // 열 때: 살짝 작게 → 튕기듯 커짐(OutBack) + 페이드 인 / 닫을 때 반대. 일시정지 중에도 동작하도록 unscaled
        private IEnumerator Animate(bool open)
        {
            float from = group.alpha;
            yield return Ease.Run(openDuration, open ? Ease.OutBack : Ease.InQuad, t =>
            {
                group.alpha = Mathf.Lerp(from, open ? 1f : 0f, Mathf.Clamp01(t));
                float s = open ? Mathf.LerpUnclamped(0.8f, 1f, t) : Mathf.Lerp(1f, 0.85f, t);
                window.localScale = new Vector3(s, s, 1f);
            }, true);
            if (!open) gameObject.SetActive(false);
        }
    }
}
