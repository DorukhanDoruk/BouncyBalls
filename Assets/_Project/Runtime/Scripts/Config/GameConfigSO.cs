using UnityEngine;
namespace Runtime.Core
{
    [CreateAssetMenu(menuName = "BouncyBalls/Game Config", fileName = "GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Movement")]
        public float HopSpeed = 4.5f;
        public float ArcHeightPerUnit = 1f;
        public float MaxArcHeight = 3f;

        [Header("Bounce Inplace")]
        public float InPlaceBounceHeight = 1f;
        public float InPlaceBounceDuration = 0.3f;

        [Header("Limits")]
        public int MaxActiveBalls = 5;
        public int DockCapacity = 5;
        public float MinLaunchInterval = 0.15f;

        [Header("Sizes")]
        public float DiscHeight = 0.5f;
        public float BallRadius = 0.4f;

        public GameConfig ToRuntime() => new GameConfig
        {
            HopSpeed = HopSpeed, ArcHeightPerUnit = ArcHeightPerUnit,
            MaxArcHeight = MaxArcHeight, InPlaceBounceHeight = InPlaceBounceHeight,
            InPlaceBounceDuration = InPlaceBounceDuration, MaxActiveBalls = MaxActiveBalls,
            DockCapacity = DockCapacity, MinLaunchInterval = MinLaunchInterval,
            DiscHeight = DiscHeight, BallRadius = BallRadius
        };
    }
}
