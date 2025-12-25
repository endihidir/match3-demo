using System.Diagnostics;
using Core.Config;
using Core.Pool;
using Core.Utils;
using Core.Views;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public abstract class BaseGridObject : PooledObject
    {
        [field: SerializeField] public ItemAnimation ItemAnimation { get; private set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStationary { get; private set; }
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
        
        public virtual void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            SpriteRenderer.sprite = baseItemConfigData.icon;
            SpriteSizeMultiplier = baseItemConfigData.spriteSizeMultiplier;
            IsStationary = baseItemConfigData.isStationary;
        }
        
        public void SetSpriteSize(float cellSize) => SpriteRenderer.size = cellSize * SpriteSizeMultiplier;
        public void SetPosition(Vector3 position) => transform.position = position;
        public void SetParent(Transform parent) => transform.SetParent(parent);
        protected override void OnDeactivate() => ResetItem();
        public void ResetItem()
        {
            ObjectType = default;
            SetPosition(Vector3.zero);
            ItemAnimation?.Dispose();
            SpriteRenderer.sprite = null;
            SpriteSizeMultiplier = Vector2.zero;
        }

        [Conditional("UNITY_EDITOR"), Button]
        private void LogCoordinate()
        {
            var gridView = FindObjectOfType<GridView>();
            
            if (!gridView && !gridView.IsInitialized)
            {
                EditorLogger.LogError("GridView not found!");
                return;
            }
            
            EditorLogger.Log(gridView.WorldToGrid(transform.position));
        }
    }
}