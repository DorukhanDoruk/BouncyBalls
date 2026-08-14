using Runtime.Config;
using System;
using Runtime.Core;
using Runtime.Presentation;
using Runtime.Simulation;
using Runtime.Simulation.Model;
using UnityEngine;

namespace Runtime.Services
{
    public class UIPresenter : IService, ITickable
    {
        private readonly UiRootView _root;
        private readonly GameFlowService _flowService;
        private readonly LevelService _levelService;
        private readonly GameConfig _gameConfig;
        private readonly BallLabelPresenter _labelPresenter;

        private GameSimulation _simulation;

        public event Action PlayAgainRequested;

        public UIPresenter(UiRootView root, GameFlowService flowService, LevelService levelService,
            Camera camera, GameConfig gameConfig)
        {
            _root = root;
            _flowService = flowService;
            _levelService = levelService;
            _gameConfig = gameConfig;

            _labelPresenter = new BallLabelPresenter(root, camera);
        }

        public void Initialize()
        {
            _root.ResultPanel.gameObject.SetActive(false);
            _root.Backdrop.gameObject.SetActive(false);
            _root.ResultPanel.PlayAgainButton.onClick.AddListener(OnPlayAgainClicked);

            _flowService.LevelStarted += OnLevelStarted;
        }

        public void Dispose()
        {
            _flowService.LevelStarted -= OnLevelStarted;

            _labelPresenter.Unsubscribe();
            Unsubscribe();
        }

        public void Tick(float deltaTime)
        {
            _labelPresenter.Tick();
        }

        // Runs for every level, first one included.
        private void OnLevelStarted(GameSimulation simulation)
        {
            Unsubscribe();

            _simulation = simulation;

            _root.TopArea.SetLevelName(_levelService.Current.name);
            _root.DockCounter.SetCount(_simulation.Flying.Count, _gameConfig.MaxActiveBalls);
            _root.ResultPanel.Hide();
            _root.Backdrop.Hide();

            _labelPresenter.Rebuild(_simulation);

            _simulation.Events.BallLaunched += OnActiveBallsChanged;
            _simulation.Events.BallFinished += OnActiveBallsChanged;
            _simulation.Events.LevelEnded += OnLevelEnded;
        }

        private void Unsubscribe()
        {
            if (_simulation == null)
            {
                return;
            }

            _simulation.Events.BallLaunched -= OnActiveBallsChanged;
            _simulation.Events.BallFinished -= OnActiveBallsChanged;
            _simulation.Events.LevelEnded -= OnLevelEnded;
            _simulation = null;
        }

        private void OnActiveBallsChanged(Ball ball)
        {
            _root.DockCounter.SetCount(_simulation.Flying.Count, _gameConfig.MaxActiveBalls);
        }

        private void OnLevelEnded(bool won)
        {
            _root.Backdrop.Show();
            _root.ResultPanel.Show(won);
        }

        private void OnPlayAgainClicked()
        {
            PlayAgainRequested?.Invoke();
        }
    }
}
