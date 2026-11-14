# 2-02 — 실제 속도로 만드는 항적과 물보라

## 시작 조건
01 관성 이동은 필수가 아닙니다. 기존 Rigidbody2D 속도로 독립 실행합니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 이 문서의 코드는 해당 파일 전체를 교체하는 완성 예제입니다.
프로젝트의 새 클래스는 TODO만 있는 상태입니다. 코드를 입력·컴파일한 다음 Inspector를 연결하세요.

## 목표와 클래스 관계
WaterWake → Rigidbody2D/ShipHealth → 선미·선수 ParticleSystem.
이동 입력이 아니라 실제 속도로 방출량을 계산합니다.

## 1. 코드
Scripts/Feel/WaterWake.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class WaterWake : MonoBehaviour
    {
        public Rigidbody2D body;
        public ShipHealth health;
        public ParticleSystem wake, bow;
        public float minimumSpeed = 0.2f, fullSpeed = 3.5f, maxRate = 18f;
        void LateUpdate()
        {
            float speed = body.linearVelocity.magnitude;
            bool moving = health.IsAlive && speed > minimumSpeed;
            float ratio = moving ? Mathf.InverseLerp(minimumSpeed, fullSpeed, speed) : 0;
            SetRate(wake, ratio * maxRate);
            bool forward = Vector2.Dot(body.linearVelocity, body.transform.up) > 0;
            SetRate(bow, forward ? ratio * maxRate * 0.6f : 0);
        }
        static void SetRate(ParticleSystem particles, float rate)
        {
            if (!particles) return;
            var emission = particles.emission; emission.rateOverTime = rate;
            if (rate > 0 && !particles.isPlaying) particles.Play();
        }
        void OnDisable()
        {
            if (wake) wake.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (bow) bow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
```

## 2. Hierarchy
PlayerShip_Feel 루트 아래 빈 자식 Wake, BowSplash를 만듭니다.

| 오브젝트 | Local Position | 구성 |
|---|---|---|
| Wake | (0,-0.7,0) | ParticleSystem |
| BowSplash | (0,0.7,0) | ParticleSystem |

루트 +Y가 선수입니다. 이미지의 회전과 혼동하지 마세요.

## 3. ParticleSystem 설정
| 필드 | Wake | BowSplash |
|---|---|---|
| Main Looping | On | On |
| Play On Awake | Off | Off |
| Start Lifetime | 0.8 | 0.3 |
| Start Speed | 0 | 0.25 |
| Start Size | 0.18 | 0.08 |
| Start Color | (0.7,0.95,1,0.5) | (0.85,1,1,0.7) |
| Gravity Modifier | 0 | 0 |
| Simulation Space | World | World |
| Stop Action | None | None |
| Emission Rate over Time | 0 | 0 |
| Shape | Circle, Radius 0.1 | Circle, Radius 0.08 |
| Color over Lifetime | Alpha 1→0 | Alpha 1→0 |
| Renderer Material | SpriteUnlit | SpriteUnlit |
| Sorting Order | 0 | 3 |

입자 Burst는 추가하지 않습니다. 코드가 Emission Rate를 갱신합니다.

## 4. 참조 연결
루트에 WaterWake를 추가합니다.
Body=루트 Rigidbody2D, Health=루트 ShipHealth, Wake/Bow=각 자식 ParticleSystem.
Minimum Speed=0.2, Full Speed=3.5, Max Rate=18.
적에도 적용하려면 EnemyShip_Feel에 같은 구성을 만들고 Full Speed=1.7로 맞춥니다.

## 5. 완료 확인
- 빠를수록 항적이 많고 정지하면 새 항적이 생기지 않습니다.
- 이미 나온 입자는 배를 따라 끌려가지 않고 월드에 남습니다.
- 후진할 때 선수 물보라가 새로 생기지 않습니다.
- 섬에 막히면 W를 눌러도 긴 항적이 생기지 않습니다.
- 재출항 후 입자가 이전 전장에 무한히 남지 않습니다.

## 문제와 비교
항적이 배를 따라오면 Simulation Space=World를 확인하세요.
처음부터 계속 방출되면 Play On Awake/기존 Burst/다른 방출 스크립트를 확인합니다.
컴포넌트를 끄면 입자가 정리됩니다. Max Rate=8/18/30을 비교하세요.
