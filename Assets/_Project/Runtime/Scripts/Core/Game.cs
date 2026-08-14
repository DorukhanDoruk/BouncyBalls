using Runtime.Config;
using Runtime.Level.Config;
using Runtime.Presentation;
using Runtime.Services;
using UnityEngine;
namespace Runtime.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Game : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO   _gameConfig;
        [SerializeField] private VisualConfigSO _visualConfig;
        [SerializeField] private LevelLayoutSO _levelLayout;

        [Header("Levels")]
        [SerializeField] private LevelData[] _levels;

        [Header("Scene References")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameView _gameView;
        [SerializeField] private UiRootView _uiRoot;

        private ServiceContainer _container;
        private GameFlowService _flowService;
        private UIPresenter _uiPresenter;
        private bool _ready;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Bootstrap();
        }

        private void Update()
        {
            if (_ready)
            {
                _container.Tick(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            _ready = false;

            _flowService.LevelStarted -= _gameView.Rebuild;
            _uiPresenter.PlayAgainRequested -= _flowService.Continue;

            _container.Dispose();
            _container = null;
        }

        private void Bootstrap()
        {
            _container = new ServiceContainer();
            Debug.Log("[Game] Bootstrap started");

            var gameConfig = _gameConfig.ToRuntime();
            var levelLayout = _levelLayout.ToRuntime();

            var levelService = new LevelService(_levels, 1);
            _container.Register(levelService);

            _flowService = new GameFlowService(gameConfig, levelLayout, levelService);
            _container.Register(_flowService);

            var inputService = new InputService(_mainCamera, _flowService, levelLayout.BallSelectionRadius);
            _container.Register(inputService);

            _uiPresenter = new UIPresenter(_uiRoot, _flowService, levelService, _mainCamera, gameConfig);
            _container.Register(_uiPresenter);

            _container.InitializeAll();

            _gameView.Initialize(_visualConfig, gameConfig, levelLayout);

            _flowService.LevelStarted += _gameView.Rebuild;
            _uiPresenter.PlayAgainRequested += _flowService.Continue;

            _flowService.StartLevel();
            _ready = true;

            Debug.Log("[Game] Bootstrap completed");
        }
    }
}
