using UnityEngine;
namespace PirateGame
{
    public sealed class Cannonball : MonoBehaviour
    {
        public float hitRadius = 0.09f;
        bool player; Vector2 direction; float damage, speed, remaining;
        CombatFeedbackEvents feedback;
        readonly RaycastHit2D[] hits = new RaycastHit2D[24];
        public void Initialize(bool fromPlayer, Vector2 forward, float power, float velocity, float distance, CombatFeedbackEvents events)
        { player = fromPlayer; direction = forward.normalized; damage = power; speed = velocity; remaining = distance; feedback = events; }
        void Update()
        {
            float step = Mathf.Min(speed * Time.deltaTime, remaining);
            if (step <= 0) return;
            var filter = new ContactFilter2D(); filter.NoFilter(); filter.useTriggers = false;
            int count = Physics2D.CircleCast(transform.position, hitRadius, direction, filter, hits, step);
            // Pick the closest valid collision; friendly hulls never block their own side.
            RaycastHit2D nearest = default; float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var hull = hits[i].collider.GetComponentInParent<ShipHealth>();
                if (hull && (!hull.IsAlive || hull.isPlayer == player)) continue;
                if (hits[i].distance < best) { best = hits[i].distance; nearest = hits[i]; }
            }
            if (nearest.collider)
            {
                var hull = nearest.collider.GetComponentInParent<ShipHealth>();
                if (hull) hull.TakeDamage(damage, nearest.point); else feedback.PublishWaterMiss(nearest.point);
                Destroy(gameObject); return;
            }
            transform.position += (Vector3)(direction * step);
            remaining -= step;
            if (remaining <= 0.001f) { feedback.PublishWaterMiss(transform.position); Destroy(gameObject); }
        }
    }
}
