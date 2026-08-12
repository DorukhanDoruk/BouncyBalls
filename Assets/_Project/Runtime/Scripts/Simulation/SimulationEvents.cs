using System;
using Runtime.Simulation.Model;

namespace Runtime.Simulation
{
    public class SimulationEvents
    {
        public event Action<Ball> BallLaunched;
        public event Action<Ball, Stick, Disc> BallLanded;
        public event Action<Ball> BallFinished;
        public event Action GridShifted;
        public event Action<bool> LevelEnded;

        public void RaiseBallLaunched(Ball ball) => BallLaunched?.Invoke(ball);
        public void RaiseBallLanded(Ball ball, Stick stick, Disc brokenDisc) => BallLanded?.Invoke(ball, stick, brokenDisc);
        public void RaiseBallFinished(Ball ball) => BallFinished?.Invoke(ball);
        public void RaiseGridShifted() => GridShifted?.Invoke();
        public void RaiseLevelEnded(bool won) => LevelEnded?.Invoke(won);
    }
}
