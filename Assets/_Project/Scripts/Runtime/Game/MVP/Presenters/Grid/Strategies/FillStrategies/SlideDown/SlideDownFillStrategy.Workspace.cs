using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Grid.Item;
using Game.Grid.Strategies.Data;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        /// <summary>Dictionary that maps a live item reference to its record slot index.</summary>
        private readonly Dictionary<BaseGridObject, int> _recordByItem = new(256);

        /// <summary>Flat struct array; only [0.._recordCount) is valid each run.</summary>
        private SlideDownMoveRecord[] _records = new SlideDownMoveRecord[256];
        private int _recordCount;

        /// <summary>World-space coordinates stored per node.</summary>
        private Vector2Int[] _pathCoord = new Vector2Int[512];

        /// <summary>Index of the next node in the chain, or -1 for end.</summary>
        private int[] _pathNext = new int[512];

        /// <summary>Number of allocated nodes this run; reset to 0 each run.</summary>
        private int _pathNodeCount;

        /// <summary>Per-cell stamp arrays for diagonal-slide deduplication.
        /// Sized to width*height; reallocated on grid size growth.</summary>
        private int[] _usedTargetStamp = Array.Empty<int>();
        private int[] _usedSourceStamp = Array.Empty<int>();

        /// <summary>Both stamps start at 1 so fresh (zero-initialised) arrays are
        /// always "un-stamped" relative to the current ID.</summary>
        private int _usedTargetStampId = 1;
        private int _usedSourceStampId = 1;

        /// <summary>How many items have been stacked above column x this run.
        /// Reset to 0 in PrepareForRun.</summary>
        private int[] _spawnStackByX = Array.Empty<int>();

        /// <summary>The latest scheduled end-time for each column (x index).</summary>
        private float[] _timelineByX = Array.Empty<float>();

        /// <summary>Per-column stamp for deduplicating "used column" collection.</summary>
        private int[] _usedColStamp = Array.Empty<int>();
        private int   _usedColStampId = 1;

        /// <summary>Temporary dense list of column-x values touched by one record.</summary>
        private int[] _usedCols = Array.Empty<int>();

        /// <summary>Sort-order array; indices into _records[0.._recordCount).</summary>
        private int[] _sortOrder = Array.Empty<int>();

        /// <summary>Collected UniTask per scheduled record.</summary>
        private UniTask[] _animTasks = new UniTask[128];

        /// <summary>The running animations from the most recent Execute() call.</summary>
        private UniTask _pendingAnimations = UniTask.CompletedTask;

        /// <summary>
        /// Called once at the top of every Execute().
        /// Resets all per-run counters AND ensures all arrays are large enough.
        /// </summary>
        private void PrepareForRun(int width, int height)
        {
            // --- per-run counters ---
            _recordByItem.Clear();
            _recordCount   = 0;
            _pathNodeCount = 0;

            // --- arrays that must be at least (width) ---
            EnsureSize(ref _timelineByX, width);
            EnsureSize(ref _usedColStamp, width);
            EnsureSize(ref _usedCols, width);
            EnsureSize(ref _spawnStackByX, width);

            // Clear spawn stacks (must be 0 at start of every run).
            Array.Clear(_spawnStackByX,0, width);

            // Clear column timelines (must be 0 at start of every run).
            Array.Clear(_timelineByX,0, width);

            // --- arrays that must be at least (width * height) ---
            var cellCount = width * height;
            EnsureSize(ref _usedTargetStamp, cellCount);
            EnsureSize(ref _usedSourceStamp, cellCount);

            // --- sort / task buffers (grown on demand, never shrunk) ---
            EnsureSize(ref _sortOrder, _records.Length);
        }

        private int AllocNode(Vector2Int coord)
        {
            if (_pathNodeCount == _pathCoord.Length)
            {
                var newSize = _pathCoord.Length * 2;
                Array.Resize(ref _pathCoord, newSize);
                Array.Resize(ref _pathNext, newSize);
            }

            var idx = _pathNodeCount++;
            _pathCoord[idx] = coord;
            _pathNext[idx]  = -1;
            return idx;
        }

        private void EnsureRecordCapacity(int minCount)
        {
            if (_records.Length >= minCount) return;

            var newSize = Math.Max(_records.Length * 2, minCount);
            Array.Resize(ref _records, newSize);

            // Sort-order array tracks records 1:1.
            EnsureSize(ref _sortOrder, newSize);
        }

        /// <summary>
        /// Increments both diagonal-slide stamps at the start of each
        /// diagonal-slide pass.  Handles overflow by clearing arrays and
        /// resetting to 1.
        /// </summary>
        private void AdvanceSlideStamps()
        {
            _usedTargetStampId++;
            _usedSourceStampId++;

            if (_usedTargetStampId == int.MaxValue || _usedSourceStampId == int.MaxValue)
            {
                Array.Clear(_usedTargetStamp, 0, _usedTargetStamp.Length);
                Array.Clear(_usedSourceStamp, 0, _usedSourceStamp.Length);
                _usedTargetStampId = 1;
                _usedSourceStampId = 1;
            }
        }

        private static void EnsureSize<T>(ref T[] array, int minLength)
        {
            if (array.Length >= minLength) return;
            var newSize = Math.Max(array.Length == 0 ? 16 : array.Length * 2, minLength);
            Array.Resize(ref array, newSize);
        }
    }
}