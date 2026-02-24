using System;
using Game.Grid.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "Game/Gameplay/Grid/Data/ObstacleData")]
    public class ObstacleDataSO : BaseGridObjectDataSO
    {
        [field: SerializeField]  public bool IsStationary { get; private set; }
        [field: SerializeField] public bool IsCollectible { get; private set; }
        [field: SerializeField, HideIf(nameof(HasCrackedSprites))] private int Life { get; set; }
        [field: SerializeField] public GridDamageSource GridDamageSource { get; private set; }
        [field: SerializeField] public Sprite[] CrackedSprites { get; private set; } = Array.Empty<Sprite>();
        [field: SerializeField] public Sprite[] ShatteredSprites { get; private set; } = Array.Empty<Sprite>();
        
        private bool HasCrackedSprites => CrackedSprites is { Length: > 0 };
        public int GetLife() => HasCrackedSprites ? CrackedSprites.Length + 1 : Life;
    }
}