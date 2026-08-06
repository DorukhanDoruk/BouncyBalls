using System;
using UnityEngine;
namespace Runtime.Configs.Model
{
    [Serializable]
    public class StickDef
    {
        public Vector3 Position;
        public DiscColorType[] Discs;
        public int ShownDiscCount;
    }
}
