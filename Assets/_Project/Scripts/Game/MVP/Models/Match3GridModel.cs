using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IMatch3GridModel : IGridModel<IGridItemBehaviour>
    {
        void Swap(Vector2Int a, Vector2Int b);
    }
    
    public class Match3GridModel : GridModel<IGridItemBehaviour>, IMatch3GridModel
    {
        public void Swap(Vector2Int a, Vector2Int b)
        {
            if (!IsInRange(a) || !IsInRange(b)) return;

            var temp = GetGridObject(a);
            
            SetGridObject(a, GetGridObject(b));
            
            SetGridObject(b, temp);
        }
    }
}