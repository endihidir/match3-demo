using System.Collections.Generic;
using Core.Config;
using Core.Configs;
using Core.Item;
using Core.Models;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideDownRefillStrategy : IRefillStrategy
    {
        private readonly RefillSettingsSO _refillSettingsSo;

        private readonly List<UniTask> _animTasks = new(256);
        private UniTask _runningAnimations;

        private readonly List<SlideMovePlan> _plans = new(128);
        private readonly List<Vector2Int> _spawnCoords = new(32);

        private readonly List<MoveAnimRecord> _moveRecords = new(256);
        private readonly Dictionary<BaseGridObject, int> _recordIndexByItem = new(256);

        private int[] _sourceStamp;
        private int[] _destStamp;
        private int _stampId;

        private int[] _colNonSpawnCounts;
        private int[] _colNonSpawnIndex;
        private int[] _colSpawnIndex;

        public SlideDownRefillStrategy(GameplayConfigContainer configContainer)
        {
            _refillSettingsSo = configContainer.ItemConfigContainer.RefillSettingsSo;
        }

        public bool CanRefill(IGridModel model) => GridRefillCalcUtil.HasStationaryAndBlocking(model);

        public IRefillStrategy Execute(GridStateContext context)
        {
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            _animTasks.Clear();
            ClearMoveRecords();

            EnsureStampCapacity(width, height);
            EnsureColumnCapacity(width);

            var outerSafety = width * height * 12;

            while (outerSafety-- > 0)
            {
                var movedAny = MarkAndApplyMoves(model, width, height);
                var spawnedAny = SpawnRefill(context, view, width, height, cellSize);

                if (!movedAny && !spawnedAny) break;
            }

            PlayMoveAnimations(model, view, cellSize, width);

            _runningAnimations = _animTasks.Count > 0 ? UniTask.WhenAll(_animTasks) : UniTask.CompletedTask;
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;

        private void EnsureStampCapacity(int width, int height)
        {
            var size = width * height;

            if (_sourceStamp == null || _sourceStamp.Length != size)
                _sourceStamp = new int[size];

            if (_destStamp == null || _destStamp.Length != size)
                _destStamp = new int[size];
        }

        private void EnsureColumnCapacity(int width)
        {
            if (_colNonSpawnCounts == null || _colNonSpawnCounts.Length != width)
                _colNonSpawnCounts = new int[width];

            if (_colNonSpawnIndex == null || _colNonSpawnIndex.Length != width)
                _colNonSpawnIndex = new int[width];

            if (_colSpawnIndex == null || _colSpawnIndex.Length != width)
                _colSpawnIndex = new int[width];
        }

        private void ClearMoveRecords()
        {
            for (int i = 0; i < _moveRecords.Count; i++)
                ListPool.Release(_moveRecords[i].CoordPath);

            _moveRecords.Clear();
            _recordIndexByItem.Clear();
        }

        private bool MarkAndApplyMoves(IGridModel model, int width, int height)
        {
            var movedAnyOverall = false;

            var safety = width * height * 12;

            while (safety-- > 0)
            {
                _plans.Clear();
                _stampId++;

                MarkVerticalPlans(model, width, height);
                MarkSlidePlans(model, width, height);

                if (_plans.Count == 0)
                    break;

                ApplyPlans(model);

                movedAnyOverall = true;
            }

            return movedAnyOverall;
        }

        private void MarkVerticalPlans(IGridModel model, int width, int height)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var dest = new Vector2Int(x, y);

                    if (!GridRefillCalcUtil.IsEmptyActiveCell(model, dest)) continue;

                    if (!GridRefillCalcUtil.TryFindVerticalSource(model, dest.x, dest.y, out var source)) continue;

                    if (!TryReserve(source, dest, width)) continue;

                    var item = model.GetGridObject(source);
                    if (!item || item.IsStationary) continue;

                    _plans.Add(new SlideMovePlan(source, dest, item, false));
                }
            }
        }

        private void MarkSlidePlans(IGridModel model, int width, int height)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var coord = new Vector2Int(x, y);

                    if (!GridRefillCalcUtil.IsEmptyActiveCell(model, coord)) continue;

                    if (!GridRefillCalcUtil.TryGetBarrierYAbove(model, coord.x, coord.y, out var barrierY)) continue;

                    var topGapY = barrierY + 1;
                    if (topGapY >= height) continue;

                    if (coord.y != topGapY) continue;

                    var topGapCell = new Vector2Int(coord.x, topGapY);

                    if (!GridRefillCalcUtil.IsEmptyActiveCell(model, topGapCell)) continue;

                    var barrierAtTop = GridRefillCalcUtil.IsBarrierAtColumnTop(model, coord.x, barrierY);

                    if (TryMarkSlideFromSide(model, topGapCell, coord.x + 1, barrierY, barrierAtTop, width)) continue;
                    TryMarkSlideFromSide(model, topGapCell, coord.x - 1, barrierY, barrierAtTop, width);
                }
            }
        }

        private bool TryMarkSlideFromSide(IGridModel model, Vector2Int targetCell, int sideX, int sourceY, bool barrierAtTop, int width)
        {
            if (sideX < 0 || sideX >= model.Width) return false;

            var source = new Vector2Int(sideX, sourceY);

            if (!model.IsCellActive(source)) return false;

            if (!TryReserve(source, targetCell, width)) return false;

            var item = model.GetGridObject(source);
            if (!item) return false;

            if (!barrierAtTop && IsSpawnedInThisSim(item)) return false;

            if (item.IsStationary) return false;

            if (GridRefillCalcUtil.CanFallStraightDown(model, source)) return false;

            _plans.Add(new SlideMovePlan(source, targetCell, item, true));
            return true;
        }

        private bool TryReserve(Vector2Int source, Vector2Int dest, int width)
        {
            var sIdx = source.x + source.y * width;
            var dIdx = dest.x + dest.y * width;

            if (_sourceStamp[sIdx] == _stampId) return false;
            if (_destStamp[dIdx] == _stampId) return false;

            _sourceStamp[sIdx] = _stampId;
            _destStamp[dIdx] = _stampId;
            return true;
        }

        private void ApplyPlans(IGridModel model)
        {
            for (int i = 0; i < _plans.Count; i++)
            {
                var plan = _plans[i];

                var current = model.GetGridObject(plan.From);
                if (!current || current != plan.Item) continue;

                var index = GetOrCreateRecordIndex(plan.Item, false);

                var record = _moveRecords[index];

                if (record.CoordPath.Count == 0)
                    record.CoordPath.Add(plan.From);
                else
                {
                    var last = record.CoordPath[record.CoordPath.Count - 1];
                    if (last != plan.From)
                        record.CoordPath.Add(plan.From);
                }

                record.CoordPath.Add(plan.To);

                _moveRecords[index] = record;

                model.SetGridObject(plan.From, null);
                model.SetGridObject(plan.To, plan.Item);
            }
        }

        private bool SpawnRefill(GridStateContext stateContext, IGridView view, int width, int height, float cellSize)
        {
            var spawnedAny = false;

            for (int x = 0; x < width; x++)
            {
                if (!GridRefillCalcUtil.TryGetSpawnCellCoord(stateContext.Model, x, height, out var spawnCellCoord)) continue;

                if (SpawnTopOpenCells(stateContext, view, x, cellSize, spawnCellCoord))
                    spawnedAny = true;
            }

            return spawnedAny;
        }

        private bool SpawnTopOpenCells(GridStateContext stateContext, IGridView view, int x, float cellSize, Vector2Int spawnCellCoord)
        {
            var model = stateContext.Model;

            _spawnCoords.Clear();

            for (int y = spawnCellCoord.y; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;

                _spawnCoords.Add(coord);
            }

            if (_spawnCoords.Count == 0) return false;

            var spawnY = view.GridToWorld(spawnCellCoord).y + cellSize;

            for (int i = 0; i < _spawnCoords.Count; i++)
            {
                var coord = _spawnCoords[i];

                var itemType = SmartSpawnDecider.Decide(model, coord, _refillSettingsSo.SpawnSettings);
                var item = stateContext.Factory.GetRegularItem(itemType);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var targetWorld = view.GridToWorld(coord);
                var startWorld = new Vector3(targetWorld.x, spawnY, targetWorld.z);

                item.SetPosition(startWorld);
                model.SetGridObject(coord, item);

                var index = GetOrCreateRecordIndex(item, true);

                var record = _moveRecords[index];
                record.CoordPath.Clear();
                record.CoordPath.Add(coord);
                _moveRecords[index] = record;
            }

            return true;
        }

        private void PlayMoveAnimations(IGridModel model, IGridView view, float cellSize, int width)
        {
            for (int i = 0; i < width; i++)
            {
                _colNonSpawnCounts[i] = 0;
                _colNonSpawnIndex[i] = 0;
                _colSpawnIndex[i] = 0;
            }

            for (int i = 0; i < _moveRecords.Count; i++)
            {
                var record = _moveRecords[i];

                if (!record.HasPath) continue;

                var last = record.CoordPath[record.CoordPath.Count - 1];

                if (!record.IsSpawned)
                    _colNonSpawnCounts[last.x]++;
            }

            for (int i = 0; i < _moveRecords.Count; i++)
            {
                var record = _moveRecords[i];

                if (!record.HasPath) continue;

                var item = record.Item;
                if (!item) continue;

                var path = record.CoordPath;
                var last = path[path.Count - 1];

                var waveIndex = record.IsSpawned ? _colNonSpawnCounts[last.x] + _colSpawnIndex[last.x]++ : _colNonSpawnIndex[last.x]++;
                var delay = waveIndex * _refillSettingsSo.ShiftDelayMultiplier;

                var isSlide = HasHorizontalStep(path);

                if (isSlide)
                    delay = 0f;

                if (!isSlide)
                {
                    var targetWorld = view.GridToWorld(last);

                    var segDist = Mathf.Abs(item.transform.position.y - targetWorld.y);
                    var distCells = segDist / cellSize;
                    var durMul = 1f + distCells * _refillSettingsSo.ShiftDurationMultiplier;

                    var tween = item.ItemAnimation.Shift(targetWorld, durMul, delay);
                    if (tween == null) continue;

                    _animTasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                    continue;
                }

                Vector3[] worldPoints;

                if (record.IsSpawned)
                {
                    worldPoints = new Vector3[path.Count + 1];
                    worldPoints[0] = item.transform.position;

                    for (int p = 0; p < path.Count; p++)
                        worldPoints[p + 1] = view.GridToWorld(path[p]);
                }
                else
                {
                    worldPoints = new Vector3[path.Count];

                    for (int p = 0; p < path.Count; p++)
                        worldPoints[p] = view.GridToWorld(path[p]);
                }

                var tweenSlide = item.ItemAnimation.ShiftPath(worldPoints, _refillSettingsSo.ShiftDurationMultiplier, delay);
                if (tweenSlide == null) continue;

                _animTasks.Add(tweenSlide.AsyncWaitForCompletion().AsUniTask());
            }
        }

        private static bool HasHorizontalStep(List<Vector2Int> path)
        {
            if (path == null || path.Count < 2) return false;

            for (int i = 1; i < path.Count; i++)
            {
                if (path[i].x != path[i - 1].x)
                    return true;
            }

            return false;
        }

        private bool IsSpawnedInThisSim(BaseGridObject item)
        {
            if (!_recordIndexByItem.TryGetValue(item, out var index))
                return false;

            return _moveRecords[index].IsSpawned;
        }

        private int GetOrCreateRecordIndex(BaseGridObject item, bool isSpawned)
        {
            if (_recordIndexByItem.TryGetValue(item, out var index))
            {
                if (isSpawned)
                {
                    var r = _moveRecords[index];
                    if (!r.IsSpawned)
                    {
                        r.IsSpawned = true;
                        _moveRecords[index] = r;
                    }
                }

                return index;
            }

            index = _moveRecords.Count;
            _recordIndexByItem.Add(item, index);

            var record = new MoveAnimRecord(item, ListPool.Get(), isSpawned);
            _moveRecords.Add(record);

            return index;
        }
    }
}
