using System.Collections.Generic;
using Runtime.Simulation.Model;
namespace Runtime.Simulation
{
    public static class PathDecisionMaker
    {
        public static PathMovementDecision MakeDecision(List<int> path, List<Stick> sticks, int currentPathIndex,
            bool gridHasBalls, out int nextPathIndex)
        {
            // No disc left anywhere on the path means the level is won.
            bool allSticksAreCleared = true;
            for (int i = 0; i < path.Count; i++)
            {
                if (sticks[path[i]].AliveCount > 0)
                {
                    allSticksAreCleared = false;
                    break;
                }
            }

            if (allSticksAreCleared)
            {
                nextPathIndex = -1;
                return PathMovementDecision.PathSticksCleared;
            }

            // Look ahead for the next stick that still has a disc; empty ones are skipped.
            for (int i = currentPathIndex + 1; i < path.Count; i++)
            {
                if (sticks[path[i]].AliveCount > 0)
                {
                    nextPathIndex = i;
                    return PathMovementDecision.Continue;
                }
            }

            // Nothing ahead while balls are still waiting, so this one retires to the dock.
            if (gridHasBalls)
            {
                nextPathIndex = -1;
                return PathMovementDecision.PathLapFinished;
            }

            // Grid is empty, so the ball wraps around; its current stick counts too.
            for (int i = 0; i <= currentPathIndex; i++)
            {
                if (sticks[path[i]].AliveCount > 0)
                {
                    nextPathIndex = i;
                    return PathMovementDecision.Continue;
                }
            }

            // Unreachable; the first scan already caught this case.
            nextPathIndex = -1;
            return PathMovementDecision.PathSticksCleared;
        }
    }
}
