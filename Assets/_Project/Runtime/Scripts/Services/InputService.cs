using Runtime.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Services
{
    public class InputService : IService, ITickable
    {
        private readonly Camera _camera;
        private readonly GameFlowService _flowService;
        private readonly LevelLayout _levelLayout;

        // Balls sit on the board plane, so a tap is projected onto it.
        private readonly Plane _boardPlane = new Plane(Vector3.up, Vector3.zero);

        public InputService(Camera camera, GameFlowService flowService, LevelLayout levelLayout)
        {
            _camera = camera;
            _flowService = flowService;
            _levelLayout = levelLayout;
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

            var simulation = _flowService.Simulation;
            if (!simulation.CanLaunch)
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(pointer.position.ReadValue());
            if (!_boardPlane.Raycast(ray, out float rayDistance))
            {
                return;
            }

            var point = ray.GetPoint(rayDistance);

            float bestDistance = _levelLayout.BallSelectionRadius;
            int bestColumn = -1;
            int bestDockIndex = -1;

            // Only the front ball of a column is playable, so the rest never become candidates.
            for (int i = 0; i < simulation.ColumnCount; i++)
            {
                var column = simulation.GridColumn(i);
                if (column.Count == 0)
                {
                    continue;
                }

                float distance = Vector3.Distance(column[0].Position, point);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestColumn = i;
                    bestDockIndex = -1;
                }
            }

            for (int i = 0; i < simulation.Dock.Count; i++)
            {
                float distance = Vector3.Distance(simulation.Dock[i].Position, point);
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
    }
}
