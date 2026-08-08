using Unity.Entities;
namespace Runtime.Components
{
    public struct StickAnimationComponent : IComponentData
    {
        public float DipElapsed;
        public float ShiftElapsed;
    }
}
