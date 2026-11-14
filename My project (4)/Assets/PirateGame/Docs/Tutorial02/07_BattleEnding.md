# 2-07 — 격침 마무리 후 결과창

## 시작 조건
다른 2차 단원이 필요 없습니다. 1차 ShipSinkingEffect가 있으면 0.65초 격침 연출과 함께 비교합니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 새 클래스의 예제는 해당 파일 전체를 교체합니다.

## 목표와 상태
Dock → RoundSelect → Battle → BattleEnding → Result → Dock.
BattleController가 확정한 결과를 보관하고 표시 연출 후 한 번만 정산합니다.
Director는 골드·내구도·승패를 직접 바꾸지 않습니다.
이 단원의 기본 예제에는 슬로모션을 넣지 않습니다.

## 1. 종료 연출 클래스
Scripts/Feel/BattleEndingDirector.cs를 교체합니다.

```csharp
using System;
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class BattleEndingDirector : MonoBehaviour
    {
        public float duration = 0.8f;
        float remaining;
        Action completed;
        public void Play(Action callback) { Cancel(); completed = callback; remaining = Mathf.Max(0,duration); }
        void Update()
        {
            if (completed == null) return;
            remaining -= Time.unscaledDeltaTime;
            if (remaining > 0) return;
            Action callback = completed; completed = null; callback();
        }
        public void Cancel() { completed = null; remaining = 0; }
        void OnDisable() => Cancel();
    }
}
```

정지 중에도 완료 타이머가 지나가도록 Time.unscaledDeltaTime을 사용합니다.
취소는 콜백을 제거합니다. Director를 중간에 끄는 경우 Flow가 다음 프레임에 결과를 완료합니다.

## 2. 게임 진행 코드 확장
Scripts/Core/GameFlowController.cs 전체를 교체합니다.

```csharp
using UnityEngine;
using PirateGame.Feel;
namespace PirateGame
{
    public enum GamePhase { Dock, RoundSelect, Battle, BattleEnding, Result }
    public sealed class GameFlowController : MonoBehaviour
    {
        public BattleController battle;
        public BattleEndingDirector endingDirector;
        bool settled;
        public GameUI ui;
        public RoundDefinition[] rounds;
        public PlayerProgress Progress { get; private set; }
        public DockService Dock { get; private set; }
        public GamePhase Phase { get; private set; }
        public BattleResult LastResult { get; private set; }
        void Awake() { Progress = new PlayerProgress(); Dock = new DockService(Progress); }
        void Start() { battle.BattleEnded += OnBattleEnded; ui.Initialize(this); ReturnToDock(); }
        public void ReturnToDock() { if (endingDirector) endingDirector.Cancel(); Time.timeScale = 1; battle.Clear(); Phase = GamePhase.Dock; ui.Show(Phase); }
        public void SelectRound() { if (Phase != GamePhase.Dock) return; Phase = GamePhase.RoundSelect; ui.Show(Phase); }
        public void StartRound(int index)
        {
            if (Phase != GamePhase.RoundSelect || index < 0 || index >= rounds.Length || Progress.Hull <= 0) return;
            settled = false; Time.timeScale = 1; Phase = GamePhase.Battle; battle.Begin(rounds[index], Progress); ui.Show(Phase);
        }
        void OnBattleEnded(BattleResult result)
        {
            if (Phase != GamePhase.Battle) return;
            Time.timeScale = 1; LastResult = result;
            Phase = GamePhase.BattleEnding; ui.Show(Phase);
            if (endingDirector && endingDirector.isActiveAndEnabled) endingDirector.Play(CompleteBattle);
            else CompleteBattle();
        }
        void Update()
        {
            if (Phase == GamePhase.BattleEnding && (!endingDirector || !endingDirector.isActiveAndEnabled)) CompleteBattle();
        }
        void CompleteBattle()
        {
            if (settled || Phase != GamePhase.BattleEnding) return;
            settled = true; Time.timeScale = 1;
            Progress.AddGold(LastResult.Reward);
            Progress.SetHull(LastResult.Victory ? LastResult.Hull : PlayerProgress.MaxHull * 0.5f);
            Phase = GamePhase.Result; ui.Show(Phase);
        }
        public void Repair() { if (Phase == GamePhase.Dock) { Dock.Repair(); ui.Refresh(); } }
        public void UpgradeDamage() { if (Phase == GamePhase.Dock) { Dock.UpgradeDamage(); ui.Refresh(); } }
        public void UpgradeReload() { if (Phase == GamePhase.Dock) { Dock.UpgradeReload(); ui.Refresh(); } }
        void OnDestroy() { if (endingDirector) endingDirector.Cancel(); if (battle) battle.BattleEnded -= OnBattleEnded; Time.timeScale = 1; }
    }
}

```

