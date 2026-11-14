# 기본 게임 씬 해설

## 1. 시작과 학습 범위
기본 씬은 제공되어 있습니다. 먼저 완성된 루프를 플레이하고, 아래 구성과 Inspector를 대조합니다.
Juice 구현은 다음 문서에서 수행합니다. 기본 씬 해설을 따라 다시 조립할 수도 있습니다.
Unity 6000.0.57f1 / 2D URP / Input System 1.14.2 / uGUI 2.0.0 기준입니다.

1. Project에서 Assets/PirateGame/Scenes/PirateBattle.unity를 엽니다.
2. Game 뷰 비율을 16:9, 권장 1280×720으로 설정합니다.
3. Play를 누릅니다. HARBOR 화면의 초기 값은 Gold 100, Hull 100, Cannon 20, Reload 0.80s입니다.
4. CHOOSE ROUND → Coastal Patrol → 적 두 척 격침 → VICTORY → RETURN TO HARBOR.
5. 다시 출항해 같은 루프가 반복되는지 확인합니다.

기본 버전이 무음이고 명중 시 화면이 흔들리지 않는 것은 정상입니다.
UI 문자열은 영어이며 문서는 한국어입니다. 플레이 중 Inspector 변경은 종료 시 사라집니다.
영구 설정 변경은 Stop 상태에서 프리팹과 Round 에셋을 수정하세요.

## 2. 폴더 구조
| 경로 | 내용 |
|---|---|
| Scripts/Core | 상태 전환, 경제, 라운드, 이벤트, 전투 진행 |
| Scripts/Ships | 내구도, 이동, 플레이어 입력, 적 AI |
| Scripts/Combat | 조준, 무기, 포탄 |
| Scripts/UI | 화면별 버튼과 표시 |
| Scripts/Juice | 실습용 빈 클래스 9개 |
| Scenes/PirateBattle.unity | 기본 게임 씬 |
| Prefabs | PlayerShip, EnemyShip, Cannonball |
| Data | Round1~Round3.asset |
| Art | Kenney 사용 이미지 복사본, 절차 생성 Ocean/White |
| Materials | SpriteUnlit.mat |
| Audio | 실습용 합성 사운드 4개 |
| Editor | 씬 생성과 개발 검증 도구 |
| Docs/Lessons | Juice 기능별 코드와 Unity 연결 절차 |

## 3. 전체 Hierarchy
```text
Main Camera
World
  Ocean
  StaticWaterLine_0 ... _17
  Arena                         [정비 화면에서는 비활성]
    Island
      Shore_0 ... _3
    Island
      Shore_0 ... _3
    ArenaEdge ×4
  Actors                        [전투 때 프리팹 인스턴스 생성]
  Projectiles                   [발사 때 포탄 인스턴스 생성]
  Effects                       [후속 실습의 효과 컨테이너]
Game Systems
  (CombatFeedbackEvents / BattleController / GameFlowController)
Juice Lab - add tutorial components here
EventSystem
Game Canvas
  Dock Panel
  Round Select Panel
  Battle HUD
    Top Readout
    Aim Readout
  Result Panel
```

Main Camera / Game Systems / Game Canvas / EventSystem은 각각 하나만 둡니다.
SampleScene을 Additive로 열어 카메라와 EventSystem을 중복시키지 마세요.

## 4. 카메라와 배경
| 오브젝트/컴포넌트 | 필드 | 값 | 의미 |
|---|---|---|---|
| Main Camera / Transform | Position | (0,0,-10) | XY 평면을 앞에서 봄 |
| Camera | Projection | Orthographic | 원근 크기 변화 없음 |
| Camera | Size | 7.8 | 세로 15.6 월드 단위 |
| Camera | Clear Flags | Solid Color | 해상 배경색 |
| AudioListener | Enabled | On | 사운드 실습의 청취점 |
| Ocean / SpriteRenderer | Sprite | Art/Ocean | 반복 가능한 생성 텍스처 |
| SpriteRenderer | Draw Mode / Size | Tiled / (32,20) | 화면 밖까지 바다 채움 |
| SpriteRenderer | Sorting Order | -20 | 가장 뒤 |
| Material | Shader | Sprites/Default | 조명 없이 표시 |
| Shore / SpriteRenderer | Sprite | ShoreCorner | Kenney tile_01 복사본 |
| Island / CircleCollider2D | Radius | 1.1 | 배와 포탄을 막는 섬 |
| Island / Transform | Position | (-10.6,4.8), (10.6,-4.8) | 모서리 장애물 |

