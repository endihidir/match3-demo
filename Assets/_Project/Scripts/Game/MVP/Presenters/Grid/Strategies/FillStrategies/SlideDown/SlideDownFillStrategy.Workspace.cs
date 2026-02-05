using System;
using Core.Item;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public partial class SlideDownFillStrategy
    {
        // Records with item lookup
        private MoveRecordBuffer<SlideDownMoveRecord, BaseGridObject> _recordBuffer;
        private bool _recordBufferInitialized;
        
        // Path storage
        private PathNodePool _pathPool;
        
        // Animation scheduling
        private AnimationTimeline _timeline;
        private AnimationTaskCollector _taskCollector;
        private SortOrderHelper _sortOrder;
        
        // Simulation stamps
        private StampTracker _targetStamp;
        private StampTracker _sourceStamp;
        
        // Spawn tracking
        private int[] _spawnStackByX;
        
        // State
        private UniTask _runningAnimations;

        private void EnsureCapacity(int width, int height)
        {
            var cellCount = width * height;
            
            if (!_recordBufferInitialized)
            {
                _recordBuffer = new MoveRecordBuffer<SlideDownMoveRecord, BaseGridObject>(useKeyLookup: true);
                _recordBufferInitialized = true;
            }
            
            _recordBuffer.EnsureCapacity(cellCount);
            _pathPool.EnsureCapacity(cellCount * 2);
            _timeline.EnsureCapacity(width);
            _taskCollector.EnsureCapacity(cellCount);
            _targetStamp.EnsureCapacity(cellCount);
            _sourceStamp.EnsureCapacity(cellCount);
            
            if (_spawnStackByX == null || _spawnStackByX.Length < width)
                _spawnStackByX = new int[width];
            else
                Array.Clear(_spawnStackByX, 0, width);
        }

        private void ResetWorkspace()
        {
            _recordBuffer.Reset();
            _pathPool.Reset();
            _taskCollector.Reset();
            _targetStamp.Reset();
            _sourceStamp.Reset();
            _passCounter = 0;
        }
    }
}