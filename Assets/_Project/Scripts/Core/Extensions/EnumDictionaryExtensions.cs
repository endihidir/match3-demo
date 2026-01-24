using System;
using System.Linq;
using AYellowpaper.SerializedCollections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Extensions
{
    public static class EnumDictionaryExtensions
    {
        public static void EnsureAllEnumKeysExist<TKey, TValue>(this SerializedDictionary<TKey, TValue> dictionary, bool exceptFirst = true) where TKey : Enum
        {
            if (dictionary == null)
                return;
            
            var enumValues = (TKey[])Enum.GetValues(typeof(TKey));
            
            var validEnumValues = enumValues.Skip(exceptFirst ? 1 : 0).ToArray();
            
            if (dictionary.Count > 0)
            {
                var message =
                    $"The SerializedDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}> contains " +
                    $"{validEnumValues.Length} key(s) that no longer exist in the {typeof(TKey).Name} enum.\n\n" +
                    "Do you want to remove these obsolete entries?";

#if UNITY_EDITOR
                var ok = EditorUtility.DisplayDialog(
                    "Enum Dictionary Cleanup",
                    message,
                    "Yes, Remove",
                    "No");
#else
                var ok = false;
#endif
                if (!ok) return;
                
            }
            
            foreach (var key in validEnumValues)
            {
                dictionary.Remove(key);
            }
                
            foreach (var key in validEnumValues)
            {
                if (!dictionary.ContainsKey(key))
                    dictionary[key] = Activator.CreateInstance<TValue>();
            }
        }
    }
}