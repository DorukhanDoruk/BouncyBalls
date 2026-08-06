using Runtime.Configs.Model;
using UnityEngine;
namespace Runtime.Utility
{
    public static class StickLayoutUtil
    {
        public static Vector3 GetCenterXZ(StickDef[] sticks)
        {
            if (sticks.Length == 0)
            {
                return Vector3.zero;
            }

            float minX = sticks[0].Position.x;
            float maxX = minX;
            float minZ = sticks[0].Position.z;
            float maxZ = minZ;

            for (int i = 1; i < sticks.Length; i++)
            {
                var position = sticks[i].Position;

                if (position.x < minX)
                {
                    minX = position.x;
                }
                if (position.x > maxX)
                {
                    maxX = position.x;
                }
                if (position.z < minZ)
                {
                    minZ = position.z;
                }
                if (position.z > maxZ)
                {
                    maxZ = position.z;
                }
            }

            return new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
        }

        public static Vector3 Position(Vector3 authoredPosition, Vector3 centerXZ, Vector3 stickOrigin)
        {
            return authoredPosition - centerXZ + stickOrigin;
        }
    }
}
