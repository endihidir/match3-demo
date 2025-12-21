using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ResolveState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;

        protected override void OnEnter()
        {
            Context.CascadeResolveRequested = false;

            var typeGrid = Context.Model.BuildTypeDataGrid();
            var removeMask = GridMatchDetectUtil.BuildMatchMaskFast(typeGrid, Context.Model.Width, Context.Model.Height, out var anyMatch);

            Context.ResolvedAnyMatch = anyMatch;

            if (anyMatch)
            {
                for (int y = 0; y < Context.Model.Height; y++)
                {
                    for (int x = 0; x < Context.Model.Width; x++)
                    {
                        if (!removeMask[x, y]) continue;

                        var pos = new Vector2Int(x, y);

                        var obj = Context.Model.GetGridObject(pos);
                        if (!obj) continue;

                        foreach (var direction in DirectionUtil.MainDirections)
                        {
                            if (!Context.Model.TryGetNeighbour(pos, direction, out var itemObject)) continue;
                            
                            if (itemObject is IDamageableItem damageableItem)
                            {
                                damageableItem.TakeDamage(1, ()=>
                                {
                                    Context.Factory.ReleaseItem(itemObject);
                                    var obstacleItemPos = pos + direction;
                                    Context.Model.SetGridObject(obstacleItemPos, null);
                                });
                            }
                        }

                        Context.Factory.ReleaseItem(obj);
                        Context.Model.SetGridObject(pos, null);
                    }
                }
            }
            
            RequestExit();
        }
    }
}