using UnityEngine;
namespace Runtime.Core
{
    [CreateAssetMenu(menuName = "BouncyBalls/Level Constants", fileName = "LevelConstants")]
    public class LevelLayoutSO : ScriptableObject
    {
        [Header("Stick Layout")]
        public Vector3 StickOrigin = new Vector3(0f, 0.05f, 4f);

        [Header("Grid Layout")]
        public Vector3 GridOrigin = new Vector3(0f, 0f, -7f);
        public float GridColumnSpacing = 1.75f;
        public float GridRowSpacing = 2.35f;

        [Header("Dock Layout")]
        public Vector3 DockOrigin = new Vector3(0f, 0.1f, -3.6f);
        public float DockSlotSpacing = 1.6f;

        [Header("Balls")]
        public float BallLift = 0.6f;

        [Header("Input")]
        public float BallSelectionRadius = 0.75f;

        public LevelLayout ToRuntime() => new LevelLayout
        {
            StickOrigin = StickOrigin, GridOrigin = GridOrigin,
            GridColumnSpacing = GridColumnSpacing, GridRowSpacing = GridRowSpacing,
            DockOrigin = DockOrigin, DockSlotSpacing = DockSlotSpacing,
            BallLift = BallLift, BallSelectionRadius = BallSelectionRadius
        };
    }
}
