using Runtime.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Runtime.Services
{
    public class InputService : IService, ITickable
    {
        private readonly Camera _camera;
        private readonly GameFlowService _flowService;
        private readonly float _selectionRadius;

        public InputService(Camera camera, GameFlowService flowService, float selectionRadius)
        {
            _camera = camera;
            _flowService = flowService;
            _selectionRadius = selectionRadius;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Tick(float deltaTime)
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
            {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var simulation = _flowService.Simulation;
            if (!simulation.CanLaunch)
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(pointer.position.ReadValue());

            float bestDistance = _selectionRadius;
            int bestColumn = -1;
            int bestDockIndex = -1;

            for (int i = 0; i < simulation.ColumnCount; i++)
            {
                var column = simulation.GridColumn(i);
                if (column.Count == 0)
                {
                    continue;
                }

                float distance = DistanceToRay(ray, column[0].Position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestColumn = i;
                    bestDockIndex = -1;
                }
            }

            for (int i = 0; i < simulation.Dock.Count; i++)
            {
                float distance = DistanceToRay(ray, simulation.Dock[i].Position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestColumn = -1;
                    bestDockIndex = i;
                }
            }

            if (bestColumn >= 0)
            {
                simulation.LaunchFromGrid(bestColumn);
            }
            else if (bestDockIndex >= 0)
            {
                simulation.LaunchFromDock(bestDockIndex);
            }
        }

        // Perpendicular distance from the ray to a point; the direction is already normalized.
        private static float DistanceToRay(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }
    }
}
