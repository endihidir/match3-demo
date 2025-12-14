using UnityEngine;

namespace Core.Item
{
    public interface IGridItemObject : IItemObjectReader, IItemTypeReader
    {
        IItemAnimation Animation { get; }
        void SetPosition(Vector3 position);
        void SetCellSize(float cellSize);
        void SetParent(Transform parent);
        void UpdateTypeData(GridItemKind gridItemKind, int typeId);
        void UpdateCoordinate(Vector2Int coordinate);
    }
}