using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "AppConfigContainer", menuName = "Game/Containers/AppConfigContainer")]
    public class AppConfigContainerSO : ScriptableObject
    {
        [field: SerializeField] public SceneLoadServiceConfigSO SceneLoadServiceConfig { get; private set; }
        [field: SerializeField] public PoolServiceConfigSO PoolServiceConfig { get; private set; }
        [field: SerializeField] public LevelDataServiceConfigSO LevelDataServiceConfig { get; private set; }
    }
}