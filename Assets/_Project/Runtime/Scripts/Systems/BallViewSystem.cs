using Runtime.Components;
using Runtime.Configs;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class BallViewSystem : SystemBase
    {
        private readonly Dictionary<Entity, Transform> _views = new Dictionary<Entity, Transform>();
        private readonly List<Entity> _tempRemove = new List<Entity>();

        private GameObject _ballPrefab;

        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
            _ballPrefab = Resources.Load<ViewConfigSO>(ViewConfigSO.ResourcePath).BallPrefab;
        }

        protected override void OnUpdate()
        {
            foreach (var (ballTransform, entity) in SystemAPI.Query<RefRO<TransformComponent>>().WithAll<BallComponent>().WithEntityAccess())
            {
                if (!_views.TryGetValue(entity, out var view))
                {
                    view = Object.Instantiate(_ballPrefab).transform;
                    _views.Add(entity, view);
                }

                view.position = ballTransform.ValueRO.Position;
            }

            _tempRemove.Clear();
            foreach (var (entity, _) in _views)
            {
                if (!EntityManager.Exists(entity))
                {
                    _tempRemove.Add(entity);
                }
            }

            foreach (var entity in _tempRemove)
            {
                // We may use objectPool in future but for now no need for it.
                Object.Destroy(_views[entity].gameObject);
                _views.Remove(entity);
            }
        }
    }
}
