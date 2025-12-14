using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<IGridItemObject>
    {
        void Swap(Vector2Int a, Vector2Int b);
    }

    public class GridModel : BaseGridModel<IGridItemObject>, IGridModel
    {
        protected override void OnInitialize()
        {
           
        }

        public void Swap(Vector2Int a, Vector2Int b)
        {
            if (!IsInRange(a) || !IsInRange(b)) return;

            var temp = GetGridObject(a);

            SetGridObject(a, GetGridObject(b));

            SetGridObject(b, temp);
        }

    }
}