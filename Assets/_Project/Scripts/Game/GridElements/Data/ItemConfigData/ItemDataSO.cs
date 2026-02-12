using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Match3/ItemConfigs/Data/ItemData", order = -1)]
    public class ItemDataSO : BaseItemDataSO
    {
        [field: SerializeField] public Color BlastColor { get; private set; }
    }
}