using Runtime.Configs.Model;
using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Animation Config", fileName = "AnimationConfig")]
    public class AnimationConfigSO : ScriptableObject
    {
        [Header("Hop")]
        public TweenDef HopArc = new TweenDef
        {
            Curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f)), SampleCount = 48,
        };

        [Header("Squash & Stratch")]
        public TweenDef HopStretch = TweenDef.Default;
        public TweenDef LandSquash = TweenDef.Default;
    }
}
