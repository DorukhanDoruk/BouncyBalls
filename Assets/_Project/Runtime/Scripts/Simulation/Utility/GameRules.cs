using Runtime.Simulation.Model;
namespace Runtime.Simulation.Utility
{
    public static class GameRules
    {
        public static bool TryBreakDiscAtTop(Stick stick, Ball ball, out Disc brokenDisc)
        {
            if (stick.AliveCount == 0)
            {
                brokenDisc = default;
                return false;
            }

            var topDisc = stick.Discs[stick.AliveCount - 1];
            if (topDisc.Color != ball.Color)
            {
                brokenDisc = default;
                return false;
            }

            stick.RemoveTop();
            ball.Counter--;
            brokenDisc = topDisc;
            return true;
        }
    }
}
