using UnityEngine;
namespace PirateGame
{
    public sealed class ShipHealth : MonoBehaviour
    {
        public bool isPlayer;
        public float MaxHull { get; private set; }
        public float Hull { get; private set; }
        public bool IsAlive => Hull > 0f;
        CombatFeedbackEvents feedback;
        public void Initialize(bool player, float maximum, float current, CombatFeedbackEvents events)
        { isPlayer = player; MaxHull = maximum; Hull = Mathf.Clamp(current, 0f, maximum); feedback = events; }
        public void TakeDamage(float amount, Vector2 impact)
        {
            if (!IsAlive || amount <= 0f) return;
            float actual = Mathf.Min(Hull, amount);
            Hull -= actual;
            feedback.PublishHit(new HitFeedback(this, impact, actual));
            if (IsAlive) return;
            foreach (Collider2D collider in GetComponents<Collider2D>()) collider.enabled = false;
            var body = GetComponent<Rigidbody2D>();
            if (body) { body.linearVelocity = Vector2.zero; body.angularVelocity = 0; body.simulated = false; }
            feedback.PublishSunk(new SinkFeedback(this));
            // Keep the visual alive briefly: the sinking tutorial animates this object.
            Destroy(gameObject, 2f);
        }
    }
}
