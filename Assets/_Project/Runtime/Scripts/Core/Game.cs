using Runtime.Components;
using System;
using UnityEngine;
using Unity.Entities;

namespace Runtime.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }
        public static bool IsReady => Instance != null && Instance._ready;
        
        [Header("UI")]
        [SerializeField] private UiRootView _uiRootPrefab;

        private ServiceContainer _container;
        private Entity _servicesEntity;
        private bool _ready;

        #region Unity
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
            if (Instance == this)
            {
                DestroyServicesEntity();

                _container?.Dispose();
                _container = null;
                Instance = null;
                _ready = false;
            }
        }
        #endregion Unity
        
        private void Bootstrap()
        {
            _container = new ServiceContainer();
            _container.Register(new UiService(_uiRootPrefab));
            
            _container.InitializeAll();
            _ready = true;

            PublishToEcsWorld();
        }

        private void PublishToEcsWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                Debug.LogError($"[{nameof(Game)}] ECS world is no tready.");
                return;
            }

            var manager = world.EntityManager;
            _servicesEntity = manager.CreateEntity();
            var componentData = new ManagedServicesComponent
            {
                Container = _container
            };
            manager.AddComponentObject(_servicesEntity, componentData);
        }

        // Without this the entity outlives the play session, and a second one would make
        // GetSingleton<ManagedServicesComponent>() throw once domain reload is turned off.
        private void DestroyServicesEntity()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return;
            }

            world.EntityManager.DestroyEntity(_servicesEntity);
        }

        public static T Get<T>() where T : class, IService
        {
            if (!IsReady)
            {
                throw new Exception($"[{nameof(Game)}] game is not ready yet.");
            }

            return Instance._container.Resolve<T>();
        }
    }
}
