# 실습 3 — 피격 플래시

## 목표
맞은 배만 잠깐 붉게 표시하여 피해 여부를 읽을 수 있게 합니다.
기본 Sprites/Default의 Color는 텍스처에 곱해집니다. 원래 흰색인 Color를 다시 흰색으로 바꾸면 번쩍임이 잘 보이지 않습니다.
이 실습은 붉은 틴트로 시작합니다. 완전한 흰색 실루엣은 별도의 플래시 셰이더 확장 과제입니다.

## 적용 절차
1. 아래 코드로 ShipHitFlash.cs를 교체합니다.
2. PlayerShip과EnemyShip 프리팹 루트에 각각 컴포넌트를 추가합니다.
3. Visual에는 각 Visual 자식의 SpriteRenderer를 연결합니다.
4. Duration=0.08초, Flash=(1,0.35,0.25,1).
5. 저장 후 적에게 한 발 명중시키고, 자신도 한 발 맞아봅니다.

## 코드

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class ShipHitFlash : MonoBehaviour
    {
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
```

## 핵심
대상 ShipHealth의 참조를 비교합니다. IsPlayer만 비교하면 같은 편 전체가 번쩍일 수 있습니다.
타이머를 다시 설정하면 연속 피격에 대응할 수 있습니다.
unscaledDeltaTime을 사용해 후속 Hit Stop 중에도 효과 시간이 지나갑니다.
사망한 배는 색 복구를 멈춰 이후 격침 페이드와 충돌하지 않게 합니다.

## 완료 확인
명중한 대상만 붉게 바뀌고 살아 있으면 원래 색상으로 복구됩니다.
다음 라운드에서도 한 번씩만 발생해야 합니다.
Duration을0.04/0.08/0.16초로 비교하세요.
