using System.Collections.Generic;
using UnityEngine;
namespace Runtime.Simulation.Model
{
    public class Stick
    {
        public List<Disc> Discs;
        public Vector3 Position;
        public int AliveCount;

        public Stick(List<Disc> discs, Vector3 position, int aliveCount)
        {
            Discs = discs;
            Position = position;
            AliveCount = aliveCount;
        }
    }
}
