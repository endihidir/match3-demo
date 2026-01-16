using System;
using System.Collections.Generic;
using Core.Item;
using Core.Models;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideDownFillStrategy : IFillStrategy
    {
        private readonly IFillItemDecider _itemDecider;
        private readonly IShiftAnimationScheduler _shiftAnimationScheduler;
        private readonly ISlideAnimationScheduler _slideAnimationScheduler;

        // One record per item (array + count)
        private readonly Dictionary<BaseGridObject, int> _recordIndexByItem = new(256);
        private SlideDownMoveRecord[] _records = Array.Empty<SlideDownMoveRecord>();
        private int _recordCount;

        // Global path node pool (linked list per record)
        private Vector2Int[] _pathCoord = Array.Empty<Vector2Int>();
        private int[] _pathNext = Array.Empty<int>();
        private int _pathNodeCount;

        // Schedule helpers
        private int[] _moveIndexByCell = Array.Empty<int>();
        private ColumnTimelineState[] _timelineByX = Array.Empty<ColumnTimelineState>();
        private int[] _usedColumnStamp = Array.Empty<int>();
        private int _usedColumnStampId = 1;
        private int[] _order = Array.Empty<int>();
        private int[] _usedColumnsByRecord = Array.Empty<int>();
        private UniTask[] _animTasks = new UniTask[128];
        private UniTask _runningAnimations = UniTask.CompletedTask;

        // Fill workspace
        private int[] _usedTargetStamp = Array.Empty<int>();
        private int[] _usedSourceStamp = Array.Empty<int>();
        private int[] _spawnStackByX = Array.Empty<int>();
        private int _usedTargetStampId = 1;
        private int _usedSourceStampId = 0;

        public SlideDownFillStrategy(IFillItemDecider itemDecider, IShiftAnimationScheduler shiftAnimationScheduler, ISlideAnimationScheduler slideAnimationScheduler)
        {
            _itemDecider = itemDecider;
            _shiftAnimationScheduler = shiftAnimationScheduler;
            _slideAnimationScheduler = slideAnimationScheduler;
        }

        public bool CanHandle(IGridModel model) => GridFillCalcUtil.HasStationaryAndBlocking(model);

        public IFillStrategy Execute(GridStateContext context)
        {
            ResetWorkspace();

            var model = context.Model;

            EnsureBuffers(model.Width, model.Height);

            var movedAny = true;

            // Keep looping while we can apply any movement (vertical fall, diagonal slide, spawn)
            while (movedAny)
            {
                movedAny = TryApplyAnyMove(context);
            }

            _runningAnimations = PlayAnimations(context);
            
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;

        // =========================================================
        // Simulation loop
        // =========================================================

        private bool TryApplyAnyMove(GridStateContext context)
        {
            var height = context.Model.Height;
            var width = context.Model.Width;
            
            var movedAny = false;
            var movedByGravity = false;
            
            movedByGravity |= ApplyVerticalFalls(context.Model, width, height);
            movedByGravity |= ApplyDiagonalSlides(context.Model, width, height);
            
            movedAny |= movedByGravity;
            
            if (!movedByGravity)
                movedAny |= ApplySpawns(context, width, height);

            return movedAny;
        }

        private bool ApplyVerticalFalls(IGridModel model, int width, int height)
        {
            var movedAny = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    var dst = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(model, dst)) continue;
                    if (!GridFillCalcUtil.CanFallVertically(model, x, y, out var src)) continue;

                    var item = model.GetGridObject(src);
                    if (!item || item.IsStationary) continue;

                    AddStep(item, src, false);
                    
                    model.SetGridObject(src, null);
                    model.SetGridObject(dst, item);

                    AddStep(item, dst, false);
                    movedAny = true;
                }
            }

            return movedAny;
        }

        private bool ApplyDiagonalSlides(IGridModel model, int width, int height)
        {
            var movedAny = false;
            BumpStamps();

            for (int y = height - 1; y >= 1; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var targetCoord = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(model, targetCoord)) continue;
                    if (GridFillCalcUtil.CanFallVertically(model, x, y, out _)) continue;

                    // Alternate side preference deterministically
                    var firstDir = ((x ^ y ^ _usedTargetStampId) & 1) == 0 ? -1 : 1;
                    var secondDir = -firstDir;

                    if (TryApplySlideCandidate(model, width, targetCoord, firstDir) || 
                        TryApplySlideCandidate(model, width, targetCoord, secondDir))
                    {
                        //AddStep(movedItem, target, false);
                        movedAny = true;
                    }
                }
            }

            return movedAny;
        }

        private bool ApplySpawns(GridStateContext context, int width, int height)
        {
            var movedAny = false;

            for (int x = 0; x < width; x++)
            {
                if (SpawnTopOpenSegment(context, x, height))
                    movedAny = true;
            }

            return movedAny;
        }

        private void BumpStamps()
        {
            _usedTargetStampId++;
            _usedSourceStampId++;

            // Rare overflow guard
            if (_usedTargetStampId == int.MaxValue || _usedSourceStampId == int.MaxValue)
            {
                Array.Clear(_usedTargetStamp, 0, _usedTargetStamp.Length);
                Array.Clear(_usedSourceStamp, 0, _usedSourceStamp.Length);
                _usedTargetStampId = 1;
                _usedSourceStampId = 1;
            }
        }

        private bool TryApplySlideCandidate(IGridModel model, int width, Vector2Int targetCoord, int dirX)
        {
            if (!GridFillCalcUtil.TryCollectDiagonalSide(model, targetCoord, dirX, out var candidate))
                return false;

            if (model.GetGridObject(candidate.From) != candidate.Item) return false;
            if (model.GetGridObject(candidate.To)) return false;

            var ti = candidate.To.x + candidate.To.y * width;
            if (_usedTargetStamp[ti] == _usedTargetStampId) return false;
            _usedTargetStamp[ti] = _usedTargetStampId;

            var fi = candidate.From.x + candidate.From.y * width;
            if (_usedSourceStamp[fi] == _usedSourceStampId) return false;
            _usedSourceStamp[fi] = _usedSourceStampId;

            AddStep(candidate.Item, candidate.From, false);

            model.SetGridObject(candidate.From, null);
            model.SetGridObject(candidate.To, candidate.Item);
            
            AddStep(candidate.Item, candidate.To, false);
            return true;
        }

        // =========================================================
        // Spawn
        // =========================================================

        private bool SpawnTopOpenSegment(GridStateContext context, int x, int height)
        {
            var model = context.Model;
            var view = context.View;
            var cellSize = view.GetCellSize();

            var spawnedAny = false;
            var segmentStartY = -1;
            var blockedInSegment = false;
            var segmentTopWorldY = 0f;

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                if (!model.IsCellActive(c))
                {
                    segmentStartY = -1;
                    blockedInSegment = false;
                    continue;
                }

                if (segmentStartY < 0)
                {
                    segmentStartY = y;
                    segmentTopWorldY = view.GridToWorld(new Vector2Int(x, y)).y + cellSize;
                }

                var obj = model.GetGridObject(c);

                if (obj)
                {
                    blockedInSegment = true;
                    continue;
                }

                if (blockedInSegment) continue;

                var spawnCount = GridFillCalcUtil.CountEmptiesDown(model, x, y, height);
                if (spawnCount <= 0) continue;

                SpawnInto(model, context, view, x, y, spawnCount, segmentTopWorldY, cellSize);

                spawnedAny = true;
                y += spawnCount - 1;
            }

            return spawnedAny;
        }

        private void SpawnInto(IGridModel model, GridStateContext context, IGridView view, int x, int startY, int spawnCount, float segmentTopWorldY, float cellSize)
        {
            var baseStack = _spawnStackByX[x];

            for (int i = 0; i < spawnCount; i++)
            {
                var target = new Vector2Int(x, startY + i);

                var type = _itemDecider.Decide(model, target);
                var item = context.Factory.GetRegularItem(type);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var w = view.GridToWorld(target);

                var reverseIndex = (spawnCount - 1) - i;
                var spawnY = segmentTopWorldY + (baseStack + reverseIndex) * cellSize;

                item.SetPosition(new Vector3(w.x, spawnY, w.z));

                model.SetGridObject(target, item);
                AddStep(item, target, true);
            }

            _spawnStackByX[x] = baseStack + spawnCount;
        }

        // =========================================================
        // Records & path nodes
        // =========================================================

        private void AddStep(BaseGridObject item, Vector2Int step, bool isSpawn)
        {
            if (!_recordIndexByItem.TryGetValue(item, out var index))
            {
                index = _recordCount;
                _recordIndexByItem[item] = index;

                EnsureRecordCapacity(_recordCount + 1);
                _records[_recordCount++] = new SlideDownMoveRecord(item);
            }

            ref var record = ref _records[index];

            record.IsSpawn |= isSpawn;

            // Avoid pushing the same coord twice in a row
            if (record.TailNode >= 0 && _pathCoord[record.TailNode] == step)
            {
                record.FinalCoord = step;
                return;
            }

            var node = AllocPathNode(step);

            if (record.HeadNode < 0)
            {
                record.HeadNode = node;
            }
            else
            {
                _pathNext[record.TailNode] = node;
            }

            record.TailNode = node;
            record.PathCount++;
            record.FinalCoord = step;
        }

        private void EnsureRecordCapacity(int need)
        {
            if (_records.Length >= need) return;

            var newSize = _records.Length == 0 ? 256 : _records.Length * 2;
            if (newSize < need) newSize = need;

            Array.Resize(ref _records, newSize);
        }

        private int AllocPathNode(Vector2Int coord)
        {
            if (_pathNodeCount >= _pathCoord.Length)
            {
                var newSize = _pathCoord.Length == 0 ? 512 : _pathCoord.Length * 2;
                Array.Resize(ref _pathCoord, newSize);
                Array.Resize(ref _pathNext, newSize);
            }

            var idx = _pathNodeCount++;
            _pathCoord[idx] = coord;
            _pathNext[idx] = -1;
            return idx;
        }

        // =========================================================
        // Animation emit (column timeline scheduling)
        // =========================================================

        private UniTask PlayAnimations(GridStateContext context)
        {
            if (_recordCount == 0)
                return UniTask.CompletedTask;

            EnsureOrderCapacity(_recordCount);

            // Update per-record flags (IsSlide) and build sort order.
            for (int i = 0; i < _recordCount; i++)
                _order[i] = i;

            for (int i = 0; i < _recordCount; i++)
            {
                ref var record = ref _records[i];
                if (!record.Item || record.PathCount == 0) continue;

                var startCoord = _pathCoord[record.HeadNode];
                record.IsSlide = startCoord.x != record.FinalCoord.x;
            }

            Array.Sort(_order, 0, _recordCount, new SlideMoveOrderComparer(_records));

            if (_animTasks.Length < _recordCount)
                Array.Resize(ref _animTasks, _recordCount);

            // Reset timelines.
            var width = context.Model.Width;
            for (int x = 0; x < width; x++)
                _timelineByX[x].Time = 0f;

            var taskCount = 0;

            var view = context.View;
            ScheduleByTimeline(view, width, passIsSpawn: false, ref taskCount);
            ScheduleByTimeline(view, width, passIsSpawn: true, ref taskCount);

            if (taskCount == 0)
                return UniTask.CompletedTask;

            if (_animTasks.Length != taskCount)
                Array.Resize(ref _animTasks, taskCount);

            return UniTask.WhenAll(_animTasks);
        }

        private void EnsureOrderCapacity(int need)
        {
            if (_order.Length < need)
                Array.Resize(ref _order, need);
        }

        private void ScheduleByTimeline(IGridView view, int width, bool passIsSpawn, ref int taskCount)
        {
            for (int i = 0; i < _recordCount; i++)
            {
                var recordIndex = _order[i];
                ref readonly var record = ref _records[recordIndex];

                if (!record.Item || record.PathCount == 0) continue;
                if (record.IsSpawn != passIsSpawn) continue;

                var usedCount = CollectUsedColumns(width, in record);
                var startTime = GetStartTime(usedCount);

                if (!record.IsSlide)
                {
                    var fallRecord = new FallDownMoveRecord(record.Item, record.FinalCoord, record.IsSpawn);

                    if (_shiftAnimationScheduler.TrySchedule(view, fallRecord, startTime, out var endTime, out var task))
                    {
                        UpdateTimeline(usedCount, endTime);
                        _animTasks[taskCount++] = task;
                    }
                }
                else
                {
                    if (_slideAnimationScheduler.TrySchedule(view, record, _pathCoord, _pathNext, startTime, out var endTime, out var task))
                    {
                        UpdateTimeline(usedCount, endTime);
                        _animTasks[taskCount++] = task;
                    }
                }
            }
        }

        private int CollectUsedColumns(int width, in SlideDownMoveRecord record)
        {
            // Reuse _usedColumnsByRecord as a dense list of used X values.
            if (_usedColumnsByRecord.Length < width)
                Array.Resize(ref _usedColumnsByRecord, width);

            _usedColumnStampId++;
            if (_usedColumnStampId == int.MaxValue)
            {
                Array.Clear(_usedColumnStamp, 0, _usedColumnStamp.Length);
                _usedColumnStampId = 1;
            }

            var count = 0;

            void MarkX(int x)
            {
                if ((uint)x >= (uint)width) return;
                if (_usedColumnStamp[x] == _usedColumnStampId) return;
                _usedColumnStamp[x] = _usedColumnStampId;
                _usedColumnsByRecord[count++] = x;
            }

            MarkX(record.FinalCoord.x);

            if (record.IsSlide)
            {
                var node = record.HeadNode;
                while (node >= 0)
                {
                    MarkX(_pathCoord[node].x);
                    node = _pathNext[node];
                }
            }

            return count;
        }

        private float GetStartTime(int usedCount)
        {
            var start = 0f;
            for (int i = 0; i < usedCount; i++)
            {
                var x = _usedColumnsByRecord[i];
                var t = _timelineByX[x].Time;
                if (t > start) start = t;
            }

            return start;
        }

        private void UpdateTimeline(int usedCount, float endTime)
        {
            for (int i = 0; i < usedCount; i++)
            {
                var x = _usedColumnsByRecord[i];
                _timelineByX[x].Time = endTime;
            }
        }

        // =========================================================
        // Buffers & reset
        // =========================================================

        private void EnsureBuffers(int width, int height)
        {
            if (_timelineByX.Length < width)
                _timelineByX = new ColumnTimelineState[width];

            if (_usedColumnStamp.Length < width)
                _usedColumnStamp = new int[width];

            var cellCount = width * height;

            if (_usedTargetStamp.Length < cellCount)
                _usedTargetStamp = new int[cellCount];

            if (_usedSourceStamp.Length < cellCount)
                _usedSourceStamp = new int[cellCount];

            if (_moveIndexByCell.Length < cellCount)
                _moveIndexByCell = new int[cellCount];

            if (_spawnStackByX.Length < width)
                _spawnStackByX = new int[width];
            else
                Array.Clear(_spawnStackByX, 0, width);

            if (_records.Length == 0)
                _records = new SlideDownMoveRecord[256];
        }

        private void ResetWorkspace()
        {
            _recordIndexByItem.Clear();
            _recordCount = 0;
            _pathNodeCount = 0;
        }
    }
}