using System;
namespace Runtime.Core
{

    [Serializable]
    public struct GameConfig
    {
        public float HopSpeed;
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
            HopSpeed = 12f, ArcHeightPerUnit = 0.35f,
            MaxArcHeight = 3f, InPlaceBounceHeight = 1.2f,
            InPlaceBounceDuration = 0.3f, MaxActiveBalls = 5,
            DockCapacity = 5, MinLaunchInterval = 0.15f,
            DiscHeight = 0.25f, BallRadius = 0.4f
        };
    }

}
