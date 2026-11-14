# 2-04 — 조준점 재장전 게이지와 완료음

## 시작 조건
다른 2차 단원이 필요 없습니다. 1차 AimArcIndicator와 함께 써도 되고 기본 직선 안내만 있어도 작동합니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 이 문서의 코드는 해당 파일 전체를 교체하는 완성 예제입니다.
프로젝트의 새 클래스는 TODO만 있는 상태입니다. 코드를 입력·컴파일한 다음 Inspector를 연결하세요.

## 목표와 관계
ReloadIndicator → CannonAim/CannonWeapon/ShipHealth.
ShotFired를 자기 발사에 한해 받아 재장전 시작을 기록하고 Reload01로 완료를 감지합니다.
진행도를 관찰해 표시할 뿐 무기 재장전 값을 변경하지 않습니다.

## 1. 코드
Scripts/Feel/ReloadIndicator.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class ReloadIndicator : MonoBehaviour
    {
        public CannonAim aim;
        public CannonWeapon weapon;
        public ShipHealth health;
        public LineRenderer ring;
        public AudioSource audioSource;
        public AudioClip readySound;
        public float radius = 0.36f;
        public int segments = 40;
        CombatFeedbackEvents events;
        bool wasReloading;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.ShotFired += OnShot;
            wasReloading = false;
        }
        void OnShot(ShotFeedback shot) { if (shot.Shooter == transform) wasReloading = true; }
        void LateUpdate()
        {
            bool visible = aim && aim.enabled && health.IsAlive;
            ring.enabled = visible;
            if (!visible) return;
            float progress = weapon.Reload01;
            if (wasReloading && progress >= 1)
            {
                wasReloading = false;
                if (audioSource && readySound) audioSource.PlayOneShot(readySound,0.4f);
            }
            Vector3 center = (Vector2)transform.position + aim.Direction * Mathf.Min(aim.Distance,aim.range);
            int points = Mathf.Max(8,segments);
            ring.loop = false; ring.positionCount = points + 1;
            for(int i=0;i<=points;i++)
            {
                float angle = Mathf.PI * 0.5f + (float)i / points * Mathf.PI * 2 * progress;
                ring.SetPosition(i,center + new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0) * radius);
            }
            Color color = !aim.InRange ? new Color(1,0.35f,0.25f) :
                wasReloading ? new Color(1,0.78f,0.37f) : new Color(0.6f,1,0.8f);
            ring.startColor = ring.endColor = color;
        }
        void OnDisable()
        {
            if (events) events.ShotFired -= OnShot;
            if (ring) ring.enabled = false;
            wasReloading = false;
        }
    }
}
```

## 2. 오브젝트 구성
PlayerShip_Feel 아래 ReloadRing과 ReloadAudio 자식을 만듭니다.
ReloadRing에 LineRenderer, ReloadAudio에 AudioSource를 추가합니다.

| 대상 | 설정 |
|---|---|
| ReloadRing Transform | Local Position=(0,0,0), Scale=(1,1,1) |
| LineRenderer | Use World Space=On, Loop=Off, Width=0.035 |
| Material / Sorting Order | SpriteUnlit / 5 |
| AudioSource | Play On Awake=Off, Loop=Off, Spatial Blend=0, Volume=1 |

루트에 ReloadIndicator를 추가하고 Aim/Weapon/Health에 같은 루트의 해당 컴포넌트를 연결합니다.
Ring=ReloadRing의 LineRenderer, Audio Source=ReloadAudio.
Ready Sound에는 Audio/Hit.wav를 임시로 연결하거나 짧은 장전 소리를 준비합니다.
Radius=0.36, Segments=40. 1차 착탄 원 Radius=0.25보다 바깥에 배치됩니다.

## 3. 표시와 소리 규칙
재장전 중은 금색, 완료는 초록, 사거리 밖은 빨강입니다.
실제 발사 후 완료로 바뀔 때만 완료음을 한 번 재생합니다.
출항 직후 이미 장전된 상태에서는 소리를 내지 않습니다.
조준점이 최대 사거리 밖이면 게이지 위치는 최대 사거리로 제한합니다.

## 4. 완료 확인
- 출항할 때 완료음이 울리지 않습니다.
- 한 번 쏘면 원호가 차오르고 완료음이 한 번만 납니다.
- 좌클릭을 누르고 연사해도 장전 완료음이 매 프레임 반복되지 않습니다.
- 정비에서 재장전 강화 후 게이지가 더 빨리 찹니다.
- 전투 종료 시 조준이 꺼지면 게이지도 숨습니다.
- 재출항해도 소리가 중복되지 않습니다.

## 비교와 복구
상단 HUD의 재장전 바와 조준점 주변 표시를 번갈아 보며 시선 이동을 비교합니다.
게이지를 끄려면 ReloadIndicator를 비활성화합니다. 기본 CannonWeapon은 그대로 작동합니다.
