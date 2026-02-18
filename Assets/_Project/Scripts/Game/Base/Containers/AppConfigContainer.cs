using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "MainConfigContainer", menuName = "Match3/Core/MainConfigContainer", order = 0)]
    public class AppConfigContainer : ScriptableObject
    {
        [field: SerializeField] public SceneLoadServiceConfig SceneLoadServiceConfig { get; private set; }
        [field: SerializeField] public PoolServiceConfig PoolServiceConfig { get; private set; }
        [field: SerializeField] public LevelDataServiceConfig LevelDataServiceConfig { get; private set; }
    }
}