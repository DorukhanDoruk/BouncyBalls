using Runtime.Components;
using Unity.Collections;
using Unity.Entities;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(ArrivalResolveSystem))]
    public partial class DiscBreakPopSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var finished = new NativeList<Entity>(Allocator.Temp);

            foreach (var (dying, entity) in SystemAPI.Query<RefRW<DyingDiscComponent>>().WithEntityAccess())
            {
                dying.ValueRW.Elapsed += deltaTime;

                if (dying.ValueRO.Elapsed >= dying.ValueRO.Duration)
                {
                    finished.Add(entity);
                }
            }

            // Collected first: destroying inside the query would invalidate it.
            foreach (var entity in finished)
            {
                EntityManager.DestroyEntity(entity);
            }

            finished.Dispose();
        }
    }
}
