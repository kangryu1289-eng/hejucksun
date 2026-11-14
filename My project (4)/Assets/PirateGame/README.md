# Pirate Game — 2D 해상 전투와 Juice 실습

Unity 6000.0.57f1에서 씬 `Assets/PirateGame/Scenes/PirateBattle.unity`를 열고 Play.
상단 메뉴 **Pirate Game > Open Game Scene**으로도 열 수 있습니다.
W/S 전진·후진, A/D 선회, 마우스 조준, 좌클릭(누르기 유지 가능) 발사.
거리 0.7~8m 내에서 발사할 수 있으며 재장전 중에는 발사하지 않습니다.

## 읽는 순서
1. [클래스와 상태 설계](Docs/01_Architecture.md)
2. [기본 씬·오브젝트·컴포넌트·설정값 해설](Docs/02_BaseGameTutorial.md)
3. [Juice 실습 안내](Docs/03_JuiceTutorial.md)
4. [검증과 문제 해결](Docs/04_Verification.md)

## 제공 범위
- 정비/업그레이드 → 라운드 선택 → 전투 → 결과 → 정비 반복.
- 라운드 3개, 적 AI, 선박/섬 충돌, 고속 포탄 구간 충돌 검사.
- 기본 씬, 선박/포탄 프리팹, 라운드 ScriptableObject, 에셋 복사본.
- Juice 9개는 주석만 있는 빈 클래스. 기능은 실습 문서에서 직접 작성하고 연결합니다.
- 실습용 합성 WAV 4개(Shot/Hit/Splash/Sink). 기본 게임에서는 재생하지 않습니다.
- 에디터 메뉴 `Pirate Game > Run Gameplay Checks`로 회귀 검증 실행 가능.
  이 명령은 게임 씬을 열고 Play에 진입한 뒤 자동으로 정지합니다. 기존 작업은 저장하세요.

## 주의할 동작
모든 라운드는 처음부터 선택할 수 있고 반복 도전할 수 있습니다. 진행도 잠금은 없습니다.
실행 종료 시 자금·업그레이드·내구도가 초기화됩니다.
사망한 배는 2초 후 사라집니다. 기본 버전에는 흔들림·플래시·효과음·Hit Stop이 없습니다.
UI는 TextMeshProUGUI와 LiberationSans SDF를 사용하며, 코드의 텍스트 참조는 TMP_Text입니다.
UI는 영어, 문서는 한국어입니다.
원본 SampleScene과 Kenney 이미지의 임포트 설정을 변경하지 않습니다.

씬 파일이 없는 경우에만 에디터 생성기가 최초 컴파일 후 자동으로 생성합니다.
자동 생성되지 않으면 `Pirate Game > Create Missing Game Assets`를 실행하세요.
기존 씬이 있으면 생성기는 해당 씬을 덮어쓰지 않습니다.
씬은 런타임 생성기가 아니라 미리 직렬화된 오브젝트/참조를 사용합니다.
