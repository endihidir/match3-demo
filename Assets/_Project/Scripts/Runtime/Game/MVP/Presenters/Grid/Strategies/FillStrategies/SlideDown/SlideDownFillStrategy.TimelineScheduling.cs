using System;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        /// <summary>
        /// Converts all recorded move paths into DOTween animations, ordered by
        /// column timeline so items in the same column cascade naturally.
        ///
        /// Pass 1 — non-spawn records (existing items that fell / slid).
        /// Pass 2 — spawn records   (newly created items falling in).
        ///
        /// Separating spawns from non-spawns ensures that newly created items
        /// never start their fall before the column has settled.
        /// </summary>
        private UniTask ScheduleAnimations()
        {
            if (_recordCount == 0)
                return UniTask.CompletedTask;

            var width = _gridModel.Width;

            // ---- classify records (fall vs slide) ----
            for (int i = 0; i < _recordCount; i++)
            {
                ref var r = ref _records[i];
                if (!r.Item || r.PathCount == 0) continue;
                r.IsSlide = PathHasXChange(in r);
            }

            // ---- build sort order ----
            EnsureSize(ref _sortOrder, _recordCount);
            
            for (int i = 0; i < _recordCount; i++)
                _sortOrder[i] = i;

            Array.Sort(_sortOrder, 0, _recordCount, new SlideMoveOrderComparer(_records));

            // ---- reset per-column timelines ----
            // (array was already cleared in PrepareForRun — but cleared again
            //  here defensively, since two passes share the same timeline)
            Array.Clear(_timelineByX, 0, width);

            // Pre-size task buffer (grown if needed, never shrunk to avoid GC).
            if (_animTasks.Length < _recordCount)
                Array.Resize(ref _animTasks, _recordCount * 2);

            var taskCount = 0;

            SchedulePass(width, passSpawn: false, ref taskCount);
            SchedulePass(width, passSpawn: true,  ref taskCount);

            if (taskCount == 0)
                return UniTask.CompletedTask;

            // Slice exactly taskCount tasks.
            var tasks = new UniTask[taskCount];
            Array.Copy(_animTasks, tasks, taskCount);
            return UniTask.WhenAll(tasks);
        }

        // -----------------------------------------------------------------------

        private void SchedulePass(int width, bool passSpawn, ref int taskCount)
        {
            for (int si = 0; si < _recordCount; si++)
            {
                var idx = _sortOrder[si];
                ref readonly var record = ref _records[idx];

                if (!record.Item || record.PathCount == 0) continue;
                if (record.IsSpawn != passSpawn) continue;

                var colCount = CollectUsedColumns(width, in record);
                var startTime = MaxTimelineOf(colCount);

                UniTask task;
                float endTime;

                if (!record.IsSlide)
                {
                    var fallRecord = new FallDownMoveRecord(record.Item, record.FinalCoord, record.IsSpawn);
                    
                    if (!_fallAnimationScheduler.TrySchedule(fallRecord, startTime, out endTime, out task)) continue;
                }
                else
                {
                    if (!_slideAnimationScheduler.TrySchedule(record, _pathCoord, _pathNext, startTime, out endTime, out task)) continue;
                }

                SetTimeline(colCount, endTime);

                if (taskCount == _animTasks.Length)
                    Array.Resize(ref _animTasks,_animTasks.Length * 2);

                _animTasks[taskCount++] = task;
            }
        }

        /// <summary>
        /// Fills <see cref="_usedCols"/> with the distinct X values touched by
        /// <paramref name="record"/>'s path (plus FinalCoord.x).
        /// Returns the number of distinct columns collected.
        /// </summary>
        private int CollectUsedColumns(int width, in SlideDownMoveRecord record)
        {
            EnsureSize(ref _usedCols, width);
            EnsureSize(ref _usedColStamp, width);

            _usedColStampId++;
            
            if (_usedColStampId == int.MaxValue)
            {
                Array.Clear(_usedColStamp, 0, _usedColStamp.Length);
                _usedColStampId = 1;
            }

            var count = 0;

            MarkColumn(record.FinalCoord.x);

            if (record.IsSlide)
            {
                var node = record.HeadNode;
                
                while (node >= 0)
                {
                    MarkColumn(_pathCoord[node].x);
                    node = _pathNext[node];
                }
            }

            return count;

            // Local function — captures count and width by ref/value.
            void MarkColumn(int x)
            {
                if (x >= width) return;
                if (_usedColStamp[x] == _usedColStampId) return;
                _usedColStamp[x] = _usedColStampId;
                _usedCols[count++] = x;
            }
        }

        private float MaxTimelineOf(int colCount)
        {
            var max = 0f;
            
            for (int i = 0; i < colCount; i++)
            {
                var t = _timelineByX[_usedCols[i]];
                if (t > max) max = t;
            }
            
            return max;
        }

        private void SetTimeline(int colCount, float endTime)
        {
            for (int i = 0; i < colCount; i++)
                _timelineByX[_usedCols[i]] = endTime;
        }

        /// <summary>Returns true if any node in the path has a different X than the head.</summary>
        private bool PathHasXChange(in SlideDownMoveRecord record)
        {
            if (record.HeadNode < 0) return false;

            var headX = _pathCoord[record.HeadNode].x;
            var node = _pathNext[record.HeadNode];

            while (node >= 0)
            {
                if (_pathCoord[node].x != headX) return true;
                node = _pathNext[node];
            }

            return false;
        }
    }
}