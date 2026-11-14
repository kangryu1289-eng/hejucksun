using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class CannonRecoil : MonoBehaviour
    {
        // 실습 TODO: ShotFired 이벤트를 구독하여 Visual 자식만 발사 반대 방향으로 밀고 원래 localPosition으로 복귀시킨다. 연속 발사 시 누적 오프셋을 방지한다.
        // 구현은 Docs/03_JuiceTutorial.md를 따라 직접 추가합니다.
        public Transform visual;
        public float distance = 0.12f, recovery = 0.14f;
        CombatFeedbackEvents events;
        Vector3 rest;
        void Awake() => rest = visual.localPosition;
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events) events.ShotFired += OnShot;
        }
        void OnShot(ShotFeedback shot)
        {
            if (shot.Shooter != transform) return;
            Vector3 local = transform.InverseTransformDirection((Vector3)shot.Direction);
            visual.localPosition = rest - local * distance;
        }
        void Update() => visual.localPosition = Vector3.MoveTowards(visual.localPosition, rest,
            distance / Mathf.Max(0.01f, recovery) * Time.deltaTime);
        void OnDisable()
        {
            if (events) events.ShotFired -= OnShot;
            if (visual) visual.localPosition = rest;
        }
    }
}
