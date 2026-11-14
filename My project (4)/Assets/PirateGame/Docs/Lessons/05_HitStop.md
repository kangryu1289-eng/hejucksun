# 실습 5 — 명중 순간 정지(Hit Stop)

## 목표
명중 순간 게임 시간을 짧게 멈추고 반드시 복구합니다.
기본 게임에는 별도 일시정지 메뉴가 없으므로 이 컴포넌트가 전투 중 순간 정지를 소유합니다.

## 적용 절차
1. 아래 코드로 HitStop.cs를 교체합니다.
2. 씬의 Juice Lab 오브젝트에 HitStop을 추가합니다.
3. Flow에 Game Systems의 GameFlowController를 연결합니다.
4. Seconds=0.045초를 입력하고 씬을 저장합니다.
5. 마지막 적을 제외한 일반 명중부터 비교합니다.

## 코드

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class HitStop : MonoBehaviour
    {
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
```

## 핵심
Time.timeScale=0이면 deltaTime 기반 타이머도 멈춥니다.
복구 타이머는 unscaledDeltaTime을 사용해야 합니다.
중복 피격은 남은 시간과 요청 시간 중 큰 값으로 합칩니다. 매번 코루틴을 추가하지 않습니다.
이 컴포넌트가 바꾸기 전 시간 배율을 한 번만 보관합니다.
결과 화면 전환이나 비활성화 시 멈춤을 해제합니다.
GameFlowController도 결과·정비·출항 시 timeScale=1로 복구합니다.

## 완료 확인
- 일반 명중 시 이동과 포탄이 잠깐 멈추었다 재개합니다.
- 카메라 흔들림과 피격 색상은 정지 중에도 갱신됩니다.
- 여러 포탄이 동시에 명중해도 영구 정지하지 않습니다.
- 결과창에서 버튼이 작동하고 다시 출항하면 정상 속도입니다.
- Play 중 HitStop을 비활성화해도 즉시 복구됩니다.

## 작은 과제
0.02/0.045/0.08초를 비교합니다. 수치가 너무 크면 조작이 끊기므로 0.1초 이상을 기본값으로 쓰지 마세요.
