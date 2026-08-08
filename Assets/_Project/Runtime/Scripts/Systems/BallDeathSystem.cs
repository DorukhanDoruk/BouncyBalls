using Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(ArrivalResolveSystem))]
    public partial class BallDeathSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<AnimationConfigComponent>();
        }

        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var vanish = SystemAPI.GetSingleton<AnimationConfigComponent>().BallVanish;

            var finished = new NativeList<Entity>(Allocator.Temp);

            foreach (var (dying, transform, entity) in
                     SystemAPI.Query<RefRW<DyingBallComponent>, RefRW<TransformComponent>>().WithEntityAccess())
            {
                dying.ValueRW.Elapsed += deltaTime;
                float scale = 1f - vanish.Evaluate(math.saturate(dying.ValueRO.Elapsed / vanish.Duration));
                transform.ValueRW.Scale = new float3(scale, scale, scale);

                if (dying.ValueRO.Elapsed >= vanish.Duration)
                {
                    finished.Add(entity);
                }
            }

            foreach (var entity in finished)
            {
                EntityManager.DestroyEntity(entity);
            }

            finished.Dispose();
        }
    }
}
