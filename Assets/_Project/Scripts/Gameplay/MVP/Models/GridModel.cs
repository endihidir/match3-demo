using System;
using System.Collections.Generic;
using Core.Extensions;
using Core.Item;
using UnityEngine;

namespace Core.Systems
{
    public interface IGridModel
    {
        int Width { get; }
        int Height { get; }
        bool DrawGizmos { get; }

        IGridItemBehaviour GetGridObject(Vector2Int gridPos);
        void SetGridObject(Vector2Int gridPos, IGridItemBehaviour ıtem);

        IGridItemBehaviour GetGridObject(Vector3 worldPos);
        Vector3 GridToWorld(Vector2Int pos);
        Vector2Int WorldToGrid(Vector3 position, bool clamp = true);

        bool IsInRange(Vector2Int pos);
        int GridPositionToIndex(Vector2Int pos);
        Vector2Int IndexToGridPosition(int index);

        void DrawGrid();
        bool TryGetGridObjectFromMousePosition(out IGridItemBehaviour obj);

        bool TryGetNeighbor(Vector2Int pos, Direction2D direction2D, out IGridItemBehaviour neighbour);
        bool TryGetNeighbors(Vector2Int pos, out IGridItemBehaviour[] neighbours);
        bool TryGetNeighborsNonAlloc(Vector2Int pos, Span<IGridItemBehaviour> resultBuffer, out int count);
    }
    
    public class GridModel : IGridModel
    {
        #region VARIABLES

        private Camera _cam = Camera.main;
        
        private int _width, _height;

        private float _screenSidePaddingRatio;
        private float _cellSpacingRatio;

        private IGridItemBehaviour[,] _gridArray;
        private Vector3 _originOffset;

        private bool _drawGizmos;
        private Color _gizmosColor = Color.yellow;

        private float _cellSize;
        
        #endregion

        #region PROPERTIES

        public int Width => _width;
        public int Height => _height;
        public bool DrawGizmos => _drawGizmos;
        
        private static readonly Direction2D[] DirectionList = (Direction2D[])Enum.GetValues(typeof(Direction2D));
        
        private static readonly Dictionary<Direction2D, Vector2Int> DirectionOffsets = new()
        {
            { Direction2D.Right, new Vector2Int( 1,  0) },
            { Direction2D.Left, new Vector2Int(-1,  0) },
            { Direction2D.Up, new Vector2Int( 0, -1) },
            { Direction2D.Down, new Vector2Int( 0,  1) },
            { Direction2D.RightUp, new Vector2Int( 1, -1) },
            { Direction2D.LeftUp, new Vector2Int(-1, -1) },
            { Direction2D.RightDown, new Vector2Int( 1,  1) },
            { Direction2D.LeftDown, new Vector2Int(-1,  1) }
        };

        #endregion

        public void Initialize(IGridItemBehaviour[,] gridItemData)
        {
            _width = gridItemData.GetLength(0);
            _height = gridItemData.GetLength(1);
            
            _gridArray = new IGridItemBehaviour[Width, Height];
            
            for (var i = 0; i < _gridArray.Length; i++)
            {
                var pos = IndexToGridPosition(i);
                
                _gridArray[pos.x, pos.y] = gridItemData[pos.x, pos.y];
            }
            
            CalculateCellSize();
        }
        
        // TODO : Make these settings SO config file
        public GridModel SetScreenSidePaddingRatio(float value)
        {
            _screenSidePaddingRatio = value;
            return this;
        }
        
        public GridModel SetCellSpacingRatio(float value)
        {
            _cellSpacingRatio = value;
            return this;
        }

        public GridModel SetOriginOffset(Vector3 value)
        {
            _originOffset = value;
            return this;
        }

        public GridModel EnableDrawGizmos(bool value)
        {
            _drawGizmos = value;
            return this;
        }

        public GridModel SetGizmosColor(Color value)
        {
            _gizmosColor = value;
            return this;
        }

