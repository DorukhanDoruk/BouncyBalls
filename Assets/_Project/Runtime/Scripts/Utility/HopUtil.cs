using Runtime.Components;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
namespace Runtime.Utility
{
    public static class HopUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HopState BeginHop(int fromPathIndex, float3 fromPosition, int toPathIndex, float3 toPosition,
            in BallConfigComponent config, float speedMultiplier)
        {
            float distance = math.distance(fromPosition.xz, toPosition.xz);
            bool inPlace = distance < float.Epsilon;

            return new HopState
            {
                FromPathIndex = fromPathIndex, 
                ToPathIndex = toPathIndex,
                FromPosition = fromPosition,
                ToPosition = toPosition,
                Elapsed = 0f,
                Duration = (inPlace ? config.InPlaceBounceTime : distance / config.HopSpeed) / speedMultiplier,
                ArcHeight = inPlace ? config.InPlaceBounceHeight : math.min(distance * config.ArchHeightPerUnit, config.MaxArchHeight),
            };
        }
    }
}
