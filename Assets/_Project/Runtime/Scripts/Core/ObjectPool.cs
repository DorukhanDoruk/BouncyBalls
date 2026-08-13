using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Core
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _idle = new();
        private readonly List<T> _active = new();

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
            _active.Add(instance);
            return instance;
        }

        public void Release(T instance)
        {
            _active.Remove(instance);
            instance.gameObject.SetActive(false);
            _idle.Push(instance);
        }

        public void ReleaseAll()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i] == null)
                {
                    continue;
                }

                _active[i].gameObject.SetActive(false);
                _idle.Push(_active[i]);
            }

            _active.Clear();
        }

        private T Create(bool active)
        {
            var instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(active);
            return instance;
        }
    }
}
