using Game.Grid.Item;
using Game.Models;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public interface IFillItemDecider
    {
        float SafetyBoost { get; }
        ItemType Decide(IGridModel model, Vector2Int targetCoord);
        public void SetSeed(int seed);
        public void ClearSeed();
    }
}