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

        public TData GetData(TEnum type) => Configs[type];

        [Button, ShowIf(nameof(IsEditor))]
        public void FillDefaultValues() => Configs.EnsureAllEnumKeysExist();
    }
}