using Runtime.Level.Model;
using System.Collections.Generic;
using UnityEngine;
namespace Runtime.Level.Config
{

    [CreateAssetMenu(menuName = "BouncyBalls/Level", fileName = "Level_00")]
    public class LevelData : ScriptableObject
    {
        [Header("Playground")]
        public List<StickData> Sticks = new();
        public List<int> Path = new();

        [Header("Grid")]
        public List<GridColumnData> GridColumns = new();
    }
}
