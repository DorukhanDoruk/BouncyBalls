using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Ball Config", fileName = "BallConfig")]
    public class BallConfigSO : ScriptableObject
    {
        public float JumpSpeed = 10f;
        public float ArchHeightPerUnit = 1f;
        public float MaxArchHeight = 3f;

        public float InPlaceBounceHeight = 1f;
        public float InPlaceBounceTime = 0.3f;

        public float MinLaunchInterval = 0.15f;
        public float MaxStretch = 1.35f;
        public float MinSquash = 0.7f;

        public byte MaxActiveBalls = 5;
        public byte MaxDockBalls = 5;
    }
}
