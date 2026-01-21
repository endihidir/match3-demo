using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<BaseGridObject>
    {
        void Swap(Vector2Int sourceCoord, Vector2Int targetCoord);
        GridObjectType[,] BuildGridTypeData();
    }
}