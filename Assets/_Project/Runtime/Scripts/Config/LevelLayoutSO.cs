using UnityEngine;
namespace Runtime.Core
{
    [CreateAssetMenu(menuName = "BouncyBalls/Level Constants", fileName = "LevelConstants")]
    public class LevelLayoutSO : ScriptableObject
    {
        [Header("World Origins")]
        public Vector3 GridOrigin  = new Vector3(0f, -4f, 0f);
        public Vector3 DockOrigin  = new Vector3(0f, -2f, 0f);
        public float   SlotSpacing = 1.1f;

        public LevelLayout ToRuntime() => new LevelLayout
        {
            GridOrigin = GridOrigin,
            DockOrigin = DockOrigin,
            SlotSpacing = SlotSpacing
        };
    }
}
