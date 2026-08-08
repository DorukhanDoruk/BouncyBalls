using Runtime.Components;
using Runtime.Components.Model;
using Runtime.Core;
using Unity.Entities;
namespace Runtime.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class GameHudSystem : SystemBase
    {
        private UiService _ui;
        private EntityQuery _activeBallQuery;

        protected override void OnCreate()
        {
            RequireForUpdate<ManagedServicesComponent>();
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<BallConfigComponent>();

            _activeBallQuery = SystemAPI.QueryBuilder().WithAll<HopState, BallComponent>().Build();
        }

        protected override void OnStartRunning()
        {
            _ui = SystemAPI.ManagedAPI.GetSingleton<ManagedServicesComponent>().Container.Resolve<UiService>();
            _ui.ShowHud();
        }

        protected override void OnUpdate()
        {
            var config = SystemAPI.GetSingleton<BallConfigComponent>();
            int claimed = SystemAPI.GetSingletonBuffer<DockBallElement>().Length + _activeBallQuery.CalculateEntityCount();

            _ui.SetDockCount(claimed, config.MaxDockBalls);

            var state = SystemAPI.GetSingleton<GameStateComponent>().GameState;
            if (state != GameState.Playing)
            {
                _ui.ShowResult(state == GameState.Won);
            }
        }
    }
}
