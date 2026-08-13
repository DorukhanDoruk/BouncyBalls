using System;
using DG.Tweening;

namespace Runtime.Core
{
    [Serializable]
    public struct BallMoveSettings
    {
        public float GridShiftDuration;
        public Ease GridShiftEase;

        public float DockInsertDuration;
        public Ease DockInsertEase;
    }
}
