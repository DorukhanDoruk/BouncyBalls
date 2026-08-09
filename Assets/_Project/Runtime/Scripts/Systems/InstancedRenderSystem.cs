using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Configs;
using Runtime.Configs.Model;
using Runtime.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
namespace Runtime.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class InstancedRenderSystem : SystemBase
    {
        private RenderConfigSO _renderConfig;

        private RenderParams _stickBodyParams;
        private RenderParams _stickBaseParams;
        private RenderParams _backgroundParams;

        private RenderParams[] _discParamsByColor;
        private RenderParams[] _ballParamsByColor;

        // Held because RenderParams keeps these by reference, so re-reading the palette into
        // them is enough to repaint without rebuilding the RenderParams.
        private MaterialPropertyBlock[] _discBlocks;
        private MaterialPropertyBlock[] _ballBlocks;

        private int _baseColorId;
        private int _shadeColorId;
        private int _specColorId;
        private NativeList<Matrix4x4>[] _discMatricesByColor;
        private NativeList<Matrix4x4>[] _ballMatricesByColor;
        private RenderParams _holeParams;
        private RenderParams _dockParams;

        private NativeList<Matrix4x4> _holeMatrices;
        private NativeList<Matrix4x4> _dockMatrices;

        private NativeList<Matrix4x4> _stickBodyMatrices;
        private NativeList<Matrix4x4> _stickBaseMatrices;
        private NativeList<Matrix4x4>[] _pieceMatrices;

        protected override void OnCreate()
        {
            RequireForUpdate<LevelLayoutComponent>();
            RequireForUpdate<BallConfigComponent>();
            RequireForUpdate<AnimationConfigComponent>();

            _renderConfig = Resources.Load<RenderConfigSO>(RenderConfigSO.ResourcePath);

            _holeMatrices = new NativeList<Matrix4x4>(64, Allocator.Persistent);
            _dockMatrices = new NativeList<Matrix4x4>(8, Allocator.Persistent);

            _stickBodyMatrices = new NativeList<Matrix4x4>(64, Allocator.Persistent);
            _stickBaseMatrices = new NativeList<Matrix4x4>(64, Allocator.Persistent);
            int colorCount = System.Enum.GetValues(typeof(DiscColorType)).Length;

            _discMatricesByColor = new NativeList<Matrix4x4>[colorCount];
            _ballMatricesByColor = new NativeList<Matrix4x4>[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                _discMatricesByColor[i] = new NativeList<Matrix4x4>(64, Allocator.Persistent);
                _ballMatricesByColor[i] = new NativeList<Matrix4x4>(8, Allocator.Persistent);
            }

            _pieceMatrices = new NativeList<Matrix4x4>[_renderConfig.DiscPieceMeshes.Length * colorCount];
            for (int i = 0; i < _pieceMatrices.Length; i++)
            {
                _pieceMatrices[i] = new NativeList<Matrix4x4>(16, Allocator.Persistent);
            }
        }

        protected override void OnDestroy()
        {
            _holeMatrices.Dispose();
            _dockMatrices.Dispose();
            _stickBodyMatrices.Dispose();
            _stickBaseMatrices.Dispose();
            for (int i = 0; i < _discMatricesByColor.Length; i++)
            {
                _discMatricesByColor[i].Dispose();
                _ballMatricesByColor[i].Dispose();
            }

            for (int i = 0; i < _pieceMatrices.Length; i++)
            {
                _pieceMatrices[i].Dispose();
            }
        }

        protected override void OnStartRunning()
        {
            _stickBodyParams = new RenderParams(_renderConfig.StickBodyMaterial) { receiveShadows = true, shadowCastingMode = ShadowCastingMode.On };
            _stickBaseParams = new RenderParams(_renderConfig.StickBaseMaterial) { receiveShadows = true, shadowCastingMode = ShadowCastingMode.On };

            _holeParams = new RenderParams(_renderConfig.HoleMaterial) { receiveShadows = true, shadowCastingMode = ShadowCastingMode.On };
            _dockParams = new RenderParams(_renderConfig.DockMaterial) { receiveShadows = true, shadowCastingMode = ShadowCastingMode.On };
            _baseColorId = Shader.PropertyToID("_BaseColor");
            _shadeColorId = Shader.PropertyToID("_ShadeColor");
            _specColorId = Shader.PropertyToID("_SpecColor");

            int colorCount = _discMatricesByColor.Length;

            _discParamsByColor = new RenderParams[colorCount];
            _ballParamsByColor = new RenderParams[colorCount];
            _discBlocks = new MaterialPropertyBlock[colorCount];
            _ballBlocks = new MaterialPropertyBlock[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                _discBlocks[i] = new MaterialPropertyBlock();
                _ballBlocks[i] = new MaterialPropertyBlock();

                _discParamsByColor[i] = new RenderParams(_renderConfig.DiscMaterial) { receiveShadows = true, shadowCastingMode = ShadowCastingMode.On, matProps = _discBlocks[i] };
                _ballParamsByColor[i] = new RenderParams(_renderConfig.BallMaterial) { receiveShadows = true, shadowCastingMode = ShadowCastingMode.On, matProps = _ballBlocks[i] };
            }
            _backgroundParams = new RenderParams(_renderConfig.BackgroundMaterial) { receiveShadows = true, shadowCastingMode = ShadowCastingMode.On };

            RefreshPaletteColors();
        }

        protected override void OnUpdate()
        {
            var layout = SystemAPI.GetSingleton<LevelLayoutComponent>();
            var animation = SystemAPI.GetSingleton<AnimationConfigComponent>();

#if UNITY_EDITOR
            // The palette never changes in a build; re-read it only so it stays tweakable in play mode.
            RefreshPaletteColors();
#endif
            BuildLayoutMatrices();

            _stickBodyMatrices.Clear();
            _stickBaseMatrices.Clear();
            for (int i = 0; i < _discMatricesByColor.Length; i++)
            {
                _discMatricesByColor[i].Clear();
                _ballMatricesByColor[i].Clear();
            }

            for (int i = 0; i < _pieceMatrices.Length; i++)
            {
                _pieceMatrices[i].Clear();
            }


            float discHeight = layout.DiscStackSpacing;

            // Body length comes from game data, so the mesh's own size and pivot must be undone.
            var bodyBounds = _renderConfig.StickBodyMesh.bounds;
            float bodyMeshHeight = bodyBounds.size.y;
            float bodyMeshCenter = bodyBounds.center.y;
            float baseMeshBottom = _renderConfig.StickBaseMesh.bounds.min.y * _renderConfig.StickBaseScale.y;

            foreach (var (stick, discs, stickAnimation) in
                     SystemAPI.Query<RefRO<Stick>, DynamicBuffer<DiscElement>, RefRO<StickAnimationComponent>>())
            {
                float3 basePosition = stick.ValueRO.Position;

                float dipT = math.saturate(stickAnimation.ValueRO.DipElapsed / animation.StickDip.Duration);
                float dipPhase = dipT < 0.5f ? dipT * 2f : 2f - dipT * 2f;
                float dip = -animation.StickDip.Evaluate(dipPhase) * animation.StickDipAmount;

                float shiftT = math.saturate(stickAnimation.ValueRO.ShiftElapsed / animation.DiscStackShift.Duration);
                float shift = -discHeight * (1f - animation.DiscStackShift.Evaluate(shiftT));

                float3 stackOffset = new float3(0f, dip + shift, 0f);

                float bodyLength = math.max(0f, stick.ValueRO.Height - discHeight);

                // Spans exactly [base, base + bodyLength] whatever the mesh pivot is.
                float bodyScaleY = bodyLength / bodyMeshHeight;
                float bodyCenterY = bodyLength * 0.5f - bodyMeshCenter * bodyScaleY;

                _stickBodyMatrices.Add(Matrix4x4.TRS(
                    basePosition + new float3(0f, bodyCenterY, 0f),
                    Quaternion.identity,
                    new Vector3(
                        _renderConfig.StickBodyScale.x,
                        bodyScaleY,
                        _renderConfig.StickBodyScale.z)));

                if (discs.Length == 0)
                {
                    _stickBaseMatrices.Add(Matrix4x4.TRS(
                        basePosition + new float3(0f, bodyLength - baseMeshBottom, 0f) + stackOffset,
                        Quaternion.identity,
                        _renderConfig.StickBaseScale));
                }

                int firstShown = math.max(0, discs.Length - stick.ValueRO.ShownDiscCount);
                for (int slot = firstShown; slot < discs.Length; slot++)
                {
                    _discMatricesByColor[(int)discs[slot].Color].Add(Matrix4x4.TRS(
                        SlotLayoutUtil.DiscPosition(stick.ValueRO, slot, discs.Length, discHeight) + stackOffset,
                        Quaternion.identity,
                        _renderConfig.DiscScale));
                }
            }

            int colorCount = _discMatricesByColor.Length;

            foreach (var pieceRef in SystemAPI.Query<RefRO<DiscPieceComponent>>())
            {
                var piece = pieceRef.ValueRO;
                float shrinkT = math.saturate(piece.Elapsed / animation.DiscPieceScale.Duration);
                float shrink = math.lerp(1f, animation.DiscPieceEndScale, animation.DiscPieceScale.Evaluate(shrinkT));

                float dissolveT = math.saturate((piece.Elapsed - animation.DiscPieceDissolveDelay) / animation.DiscPieceDissolve.Duration);
                float scale = shrink * (1f - animation.DiscPieceDissolve.Evaluate(dissolveT));

                var pieceMesh = _renderConfig.DiscPieceMeshes[piece.MeshIndex];

                _pieceMatrices[piece.MeshIndex * colorCount + (int)piece.Color].Add(
                    Matrix4x4.TRS(piece.Position, Quaternion.Euler(piece.Rotation), _renderConfig.DiscScale * scale)
                    * Matrix4x4.Translate(-pieceMesh.bounds.center));
            }

            float ballMeshBottom = _renderConfig.BallMesh.bounds.min.y;
            foreach (var (ballTransform, ball) in SystemAPI.Query<RefRO<TransformComponent>, RefRO<BallComponent>>())
            {
                var transform = ballTransform.ValueRO;
                Vector3 ballScale = Vector3.Scale(_renderConfig.BallScale, transform.Scale);

                _ballMatricesByColor[(int)ball.ValueRO.Color].Add(Matrix4x4.TRS(
                    transform.Position + new float3(0f, -ballMeshBottom * ballScale.y, 0f),
                    Quaternion.Euler(transform.Rotation),
                    ballScale));
            }

            // Single instance, so no batching needed.
            Graphics.RenderMesh(
                _backgroundParams,
                _renderConfig.BackgroundMesh,
                0,
                Matrix4x4.TRS(
                    _renderConfig.BackgroundPosition,
                    Quaternion.Euler(_renderConfig.BackgroundEulerAngles),
                    _renderConfig.BackgroundScale));

            Draw(_holeParams, _renderConfig.HoleMesh, _holeMatrices);
            Draw(_dockParams, _renderConfig.DockMesh, _dockMatrices);
            Draw(_stickBodyParams, _renderConfig.StickBodyMesh, _stickBodyMatrices);
            Draw(_stickBaseParams, _renderConfig.StickBaseMesh, _stickBaseMatrices);
            for (int i = 0; i < _discMatricesByColor.Length; i++)
            {
                Draw(_discParamsByColor[i], _renderConfig.DiscMesh, _discMatricesByColor[i]);
                Draw(_ballParamsByColor[i], _renderConfig.BallMesh, _ballMatricesByColor[i]);
            }

            for (int mesh = 0; mesh < _renderConfig.DiscPieceMeshes.Length; mesh++)
            {
                for (int color = 0; color < colorCount; color++)
                {
                    Draw(_discParamsByColor[color], _renderConfig.DiscPieceMeshes[mesh],
                        _pieceMatrices[mesh * colorCount + color]);
                }
            }
        }
        
        private void RefreshPaletteColors()
        {
            var palette = _renderConfig.Palette;

            for (int i = 0; i < _discBlocks.Length; i++)
            {
                var color = (DiscColorType)i;

                var discEntry = palette.GetDisc(color);
                _discBlocks[i].SetColor(_baseColorId, discEntry.BaseColor);
                _discBlocks[i].SetColor(_shadeColorId, discEntry.ShadeColor);
                _discBlocks[i].SetColor(_specColorId, discEntry.SpecularColor);

                var ballEntry = palette.GetBall(color);
                _ballBlocks[i].SetColor(_baseColorId, ballEntry.BaseColor);
                _ballBlocks[i].SetColor(_shadeColorId, ballEntry.ShadeColor);
                _ballBlocks[i].SetColor(_specColorId, ballEntry.SpecularColor);
            }
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
