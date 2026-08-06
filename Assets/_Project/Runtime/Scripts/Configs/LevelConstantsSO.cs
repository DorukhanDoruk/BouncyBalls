using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Level Constants", fileName = "LevelConstants")]
    public class LevelConstantsSO : ScriptableObject
    {
        public Vector3 StickSize;
        public Vector3 DiscSize;
    }
}
