using System;
using System.Collections.Generic;
using Game.Grid.Item;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public partial class SlideDownFillStrategy
    {
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
        private float[] _timelineByX = Array.Empty<float>();
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
        private void EnsureBuffers(int width, int height)
        {
            if (_timelineByX.Length < width)
                _timelineByX = new float[width];

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