using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Level Constants", fileName = "LevelConstants")]
    public class LevelConstantsSO : ScriptableObject
    {
        public float StickWidth;
        public Vector3 DiscSize;

        [Header("Stick Layout")]
        public Vector3 StickOrigin;

        [Header("Grid Layout")]
        public Vector3 GridOrigin = new Vector3(0f, 0f, -8f);
        public float GridColumnSpacing = 1.5f;
        public float GridRowSpacing = 1.5f;

        [Header("Dock Layout")]
        public Vector3 DockOrigin = new Vector3(0f, 0f, -4f);
        public float DockSlotSpacing = 1.5f;

        [Header("Input")]
        public float BallSelectionRadius = 0.75f;
    }
}
