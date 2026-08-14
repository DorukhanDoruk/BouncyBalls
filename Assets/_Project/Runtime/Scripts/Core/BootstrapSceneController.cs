using UnityEngine;

namespace Runtime.Core
{
    public sealed class BootstrapSceneController : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
            SceneLoader.Load(SceneNames.Gameplay);
        }
    }
}
