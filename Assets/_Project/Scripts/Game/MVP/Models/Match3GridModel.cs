using Core.Item;
using Core.Level;
using Core.Utils;
using UnityEngine;

namespace Core.Models
{
    public interface IMatch3GridModel : IGridModel<IGridItemState>
    {
        bool[,] ActiveData { get; }
        void FillActiveStatus(GridObjectTypeData[,] gridObjectTypeData);
        void Swap(Vector2Int a, Vector2Int b);
    }
    
    public class Match3GridModel : GridModel<IGridItemState>, IMatch3GridModel
    {
        public bool[,] ActiveData { get; private set; }

        public void FillActiveStatus(GridObjectTypeData[,] gridObjectTypeData)
        {
            ActiveData = new bool[Width, Height];
            
            for (int i = 0; i < Width * Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, Width);

                var x = coordinate.x;
                var y = coordinate.y;
                
                var typeData = gridObjectTypeData[x, y];
                
                ActiveData[x, y] = typeData.gridItemKind != GridItemKind.Regular || typeData.typeId != 0;
            }
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