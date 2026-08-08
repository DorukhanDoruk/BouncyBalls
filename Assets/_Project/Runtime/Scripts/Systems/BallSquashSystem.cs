using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(HopMotionSystem))]
    public partial class BallSquashSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<AnimationConfigComponent>();
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnUpdate()
        {
            var animation = SystemAPI.GetSingleton<AnimationConfigComponent>();
            var config = SystemAPI.GetSingleton<BallConfigComponent>();

            foreach (var (hop, transform) in SystemAPI.Query<RefRO<HopState>, RefRW<TransformComponent>>())
            {
                var hopState = hop.ValueRO;

                float takeoff = 1f - math.saturate(hopState.Elapsed / animation.HopStretch.Duration);
                float landing = 1f - math.saturate((hopState.Duration - hopState.Elapsed) / animation.LandSquash.Duration);

                float stretch = animation.HopStretch.Evaluate(takeoff);
                float squash = animation.LandSquash.Evaluate(landing);

                float scaleY = math.lerp(1f, config.MaxStretch, stretch) * math.lerp(1f, config.MinSquash, squash);
                float scaleXZ = math.rsqrt(scaleY);

                transform.ValueRW.Scale = new float3(scaleXZ, scaleY, scaleXZ);
            }
        }
    }
}
