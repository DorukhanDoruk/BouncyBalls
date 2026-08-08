using Runtime.Configs.Model;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Components
{
    public struct DyingDiscComponent : IComponentData
    {
        public DiscColorType Color;
        public float3 Position;
        public float Elapsed;
        public float Duration;
    }
}
