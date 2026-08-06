using Runtime.Components;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
namespace Runtime.Utility
{
    public static class SlotLayoutUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GridPosition(in LevelLayoutComponent layout, int column, int queueIndex)
        {
            return layout.GridOrigin + new float3(column * layout.GridColumnSpacing, 0f, -queueIndex * layout.GridRowSpacing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 DockPosition(in LevelLayoutComponent layout, int slotIndex)
        {
            return layout.DockOrigin + new float3(slotIndex * layout.DockSlotSpacing, 0f, 0f);
        }
    }
}
