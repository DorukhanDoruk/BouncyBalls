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
            foreach (var stickDef in LevelConfigSo.Sticks)
            {
                var stickHeight = stickDef.ShownDiscCount * LevelConstantsSo.DiscSize.y;
                var stickSize = new Vector3(LevelConstantsSo.StickWidth, stickHeight, LevelConstantsSo.StickWidth);
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(stickDef.Position + (Vector3.up * stickHeight) / 2f, stickSize);

                var discs = stickDef.Discs;
                var shownCount = stickDef.ShownDiscCount;
                var firstShown = discs.Length - shownCount;
                var discHeight = LevelConstantsSo.DiscSize.y;

                for (int slot = 0; slot < shownCount; slot++)
                {
                    var discColor = discs[firstShown + slot];
                    var center = stickDef.Position + Vector3.up * (discHeight * (slot + 0.5f));

                    Gizmos.color = LevelColorUtility.GetColorOfDiskByDiscColorType_Unsafe(discColor);
                    Gizmos.DrawWireCube(center, LevelConstantsSo.DiscSize);
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

                float3 startPos = LevelConfigSo.Sticks[fromStick].Position;
                float3 endPos = LevelConfigSo.Sticks[toStick].Position;

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
                    GridOrigin = constants.GridOrigin, GridColumnSpacing = constants.GridColumnSpacing,
                    GridRowSpacing = constants.GridRowSpacing, DockOrigin = constants.DockOrigin,
                    DockSlotSpacing = constants.DockSlotSpacing,
                    BallSelectionRadius = constants.BallSelectionRadius,
                };

                AddComponent(rootEntity, layout);
                AddComponent(rootEntity, new LaunchRequestComponent { Ball = Entity.Null, Locked = false });
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

                    AddComponent(stickEntity, new Stick { Index = i, Position = stickDef.Position });

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
