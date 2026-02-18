using System;
using AYellowpaper.SerializedCollections;
using Core.Extensions;
using UnityEngine;

namespace Game.Configs
{
    [Serializable]
    public class EnumConfigMap<TEnum, TData> where TEnum : Enum
    {
        [field: SerializeField] protected SerializedDictionary<TEnum, TData> Map { get; private set; }
        public TData Get(TEnum type) => Map[type];
        public bool TryGet(TEnum type, out TData data) => Map.TryGetValue(type, out data);
        public bool TryGet<T>(TEnum type, out T data) where T : TData
        {
            if (Map.TryGetValue(type, out var raw) && raw is T typed)
            {
                data = typed;
                return true;
            }

            data = default;
            return false;
        }
        public void EnsureAllKeysExist() => Map.EnsureAllEnumKeysExist();
    }
}