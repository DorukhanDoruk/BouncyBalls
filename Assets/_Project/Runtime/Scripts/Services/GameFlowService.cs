using Runtime.Config;
using System;
using Runtime.Core;
using Runtime.Simulation;

namespace Runtime.Services
{
    public class GameFlowService : IService, ITickable
    {
        private readonly GameConfig _gameConfig;
        private readonly LevelLayout _levelLayout;
        private readonly LevelService _levelService;

        private bool _lastLevelWon;
        public GameSimulation Simulation { get; private set; }
        public event Action<GameSimulation> LevelStarted;

        public GameFlowService(GameConfig gameConfig, LevelLayout levelLayout, LevelService levelService)
        {
            _gameConfig = gameConfig;
            _levelLayout = levelLayout;
            _levelService = levelService;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void StartLevel()
        {
            Simulation = new GameSimulation(_levelService.Current, _gameConfig, _levelLayout);
            Simulation.Events.LevelEnded += OnLevelEnded;

            LevelStarted?.Invoke(Simulation);
        }

        public void Continue()
        {
            if (_lastLevelWon)
            {
                _levelService.Next();
            }

            StartLevel();
        }

        public void Tick(float deltaTime)
        {
            Simulation.Tick(deltaTime);
        }

        private void OnLevelEnded(bool won)
        {
            _lastLevelWon = won;
        }
    }
}
