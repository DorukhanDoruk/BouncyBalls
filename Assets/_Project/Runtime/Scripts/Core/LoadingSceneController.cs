using UnityEngine;
using UnityEngine.SceneManagement;
namespace Runtime.Core
{
    public sealed class LoadingSceneController : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene(Game.Get<SceneLoaderService>().Target);
        }
    }
}
