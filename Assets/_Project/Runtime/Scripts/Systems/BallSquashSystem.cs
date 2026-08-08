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
            RequireForUpdate<AnimationConfigRefComponent>();
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnUpdate()
        {
            var configBlob = SystemAPI.GetSingleton<AnimationConfigRefComponent>().ConfigBlob;
            var config = SystemAPI.GetSingleton<BallConfigComponent>();

            foreach (var (hop, transform) in SystemAPI.Query<RefRO<HopState>, RefRW<TransformComponent>>())
            {
                var hopState = hop.ValueRO;

                float takeoff = 1f - math.saturate(hopState.Elapsed / configBlob.Value.HopStretch.Duration);
                float landing = 1f - math.saturate((hopState.Duration - hopState.Elapsed) / configBlob.Value.LandSquash.Duration);

                float stretch = configBlob.Value.HopStretch.Evaulate(takeoff);
                float squash = configBlob.Value.LandSquash.Evaulate(landing);

                float scaleY = math.lerp(1f, config.MaxStretch, stretch) * math.lerp(1f, config.MinSquash, squash);
                float scaleXZ = math.rsqrt(scaleY);

                transform.ValueRW.Scale = new float3(scaleXZ, scaleY, scaleXZ);
            }
        }
    }
}
