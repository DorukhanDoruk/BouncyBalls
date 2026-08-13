using System.Collections.Generic;
using Runtime.Core;
using Runtime.Level.Config;
using Runtime.Simulation.Model;
using UnityEngine;

namespace Runtime.Simulation.Utility
{
    public static class LevelLoader
    {
        public static List<Stick> LoadSticks(LevelData level, LevelLayout layout)
        {
            var sticks = new List<Stick>(level.Sticks.Count);
            for (int i = 0; i < level.Sticks.Count; i++)
            {
                var data = level.Sticks[i];
                var discs = new List<Disc>(data.DiscColors.Count);

                for (int d = 0; d < data.DiscColors.Count; d++)
                {
                    discs.Add(new Disc(data.DiscColors[d]));
                }

                sticks.Add(new Stick(discs, data.Position + layout.StickOrigin, discs.Count, data.ShownDiscCount));
            }

            return sticks;
        }

        public static List<List<Ball>> LoadGrid(LevelData level, in LevelLayout layout)
        {
            int columnCount = level.GridColumns.Count;
            var grid = new List<List<Ball>>(columnCount);
            for (int c = 0; c < columnCount; c++)
            {
                var source = level.GridColumns[c].GridBalls;
                var column = new List<Ball>(source.Count);

                for (int r = 0; r < source.Count; r++)
                {
                    var slot = GridBallSlot(c, columnCount, r, layout);
                    column.Add(new Ball(source[r].ColorId, source[r].Counter, slot));
                }

                grid.Add(column);
            }

            return grid;
        }

        public static Vector3 GridBallSlot(int column, int columnCount, int row, LevelLayout layout)
        {
            float x = (column - (columnCount - 1) * 0.5f) * layout.GridColumnSpacing;
            float z = -row * layout.GridRowSpacing;
            return layout.GridOrigin + new Vector3(x, layout.BallLift, z);
        }

        public static Vector3 DockSlot(int index, int capacity, LevelLayout layout)
        {
            float x = (index - (capacity - 1) * 0.5f) * layout.DockSlotSpacing;
            return layout.DockOrigin + new Vector3(x, 0f, 0f);
        }

        public static Vector3 DockBallSlot(int index, int capacity, LevelLayout layout)
        {
            return DockSlot(index, capacity, layout) + Vector3.up * layout.BallLift;
        }

        // The stack hangs from the stick top, so the landing height never changes as discs break.
        public static Vector3 SurfacePoint(Stick stick, float discHeight)
        {
            return stick.Position + Vector3.up * (stick.ShownDiscCount * discHeight);
        }
    }
}
