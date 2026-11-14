using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class CameraShake : MonoBehaviour
    {
        // 실습 TODO: Hit/Sunk 이벤트를 구독하여 강도와 지속시간이 다른 카메라 흔들림을 구현한다. 기준 위치를 유지하고 unscaledDeltaTime으로 감쇠한다.
        // 구현은 Docs/03_JuiceTutorial.md를 따라 직접 추가합니다.
        public GameFlowController flow;
        public float hitStrength = 0.08f, sinkStrength = 0.18f, duration = 0.16f;
        CombatFeedbackEvents events;
        Vector3 rest;
        float strength, remaining;
        void Awake() => rest = transform.localPosition;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) { events.Hit += OnHit; events.Sunk += OnSunk; }
        }
        void OnHit(HitFeedback hit) => Request(hitStrength * (hit.IsPlayer ? 1.4f : 1));
        void OnSunk(SinkFeedback sink) => Request(sinkStrength);
        void Request(float power) { strength = Mathf.Max(strength, power); remaining = duration; }
        void LateUpdate()
        {
            if (flow.Phase != GamePhase.Battle) remaining = 0;
            if (remaining <= 0) { transform.localPosition = rest; strength = 0; return; }
            remaining = Mathf.Max(0, remaining - Time.unscaledDeltaTime);
            Vector2 offset = Random.insideUnitCircle * strength * (remaining / Mathf.Max(0.01f, duration));
            transform.localPosition = rest + new Vector3(offset.x, offset.y, 0);
        }
        void OnDisable()
        {
            if (events) { events.Hit -= OnHit; events.Sunk -= OnSunk; }
            transform.localPosition = rest; remaining = strength = 0;
        }
    }
}
