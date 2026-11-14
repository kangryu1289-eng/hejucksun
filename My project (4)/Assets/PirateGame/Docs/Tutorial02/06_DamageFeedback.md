# 2-06 — 피해 강도와 위험 상태

## 시작 조건
손상·연기·경고는 독립 실행할 수 있습니다. CameraShake/HitStop의 강도 연결은 해당 1차 실습을 완료한 경우에 적용합니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 새 클래스의 예제는 해당 파일 전체를 교체합니다.

## 목표
일반 명중·플레이어 피격·격침을 구분하고, 저체력 상태를 외형과 소리로 알립니다.
주 스프라이트 색을 매 프레임 바꾸지 않아 기존 피격 플래시·격침 페이드와 충돌을 줄입니다.

## 1. 강도 데이터
Scripts/Feel/FeedbackProfile.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    [CreateAssetMenu(menuName = "Pirate Game/Feel/Feedback Profile")]
    public sealed class FeedbackProfile : ScriptableObject
    {
        public float enemyHitShake = 0.08f, playerHitShake = 0.12f, sinkShake = 0.18f;
        public float hitStopSeconds = 0.045f;
        public float damageThreshold = 0.5f, criticalThreshold = 0.25f;
        public float impactDistance = 0.08f;
        public float Shake(HitFeedback hit)
        {
            float factor = Mathf.Clamp(hit.Damage / 20f,0.5f,1.5f);
            return (hit.IsPlayer ? playerHitShake : enemyHitShake) * factor;
        }
    }
}
```

Create > Pirate Game > Feel > Feedback Profile로 FeelLab/Data/CombatFeedback.asset을 만듭니다.
기본값은 적 명중 0.08, 플레이어 피격 0.12, 격침 0.18, Hit Stop 0.045초입니다.
Damage Threshold=0.5, Critical Threshold=0.25, Impact Distance=0.08.

## 2. 손상 표시와 피격 밀림
Scripts/Feel/ShipDamageVisual.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class ShipDamageVisual : MonoBehaviour
    {
        public FeedbackProfile profile;
        public ShipHealth health;
        public Transform impactPivot;
        public GameObject damageMark;
        public ParticleSystem smoke;
        public AudioSource warningSource;
        public AudioClip warningSound;
        public float recovery = 0.12f, warningInterval = 2f;
        CombatFeedbackEvents events;
        Vector3 rest, displacement;
        float nextWarning;
        bool critical;
        void Awake() => rest = impactPivot.localPosition;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.Hit += OnHit;
        }
        void OnHit(HitFeedback hit)
        {
            if (hit.Target != health) return;
            Vector2 away = (Vector2)transform.position - hit.Position;
            if (away.sqrMagnitude < 0.001f) away = -transform.up;
            displacement = transform.InverseTransformDirection((Vector3)away.normalized) * profile.impactDistance;
        }
        void LateUpdate()
        {
            bool alive = health.IsAlive && health.MaxHull > 0;
            float ratio = alive ? health.Hull / health.MaxHull : 0;
            damageMark.SetActive(alive && ratio <= profile.damageThreshold);
            bool danger = alive && ratio <= profile.criticalThreshold;
            if (danger != critical)
            {
                critical = danger;
                if (critical) { smoke.Play(); nextWarning = Time.unscaledTime; }
                else { smoke.Stop(true,ParticleSystemStopBehavior.StopEmitting); if(warningSource) warningSource.Stop(); }
            }
            if (critical && warningSource && warningSound && Time.unscaledTime >= nextWarning)
            {
                warningSource.PlayOneShot(warningSound,0.35f);
                nextWarning = Time.unscaledTime + Mathf.Max(0.2f,warningInterval);
            }
            displacement = Vector3.MoveTowards(displacement,Vector3.zero,profile.impactDistance / Mathf.Max(0.01f,recovery) * Time.deltaTime);
            impactPivot.localPosition = rest + displacement;
        }
        void OnDisable()
        {
            if(events) events.Hit -= OnHit;
            if(impactPivot) impactPivot.localPosition = rest;
            if(damageMark) damageMark.SetActive(false);
            if(smoke) smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(warningSource) warningSource.Stop();
            displacement = Vector3.zero; critical = false;
        }
    }
}
```

HitFeedback의 실제 명중점에서 배 중심을 향하는 방향으로 Visual 계층을 짧게 밀어냅니다.
명중점과 중심이 같으면 배 뒤쪽 방향을 사용합니다.
최소 회복 시간 0.01초로 나눗셈을 보호하고, 연기는 위험 상태 진입/이탈 시에만 재생·정지합니다.

## 3. 선박 계층 재구성
PlayerShip_Feel을 다음 계층으로 만듭니다.

