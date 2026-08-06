using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Utility;
using Unity.Entities;
namespace Runtime.Systems
{
    public partial class SlotPositionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<LevelLayoutComponent>();
        }

        protected override void OnUpdate()
        {
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var columnRefs = SystemAPI.GetSingletonBuffer<GridColumnRefElement>();
            var dockBalls = SystemAPI.GetSingletonBuffer<DockBallElement>();

            for (int c = 0; c < columnRefs.Length; c++)
            {
                var ballQueue = EntityManager.GetBuffer<GridBallElement>(columnRefs[c].Entity);
                for (int i = 0; i < ballQueue.Length; i++)
                {
                    WritePosition(ballQueue[i].Entity, SlotLayoutUtil.GridPosition(layout, c, i));
                }
            }

            for (int i = 0; i < dockBalls.Length; i++)
            {
                WritePosition(dockBalls[i].Entity, SlotLayoutUtil.DockPosition(layout, i));
            }
        }

        private void WritePosition(Entity ballEntity, Unity.Mathematics.float3 position)
        {
            var transform = EntityManager.GetComponentData<TransformComponent>(ballEntity);
            transform.Position = position;
            EntityManager.SetComponentData(ballEntity, transform);
        }
    }
}
