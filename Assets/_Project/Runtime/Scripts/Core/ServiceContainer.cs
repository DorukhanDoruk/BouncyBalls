using System;
using System.Collections.Generic;
namespace Runtime.Core
{
    public interface IService
    {
        void Initialize();

        void Dispose();
    }

    public interface ITickable
    {
        void Tick(float deltaTime);
    }

    public sealed class ServiceContainer : IDisposable
    {
        readonly Dictionary<Type, IService> _byType = new();
        readonly List<IService> _initOrder = new();
        readonly List<ITickable> _tickables = new();
        bool _initialized;

        public T Register<T>(T service) where T : class, IService
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            if (_initialized)
            {
                throw new InvalidOperationException("You cannto register new services after initialize.");
            }

            var key = typeof(T);
            if (_byType.ContainsKey(key))
            {
                throw new InvalidOperationException($"{key.Name} already exists.");
            }

            _byType[key] = service;
            _initOrder.Add(service);
            if (service is ITickable tickable)
            {
                _tickables.Add(tickable);
            }
            return service;
        }

        public T Resolve<T>() where T : class, IService
        {
            if (_byType.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"{typeof(T).Name} not exist. Check the registration order at Game.Bootstrap .");
        }

        public void InitializeAll()
        {
            for (int i = 0; i < _initOrder.Count; i++)
            {
                _initOrder[i].Initialize();
            }
            _initialized = true;
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _tickables.Count; i++)
            {
                _tickables[i].Tick(deltaTime);
            }
        }

        public void Dispose()
        {
            for (int i = _initOrder.Count - 1; i >= 0; i--)
            {
                _initOrder[i].Dispose();
            }

            _initOrder.Clear();
            _tickables.Clear();
            _byType.Clear();
            _initialized = false;
        }
    }
}
