# 실습 2 — 발사 반동

## 목표
대포 발사 때 선박 그림이 반대 방향으로 밀렸다 돌아오게 만듭니다.
물리 루트는 움직이지 않고Visual 자식만 움직입니다.

## 적용 절차
1. 아래 코드로 CannonRecoil.cs를 교체합니다.
2. PlayerShip 프리팹 루트에 CannonRecoil을 추가합니다.
3. Visual 필드에 자식 Visual Transform을 연결합니다.
4. Distance=0.12m, Recovery=0.14초를 입력합니다.
5. EnemyShip 프리팹에도 같은 컴포넌트와 참조를 추가합니다.
6. 저장 후 Round1을 플레이합니다.

## 코드

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class CannonRecoil : MonoBehaviour
    {
        public Transform visual;
        public float distance = 0.12f, recovery = 0.14f;
        CombatFeedbackEvents events;
        Vector3 rest;
        void Awake() => rest = visual.localPosition;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.ShotFired += OnShot;
        }
        void OnShot(ShotFeedback shot)
        {
            if (shot.Shooter != transform) return;
            Vector3 local = transform.InverseTransformDirection((Vector3)shot.Direction);
            visual.localPosition = rest - local * distance;
        }
        void Update() => visual.localPosition = Vector3.MoveTowards(visual.localPosition, rest,
            distance / Mathf.Max(0.01f, recovery) * Time.deltaTime);
        void OnDisable()
        {
            if (events) events.ShotFired -= OnShot;
            if (visual) visual.localPosition = rest;
        }
    }
}
```

## 핵심
ShotFeedback.Shooter와 자기Transform을 비교하여 다른 배의 발사에 반응하지 않게 합니다.
이벤트의 방향은 월드 좌표이므로 InverseTransformDirection으로 로컬 좌표로 바꿉니다.
매번 원래 위치를 기준으로 반동을 주어 연속 발사 시 위치가 누적되지 않게 합니다.

## 완료 확인
상하좌우로 쏠 때 항상 발사 반대 방향으로Visual이 움직여야 합니다.
Inspector에서 루트 Rigidbody2D 위치는 반동 때문에 변하지 않아야 합니다.
적이 발사할 때 플레이어까지 같이 흔들리면 Shooter 비교를 확인하세요.
컴포넌트를 비활성화하면Visual이 원래 위치로 돌아와야 합니다.

## 작은 과제
Distance=0.04/0.12/0.25, Recovery=0.08/0.14/0.25를 비교하고 가장 읽기 좋은 조합을 기록하세요.
