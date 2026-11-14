# 2차 튜토리얼 공통 준비

## 제공 상태와 실습 단위
2차 튜토리얼은 8개 개별 문서로 나눴습니다.
각 문서는 목표, 코드, Unity 오브젝트·컴포넌트, Inspector 값, 적용 확인, 비교와 복구를 포함합니다.
11개 새 스크립트는 Scripts/Feel에 빈 클래스와 TODO로 제공했습니다.
기능 구현은 문서의 코드를 직접 입력해 완성합니다. 2차 씬과 효과를 자동 적용하지 않았습니다.
관성 이동·적 포격·게임 진행은 기존 공용 클래스의 확장도 필요합니다. 해당 문서에 전체 교체 코드를 제공합니다.

## 1. 실습용 에셋 분리
Stop 상태에서 수행합니다.

1. Assets/PirateGame 아래 FeelLab/Scenes, FeelLab/Prefabs, FeelLab/Data 폴더를 만듭니다.
2. PirateBattle.unity를 복제하고 FeelLab/Scenes/PirateBattle_Feel.unity로 이동합니다.
3. PlayerShip, EnemyShip, Cannonball 프리팹을 복제해 FeelLab/Prefabs로 이동합니다.
4. 이름을 PlayerShip_Feel, EnemyShip_Feel, Cannonball_Feel로 정합니다.
5. 두 선박 복사본의 CannonWeapon.Projectile Prefab에 Cannonball_Feel을 연결합니다.
6. 새 씬 Game Systems/BattleController의 Player Prefab과 Enemy Prefab에 새 선박을 연결합니다.
7. 라운드 값을 실험하려면 Round1~3 에셋도 복제하고 새 씬 Flow의 Rounds에 연결합니다.
8. Play하여 정비 → 선택 → 전투 → 결과 → 정비를 한 번 반복합니다.

씬 복사만으로 프리팹 참조가 분리되지는 않습니다.
이후 설명의 PlayerShip_Feel/EnemyShip_Feel은 모두 복사본을 뜻합니다.

## 2. 개별 실습의 의존성
| 문서 | 다른 2차 단원 필요 여부 |
|---|---|
| 01 이동 | 없음 |
| 02 항적 | 없음. 기존 모터의 실제 속도로도 작동 |
| 03 포탄 | 없음 |
| 04 재장전 | 없음. 1차 곡선 인디케이터는 선택 |
| 05 적 포격 | 없음 |
| 06 피해 상태 | 없음. 1차 효과의 강도 연결은 선택 |
| 07 종료 연출 | 없음. 1차 격침 연출이 있으면 함께 확인 |
| 08 보상·강화 | 없음. 기존 결과창에서도 작동 |

01,05,07은 공용 코드를 바꾸지만 설정·컴포넌트가 없으면 기존 동작을 유지합니다.
06에서 1차 CameraShake/HitStop을 수정하려면 그 1차 실습을 먼저 완료해야 합니다.
선택 슬로모션은 이번 개별 구현 예제에 포함하지 않습니다.

## 3. 공통 UI·렌더링 설정
- UI 컴포넌트: TextMeshProUGUI. 코드 필드: TMP_Text.
- Font Asset: Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.
- World LineRenderer/TrailRenderer: Materials/SpriteUnlit, Sorting Layer Default.
- LineRenderer는 Use World Space=On.
- ParticleSystem은 조명이 없어도 보이는 SpriteUnlit 재질을 사용합니다.
- 선박 앞 방향은 루트 +Y, 선미는 로컬 -Y입니다.
- 모든 값은 시작값입니다. 한 번에 하나씩 바꾸어 비교하세요.

## 4. 파일을 교체하는 방법
1. Stop 상태에서 문서에 나온 파일을 엽니다.
2. 클래스 안에 예제 클래스를 중첩하지 말고 파일 전체 내용을 교체합니다.
3. 동일한 클래스가 다른 파일에 중복되지 않도록 합니다.
4. Unity 컴파일 완료 후 Add Component와 참조 연결을 수행합니다.
5. Inspector 필드를 모두 연결한 뒤 프리팹과 씬을 저장합니다.
6. 실습마다 한 라운드와 재출항을 확인합니다.

Scripts/Feel의 빈 클래스만 부착하면 효과가 없는 것이 정상입니다.
공용 코드 교체 전에 원본 파일을 별도로 보관하거나 버전 관리에 기록하세요.

## 5. 단원별 비교
각 문서의 완료 기준을 확인하고 컴포넌트/설정을 끈 상태와 비교합니다.
최종 통합 확인은 [완료 체크리스트](09_Checklist.md)를 사용합니다.
