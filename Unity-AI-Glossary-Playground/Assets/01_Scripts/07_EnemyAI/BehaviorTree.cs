using System;
using System.Collections.Generic;
using System.Text;

namespace GlossaryDemo.EnemyAI
{
    // 최소한의 비헤이비어 트리 (Behavior Tree)
    // - Selector : 자식을 위에서부터 시도, 하나라도 Success/Running 이면 거기서 멈춤 ("~하거나")
    // - Sequence : 자식을 차례로 실행, 하나라도 Failure 면 멈춤 ("~하고 ~한다")
    // - Condition: 조건 검사 (bool → Success/Failure)
    // - Action   : 실제 행동 (시간이 걸리면 Running)
    public enum BTState { Success, Failure, Running }

    public abstract class BTNode
    {
        public readonly string Name;
        public readonly List<BTNode> Children = new List<BTNode>();
        public BTState LastState { get; private set; }
        public bool Visited { get; private set; } // 이번 프레임에 평가됐는지 (시각화용)

        protected BTNode(string name, params BTNode[] children)
        {
            Name = name;
            Children.AddRange(children);
        }

        public BTState Tick()
        {
            Visited = true;
            LastState = OnTick();
            return LastState;
        }

        protected abstract BTState OnTick();

        public void ClearVisit()
        {
            Visited = false;
            foreach (var c in Children) c.ClearVisit();
        }

        // 트리를 텍스트로 그림. 이번 프레임에 평가된 노드만 색칠
        public void Dump(StringBuilder sb, int depth = 0)
        {
            sb.Append(' ', depth * 4);
            string kind = this is BTSelector ? "[?] " : this is BTSequence ? "[→] " : this is BTCondition ? "◇ " : "▶ ";
            if (Visited)
            {
                string color = LastState switch { BTState.Success => "#7CFC8A", BTState.Running => "#FFD54A", _ => "#FF7A7A" };
                sb.Append($"<color={color}>{kind}{Name}  ({LastState})</color>\n");
            }
            else sb.Append($"<color=#777777>{kind}{Name}</color>\n");
            foreach (var c in Children) c.Dump(sb, depth + 1);
        }
    }

    public class BTSelector : BTNode
    {
        public BTSelector(string name, params BTNode[] children) : base(name, children) { }

        protected override BTState OnTick()
        {
            foreach (var c in Children)
            {
                var s = c.Tick();
                if (s != BTState.Failure) return s;
            }
            return BTState.Failure;
        }
    }

    public class BTSequence : BTNode
    {
        public BTSequence(string name, params BTNode[] children) : base(name, children) { }

        protected override BTState OnTick()
        {
            foreach (var c in Children)
            {
                var s = c.Tick();
                if (s != BTState.Success) return s;
            }
            return BTState.Success;
        }
    }

    public class BTCondition : BTNode
    {
        private readonly Func<bool> check;
        public BTCondition(string name, Func<bool> check) : base(name) { this.check = check; }
        protected override BTState OnTick() => check() ? BTState.Success : BTState.Failure;
    }

    public class BTAction : BTNode
    {
        private readonly Func<BTState> run;
        public BTAction(string name, Func<BTState> run) : base(name) { this.run = run; }
        protected override BTState OnTick() => run();
    }
}
