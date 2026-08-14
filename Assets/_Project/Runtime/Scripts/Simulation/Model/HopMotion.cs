using Runtime.Config;
using Runtime.Core;
using UnityEngine;
namespace Runtime.Simulation.Model
{
    public readonly struct HopMotion
    {
        public readonly Vector3 From;
        public readonly Vector3 To;

        public readonly float Duration;
        public readonly float ArcHeight;
        
        // Flat slide for balls settling into a grid or dock slot.
        public HopMotion(Vector3 from, Vector3 to, float duration)
        {
            From = from;
            To = to;
            Duration = duration;
            ArcHeight = 0f;
        }

        public HopMotion(Vector3 from, Vector3 to, GameConfig gameConfig)
        {
            From = from;
            To = to;
            
            
            var distance = Vector3.Distance(from, to);
            if (distance > float.Epsilon)
            {
                Duration = gameConfig.HopDuration;
                ArcHeight = Mathf.Min(distance * gameConfig.ArcHeightPerUnit, gameConfig.MaxArcHeight);
            }
            else
            {
                Duration = gameConfig.InPlaceBounceDuration;
                ArcHeight = gameConfig.InPlaceBounceHeight;
            }
        }

        public Vector3 Evaluate(float elapsed)
        {
            float t = Mathf.Clamp01(elapsed / Duration);
            Vector3 point = Vector3.Lerp(From, To, t);

            // Smoothly arcs up and down, reaching max ArcHeight right in the middle (t = 0.5)
            point.y += 4f * ArcHeight * t * (1f - t);
            return point;
        }
    }
}
