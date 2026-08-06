using Runtime.Configs.Model;
using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/Level Config", fileName = "LevelConfig")]
    public class LevelConfigSO : ScriptableObject
    {
        public StickDef[] Sticks;
        public int[] PathOrder;
    }
}
