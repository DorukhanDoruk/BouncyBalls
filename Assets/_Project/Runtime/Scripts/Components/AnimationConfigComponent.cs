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
        public Tween DiscBreakPop;
    }
}
