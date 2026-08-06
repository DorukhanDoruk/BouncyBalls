using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Systems
{
    public partial class HopMotionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<AnimationConfigRefComponent>();
        }

        protected override void OnUpdate()
        {
            var configBlob = SystemAPI.GetSingleton<AnimationConfigRefComponent>().ConfigBlob;
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (hop, transform) in SystemAPI.Query<RefRW<HopState>, RefRW<TransformComponent>>())
            {
                ref var hopState = ref hop.ValueRW;

                hopState.Elapsed += deltaTime;

                float t = math.saturate(hopState.Elapsed / hopState.Duration);

                float3 position = math.lerp(hopState.FromPosition, hopState.ToPosition, t);
                position.y += configBlob.Value.HopArc.Evaulate(t) * hopState.ArcHeight;

                transform.ValueRW.Position = position;
            }
        }
    }
}
