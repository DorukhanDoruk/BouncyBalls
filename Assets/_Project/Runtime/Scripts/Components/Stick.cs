using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Components
{
    public struct Stick : IComponentData
    {
        public int Index;
        public float3 Position;
        public float Height; // ShownDiscCount * DiscStackSpacing
    }
}
