using Runtime.Level.Model;
using UnityEngine;
namespace Runtime.Simulation.Model
{
    public class Ball
    {
        public readonly ObjectColor Color;
        public int Counter;
        public Vector3 Position;

        public int GridRowIndex;
        public int PathIndex;
        public float HopElapsed;
        public HopMotion Hop;
        public BallState State;

        public Ball(ObjectColor color, int counter, Vector3 position, int gridRowIndex)
        {
            Color = color;
            Counter = counter;
            Position = position;
            GridRowIndex = gridRowIndex;
            PathIndex = -1;
            HopElapsed = 0;
            Hop = default;
            State = default;
        }
    }
}
