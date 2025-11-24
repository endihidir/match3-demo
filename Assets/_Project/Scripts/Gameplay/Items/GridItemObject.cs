using Core.Pool;
using UnityEngine;

namespace Core.Item
{
    public class GridItemObject : PooledObject
    {
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        public IGridItemBehaviour Behaviour { get; private set; }
        public void BindBehaviour(IGridItemBehaviour behaviour) => Behaviour = behaviour;
    }
}