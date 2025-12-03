using System;
using Core.Pool;
using UnityEngine;

namespace Core.Item
{
    public class GridItemObject : PooledObject, IGridItemObject
    {
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        public Transform Transform => transform;
        public IGridItemBehaviour Behaviour { get; private set; }
        public void BindBehaviour(IGridItemBehaviour behaviour) => Behaviour = behaviour;
        
        public override void Deactivate(float duration = 0, float delay = 0, Action onComplete = null)
        {
            base.Deactivate(duration, delay, onComplete);
            Reset();
        }

        private void OnDestroy() => Reset();

        private void Reset()
        {
            Behaviour?.Dispose();
            Behaviour = null;
        }
    }

    public interface IGridItemObject
    {
        public SpriteRenderer SpriteRenderer { get; }
        public Transform Transform { get; }
    }
}