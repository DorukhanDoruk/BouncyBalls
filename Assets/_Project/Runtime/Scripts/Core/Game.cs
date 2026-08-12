using Runtime.Level.Config;
using UnityEngine;
namespace Runtime.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }
        public static bool IsReady => Instance != null && Instance._ready;

        [Header("Config")]
        [SerializeField] GameConfigSO   _gameConfig;
        [SerializeField] VisualConfigSO _visualConfig;
        [SerializeField] LevelLayoutSO _levelLayout;

        [Header("Levels")]
        [SerializeField] LevelData[] _levels;

        [Header("Scene References")]
        [SerializeField] Camera _mainCamera;

        ServiceContainer _container;
        bool _ready;

        void Awake()
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

        void Update()
        {
            if (_ready)
            {
                _container.Tick(Time.deltaTime);
            }
        }

        void OnDestroy()
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

        void Bootstrap()
        {
            _container = new ServiceContainer();
            Debug.Log("[Game] Bootstrap started");

            _container.InitializeAll();
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
