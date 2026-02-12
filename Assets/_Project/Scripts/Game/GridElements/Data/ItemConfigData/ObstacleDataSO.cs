using System;
using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "Match3/ItemConfigs/Data/ObstacleData", order = -1)]
    public class ObstacleDataSO : BaseItemDataSO
    {
        [field: SerializeField, HideIf(nameof(HasCrackedSprites))] private int Life { get; set; }
        [field: SerializeField] public GridDamageSource GridDamageSource { get; private set; }
        [field: SerializeField] public bool IsCollectible { get; private set; }
        [field: SerializeField] public Sprite[] CrackedSprites { get; private set; } = Array.Empty<Sprite>();
        [field: SerializeField] public Sprite[] ShatteredSprites { get; private set; } = Array.Empty<Sprite>();
        
        private bool HasCrackedSprites => CrackedSprites is { Length: > 0 };
        public int GetLife() => HasCrackedSprites ? CrackedSprites.Length + 1 : Life;
    }
}