씬은 Unlit 스프라이트를 쓰므로 Global Light 2D가 없어도 표시됩니다.
기본 전장 경계는 X ±12, Y ±6.5입니다. 모터는 선박 중심을 경계에서 0.6m 안쪽으로 제한합니다.
ArenaEdge는 표시용이며 충돌체가 아닙니다.
이 씬은 16:9 학습용입니다. 세로 화면 대응은 이번 범위에 포함하지 않습니다.

## 5. 플레이어 프리팹
Project에서 Prefabs/PlayerShip을 더블클릭하여 Prefab Mode에서 확인하세요.
```text
PlayerShip
  Visual              [SpriteRenderer]
  BasicAimGuide       [LineRenderer]
```

### 루트 오브젝트
| 컴포넌트 | 설정 | 값 |
|---|---|---|
| Transform | Local Scale | (1,1,1) |
| Rigidbody2D | Body Type | Dynamic |
| Rigidbody2D | Mass / Gravity Scale | 2 / 0 |
| Rigidbody2D | Linear / Angular Damping | 2 / 2 |
| Rigidbody2D | Interpolate | Interpolate |
| Rigidbody2D | Collision Detection | Continuous |
| CapsuleCollider2D | Direction / Size / Offset | Vertical / (0.8,1.55) / (0,0) |
| ShipHealth | Is Player | On |
| ShipMotor | Forward / Reverse Speed | 3.5 / 1.75 m/s |
| ShipMotor | Turn Speed | 110 deg/s |
| ShipMotor | Arena Half Size | (12,6.5) |
| CannonAim | Range | 8m |
| CannonAim | Guide | BasicAimGuide의 LineRenderer |
| CannonAim | World Camera | 프리팹에서는 None. 출항 시 BattleController가 주입 |
| CannonWeapon | Projectile Prefab | Prefabs/Cannonball |
| CannonWeapon | Damage / Reload Seconds | 20 / 0.8. 출항 시 강화값으로 덮어씀 |
| CannonWeapon | Range / Projectile Speed | 8 / 10 |
| CannonWeapon | Projectile Root / Feedback | 프리팹에서는 None. BattleController가 주입 |
| PlayerShipInput | 추가 설정 | 없음. 같은 오브젝트의 컴포넌트를 Awake에서 가져옴 |

W/S는 전진·후진, A/D는 좌우 선회입니다. 좌클릭을 누르고 있으면 재장전마다 발사합니다.
UI 위에서는 발사를 차단하며 이동은 계속됩니다.
배의 전방은 루트의 +Y입니다. 이미지가 아래를 향하므로 Visual만 Z=180도 회전했습니다.
초기 생성 시 루트 Z=-90도로 오른쪽을 향합니다.

### Visual
| 항목 | 값 |
|---|---|
| Local Position | (0,0,0) |
| Local Rotation | (0,0,180) |
| Local Scale | (1.5,1.5,1.5) |
| Sprite | Art/PlayerShip (Kenney ship (5)) |
| Color | White |
| Material / Sorting Order | SpriteUnlit / 2 |

반동과 격침 연출은 Visual에 적용합니다. Rigidbody2D나 Collider가 있는 루트를 흔들지 않습니다.

### BasicAimGuide
| LineRenderer 항목 | 값 |
|---|---|
| Position Count | 2 |
| Use World Space | On |
| Width | 0.025 |
| Material | Materials/SpriteUnlit |
| Sorting Order | 1 |

CannonAim은 매 프레임 배에서 커서 방향으로 선을 갱신합니다.
선은 최대 8m에서 멈추고, 커서가 8m를 넘으면 빨간색으로 표시됩니다.
커서까지 실제 거리는 하단 HUD에 표시합니다. 0.7m 미만도 발사할 수 없습니다.
Juice 실습 1에서 이 선을 숨기고 곡선과 착탄 원을 추가합니다.

## 6. 적 선박 프리팹
EnemyShip의 물리 구성과 Visual 회전은 PlayerShip과 같습니다.
| 플레이어와 다른 부분 | 값 |
|---|---|
| Sprite | Art/EnemyShip (Kenney ship (13)) |
| ShipHealth.IsPlayer | Off |
| ShipMotor.ForwardSpeed | 1.7 |
| ShipMotor.TurnSpeed | 65 |
| EnemyShipAI | 추가 |
| EnemyShipAI.PreferredDistance | 4.2m |
| PlayerShipInput / CannonAim / BasicAimGuide | 없음 |
| EnemyShipAI.Target | 출항 시 플레이어 ShipHealth 주입 |
| CannonWeapon.ReloadSeconds | 출항 시 1.8 + 개체 번호×0.15초 |

