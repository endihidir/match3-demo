using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "MainConfigContainer", menuName = "Match3/Core/MainConfigContainer", order = 0)]
    public class AppConfigContainer : ScriptableObject
    {
        public SceneLoadServiceConfig sceneLoadServiceConfig;
        public PoolServiceConfig poolServiceConfig;
        public LevelDataServiceConfig levelDataServiceConfig;
        
        public void Initialize()
        {
            
        }
    }
}