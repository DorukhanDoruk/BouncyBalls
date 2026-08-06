using Runtime.Configs.Model;
using UnityEngine;
namespace Runtime.Configs
{
    [CreateAssetMenu(menuName = "BouncyBalls/View Config", fileName = "ViewConfig")]
    public class ViewConfigSO : ScriptableObject
    {
        public const string ResourcePath = "Configuration/ViewConfig";

        [Header("Prefabs")]
        public GameObject BallPrefab;

        [Header("Debug Ball")]
        public DiscColorType DebugBallColor = DiscColorType.Yellow;
        public int DebugBallRemaining = 4;
    }
}
