using Runtime.Config;
using Runtime.Core;
using Runtime.Simulation;
using Runtime.Simulation.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Runtime.Presentation
{
    public class GameView : MonoBehaviour
    {
        private readonly Dictionary<Ball, BallView> _ballViews = new(ReferenceComparer<Ball>.Instance);
        private readonly Dictionary<Disc, DiscView> _discViews = new(ReferenceComparer<Disc>.Instance);
        private readonly Dictionary<Stick, StickView> _stickViews = new(ReferenceComparer<Stick>.Instance);

        private GameSimulation _simulation;
        private VisualConfigSO _visualConfig;
        private GameConfig _gameConfig;
        private LevelLayout _levelLayout;
        private ViewFactory _factory;

        public void Initialize(VisualConfigSO visualConfig, GameConfig gameConfig, LevelLayout levelLayout)
        {
            _visualConfig = visualConfig;
            _gameConfig = gameConfig;
            _levelLayout = levelLayout;

            _factory = new ViewFactory(_visualConfig, transform);
        }

        public void Rebuild(GameSimulation simulation)
        {
            Teardown();

            _simulation = simulation;

            BuildDock();
            BuildSticks();
            BuildBalls();

            _simulation.Events.BallLanded += EventsOnBallLanded;
            _simulation.Events.BallFinished += EventsOnBallFinished;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Teardown()
        {
            Unsubscribe();

            _factory.ReleaseAll();
            _ballViews.Clear();
            _discViews.Clear();
            _stickViews.Clear();
        }

        private void Unsubscribe()
        {
            if (_simulation == null)
            {
                return;
            }

            _simulation.Events.BallLanded -= EventsOnBallLanded;
            _simulation.Events.BallFinished -= EventsOnBallFinished;
            _simulation = null;
        }

        private void BuildDock()
        {
            for (int i = 0; i < _gameConfig.DockCapacity; i++)
            {
                _factory.CreateDock(LevelLoader.DockSlot(i, _gameConfig.DockCapacity, _levelLayout));
            }
        }

        private void BuildSticks()
        {
            var sticks = _simulation.Sticks;
            for (int i = 0; i < sticks.Count; i++)
            {
                var stick = sticks[i];
                var stickView = _factory.CreateStick(stick, _gameConfig.DiscHeight);
                _stickViews.Add(stick, stickView);

                for (int j = Mathf.Max(0, stick.AliveCount - stick.ShownDiscCount); j < stick.AliveCount; j++)
                {
                    var disc = stick.Discs[j];
                    var discView = _factory.CreateDisc(disc, stick.Position, stickView.transform);

                    _discViews.Add(disc, discView);
                    stickView.AddDisc(discView.transform);
                }

                stickView.LayoutDiscs(0f, _visualConfig.DiscShiftEase);
            }
        }

        private void BuildBalls()
        {
            for (int i = 0; i < _simulation.ColumnCount; i++)
            {
                var column = _simulation.GridColumn(i);
                for (int j = 0; j < column.Count; j++)
                {
                    var ball = column[j];
                    _ballViews.Add(ball, _factory.CreateBall(ball));
                }
            }
        }

        private void EventsOnBallFinished(Ball ball)
        {
            if (ball.State == BallState.Dead)
            {
                if (!_ballViews.TryGetValue(ball, out BallView ballView))
                {
                    throw new Exception("Finished ball doesnt have registered BallView");
                }

                _factory.ReleaseBall(ballView);
                _ballViews.Remove(ball);
            }
        }

        private void EventsOnBallLanded(Ball ball, Stick stick, Disc disc)
        {
            var stickView = _stickViews[stick];
            stickView.Dip(_visualConfig.StickDipAmount, _visualConfig.StickDipDuration);

            if (disc == null)
            {
                return;
            }

            if (!_discViews.TryGetValue(disc, out DiscView discView))
            {
                throw new Exception("Landed disc doesnt have registered DiscView");
            }

            _factory.BreakDisc(discView, disc, stick.Position.y);
            _discViews.Remove(disc);

            stickView.RemoveTopDisc();
            RevealHiddenDisc(stick, stickView);
            stickView.LayoutDiscs(_visualConfig.DiscShiftDuration, _visualConfig.DiscShiftEase);
        }

        private void RevealHiddenDisc(Stick stick, StickView stickView)
        {
            int index = stick.AliveCount - stick.ShownDiscCount;

            if (index < 0)
            {
                return;
            }

            var disc = stick.Discs[index];
            var discView = _factory.CreateDisc(disc, stick.Position, stickView.transform);

            _discViews.Add(disc, discView);
            stickView.AddDiscBelow(discView.transform);
        }
    }
}
