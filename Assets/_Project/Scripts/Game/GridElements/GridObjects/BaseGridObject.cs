using Core.Config;
using Core.Pool;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public abstract class BaseGridObject : PooledObject
    {
        [field: SerializeField] public ItemAnimation ItemAnimation { get; private set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStationary { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2Int Coord { get; private set; }
        public GridObjectType ObjectType { get; private set; }
        public GridItemKind ItemKind => ObjectType.ItemKind;
        public int TypeId => ObjectType.TypeId;
        public bool IsNone
        {
            get => _isNone;
            private set
            {
                _isNone = value;
                SpriteRenderer.enabled = !_isNone;
                if (_isNone) IsStationary = false;
            }
        }
        
        public bool IsFallInProgress => ItemAnimation.IsFallInProgress;
        private Vector2 SpriteSizeMultiplier { get; set; }
        private bool _isNone;
        
        public BaseGridObject Initialize(GridObjectType objectType)
        {
            ObjectType = objectType;
            OnInitialize();
            UpdateIdentity();
            return this;
        }
        
        protected abstract void OnInitialize();
        protected void UpdateIdentity()
        {
#if UNITY_EDITOR
            name = ToString();
#endif
            IsNone = TypeId == 0;
        }
        
        public virtual void ApplyData(BaseItemDataSO baseItemDataSo)
        {
            SpriteRenderer.sprite = baseItemDataSo.icon;
            SpriteSizeMultiplier = baseItemDataSo.spriteSizeMultiplier;
            IsStationary = baseItemDataSo.isStationary;
        }

        public void SetFrontOf(BaseGridObject targetObj) => SpriteRenderer.sortingOrder = targetObj.SpriteRenderer.sortingOrder + 1;
        public void SetCoordinate(Vector2Int coord) => Coord = coord;
        public void SetSpriteSize(float cellSize) => SpriteRenderer.size = cellSize * SpriteSizeMultiplier;
        public void SetPosition(Vector3 position) => transform.position = position;
        public void SetParent(Transform parent) => transform.SetParent(parent);
        public override void Activate()
        {
            base.Activate();
            ItemAnimation.CacheAnimations();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            ResetItem();
        }

        public void ResetItem()
        {
            Coord = new Vector2Int(-1, -1);
            ObjectType = default;
            SetPosition(Vector3.zero);
            ItemAnimation?.Dispose();
            SpriteRenderer.sprite = null;
            SpriteSizeMultiplier = Vector2.zero;
        }
    }
}