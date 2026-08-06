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

            foreach (var ballEntity in arrived)
            {
                var path = SystemAPI.GetSingletonBuffer<PathElement>();
                var stickRefs = SystemAPI.GetSingletonBuffer<StickRefElement>();
                var discLookup = SystemAPI.GetBufferLookup<DiscElement>();

                var ball = EntityManager.GetComponentData<BallComponent>(ballEntity);
                var hop = EntityManager.GetComponentData<HopState>(ballEntity);

                int arrivedStickIndex = path[hop.ToPathIndex].StickIndex;
                var discs = EntityManager.GetBuffer<DiscElement>(stickRefs[arrivedStickIndex].Value);

                bool broke = discs.Length > 0 && discs[discs.Length - 1].Color == ball.Color;
                if (broke)
                {
                    discs.RemoveAt(discs.Length - 1);
                    ball.Remaining--;
                }

                Debug.Log($"[{nameof(ArrivalResolveSystem)}] path {hop.ToPathIndex} -> stick {arrivedStickIndex}, \nbroke={{broke}}, remaining={{ball.Remaining}}");

                if (ball.Remaining <= 0)
                {
                    Debug.Log($"[{nameof(ArrivalResolveSystem)}] ball exhausted, destroying.");
                    EntityManager.DestroyEntity(ballEntity);
                    continue;
                }

                int nextPathIndex = PathUtil.ResolveNext(hop.ToPathIndex, path, stickRefs, discLookup);

                if (nextPathIndex < 0)
                {
                    Debug.Log($"[{nameof(ArrivalResolveSystem)}] no stick left with discs, destroying ball.");
                    EntityManager.DestroyEntity(ballEntity);
                    continue;
                }

                var nextStick = EntityManager.GetComponentData<Stick>(stickRefs[path[nextPathIndex].StickIndex].Value);

                EntityManager.SetComponentData(ballEntity, ball);
                EntityManager.SetComponentData(ballEntity, HopUtil.BeginHop(hop.ToPathIndex, hop.ToPosition, nextPathIndex, nextStick.Position, config));
            }

            arrived.Dispose();
        }
    }
}
