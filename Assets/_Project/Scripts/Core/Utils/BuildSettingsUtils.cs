using System.IO;
using UnityEngine.SceneManagement;

namespace Core.Utils
{
    public static class BuildSettingsUtils
    {
        public static string GetFirstBuildSceneName()
        {
            if (SceneManager.sceneCountInBuildSettings <= 0)
                return null;

            string path = SceneUtility.GetScenePathByBuildIndex(0);
            
            if (string.IsNullOrEmpty(path))
                return null;

            return Path.GetFileNameWithoutExtension(path);
        }
        
        public static string GetBuildSceneName(int index)
        {
            if (SceneManager.sceneCountInBuildSettings <= 0)
                return null;

            string path = SceneUtility.GetScenePathByBuildIndex(index);
            
            if (string.IsNullOrEmpty(path))
                return null;

            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetFirstBuildScenePath()
        {
            if (SceneManager.sceneCountInBuildSettings <= 0)
                return null;

            return SceneUtility.GetScenePathByBuildIndex(0);
        }
    }
}