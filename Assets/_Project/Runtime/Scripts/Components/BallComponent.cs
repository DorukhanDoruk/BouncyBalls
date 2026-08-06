using Runtime.Configs.Model;
using Unity.Entities;
namespace Runtime.Components
{
    public struct BallComponent : IComponentData
    {
        public DiscColorType Color;
        public int Remaining;
    }
}
