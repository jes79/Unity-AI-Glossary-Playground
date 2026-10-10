using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 클릭하면 처치(또는 채집)되는 필드 오브젝트. QuestManager 에 id 만 알리고 다른 곳에서 다시 나타남
    public class QuestMonster : MonoBehaviour
    {
        [SerializeField] private string targetId = "slime";
        [SerializeField] private string displayName = "슬라임";
        [SerializeField] private Rect area = new Rect(-1f, -4f, 10f, 6f);
        [SerializeField] private float wanderSpeed = 1f;
        [SerializeField] private bool bob = true;

        private Vector3 goal;

        private void Start() => PickGoal();

        private void Update()
        {
            if (wanderSpeed > 0f)
            {
                transform.position = Vector3.MoveTowards(transform.position, goal, wanderSpeed * Time.deltaTime);
                if ((transform.position - goal).sqrMagnitude < 0.01f) PickGoal();
            }

            if (DemoInput.LeftClick)
            {
                var col = Physics2D.OverlapPoint(DemoInput.MouseWorld2D());
                if (col != null && col.gameObject == gameObject) Defeat();
            }
        }

        private void Defeat()
        {
            DamagePopup.Spawn(transform.position + Vector3.up * 0.6f, $"{displayName} +1", Color.white, 4f);
            QuestManager.Report(targetId); // 퀘스트 시스템에는 "무엇을 했는지"만 알림
            transform.position = RandomPoint();
            PickGoal();
        }

        private void PickGoal() => goal = RandomPoint();

        private Vector3 RandomPoint() => new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax), 0f);
    }
}
