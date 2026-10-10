using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.PhysicsDemo
{
    // 히트박스(공격 판정) / 허트박스(맞는 판정)
    // - 검을 0.4초 동안 휘두르지만, 히트박스는 중간의 activeStart ~ activeStart+activeDuration (0.1초)만 켜짐
    // - 켜져 있는 동안 매 프레임 Physics.OverlapBox 로 허트박스(Hitbox 레이어의 트리거)를 찾음
    // - 한 번 휘두를 때 같은 대상은 한 번만 맞도록 HashSet 으로 기록
    // H: 히트박스를 휘두르는 내내 켜기(비교) → 양옆 허수아비까지 맞음
    // T: 슬로 모션
    public class SwordHitboxDemo : MonoBehaviour
    {
        [Header("휘두르기")]
        [SerializeField] private Transform swordPivot;
        [SerializeField] private float swingDuration = 0.4f;
        [SerializeField] private float swingArc = 140f;

        [Header("히트박스")]
        [SerializeField] private Transform hitbox;              // 검날 위치의 상자 (크기 = localScale)
        [SerializeField] private Renderer hitboxRenderer;
        [SerializeField] private float activeStart = 0.15f;      // 휘두르기 시작 후 몇 초에 켤지
        [SerializeField] private float activeDuration = 0.1f;    // 요청값: 0.1초만
        [SerializeField] private LayerMask hurtboxMask;          // 허트박스 레이어 (Hitbox)
        [SerializeField] private bool alwaysOn;

        [Header("타임라인 UI")]
        [SerializeField] private RectTransform timelineWindow;   // 활성 구간 표시
        [SerializeField] private RectTransform timelineCursor;

        private float swingTime = -1f;
        private readonly HashSet<Collider> hitThisSwing = new HashSet<Collider>();
        private readonly Collider[] buffer = new Collider[16];
        private int totalHits;
        private string lastHits = "-";

        public bool HitboxActive
        {
            get
            {
                if (swingTime < 0f) return false;
                if (alwaysOn) return true;
                return swingTime >= activeStart && swingTime <= activeStart + activeDuration;
            }
        }

        private void Update()
        {
            if ((DemoInput.JumpPressed || PhysicsClick.LeftWorld) && swingTime < 0f) StartSwing();
            if (DemoInput.KeyDown(Key.H)) alwaysOn = !alwaysOn;
            if (DemoInput.KeyDown(Key.T)) Time.timeScale = Time.timeScale < 1f ? 1f : 0.2f;

            if (swingTime >= 0f)
            {
                swingTime += Time.deltaTime;
                float t = Mathf.Clamp01(swingTime / swingDuration);
                swordPivot.localRotation = Quaternion.Euler(0f, -swingArc * 0.5f + swingArc * t, 0f);
                if (HitboxActive) CheckHits();
                if (swingTime >= swingDuration) { swingTime = -1f; swordPivot.gameObject.SetActive(false); }
            }

            if (hitboxRenderer != null)
                hitboxRenderer.material.SetColor("_BaseColor", HitboxActive ? new Color(1f, 0.2f, 0.2f, 0.6f) : new Color(0.8f, 0.8f, 0.8f, 0.12f));

            UpdateTimeline();
            GlossaryInfoPanel.Status = $"히트박스: {(alwaysOn ? "휘두르는 내내 ON (비교)" : $"{activeStart:0.00}~{activeStart + activeDuration:0.00}초만 ON ({activeDuration:0.0}초)")}" +
                                       $"   {(Time.timeScale < 1f ? "슬로 모션 ×0.2" : "")}\n" +
                                       $"지금 히트박스: {(HitboxActive ? "<color=#FF6060>ON</color>" : "OFF")}   총 타격 {totalHits}\n마지막 휘두르기: {lastHits}";
        }

        private void StartSwing()
        {
            swingTime = 0f;
            hitThisSwing.Clear();
            lastHits = "(없음)";
            swordPivot.gameObject.SetActive(true);
        }

        private void CheckHits()
        {
            // 히트박스 상자 안의 허트박스를 찾음 (트리거도 포함해야 하므로 QueryTriggerInteraction.Collide)
            int n = Physics.OverlapBoxNonAlloc(hitbox.position, hitbox.lossyScale * 0.5f, buffer, hitbox.rotation, hurtboxMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < n; i++)
            {
                if (!hitThisSwing.Add(buffer[i])) continue; // 이번 휘두르기에서 이미 맞았으면 무시
                totalHits++;
                lastHits = lastHits == "(없음)" ? buffer[i].transform.parent.name : lastHits + ", " + buffer[i].transform.parent.name;
                var health = buffer[i].GetComponentInParent<DemoHealth>();
                if (health != null) health.TakeDamage(10f);
            }
        }

        private void UpdateTimeline()
        {
            if (timelineWindow != null)
            {
                float a = alwaysOn ? 0f : activeStart / swingDuration, b = alwaysOn ? 1f : (activeStart + activeDuration) / swingDuration;
                timelineWindow.anchorMin = new Vector2(a, 0f);
                timelineWindow.anchorMax = new Vector2(b, 1f);
                timelineWindow.offsetMin = timelineWindow.offsetMax = Vector2.zero;
            }
            if (timelineCursor != null)
            {
                float x = swingTime < 0f ? 0f : Mathf.Clamp01(swingTime / swingDuration);
                timelineCursor.anchorMin = new Vector2(x, 0f);
                timelineCursor.anchorMax = new Vector2(x, 1f);
                timelineCursor.anchoredPosition = Vector2.zero;
            }
        }

        private void OnDisable() => Time.timeScale = 1f;
    }
}
