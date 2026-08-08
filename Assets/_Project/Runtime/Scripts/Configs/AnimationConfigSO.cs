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
        public TweenDef DiscStackShift = TweenDef.Default;

        [Header("Disc Shatter")]
        public float DiscPieceGravity = 18f;
        public float DiscPieceOutwardSpeed = 2.5f;
        public float DiscPieceUpwardSpeed = 3f;
        public float DiscPieceSpinSpeed = 540f;
        public float DiscPieceEndScale = 0.6f;
        public TweenDef DiscPieceScale = TweenDef.Default;
        public float DiscPieceDissolveDelay = 0.6f;
        public TweenDef DiscPieceDissolve = TweenDef.Default;

        [Header("Stick")]
        public TweenDef StickDip = TweenDef.Default;
        public float StickDipAmount = 0.15f;
    }
}
