using Core.Item;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ShiftRefillState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;

        private int _pendingAnims;

        private const float ColumnYieldSeconds = 0.01f;
        private const float WaveDelayStep = 0.05f;
        private const float FallDistanceMultiplier = 0.2f;
        private const float SpawnYOffset = 1.25f;

        protected override void OnEnter()
        {
            if (!Context.ResolvedAnyMatch)
            {
                RequestExit();
                return;
            }

            Context.CascadeInProgress = true;
            RunCascade().Forget();
            RequestExit();
        }

        private async UniTask RunCascade()
        {
            _pendingAnims = 0;

            var model = Context.Model;
            var view = Context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                await UniTask.WaitForSeconds(ColumnYieldSeconds);

                int wave = 0;

                ShiftColumn(model, view, x, height, ref wave);

                await UniTask.WaitForSeconds(ColumnYieldSeconds);

                if (!TryGetSpawnY(model, view, x, height, cellSize, out var spawnY))
                {
                    await UniTask.WaitForSeconds(ColumnYieldSeconds);
                    continue;
                }

                RefillColumn(model, view, x, height, cellSize, spawnY, ref wave);

                await UniTask.WaitForSeconds(ColumnYieldSeconds);
            }

            if (_pendingAnims == 0)
                FinishCascade();
        }

        private void ShiftColumn(IGridModel model, IGridView view, int x, int height, ref int wave)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                var dest = new Vector2Int(x, y);

                if (!model.IsCellActive(dest)) continue;
                if (model.GetGridObject(dest)) continue;

                var srcY = model.FindFallSourceY(x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var obj = model.GetGridObject(src);
                if (!obj) continue;

                model.SetGridObject(dest, obj);
                model.SetGridObject(src, null);

                var delay = wave * WaveDelayStep;
                var dist = Mathf.Abs(dest.y - src.y);
                var durMul = 1f + dist * FallDistanceMultiplier;

                StartShiftAnim(obj, view.GridToWorld(dest), durMul, delay);

                wave++;
            }
        }

        private bool TryGetSpawnY(IGridModel model, IGridView view, int x, int height, float cellSize, out float spawnY)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2Int(x, y);
                if (!model.IsCellActive(pos)) continue;

                spawnY = view.GridToWorld(pos).y + cellSize * SpawnYOffset;
                return true;
            }

            spawnY = 0f;
            return false;
        }

        private void RefillColumn(IGridModel model, IGridView view, int x, int height, float cellSize, float spawnY, ref int wave)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);

                if (!model.IsCellActive(pos)) continue;
                if (model.GetGridObject(pos)) continue;

                var typeId = model.GetRandomRegularTypeId();
                var item = Context.Factory.GetItem<ItemObject>(new GridObjectTypeData(GridItemKind.Regular, typeId));

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var target = view.GridToWorld(pos);
                item.SetPosition(new Vector3(target.x, spawnY, target.z));

                model.SetGridObject(pos, item);

                var delay = wave * WaveDelayStep;
                var dist = Mathf.Abs(spawnY - target.y) / cellSize;
                var durMul = 1f + dist * FallDistanceMultiplier;

                StartShiftAnim(item, target, durMul, delay);

                wave++;
            }
        }

        private void StartShiftAnim(BaseItemObject obj, Vector3 target, float durMul, float delay)
        {
            _pendingAnims++;

            var tween = obj.ItemAnimation.Shift(target, durMul, delay);
            if (tween == null)
            {
                OnAnimDone();
                return;
            }

            tween.OnComplete(OnAnimDone);
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

            var model = Context.Model;
            var grid = model.BuildTypeDataGrid();

            Context.CascadeResolveRequested = GridMatchDetectUtil.HasAnyRegularMatchOnBoard(grid, model.Width, model.Height);
        }
    }
}
