using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game.Configs;
using UnityEditor;

namespace Core.Editors
{
    public static class SceneIdGeneratorEditor
    {
        private static readonly string OutputFolder = "Assets/_Project/Scripts/Core/Services/Scene/Generated";
        private static readonly string ScriptName = "SceneIdLookup";
        private static readonly string FullPath = Path.Combine(OutputFolder, $"{ScriptName}.cs");
        private const string SessionKey = "SceneIdGenerator_Ran";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (SessionState.GetBool(SessionKey, false)) return;

            EditorApplication.delayCall += () =>
            {
                TryGenerate();
                SessionState.SetBool(SessionKey, true);
            };
        }

        [MenuItem("Tools/Regenerate SceneIdLookup")]
        private static void RegenerateFromMenu()
        {
            TryGenerate();
        }

        private static void TryGenerate()
        {
            var config = FindFirstConfig();
            if (!config) return;
            
            var sceneNames = ExtractSceneNames(config).ToList();
            var sanitized = new List<string>(sceneNames.Count);
            var used = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < sceneNames.Count; i++)
            {
                var raw = sceneNames[i] ?? string.Empty;
                var name = SanitizeEnumName(raw);
                if (string.IsNullOrEmpty(name)) continue;

                var unique = MakeUnique(name, used);
                sanitized.Add(unique);
                used.Add(unique);
            }

            var code = BuildCode(sanitized, sceneNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList());

            Directory.CreateDirectory(OutputFolder);

            var exists = File.Exists(FullPath);
            var prev = exists ? File.ReadAllText(FullPath) : null;

            if (!exists || !string.Equals(prev, code, StringComparison.Ordinal))
            {
                File.WriteAllText(FullPath, code, Encoding.UTF8);
                AssetDatabase.ImportAsset(FullPath);
            }
        }

        private static SceneLoadServiceConfigSO FindFirstConfig()
        {
            var guids = AssetDatabase.FindAssets("t:SceneLoadServiceConfig");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SceneLoadServiceConfigSO>(path);
                if (asset) return asset;
            }
            return null;
        }
        
        private static IEnumerable<string> ExtractSceneNames(SceneLoadServiceConfigSO configSo)
        {
            var so = new SerializedObject(configSo);
            var listProp = so.FindProperty("sceneAssetConfigs");
            if (listProp == null || !listProp.isArray) yield break;

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                var sceneAssetConfig = elem.objectReferenceValue as SceneAssetConfigSO;
                if (sceneAssetConfig == null) continue;
                
                var groupId = sceneAssetConfig.GetSceneGroupId();
                
                if (string.IsNullOrWhiteSpace(groupId))
                {
                    var list = sceneAssetConfig.SceneDataList;

                    if (list != null && list.Count > 0)
                    {
                        string candidate = null;

                        for (int j = 0; j < list.Count; j++)
                        {
                            var data = list[j];
                            if (data.sceneReference != null && data.isActiveByDefault)
                            {
                                candidate = data.sceneReference.Name;
                                break;
                            }
                        }
                        
                        if (string.IsNullOrWhiteSpace(candidate))
                        {
                            for (int j = 0; j < list.Count; j++)
                            {
                                var data = list[j];
                                if (data.sceneReference != null)
                                {
                                    candidate = data.sceneReference.Name;
                                    break;
                                }
                            }
                        }

                        groupId = candidate;
                    }
                }

                if (string.IsNullOrWhiteSpace(groupId)) continue;
                yield return groupId;
            }
        }

        private static string SanitizeEnumName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var s = raw.Trim().Replace(' ', '_');
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_') sb.Append(ch);
            }
            if (sb.Length == 0) return string.Empty;
            if (char.IsDigit(sb[0])) sb.Insert(0, '_');
            return sb.ToString();
        }

        private static string MakeUnique(string name, HashSet<string> used)
        {
            if (!used.Contains(name)) return name;
            int i = 1;
            while (used.Contains($"{name}_{i}")) i++;
            return $"{name}_{i}";
        }

        private static string BuildCode(IReadOnlyList<string> enumNames, IReadOnlyList<string> rawNamesFiltered)
        {
            var ns = "Core.Generated";
            var enumTypeName = "SceneGroupType";

            var enumLines = new StringBuilder();
            enumLines.AppendLine("        None = 0,");
            for (int i = 0; i < enumNames.Count; i++)
            {
                enumLines.Append("        ").Append(enumNames[i]).Append(" = ").Append(i + 1).Append(",\n");
            }

            var nameLines = new StringBuilder();
            nameLines.AppendLine("            \"\",");
            for (int i = 0; i < rawNamesFiltered.Count; i++)
            {
                nameLines
                    .Append("            \"")
                    .Append(rawNamesFiltered[i].Replace("\"", "\\\""))
                    .Append("\",\n");
            }

            return
@"using System;

namespace " + ns + @"
{
    // <auto-generated>
    // This file is automatically generated by the Unity Editor tool `SceneIdGeneratorEditor`.
    // Source data: SceneLoadServiceConfig.sceneAssetConfigs -> SceneAssetConfig.SceneGroupId / SceneDataList
    // Purpose: To provide a strongly-typed enum (SceneGroupType) for referencing scene group ids at compile time.
    // Do not edit this file manually — any changes will be overwritten when regenerated.
    // </auto-generated>

    public static class " + ScriptName + @"
    {
        private static readonly string[] Names = new string[]
        {
" + nameLines.ToString().TrimEnd('\n', ',') + @"
        };

        public static string GetSceneGroupId(" + enumTypeName + @" groupType)
        {
            return Names[(int)groupType];
        }

        public static " + enumTypeName + @" GetSceneGroupType(string groupId)
        {
            int index = Array.IndexOf(Names, groupId);
            return (" + enumTypeName + @")index;
        }
    }

    public enum " + enumTypeName + @"
    {
" + enumLines.ToString().TrimEnd('\n', ',') + @"
    }
}
";
        }

        /*internal class SceneIdAssetPostprocessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                var shouldRegenerate = false;

                foreach (var path in importedAssets)
                {
                    if (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) && AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SceneLoadServiceConfig))
                    {
                        shouldRegenerate = true;
                        break;
                    }
                }

                if (!shouldRegenerate)
                {
                    foreach (var path in importedAssets)
                    {
                        var type = AssetDatabase.GetMainAssetTypeAtPath(path);

                        if (type == null || !type.Name.Contains("SceneAssetConfig")) continue;

                        shouldRegenerate = true;

                        break;
                    }
                }

                if (shouldRegenerate)
                {
                    EditorApplication.delayCall += TryGenerate;
                }
            }
        }*/
    }
}
