using TMPro;
using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 공격 연습용 허수아비: 맞으면 흔들리고 숫자가 줄어듦
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private TextMeshPro label;

        private float hp;
        private float wobble;

        private void Start()
        {
            hp = maxHp;
            Refresh();
        }

        public void Hit(float damage)
        {
            hp -= damage;
            if (hp <= 0f) hp = maxHp; // 데모: 다시 채움
            wobble = 1f;
            Refresh();
        }

        private void Update()
        {
            wobble = Mathf.MoveTowards(wobble, 0f, Time.deltaTime * 3f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 40f) * 12f * wobble);
            if (label != null) label.transform.rotation = Camera.main.transform.rotation;
        }

        private void Refresh()
        {
            if (label != null) label.text = $"{hp:0}";
        }
    }
}
