# 실습 6 — 발사·명중·물보라·격침 연출

## 목표와 순서
6-A 발사 → 6-B 명중/물보라 → 6-C 격침으로 나눠 적용합니다.
Unity ParticleSystem과 SpriteRenderer를 사용합니다. 별도 트윈 패키지는 필요하지 않습니다.
입자 프리팹 세 개를 직접 만들어 연출 구성과 수명 관리를 실습합니다.

## 6-A. 포구 파티클 프리팹
1. Stop 상태에서 빈 오브젝트 MuzzleBurst를 만듭니다.
2. ParticleSystem을 추가하고 Position/Rotation=(0,0,0), Scale=(1,1,1).
3. 아래 표대로 설정합니다.

| 모듈/필드 | 값 |
|---|---|
| Main / Duration | 0.3 |
| Main / Looping | Off |
| Main / Play On Awake | On |
| Main / Start Lifetime | 0.12 |
| Main / Start Speed | 0.8 |
| Main / Start Size | 0.22 |
| Main / Start Color | (1,0.75,0.25,1) |
| Main / Gravity Modifier | 0 |
| Main / Simulation Space | World |
| Main / Stop Action | Destroy |
| Emission / Rate over Time | 0 |
| Emission / Bursts | Time=0, Count=6 |
| Shape / Shape | Circle |
| Shape / Radius | 0.08 |
| Color over Lifetime | On, Alpha 1→0 |
| Renderer / Render Mode | Billboard |
| Renderer / Material | Materials/SpriteUnlit |
| Renderer / Sorting Order | 12 |

4. Prefabs 폴더로 드래그하여 MuzzleBurst.prefab을 저장합니다.
5. 씬의 임시 인스턴스는 비활성화합니다. 실제 발사 때 코드가 새 인스턴스를 만듭니다.
6. CannonFireEffect.cs를 아래 코드로 교체합니다.
7. Juice Lab에 CannonFireEffect를 추가합니다.
8. Muzzle Prefab에 MuzzleBurst의 ParticleSystem을 연결합니다.
9. Effects Root에 World/Effects를 연결하고 씬을 저장합니다.

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class CannonFireEffect : MonoBehaviour
    {
        public ParticleSystem muzzlePrefab;
        public Transform effectsRoot;
        CombatFeedbackEvents events;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.ShotFired += OnShot;
        }
        void OnShot(ShotFeedback shot)
        {
            if (!muzzlePrefab) return;
            float angle = Mathf.Atan2(shot.Direction.y, shot.Direction.x) * Mathf.Rad2Deg;
            Instantiate(muzzlePrefab, shot.Position, Quaternion.Euler(0,0,angle), effectsRoot);
        }
        void OnDisable() { if (events) events.ShotFired -= OnShot; }
    }
}
```

포구에 짧게 입자가 나타나고 World/Effects의 인스턴스가 자동으로 사라지는지 확인합니다.
색이 표시되지 않으면 Renderer의 Material을 확인합니다.
흰색 텍스처를 사용하는 Sprites/Default 재질은 Particle Color를 반영합니다.

## 6-B. 명중과 물보라
Project에서 MuzzleBurst를 복제하여 HitBurst와 WaterSplash 프리팹을 만듭니다.

| 필드 | HitBurst | WaterSplash |
|---|---|---|
| Main Duration | 0.35 | 0.45 |
| Start Lifetime | 0.22 | 0.35 |
| Start Speed | 1.8 | 1.1 |
| Start Size | 0.16 | 0.1 |
| Start Color | (1,0.45,0.15,1) | (0.7,0.95,1,1) |
| Burst Count | 12 | 10 |
| Shape Radius | 0.05 | 0.08 |
| Stop Action | Destroy | Destroy |
| Sorting Order | 12 | 11 |

Looping=Off, Rate over Time=0, Color over Lifetime의 Alpha 1→0을 유지합니다.
Art/Explosion1~3으로 새 Particle Material을 만드는 것은 선택 확장 과제입니다.
이번 완료 조건은 기본 입자만으로 확인할 수 있습니다.

1. ImpactEffect.cs를 아래 코드로 교체합니다.
2. Juice Lab에 ImpactEffect를 추가합니다.
3. Hit Prefab에는 HitBurst, Splash Prefab에는 WaterSplash의 ParticleSystem을 연결합니다.
4. Effects Root에 World/Effects를 연결합니다.

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class ImpactEffect : MonoBehaviour
    {
        public ParticleSystem hitPrefab, splashPrefab;
        public Transform effectsRoot;
        CombatFeedbackEvents events;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) { events.Hit += OnHit; events.WaterMiss += OnMiss; }
        }
        void Spawn(ParticleSystem prefab, Vector2 position)
        { if (prefab) Instantiate(prefab, position, Quaternion.identity, effectsRoot); }
        void OnHit(HitFeedback hit) => Spawn(hitPrefab, hit.Position);
        void OnMiss(Vector2 position) => Spawn(splashPrefab, position);
        void OnDisable() { if (events) { events.Hit -= OnHit; events.WaterMiss -= OnMiss; } }
    }
}
```

