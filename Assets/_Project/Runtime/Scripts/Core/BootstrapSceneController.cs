using UnityEngine;

namespace Runtime.Core
{
    public sealed class BootstrapSceneController : MonoBehaviour
    {
        private void Start()
        {
            SceneLoader.Load(SceneNames.Gameplay);
        }
    }
}
