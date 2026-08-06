using Runtime.Components;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
namespace Runtime.Utility
{
    public static class SlotLayoutUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GridPosition(in LevelLayoutComponent layout, int column, int columnCount, int queueIndex)
        {
            float offsetX = (column - (columnCount - 1) * 0.5f) * layout.GridColumnSpacing;
            return layout.GridOrigin + new float3(offsetX, 0f, -queueIndex * layout.GridRowSpacing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 DockPosition(in LevelLayoutComponent layout, int slotIndex, int slotCount)
        {
            float offsetX = (slotIndex - (slotCount - 1) * 0.5f) * layout.DockSlotSpacing;
            return layout.DockOrigin + new float3(offsetX, 0f, 0f);
        }
    }
}
