# 2-08 — 보상 카운트업과 강화 미리보기

## 시작 조건
07 종료 연출이 없어도 기존 ResultPanel/DockPanel에서 독립 실행합니다. UI는 모두 TMP를 사용합니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 새 클래스의 예제는 해당 파일 전체를 교체합니다.

## 목표와 책임
RewardPresentation은 이미 확정·지급된 보상을 숫자로 표시합니다.
UpgradePreview는 PlayerProgress를 읽어 현재 값과 다음 값을 비교합니다.
두 컴포넌트는 Gold, 레벨, Hull을 변경하지 않습니다.

## 1. 보상 표시 코드
Scripts/Feel/RewardPresentation.cs를 교체합니다.

```csharp
using TMPro;
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class RewardPresentation : MonoBehaviour
    {
        public GameFlowController flow;
        public TMP_Text counter;
        public float duration = 0.5f;
        int reward;
        float elapsed;
        void OnEnable() { reward = flow ? flow.LastResult.Reward : 0; elapsed = 0; }
        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f,duration));
            int shown = Mathf.RoundToInt(Mathf.Lerp(0,reward,1 - (1-t)*(1-t)));
            counter.text = $"+{shown} G";
        }
    }
}
```

## 2. 결과 화면 구성
PirateBattle_Feel의 Game Canvas/Result Panel에 RewardPresentation을 추가합니다.
Reward Counter 자식을 UI > Text - TextMeshPro로 만듭니다.

| 대상 | 필드 | 값 |
|---|---|---|
| Reward Counter RectTransform | Anchor/Pivot | 중앙 (0.5,0.5) |
| RectTransform | Position / Size | (0,110) / (600,45) |
| TextMeshProUGUI | Font / Size | LiberationSans SDF / 30 |
| TextMeshProUGUI | Alignment / Color | Center / (1,0.78,0.37,1) |
| TextMeshProUGUI | Raycast Target | Off |
| Summary RectTransform | Position / Size | (0,-20) / (630,220) |

RewardPresentation.Flow=Game Systems의 GameFlowController.
Counter=Reward Counter의 TextMeshProUGUI, Duration=0.5초.

기존 Summary에 보상 줄이 중복 표시되지 않게 ResultPanel.Refresh의 summary.text에서
문자열 조각 \nREWARD  +{r.Reward} G를 제거합니다.
다른 줄과 보상 계산 코드는 바꾸지 않습니다.
수정할 대입문 전체는 다음과 같습니다.

```csharp
summary.text = $"SHIPS SUNK  {r.Sunk}\nBATTLE TIME  {r.Seconds:0.0}s\nHULL AT DOCK  {flow.Progress.Hull:0}/100"
    + (r.Victory ? "" : "\nEmergency repairs restored 50 hull.");
```

이 Summary 변경은 공용 ResultPanel이므로 기본 씬에서도 보상 줄을 유지하려면 아래 방식을 사용하세요.
- ResultPanel에 public bool showRewardInSummary = true; 필드를 추가합니다.
- 공용 대입문은 다음 조건식으로 작성하고, **2차 씬에서만 체크를 끕니다.**

```csharp
summary.text = $"SHIPS SUNK  {r.Sunk}"
    + (showRewardInSummary ? $"\nREWARD  +{r.Reward} G" : "")
    + $"\nBATTLE TIME  {r.Seconds:0.0}s\nHULL AT DOCK  {flow.Progress.Hull:0}/100"
    + (r.Victory ? "" : "\nEmergency repairs restored 50 hull.");
```

두 대입문을 동시에 넣지 않습니다. 기본 씬 비교를 위해 조건식 방식을 권장합니다.

## 3. 강화 표시 코드
Scripts/Feel/UpgradePreview.cs를 교체합니다.

