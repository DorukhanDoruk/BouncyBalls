using Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(ArrivalResolveSystem))]
    public partial class DiscShatterSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<AnimationConfigComponent>();
        }

        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var animation = SystemAPI.GetSingleton<AnimationConfigComponent>();

            float lifetime = animation.DiscPieceDissolveDelay + animation.DiscPieceDissolve.Duration;
            var finished = new NativeList<Entity>(Allocator.Temp);

            foreach (var (pieceRef, entity) in SystemAPI.Query<RefRW<DiscPieceComponent>>().WithEntityAccess())
            {
                ref var piece = ref pieceRef.ValueRW;
                piece.Elapsed += deltaTime;

                if (piece.Position.y > piece.GroundY)
                {
                    piece.Velocity.y -= animation.DiscPieceGravity * deltaTime;
                    piece.Position += piece.Velocity * deltaTime;
                    piece.Rotation += piece.AngularVelocity * deltaTime;

                    // Settle on the ground instead of sinking through it.
                    if (piece.Position.y <= piece.GroundY)
                    {
                        piece.Position.y = piece.GroundY;
                        piece.Velocity = float3.zero;
                        piece.AngularVelocity = float3.zero;
                    }
                }

                if (piece.Elapsed >= lifetime)
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
