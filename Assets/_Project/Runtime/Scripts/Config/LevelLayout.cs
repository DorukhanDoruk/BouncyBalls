using System;
using UnityEngine;
namespace Runtime.Core
{
    [Serializable]
    public struct LevelLayout
    {
        public Vector3 StickOrigin;

        public Vector3 GridOrigin;
        public float GridColumnSpacing;
        public float GridRowSpacing;

        public Vector3 DockOrigin;
        public float DockSlotSpacing;

        public float BallSelectionRadius;
    }
}
