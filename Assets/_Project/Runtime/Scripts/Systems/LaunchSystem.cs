using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Utility;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateBefore(typeof(HopMotionSystem))]
    public partial class LaunchSystem : SystemBase
    {
        private EntityQuery _activeBallQuery;

        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
            _activeBallQuery = SystemAPI.QueryBuilder().WithAll<HopState>().WithNone<DockReturnComponent>().Build();
        }

        protected override void OnUpdate()
        {
            var request = SystemAPI.GetSingleton<LaunchRequestComponent>();
            if (request.Ball == Entity.Null)
            {
                return;
            }

            var ballEntity = request.Ball;
            request.Ball = Entity.Null;
            SystemAPI.SetSingleton(request);

            var config = SystemAPI.GetSingleton<BallConfigComponent>();
            var launchState = SystemAPI.GetSingleton<LaunchStateComponent>();

            float now = (float)SystemAPI.Time.ElapsedTime;
            if (now - launchState.LastLaunchTime < config.MinLaunchInterval)
            {
                return;
            }

            if (_activeBallQuery.CalculateEntityCount() >= config.MaxActiveBalls)
            {
                return;
            }

            var path = SystemAPI.GetSingletonBuffer<PathElement>();
            var stickRefs = SystemAPI.GetSingletonBuffer<StickRefElement>();
            var discLookup = SystemAPI.GetBufferLookup<DiscElement>();

            int targetPathIndex = PathUtil.ResolveFirst(path, stickRefs, discLookup);
            if (targetPathIndex < 0)
            {
                return;
            }

            var targetStick = EntityManager.GetComponentData<Stick>(stickRefs[path[targetPathIndex].StickIndex].Entity);
            float3 targetPosition = SlotLayoutUtil.StickTopPosition(targetStick);
            float3 startPosition = EntityManager.GetComponentData<TransformComponent>(ballEntity).Position;

            if (!TryTakeFromSlots(ballEntity))
            {
                return;
            }

            float speedMultiplier = SystemAPI.GetSingleton<LoopModeComponent>().IsActive ? config.LoopModeSpeedMultiplier : 1f;
            speedMultiplier *= config.LaunchSpeedMultiplier;

            EntityManager.AddComponentData(ballEntity, HopUtil.BeginHop(targetPathIndex, startPosition, targetPathIndex, targetPosition, config, speedMultiplier));
            EntityManager.AddComponentData(ballEntity, new LapProgressComponent { StepsTaken = 0 });

            launchState.LastLaunchTime = now;
            SystemAPI.SetSingleton(launchState);
        }

        private bool TryTakeFromSlots(Entity ballEntity)
        {
            var columnRefs = SystemAPI.GetSingletonBuffer<GridColumnRefElement>();
            for (int c = 0; c < columnRefs.Length; c++)
            {
                var ballQueue = EntityManager.GetBuffer<GridBallElement>(columnRefs[c].Entity);
                if (ballQueue.Length > 0 && ballQueue[0].Entity == ballEntity)
                {
                    ballQueue.RemoveAt(0);
                    return true;
                }
            }

            var dockBalls = SystemAPI.GetSingletonBuffer<DockBallElement>();
            for (int i = 0; i < dockBalls.Length; i++)
            {
                if (dockBalls[i].Entity == ballEntity)
                {
                    dockBalls.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
}
