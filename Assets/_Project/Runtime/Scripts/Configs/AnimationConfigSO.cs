using Runtime.Configs.Model;
using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Animation Config", fileName = "AnimationConfig")]
    public class AnimationConfigSO : ScriptableObject
    {
        [Header("Squash & Stretch")]
        public TweenDef HopStretch = TweenDef.Default;
        public TweenDef LandSquash = TweenDef.Default;

        [Header("Slots")]
        public TweenDef GridColumnAdvance = TweenDef.Default;
        public TweenDef DockSlotSettle = TweenDef.Default;

        [Header("Disc")]
        public TweenDef DiscBreakPop = TweenDef.Default;
    }
}
