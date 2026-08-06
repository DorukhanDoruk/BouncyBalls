using Unity.Entities;
namespace Runtime.Components
{
    public struct LapProgressComponent : IComponentData
    {
        public int StepsTaken;
    }
}
