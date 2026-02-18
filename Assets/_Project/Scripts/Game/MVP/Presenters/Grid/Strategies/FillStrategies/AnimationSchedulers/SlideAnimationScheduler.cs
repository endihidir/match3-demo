using Core.Pool.Services;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Strategies.Schedulers
{
    public sealed class SlideAnimationScheduler : ISlideAnimationScheduler
    {
        public bool TrySchedule(IGridView view, in SlideDownMoveRecord record, Vector2Int[] pathCoord, int[] pathNext, float startTime, out float endTime, out UniTask task)
        {
            task = UniTask.CompletedTask;
            endTime = startTime;
            
            if(!record.Item) return false;

            var length = record.PathCount + (record.IsSpawn ? 1 : 0);

            using var path = new PooledArray<Vector3>(length);
            using var seg = new PooledArray<float>(length);

            var filled = 0;

            if (record.IsSpawn)
                path.Array[filled++] = record.Item.transform.position;

            var node = record.HeadNode;

            while (node >= 0)
            {
                path.Array[filled++] = view.GridToWorld(pathCoord[node]);
                node = pathNext[node];
            }

            var current = record.Item.transform.position;

            for (int i = 0; i < length; i++)
            {
                var next = path.Array[i];
                seg.Array[i] = Mathf.Abs(next.y - current.y) / view.GetCellSize();
                current = next;
            }

            var animation = record.Item.Animation;
            
            var tween = animation.SlideAlongPath(path.Array, length, seg.Array, startTime);
            
            task = tween?.ToUniTask() ?? UniTask.CompletedTask;
            
            endTime = startTime + animation.GetSlideDelay();
            
            return true;
        }
    }
}