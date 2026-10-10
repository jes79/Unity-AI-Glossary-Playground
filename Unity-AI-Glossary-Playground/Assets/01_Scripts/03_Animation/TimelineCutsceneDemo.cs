using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace GlossaryDemo.AnimationDemo
{
    // Timeline 컷신 제어: PlayableDirector 로 재생 / 일시정지 / 건너뛰기
    // 컷신 내용(보스 등장 애니메이션, 카메라 이동, 대사 표시, 사운드)은 TimelineAsset 의 트랙들이 담당하고
    // 스크립트는 director.Play() / Pause() / time 만 다룸
    public class TimelineCutsceneDemo : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private Image progressFill;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space))
            {
                director.time = 0;
                director.Play();
            }
            if (DemoInput.KeyDown(Key.P))
            {
                if (director.state == PlayState.Playing) director.Pause();
                else director.Resume();
            }
            if (DemoInput.KeyDown(Key.S))
            {
                // 건너뛰기: 마지막 시간으로 이동해 결과 상태를 적용 (Wrap Mode = Hold 라 끝 상태 유지)
                director.time = director.duration;
                director.Evaluate();
                director.Pause();
            }

            float t = (float)director.time, d = (float)director.duration;
            if (progressFill != null) progressFill.fillAmount = d > 0f ? t / d : 0f;
            string state = director.state == PlayState.Playing ? "재생 중" : (t >= d - 0.01f ? "끝" : "일시정지");
            GlossaryInfoPanel.Status = $"Timeline {t:0.0} / {d:0.0}초  ({state})\n" +
                                       "트랙: 보스 애니메이션 · 카메라 이동 · 대사(Activation) · 사운드(Audio)";
        }
    }
}
