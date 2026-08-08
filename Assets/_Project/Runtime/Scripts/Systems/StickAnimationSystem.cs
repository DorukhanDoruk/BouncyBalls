using Runtime.Components;
using Unity.Entities;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(ArrivalResolveSystem))]
    public partial class StickAnimationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var stickAnimation in SystemAPI.Query<RefRW<StickAnimationComponent>>())
            {
                stickAnimation.ValueRW.DipElapsed += deltaTime;
                stickAnimation.ValueRW.ShiftElapsed += deltaTime;
            }
        }
    }
}
