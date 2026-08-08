using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Configs;
using Runtime.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class InstancedRenderSystem : SystemBase
    {
        private RenderConfigSO _renderConfig;

        private RenderParams _stickBodyParams;
        private RenderParams _stickBaseParams;
        private RenderParams _discParams;
        private RenderParams _ballParams;
        private RenderParams _holeParams;
        private RenderParams _dockParams;

        private NativeList<Matrix4x4> _holeMatrices;
        private NativeList<Matrix4x4> _dockMatrices;

        private NativeList<Matrix4x4> _stickBodyMatrices;
        private NativeList<Matrix4x4> _stickBaseMatrices;
        private NativeList<Matrix4x4> _discMatrices;
        private NativeList<Matrix4x4> _ballMatrices;

        protected override void OnCreate()
        {
            RequireForUpdate<LevelLayoutComponent>();
            RequireForUpdate<BallConfigComponent>();

            _holeMatrices = new NativeList<Matrix4x4>(64, Allocator.Persistent);
            _dockMatrices = new NativeList<Matrix4x4>(8, Allocator.Persistent);

            _stickBodyMatrices = new NativeList<Matrix4x4>(64, Allocator.Persistent);
            _stickBaseMatrices = new NativeList<Matrix4x4>(64, Allocator.Persistent);
            _discMatrices = new NativeList<Matrix4x4>(256, Allocator.Persistent);
            _ballMatrices = new NativeList<Matrix4x4>(16, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _holeMatrices.Dispose();
            _dockMatrices.Dispose();
            _stickBodyMatrices.Dispose();
            _stickBaseMatrices.Dispose();
            _discMatrices.Dispose();
            _ballMatrices.Dispose();
        }

        protected override void OnStartRunning()
        {
            _renderConfig = Resources.Load<RenderConfigSO>(RenderConfigSO.ResourcePath);

            _stickBodyParams = new RenderParams(_renderConfig.StickBodyMaterial) { receiveShadows = true };
            _stickBaseParams = new RenderParams(_renderConfig.StickBaseMaterial) { receiveShadows = true };
            _discParams = new RenderParams(_renderConfig.DiscMaterial) { receiveShadows = true };
            _holeParams = new RenderParams(_renderConfig.HoleMaterial) { receiveShadows = true };
            _dockParams = new RenderParams(_renderConfig.DockMaterial) { receiveShadows = true };
            _ballParams = new RenderParams(_renderConfig.BallMaterial) { receiveShadows = true };
        }

        protected override void OnUpdate()
        {
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();

            BuildLayoutMatrices();

            _stickBodyMatrices.Clear();
            _stickBaseMatrices.Clear();
            _discMatrices.Clear();
            _ballMatrices.Clear();

            float discHeight = layout.DiscStackSpacing;

            foreach (var (stick, discs) in SystemAPI.Query<RefRO<Stick>, DynamicBuffer<DiscElement>>())
            {
                float3 basePosition = stick.ValueRO.Position;

                _stickBodyMatrices.Add(Matrix4x4.TRS(
                    basePosition + new float3(0f, stick.ValueRO.Height * 0.5f, 0f),
                    Quaternion.identity,
                    new Vector3(
                        _renderConfig.StickBodyScale.x,
                        _renderConfig.StickBodyScale.y * stick.ValueRO.Height,
                        _renderConfig.StickBodyScale.z)));

                if (discs.Length == 0)
                {
                    _stickBaseMatrices.Add(Matrix4x4.TRS(
                        basePosition + new float3(0f, discHeight * 0.5f, 0f),
                        Quaternion.identity,
                        _renderConfig.StickBaseScale));
                }

                for (int slot = 0; slot < discs.Length; slot++)
                {
                    _discMatrices.Add(Matrix4x4.TRS(
                        basePosition + new float3(0f, discHeight * (slot + 0.5f), 0f),
                        Quaternion.identity,
                        _renderConfig.DiscScale));
                }
            }

            foreach (var ballTransform in SystemAPI.Query<RefRO<TransformComponent>>().WithAll<BallComponent>())
            {
                var transform = ballTransform.ValueRO;

                _ballMatrices.Add(Matrix4x4.TRS(
                    transform.Position,
                    Quaternion.Euler(transform.Rotation),
                    Vector3.Scale(_renderConfig.BallScale, transform.Scale)));
            }

            Draw(_holeParams, _renderConfig.HoleMesh, _holeMatrices);
            Draw(_dockParams, _renderConfig.DockMesh, _dockMatrices);
            Draw(_stickBodyParams, _renderConfig.StickBodyMesh, _stickBodyMatrices);
            Draw(_stickBaseParams, _renderConfig.StickBaseMesh, _stickBaseMatrices);
            Draw(_discParams, _renderConfig.DiscMesh, _discMatrices);
            Draw(_ballParams, _renderConfig.BallMesh, _ballMatrices);
        }
        
        private void BuildLayoutMatrices()
        {
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var config = SystemAPI.GetSingleton<BallConfigComponent>();

            _holeMatrices.Clear();
            foreach (var stick in SystemAPI.Query<RefRO<Stick>>())
            {
                float3 position = stick.ValueRO.Position;
                position.y += _renderConfig.GroundOffset;

                _holeMatrices.Add(Matrix4x4.TRS(position, Quaternion.identity, _renderConfig.HoleScale));
            }

            _dockMatrices.Clear();
            for (int slot = 0; slot < config.MaxDockBalls; slot++)
            {
                float3 position = SlotLayoutUtil.DockPosition(layout, slot, config.MaxDockBalls);
                position.y += _renderConfig.GroundOffset;

                _dockMatrices.Add(Matrix4x4.TRS(position, Quaternion.identity, _renderConfig.DockScale));
            }
        }

        private static void Draw(in RenderParams renderParams, Mesh mesh, NativeList<Matrix4x4> matrices)
        {
            if (matrices.Length == 0)
            {
                return;
            }

            Graphics.RenderMeshInstanced(renderParams, mesh, 0, matrices.AsArray());
        }
    }
}
