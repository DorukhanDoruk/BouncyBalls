using Runtime.Components.Model;
using System.Runtime.CompilerServices;
using Unity.Entities;
namespace Runtime.Utility
{
    public static class PathUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ResolveNext(int currentPathIndex, in DynamicBuffer<PathElement> path,
            in DynamicBuffer<StickRefElement> stickRefs, in BufferLookup<DiscElement> discLookup)
        {
            return Resolve(currentPathIndex + 1, path, stickRefs, discLookup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ResolveFirst(in DynamicBuffer<PathElement> path,
            in DynamicBuffer<StickRefElement> stickRefs, in BufferLookup<DiscElement> discLookup)
        {
            return Resolve(0, path, stickRefs, discLookup);
        }

        private static int Resolve(int startIndex, in DynamicBuffer<PathElement> path,
            in DynamicBuffer<StickRefElement> stickRefs, in BufferLookup<DiscElement> discLookup)
        {
            for (int step = 0; step < path.Length; step++)
            {
                int candidate = (startIndex + step) % path.Length;
                var stickEntity = stickRefs[path[candidate].StickIndex].Entity;

                if (discLookup[stickEntity].Length > 0)
                {
                    return candidate;
                }
            }

            return -1;
        }
    }
}