        private void CalculateCellSize()
        {
            var screenWidth = GetScreenWidth();
            var screenHeight = GetScreenHeight();

            var borderOffsetW = screenWidth  * (_screenSidePaddingRatio / 100f);
            var gridOffsetW = screenWidth  * (_cellSpacingRatio / 100f);
            var gridOffsetH = screenHeight * (_cellSpacingRatio / 100f);

            var cellSizeW = (screenWidth - borderOffsetW - (gridOffsetW * (Width - 1))) / Width;
            var totalGridHeight = (cellSizeW * Height) + (gridOffsetH * (Height - 1));
            
            var maxWorldHeight = screenHeight * 0.9f;

            if (totalGridHeight > maxWorldHeight)
            {
                _cellSize = (maxWorldHeight - (gridOffsetH * (Height - 1))) / Height;
            }
            else
            {
                _cellSize = cellSizeW;
            }
        }
        
        public Vector3 GridToWorld(Vector2Int pos)
        {
            if (!IsInRange(pos)) return Vector3.zero;

            var screenWidth = GetScreenWidth();
            var screenHeight = GetScreenHeight();

            var gridOffsetX = screenWidth  * (_cellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (_cellSpacingRatio / 100f);

            var totalGridWidth = (Width * _cellSize) + ((Width - 1) * gridOffsetX);
            var leftStartX = GetLeftX() + ((screenWidth - totalGridWidth) * 0.5f);

            var x = leftStartX + (pos.x * (_cellSize + gridOffsetX)) + (_cellSize * 0.5f);
            var y = GetTopY()  - (pos.y * (_cellSize + gridOffsetY)) - (_cellSize * 0.5f);

            return new Vector3(x, y, 0f);
        }
        
        public Vector2Int WorldToGrid(Vector3 worldPos, bool clamp = true)
        {
            var position = new Vector2Int(-1, -1);
            
            var screenWidth = GetScreenWidth();
            var screenHeight = GetScreenHeight();

            var gridOffsetX = screenWidth  * (_cellSpacingRatio / 100f);
            var gridOffsetY = screenHeight * (_cellSpacingRatio / 100f);

            var totalGridWidth = (Width * _cellSize) + ((Width - 1) * gridOffsetX);
            var leftStartX = GetLeftX() + ((screenWidth - totalGridWidth) * 0.5f);
            
            var absXFromGrid = worldPos.x - leftStartX;
            var absYFromTop = GetTopY() - worldPos.y;

            var dividerX = _cellSize + gridOffsetX;
            var dividerY = _cellSize + gridOffsetY;
            
            if ((absXFromGrid % dividerX) > _cellSize || (absYFromTop % dividerY) > _cellSize)
            {
                return position;
            }

            var gx = Mathf.FloorToInt(absXFromGrid / dividerX);
            var gy = Mathf.FloorToInt(absYFromTop  / dividerY);

            if (clamp)
            {
                gx = Mathf.Clamp(gx, 0, Width  - 1);
                gy = Mathf.Clamp(gy, 0, Height - 1);
            }

            position.x = gx;
            position.y = gy;
            return position;
        }

        public IGridItemBehaviour GetGridObject(Vector2Int pos)
        {
            if (!IsInRange(pos)) return default;
            return _gridArray[pos.x, pos.y];
        }
        
        public bool TryGetGridObjectFromMousePosition(out IGridItemBehaviour obj)
        {
            var worldPosition = _cam
                .ScreenToWorldPoint(Input.mousePosition)
                .With(z: _cam.nearClipPlane);
            
            var pos = WorldToGrid(worldPosition, false);
            
            if (!IsInRange(pos))
            {
                obj = default;
                return false;
            }

            obj = _gridArray[pos.x, pos.y];
            return true;
        }

        public IGridItemBehaviour GetGridObject(Vector3 worldPos)
        {
            var pos = WorldToGrid(worldPos);
            if (!IsInRange(pos)) return default;
            return _gridArray[pos.x, pos.y];
        }

        public void SetGridObject(Vector2Int pos, IGridItemBehaviour value)
        {
            if (!IsInRange(pos)) return;
            _gridArray[pos.x, pos.y] = value;
        }

        public bool TryGetNeighbor(Vector2Int pos, Direction2D direction2D, out IGridItemBehaviour neighbour)
        {
            neighbour = default;
            
            if (direction2D == Direction2D.None)
            {
                if (IsInRange(pos))
                {
                    neighbour = _gridArray[pos.x, pos.y];
                    return !EqualityComparer<IGridItemBehaviour>.Default.Equals(neighbour, default);
                }
                
                return false;
            }
            
            if (!DirectionOffsets.TryGetValue(direction2D, out var offset)) return false;
            
            var newPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);
            
            if (IsInRange(newPos))
            {
                neighbour = _gridArray[newPos.x, newPos.y];
                return !EqualityComparer<IGridItemBehaviour>.Default.Equals(neighbour, default);
            }

            return false;
        }

