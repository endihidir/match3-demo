using System;
using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<BaseItemObject>
    {
        bool TryGet<T>(Vector2Int pos, out T value) where T : BaseItemObject;
        T Get<T>(Vector2Int pos) where T : BaseItemObject => !TryGet<T>(pos, out var value) ? null : value;

        void Swap(Vector2Int a, Vector2Int b);

        bool IsCellActive(Vector2Int pos);
        int FindFallSourceY(int x, int startY);
        GridObjectTypeData[,] BuildTypeDataGrid();

        int GetRandomRegularTypeId();
    }

    public class GridModel : BaseGridModel<BaseItemObject>, IGridModel
    {
        private static readonly int[] RegularTypeIds = BuildRegularTypeIds();

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

        public bool IsCellActive(Vector2Int pos)
        {
            if (!IsInRange(pos)) return false;
            return ActiveCells[pos.x, pos.y];
        }

        public int FindFallSourceY(int x, int startY)
        {
            if (x < 0 || x >= Width) return -1;

            for (int yy = startY; yy >= 0; yy--)
            {
                if (!ActiveCells[x, yy]) continue;

                var obj = GetGridObject(new Vector2Int(x, yy));
                if (obj) return yy;
            }

            return -1;
        }

        public GridObjectTypeData[,] BuildTypeDataGrid()
        {
            var grid = new GridObjectTypeData[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var obj = GetGridObject(new Vector2Int(x, y));
                    grid[x, y] = obj ? obj.TypeData : default;
                }
            }

            return grid;
        }

        public int GetRandomRegularTypeId()
        {
            if (RegularTypeIds.Length == 1) return RegularTypeIds[0];
            return RegularTypeIds[UnityEngine.Random.Range(0, RegularTypeIds.Length)];
        }

        private static int[] BuildRegularTypeIds()
        {
            var values = (ItemType[])Enum.GetValues(typeof(ItemType));
            var list = new List<int>();

            foreach (var v in values)
            {
                var id = (int)v;
                if (id <= 0) continue;
                list.Add(id);
            }

            return list.Count == 0 ? new[] { 1 } : list.ToArray();
        }
    }
}