using Runtime.Components;
using Unity.Entities;
using UnityEngine;
namespace Runtime.Systems
{
    public partial class ConfigVerificationSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
            RequireForUpdate<AnimationConfigComponent>();
        }

        protected override void OnStartRunning()
        {
            var ball = SystemAPI.GetSingleton<BallConfigComponent>();
            var animation = SystemAPI.GetSingleton<AnimationConfigComponent>();

            Debug.Log($"[ConfigVerify] BallConfig: HopSpeed={ball.HopSpeed}, MaxActiveBalls={ball.MaxActiveBalls}");
            Debug.Log($"[ConfigVerify] StickDip: duration={animation.StickDip.Duration}, ease={animation.StickDip.EaseType}, amount={animation.StickDipAmount}");
            Debug.Log($"[ConfigVerify] DiscShatter: gravity={animation.DiscPieceGravity}, dissolve={animation.DiscPieceDissolveDelay}+{animation.DiscPieceDissolve.Duration}s");
        }

        protected override void OnUpdate() { }
    }
}
