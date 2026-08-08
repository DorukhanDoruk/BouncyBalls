using Unity.Entities;
namespace Runtime.Components
{
    public struct BallConfigComponent : IComponentData
    {
        public float HopSpeed;
        public float LaunchSpeedMultiplier;
        public float DockReturnSpeedMultiplier;
        public float ArchHeightPerUnit;
        public float MaxArchHeight;

        public float InPlaceBounceHeight;
        public float InPlaceBounceTime;

        public float MinLaunchInterval;
        public float MaxStretch;
        public float MinSquash;

        public byte MaxActiveBalls;
        public byte MaxDockBalls;

        public float LoopModeSpeedMultiplier;
    }
}
