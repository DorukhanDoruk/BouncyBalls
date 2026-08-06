using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Configs;
using Runtime.Utility;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateBefore(typeof(HopMotionSystem))]
    public partial class DebugBallSpawnSystem : SystemBase
    {
        private const int _startPathIndex = 0;

        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnStartRunning()
        {
            var viewConfig = Resources.Load<ViewConfigSO>(ViewConfigSO.ResourcePath);
            var config = SystemAPI.GetSingleton<BallConfigComponent>();
            var path = SystemAPI.GetSingletonBuffer<PathElement>();
            var stickRefs = SystemAPI.GetSingletonBuffer<StickRefElement>();
            
            float3 targetPosition = EntityManager.GetComponentData<Stick>(stickRefs[path[_startPathIndex].StickIndex].Value).Position;
            var ballEntity = EntityManager.CreateEntity(typeof(BallComponent), typeof(TransformComponent), typeof(HopState));

            EntityManager.SetName(ballEntity, "DebugBall");
            EntityManager.SetComponentData(ballEntity, new BallComponent
            {
                Color = viewConfig.DebugBallColor, Remaining = viewConfig.DebugBallRemaining,
            });

            EntityManager.SetComponentData(ballEntity, new TransformComponent
            {
                Position = targetPosition, Rotation = float3.zero,
                Scale = new float3(1f, 1f, 1f),
            });

            EntityManager.SetComponentData(ballEntity, HopUtil.BeginHop(_startPathIndex, targetPosition, _startPathIndex, targetPosition, config));

            Debug.Log($"[{nameof(DebugBallSpawnSystem)}] spawned {viewConfig.DebugBallColor} ball \n(remaining={viewConfig.DebugBallRemaining}) first hop -> path {_startPathIndex}");
        }

        protected override void OnUpdate() { }
    }
}