적 명중은 주황색, 빈 바다나 섬 충돌은 푸른색 효과가 나와야 합니다.
같은 컴포넌트를 두 번 부착하면 이벤트당 효과가 두 개씩 생깁니다. 하나만 부착하세요.
ParticleSystem의 Stop Action=Destroy가 수명 종료 시 인스턴스를 정리합니다.
World/Effects는 전투 직후 결과창 뒤에서도 입자가 수명 동안 마무리되는 컨테이너입니다.
정비로 즉시 돌아갈 때 남은 입자도 정리하려면 BattleController.Clear에서 Effects 자식을 정리하는 방식으로 확장할 수 있습니다.

## 6-C. 격침 Visual
1. ShipSinkingEffect.cs를 아래 코드로 교체합니다.
2. PlayerShip과 EnemyShip 프리팹 루트에 각각 ShipSinkingEffect를 추가합니다.
3. Visual에 자식 Visual의 Transform, Sprite에 같은 자식의 SpriteRenderer를 연결합니다.
4. Duration=0.65초, Rotation=65도로 설정하고 프리팹을 저장합니다.

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class ShipSinkingEffect : MonoBehaviour
    {
        public Transform visual;
        public SpriteRenderer sprite;
        public float duration = 0.65f, rotation = 65f;
        CombatFeedbackEvents events;
        ShipHealth health;
        Vector3 restScale;
        Quaternion restRotation;
        Color restColor;
        bool sinking;
        float elapsed;
        void Awake()
        {
            health = GetComponent<ShipHealth>(); restScale = visual.localScale;
            restRotation = visual.localRotation; restColor = sprite.color;
        }
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.Sunk += OnSunk;
        }
        void OnSunk(SinkFeedback sink) { if (sink.Target == health) { sinking = true; elapsed = 0; } }
        void LateUpdate()
        {
            if (!sinking) return;
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Clamp(duration,0.05f,1.5f));
            visual.localScale = restScale * Mathf.Lerp(1,0.2f,t);
            visual.localRotation = restRotation * Quaternion.Euler(0,0,rotation*t);
            sprite.color = new Color(restColor.r,restColor.g,restColor.b,1-t);
        }
        void OnDisable()
        {
            if (events) events.Sunk -= OnSunk;
            if (visual) { visual.localScale = restScale; visual.localRotation = restRotation; }
            if (sprite) sprite.color = restColor;
            sinking = false;
        }
    }
}
```

ShipHealth는 사망 후 2초에 삭제합니다. 예제는 최대 1.5초로 제한하여 그 전에 연출을 끝냅니다.
루트 Transform이나 Collider는 애니메이션하지 않습니다.
피격 플래시는 사망한 배의 색 복구를 멈춰 격침 페이드와 충돌하지 않게 합니다.

## 완료 확인
- 발사 성공 때만 포구 효과가 나타납니다.
- 명중과 빗나감을 색상과 위치로 구분할 수 있습니다.
- 중간 적 격침 시 축소·회전·페이드 후 삭제됩니다.
- 마지막 적 격침은 결과창 뒤에서 마무리됩니다.
- 여러 라운드를 반복해도 파티클 인스턴스가 끝없이 증가하지 않습니다.
