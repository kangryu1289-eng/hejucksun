using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class AimArcIndicator : MonoBehaviour
    {
        // 실습 TODO: 마우스 조준 목표까지 포물선 모양 LineRenderer를 그린다. 사거리 내외 색상과 착탄 원을 표시한다. 실제 포탄 충돌 규칙은 변경하지 않는다.
        // 구현은 Docs/03_JuiceTutorial.md를 따라 직접 추가합니다.
        public CannonAim aim;
        public ShipHealth health;
        public LineRenderer arc, landing;
        public float height = 1.2f, radius = 0.25f;
        public int segments = 32;
        void LateUpdate()
        {
            bool visible = aim && aim.enabled && health.IsAlive;
            arc.enabled = landing.enabled = visible;
            if (!visible) return;
            if (aim.guide) aim.guide.enabled = false;
            Vector3 from = transform.position;
            Vector3 to = (Vector2)from + aim.Direction * Mathf.Min(aim.Distance, aim.range);
            int count = Mathf.Max(8, segments);
            arc.positionCount = count + 1;
            landing.positionCount = count;
            landing.loop = true;
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                arc.SetPosition(i, Vector3.Lerp(from, to, t) + Vector3.up * (4 * height * t * (1 - t)));
            }
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2 / count;
                landing.SetPosition(i, to + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
            }
            Color c = aim.InRange ? new Color(0.6f, 1, 0.8f) : new Color(1, 0.35f, 0.25f);
            arc.startColor = arc.endColor = landing.startColor = landing.endColor = c;
        }
        void OnDisable()
        {
            if (arc) arc.enabled = false;
            if (landing) landing.enabled = false;
            if (aim && aim.guide) aim.guide.enabled = true;
        }
    }
}
