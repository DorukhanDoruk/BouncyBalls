using UnityEngine;
namespace Runtime.Core
{
    public sealed class BootstrapSceneController : MonoBehaviour
    {
        private void Start()
        {
            Game.Get<SceneLoaderService>().Load(SceneNames.Gameplay);
        }
    }
}
