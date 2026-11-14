# 실습 4 — 카메라 흔들림

## 목표
명중과 격침의 강도를 다르게 전달하고 카메라 위치가 누적되지 않게 합니다.

## 적용 절차
1. 아래 코드로 CameraShake.cs를 교체합니다.
2. 씬의 Main Camera에 CameraShake를 추가합니다.
3. Flow에 Game Systems의GameFlowController를 연결합니다.
4. Hit Strength=0.08, Sink Strength=0.18, Duration=0.16초.
5. 카메라 Position=(0,0,-10)을 확인하고 씬을 저장합니다.

## 코드

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class CameraShake : MonoBehaviour
    {
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
```

## 핵심
기준 위치rest에 매 프레임 새 오프셋을 더합니다. 현재 위치에 계속 더하면 카메라가 떠내려갑니다.
시간에 따라 오프셋을 감쇠하고, 중복 이벤트는 더 큰 강도만 유지합니다.
플레이어 피격은 1.4배로 강조합니다. Hit Stop에도 작동하도록 unscaledDeltaTime을 씁니다.
전투를 벗어나거나 컴포넌트를 끄면 기준 위치를 복구합니다.

## 완료 확인
- 일반 명중보다 중간 적 격침이 강하게 흔들립니다.
- 플레이어 피격이 적 피격보다 조금 더 강합니다.
- 정비 화면으로 돌아왔을 때Position=(0,0,-10)입니다.
- 마지막 적 격침은 즉시Result로 넘어가므로 흔들림을 종료합니다. 중간 적으로 비교하세요.

## 작은 과제
Hit Strength=0.03/0.08/0.18을 비교하세요. 시야가 불편하면 강도보다 지속시간을 먼저 줄여보세요.