적은 플레이어를 향해 선회·접근하고, 거리 4.2m 이내에서는 전진을 멈춥니다.
대포는 배의 방향과 관계없이 8m 이내의 플레이어를 직접 조준합니다.
장애물 경로 탐색과 아군 회피는 구현 범위에 포함하지 않습니다.
섬에 걸린 적도 격침하면 승리할 수 있습니다.

## 7. 포탄 프리팹과 판정
| 컴포넌트/필드 | 값 |
|---|---|
| Transform.LocalScale | (1.4,1.4,1.4) |
| SpriteRenderer.Sprite | Art/Cannonball |
| SpriteRenderer.SortingOrder | 10 |
| Cannonball.HitRadius | 0.09m |
| Rigidbody2D / Collider2D | 없음 |

포탄은 Update에서 이동할 구간을 CircleCast로 검사합니다.
프레임 사이로 적을 통과하는 누락을 줄이고, 가장 가까운 유효 충돌만 처리합니다.
아군 선박과 사망한 선박은 무시합니다. 섬은 포탄을 막습니다.
최대 사거리까지 직선으로 이동합니다. 커서는 방향·발사 허용 여부를 정하고 탄의 조기 소멸 지점을 정하지 않습니다.
적 충돌은 TakeDamage → Hit → 사망 시 Sunk 순서입니다.
섬 충돌과 사거리 소진은 WaterMiss 이벤트를 발생시킵니다.
사망한 선박은 충돌을 끄고 2초 후 삭제합니다. 승패는 삭제가 아닌 내구도를 기준으로 판정합니다.

## 8. Game Systems 참조 연결
| 컴포넌트 | 필드 | 드래그할 대상 |
|---|---|---|
| GameFlowController | Battle | 같은 오브젝트의 BattleController |
| GameFlowController | UI | Game Canvas의 GameUI |
| GameFlowController | Rounds Size | 3 |
| GameFlowController | Rounds 0/1/2 | Data/Round1, Round2, Round3 |
| BattleController | Player / Enemy Prefab | Prefabs/PlayerShip, EnemyShip |
| BattleController | Actors Root | World/Actors |
| BattleController | Projectile Root | World/Projectiles |
| BattleController | Arena Root | World/Arena |
| BattleController | World Camera | Main Camera |
| BattleController | Feedback | 같은 오브젝트의 CombatFeedbackEvents |

RoundDefinition은 Create > Pirate Game > Round로 만들 수 있습니다.
| 데이터 | Round1 | Round2 | Round3 |
|---|---:|---:|---:|
| Title | Coastal Patrol | Smuggler Waters | Pirate Armada |
| Enemy Count | 2 | 3 | 4 |
| Enemy Hull | 40 | 60 | 80 |
| Enemy Damage | 8 | 10 | 12 |
| Victory Gold | 80 | 120 | 180 |
| Arena Half Size | (12,6.5) | (12,6.5) | (12,6.5) |

이번 씬의 섬/경계선은 고정 배치입니다. Arena Half Size를 바꾸면 경계선과 카메라도 함께 수정하세요.
적 수는 1~8 범위를 전제로 합니다. 기본 데이터는 2~4입니다.

## 9. UI와 이벤트 시스템
Canvas는 Screen Space Overlay, CanvasScaler는 Scale With Screen Size입니다.
Reference Resolution=(1280,720), Match=0.5. GraphicRaycaster가 버튼 입력을 처리합니다.
EventSystem에는 InputSystemUIInputModule을 사용합니다. StandaloneInputModule을 추가하지 않습니다.
기본 UI 액션은 생성기가 AssignDefaultActions로 연결합니다.
텍스트 컴포넌트는 TextMeshProUGUI, 코드 참조 타입은 TMP_Text입니다.
Font Asset은 Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF를 사용합니다.
Alignment=Middle/Center, Text Wrapping=Normal, Overflow=Truncate, Raycast Target=Off입니다.
버튼과 Slider는 uGUI 컴포넌트를 유지합니다.
버튼의 On Click을 Inspector에서 중복 연결하지 마세요. 각 Panel.Bind가 코드로 연결합니다.

| 패널 | 크기 | 주요 자식 |
|---|---|---|
| Dock Panel | 740×570 | Title, Ship Stats, Repair, Upgrade Damage, Upgrade Reload, Set Sail |
| Round Select Panel | 740×520 | Round 1~3, Back |
| Battle HUD | 전체 Stretch | Top Readout, Aim Readout |
| Result Panel | 680×500 | Heading, Summary, Return to Dock |

