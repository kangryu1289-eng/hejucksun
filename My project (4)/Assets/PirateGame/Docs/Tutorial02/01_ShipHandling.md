# 2-01 — 가속·감속과 속도별 선회

## 시작 조건
다른 2차 단원 없이 시작할 수 있습니다. 기존 ShipMotor를 확장합니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 이 문서의 코드는 해당 파일 전체를 교체하는 완성 예제입니다.
프로젝트의 새 클래스는 TODO만 있는 상태입니다. 코드를 입력·컴파일한 다음 Inspector를 연결하세요.

## 목표와 클래스 관계
PlayerShipInput/EnemyShipAI → ShipMotor → 선택적인 ShipHandlingSettings.
입력 창구 SetInput은 유지합니다. 설정이 없으면 기존 직접 이동을 사용합니다.
새 모터를 추가하여 두 컴포넌트가 Rigidbody2D를 동시에 제어하지 않게 합니다.

## 1. 설정 클래스
Scripts/Feel/ShipHandlingSettings.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    [CreateAssetMenu(menuName = "Pirate Game/Feel/Ship Handling")]
    public sealed class ShipHandlingSettings : ScriptableObject
    {
        public float acceleration = 3f, deceleration = 4f, braking = 6f;
        public float lowSpeedTurn = 110f, highSpeedTurn = 65f;
    }
}
```

## 2. 모터 확장
Scripts/Ships/ShipMotor.cs 전체를 교체합니다.

```csharp
using UnityEngine;
using PirateGame.Feel;
namespace PirateGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(ShipHealth))]
    public sealed class ShipMotor : MonoBehaviour
    {
        public ShipHandlingSettings handling;
        float currentSpeed;
        public float forwardSpeed = 3.5f;
        public float reverseSpeed = 1.75f;
        public float turnSpeed = 110f;
        public Vector2 arenaHalfSize = new Vector2(12, 6.5f);
        Rigidbody2D body;
        ShipHealth health;
        float throttle, steering;
        void Awake() { body = GetComponent<Rigidbody2D>(); health = GetComponent<ShipHealth>(); }
        public void SetInput(float forward, float turn) { throttle = Mathf.Clamp(forward, -1, 1); steering = Mathf.Clamp(turn, -1, 1); }
        void FixedUpdate()
        {
            if (!health.IsAlive) return;
            float dt = Time.fixedDeltaTime;
            float actualSpeed = Vector2.Dot(body.linearVelocity, transform.up);
            currentSpeed = actualSpeed;
            float desired = throttle * (throttle >= 0 ? forwardSpeed : reverseSpeed);
            float rate = 0;
            if (handling)
            {
                bool opposing = Mathf.Abs(currentSpeed) > 0.05f && currentSpeed * throttle < 0;
                rate = opposing ? handling.braking : Mathf.Abs(throttle) < 0.01f ? handling.deceleration : handling.acceleration;
            }
            currentSpeed = handling ? Mathf.MoveTowards(currentSpeed, desired, Mathf.Max(0, rate) * dt) : desired;
            float speed01 = Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(0.01f, forwardSpeed));
            float turning = handling ? Mathf.Lerp(handling.lowSpeedTurn, handling.highSpeedTurn, speed01) : turnSpeed;
            float angle = body.rotation - steering * turning * dt;
            body.MoveRotation(angle);
            Vector2 forward = Quaternion.Euler(0, 0, angle) * Vector2.up;
            body.linearVelocity = forward * currentSpeed;
            Vector2 p = body.position;
            Vector2 bounded = new Vector2(Mathf.Clamp(p.x, -arenaHalfSize.x + 0.6f, arenaHalfSize.x - 0.6f), Mathf.Clamp(p.y, -arenaHalfSize.y + 0.6f, arenaHalfSize.y - 0.6f));
            if (p != bounded) body.position = bounded;
        }
        void OnDisable() { if (body) { body.linearVelocity = Vector2.zero; body.angularVelocity = 0; } }
    }
}

```

MoveTowards는 초당 변화량×fixedDeltaTime만큼 목표 속도에 접근합니다.
매 프레임 실제 물리 속도를 읽으므로 섬에 막힌 상태에서 내부 속도만 계속 쌓이지 않습니다.
반대 방향 입력은 제동값, 무입력은 감속값, 같은 방향 입력은 가속값을 사용합니다.
현재 속도가 높아질수록 선회 속도를 110→65deg/s로 줄입니다.

## 3. 에셋과 Inspector
1. Create > Pirate Game > Feel > Ship Handling으로 FeelLab/Data/PlayerHandling.asset을 만듭니다.
2. PlayerShip_Feel 루트의 ShipMotor.Handling에 연결합니다.
3. Rigidbody2D.Linear Damping을 **0**으로 설정합니다. 기존 2를 유지하면 별도 감쇠가 가속과 겹쳐 목표 속도에 도달하기 어렵습니다.
4. Angular Damping=2, Gravity Scale=0, Interpolate와 Continuous는 유지합니다.

| 대상 | 필드 | 시작값 |
|---|---|---:|
| PlayerHandling | Acceleration | 3 m/s² |
| PlayerHandling | Deceleration | 4 m/s² |
| PlayerHandling | Braking | 6 m/s² |
| PlayerHandling | Low Speed Turn | 110 deg/s |
| PlayerHandling | High Speed Turn | 65 deg/s |
| ShipMotor | Forward / Reverse Speed | 3.5 / 1.75 |
| ShipMotor | Arena Half Size | (12,6.5) |

적에게 적용하는 것은 선택입니다. 적용할 경우 적 프리팹도 Damping=0으로 바꾸고 별도 설정을 만드세요.
기본 PlayerShip 프리팹에는 Handling을 연결하지 않습니다.

## 4. 적용 확인
- W를 누르면 약 1.2초에 걸쳐 최대 속도에 접근합니다.
- W를 놓으면 점진적으로 정지합니다.
- 전진 중 S를 누르면 제동 후 후진합니다.
- 저속과 고속의 선회 반경이 달라집니다.
- 섬을 향해 계속 전진한 뒤 빠져나올 때 속도가 비정상적으로 튀지 않습니다.
- 경계를 넘어가지 않으며 사망 후 움직임이 정지합니다.

## 비교와 복구
Handling을 None으로 바꾸면 기존 직접 이동입니다. 비교 시 Rigidbody2D Damping도 기존 2로 복구합니다.
Acceleration=2/3/5, High Speed Turn=45/65/90을 하나씩 비교하세요.
이 단원의 변경은 1차 씬에도 로드되는 공용 코드이므로 기본 씬의 루프도 확인합니다.
