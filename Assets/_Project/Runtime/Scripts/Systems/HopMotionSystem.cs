using Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Systems
{
    [BurstCompile]
    public partial struct HopMotionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new HopMotionJob { DeltaTime = SystemAPI.Time.DeltaTime }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct HopMotionJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref HopState hop, ref TransformComponent transform)
        {
            hop.Elapsed += DeltaTime;

            float t = math.saturate(hop.Elapsed / hop.Duration);
            float arc = 4f * t * (1f - t);

            float3 position = math.lerp(hop.FromPosition, hop.ToPosition, t);
            position.y += arc * hop.ArcHeight;

            transform.Position = position;
        }
    }
}
