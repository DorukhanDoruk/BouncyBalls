using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Configs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Runtime.Systems
{
    public partial class PointerSelectionSystem : SystemBase
    {
        private Camera _camera;
        private float _ballLift;

        protected override void OnCreate()
        {
            RequireForUpdate<LaunchRequestComponent>();
        }

        protected override void OnStartRunning()
        {
            _camera = Camera.main;

            var renderConfig = Resources.Load<RenderConfigSO>(RenderConfigSO.ResourcePath);
            _ballLift = -renderConfig.BallMesh.bounds.min.y * renderConfig.BallScale.y;
        }

        protected override void OnUpdate()
        {
            var request = SystemAPI.GetSingleton<LaunchRequestComponent>();
            if (request.Locked)
            {
                return;
            }

            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(pointer.position.ReadValue());

            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var columnRefs = SystemAPI.GetSingletonBuffer<GridColumnRefElement>();
            var dockBalls = SystemAPI.GetSingletonBuffer<DockBallElement>();

            var selected = Entity.Null;
            float bestDistanceSq = layout.BallSelectionRadius * layout.BallSelectionRadius;

            for (int c = 0; c < columnRefs.Length; c++)
            {
                var ballQueue = EntityManager.GetBuffer<GridBallElement>(columnRefs[c].Entity);
                if (ballQueue.Length == 0)
                {
                    continue;
                }

                Consider(ballQueue[0].Entity, ray, ref selected, ref bestDistanceSq);
            }

            for (int i = 0; i < dockBalls.Length; i++)
            {
                Consider(dockBalls[i].Entity, ray, ref selected, ref bestDistanceSq);
            }

            if (selected == Entity.Null)
            {
                return;
            }

            request.Ball = selected;
            SystemAPI.SetSingleton(request);
        }

        private static bool TryGetPlaneHit(in Ray ray, float planeY, out float3 hitPoint)
        {
            hitPoint = default;

            if (math.abs(ray.direction.y) < 1e-6f)
            {
                return false;
            }

            float distance = (planeY - ray.origin.y) / ray.direction.y;
            if (distance < 0f)
            {
                return false;
            }

            hitPoint = (float3)ray.origin + (float3)ray.direction * distance;
            return true;
        }

        private void Consider(Entity ballEntity, in Ray ray, ref Entity selected, ref float bestDistanceSq)
        {
            float3 ballPosition = EntityManager.GetComponentData<TransformComponent>(ballEntity).Position;

            if (!TryGetPlaneHit(ray, ballPosition.y + _ballLift, out float3 hitPoint))
            {
                return;
            }

            float distanceSq = math.distancesq(ballPosition.xz, hitPoint.xz);
            if (distanceSq > bestDistanceSq)
            {
                return;
            }

            bestDistanceSq = distanceSq;
            selected = ballEntity;
        }
    }
}
