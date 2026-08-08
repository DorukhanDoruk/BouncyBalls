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
            _activeBallQuery = SystemAPI.QueryBuilder().WithAll<HopState>().Build();
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

            var firstStick = EntityManager.GetComponentData<Stick>(stickRefs[path[0].StickIndex].Entity);
            float3 targetPosition = SlotLayoutUtil.StickTopPosition(firstStick);
            float3 startPosition = EntityManager.GetComponentData<TransformComponent>(ballEntity).Position;

            if (!TryTakeFromSlots(ballEntity))
            {
                return;
            }

            float speedMultiplier = SystemAPI.GetSingleton<LoopModeComponent>().IsActive ? config.LoopModeSpeedMultiplier : 1f;

            EntityManager.AddComponentData(ballEntity, HopUtil.BeginHop(0, startPosition, 0, targetPosition, config, speedMultiplier));
            EntityManager.AddComponentData(ballEntity, new LapProgressComponent { StepsTaken = 0 });

            launchState.LastLaunchTime = now;
            SystemAPI.SetSingleton(launchState);

            var ball = EntityManager.GetComponentData<BallComponent>(ballEntity);
            Debug.Log($"[{nameof(LaunchSystem)}] launched {ball.Color}({ball.Remaining}), active balls = {_activeBallQuery.CalculateEntityCount()}");
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
