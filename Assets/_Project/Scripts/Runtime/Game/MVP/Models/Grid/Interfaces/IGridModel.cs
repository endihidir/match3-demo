using Core.Models;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Models
{
    public interface IGridModel : IBaseGridModel<BaseGridObject>
    {
        void Swap(Vector2Int sourceCoord, Vector2Int targetCoord);
        GridObjectType[,] BuildGridTypeData();
        GridObjectType[] BuildGridTypeDataArray();
    }
}