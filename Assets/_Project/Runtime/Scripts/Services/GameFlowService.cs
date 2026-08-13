using Runtime.Core;
using Runtime.Simulation;
namespace Runtime.Services
{
    public class GameFlowService : IService, ITickable
    {
        private readonly GameConfig _gameConfig;
        private readonly LevelLayout _levelLayout;
        private readonly LevelService _levelService;

        public GameSimulation Simulation { get; private set; }
        
        public GameFlowService(GameConfig gameConfig, LevelLayout levelLayout, LevelService levelService)
        {
            _gameConfig = gameConfig;
            _levelLayout = levelLayout;
            _levelService = levelService;
        }

        public void Initialize()
        {
            Simulation = new GameSimulation(_levelService.Current, _gameConfig, _levelLayout);
        }

        public void Dispose()
        {
            
        }

        public void Tick(float deltaTime)
        {
            Simulation.Tick(deltaTime);
        }
    }
}
