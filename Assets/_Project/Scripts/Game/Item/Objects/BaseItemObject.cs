using System.Diagnostics;
using Core.Config;
using Core.Pool;
using Core.Utils;
using Core.Views;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Item
{
    public abstract class BaseItemObject : PooledObject, IItemObject, IItemType
    {
        [field: SerializeField] public ItemAnimation ItemAnimation { get; private set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2 CellSize { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStationary { get; private set; }
        public GridObjectTypeData GridObjectType { get; private set; }
        private Vector2 CellSizeMultiplier { get; set; }
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

        private bool _isEmpty;
        
        public BaseItemObject Initialize(GridObjectTypeData gridObjectTypeData)
        {
            GridObjectType = gridObjectTypeData;
            name = ToString();
            OnInitialize(GridObjectType.TypeId);
            IsEmpty = GridObjectType.ItemKind == GridItemKind.None || GridObjectType.TypeId == 0;
            return this;
        }
        protected abstract void OnInitialize(int typeId);
        
        public virtual void ApplyData(BaseItemConfigData baseItemConfigData)
        {
            var sprite = baseItemConfigData.icon;
            SpriteRenderer.sprite = sprite;
            CellSizeMultiplier = baseItemConfigData.spriteSizeMultiplier;
            IsStationary = baseItemConfigData.isStationary;
        }
        
        public void SetCellSize(Vector2 cellSize)
        {
            CellSize = cellSize * CellSizeMultiplier;
            SpriteRenderer.size = CellSize;
        }
        
        public void SetPosition(Vector3 position) => transform.position = position;
        public void SetParent(Transform parent) => transform.SetParent(parent);
        
        public void ResetState()
        {
            SpriteRenderer.sprite = null;
            CellSize = Vector2.zero;
            CellSizeMultiplier = Vector2.zero;
            IsStationary = false;
            GridObjectType = default;
            CellSize = default;
            name = ToString();
        }

        protected override void OnDeactivate() => ResetState();
        public override string ToString() => $"Type: {GridObjectType.Type}";

        [Conditional("UNITY_EDITOR"), Button]
        private void DebugCoord()
        {
            var gridView = FindObjectOfType<GridView>();
            
            if (!gridView)
            {
                EditorLogger.LogError("GridView not found!");
                return;
            }
            
            EditorLogger.Log(gridView.WorldToGrid(transform.position));
        }
    }
}