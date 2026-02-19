using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "PoolServiceConfig", menuName = "Game/App/Services/PoolServiceConfig")]
    public class PoolServiceConfigSO : ScriptableObject
    {
        [field: SerializeField] public PooledAssetConfig[] PooledAssets { get; private set; }
    }
}