        public bool TryGetNeighbors(Vector2Int pos, out IGridItemBehaviour[] neighbours)
        {
            if (!IsInRange(pos))
            {
                neighbours = Array.Empty<IGridItemBehaviour>();
                return false;
            }

            var result = new List<IGridItemBehaviour>();

            foreach (var direction in DirectionList)
            {
                if (direction == Direction2D.None) continue;

                if (TryGetNeighbor(pos, direction, out var neighbour))
                {
                    result.Add(neighbour);
                }
            }

            neighbours = result.ToArray();
            return neighbours.Length > 0;
        }
        
        public bool TryGetNeighborsNonAlloc(Vector2Int pos, Span<IGridItemBehaviour> resultBuffer, out int count)
        {
            count = 0;

            if (!IsInRange(pos)) return false;

            foreach (var direction in DirectionList)
            {
                if (direction == Direction2D.None) continue;
                
                if (!TryGetNeighbor(pos, direction, out var neighbour)) continue;
                if (EqualityComparer<IGridItemBehaviour>.Default.Equals(neighbour, default)) continue;

                if (count >= resultBuffer.Length) return true;

                resultBuffer[count++] = neighbour;
            }

            return count > 0;
        }

        public int GridPositionToIndex(Vector2Int pos) => (pos.y * Width) + pos.x;

        public Vector2Int IndexToGridPosition(int index)
        {
            var x = index % Width;
            var y = Mathf.FloorToInt(index / (float)Width);
            return new Vector2Int(x, y);
        }
        
        public void DrawGrid()
        {
            if(!DrawGizmos) return;
    
            Gizmos.color = _gizmosColor;
            
            var cellCount = Width * Height;

            for (int i = 0; i < cellCount; i++)
            {
                var pos    = IndexToGridPosition(i);
                var center = GridToWorld(pos);
                var half   = _cellSize * 0.5f;
                
                var topLeft = new Vector3(center.x - half, center.y + half, center.z);
                var topRight = new Vector3(center.x + half, center.y + half, center.z);
                var bottomRight = new Vector3(center.x + half, center.y - half, center.z);
                var bottomLeft = new Vector3(center.x - half, center.y - half, center.z);
                
                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }
        
        public bool IsInRange(Vector2Int pos) => pos is { x: >= 0, y: >= 0 } && pos.x < Width && pos.y < Height;

        private float GetScreenWidth() => Mathf.Abs(GetLeftX() - GetRightX());
        private float GetScreenHeight() => Mathf.Abs(GetTopY() - GetBottomY());
        private float GetRightX() => GetOriginPos(Vector3.right).x;
        private float GetTopY() => GetOriginPos(Vector3.up).y;
        private float GetLeftX() => GetOriginPos(Vector3.zero).x;
        private float GetBottomY() => GetOriginPos(Vector3.zero).y;
        private Vector3 GetOriginPos(Vector3 origin) => _cam.ViewportToWorldPoint(origin.With(z: _cam.nearClipPlane)) + 
                                                        new Vector3(_originOffset.x, -_originOffset.y, 0f);
    }
    
    public enum Direction2D
    {
        None,
        Self,
        Up,
        Down,
        Right,
        Left,
        LeftDown,
        LeftUp,
        RightUp,
        RightDown
    }
}