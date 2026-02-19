using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "PoolServiceConfig", menuName = "Game/App/Services/PoolServiceConfig")]
    public sealed class PoolServiceConfigSO : ScriptableObject
    {
        [field: SerializeField] public PooledAssetConfigSO[] PooledAssets { get; private set; }
    }
}