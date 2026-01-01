using System;
using AYellowpaper.SerializedCollections;
using Core.Extensions;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    public abstract class BaseItemConfigSO : ScriptableObject
    {
        protected bool IsEditor => Application.isEditor;
    }
    
    public abstract class EnumItemConfigSO<TEnum, TData> : BaseItemConfigSO where TEnum : Enum where TData : BaseItemDataSO
    {
        [field: SerializeField] 
        protected SerializedDictionary<TEnum, TData> Configs { get; private set; }

        public TData GetData(TEnum type) => Configs[type];

        [Button, ShowIf(nameof(IsEditor))]
        public void FillDefaultValues() => Configs.EnsureAllEnumKeysExist();
    }
}