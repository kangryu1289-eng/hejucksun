using UnityEngine;
namespace PirateGame
{
    [RequireComponent(typeof(ShipHealth))]
    public sealed class CannonWeapon : MonoBehaviour
    {
        public Cannonball projectilePrefab;
        public float damage = 20f;
        public float reloadSeconds = 0.8f;
        public float range = 8f;
        public float projectileSpeed = 10f;
        public Transform projectileRoot;
        public CombatFeedbackEvents feedback;
        ShipHealth health;
        float readyAt;
        public float Reload01 => Mathf.Clamp01(1 - (readyAt - Time.time) / Mathf.Max(0.05f, reloadSeconds));
        void Awake() => health = GetComponent<ShipHealth>();
        public bool TryFire(Vector2 target)
        {
            if (!health.IsAlive || Time.time < readyAt || !projectilePrefab || !projectileRoot || !feedback) return false;
            Vector2 offset = target - (Vector2)transform.position;
            if (offset.magnitude < 0.7f || offset.magnitude > range) return false;
            Vector2 direction = offset.normalized;
            Vector2 muzzle = (Vector2)transform.position + direction * 0.65f;
            Cannonball ball = Instantiate(projectilePrefab, muzzle, Quaternion.identity, projectileRoot);
            ball.Initialize(health.isPlayer, direction, damage, projectileSpeed, range - 0.65f, feedback);
            readyAt = Time.time + Mathf.Max(0.05f, reloadSeconds);
            feedback.PublishShot(new ShotFeedback(transform, muzzle, direction, health.isPlayer));
            return true;
        }
    }
}
