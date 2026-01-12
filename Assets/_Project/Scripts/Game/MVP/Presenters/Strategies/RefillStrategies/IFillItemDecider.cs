using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Handlers
{
    public interface IFillItemDecider
    {
        ItemType Decide(IGridModel model, Vector2Int targetCoord);
        public void SetSeed(int seed);
        public void ClearSeed();
    }
}