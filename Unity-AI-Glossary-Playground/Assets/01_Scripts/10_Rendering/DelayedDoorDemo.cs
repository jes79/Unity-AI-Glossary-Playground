using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // "3초 뒤 문이 열리기"를 두 가지 방법으로
    //  왼쪽 문: 코루틴   yield return new WaitForSeconds(3f)
    //  오른쪽 문: async/await   await Awaitable.WaitForSecondsAsync(3f)   (Unity 6 의 Awaitable)
    // 둘 다 기다리는 동안 게임이 멈추지 않음(한 프레임에 다 하지 않고 나눠서 실행)
    public class DelayedDoorDemo : MonoBehaviour
    {
        [SerializeField] private Transform coroutineDoor;
        [SerializeField] private Transform asyncDoor;
        [SerializeField] private TextMeshPro coroutineLabel;
        [SerializeField] private TextMeshPro asyncLabel;
        [SerializeField] private float delay = 3f;        // 요청 예문: 3초 뒤
        [SerializeField] private float openHeight = 3.2f;
        [SerializeField] private float openDuration = 0.8f;

        private Vector3 coroutineClosed, asyncClosed;
        private Coroutine running;                          // 코루틴 취소용 핸들
        private CancellationTokenSource asyncCts;           // async 취소용 토큰
        private float coroutineRemain = -1f, asyncRemain = -1f;

        private void Awake()
        {
            coroutineClosed = coroutineDoor.position;
            asyncClosed = asyncDoor.position;
        }

        private void OnDestroy() => asyncCts?.Cancel();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) StartBoth();
            if (DemoInput.KeyDown(Key.C)) CancelBoth();
            if (DemoInput.KeyDown(Key.R)) ResetDoors();

            coroutineLabel.text = Label("코루틴", coroutineRemain, coroutineDoor.position.y > coroutineClosed.y + 0.1f);
            asyncLabel.text = Label("async / await", asyncRemain, asyncDoor.position.y > asyncClosed.y + 0.1f);
            GlossaryInfoPanel.Status = $"코루틴: {State(coroutineRemain, coroutineDoor, coroutineClosed)}\n" +
                                       $"async: {State(asyncRemain, asyncDoor, asyncClosed)}\n" +
                                       $"Time.time {Time.time:0.0}  (기다리는 동안에도 계속 증가 = 게임이 멈추지 않음)";
        }

        private static string Label(string title, float remain, bool open) =>
            remain >= 0f ? $"{title}\n{remain:0.0}초" : open ? $"{title}\n열림" : $"{title}\n대기";

        private static string State(float remain, Transform door, Vector3 closed) =>
            remain >= 0f ? $"기다리는 중 {remain:0.0}초 남음" : door.position.y > closed.y + 0.1f ? "열림" : "닫힘";

        public void StartBoth()
        {
            ResetDoors();
            running = StartCoroutine(OpenAfterDelayCoroutine());
            // 오브젝트가 파괴되면 자동 취소되도록 destroyCancellationToken 과 연결
            asyncCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _ = OpenAfterDelayAsync(asyncCts.Token); // 반환값(Awaitable)을 기다리지 않고 실행만 시킴
        }

        // ───── 방법 1: 코루틴 ─────
        private IEnumerator OpenAfterDelayCoroutine()
        {
            coroutineRemain = delay;
            // 남은 시간을 보여 주려고 매 프레임 기다림. 단순히 기다리기만 하면 아래 한 줄로 충분:
            // yield return new WaitForSeconds(delay);
            while (coroutineRemain > 0f)
            {
                yield return null;                 // 다음 프레임까지 쉼
                coroutineRemain -= Time.deltaTime;
            }
            coroutineRemain = -1f;
            yield return Slide(coroutineDoor, coroutineClosed);
            running = null;
        }

        private IEnumerator Slide(Transform door, Vector3 closed)
        {
            yield return Ease.Run(openDuration, Ease.OutCubic, t => door.position = closed + Vector3.up * (openHeight * t));
        }

        // ───── 방법 2: async / await (Awaitable) ─────
        private async Awaitable OpenAfterDelayAsync(CancellationToken token)
        {
            try
            {
                asyncRemain = delay;
                float end = Time.time + delay;
                // 단순히 기다리기만 하면: await Awaitable.WaitForSecondsAsync(delay, token);
                while (Time.time < end)
                {
                    await Awaitable.NextFrameAsync(token);   // 다음 프레임까지 쉼 (취소 가능)
                    asyncRemain = Mathf.Max(0f, end - Time.time);
                }
                asyncRemain = -1f;
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / openDuration;
                    asyncDoor.position = asyncClosed + Vector3.up * (openHeight * Ease.OutCubic(Mathf.Clamp01(t)));
                    await Awaitable.NextFrameAsync(token);
                }
            }
            catch (OperationCanceledException)
            {
                // 취소되면 여기로 옴 (오브젝트가 파괴돼도 destroyCancellationToken 으로 안전하게 멈출 수 있음)
                asyncRemain = -1f;
            }
        }

        public void CancelBoth()
        {
            if (running != null) { StopCoroutine(running); running = null; }
            coroutineRemain = -1f;
            asyncCts?.Cancel();
            asyncCts = null;
        }

        public void ResetDoors()
        {
            CancelBoth();
            coroutineDoor.position = coroutineClosed;
            asyncDoor.position = asyncClosed;
        }
    }
}
