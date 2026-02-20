using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public interface IFillItemDecider
    {
        float SafetyBoost { get; }
        ItemType Decide(Vector2Int targetCoord);
        public void SetSeed(int seed);
        public void ClearSeed();
    }
}