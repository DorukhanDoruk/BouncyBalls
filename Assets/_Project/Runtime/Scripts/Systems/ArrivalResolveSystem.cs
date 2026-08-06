using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(HopMotionSystem))]
    public partial class ArrivalResolveSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnUpdate()
        {
            var arrived = new NativeList<Entity>(Allocator.Temp);

            foreach (var (hop, entity) in SystemAPI.Query<RefRO<HopState>>().WithAll<BallComponent>().WithEntityAccess())
            {
                if (hop.ValueRO.Elapsed >= hop.ValueRO.Duration)
                {
                    arrived.Add(entity);
                }
            }

            if (arrived.Length == 0)
            {
                arrived.Dispose();
                return;
            }

            var config = SystemAPI.GetSingleton<BallConfigComponent>();
            bool loopMode = SystemAPI.GetSingleton<LoopModeComponent>().IsActive;
            float speedMultiplier = loopMode ? config.LoopModeSpeedMultiplier : 1f;

            foreach (var ballEntity in arrived)
            {
                var path = SystemAPI.GetSingletonBuffer<PathElement>();
                var stickRefs = SystemAPI.GetSingletonBuffer<StickRefElement>();
                var discLookup = SystemAPI.GetBufferLookup<DiscElement>();

                var ball = EntityManager.GetComponentData<BallComponent>(ballEntity);
                var hop = EntityManager.GetComponentData<HopState>(ballEntity);
                var lap = EntityManager.GetComponentData<LapProgressComponent>(ballEntity);

                lap.StepsTaken += (hop.ToPathIndex - hop.FromPathIndex + path.Length) % path.Length;

                int arrivedStickIndex = path[hop.ToPathIndex].StickIndex;
                var discs = EntityManager.GetBuffer<DiscElement>(stickRefs[arrivedStickIndex].Entity);

                bool broke = discs.Length > 0 && discs[discs.Length - 1].Color == ball.Color;
                if (broke)
                {
                    discs.RemoveAt(discs.Length - 1);
                    ball.Remaining--;
                }

                Debug.Log($"[{nameof(ArrivalResolveSystem)}] path {hop.ToPathIndex} -> stick {arrivedStickIndex}, \nbroke={broke}, remaining={ball.Remaining}");
                if (ball.Remaining <= 0)
                {
                    Debug.Log($"[{nameof(ArrivalResolveSystem)}] ball exhausted, destroying.");
                    EntityManager.DestroyEntity(ballEntity);
                    continue;
                }

                int nextPathIndex = PathUtil.ResolveNext(hop.ToPathIndex, path, stickRefs, discLookup);

                if (nextPathIndex < 0)
                {
                    if (!loopMode)
                    {
                        Debug.Log($"[{nameof(ArrivalResolveSystem)}] no stick left with discs, returning to dock.");
                        EntityManager.SetComponentData(ballEntity, ball);
                        ReturnToDock(ballEntity);
                        continue;
                    }

                    EntityManager.SetComponentData(ballEntity, ball);
                    EntityManager.SetComponentData(ballEntity, lap);
                    EntityManager.SetComponentData(ballEntity, HopUtil.BeginHop(hop.ToPathIndex, hop.ToPosition, hop.ToPathIndex, hop.ToPosition, config, speedMultiplier));
                    continue;
                }

                int nextSteps = (nextPathIndex - hop.ToPathIndex + path.Length) % path.Length;
                if (!loopMode && lap.StepsTaken + nextSteps >= path.Length)
                {
                    Debug.Log($"[{nameof(ArrivalResolveSystem)}] lap complete at stick {arrivedStickIndex} \n({lap.StepsTaken + nextSteps} steps), returning to dock.");
                    EntityManager.SetComponentData(ballEntity, ball);
                    ReturnToDock(ballEntity);
                    continue;
                }

                var nextStick = EntityManager.GetComponentData<Stick>(stickRefs[path[nextPathIndex].StickIndex].Entity);

                EntityManager.SetComponentData(ballEntity, ball);
                EntityManager.SetComponentData(ballEntity, lap);
                EntityManager.SetComponentData(ballEntity, HopUtil.BeginHop(hop.ToPathIndex, hop.ToPosition, nextPathIndex, nextStick.Position, config, speedMultiplier));
            }

            arrived.Dispose();
        }

        private void ReturnToDock(Entity ballEntity)
        {
            EntityManager.RemoveComponent<HopState>(ballEntity);
            EntityManager.RemoveComponent<LapProgressComponent>(ballEntity);

            SystemAPI.GetSingletonBuffer<DockBallElement>().Add(new DockBallElement { Entity = ballEntity });
        }
    }
}