```text
PlayerShip_Feel
  ImpactPivot
    RecoilPivot
      Visual
        DamageMark
  DamageSmoke
  WarningAudio
  (기존 조준 안내와 항적 자식)
```

새 Pivot은 Local Position=(0,0,0), Rotation=(0,0,0), Scale=(1,1,1).
Visual을 RecoilPivot 아래로 옮긴 뒤 기존 Local Position=0, Rotation Z=180, Scale=1.5를 확인합니다.
DamageMark는 SpriteRenderer를 가진 자식입니다. Visual과 같은 Sprite, Local Position=0, Rotation=0, Scale=1.
Material=SpriteUnlit, Color=(0.2,0.15,0.1,0.35), Sorting Order=3, 초기 Active=Off.
주 Visual 위에 얇은 어두운 손상 틴트를 올리는 입문 표현입니다.

1차 CannonRecoil.Visual은 RecoilPivot에 연결합니다.
ShipHitFlash.Visual은 원래 Visual의 SpriteRenderer에 연결합니다.
ShipSinkingEffect.Visual은 원래 Visual Transform, Sprite는 해당 SpriteRenderer에 연결합니다.
피격 밀림은 ImpactPivot을 사용하므로 반동과 서로 위치를 덮어쓰지 않습니다.

## 4. 연기와 경고음
DamageSmoke: Local Position=(0,0,0), ParticleSystem 추가.

| 필드 | 값 |
|---|---|
| Looping / Play On Awake | On / Off |
| Start Lifetime / Speed / Size | 0.8 / 0.3 / 0.2 |
| Start Color | (0.15,0.15,0.15,0.45) |
| Simulation Space / Gravity | World / 0 |
| Emission Rate | 6 |
| Shape | Circle, Radius 0.12 |
| Color over Lifetime | Alpha 0.45→0 |
| Renderer Material / Order | SpriteUnlit / 4 |
| Stop Action | None |

WarningAudio에 전용 AudioSource를 추가합니다.
Play On Awake=Off, Loop=Off, Spatial Blend=0.
오디오를 다른 효과와 공유하면 경고 종료 시 다른 소리까지 끊을 수 있으므로 전용 소스를 사용합니다.

루트에 ShipDamageVisual을 추가하고 Profile/Health/Impact Pivot/Damage Mark/Smoke를 각각 연결합니다.
Warning Source=WarningAudio, Warning Sound=짧은 경고음 또는 Audio/Hit.wav.
Recovery=0.12초, Warning Interval=2초.
적에게도 적용할 수 있지만 적의 Warning Source/Sound는 비워 경고음 혼잡을 줄입니다.

## 5. 1차 카메라·Hit Stop에 강도 데이터 연결(선택)
다음은 1차 문서 예제 구현 기준입니다. 아직 빈 클래스라면 먼저 1차 실습을 완료하세요.
CameraShake.cs와 HitStop.cs 위에 using PirateGame.Feel;을 추가하고 각 클래스에 public FeedbackProfile profile;을 추가합니다.

CameraShake의 OnHit/OnSunk 두 메서드를 교체합니다.

```csharp
void OnHit(HitFeedback hit) => Request(profile ? profile.Shake(hit) : hitStrength * (hit.IsPlayer ? 1.4f : 1));
void OnSunk(SinkFeedback sink) => Request(profile ? profile.sinkShake : sinkStrength);
```

HitStop.OnHit의 remaining = Mathf.Max(remaining, seconds); 줄을 교체합니다.

```csharp
remaining = Mathf.Max(remaining, profile ? profile.hitStopSeconds : seconds);
```

두 컴포넌트의 Profile에 CombatFeedback.asset을 연결합니다.
프로필을 None으로 두면 원래 1차 강도를 사용합니다.
새 카메라 흔들림이나 Hit Stop 컴포넌트를 중복 부착하지 않습니다.

## 6. 완료 확인과 비교
- HP 50% 이하에서 DamageMark가 표시됩니다.
- HP 25% 이하에서 연기와 2초 간격 경고가 나타납니다.
- 사망 후 경고가 멈추고 기존 격침 연출이 진행됩니다.
- 반동과 피격 밀림이 다른 Pivot에서 동작합니다.
- 적 일반 명중보다 플레이어 피격과 격침의 강조가 큽니다.
- 컴포넌트를 꺼도 내구도와 승패 판정은 유지됩니다.

적 데미지를 실습용 라운드 복사본에서 25로 설정하면 50/25% 경계를 보기 쉽습니다.
위험 상태에서 EnemyShipAI를 잠깐 비활성화해 연기를 확인하고 Stop 후 원래 라운드 수치를 복구하세요.
