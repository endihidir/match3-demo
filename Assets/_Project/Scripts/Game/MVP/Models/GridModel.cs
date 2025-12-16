using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<BaseItemObject>
    {
        bool TryGet<T>(Vector2Int pos, out T value) where T : BaseItemObject;
        T Get<T>(Vector2Int pos) where T : BaseItemObject => !TryGet<T>(pos, out var value) ? null : value;
        void Swap(Vector2Int a, Vector2Int b);
    }

    public class GridModel : BaseGridModel<BaseItemObject>, IGridModel
    {
        protected override void OnInitialize()
        {
           
        }
        
        public bool TryGet<T>(Vector2Int pos, out T value) where T : BaseItemObject
        {
            value = null;

            if (!IsInRange(pos)) return false;

            var obj = GetGridObject(pos);
            if (obj is not T typed) return false;

            value = typed;
            return true;
        }

        public void Swap(Vector2Int a, Vector2Int b)
        {
            if (!IsInRange(a) || !IsInRange(b)) return;

            var objA = GetGridObject(a);
            var objB = GetGridObject(b);

            SetGridObject(a, objB);
            SetGridObject(b, objA);
        }
        
    }
}