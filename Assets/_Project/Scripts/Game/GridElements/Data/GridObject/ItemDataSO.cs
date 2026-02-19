using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Game/Gameplay/Grid/Data/ItemData", order = -1)]
    public class ItemDataSO : BaseGridObjectDataSO
    {
        [field: SerializeField] public Color BlastColor { get; private set; }
    }
}