```csharp
using TMPro;
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class UpgradePreview : MonoBehaviour
    {
        public GameFlowController flow;
        public TMP_Text damagePreview, reloadPreview;
        public float highlightSeconds = 0.18f;
        int previousDamage = -1, previousReload = -1;
        float damageTimer, reloadTimer;
        Color damageColor, reloadColor;
        void Awake() { damageColor = damagePreview.color; reloadColor = reloadPreview.color; }
        void OnEnable() { previousDamage = previousReload = -1; damageTimer = reloadTimer = 0; }
        void LateUpdate()
        {
            if(!flow || flow.Progress == null) return;
            var p = flow.Progress;
            if(previousDamage >= 0 && p.DamageLevel > previousDamage) damageTimer = highlightSeconds;
            if(previousReload >= 0 && p.ReloadLevel > previousReload) reloadTimer = highlightSeconds;
            previousDamage = p.DamageLevel; previousReload = p.ReloadLevel;
            damagePreview.text = p.DamageLevel >= DockService.MaxLevel ? $"Damage {p.Damage:0} / MAX" : $"Damage {p.Damage:0} -> {p.Damage+5:0}";
            reloadPreview.text = p.ReloadLevel >= DockService.MaxLevel ? $"Reload {p.ReloadSeconds:0.00}s / MAX" : $"Reload {p.ReloadSeconds:0.00}s -> {p.ReloadSeconds*0.9f:0.00}s";
            damageTimer -= Time.unscaledDeltaTime; reloadTimer -= Time.unscaledDeltaTime;
            damagePreview.color = damageTimer > 0 ? new Color(1,0.78f,0.37f) : damageColor;
            reloadPreview.color = reloadTimer > 0 ? new Color(1,0.78f,0.37f) : reloadColor;
        }
        void OnDisable()
        {
            if(damagePreview) damagePreview.color = damageColor;
            if(reloadPreview) reloadPreview.color = reloadColor;
        }
    }
}
```

값이 실제로 증가했을 때만 금색 강조를 재생합니다.
화면을 처음 열었을 때 레벨을 읽는 것은 구매로 처리하지 않습니다.

## 4. 정비 화면 구성
Dock Panel에 UpgradePreview를 추가합니다.
2차 씬의 Dock Panel 크기를 (740,590)으로 바꾸고 아래처럼 배치합니다.

| 오브젝트 | 중앙 기준 Position | Size |
|---|---|---|
| Ship Stats | (0,105) | (680,100) |
| Repair | (0,12) | (560,48) |
| Upgrade Damage | (0,-60) | (560,64) |
| Upgrade Reload | (0,-136) | (560,64) |
| Set Sail | (0,-218) | (560,52) |
| Footer | (0,-275) | (650,22) |

Upgrade Damage/Upgrade Reload의 기존 Label은 각각 Position=(0,10), Size=(544,28), Font Size=18.
각 버튼에 Preview 자식을 TextMeshProUGUI로 추가합니다.
Preview의 Position=(0,-17), Size=(544,22), Font Size=15, Alignment=Center, Color=White, Raycast Target=Off.
이름은 Damage Preview와 Reload Preview로 정합니다.

UpgradePreview.Flow=Game Systems의 GameFlowController.
Damage Preview/Reload Preview=각 자식의 TextMeshProUGUI, Highlight Seconds=0.18.
기존 DockPanel은 비용과 버튼 활성 상태를 계속 관리합니다. 두 스크립트가 같은 Text를 쓰지 않도록 새 Preview만 연결합니다.

## 5. 완료 확인
- 결과 보상 숫자가 0→실제 획득량으로 증가합니다.
- 카운트업 중 정비로 이동해도 실제 골드는 이미 한 번만 지급되어 있습니다.
- 다음 출항과 결과 화면에서도 중복 지급되지 않습니다.
- 공격력 강화 전 20→25, 구매 후 25→30이 표시됩니다.
- 재장전 강화 전 0.80s→0.72s처럼 표시됩니다.
- 자금 부족·최대 레벨에서는 기존 버튼 제한이 유지됩니다.
- 구매 성공 때만 해당 Preview가 짧게 강조됩니다.

## 비교와 복구
Duration=0.2/0.5/0.9초를 비교합니다.
효과를 끌 때는 RewardPresentation/UpgradePreview와 새 텍스트를 함께 비활성화합니다.
Show Reward In Summary를 다시 켜면 기존 결과창의 보상 줄을 복구할 수 있습니다.
