using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(HopMotionSystem))]
    public partial class ArrivalResolveSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
            RequireForUpdate<AnimationConfigComponent>();
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
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var animation = SystemAPI.GetSingleton<AnimationConfigComponent>();
            bool loopMode = SystemAPI.GetSingleton<LoopModeComponent>().IsActive;

            // Creating entities is structural, so breaks are collected and spawned after the loop.
            var brokenDiscs = new NativeList<DyingDiscComponent>(Allocator.Temp);
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
                var arrivedStickEntity = stickRefs[arrivedStickIndex].Entity;
                var discs = EntityManager.GetBuffer<DiscElement>(arrivedStickEntity);

                bool broke = discs.Length > 0 && discs[discs.Length - 1].Color == ball.Color;
                if (broke)
                {
                    int topSlot = discs.Length - 1;
                    var arrivedStick = EntityManager.GetComponentData<Stick>(arrivedStickEntity);

                    brokenDiscs.Add(new DyingDiscComponent
                    {
                        Color = discs[topSlot].Color,
                        Position = SlotLayoutUtil.DiscPosition(arrivedStick, topSlot, discs.Length, layout.DiscStackSpacing),
                        Elapsed = 0f,
                        Duration = animation.DiscBreakPop.Duration,
                    });

                    discs.RemoveAt(topSlot);
                    ball.Remaining--;
                }

                var stickAnimation = EntityManager.GetComponentData<StickAnimationComponent>(arrivedStickEntity);
                stickAnimation.DipElapsed = 0f;
                if (broke)
                {
                    stickAnimation.ShiftElapsed = 0f;
                }

                EntityManager.SetComponentData(arrivedStickEntity, stickAnimation);

                Debug.Log($"[{nameof(ArrivalResolveSystem)}] path {hop.ToPathIndex} -> stick {arrivedStickIndex}, broke={broke}, remaining={ball.Remaining}");
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
                    Debug.Log($"[{nameof(ArrivalResolveSystem)}] lap complete at stick {arrivedStickIndex} ({lap.StepsTaken + nextSteps} steps), returning to dock.");
                    EntityManager.SetComponentData(ballEntity, ball);
                    ReturnToDock(ballEntity);
                    continue;
                }

                var nextStick = EntityManager.GetComponentData<Stick>(stickRefs[path[nextPathIndex].StickIndex].Entity);
                float3 nextPosition = SlotLayoutUtil.StickTopPosition(nextStick);

                EntityManager.SetComponentData(ballEntity, ball);
                EntityManager.SetComponentData(ballEntity, lap);
                EntityManager.SetComponentData(ballEntity, HopUtil.BeginHop(hop.ToPathIndex, hop.ToPosition, nextPathIndex, nextPosition, config, speedMultiplier));
            }

            foreach (var brokenDisc in brokenDiscs)
            {
                var discEntity = EntityManager.CreateEntity(typeof(DyingDiscComponent));
                EntityManager.SetComponentData(discEntity, brokenDisc);
            }

            brokenDiscs.Dispose();
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
