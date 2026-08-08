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
        private Dictionary<Type, IService> _servicesByType = new Dictionary<Type, IService>();
        private List<IService> _initializeOrder = new List<IService>();
        private List<ITickable> _tickableList = new List<ITickable>();
        private bool _initialized;
        
        public void Dispose()
        {
            for (int i = _initializeOrder.Count - 1; i >= 0; i--)
                _initializeOrder[i].Dispose();
 
            _initializeOrder.Clear();
            _tickableList.Clear();
            _servicesByType.Clear();
            _initialized = false;
        }

        #region Register & Resolve
        public T Register<T>(T service) where T : class, IService
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
 
            var key = typeof(T);
            if (!_servicesByType.TryAdd(key, service))
            {
                throw new InvalidOperationException($"{key.Name} already exists.");
            }

            _initializeOrder.Add(service);
            if (service is ITickable tickable)
            {
                _tickableList.Add(tickable);
            }
            return service;
        }
 
        public T Resolve<T>() where T : class, IService
        {
            if (_servicesByType.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
 
            throw new InvalidOperationException($"{typeof(T).Name} is not registered.");
        }
 
        public bool TryResolve<T>(out T service) where T : class, IService
        {
            if (_servicesByType.TryGetValue(typeof(T), out var found))
            {
                service = (T)found; 
                return true;
            }
            
            service = null; 
            return false;
        }
        #endregion Register & Resolve
 
        public void InitializeAll()
        {
            foreach (IService service in _initializeOrder)
            {
                service.Initialize();
            }
            
            _initialized = true;
        }
 
        public void Tick(float deltaTime)
        {
            foreach (ITickable tickable in _tickableList)
            {
                tickable.Tick(deltaTime);
            }
        }
    }
}
