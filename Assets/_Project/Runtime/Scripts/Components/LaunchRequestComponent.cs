using Unity.Entities;
namespace Runtime.Components
{
    public struct LaunchRequestComponent : IComponentData
    {
        public Entity Ball;
        public bool Locked;
    }
}
