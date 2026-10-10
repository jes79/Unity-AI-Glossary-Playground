using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 0214 총알: 벽에 맞으면 "막힘", 벽 뒤(z > wallZ)로 넘어가면 "뚫음" 으로 집계
    public class TunnelingBullet : MonoBehaviour
    {
        private int lane;
        private float wallZ;
        private bool counted;

        public void Init(int laneIndex, float wall)
        {
            lane = laneIndex;
            wallZ = wall;
            Destroy(gameObject, 2f);
        }

        private void FixedUpdate()
        {
            if (!counted && transform.position.z > wallZ + 0.5f)
            {
                counted = true;
                TunnelingDemo.Passed[lane]++;
                GetComponent<Renderer>().material.SetColor("_BaseColor", new Color(1f, 0.3f, 0.3f));
            }
        }

        private void OnCollisionEnter(Collision c)
        {
            if (counted) return;
            counted = true;
            TunnelingDemo.Blocked[lane]++;
            Destroy(gameObject);
        }
    }
}
