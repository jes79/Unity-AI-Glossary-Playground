using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 0204 적 총알 (EnemyBullet 레이어). 무엇에 맞았는지 레이어로 구분해 표시하고 사라짐
    public class LayerBullet : MonoBehaviour
    {
        private void Start() => Destroy(gameObject, 5f);

        private void OnCollisionEnter(Collision c)
        {
            string layer = LayerMask.LayerToName(c.collider.gameObject.layer);
            if (layer == "Enemy")
            {
                LayerMatrixDemo.FriendlyFireHits++;
                FloatingText.Spawn(c.transform.position + Vector3.up * 1.2f, "아군 오사!", new Color(1f, 0.5f, 0.4f), 4f);
            }
            else if (layer == "Player")
            {
                LayerMatrixDemo.PlayerHits++;
                FloatingText.Spawn(c.transform.position + Vector3.up * 1.2f, "피격", new Color(1f, 0.9f, 0.4f), 4f);
            }
            Destroy(gameObject);
        }
    }
}
