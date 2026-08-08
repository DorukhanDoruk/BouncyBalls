using Runtime.Components;
using Runtime.Configs;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class BallTrailViewSystem : SystemBase
    {
        private readonly Dictionary<Entity, ParticleSystem> _trails = new Dictionary<Entity, ParticleSystem>();
        private readonly List<Entity> _stale = new List<Entity>();

        private RenderConfigSO _renderConfig;
        private Transform _root;
        private float _ballLift;

        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnStartRunning()
        {
            _renderConfig = Resources.Load<RenderConfigSO>(RenderConfigSO.ResourcePath);
            _ballLift = -_renderConfig.BallMesh.bounds.min.y * _renderConfig.BallScale.y;

            _root = new GameObject("BallTrails").transform;
        }

        protected override void OnStopRunning()
        {
            _trails.Clear();

            if (_root != null)
            {
                Object.Destroy(_root.gameObject);
            }
        }

        protected override void OnUpdate()
        {
            var palette = _renderConfig.Palette;

            foreach (var (ballTransform, ball, entity) in SystemAPI.Query<RefRO<TransformComponent>, RefRO<BallComponent>>().WithAll<HopState>().WithEntityAccess())
            {
                if (!_trails.TryGetValue(entity, out var trail))
                {
                    trail = Object.Instantiate(_renderConfig.BallTrailPrefab, _root).GetComponent<ParticleSystem>();
                    _trails.Add(entity, trail);

                    var main = trail.main;
                    main.startColor = palette.GetBall(ball.ValueRO.Color).BaseColor;

                    trail.transform.position = BallCentre(ballTransform.ValueRO);
                    trail.Clear(true);
                    trail.Play(true);
                    continue;
                }

                trail.transform.position = BallCentre(ballTransform.ValueRO);
            }

            _stale.Clear();
            foreach (var pair in _trails)
            {
                if (!EntityManager.Exists(pair.Key) || !EntityManager.HasComponent<HopState>(pair.Key))
                {
                    _stale.Add(pair.Key);
                }
            }

            foreach (var entity in _stale)
            {
                var trail = _trails[entity];
                _trails.Remove(entity);

                trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Object.Destroy(trail.gameObject, trail.main.startLifetime.constantMax);
            }
        }

        private Vector3 BallCentre(in TransformComponent transform)
        {
            return (Vector3)transform.Position + Vector3.up * (_ballLift * transform.Scale.y);
        }
    }
}
