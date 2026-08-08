using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateBefore(typeof(HopMotionSystem))]
    public partial class LoopModeSystem : SystemBase
    {
        private EntityQuery _activeBallQuery;

        protected override void OnCreate()
        {
            RequireForUpdate<LoopModeComponent>();
            RequireForUpdate<BallConfigComponent>();
            _activeBallQuery = SystemAPI.QueryBuilder().WithAll<HopState>().Build();
        }

        protected override void OnUpdate()
        {
            if (SystemAPI.GetSingleton<LoopModeComponent>().IsActive)
            {
                return;
            }

            if (SystemAPI.GetSingleton<GameStateComponent>().GameState != GameState.Playing)
            {
                return;
            }

            var columnRefs = SystemAPI.GetSingletonBuffer<GridColumnRefElement>();
            for (int c = 0; c < columnRefs.Length; c++)
            {
                if (EntityManager.GetBuffer<GridBallElement>(columnRefs[c].Entity).Length > 0)
                {
                    return;
                }
            }

            var config = SystemAPI.GetSingleton<BallConfigComponent>();
            var dockBalls = SystemAPI.GetSingletonBuffer<DockBallElement>();

            if (dockBalls.Length + _activeBallQuery.CalculateEntityCount() != config.MaxDockBalls)
            {
                return;
            }

            var path = SystemAPI.GetSingletonBuffer<PathElement>();
            var stickRefs = SystemAPI.GetSingletonBuffer<StickRefElement>();
            var firstStick = EntityManager.GetComponentData<Stick>(stickRefs[path[0].StickIndex].Entity);
            float3 targetPosition = SlotLayoutUtil.StickTopPosition(firstStick);

            var toLaunch = new NativeList<Entity>(Allocator.Temp);
            for (int i = 0; i < dockBalls.Length; i++)
            {
                toLaunch.Add(dockBalls[i].Entity);
            }

            dockBalls.Clear();
            SystemAPI.SetSingleton(new LoopModeComponent { IsActive = true });

            foreach (var ballEntity in toLaunch)
            {
                float3 startPosition = EntityManager.GetComponentData<TransformComponent>(ballEntity).Position;

                EntityManager.AddComponentData(ballEntity,
                    HopUtil.BeginHop(0, startPosition, 0, targetPosition, config, config.LoopModeSpeedMultiplier));
                EntityManager.AddComponentData(ballEntity, new LapProgressComponent { StepsTaken = 0 });
            }

            Debug.Log($"[{nameof(LoopModeSystem)}] loop mode ON, launched {toLaunch.Length} docked ball(s), speed x{config.LoopModeSpeedMultiplier}");
            toLaunch.Dispose();
        }
    }
}
