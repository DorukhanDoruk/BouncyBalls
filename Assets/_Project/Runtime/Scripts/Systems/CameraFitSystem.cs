using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Utility;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class CameraFitSystem : SystemBase
    {
        private Camera _camera;
        private Bounds _playfield;

        private int _lastScreenWidth;
        private int _lastScreenHeight;

        protected override void OnCreate()
        {
            RequireForUpdate<LevelLayoutComponent>();
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnStartRunning()
        {
            _camera = Camera.main;
            _playfield = CalculatePlayfieldBounds();

            _lastScreenWidth = 0;
            _lastScreenHeight = 0;
        }

        protected override void OnUpdate()
        {
            if (Screen.width == _lastScreenWidth && Screen.height == _lastScreenHeight)
            {
                return;
            }

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            Fit();
        }

        private Bounds CalculatePlayfieldBounds()
        {
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var config = SystemAPI.GetSingleton<BallConfigComponent>();

            var bounds = new Bounds();
            bool initialized = false;

            foreach (var (stick, discs) in SystemAPI.Query<RefRO<Stick>, DynamicBuffer<DiscElement>>())
            {
                float3 basePosition = stick.ValueRO.Position;
                float3 stackTop = basePosition + new float3(0f, discs.Length * layout.DiscStackSpacing, 0f);

                Encapsulate(ref bounds, ref initialized, basePosition);
                Encapsulate(ref bounds, ref initialized, stackTop);
            }

            var columnRefs = SystemAPI.GetSingletonBuffer<GridColumnRefElement>();
            for (int c = 0; c < columnRefs.Length; c++)
            {
                var ballQueue = EntityManager.GetBuffer<GridBallElement>(columnRefs[c].Entity);
                for (int i = 0; i < ballQueue.Length; i++)
                {
                    Encapsulate(ref bounds, ref initialized, SlotLayoutUtil.GridPosition(layout, c, columnRefs.Length, i));
                }
            }

            for (int i = 0; i < config.MaxDockBalls; i++)
            {
                Encapsulate(ref bounds, ref initialized, SlotLayoutUtil.DockPosition(layout, i, config.MaxDockBalls));
            }

            bounds.Expand(layout.CameraPadding * 2f);
            return bounds;
        }

        private static void Encapsulate(ref Bounds bounds, ref bool initialized, float3 point)
        {
            if (!initialized)
            {
                bounds = new Bounds(point, Vector3.zero);
                initialized = true;
                return;
            }

            bounds.Encapsulate((Vector3)point);
        }

        private void Fit()
        {
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var cameraTransform = _camera.transform;

            cameraTransform.position = _playfield.center - cameraTransform.forward * layout.CameraDistance;

            float aspect = (float)Screen.width / Screen.height;
            float requiredSize = 0f;

            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 worldCorner = new Vector3(
                    (corner & 1) == 0 ? _playfield.min.x : _playfield.max.x,
                    (corner & 2) == 0 ? _playfield.min.y : _playfield.max.y,
                    (corner & 4) == 0 ? _playfield.min.z : _playfield.max.z);

                Vector3 local = cameraTransform.InverseTransformPoint(worldCorner);

                requiredSize = Mathf.Max(requiredSize, Mathf.Abs(local.y));
                requiredSize = Mathf.Max(requiredSize, Mathf.Abs(local.x) / aspect);
            }

            _camera.orthographicSize = requiredSize;

            Debug.Log($"[{nameof(CameraFitSystem)}] {Screen.width}x{Screen.height} (aspect {aspect:F2}) -> orthographicSize {requiredSize:F2}");
        }
    }
}
