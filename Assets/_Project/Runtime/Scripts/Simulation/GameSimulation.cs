using System.Collections.Generic;
using Runtime.Core;
using Runtime.Level.Config;
using Runtime.Simulation.Model;
using Runtime.Simulation.Utility;

namespace Runtime.Simulation
{
    public class GameSimulation
    {
        private readonly GameConfig _config;
        private readonly LevelLayout _layout;

        private readonly List<Stick> _sticks;
        private readonly List<int> _path;
        private readonly List<List<Ball>> _grid;
        private readonly List<Ball> _flying = new();
        private readonly List<Ball> _dock = new();

        private float _launchCooldown;
        private bool _isOver;

        public SimulationEvents Events { get; } = new();

        public GameSimulation(LevelData level, GameConfig config, LevelLayout layout)
        {
            _config = config;
            _layout = layout;
            _path = new List<int>(level.Path);

            _sticks = LevelLoader.LoadSticks(level, layout);
            _grid = LevelLoader.LoadGrid(level, layout);
        }

        public IReadOnlyList<Stick> Sticks => _sticks;
        public IReadOnlyList<Ball> Flying => _flying;
        public IReadOnlyList<Ball> Dock => _dock;
        public IReadOnlyList<Ball> GridColumn(int column) => _grid[column];
        
        public bool IsOver => _isOver;
        public int ColumnCount => _grid.Count;
        public bool CanLaunch => !_isOver && _flying.Count < _config.MaxActiveBalls && _launchCooldown <= 0f;

        public void Tick(float deltaTime)
        {
            if (_isOver)
            {
                return;
            }

            if (_launchCooldown > 0f)
            {
                _launchCooldown -= deltaTime;
            }

            // Backwards, because an arriving ball can leave the list.
            for (int i = _flying.Count - 1; i >= 0; i--)
            {
                var ball = _flying[i];
                ball.HopElapsed += deltaTime;
                ball.Position = ball.Hop.Evaluate(ball.HopElapsed);

                if (ball.HopElapsed < ball.Hop.Duration)
                {
                    continue;
                }

                Arrive(ball, i);

                if (_isOver)
                {
                    return;
                }
            }
        }

        public void LaunchFromGrid(int column)
        {
            var ball = _grid[column][0];
            _grid[column].RemoveAt(0);

            RepositionColumn(column);
            Events.RaiseGridShifted();

            StartFlight(ball);
        }

        public void LaunchFromDock(int index)
        {
            var ball = _dock[index];
            _dock.RemoveAt(index);

            RepositionDock();

            StartFlight(ball);
        }

        private bool DoesGridHasBalls()
        {
            for (int c = 0; c < _grid.Count; c++)
            {
                if (_grid[c].Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void StartFlight(Ball ball)
        {
            GameRules.MakeDecision(_path, _sticks, -1, DoesGridHasBalls(), out int next);

            ball.PathIndex = next;
            ball.State = BallState.Flying;
            ball.HopElapsed = 0f;
            ball.Hop = BuildHop(ball, next);

            _flying.Add(ball);
            _launchCooldown = _config.MinLaunchInterval;

            Events.RaiseBallLaunched(ball);
        }

        private void Arrive(Ball ball, int flyingIndex)
        {
            ball.Position = ball.Hop.To;

            if (ball.State == BallState.ToDock)
            {
                CompleteDocking(ball, flyingIndex);
                return;
            }

            var stick = _sticks[_path[ball.PathIndex]];
            GameRules.TryBreakDiscAtTop(stick, ball, out var brokenDisc);
            Events.RaiseBallLanded(ball, stick, brokenDisc);

            var decision = GameRules.MakeDecision(_path, _sticks, ball.PathIndex, DoesGridHasBalls(), out int next);

            bool ballDied = ball.Counter <= 0;
            if (ballDied)
            {
                KillBall(ball, flyingIndex);
            }

            if (decision == PathMovementDecision.PathSticksCleared)
            {
                EndLevel(true);
                return;
            }

            if (ballDied)
            {
                return;
            }

            if (decision == PathMovementDecision.PathLapFinished)
            {
                SendToDock(ball);
                return;
            }

            ball.PathIndex = next;
            ball.HopElapsed = 0f;
            ball.Hop = BuildHop(ball, next);
        }

        private HopMotion BuildHop(Ball ball, int pathIndex)
        {
            var target = _sticks[_path[pathIndex]];
            var landing = LevelLoader.SurfacePoint(target, _config.DiscHeight);
            return new HopMotion(ball.Position, landing, _config);
        }

        private void KillBall(Ball ball, int flyingIndex)
        {
            _flying.RemoveAt(flyingIndex);
            ball.State = BallState.Dead;

            Events.RaiseBallFinished(ball);
        }

        private void SendToDock(Ball ball)
        {
            var slot = LevelLoader.DockBallSlot(_dock.Count, _config.DockCapacity, _layout);

            ball.State = BallState.ToDock;
            ball.HopElapsed = 0f;
            ball.Hop = new HopMotion(ball.Position, slot, _config);
        }

        private void CompleteDocking(Ball ball, int flyingIndex)
        {
            if (_dock.Count >= _config.DockCapacity)
            {
                EndLevel(false);
                return;
            }

            _flying.RemoveAt(flyingIndex);
            ball.State = BallState.AtDock;
            _dock.Add(ball);

            RepositionDock();

            Events.RaiseBallFinished(ball);
        }

        private void EndLevel(bool won)
        {
            _isOver = true;
            Events.RaiseLevelEnded(won);
        }

        private void RepositionColumn(int column)
        {
            var balls = _grid[column];
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].Position = LevelLoader.GridBallSlot(column, ColumnCount, i, _layout);
            }
        }

        private void RepositionDock()
        {
            for (int i = 0; i < _dock.Count; i++)
            {
                _dock[i].Position = LevelLoader.DockBallSlot(i, _config.DockCapacity, _layout);
            }
        }
    }
}
