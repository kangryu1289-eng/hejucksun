# 실습 1 — 포물선 모양 조준 인디케이터

## 목표
직선 기본 안내를 곡선과 착탄 원으로 바꿉니다. 거리와 발사 가능 여부를 눈으로 읽습니다.
곡선은 XY 화면상의 시각 안내이며 실제 포탄은 직선으로 이동합니다.
하늘 높이 또는 실제 중력 탄도 예측을 뜻하지 않습니다. 섬에 막힐 수 있고 적 이동 예측도 하지 않습니다.

## 오브젝트와 컴포넌트
1. Stop → PlayerShip 프리팹을 엽니다.
2. 루트 아래 빈 오브젝트 AimArc와 LandingRing을 만듭니다. Local Position=(0,0,0), Scale=(1,1,1).
3. 각각 LineRenderer를 추가합니다.
4. 두 LineRenderer의 Material에 Materials/SpriteUnlit을 연결합니다.
5. Use World Space=On, Width=0.035, Sorting Order=4.
6. AimArc는 Loop=Off, LandingRing은 Loop=On. 점 개수는 코드가 갱신합니다.
7. 아래 코드로 AimArcIndicator.cs를 교체합니다.
8. 루트에 AimArcIndicator를 추가하고 참조를 연결합니다.

| 필드 | 연결/값 |
|---|---|
| Aim | 루트의 CannonAim |
| Health | 루트의 ShipHealth |
| Arc | AimArc의 LineRenderer |
| Landing | LandingRing의 LineRenderer |
| Height | 1.2 |
| Radius | 0.25 |
| Segments | 32 |

BasicAimGuide는 지우지 마세요. 실습 컴포넌트가 실행 중 숨기고 비활성화 시 복구합니다.

## 코드

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class AimArcIndicator : MonoBehaviour
    {
        public CannonAim aim;
        public ShipHealth health;
        public LineRenderer arc, landing;
        public float height = 1.2f, radius = 0.25f;
        public int segments = 32;
        void LateUpdate()
        {
            bool visible = aim && aim.enabled && health.IsAlive;
            arc.enabled = landing.enabled = visible;
            if (!visible) return;
            if (aim.guide) aim.guide.enabled = false;
            Vector3 from = transform.position;
            Vector3 to = (Vector2)from + aim.Direction * Mathf.Min(aim.Distance, aim.range);
            int count = Mathf.Max(8, segments);
            arc.positionCount = count + 1;
            landing.positionCount = count;
            landing.loop = true;
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                arc.SetPosition(i, Vector3.Lerp(from, to, t) + Vector3.up * (4 * height * t * (1-t)));
            }
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2 / count;
                landing.SetPosition(i, to + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
            }
            Color c = aim.InRange ? new Color(0.6f,1,0.8f) : new Color(1,0.35f,0.25f);
            arc.startColor = arc.endColor = landing.startColor = landing.endColor = c;
        }
        void OnDisable()
        {
            if (arc) arc.enabled = false;
            if (landing) landing.enabled = false;
            if (aim && aim.guide) aim.guide.enabled = true;
        }
    }
}
```

## 핵심
Lerp(from,to,t)는 두 점 사이 직선입니다. 여기에4×height×t×(1-t)를 더하면 시작과 끝은0, 중간은height인 포물선 모양이 됩니다.
사거리 밖에서는 끝점을 최대 사거리로 제한하고 빨간색으로 바꿉니다.
색상은 원과 곡선에 동시에 적용합니다. 기본 포탄 규칙은 수정하지 않습니다.

## 완료 확인
- 커서를 움직이면 곡선 끝과 착탄 원이 같이 이동합니다.
- 8m 밖은 빨간색이며 클릭해도 발사되지 않습니다.
- 0.7m 미만도 빨간색입니다.
- 컴포넌트를 끄면 기존 직선 안내가 복구됩니다.
- 격침/결과 화면에서는 안내가 사라집니다.

## 작은 과제
Height를0, 0.4, 1.2로 비교하세요. Radius를0.15~0.4로 바꾸어 조준점 가독성을 비교하세요.
