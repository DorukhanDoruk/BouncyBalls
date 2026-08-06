using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Level Constants", fileName = "LevelConstants")]
    public class LevelConstantsSO : ScriptableObject
    {
        public float StickWidth;
        public Vector3 DiscSize;

        [Header("Grid Layout")]
        // Sutun 0'in en on topunun konumu. Sutunlar +X, siralar -Z yonunde aciliyor.
        public Vector3 GridOrigin = new Vector3(0f, 0f, -8f);
        public float GridColumnSpacing = 1.5f;
        public float GridRowSpacing = 1.5f;

        [Header("Dock Layout")]
        public Vector3 DockOrigin = new Vector3(0f, 0f, -4f);
        public float DockSlotSpacing = 1.5f;
    }
}
