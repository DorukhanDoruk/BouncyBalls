using System;
using UnityEngine;
using Unity.Entities;

namespace Runtime.Core
{
    public sealed class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }
        public static bool IsReady => Instance != null && Instance._ready;
        
        //[Header("Config Assets")]

        private ServiceContainer _container;
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
            
            // Levels
            // Camera
            // Flow
            // UI
            
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
            var entity = manager.CreateEntity();
            var componentData = new ManagedServices
            {
                Container = _container
            };
            manager.AddComponentObject(entity, componentData);
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
