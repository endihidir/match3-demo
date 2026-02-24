using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using Core.Utils;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "SceneLoadServiceConfig", menuName = "Game/App/Services/SceneLoadServiceConfig")]
    public sealed class SceneLoadServiceConfigSO : ScriptableObject
    {
        [field: SerializeField] private List<SceneAssetConfigSO> SceneAssetConfigs { get; set; }
        [field: SerializeField] public float ProgressSpeed { get; private set; } = 4f;
        
        public List<SceneReference> GetSceneGroupData(string sceneGroupId)
        {
            var sceneConfig = SceneAssetConfigs.FirstOrDefault(x => x.GetSceneGroupId() == sceneGroupId);

            var sceneReferences = new List<SceneReference>();

            if (!sceneConfig)
            {
                EditorLogger.LogError($"GetSceneData failed: config not found for sceneGroupId '{sceneGroupId}'.");
                return sceneReferences;
            }
            
            var sceneDataList = sceneConfig.SceneDataList;

            foreach (var groupSceneData in sceneDataList)
            {
                sceneReferences.Add(groupSceneData.sceneReference);
            }

            return sceneReferences;
        }
        
        public bool TryGetActiveSceneBy(string sceneGroupId, out UnityEngine.SceneManagement.Scene scene)
        {
            scene = default;
            
            var sceneConfig = SceneAssetConfigs.FirstOrDefault(x => x.GetSceneGroupId() == sceneGroupId);

            if (!sceneConfig)
            {
                EditorLogger.LogError($"TryGetActiveSceneById failed: config not found for sceneGroupId '{sceneGroupId}'.");
                return false;
            }
            
            var sceneDataList = sceneConfig.SceneDataList;

            foreach (var groupSceneData in sceneDataList)
            {
                if (groupSceneData.isActiveByDefault)
                {
                    scene = groupSceneData.sceneReference.LoadedScene;
                    
                    return true;
                }
            }
            
            return false;
        }
    }
}