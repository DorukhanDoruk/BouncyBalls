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
            RequireForUpdate<AnimationConfigRefComponent>();
        }

        protected override void OnStartRunning()
        {
            var ball = SystemAPI.GetSingleton<BallConfigComponent>();
            var anim = SystemAPI.GetSingleton<AnimationConfigRefComponent>();

            Debug.Log($"[ConfigVerify] BallConfig: HopSpeed={ball.HopSpeed}, MaxActiveBalls={ball.MaxActiveBalls}");

            if (!anim.ConfigBlob.IsCreated)
            {
                Debug.LogError("[ConfigVerify] Animation blob was never created.");
                return;
            }

            ref var blob = ref anim.ConfigBlob.Value;
            Debug.Log($"[ConfigVerify] HopArc: duration={blob.HopArc.Duration}, samples={blob.HopArc.Samples.Length}, eval(0)={blob.HopArc.Evaulate(0f)}, eval(0.5)={blob.HopArc.Evaulate(0.5f)}, eval(1)={blob.HopArc.Evaulate(1f)}");
        }

        protected override void OnUpdate() { }
    }
}
