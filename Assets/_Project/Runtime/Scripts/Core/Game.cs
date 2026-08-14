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
        public static Game Instance { get; private set; }
        public static bool IsReady => Instance != null && Instance._ready;

        [Header("Config")]
        [SerializeField] private GameConfigSO   _gameConfig;
        [SerializeField] private VisualConfigSO _visualConfig;
        [SerializeField] private LevelLayoutSO _levelLayout;

        [Header("Levels")]
        [SerializeField] private LevelData[] _levels;

        [Header("Scene References")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private GameView _gameView;
        [SerializeField] private UiRootView _uiRootPrefab;

        private ServiceContainer _container;
        private bool _ready;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); 
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

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
            if (Instance != this)
            {
                return;
            }
            _container?.Dispose();
            _container = null;
            Instance = null;
            _ready = false;
        }

        private void Bootstrap()
        {
            _container = new ServiceContainer();
            Debug.Log("[Game] Bootstrap started");

            var gameConfig = _gameConfig.ToRuntime();
            var levelLayout = _levelLayout.ToRuntime();
            
            var levelService = new LevelService(_levels, 1);
            _container.Register(levelService);

            var flowService = new GameFlowService(gameConfig, levelLayout, levelService);
            _container.Register(flowService);

            var inputService = new InputService(_mainCamera, flowService, levelLayout.BallSelectionRadius);
            _container.Register(inputService);

            var uiPresenter = new UIPresenter(_uiRootPrefab, flowService, levelService, _mainCamera, gameConfig);
            _container.Register(uiPresenter);

            _container.InitializeAll();

            _gameView.Initialize(_visualConfig, gameConfig, levelLayout);

            flowService.LevelStarted += _gameView.Rebuild;
            uiPresenter.PlayAgainRequested += flowService.Continue;

            flowService.StartLevel();
            _ready = true;

            Debug.Log("[Game] Bootstrap completed");
        }

        public static T Get<T>() where T : class, IService
        {
            if (!IsReady)
            {
                throw new System.InvalidOperationException("Game is not ready yet, check the execution of invocation");
            }

            return Instance._container.Resolve<T>();
        }
    }
}
