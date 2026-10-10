using UnityEngine;

namespace GlossaryDemo.Rendering
{
    // 매 프레임 회전만 하는 오브젝트. 기다리는 동안에도 계속 돌면 "게임이 멈추지 않았다"는 증거
    // (Thread.Sleep(3000) 처럼 메인 스레드를 막으면 이 공도 3초간 멈춤)
    public class SpinWhileWaiting : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 180f;

        private void Update() => transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f);
    }
}
