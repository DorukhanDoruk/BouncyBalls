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
            Debug.Log($"[ConfigVerify] DiscBreakPop: duration={animation.DiscBreakPop.Duration}, ease={animation.DiscBreakPop.EaseType}, " +
                      $"eval(0)={animation.DiscBreakPop.Evaluate(0f)}, eval(0.5)={animation.DiscBreakPop.Evaluate(0.5f)}, eval(1)={animation.DiscBreakPop.Evaluate(1f)}");
        }

        protected override void OnUpdate() { }
    }
}
