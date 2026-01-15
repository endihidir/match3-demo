using Core.Config;
using Core.Configs;
using Core.Item;
using Core.Pool;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideAnimationScheduler : ISlideAnimationScheduler
    {
        private readonly FillAnimationSettingsSO _settings;

        public SlideAnimationScheduler(GameplayConfigContainer config)
        {
            _settings = config.FillAnimationSettings;
        }
        
        // Schedules a slide along a path. Wave logic is the same as before.
        public bool TrySchedule(IGridView view, in SlideDownMoveRecord record, Vector2Int[] pathCoord, int[] pathNext, ref ColumnWaveState state, out UniTask task)
        {
            task = UniTask.CompletedTask;
            
            if(!record.Item) return false;
            
            var wave = record.IsSpawn ? state.SpawnSlide + state.Slide + state.Fall : state.Slide + state.Fall;

            if (record.IsSpawn) state.SpawnSlide++; else state.Slide++;

            var delay = wave * _settings.SlideDelayMultiplier;

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

            var tween = record.Item.ItemAnimation.Slide(path.Array, length, seg.Array, delay);
            task = tween.ToUniTask();
            return true;
        }
    }
}