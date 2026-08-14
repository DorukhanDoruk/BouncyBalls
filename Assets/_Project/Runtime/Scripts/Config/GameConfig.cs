using System;
namespace Runtime.Config
{

    [Serializable]
    public struct GameConfig
    {
        public float HopDuration;
        public float ArcHeightPerUnit;
        public float MaxArcHeight;
        public float InPlaceBounceHeight;
        public float InPlaceBounceDuration;
        public int MaxActiveBalls;
        public int DockCapacity;
        public float MinLaunchInterval;
        public float DiscHeight;
        public float BallRadius;

        public static GameConfig Default => new GameConfig
        {
            HopDuration = 0.45f, ArcHeightPerUnit = 1f,
            MaxArcHeight = 3f, InPlaceBounceHeight = 1f,
            InPlaceBounceDuration = 0.3f, MaxActiveBalls = 5,
            DockCapacity = 5, MinLaunchInterval = 0.15f,
            DiscHeight = 0.5f, BallRadius = 0.4f
        };
    }

}
