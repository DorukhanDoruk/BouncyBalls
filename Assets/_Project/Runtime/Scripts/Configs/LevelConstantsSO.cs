using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Level Constants", fileName = "LevelConstants")]
    public class LevelConstantsSO : ScriptableObject
    {
        public float StickWidth;
        public Vector3 DiscSize;
    }
}
