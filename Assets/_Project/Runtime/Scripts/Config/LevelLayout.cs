using System;
using UnityEngine;
namespace Runtime.Core
{
    [Serializable]
    public struct LevelLayout
    {
        public Vector3 GridOrigin;
        public Vector3 DockOrigin;
        public float SlotSpacing;
    }
}
