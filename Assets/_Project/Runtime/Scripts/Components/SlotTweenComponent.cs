using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Components
{
    public struct SlotTweenComponent : IComponentData
    {
        public float3 From;
        public float3 To;
        public float Elapsed;
        public float Duration;
    }
}
