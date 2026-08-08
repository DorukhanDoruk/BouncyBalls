using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Configs.Model;
using Runtime.Utility;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Systems
{
    public partial class SlotPositionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<LevelLayoutComponent>();
            RequireForUpdate<BallConfigComponent>();
            RequireForUpdate<AnimationConfigRefComponent>();
        }

        protected override void OnUpdate()
        {
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var config = SystemAPI.GetSingleton<BallConfigComponent>();
            var configBlob = SystemAPI.GetSingleton<AnimationConfigRefComponent>().ConfigBlob;
            var columnRefs = SystemAPI.GetSingletonBuffer<GridColumnRefElement>();
            var dockBalls = SystemAPI.GetSingletonBuffer<DockBallElement>();

            float deltaTime = SystemAPI.Time.DeltaTime;

            for (int c = 0; c < columnRefs.Length; c++)
            {
                var ballQueue = EntityManager.GetBuffer<GridBallElement>(columnRefs[c].Entity);
                for (int i = 0; i < ballQueue.Length; i++)
                {
                    MoveToSlot(
                        ballQueue[i].Entity,
                        SlotLayoutUtil.GridPosition(layout, c, columnRefs.Length, i),
                        ref configBlob.Value.GridColumnAdvance,
                        deltaTime);
                }
            }

            for (int i = 0; i < dockBalls.Length; i++)
            {
                MoveToSlot(
                    dockBalls[i].Entity,
                    SlotLayoutUtil.DockPosition(layout, i, config.MaxDockBalls),
                    ref configBlob.Value.DockSlotSettle,
                    deltaTime);
            }
        }

        // Scale is reset here because BallSquashSystem deforms balls while they hop
        private void MoveToSlot(Entity ballEntity, float3 target, ref TweenBlob tween, float deltaTime)
        {
            var transform = EntityManager.GetComponentData<TransformComponent>(ballEntity);
            var slot = EntityManager.GetComponentData<SlotTweenComponent>(ballEntity);

            // A new target means the column advanced or the ball just entered the dock.
            if (!target.Equals(slot.To))
            {
                slot.From = transform.Position;
                slot.To = target;
                slot.Elapsed = 0f;
                slot.Duration = tween.Duration;
            }

            slot.Elapsed += deltaTime;
            float t = math.saturate(slot.Elapsed / slot.Duration);

            transform.Position = math.lerp(slot.From, slot.To, tween.Evaulate(t));
            transform.Scale = new float3(1f, 1f, 1f);

            EntityManager.SetComponentData(ballEntity, transform);
            EntityManager.SetComponentData(ballEntity, slot);
        }
    }
}
