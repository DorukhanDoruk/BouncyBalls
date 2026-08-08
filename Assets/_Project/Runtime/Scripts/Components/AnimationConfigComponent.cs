using Runtime.Configs.Model;
using Unity.Entities;
namespace Runtime.Components
{
    public struct AnimationConfigComponent : IComponentData
    {
        public Tween HopStretch;
        public Tween LandSquash;

        public Tween GridColumnAdvance;
        public Tween DockSlotSettle;

        public Tween DiscStackShift;
        public Tween StickDip;
        public float StickDipAmount;

        public float DiscPieceGravity;
        public float DiscPieceOutwardSpeed;
        public float DiscPieceUpwardSpeed;
        public float DiscPieceSpinSpeed;

        public float DiscPieceEndScale;
        public Tween DiscPieceScale;

        public float DiscPieceDissolveDelay;
        public Tween DiscPieceDissolve;
    }
}
