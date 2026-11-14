using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class CannonFireEffect : MonoBehaviour
    {
        // 실습 TODO: ShotFired 위치와 방향에 포구 화염과 연기를 생성한다. ParticleSystem 완료 후 제거하고 필요하면 풀링으로 확장한다.
        // 구현은 Docs/03_JuiceTutorial.md를 따라 직접 추가합니다.
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
            Instantiate(muzzlePrefab, shot.Position, Quaternion.Euler(0, 0, angle), effectsRoot);
        }
        void OnDisable() { if (events) events.ShotFired -= OnShot; }
    }
}
