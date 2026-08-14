using System;

namespace Runtime.Config
{
    [Serializable]
    public struct DiscShatterSettings
    {
        public float Gravity;
        public float OutwardSpeed;
        public float UpwardSpeed;
        public float SpinSpeed;

        public float EndScale;
        public float ShrinkDuration;

        public float DissolveDelay;
        public float DissolveDuration;

        public float Lifetime => DissolveDelay + DissolveDuration;
    }
}
