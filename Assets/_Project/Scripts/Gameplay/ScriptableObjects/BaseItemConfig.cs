using System;
using AYellowpaper.SerializedCollections;
using Core.Extensions;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    public abstract class BaseItemConfig : ScriptableObject
    {
        protected bool IsEditor => Application.isEditor;
        
        public ShiftSettingsConfig defaultShiftSettings;
        public ShakeSettingsConfig defaultShakeSettings;
    }
    
    public abstract class BaseEnumConfig<TEnum, TData> : BaseItemConfig where TEnum : Enum
    {
        [field: SerializeField] 
        protected SerializedDictionary<TEnum, TData> Configs { get; private set; }

        [Button, ShowIf(nameof(IsEditor))]
        public void FillDefaultValues() => Configs.EnsureAllEnumKeysExist();
    }
}