OnBattleEnded는 Phase가 Battle일 때만 결과를 받습니다.
CompleteBattle은 settled와 Phase 조건으로 중복 정산을 차단합니다.
Ending Director가 없거나 비활성화된 기본 씬은 기존처럼 즉시 결과창으로 진행합니다.
ReturnToDock/OnDestroy는 남은 콜백을 취소합니다.

## 3. 씬 연결
1. PirateBattle_Feel 씬의 Juice Lab 오브젝트에 BattleEndingDirector를 추가합니다.
2. Duration=0.8초.
3. Game Systems/GameFlowController.Ending Director에 연결합니다.
4. 기본 PirateBattle 씬에는 연결하지 않습니다.
5. GameUI는 BattleEnding 상태에서 기존 4개 화면을 모두 숨깁니다. 새 UI 패널은 만들지 않습니다.

기존 BattleController.Finish가 입력·AI·조준·포탄을 정지시키므로 이 기본 예제에서는 해당 메서드를 교체하지 않습니다.
내구도는 전투 종료 후 추가 포탄 판정을 받지 않으며 결과 스냅샷으로 정산합니다.
섬 접촉에는 별도 충돌 데미지가 없다는 현재 규칙을 전제로 합니다.

## 4. 남은 표시 정리
BattleController.Finish의 기존 조준 비활성화 줄 바로 뒤에 추가합니다.

```csharp
foreach (var line in actorsRoot.GetComponentsInChildren<LineRenderer>()) line.enabled = false;
```

이는 기본 직선 안내도 종료 시 숨기기 위한 처리입니다.
04 ReloadIndicator와 1차 AimArcIndicator는 CannonAim.enabled를 확인하여 다시 켜지 않습니다.
05 Controller는 battle.Running=false를 보고 예고를 취소합니다.

## 5. 1차 카메라 흔들림을 종료 연출 동안 유지(선택)
1차 CameraShake.LateUpdate의 다음 줄을 변경합니다.

```csharp
// 기존: if (flow.Phase != GamePhase.Battle) remaining = 0;
if (flow.Phase != GamePhase.Battle && flow.Phase != GamePhase.BattleEnding) remaining = 0;
```

HitStop은 기존처럼 Battle 밖에서 복구하도록 유지합니다.
따라서 마지막 격침의 순간 정지는 종료 상태 진입 시 풀리고, 카메라·입자·침몰·사운드는 마무리됩니다.
원치 않는 시간 배율 경쟁을 피하기 위해 별도의 슬로모션 컴포넌트를 추가하지 않습니다.

## 6. 완료 확인
- 마지막 적 격침 후 조작이 멈추고 약 0.8초 뒤 결과창이 나옵니다.
- 그동안 기존 격침·폭발·사운드가 마무리됩니다.
- 전투 시간은 마지막 격침 시점의 스냅샷이며 연출 대기시간이 더해지지 않습니다.
- 결과창의 보상은 한 번만 지급됩니다.
- Director를 연출 중 끄면 다음 프레임에 결과창이 나옵니다.
- 패배도 같은 순서로 진행하고 정비 후 다시 출항할 수 있습니다.
- 기본 씬은 Ending Director=None이므로 기존 즉시 전환을 유지합니다.

## 비교와 복구
Duration=0.3/0.8/1.2를 비교합니다. 길수록 좋다고 가정하지 마세요.
Director를 비활성화하면 즉시 전환으로 비교할 수 있습니다.
공용 코드가 확장되었으므로 기본 씬에서도 기존 게임 루프 검사를 실행합니다.
