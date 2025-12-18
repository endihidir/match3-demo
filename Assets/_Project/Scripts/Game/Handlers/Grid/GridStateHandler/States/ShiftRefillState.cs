using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ShiftRefillState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;

        private int _pendingAnims;

        private const float WaveDelayStep = 0.05f;
        private const float FallDistanceMultiplier = 0.2f;

        protected override void OnEnter()
        {
            IsExitReady = false;

            if (!Context.ResolvedAnyMatch)
            {
                IsExitReady = true;
                RequestExit();
                return;
            }

            Context.CascadeInProgress = true;
            StartShiftAndRefill();

            IsExitReady = true;
            RequestExit();
        }

        private void StartShiftAndRefill()
        {
            _pendingAnims = 0;

            var cellSize = Context.View.GetCellSize();

            for (int x = 0; x < Context.Model.Width; x++)
            {
                int wave = 0;

                for (int y = Context.Model.Height - 1; y >= 0; y--)
                {
                    var dest = new Vector2Int(x, y);

                    if (!Context.Model.IsCellActive(dest)) continue;
                    if (Context.Model.GetGridObject(dest)) continue;

                    var srcY = Context.Model.FindFallSourceY(x, y - 1);
                    if (srcY < 0) continue;

                    var src = new Vector2Int(x, srcY);
                    var obj = Context.Model.GetGridObject(src);
                    if (!obj) continue;

                    Context.Model.SetGridObject(dest, obj);
                    Context.Model.SetGridObject(src, null);

                    _pendingAnims++;

                    var delay = wave * WaveDelayStep;
                    var dist = Mathf.Abs(dest.y - src.y);
                    var durMul = 1f + dist * FallDistanceMultiplier;

                    var tween = obj.ItemAnimation.Shift(Context.View.GridToWorld(dest), durMul, delay);
                    
                    tween.OnComplete(OnAnimDone);
                    
                    wave++;
                }

                float spawnY = 0f;
                bool found = false;

                for (int y = 0; y < Context.Model.Height; y++)
                {
                    if (!Context.Model.IsCellActive(new Vector2Int(x, y))) continue;

                    spawnY = Context.View.GridToWorld(new Vector2Int(x, y)).y + cellSize * 1.25f;
                    found = true;
                    break;
                }

                if (!found) continue;

                for (int y = Context.Model.Height - 1; y >= 0; y--)
                {
                    var pos = new Vector2Int(x, y);

                    if (!Context.Model.IsCellActive(pos)) continue;
                    if (Context.Model.GetGridObject(pos)) continue;

                    var typeId = Context.Model.GetRandomRegularTypeId();
                    var item = Context.Factory.GetItem<ItemObject>(new GridObjectTypeData(GridItemKind.Regular, typeId));

                    item.SetParent(Context.View.GridObjectsParent);
                    item.SetSpriteSize(Vector2.one * cellSize);

                    var target = Context.View.GridToWorld(pos);
                    item.SetPosition(new Vector3(target.x, spawnY, target.z));

                    Context.Model.SetGridObject(pos, item);

                    _pendingAnims++;

                    var dist = Mathf.Abs(spawnY - target.y) / cellSize;
                    var durMul = 1f + dist * FallDistanceMultiplier;

                    var tween = item.ItemAnimation.Shift(target, durMul, wave * WaveDelayStep);

                    tween.OnComplete(OnAnimDone);
                    
                    wave++;
                }
            }

            if (_pendingAnims == 0)
                FinishCascade();
        }

        private void OnAnimDone()
        {
            _pendingAnims--;
            if (_pendingAnims > 0) return;

            FinishCascade();
        }

        private void FinishCascade()
        {
            Context.CascadeInProgress = false;

            var grid = Context.Model.BuildTypeDataGrid();
            Context.CascadeResolveRequested =
                GridMatchDetectUtil.HasAnyRegularMatchOnBoard(
                    grid,
                    Context.Model.Width,
                    Context.Model.Height
                );
        }
    }
}
