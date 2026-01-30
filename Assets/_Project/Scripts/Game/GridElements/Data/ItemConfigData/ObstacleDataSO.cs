using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "Match3/ItemConfigs/Data/ObstacleData", order = -1)]
    public class ObstacleDataSO : BaseItemDataSO
    {
        [field: SerializeField, HideIf(nameof(HasBrokenSprites))] private int Life { get; set; }
        [field: SerializeField] public DamageSource DamageSource { get; private set; }
        [field: SerializeField] public bool IsCollectible { get; private set; }
        [field: SerializeField] public Sprite[] BrokenSprites { get; private set; }
        
        private bool HasBrokenSprites => BrokenSprites is { Length: > 0 };
        public int GetLife() => HasBrokenSprites ? BrokenSprites.Length + 1 : Life;
    }
}