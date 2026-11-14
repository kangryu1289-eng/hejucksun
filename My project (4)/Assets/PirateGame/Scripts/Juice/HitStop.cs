using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class HitStop : MonoBehaviour
    {
        // 실습 TODO: Hit 이벤트를 구독하여 Time.timeScale을 짧게 0으로 만든다. unscaledDeltaTime으로 복구하고 중복 요청은 남은 시간의 최댓값으로 합친다. 전투 종료/비활성화 시 시간 배율을 반드시 복구한다.
        // 구현은 Docs/03_JuiceTutorial.md를 따라 직접 추가합니다.
        public GameFlowController flow;
        public float seconds = 0.045f;
        CombatFeedbackEvents events;
        float remaining, restoreScale = 1;
        bool ownsPause;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.Hit += OnHit;
        }
        void OnHit(HitFeedback hit)
        {
            if (flow.Phase != GamePhase.Battle) return;
            if (!ownsPause) { restoreScale = Time.timeScale; ownsPause = true; }
            remaining = Mathf.Max(remaining, seconds);
            Time.timeScale = 0;
        }
        void Update()
        {
            if (!ownsPause) return;
            if (flow.Phase != GamePhase.Battle) { Restore(); return; }
            remaining -= Time.unscaledDeltaTime;
            if (remaining <= 0) Restore();
        }
        void Restore()
        {
            if (ownsPause) Time.timeScale = flow && flow.Phase == GamePhase.Battle ? restoreScale : 1;
            ownsPause = false; remaining = 0;
        }
        void OnDisable()
        {
            if (events) events.Hit -= OnHit;
            Restore();
        }
    }
}
