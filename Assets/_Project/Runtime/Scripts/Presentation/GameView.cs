using Runtime.Core;
using Runtime.Simulation;
using Runtime.Simulation.Model;
using Runtime.Simulation.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Runtime.Presentation
{
    public class GameView : MonoBehaviour
    {
        private readonly Dictionary<Ball, BallView> _ballViews = new Dictionary<Ball, BallView>();
        private readonly Dictionary<Disc, DiscView> _discViews = new Dictionary<Disc, DiscView>();
        
        private GameSimulation _simulation;
        private VisualConfigSO _visualConfig;
        private GameConfig _gameConfig;
        private ViewFactory _factory;
        
        public void Initialize(GameSimulation simulation, VisualConfigSO visualConfig, GameConfig gameConfig, LevelLayout levelLayout)
        {
            _simulation = simulation;
            _visualConfig = visualConfig;
            _gameConfig = gameConfig;

            _factory = new ViewFactory(_visualConfig, transform);

            for (int i = 0; i < _gameConfig.DockCapacity; i++)
            {
                _factory.CreateDock(LevelLoader.DockSlot(i, _gameConfig.DockCapacity, levelLayout));
            }

            foreach (var stick in simulation.Sticks)
            {
                _factory.CreateStick(stick);

                int aliveCount = stick.AliveCount;
                for (int i = 0; i < aliveCount; i++)
                {
                    var disc = stick.Discs[i];
                    var discView = _factory.CreateDisc(disc, (stick.Position + (Vector3.up * (_gameConfig.DiscHeight / 2f))) + (Vector3.up * (i * _gameConfig.DiscHeight)));
                    _discViews.Add(disc, discView);
                }
            }

            for (int i = 0; i < _simulation.ColumnCount; i++)
            {
                var column = simulation.GridColumn(i);
                foreach (var ball in column)
                {
                    var ballView = _factory.CreateBall(ball);
                    _ballViews.Add(ball, ballView);
                }
            }
            
            _simulation.Events.BallLanded += EventsOnBallLanded;
            _simulation.Events.BallFinished += EventsOnBallFinished;
        }

        private void OnDestroy()
        {
            _simulation.Events.BallLanded -= EventsOnBallLanded;
            _simulation.Events.BallFinished -= EventsOnBallFinished;
        }

        private void EventsOnBallFinished(Ball ball)
        {
            if (ball.State == BallState.Dead)
            {
                if (!_ballViews.TryGetValue(ball, out BallView ballView))
                {
                    throw new Exception("Landed disc doesn't have registered BallView");
                }

                _factory.ReleaseBall(ballView);
                _ballViews.Remove(ball);
            }
        }

        private void EventsOnBallLanded(Ball ball, Stick stick, Disc disc)
        {
            if (disc != null)
            {
                if (!_discViews.TryGetValue(disc, out DiscView discView))
                {
                    throw new Exception("Landed disc doesn't have registered DiskView");
                }

                _factory.ReleaseDisc(discView);
                _discViews.Remove(disc);
            }
        }
    }
}
