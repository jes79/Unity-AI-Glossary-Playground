using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 가중치 랜덤 드롭 테이블: 일반 70 / 희귀 25 / 전설 5
    // 0 ~ (가중치 합) 사이 난수를 뽑고, 가중치를 차례로 빼 가며 어느 구간에 들어가는지 찾음
    public class DropTableDemo : MonoBehaviour
    {
        [System.Serializable]
        public class DropEntry
        {
            public string name;
            public float weight;
            public Color color = Color.white;
            public Image bar;
            public TextMeshProUGUI label;
            [HideInInspector] public int count;
        }

        [SerializeField] private DropEntry[] table;
        [SerializeField] private float barMaxHeight = 380f;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Image[] recentIcons; // 최근 결과 10개

        private int total;
        private int recentIndex;

        private void Start() => Refresh();

        // 핵심 함수: 가중치 랜덤
        public int Roll()
        {
            float sum = 0f;
            foreach (var e in table) sum += e.weight;
            float r = Random.Range(0f, sum);
            for (int i = 0; i < table.Length; i++)
            {
                if (r < table[i].weight) return i;
                r -= table[i].weight;
            }
            return table.Length - 1;
        }

        public void RollTimes(int times)
        {
            int last = 0;
            for (int i = 0; i < times; i++)
            {
                last = Roll();
                table[last].count++;
                total++;
                if (recentIcons != null && recentIcons.Length > 0)
                {
                    recentIcons[recentIndex % recentIcons.Length].color = table[last].color;
                    recentIndex++;
                }
            }
            if (resultText != null)
            {
                resultText.text = times == 1 ? $"획득: <b>{table[last].name}</b>" : $"{times}회 뽑기 완료 (마지막: {table[last].name})";
                resultText.color = table[last].color;
            }
            Refresh();
        }

        public void ResetCounts()
        {
            foreach (var e in table) e.count = 0;
            total = 0;
            if (resultText != null) resultText.text = "";
            if (recentIcons != null) foreach (var i in recentIcons) i.color = new Color(1f, 1f, 1f, 0.08f);
            Refresh();
        }

        // 히스토그램: 실제 비율과 기대 비율 비교
        private void Refresh()
        {
            float sum = 0f;
            foreach (var e in table) sum += e.weight;
            var status = new System.Text.StringBuilder($"총 {total}회   ");
            foreach (var e in table)
            {
                float expected = e.weight / sum;
                float actual = total > 0 ? (float)e.count / total : 0f;
                if (e.bar != null)
                {
                    var rt = e.bar.rectTransform;
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Max(2f, barMaxHeight * actual));
                }
                if (e.label != null)
                    e.label.text = $"<b>{e.name}</b>\n{e.count}회\n실제 {actual * 100f:0.0}%\n<color=#9AA3B5>기대 {expected * 100f:0}%</color>";
                status.Append($"{e.name} {actual * 100f:0.0}% (기대 {expected * 100f:0}%)   ");
            }
            GlossaryInfoPanel.Status = status.ToString() + "\n뽑는 횟수가 많을수록 실제 비율이 70 / 25 / 5 에 가까워짐";
        }
    }
}