기본 패널 배경색은 (0.035,0.10,0.16,0.96), 버튼 배경은 (0.14,0.32,0.39,1), 강조색은 (1,0.78,0.37,1)입니다.
정비 버튼 크기는 560×48, 간격은 60px, 출항 버튼은 560×52입니다.
라운드 버튼은 620×80, 간격 100px입니다. 기본 글자 크기는 20, 제목 32~40입니다.

### GameUI 연결
- Dock → Dock Panel의 DockPanel.
- Round Select → Round Select Panel의 RoundSelectPanel.
- Battle HUD → Battle HUD의 BattleHUD.
- Result → Result Panel의 ResultPanel.

### DockPanel 연결
- Stats → Ship Stats의 TextMeshProUGUI.
- Repair/Damage/Reload Label → 해당 버튼의 Label 자식 TextMeshProUGUI.
- Repair/Damage/Reload/Sail Button → 각각 Repair, Upgrade Damage, Upgrade Reload, Set Sail의 Button.
- 수리비는 ceil(100-현재 내구도). 전액 지불 가능할 때만 전체 수리합니다.
- 공격력 강화 비용은 50×(레벨+1), 재장전은 60×(레벨+1), 각 최대 5레벨입니다.
- 공격력은 20+5×레벨, 재장전 시간은 0.8×0.9^레벨입니다.

### RoundSelectPanel 연결
- Round Buttons / Round Labels 배열 크기3.
- 각 Round N의 Button과 자식 Label을 같은 순서로 연결.
- Back Button → Back.
- 순서가 Data/Round 배열 순서와 일치해야 합니다.

### BattleHUD 연결
- Status → Top Readout/Status TextMeshProUGUI.
- Hull Bar / Reload Bar → Top Readout 아래의 Slider.
- Aim Text → Aim Readout/Aim Text의 TextMeshProUGUI.
- Flow는 GameUI.Initialize에서 실행 중 주입.
- 상단 판독창은 600×78, Anchor=(0.5,1), Position=(0,-45).
- 하단은 760×70, Anchor=(0.5,0), Position=(0,43).
- 각 Slider 크기 230×12, Min=0, Max=1, Interactable=Off.
- Fill 자식은 전체 Stretch, Fill Rect에 자기 RectTransform 연결. Handle은 없음.
- Hull과 Reload 채움은 초록색과 금색입니다.

### ResultPanel 연결
- Heading → Heading TextMeshProUGUI.
- Summary → Summary TextMeshProUGUI.
- Dock Button → Return to Dock Button.
- 승리 시 해당 라운드 보상, 패배 시 20+격침수×10 골드.
- 승리 내구도는 그대로 유지, 패배 시 무료 50 HP 복구.
- 정산은 Result 진입 시 단 한 번 수행합니다.

## 10. 코드 읽기와 작은 확인 실습
1. GameFlowController.OnBattleEnded를 읽고 보상을 두 번 주지 않는 조건을 찾습니다.
2. Round1의 Victory Gold를 90으로 바꾸어 Play. 승리 보상이 90인지 확인 후 80으로 복구합니다.
3. PlayerShip 프리팹에서 ForwardSpeed를 2로 낮추어 차이를 확인하고 3.5로 복구합니다.
4. Damage 강화 한 번 후 CannonWeapon.damage가 25인지 Play 중 Inspector에서 확인합니다.
5. 내구도가 줄어든 상태로 승리 후 정비창 수리비가 ceil(100-남은 HP)인지 비교합니다.
6. 패배 후 50 HP로 다시 출항 가능한지 확인합니다.

문제가 있으면04_Verification.md의 항목을 확인하세요.

## TMP 텍스트 연결
DockPanel의 Stats/Repair Label/Damage Label/Reload Label, RoundSelectPanel의 Round Labels,
BattleHUD의 Status/Aim Text, ResultPanel의 Heading/Summary는 모두 TMP_Text 필드입니다.
해당 오브젝트의 TextMeshProUGUI 컴포넌트를 드래그해 연결합니다.
Legacy Text를 새로 추가하지 말고 UI > Text - TextMeshPro를 사용하세요.
씬 생성기 역시 TextMeshProUGUI와 TMP_Text 배열을 생성합니다.
TMP Essential Resources가 없는 프로젝트로 옮길 때는 Window > TextMeshPro > Import TMP Essential Resources를 먼저 실행하세요.
