using System;
using UnityEngine;
namespace Runtime.Configs.Model
{
    [Serializable]
    public struct TweenDef
    {
        // Only for fixed time tween.
        public float Duration;
        public EaseType EaseType;
        public AnimationCurve Curve;
        public int SampleCount;

        public static TweenDef Default = new TweenDef
        {
            Duration = 0.25f, EaseType = EaseType.Linear,
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f), SampleCount = 32,
        };
    }
}
