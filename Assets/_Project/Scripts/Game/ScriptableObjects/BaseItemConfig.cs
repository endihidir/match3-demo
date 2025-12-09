using System;
using AYellowpaper.SerializedCollections;
using Core.Extensions;
using Core.Utils;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    public abstract class BaseItemConfig : ScriptableObject
    {
        protected bool IsEditor => Application.isEditor;
    }
    
    public abstract class EnumItemConfig<TEnum, TData> : BaseItemConfig where TEnum : Enum where TData : BaseItemConfigData
    {
        [field: SerializeField] 
        protected SerializedDictionary<TEnum, TData> Configs { get; private set; }
        
        public bool TryGetData(TEnum type, out TData configData) => Configs.TryGetValue(type, out configData);

        public Sprite GetSprite(TEnum type)
        {
            var canGet = TryGetData(type, out var configData);

            switch (canGet)
            {
                case false:
                    EditorLogger.LogWarning($"{type.ToString()} data is missing!");
                    break;
                case true when !configData.icon:
                    EditorLogger.LogWarning($"{type.ToString()} sprite is missing!");
                    break;
            }
            
            return canGet ? configData.icon : null;
        }
        
        public float GetSizeMultiplier(TEnum type)
        {
            var canGet = TryGetData(type, out var configData);

            switch (canGet)
            {
                case false:
                    EditorLogger.LogWarning($"{type.ToString()} data is missing!");
                    break;
            }
            
            return canGet ? configData.spriteSizeMultiplier : 1f;
        }
        
        public ItemAnimationConfig GetAnimationConfig(TEnum type)
        {
            var canGet = TryGetData(type, out var configData);

            switch (canGet)
            {
                case false:
                    EditorLogger.LogWarning($"{type.ToString()} data is missing!");
                    break;
                case true when configData.overrideAnimation && !configData.animationConfig:
                    EditorLogger.LogWarning($"{type.ToString()} animation data is missing!");
                    break;
            }
            
            return canGet && configData.overrideAnimation ? configData.animationConfig : null;
        }

        [Button, ShowIf(nameof(IsEditor))]
        public void FillDefaultValues() => Configs.EnsureAllEnumKeysExist();
    }

    [Serializable]
    public abstract class BaseItemConfigData
    {
        public Sprite icon;
        public float spriteSizeMultiplier = 1f;
        public bool overrideAnimation;
        [ShowIf(nameof(overrideAnimation)), AllowNesting]
        public ItemAnimationConfig animationConfig;
    }
}