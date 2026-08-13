using System;
using System.Collections.Generic;
using UnityEngine;
namespace Runtime.Level.Model
{
    [Serializable]
    public class StickData
    {
        public Vector3 Position;
        public List<ObjectColor> DiscColors = new();

        public int ShownDiscCount = 4;
        public int Height => DiscColors.Count;
    }
}
