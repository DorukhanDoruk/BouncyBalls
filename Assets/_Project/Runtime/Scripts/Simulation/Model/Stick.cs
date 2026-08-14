using System.Collections.Generic;
using UnityEngine;
namespace Runtime.Simulation.Model
{
    public class Stick
    {
        public List<Disc> Discs;
        public Vector3 Position;
        public int AliveCount;
        public int ShownDiscCount;

        public Stick(List<Disc> discs, Vector3 position, int aliveCount, int shownDiscCount)
        {
            Discs = discs;
            Position = position;
            AliveCount = aliveCount;
            ShownDiscCount = shownDiscCount;
        }

        public void RemoveTop()
        {
            AliveCount--;
        }
    }
}
