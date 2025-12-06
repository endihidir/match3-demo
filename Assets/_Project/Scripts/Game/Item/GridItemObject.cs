using System;
using Core.Pool;
using UnityEngine;

namespace Core.Item
{
    public class GridItemObject : PooledObject, IGridItemObject
    {
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField] public GridItemState State { get; private set; }
        public Transform Transform => transform;
        public void BindState(GridItemState state) => State = state;
        public void ResetState()
        {
            State?.Dispose();
            State = null;
        }
        public override void Deactivate(float duration = 0, float delay = 0, Action onComplete = null)
        {
            base.Deactivate(duration, delay, onComplete);
            ResetState();
        }

        private void OnDestroy() => ResetState();

    }

    public interface IGridItemObject
    {
        public SpriteRenderer SpriteRenderer { get; }
        public Transform Transform { get; }
    }
}