using System.Collections.Generic;
using Runtime.Core;
using Runtime.Presentation;
using Runtime.Simulation;
using Runtime.Simulation.Model;
using UnityEngine;

namespace Runtime.Services
{
    public sealed class BallLabelPresenter
    {
        private readonly UiRootView _root;
        private readonly Camera _camera;
        private readonly RectTransform _layer;
        private readonly ObjectPool<BallLabelView> _pool;
        private readonly Dictionary<Ball, BallLabelView> _labels = new(ReferenceComparer<Ball>.Instance);

        private GameSimulation _simulation;

        public BallLabelPresenter(UiRootView root, Camera camera)
        {
            _root = root;
            _camera = camera;
            _layer = CreateLayer();
            _pool = new ObjectPool<BallLabelView>(root.BallLabelPrefab, _layer);
        }

        public void Rebuild(GameSimulation simulation)
        {
            Unsubscribe();

            _pool.ReleaseAll();
            _labels.Clear();

            _simulation = simulation;

            for (int i = 0; i < _simulation.ColumnCount; i++)
            {
                var column = _simulation.GridColumn(i);
                for (int j = 0; j < column.Count; j++)
                {
                    CreateLabel(column[j]);
                }
            }

            _simulation.Events.BallLanded += OnBallLanded;
            _simulation.Events.BallFinished += OnBallFinished;
        }

        public void Tick()
        {
            foreach (var pair in _labels)
            {
                Vector2 screenPoint = _camera.WorldToScreenPoint(pair.Key.Position + _root.BallLabelWorldOffset);
                screenPoint += _root.BallLabelScreenOffset;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, screenPoint, null, out Vector2 localPoint))
                {
                    pair.Value.SetAnchoredPosition(localPoint);
                }
            }
        }

        public void Unsubscribe()
        {
            if (_simulation == null)
            {
                return;
            }

            _simulation.Events.BallLanded -= OnBallLanded;
            _simulation.Events.BallFinished -= OnBallFinished;
            _simulation = null;
        }

        private RectTransform CreateLayer()
        {
            var layer = (RectTransform)new GameObject("BallLabels", typeof(RectTransform)).transform;
            layer.SetParent(_root.Root, false);
            layer.SetAsFirstSibling();

            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = Vector2.zero;
            layer.offsetMax = Vector2.zero;

            return layer;
        }

        private void CreateLabel(Ball ball)
        {
            var label = _pool.Get();
            label.SetCounter(ball.Counter);

            _labels.Add(ball, label);
        }

        private void OnBallLanded(Ball ball, Stick stick, Disc brokenDisc)
        {
            if (brokenDisc != null && _labels.TryGetValue(ball, out var label))
            {
                label.SetCounter(ball.Counter);
            }
        }

        private void OnBallFinished(Ball ball)
        {
            if (ball.State != BallState.Dead)
            {
                return;
            }

            if (_labels.TryGetValue(ball, out var label))
            {
                _pool.Release(label);
                _labels.Remove(ball);
            }
        }
    }
}
