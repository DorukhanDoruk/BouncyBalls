using Runtime.Simulation.Model;
namespace Runtime.Simulation
{
    public static class GameRules
    {
        public static Disc BreakDiscAtTop(Stick stick, Ball ball)
        {
            if (stick.AliveCount == 0)
            {
                return null;
            }

            var topDisc = stick.Discs[stick.AliveCount - 1];
            if (topDisc.Color != ball.Color)
            {
                return null;
            }

            stick.RemoveTop();
            ball.Counter--;
            return topDisc;
        }
    }
}
