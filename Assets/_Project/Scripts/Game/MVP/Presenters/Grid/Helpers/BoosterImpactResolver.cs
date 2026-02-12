using System;
using Core.Handlers;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class BoosterImpactResolver
    {
        public static void ResolveLinearArea(IGridModel model, BoosterActionContext boosterActionContext, BoosterImpactRecord[,] markData, int damageAmount, int lineCount, Vector2Int[] directions, Action<BoosterActionContext> enqueue)
        {
            foreach (var dir in directions)
            {
                VisitLineExceptSelf(model, boosterActionContext.OriginCoord, dir, lineCount, 
                    obj => ResolveVisitedObject(obj, boosterActionContext.GroupId, markData, damageAmount, enqueue));
            }
        }

        public static void ResolveSquareArea(IGridModel model, BoosterActionContext boosterActionContext, BoosterImpactRecord[,] markData, int damageAmount, int radius, Action<BoosterActionContext> enqueue)
        {
            for (int r = 1; r <= radius; r++)
            {
                VisitRingExceptSelf(model, boosterActionContext.OriginCoord, r, 
                    obj => ResolveVisitedObject(obj, boosterActionContext.GroupId, markData, damageAmount, enqueue));
            }
        }
        
        public static void ResolveAllArea(IGridModel model, BoosterActionContext boosterActionContext, BoosterImpactRecord[,] markData, int damageAmount, Action<BoosterActionContext> enqueue)
        {
            var maxRadius = Mathf.Max(model.Width, model.Height);

            for (int r = 1; r <= maxRadius; r++)
            {
                VisitRingExceptSelf(model, boosterActionContext.OriginCoord, r, 
                    obj => ResolveVisitedObject(obj, boosterActionContext.GroupId, markData, damageAmount, enqueue));
            }
        }

        private static void ResolveVisitedObject(BaseGridObject obj, int actionGroupId, BoosterImpactRecord[,] markData, int damageAmount, Action<BoosterActionContext> enqueue)
        {
            var coord = obj.Coord;
            
            ref var cell = ref markData[coord.x, coord.y];
            
            var damageable = obj as IDamageableGridObject;
            var trigger = obj as IBoosterActionSource;
            
            var isDamageable = damageable != null;
            var isTrigger = trigger != null;

            if (isDamageable)
            {
                cell.AddDamage(actionGroupId, damageAmount, GridDamageSource.Booster);
            }

            var shouldTrigger = isTrigger && (!isDamageable || damageable.Life - damageAmount <= 0);
            
            if (shouldTrigger)
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
            var offsets = BuildLineOffsets(lineCount);
            
            if (dir.x != 0)
            {
                foreach (var offset in offsets)
                {
                    var y = origin.y + offset;
                    if (y < 0 || y >= model.Height) continue;
            
                    for (int x = 0; x < model.Width; x++)
                    {
                        var c = new Vector2Int(x, y);
                        if (c == origin) continue;
                
                        var obj = model.GetGridObject(c);
                        if (obj) visit(obj);
                    }
                }
                return;
            }
            
            foreach (var offset in offsets)
            {
                var x = origin.x + offset;
                if (x < 0 || x >= model.Width) continue;
        
                for (int y = 0; y < model.Height; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (c == origin) continue;
            
                    var obj = model.GetGridObject(c);
                    if (obj) visit(obj);
                }
            }
        }

        public static int[] BuildLineOffsets(int lineCount)
        {
            if (lineCount <= 0) return Array.Empty<int>();
    
            var offsets = new int[lineCount];
            offsets[0] = 0;
    
            for (int i = 1; i < lineCount; i++)
            {
                var distance = (i + 1) / 2;
                var sign = (i % 2 == 1) ? 1 : -1;
                offsets[i] = distance * sign;
            }
    
            return offsets;
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