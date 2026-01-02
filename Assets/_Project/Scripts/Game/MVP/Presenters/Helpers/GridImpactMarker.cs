using System;
using Core.Handlers;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridImpactMarker
    {
        public static void MarkOriginObject(BaseGridObject obj, CellImpactMarkData[,] markData)
        {
            var coord = obj.Coord;
            
            ref var cell = ref markData[coord.x, coord.y];

            if (obj is IBoosterActionSource trigger)
            {
                cell.MarkRemove();
                return;
            }

            if (obj is IDamageableItem)
            {
                cell.AddDamage(1, DamageSource.Booster);
            }
        }
        
        public static void MarkLinearArea(IGridModel model, PendingBoosterAction boosterAction, CellImpactMarkData[,] markData, int damageAmount, int lineCount, Vector2Int[] directions, Action<PendingBoosterAction> enqueue)
        {
            foreach (var dir in directions)
            {
                VisitLineExceptSelf(model, boosterAction.OriginCoord, dir, lineCount, 
                    obj => MarkVisitedObject(obj, markData, damageAmount, enqueue));
            }
        }

        public static void MarkSquareArea(IGridModel model, PendingBoosterAction boosterAction, CellImpactMarkData[,] markData, int damageAmount, int radius, Action<PendingBoosterAction> enqueue)
        {
            for (int r = 1; r <= radius; r++)
            {
                VisitRingExceptSelf(model, boosterAction.OriginCoord, r, 
                    obj => MarkVisitedObject(obj, markData, damageAmount, enqueue));
            }
        }
        
        public static void MarkAllAreaFromOrigin(IGridModel model, PendingBoosterAction boosterAction, CellImpactMarkData[,] markData, int damageAmount, Action<PendingBoosterAction> enqueue)
        {
            var maxRadius = Mathf.Max(model.Width, model.Height);

            for (int r = 1; r <= maxRadius; r++)
            {
                VisitRingExceptSelf(model, boosterAction.OriginCoord, r, obj => MarkVisitedObject(obj, markData, damageAmount, enqueue));
            }
        }

        private static void MarkVisitedObject(BaseGridObject obj, CellImpactMarkData[,] markData, int damageAmount, Action<PendingBoosterAction> enqueue)
        {
            var coord = obj.Coord;
            
            ref var cell = ref markData[coord.x, coord.y];
            
            var isDamageable = obj is IDamageableItem;
            var trigger = obj as IBoosterActionSource;

            if (isDamageable)
            {
                cell.AddDamage(damageAmount, DamageSource.Booster);
            }

            if (trigger != null)
            {
                if (trigger.TryBuildAction(coord, out var boosterAction))
                {
                    enqueue(boosterAction);
                }

                cell.MarkRemove();
                return;
            }

            if (!isDamageable)
            {
                cell.MarkRemove();
            }
        }

        private static void VisitLineExceptSelf(IGridModel model, Vector2Int origin, Vector2Int dir, int lineCount, Action<BaseGridObject> visit)
        {
            if (lineCount <= 0) return;

            var half = (lineCount - 1) / 2;
            var start = (lineCount & 1) == 1 ? -half : 0;
            var end   = (lineCount & 1) == 1 ?  half : lineCount - 1;

            if (dir.x != 0)
            {
                for (int dy = start; dy <= end; dy++)
                {
                    var y = origin.y + dy;

                    for (int x = 0; x < model.Width; x++)
                    {
                        var c = new Vector2Int(x, y);
                        if (c == origin || !model.IsInRange(c)) continue;

                        var obj = model.GetGridObject(c);
                        if (obj) visit(obj);
                    }
                }
                return;
            }

            for (int dx = start; dx <= end; dx++)
            {
                var x = origin.x + dx;

                for (int y = 0; y < model.Height; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (c == origin || !model.IsInRange(c)) continue;

                    var obj = model.GetGridObject(c);
                    if (obj) visit(obj);
                }
            }
        }
        
        private static void VisitRingExceptSelf(IGridModel model, Vector2Int origin, int radius, Action<BaseGridObject> visit)
        {
            if (radius <= 0) return;

            var minX = origin.x - radius;
            var maxX = origin.x + radius;
            var minY = origin.y - radius;
            var maxY = origin.y + radius;

            // Top & Bottom edges
            for (int x = minX; x <= maxX; x++)
            {
                var top = new Vector2Int(x, maxY);
                if (top != origin && model.IsInRange(top))
                {
                    var obj = model.GetGridObject(top);
                    if (obj) visit(obj);
                }

                var bottom = new Vector2Int(x, minY);
                if (bottom != origin && model.IsInRange(bottom))
                {
                    var obj = model.GetGridObject(bottom);
                    if (obj) visit(obj);
                }
            }

            // Left & Right edges (skip corners)
            for (int y = minY + 1; y <= maxY - 1; y++)
            {
                var left = new Vector2Int(minX, y);
                if (left != origin && model.IsInRange(left))
                {
                    var obj = model.GetGridObject(left);
                    if (obj) visit(obj);
                }

                var right = new Vector2Int(maxX, y);
                if (right != origin && model.IsInRange(right))
                {
                    var obj = model.GetGridObject(right);
                    if (obj) visit(obj);
                }
            }
        }

    }
}