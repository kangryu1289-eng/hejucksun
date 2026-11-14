# 2-05 — 적 공격 예고와 회피

## 시작 조건
다른 2차 단원이 필요 없습니다. EnemyShipAI의 기존 이동과 목표 연결을 유지합니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 새 클래스의 예제는 해당 파일 전체를 교체합니다.

## 목표와 관계
EnemyShipAI → EnemyAttackController → CannonWeapon.
공격 예약은 Controller가, 선 표시는 EnemyAttackTelegraph가 담당합니다.
대기 → 목표 위치 확정/예고 → 발사 시도 → 재장전 순서입니다.
다음 예고는 재장전이 끝난 후 시작하므로 기존보다 예고 시간만큼 공격 간격이 길어집니다.

## 1. 예고 표시 클래스
Scripts/Feel/EnemyAttackTelegraph.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class EnemyAttackTelegraph : MonoBehaviour
    {
        public LineRenderer line;
        public void Show(Vector2 origin, Vector2 target)
        {
            line.enabled = true; line.positionCount = 2;
            line.SetPosition(0,origin); line.SetPosition(1,target);
        }
        public void Hide() { if (line) line.enabled = false; }
        void OnDisable() => Hide();
    }
}
```

## 2. 공격 예약 클래스
Scripts/Feel/EnemyAttackController.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    [RequireComponent(typeof(CannonWeapon),typeof(ShipHealth))]
    public sealed class EnemyAttackController : MonoBehaviour
    {
        public EnemyAttackTelegraph telegraph;
        public float warningSeconds = 0.55f;
        CannonWeapon weapon;
        ShipHealth health, target;
        EnemyShipAI ai;
        BattleController battle;
        Vector2 lockedTarget;
        float fireAt, retryAt;
        bool pending;
        void Awake()
        {
            weapon = GetComponent<CannonWeapon>(); health = GetComponent<ShipHealth>();
            ai = GetComponent<EnemyShipAI>(); battle = FindFirstObjectByType<BattleController>();
        }
        public void TickAttack(ShipHealth value)
        {
            if (!isActiveAndEnabled || !health.IsAlive || !value || !value.IsAlive || !battle || !battle.Running)
            { Cancel(); return; }
            target = value;
            if (pending)
            {
                telegraph.Show(transform.position,lockedTarget);
                if (Time.time < fireAt) return;
                bool fired = weapon.TryFire(lockedTarget);
                Cancel(); retryAt = Time.time + (fired ? 0 : 0.2f);
                return;
            }
            float distance = Vector2.Distance(transform.position,value.transform.position);
            if (Time.time < retryAt || weapon.Reload01 < 1 || distance < 0.7f || distance > weapon.range) return;
            lockedTarget = value.transform.position;
            fireAt = Time.time + Mathf.Max(0.05f,warningSeconds);
            pending = true; telegraph.Show(transform.position,lockedTarget);
        }
        void Update()
        {
            if (pending && (!health.IsAlive || !target || !target.IsAlive || !ai || !ai.enabled || !battle || !battle.Running)) Cancel();
        }
        public void Cancel() { pending = false; target = null; if (telegraph) telegraph.Hide(); }
        void OnDisable() => Cancel();
    }
}
```

예고 시점의 좌표를 lockedTarget에 저장합니다. 플레이어가 움직여도 이 좌표는 따라가지 않습니다.
선의 시작점은 움직이는 적 선박을 따라가지만 끝점은 고정됩니다.
발사 시 현재 위치에서 저장한 목표가 사거리 밖이면 취소하고 0.2초 후 다시 시도합니다.
실제 발사 성공에만 무기의 재장전과 ShotFired가 발생합니다.

## 3. 기존 AI 연결
Scripts/Ships/EnemyShipAI.cs 전체를 교체합니다.

```csharp
using UnityEngine;
using PirateGame.Feel;
namespace PirateGame
{
    [RequireComponent(typeof(ShipMotor), typeof(CannonWeapon), typeof(ShipHealth))]
    public sealed class EnemyShipAI : MonoBehaviour
    {
        public ShipHealth target;
        public float preferredDistance = 4.2f;
        ShipMotor motor; CannonWeapon weapon; ShipHealth health;
        EnemyAttackController attack;
        void Awake() { motor = GetComponent<ShipMotor>(); weapon = GetComponent<CannonWeapon>(); health = GetComponent<ShipHealth>(); attack = GetComponent<EnemyAttackController>(); }
        void Update()
        {
            if (!health.IsAlive || !target || !target.IsAlive) { motor.SetInput(0, 0); if (attack) attack.Cancel(); return; }
            Vector2 delta = target.transform.position - transform.position;
            float angle = Vector2.SignedAngle(transform.up, delta);
            float turn = -Mathf.Clamp(angle / 35f, -1, 1);
            float forward = delta.magnitude > preferredDistance ? (Mathf.Abs(angle) < 70 ? 1f : 0.2f) : 0f;
            motor.SetInput(forward, turn);
            if (attack && attack.enabled) attack.TickAttack(target);
            else weapon.TryFire(target.transform.position);
        }
    }
}

```

기존 이동·선회는 그대로입니다. EnemyAttackController가 없거나 비활성화되면 기존 즉시 사격으로 동작합니다.
Controller에 별도 목표를 Inspector로 연결하지 않습니다. AI가 기존 target을 전달합니다.

## 4. 프리팹 구성
1. EnemyShip_Feel 루트에 EnemyAttackController와 EnemyAttackTelegraph를 추가합니다.
2. 자식 AttackWarning을 만들고 LineRenderer를 추가합니다.
3. Position=(0,0,0), Scale=(1,1,1), Use World Space=On, Width=0.04, Position Count=2.
4. Material=SpriteUnlit, Sorting Order=1, Start/End Color=(1,0.35,0.25,0.55).
5. LineRenderer Enabled=Off. 코드가 예고 중에만 켭니다.
6. Telegraph.Line에 자식의 LineRenderer를 연결합니다.
7. Controller.Telegraph에 같은 루트의 EnemyAttackTelegraph를 연결합니다.
8. Warning Seconds=0.55초로 설정합니다.

플레이어에게 이 컴포넌트를 추가하지 않습니다.

## 5. 완료 확인
- 적이 발사하기 전에 빨간 선이 나타납니다.
- 예고 직후 이동하면 선의 끝점은 이전 위치에 남습니다.
- 정지 상태와 이동 상태에서 맞는 빈도를 비교합니다.
- 예고 중 적을 격침하면 선과 예약 공격이 취소됩니다.
- 플레이어 사망이나 결과 화면에서도 추가 발사가 일어나지 않습니다.
- Controller를 끄면 예고 없이 기존 방식으로 발사합니다.

## 문제와 비교
발사가 두 번 나가면 기존 weapon.TryFire 호출을 남긴 채 추가 호출하지 않았는지 확인하세요.
이 예제는 예고된 점을 향한 **방향 사격**이며 포탄은 그 점을 지나 최대 사거리까지 이동할 수 있습니다.
Warning Seconds=0.3/0.55/0.8을 비교하고 적 공격력은 동시에 바꾸지 않습니다.
