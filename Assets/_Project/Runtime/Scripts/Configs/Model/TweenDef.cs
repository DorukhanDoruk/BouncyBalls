using System;
namespace Runtime.Configs.Model
{
    [Serializable]
    public struct TweenDef
    {
        public float Duration;
        public EaseType EaseType;

        public static TweenDef Default = new TweenDef { Duration = 0.25f, EaseType = EaseType.Linear };
    }
}
