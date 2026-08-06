using Runtime.Configs;
using Runtime.Utility;
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
                var stickSize = LevelConstantsSo.StickSize;
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(stickDef.Position + (Vector3.up * LevelConstantsSo.StickSize.y) / 2f, stickSize);

                var stickDefDiscs = stickDef.Discs;
                for (int index = 0; index < stickDefDiscs.Length; index++)
                {
                    var discColor = stickDefDiscs[index];
                    var discSize = LevelConstantsSo.DiscSize;
                    Gizmos.color = LevelColorUtility.GetColorOfDiskByDiscColorType_Unsafe(discColor);
                    Gizmos.DrawWireCube((stickDef.Position + (((Vector3.up * LevelConstantsSo.DiscSize.y) * (index + 1)))) - (Vector3.up * (LevelConstantsSo.DiscSize.y / 2f)), discSize);
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
    }
}
