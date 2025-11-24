using Core.Configs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "MainConfigContainer", menuName = "Match3/Core/MainConfigContainer", order = 0)]
    public class AppConfigContainer : ScriptableObject
    {
        public SceneLoadServiceConfig sceneLoadServiceConfig;
        public PoolServiceConfig poolServiceConfig;
        
        public void Initialize()
        {
            
        }
    }
}