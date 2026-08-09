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

        private void OnDrawGizmos()
        {
            const float arrowSize = 0.25f;

            var sticks = LevelConfigSo.Sticks;
            var pathOrder = LevelConfigSo.PathOrder;

            Gizmos.color = Color.white;

            for (int i = 0; i < pathOrder.Length - 1; i++)
            {
                int fromStick = pathOrder[i];
                int toStick = pathOrder[i + 1];

                if (fromStick < 0 || fromStick >= sticks.Length)
                {
                    continue;
                }
                if (toStick < 0 || toStick >= sticks.Length)
                {
                    continue;
                }

                float3 startPos = sticks[fromStick].Position + LevelConstantsSo.StickOrigin;
                float3 endPos = sticks[toStick].Position + LevelConstantsSo.StickOrigin;

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

                var stickRefs = AddBuffer<StickRefElement>(rootEntity);
                for (int i = 0; i < config.Sticks.Length; i++)
                {
                    var stickDef = config.Sticks[i];
                    var stickEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, $"Stick_{i}");
                    stickRefs.Add(new StickRefElement { Entity = stickEntity });

                    AddComponent(stickEntity, new Stick
                    {
                        Position = stickDef.Position + constants.StickOrigin,
                        Height = stickDef.ShownDiscCount * constants.DiscStackSpacing,
                        ShownDiscCount = stickDef.ShownDiscCount,
                    });

                    // Both start finished so no stick animates on the first frame.
                    AddComponent(stickEntity, new StickAnimationComponent
                    {
                        DipElapsed = float.MaxValue, ShiftElapsed = float.MaxValue,
                    });

                    var discs = stickDef.Discs;
                    var discBuffer = AddBuffer<DiscElement>(stickEntity);
                    for (int j = 0; j < discs.Length; j++)
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

                        var slotPosition = SlotLayoutUtil.GridPosition(layout, c, config.GridColumns.Length, b);
                        AddComponent(ballEntity, new SlotTweenComponent
                        {
                            From = slotPosition, To = slotPosition, Elapsed = 1f, Duration = 1f,
                        });

                        ballQueue.Add(new GridBallElement { Entity = ballEntity });
                    }
                }
            }
        }
    }
}
