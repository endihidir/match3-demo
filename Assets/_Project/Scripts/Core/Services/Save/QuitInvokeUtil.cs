using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.SaveSystem
{
    public static class QuitInvokeUtil
    {
        private static bool _cached;
        
        private static readonly List<Action> _staticInvokes = new();
        private static readonly Dictionary<Type, List<Action<Object>>> _instanceInvokesByDeclType = new();
        
        private static readonly string[] _allowedAsmPrefixes =
        {
            "Assembly-CSharp",
            "UnityBase",
            "Game"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            CacheIfNeeded();
        }

        public static void InvokeAll()
        {
            CacheIfNeeded();
            
            for (int i = 0; i < _staticInvokes.Count; i++)
            {
                SafeInvoke(_staticInvokes[i]);
            }
            
            foreach (var kv in _instanceInvokesByDeclType)
            {
                var declType = kv.Key;
                var invokes = kv.Value;

                var instances = FindAllOfType(declType);
                if (instances == null || instances.Count == 0) continue;

                for (int i = 0; i < instances.Count; i++)
                {
                    var obj = instances[i];
                    for (int j = 0; j < invokes.Count; j++)
                    {
                        SafeInvoke(invokes[j], obj);
                    }
                }
            }

            try { PlayerPrefs.Save(); }
            catch (Exception e) { Debug.LogException(e); }
        }

        private static void CacheIfNeeded()
        {
            if (_cached) return;
            _cached = true;

            EnsureSaveDispatcherExists();
#if UNITY_EDITOR
            var methods = TypeCache.GetMethodsWithAttribute<AutoSaveAttribute>();
            
            for (int i = 0; i < methods.Count; i++)
            {
                TryRegisterMethod(methods[i]);
            }
#else

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!IsAllowedAssembly(asm)) continue;

                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }

                for (int ti = 0; ti < types.Length; ti++)
                {
                    var t = types[ti];
                    MethodInfo[] methods;
                    try
                    {
                        methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                               BindingFlags.Static | BindingFlags.Instance |
                                               BindingFlags.DeclaredOnly);
                    }
                    catch { continue; }

                    for (int mi = 0; mi < methods.Length; mi++)
                    {
                        var m = methods[mi];
                        if (m.GetCustomAttribute<AutoSaveAttribute>() == null) continue;
                        TryRegisterMethod(m);
                    }
                }
            }
#endif
        }

        private static void TryRegisterMethod(MethodInfo m)
        {
            if (m.ReturnType != typeof(void) || m.GetParameters().Length != 0) return;

            if (m.IsStatic)
            {
                try
                {
                    var d = (Action)Delegate.CreateDelegate(typeof(Action), m);
                    _staticInvokes.Add(d);
                }
                catch
                {
                    _staticInvokes.Add(() => m.Invoke(null, null));
                }
            }
            else
            {
                var declType = m.DeclaringType;
                if (declType == null) return;

                if (!_instanceInvokesByDeclType.TryGetValue(declType, out var list))
                {
                    list = new List<Action<Object>>();
                    _instanceInvokesByDeclType[declType] = list;
                }

                try
                {
                    var del = m.CreateDelegate(typeof(Action<>).MakeGenericType(declType));
                    list.Add((target) => del.DynamicInvoke(target));
                }
                catch
                {
                    list.Add((target) => m.Invoke(target, null));
                }
            }
        }

        private static bool IsAllowedAssembly(Assembly asm)
        {
            var n = asm.GetName().Name;
            for (int i = 0; i < _allowedAsmPrefixes.Length; i++)
            {
                if (n.StartsWith(_allowedAsmPrefixes[i], StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static List<Object> FindAllOfType(Type t)
        {
#if UNITY_2023_1_OR_NEWER
            var arr = Object.FindObjectsByType(t, FindObjectsInactive.Include, FindObjectsSortMode.None);
            return arr?.ToList();
#else
            var arr = Resources.FindObjectsOfTypeAll(t);
            return arr?.ToList();
#endif
        }

        private static void SafeInvoke(Action a)
        {
            try { a(); }
            catch (Exception e) { Debug.LogException(e); }
        }

        private static void SafeInvoke(Action<Object> a, Object target)
        {
            try { a(target); }
            catch (Exception e) { Debug.LogException(e); }
        }
        
        private static void EnsureSaveDispatcherExists()
        {
            if (Object.FindObjectOfType<SaveDispatcher>(true)) return;
            
            var obj = new GameObject(nameof(SaveDispatcher));
            
            Object.DontDestroyOnLoad(obj);
            
            obj.AddComponent<SaveDispatcher>();
        }
    }
}
