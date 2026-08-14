using UnityEngine.SceneManagement;

namespace Runtime.Core
{
    public static class SceneLoader
    {
        public static string Target { get; private set; }

        public static void Load(string sceneName)
        {
            Target = sceneName;
            SceneManager.LoadScene(SceneNames.Loading);
        }
    }
}
