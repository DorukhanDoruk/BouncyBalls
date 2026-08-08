using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Configs;
using Runtime.Utility;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Core
{
    public class LevelAuthoring : MonoBehaviour
    {
        public LevelConfigSO LevelConfigSo;
        public LevelConstantsSO LevelConstantsSo;
        public BallConfigSO BallConfigSo;
        public RenderConfigSO RenderConfigSo;

        private void OnDrawGizmos()
        {
            var stickCenter = StickLayoutUtil.GetCenterXZ(LevelConfigSo.Sticks);

            float stackSpacing = LevelConstantsSo.DiscStackSpacing;

            foreach (var stickDef in LevelConfigSo.Sticks)
            {
                var stickPosition = StickLayoutUtil.Position(stickDef.Position, stickCenter, LevelConstantsSo.StickOrigin);
                var shownCount = stickDef.ShownDiscCount;
                var stickHeight = shownCount * stackSpacing;

                Gizmos.color = Color.gray;
                Gizmos.DrawWireMesh(
                    RenderConfigSo.HoleMesh,
                    stickPosition + Vector3.up * RenderConfigSo.GroundOffset,
                    Quaternion.identity,
                    RenderConfigSo.HoleScale);

                Gizmos.DrawWireMesh(
                    RenderConfigSo.StickBodyMesh,
                    stickPosition + Vector3.up * (stickHeight * 0.5f),
                    Quaternion.identity,
                    new Vector3(
                        RenderConfigSo.StickBodyScale.x,
                        RenderConfigSo.StickBodyScale.y * stickHeight,
                        RenderConfigSo.StickBodyScale.z));

                if (shownCount == 0)
                {
                    Gizmos.DrawWireMesh(
                        RenderConfigSo.StickBaseMesh,
                        stickPosition + Vector3.up * (stackSpacing * 0.5f),
                        Quaternion.identity,
                        RenderConfigSo.StickBaseScale);
                }

                var discs = stickDef.Discs;
                var firstShown = discs.Length - shownCount;

                for (int slot = 0; slot < shownCount; slot++)
                {
                    Gizmos.color = LevelColorUtility.GetColorOfDiskByDiscColorType_Unsafe(discs[firstShown + slot]);
                    Gizmos.DrawWireMesh(
                        RenderConfigSo.DiscMesh,
                        stickPosition + Vector3.up * (stackSpacing * (slot + 0.5f)),
                        Quaternion.identity,
                        RenderConfigSo.DiscScale);
                }
            }

            const float arrowSize = 0.25f;

            Gizmos.color = Color.white;
            var pathOrder = LevelConfigSo.PathOrder;
            for (int i = 0; i < pathOrder.Length - 1; i++)
            {
                int fromStick = pathOrder[i];
                int toStick = pathOrder[i + 1];

                if (fromStick < 0 || fromStick >= LevelConfigSo.Sticks.Length)
                {
                    continue;
                }
                if (toStick < 0 || toStick >= LevelConfigSo.Sticks.Length)
                {
                    continue;
                }

                float3 startPos = StickLayoutUtil.Position(LevelConfigSo.Sticks[fromStick].Position, stickCenter, LevelConstantsSo.StickOrigin);
                float3 endPos = StickLayoutUtil.Position(LevelConfigSo.Sticks[toStick].Position, stickCenter, LevelConstantsSo.StickOrigin);

                float3 delta = endPos - startPos;
                float dist = math.length(delta);
                if (dist < float.Epsilon)
                {
                    continue;
                }

                float3 dir = delta / dist;
                float3 midPos = startPos + dir * (dist * 0.5f);

                float3 side = math.cross(dir, math.up());
                if (math.lengthsq(side) < float.Epsilon)
                {
                    side = math.cross(dir, math.right());
                }
                side = math.normalize(side);

                float3 tail = midPos - dir * arrowSize;

                Gizmos.DrawLine(startPos, endPos);
                Gizmos.DrawLine(midPos, tail + side * (arrowSize * 0.5f));
                Gizmos.DrawLine(midPos, tail - side * (arrowSize * 0.5f));
            }

            DrawSlotGizmos();
        }

        private void DrawSlotGizmos()
        {
            var layout = new LevelLayoutComponent
            {
                GridOrigin = LevelConstantsSo.GridOrigin,
                GridColumnSpacing = LevelConstantsSo.GridColumnSpacing,
                GridRowSpacing = LevelConstantsSo.GridRowSpacing,
                DockOrigin = LevelConstantsSo.DockOrigin,
                DockSlotSpacing = LevelConstantsSo.DockSlotSpacing,
            };

            var columns = LevelConfigSo.GridColumns;

            for (int c = 0; c < columns.Length; c++)
            {
                var balls = columns[c].Balls;
                for (int b = 0; b < balls.Length; b++)
                {
                    Gizmos.color = LevelColorUtility.GetColorOfDiskByDiscColorType_Unsafe(balls[b].Color);
                    Gizmos.DrawWireMesh(
                        RenderConfigSo.BallMesh,
                        SlotLayoutUtil.GridPosition(layout, c, columns.Length, b),
                        Quaternion.identity,
                        RenderConfigSo.BallScale);
                }
            }

            Gizmos.color = Color.gray;
            for (int slot = 0; slot < BallConfigSo.MaxDockBalls; slot++)
            {
                float3 position = SlotLayoutUtil.DockPosition(layout, slot, BallConfigSo.MaxDockBalls);
                position.y += RenderConfigSo.GroundOffset;

                Gizmos.DrawWireMesh(RenderConfigSo.DockMesh, position, Quaternion.identity, RenderConfigSo.DockScale);
            }
        }

        public class LevelBaker : Baker<LevelAuthoring>
        {
            public override void Bake(LevelAuthoring authoring)
            {
                DependsOn(authoring.LevelConfigSo);
                DependsOn(authoring.LevelConstantsSo);

                var config = authoring.LevelConfigSo;
                var constants = authoring.LevelConstantsSo;
                var rootEntity = GetEntity(TransformUsageFlags.None);

                var layout = new LevelLayoutComponent
                {
                    GridOrigin = constants.GridOrigin, 
                    GridColumnSpacing = constants.GridColumnSpacing,
                    GridRowSpacing = constants.GridRowSpacing, 
                    DockOrigin = constants.DockOrigin,
                    DockSlotSpacing = constants.DockSlotSpacing, 
                    BallSelectionRadius = constants.BallSelectionRadius,
                    DiscStackSpacing = constants.DiscStackSpacing, 
                    CameraPadding = constants.CameraPadding,
                    CameraDistance = constants.CameraDistance,
                };

                AddComponent(rootEntity, layout);
                AddComponent(rootEntity, new LaunchRequestComponent { Ball = Entity.Null, Locked = false });
                AddComponent(rootEntity, new GameStateComponent { GameState = GameState.Playing });
                AddComponent(rootEntity, new LoopModeComponent { IsActive = false });
                AddComponent(rootEntity, new LaunchStateComponent { LastLaunchTime = float.NegativeInfinity });
                AddBuffer<DockBallElement>(rootEntity);

                var pathBuffer = AddBuffer<PathElement>(rootEntity);
                foreach (int stickIndex in config.PathOrder)
                {
                    pathBuffer.Add(new PathElement { StickIndex = stickIndex });
                }

                var stickCenter = StickLayoutUtil.GetCenterXZ(config.Sticks);

                var stickRefs = AddBuffer<StickRefElement>(rootEntity);
                for (int i = 0; i < config.Sticks.Length; i++)
                {
                    var stickDef = config.Sticks[i];
                    var stickEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, $"Stick_{i}");
                    stickRefs.Add(new StickRefElement { Entity = stickEntity });

                    AddComponent(stickEntity, new Stick
                    {
                        Index = i,
                        Position = StickLayoutUtil.Position(stickDef.Position, stickCenter, constants.StickOrigin),
                        Height = stickDef.ShownDiscCount * constants.DiscStackSpacing,
                    });

                    var discs = stickDef.Discs;
                    var firstShown = discs.Length - stickDef.ShownDiscCount;

                    var discBuffer = AddBuffer<DiscElement>(stickEntity);
                    for (int j = firstShown; j < discs.Length; j++)
                    {
                        discBuffer.Add(new DiscElement { Color = discs[j] });
                    }
                }

                var columnRefs = AddBuffer<GridColumnRefElement>(rootEntity);
                for (int c = 0; c < config.GridColumns.Length; c++)
                {
                    var columnDef = config.GridColumns[c];
                    var columnEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, $"GridColumn_{c}");
                    columnRefs.Add(new GridColumnRefElement { Entity = columnEntity });

                    AddComponent(columnEntity, new GridColumn { Index = c });

                    var ballQueue = AddBuffer<GridBallElement>(columnEntity);
                    for (int b = 0; b < columnDef.Balls.Length; b++)
                    {
                        var ballDef = columnDef.Balls[b];
                        var ballEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, $"GridBall_{c}_{b}");

                        AddComponent(ballEntity, new BallComponent
                        {
                            Color = ballDef.Color, Remaining = ballDef.Count,
                        });

                        AddComponent(ballEntity, new TransformComponent
                        {
                            Position = SlotLayoutUtil.GridPosition(layout, c, config.GridColumns.Length, b), Rotation = float3.zero,
                            Scale = new float3(1f, 1f, 1f),
                        });

                        ballQueue.Add(new GridBallElement { Entity = ballEntity });
                    }
                }
            }
        }
    }
}
