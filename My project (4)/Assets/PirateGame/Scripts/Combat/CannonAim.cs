using UnityEngine;
using UnityEngine.InputSystem;
namespace PirateGame
{
    public sealed class CannonAim : MonoBehaviour
    {
        public Camera worldCamera;
        public LineRenderer guide;
        public float range = 8f;
        public Vector2 Target { get; private set; }
        public Vector2 Direction => (Target - (Vector2)transform.position).normalized;
        public float Distance => Vector2.Distance(transform.position, Target);
        public bool InRange => Distance >= 0.7f && Distance <= range;
        void Update()
        {
            if (Mouse.current == null || !worldCamera) return;
            Vector3 screen = Mouse.current.position.ReadValue();
            screen.z = -worldCamera.transform.position.z;
            Target = worldCamera.ScreenToWorldPoint(screen);
            if (!guide) return;
            guide.SetPosition(0, transform.position);
            guide.SetPosition(1, (Vector2)transform.position + Direction * Mathf.Min(Distance, range));
            Color color = InRange ? new Color(0.7f, 1, 0.85f, 0.65f) : new Color(1, 0.45f, 0.35f, 0.65f);
            guide.startColor = guide.endColor = color;
        }
    }
}
