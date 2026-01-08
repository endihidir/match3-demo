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
        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                SpriteRenderer.enabled = !_isEmpty;
                if (_isEmpty) IsStationary = false;
            }
        }
        
        public bool IsShiftInProgress => ItemAnimation.IsShiftInProgress;
        private Vector2 SpriteSizeMultiplier { get; set; }
        private bool _isEmpty;
        
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
            name = ToString();
            IsEmpty = TypeId == 0;
        }
        
        public virtual void ApplyData(BaseItemDataSO baseItemDataSo)
        {
            SpriteRenderer.sprite = baseItemDataSo.icon;
            SpriteSizeMultiplier = baseItemDataSo.spriteSizeMultiplier;
            IsStationary = baseItemDataSo.isStationary;
        }
        
        public void SetCoordinate(Vector2Int coord) => Coord = coord;
        public void SetSpriteSize(float cellSize) => SpriteRenderer.size = cellSize * SpriteSizeMultiplier;
        public void SetPosition(Vector3 position) => transform.position = position;
        public void SetParent(Transform parent) => transform.SetParent(parent);
        protected override void OnActivate() => ItemAnimation.InitAnimations();
        protected override void OnDeactivate() => ResetItem();
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