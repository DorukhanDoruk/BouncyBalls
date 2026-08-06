using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Components
{
    public struct HopState : IComponentData
    {
        public int FromPathIndex;
        public int ToPathIndex;
        public float3 FromPosition;
        public float3 ToPosition;
        public float Elapsed;
        public float Duration;
        public float ArcHeight;
    }
}
