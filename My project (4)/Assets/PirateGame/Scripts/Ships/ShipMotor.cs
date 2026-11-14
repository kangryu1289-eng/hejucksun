using UnityEngine;
namespace PirateGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(ShipHealth))]
    public sealed class ShipMotor : MonoBehaviour
    {
        public float forwardSpeed = 3.5f;
        public float reverseSpeed = 1.75f;
        public float turnSpeed = 110f;
        public Vector2 arenaHalfSize = new Vector2(12, 6.5f);
        Rigidbody2D body;
        ShipHealth health;
        float throttle, steering;
        void Awake() { body = GetComponent<Rigidbody2D>(); health = GetComponent<ShipHealth>(); }
        public void SetInput(float forward, float turn) { throttle = Mathf.Clamp(forward, -1, 1); steering = Mathf.Clamp(turn, -1, 1); }
        void FixedUpdate()
        {
            if (!health.IsAlive) return;
            float angle = body.rotation - steering * turnSpeed * Time.fixedDeltaTime;
            body.MoveRotation(angle);
            Vector2 forward = Quaternion.Euler(0, 0, angle) * Vector2.up;
            body.linearVelocity = forward * throttle * (throttle >= 0 ? forwardSpeed : reverseSpeed);
            Vector2 p = body.position;
            Vector2 bounded = new Vector2(Mathf.Clamp(p.x, -arenaHalfSize.x + 0.6f, arenaHalfSize.x - 0.6f), Mathf.Clamp(p.y, -arenaHalfSize.y + 0.6f, arenaHalfSize.y - 0.6f));
            if (p != bounded) body.position = bounded;
        }
        void OnDisable() { if (body) { body.linearVelocity = Vector2.zero; body.angularVelocity = 0; } }
    }
}
