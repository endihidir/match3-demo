using Core.Pool;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideAnimationScheduler : ISlideAnimationScheduler
    {
        public bool TrySchedule(IGridView view, in SlideDownMoveRecord record, ref PathNodePool pathPool, float startTime, out float endTime, out UniTask task)
        {
            task = UniTask.CompletedTask;
            endTime = startTime;

            if (!record.Item) return false;

            var length = record.PathCount + (record.IsSpawn ? 1 : 0);

            using var path = new PooledArray<Vector3>(length);
            using var segments = new PooledArray<float>(length);

            var filled = 0;

            // If spawned, start from current position
            if (record.IsSpawn)
                path.Array[filled++] = record.Item.transform.position;

            // Walk the path linked list
            var node = record.HeadNode;
            while (node >= 0)
            {
                path.Array[filled++] = view.GridToWorld(pathPool.GetCoord(node));
                node = pathPool.GetNext(node);
            }

            // Calculate segment distances
            var current = record.Item.transform.position;
            var cellSize = view.GetCellSize();

            for (int i = 0; i < length; i++)
            {
                var next = path.Array[i];
                segments.Array[i] = Mathf.Abs(next.y - current.y) / cellSize;
                current = next;
            }

            var animation = record.Item.Animation;
            var tween = animation.SlideAlongPath(path.Array, length, segments.Array, startTime);

            task = tween?.ToUniTask() ?? UniTask.CompletedTask;
            endTime = startTime + animation.GetSlideDelay();

            return true;
        }
    }
}