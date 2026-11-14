using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class ImpactEffect : MonoBehaviour
    {
        // 실습 TODO: Hit에는 폭발, WaterMiss에는 물보라를 생성한다. 월드 위치에 생성하고 수명 완료 후 정리한다.
        // 구현은 Docs/03_JuiceTutorial.md를 따라 직접 추가합니다.
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
