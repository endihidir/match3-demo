using System;
using Core.Models;
using Core.Utils;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public partial class FallDownFillStrategy
    {
        private SimpleMoveRecordBuffer<FallDownMoveRecord> _recordBuffer;
        
        // Animation scheduling
        private AnimationTimeline _timeline;
        private AnimationTaskCollector _taskCollector;
        private SortOrderHelper _sortOrder;
        
        // State
        private UniTask _runningAnimations;

        private void EnsureCapacity(int width, int height)
        {
            _recordBuffer.EnsureCapacity(width * height);
            _timeline.EnsureCapacity(width);
            _taskCollector.EnsureCapacity(width * height);
        }

        private void ResetWorkspace()
        {
            _recordBuffer.Reset();
            _taskCollector.Reset();
        }

        private void AddRecord(in FallDownMoveRecord record)
        {
            _recordBuffer.Add(record);
        }
    }
}
