using UnityEngine.SceneManagement;
namespace Runtime.Core
{
    public sealed class SceneLoaderService : IService
    {
        private string _target;
        public string Target => _target;

        public void Initialize() { }

        public void Dispose() { }

        public void Load(string sceneName)
        {
            _target = sceneName;
            SceneManager.LoadScene(SceneNames.Loading);
        }
    }
}
