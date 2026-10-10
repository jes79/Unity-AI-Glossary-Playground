using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // 상태 패턴: 상태 하나 = 클래스 하나. 각 상태는 "들어갈 때 / 매 프레임 / 나갈 때" 만 신경 쓰고
    // 다음 상태로 넘어갈 조건도 자기 안에서 판단함 → 거대한 switch 문이 사라짐
    public interface IPlayerState
    {
        string Name { get; }
        void Enter(StatePatternPlayer p);
        void Tick(StatePatternPlayer p);
        void Exit(StatePatternPlayer p);
    }

    public class IdleState : IPlayerState
    {
        public string Name => "Idle";

        public void Enter(StatePatternPlayer p) => p.SetLook(new Color(0.3f, 0.65f, 1f), new Vector2(1f, 1f));

        public void Tick(StatePatternPlayer p)
        {
            p.Move(0f);
            if (p.AttackPressed) p.ChangeState(p.Attack);
            else if (p.JumpPressed && p.Grounded) p.ChangeState(p.Jump);
            else if (Mathf.Abs(p.InputX) > 0.1f) p.ChangeState(p.Run);
        }

        public void Exit(StatePatternPlayer p) { }
    }

    public class RunState : IPlayerState
    {
        public string Name => "Run";

        public void Enter(StatePatternPlayer p) => p.SetLook(new Color(0.35f, 0.85f, 0.45f), new Vector2(1.1f, 0.92f));

        public void Tick(StatePatternPlayer p)
        {
            p.Move(p.InputX);
            if (p.AttackPressed) p.ChangeState(p.Attack);
            else if (p.JumpPressed && p.Grounded) p.ChangeState(p.Jump);
            else if (Mathf.Abs(p.InputX) <= 0.1f) p.ChangeState(p.Idle);
        }

        public void Exit(StatePatternPlayer p) { }
    }

    public class JumpState : IPlayerState
    {
        public string Name => "Jump";
        private float timer;

        public void Enter(StatePatternPlayer p)
        {
            timer = 0f;
            p.DoJump();
            p.SetLook(new Color(1f, 0.85f, 0.3f), new Vector2(0.85f, 1.2f));
        }

        public void Tick(StatePatternPlayer p)
        {
            timer += Time.deltaTime;
            p.Move(p.InputX); // 공중에서도 좌우 조작
            if (timer > 0.1f && p.Grounded && p.VelocityY <= 0.01f)
                p.ChangeState(Mathf.Abs(p.InputX) > 0.1f ? p.Run : p.Idle);
        }

        public void Exit(StatePatternPlayer p) { }
    }

    public class AttackState : IPlayerState
    {
        public string Name => "Attack";
        private float timer;

        public void Enter(StatePatternPlayer p)
        {
            timer = 0f;
            p.SetLook(new Color(1f, 0.4f, 0.35f), new Vector2(1.2f, 0.9f));
            p.ShowSlash(true);
        }

        public void Tick(StatePatternPlayer p)
        {
            timer += Time.deltaTime;
            p.Move(0f); // 공격 중에는 이동 불가 (이 규칙이 이 상태 안에만 있음)
            if (timer >= p.AttackDuration) p.ChangeState(p.Idle);
        }

        public void Exit(StatePatternPlayer p) => p.ShowSlash(false);
    }
}
