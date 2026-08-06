using Runtime.Components.Model;
using System.Runtime.CompilerServices;
using Unity.Entities;
namespace Runtime.Utility
{
    public static class PathUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ResolveNext(int currentPathIndex, in DynamicBuffer<PathElement> path, in DynamicBuffer<StickRefElement> stickRefs,
            in BufferLookup<DiscElement> discLookup)
        {
            for (int step = 1; step <= path.Length; step++)
            {
                int candidate = (currentPathIndex + step) % path.Length;
                var stickEntity = stickRefs[path[candidate].StickIndex].Value;

                if (discLookup[stickEntity].Length > 0)
                {
                    return candidate;
                }
            }

            return -1;
        }
    }
}
