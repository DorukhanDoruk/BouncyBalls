using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Components
{
    public struct LevelLayoutComponent : IComponentData
    {
        public float3 GridOrigin;
        public float GridColumnSpacing;
        public float GridRowSpacing;

        public float3 DockOrigin;
        public float DockSlotSpacing;

        public float BallSelectionRadius;

        public float DiscHeight;
        public float CameraPadding;
        public float CameraDistance;
    }
}
