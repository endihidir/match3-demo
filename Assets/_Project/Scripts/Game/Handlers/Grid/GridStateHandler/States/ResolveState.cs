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
            IsExitReady = false;

            Context.CascadeResolveRequested = false;

            var typeGrid = Context.Model.BuildTypeDataGrid();
            var removeMask = GridMatchDetectUtil.BuildMatchMask(typeGrid, Context.Model.Width, Context.Model.Height, out var anyMatch);

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

                        Context.Factory.ReleaseItem(obj);
                        Context.Model.SetGridObject(pos, null);
                    }
                }
            }

            IsExitReady = true;
            RequestExit();
        }
    }
}