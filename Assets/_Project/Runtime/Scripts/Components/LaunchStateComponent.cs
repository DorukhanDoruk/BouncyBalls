using Unity.Entities;
namespace Runtime.Components
{
    public struct LaunchStateComponent : IComponentData
    {
        public float LastLaunchTime;
    }
}
