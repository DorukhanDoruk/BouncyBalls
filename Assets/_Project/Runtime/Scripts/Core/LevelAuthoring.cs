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
                int toStick   = pathOrder[i + 1];

                if (fromStick < 0 || fromStick >= LevelConfigSo.Sticks.Length)
                {
                    continue;
                }
                if (toStick < 0 || toStick >= LevelConfigSo.Sticks.Length)
                {
                    continue;
                }

                float3 startPos = LevelConfigSo.Sticks[fromStick].Position;
                float3 endPos   = LevelConfigSo.Sticks[toStick].Position;

                float3 delta = endPos - startPos;
                float  dist  = math.length(delta);
                if (dist < float.Epsilon)
                {
                    continue;
                }

                float3 dir    = delta / dist;
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

                var config = authoring.LevelConfigSo;
                var rootEntity = GetEntity(TransformUsageFlags.None);

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
                    stickRefs.Add(new StickRefElement { Value = stickEntity });

                    AddComponent(stickEntity, new Stick
                    {
                        Index    = i,
                        Position = stickDef.Position
                    });

                    var discs = stickDef.Discs;
                    var firstShown = discs.Length - stickDef.ShownDiscCount;

                    var discBuffer = AddBuffer<DiscElement>(stickEntity);
                    for (int j = firstShown; j < discs.Length; j++)
                    {
                        discBuffer.Add(new DiscElement { Color = discs[j] });
                    }
                }
            }
        }
    }
}
