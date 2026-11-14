# 2-03 — 포탄 궤적과 그림자

## 시작 조건
다른 2차 단원이 필요 없습니다. Cannonball의 이동·충돌 코드는 바꾸지 않습니다.
먼저 [공통 준비](00_Setup.md)를 완료합니다. 이 문서의 코드는 해당 파일 전체를 교체하는 완성 예제입니다.
프로젝트의 새 클래스는 TODO만 있는 상태입니다. 코드를 입력·컴파일한 다음 Inspector를 연결하세요.

## 목표
포탄 방향을 짧은 궤적으로 읽고 그림자로 위치를 강조합니다.
그림자는 표현이며 새로운 탄도나 충돌 판정을 뜻하지 않습니다.

## 1. 코드
Scripts/Feel/ProjectilePresentation.cs를 교체합니다.

```csharp
using UnityEngine;
namespace PirateGame.Feel
{
    public sealed class ProjectilePresentation : MonoBehaviour
    {
        public SpriteRenderer projectile, shadow;
        public TrailRenderer trail;
        public Vector2 shadowOffset = new Vector2(0.08f,-0.08f);
        void Awake()
        {
            shadow.sprite = projectile.sprite;
            shadow.color = new Color(0,0,0,0.25f);
            shadow.sortingLayerID = projectile.sortingLayerID;
            shadow.sortingOrder = projectile.sortingOrder - 1;
            shadow.transform.localPosition = shadowOffset;
            trail.sharedMaterial = projectile.sharedMaterial;
            trail.time = 0.12f; trail.startWidth = 0.05f; trail.endWidth = 0;
            trail.startColor = new Color(1,1,1,0.6f); trail.endColor = new Color(1,1,1,0);
            trail.sortingLayerID = projectile.sortingLayerID;
            trail.sortingOrder = projectile.sortingOrder - 2;
            trail.minVertexDistance = 0.03f;
        }
        void OnEnable() { trail.Clear(); trail.emitting = true; }
        void OnDisable() { if (trail) { trail.emitting = false; trail.Clear(); } }
    }
}
```

## 2. Hierarchy와 컴포넌트
Cannonball_Feel 프리팹을 엽니다.
- 루트: 기존 SpriteRenderer와 Cannonball을 유지하고 ProjectilePresentation 추가.
- Shadow 자식: Local Scale=(1,1,1), SpriteRenderer 추가.
- Trail 자식: Local Position=(0,0,0), Scale=(1,1,1), TrailRenderer 추가.

Shadow Material=SpriteUnlit, Trail Auto Destruct=Off.
Shadow에는 Collider나 Rigidbody2D를 붙이지 않습니다.
ProjectilePresentation의 Projectile=루트 SpriteRenderer, Shadow=자식 SpriteRenderer, Trail=자식 TrailRenderer.
Shadow Offset=(0.08,-0.08). 나머지 궤적 수치는 Awake에서 코드로 적용합니다.
양쪽 선박 복사본의 CannonWeapon.Projectile Prefab이 Cannonball_Feel인지 확인합니다.

## 3. 수명 정책
이 예제는 충돌 시 포탄과 자식 궤적을 함께 삭제합니다.
입문 단계에서 파괴된 포탄의 Trail을 별도 객체로 떼어 관리하지 않습니다.
충돌 후 궤적을 남기는 기능은 독립적인 수명/정리 정책이 필요한 선택 확장입니다.

## 4. 완료 확인
- 직선 이동 중 짧은 잔상이 보입니다.
- 그림자는 포탄과 같은 방향으로 이동합니다.
- 포탄은 적과 섬에 기존 위치에서 충돌합니다.
- 사거리와 데미지가 변하지 않습니다.
- 발사 반복 후 이전 궤적이 새 포탄 시작점으로 이어지지 않습니다.

## 문제와 비교
분홍색이면 Material/Shader를 확인합니다.
포탄보다 그림자가 앞에 나오면 Sorting Layer와 Order를 확인합니다.
Trail.time=0.06/0.12/0.25, Start Width=0.03/0.05/0.08을 비교하세요.
효과를 빼려면 ProjectilePresentation과 두 시각 자식을 비활성화합니다.
