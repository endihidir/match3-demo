using System.Collections.Generic;
using Core.Config;
using Core.Configs;
using Core.Item;
using Core.Models;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideDownRefillStrategy : IRefillStrategy
    {
        private readonly List<SlideMoveRecord> _records = new(256);
        private readonly RefillSettingsSO _refillSettingsSo;
        private UniTask _runningAnimations = UniTask.CompletedTask;

        public SlideDownRefillStrategy(GameplayConfigContainer configContainer) => _refillSettingsSo = configContainer.ItemConfigContainer.RefillSettingsSo;

        public bool CanRefill(IGridModel model) => GridRefillCalcUtil.HasStationaryAndBlocking(model);

         public IRefillStrategy Execute(GridStateContext context)
        {
            _records.Clear();

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;
            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                var wave = 0;

                ShiftColumnLogic(context, x, height, cellSize, ref wave, _records);

                if (GridRefillCalcUtil.TryGetSpawnCellCoord(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    RefillColumnLogic(context, x, height, cellSize, spawnY, ref wave, _records);
                }
            }

            _runningAnimations = PlayAnimations(_records);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;
        
         private void ShiftColumnLogic(GridStateContext stateContext, int x, int height, float cellSize, ref int wave, List<SlideMoveRecord> records)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;

                var srcY = GridRefillCalcUtil.FindFallSourceY(model, x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var obj = model.GetGridObject(src);

                if (!obj) continue;
                
                if(obj.IsStationary) continue;

                var startWorld = obj.transform.position;
                var finalWorld = view.GridToWorld(coord);

                model.SetGridObject(coord, obj);
                model.SetGridObject(src, null);

                var dist = Mathf.Abs(finalWorld.y - startWorld.y) / cellSize;
                var durMul = 1f + dist * _refillSettingsSo.ShiftDurationMultiplier;
                var delay = wave * _refillSettingsSo.ShiftDelayMultiplier;

                records.Add(new SlideMoveRecord(obj, default, finalWorld, durMul, delay));
                wave++;
            }
        }

        private void RefillColumnLogic(GridStateContext stateContext, int x, int height, float cellSize, float spawnY, ref int wave, List<SlideMoveRecord> records)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            // Collect stationary Ys in this column (bottom -> top)
            var stationaryYs = new List<int>(4);
            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);
                if (!model.IsCellActive(c)) continue;

                var o = model.GetGridObject(c);
                if (o && o.IsStationary) stationaryYs.Add(y);
            }

            // Slide: process each stationary band separately so upper stationary donor never fills lower stationary band.
            // Band definition: cells with y in (stationaryY, nextStationaryY)  (above this stationary, below next stationary)
            for (int i = 0; i < stationaryYs.Count; i++)
            {
                var stationaryY = stationaryYs[i];
                var nextStationaryY = (i + 1 < stationaryYs.Count) ? stationaryYs[i + 1] : height;

                var finalCoord = new Vector2Int(int.MinValue, int.MinValue);
                var slideCoord = new Vector2Int(int.MaxValue, int.MaxValue);

                // Find empties in this band
                for (int y = stationaryY + 1; y < nextStationaryY; y++)
                {
                    var c = new Vector2Int(x, y);

                    if (!model.IsCellActive(c)) continue;
                    if (model.GetGridObject(c)) continue;

                    if (y > finalCoord.y) finalCoord = c;
                    if (y < slideCoord.y) slideCoord = c;
                }

                if (finalCoord.x == int.MinValue) continue;
                if (slideCoord.x == int.MaxValue) continue;

                var leftDonorCoord = new Vector2Int(x - 1, stationaryY);
                var rightDonorCoord = new Vector2Int(x + 1, stationaryY);

                BaseGridObject donorObj = null;
                Vector2Int donorCoord = default;

                if (model.IsInRange(leftDonorCoord))
                {
                    var left = model.GetGridObject(leftDonorCoord);
                    if (left && !left.IsStationary && !left.IsShiftInProgress) { donorObj = left; donorCoord = leftDonorCoord; }
                }

                if (!donorObj && model.IsInRange(rightDonorCoord))
                {
                    var right = model.GetGridObject(rightDonorCoord);
                    if (right && !right.IsStationary && !right.IsShiftInProgress) { donorObj = right; donorCoord = rightDonorCoord; }
                }

                if (!donorObj) continue;

                // Move donor -> final
                model.SetGridObject(finalCoord, donorObj);
                model.SetGridObject(donorCoord, null);
                
                var finalWorld = view.GridToWorld(finalCoord);
                var slidePos = view.GridToWorld(slideCoord);

                records.Add(new SlideMoveRecord(donorObj, slidePos, finalWorld, 1.2f, 0.1f));
            }

            // Spawn: do NOT spawn into any cell that is above at least one stationary in this column (slide bands).
            var blockSpawn = new bool[height];
            var seenStationary = false;

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                if (!model.IsCellActive(c)) continue;

                var o = model.GetGridObject(c);
                if (o && o.IsStationary) seenStationary = true;

                blockSpawn[y] = seenStationary && y > 0;
            }

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;
                if (blockSpawn[y]) continue;

                var itemType = SmartSpawnDecider.Decide(model, coord, _refillSettingsSo.SpawnSettings);
                var item = stateContext.Factory.GetRegularItem(itemType);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var target = view.GridToWorld(coord);
                var start = new Vector3(target.x, spawnY, target.z);

                item.SetPosition(start);
                model.SetGridObject(coord, item);

                var dist = Mathf.Abs(target.y - start.y) / cellSize;
                var durMul = 1f + dist * _refillSettingsSo.ShiftDurationMultiplier;
                var delay = wave * _refillSettingsSo.ShiftDelayMultiplier;

                records.Add(new SlideMoveRecord(item, default, target, durMul, delay));
                wave++;
            }
        }

        private UniTask PlayAnimations(List<SlideMoveRecord> records)
        {
            var count = 0;

            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].Obj) count++;
            }

            if (count == 0)
                return UniTask.CompletedTask;

            var tcs = new UniTaskCompletionSource();
            var remaining = count;

            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                if (!r.Obj) continue;

                Tween tween;

                if (r.SlidePos != default)
                    tween = r.Obj.ItemAnimation.SlideAndShift(r.SlidePos, r.FinalWorld, r.DurMul, r.Delay);
                else
                    tween = r.Obj.ItemAnimation.Shift(r.FinalWorld, r.DurMul, r.Delay);

                var done = false;

                tween.OnComplete(CompleteOne);
                tween.OnKill(CompleteOne);
                continue;

                void CompleteOne()
                {
                    if (done) return;
                    done = true;

                    remaining--;
                    if (remaining <= 0)
                        tcs.TrySetResult();
                }
            }

            return tcs.Task;
        }
    }
}