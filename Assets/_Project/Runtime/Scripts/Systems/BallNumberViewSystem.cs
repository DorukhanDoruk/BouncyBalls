using Runtime.Components;
using Runtime.Configs;
using Runtime.Core;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class BallNumberViewSystem : SystemBase
    {
        private struct BallLabel
        {
            public TextMeshProUGUI Text;
            public int Shown;
        }

        private readonly Dictionary<Entity, BallLabel> _labels = new Dictionary<Entity, BallLabel>();
        private readonly List<Entity> _stale = new List<Entity>();

        private Camera _camera;
        private UiService _ui;
        private float _ballLift;

        protected override void OnCreate()
        {
            RequireForUpdate<ManagedServicesComponent>();
        }

        protected override void OnStartRunning()
        {
            _camera = Camera.main;
            _ui = SystemAPI.ManagedAPI.GetSingleton<ManagedServicesComponent>().Container.Resolve<UiService>();

            var renderConfig = Resources.Load<RenderConfigSO>(RenderConfigSO.ResourcePath);
            _ballLift = -renderConfig.BallMesh.bounds.min.y * renderConfig.BallScale.y;
        }

        protected override void OnDestroy()
        {
            _labels.Clear();
        }

        protected override void OnUpdate()
        {
            foreach (var (ballTransform, ball, entity) in
                     SystemAPI.Query<RefRO<TransformComponent>, RefRO<BallComponent>>().WithEntityAccess())
            {
                if (!_labels.TryGetValue(entity, out var label))
                {
                    label = new BallLabel { Text = _ui.CreateBallLabel(), Shown = -1 };
                }

                // TMP rebuilds its mesh on every text assignment, so only write on change.
                int remaining = ball.ValueRO.Remaining;
                if (label.Shown != remaining)
                {
                    label.Text.SetText("{0}", remaining);
                    label.Shown = remaining;
                }

                var scale = ballTransform.ValueRO.Scale;
                Vector3 world = (Vector3)ballTransform.ValueRO.Position + Vector3.up * (_ballLift * scale.y) + _ui.BallLabelWorldOffset * scale.y;
                Vector3 screen = _camera.WorldToScreenPoint(world);
                Vector2 screenOffset = _ui.BallLabelScreenOffset;

                label.Text.rectTransform.position = new Vector3(screen.x + screenOffset.x, screen.y + screenOffset.y, 0f);
                label.Text.rectTransform.localScale = new Vector3(scale.x, scale.y, 1f);

                // BallLabel is a struct, so the mutated copy has to be written back.
                _labels[entity] = label;
            }

            _stale.Clear();
            foreach (var pair in _labels)
            {
                if (!EntityManager.Exists(pair.Key))
                {
                    _stale.Add(pair.Key);
                }
            }

            foreach (var entity in _stale)
            {
                Object.Destroy(_labels[entity].Text.gameObject);
                _labels.Remove(entity);
            }
        }
    }
}
