using Runtime.Config;
using System;
using System.Collections.Generic;
using Runtime.Core;
using Runtime.Presentation;
using Runtime.Simulation;
using Runtime.Simulation.Model;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime.Services
{
    public class UIPresenter : IService, ITickable
    {
        private readonly UiRootView _rootPrefab;
        private readonly GameFlowService _flowService;
        private readonly LevelService _levelService;
        private readonly Camera _camera;
        private readonly GameConfig _gameConfig;

        private readonly Dictionary<Ball, TextMeshProUGUI> _labels = new();

        private UiRootView _root;
        private RectTransform _labelRoot;
        private GameSimulation _simulation;

        public event Action PlayAgainRequested;

        public UIPresenter(UiRootView rootPrefab, GameFlowService flowService, LevelService levelService,
            Camera camera, GameConfig gameConfig)
        {
            _rootPrefab = rootPrefab;
            _flowService = flowService;
            _levelService = levelService;
            _camera = camera;
            _gameConfig = gameConfig;
        }

        public void Initialize()
        {
            _root = Object.Instantiate(_rootPrefab);

            CreateLabelRoot();

            _root.ResultPanel.gameObject.SetActive(false);
            _root.Backdrop.gameObject.SetActive(false);
            _root.ResultPanel.PlayAgainButton.onClick.AddListener(OnPlayAgainClicked);

            _flowService.LevelStarted += OnLevelStarted;
        }

        public void Dispose()
        {
            _flowService.LevelStarted -= OnLevelStarted;
            Teardown();
            if (_root == null)
            {
                return;
            }

            _root.ResultPanel.PlayAgainButton.onClick.RemoveListener(OnPlayAgainClicked);
            Object.Destroy(_root.gameObject);
            _root = null;
        }

        // Runs for every level, first one included.
        private void OnLevelStarted(GameSimulation simulation)
        {
            Teardown();

            _simulation = simulation;

            _root.TopArea.SetLevelName(_levelService.Current.name);
            _root.ResultPanel.Hide();
            _root.Backdrop.Hide();

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
            _simulation.Events.LevelEnded += OnLevelEnded;
        }

        private void Teardown()
        {
            if (_simulation != null)
            {
                _simulation.Events.BallLanded -= OnBallLanded;
                _simulation.Events.BallFinished -= OnBallFinished;
                _simulation.Events.LevelEnded -= OnLevelEnded;
                _simulation = null;
            }

            foreach (var label in _labels.Values)
            {
                if (label != null)
                {
                    Object.Destroy(label.gameObject);
                }
            }

            _labels.Clear();
        }

        public void Tick(float deltaTime)
        {
            _root.DockCounter.SetCount(_simulation.Dock.Count, _gameConfig.DockCapacity);

            foreach (var pair in _labels)
            {
                Vector2 screenPoint = _camera.WorldToScreenPoint(pair.Key.Position + _root.BallLabelWorldOffset);
                screenPoint += _root.BallLabelScreenOffset;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_labelRoot, screenPoint, null, out Vector2 localPoint))
                {
                    pair.Value.rectTransform.anchoredPosition = localPoint;
                }
            }
        }

        private void CreateLabelRoot()
        {
            _labelRoot = (RectTransform)new GameObject("BallLabels", typeof(RectTransform)).transform;
            _labelRoot.SetParent(_root.Root, false);
            _labelRoot.SetAsFirstSibling();

            _labelRoot.anchorMin = Vector2.zero;
            _labelRoot.anchorMax = Vector2.one;
            _labelRoot.offsetMin = Vector2.zero;
            _labelRoot.offsetMax = Vector2.zero;
        }

        private void CreateLabel(Ball ball)
        {
            var label = Object.Instantiate(_root.BallLabelPrefab, _labelRoot);
            _labels.Add(ball, label);
            label.SetText("{0}", ball.Counter);
        }

        private void OnBallLanded(Ball ball, Stick stick, Disc brokenDisc)
        {
            if (brokenDisc != null && _labels.TryGetValue(ball, out var label))
            {
                label.SetText("{0}", ball.Counter);
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
                Object.Destroy(label.gameObject);
                _labels.Remove(ball);
            }
        }

        private void OnLevelEnded(bool won)
        {
            _root.Backdrop.Show();
            _root.ResultPanel.Show(won);
        }

        private void OnPlayAgainClicked()
        {
            PlayAgainRequested?.Invoke();
        }
    }
}
