using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class ShipHitFlash : MonoBehaviour
    {
        // 실습 TODO: Hit 이벤트의 Target을 확인하여 자신의 SpriteRenderer 색상을 짧게 밝게 바꾼다. 원래 색상을 보관하고 중복 피격/비활성화 시 복구한다.
        // 구현은 Docs/03_JuiceTutorial.md를 따라 직접 추가합니다.
        public SpriteRenderer visual;
        public float duration = 0.08f;
        public Color flash = new Color(1f, 0.35f, 0.25f, 1f);
        CombatFeedbackEvents events;
        ShipHealth health;
        Color rest;
        float remaining;
        void Awake() { health = GetComponent<ShipHealth>(); rest = visual.color; }
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.Hit += OnHit;
        }
        void OnHit(HitFeedback hit)
        {
            if (hit.Target != health) return;
            remaining = duration;
            visual.color = flash;
        }
        void Update()
        {
            if (remaining <= 0) return;
            remaining -= Time.unscaledDeltaTime;
            if (remaining <= 0 && health.IsAlive) visual.color = rest;
        }
        void OnDisable()
        {
            if (events) events.Hit -= OnHit;
            if (visual && (!health || health.IsAlive)) visual.color = rest;
            remaining = 0;
        }
    }
}
