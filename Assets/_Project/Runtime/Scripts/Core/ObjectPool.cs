using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Core
{
    // Minimal pool for view components; instances are parented once and reused.
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _idle = new();

        public ObjectPool(T prefab, Transform parent, int prewarm = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < prewarm; i++)
            {
                _idle.Push(Create(false));
            }
        }

        public T Get(Vector3 position)
        {
            var instance = _idle.Count > 0 ? _idle.Pop() : Create(true);

            instance.transform.position = position;
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            _idle.Push(instance);
        }

        private T Create(bool active)
        {
            var instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(active);
            return instance;
        }
    }
}
