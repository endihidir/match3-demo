using Game.Configs;
using Core.Pool.Services;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Grid.Item
{
    public abstract class BaseGridObject : PooledObject
    {
        [field: SerializeField] public GridObjectAnimation Animation { get; private set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2Int Coord { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStationary { get; private set; }
        public GridObjectType ObjectType { get; private set; }
        public GridObjectKind ObjectKind => ObjectType.ObjectKind;
        public int TypeId => ObjectType.TypeId;
        
        public bool IsFallInProgress => Animation.IsFallInProgress;
        private Vector2 SpriteSizeMultiplier { get; set; }
        
        public BaseGridObject Initialize(GridObjectType objectType)
        {
            ObjectType = objectType;
            OnInitialize();
            return this;
        }
        
        protected abstract void OnInitialize();
        
        public virtual void ApplyData(BaseGridObjectDataSO baseGridObjectData)
        {
            IsStationary = baseGridObjectData.IsStationary;
            SpriteRenderer.sprite = baseGridObjectData.Icon;
            SpriteSizeMultiplier = baseGridObjectData.SpriteSizeMultiplier;
        }

        public void SetFrontOf(BaseGridObject targetObj) => SpriteRenderer.sortingOrder = targetObj.SpriteRenderer.sortingOrder + 1;
        public void SetCoordinate(Vector2Int coord) => Coord = coord;
        public void SetSpriteSize(float cellSize) => SpriteRenderer.size = cellSize * SpriteSizeMultiplier;
        public void SetPosition(Vector3 position) => transform.position = position;
        public void SetParent(Transform parent) => transform.SetParent(parent);
        protected override void OnActivate() => Animation.CacheAnimations();
        protected override void OnDeactivate() => ResetItem();
        
        protected void UpdateIdentity()
        {
#if UNITY_EDITOR
            name = ToString();
#endif
        }
        
        public void ResetItem()
        {
            IsStationary = false;
            Coord = new Vector2Int(-1, -1);
            ObjectType = default;
            SetPosition(Vector3.zero);
            Animation?.Dispose();
            SpriteRenderer.sprite = null;
            SpriteRenderer.sortingOrder = 0;
            SpriteSizeMultiplier = Vector2.zero;
        }
    }
}