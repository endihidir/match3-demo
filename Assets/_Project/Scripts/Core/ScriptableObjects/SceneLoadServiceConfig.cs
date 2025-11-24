using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using Core.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "SceneLoadServiceConfig", menuName = "Match3/Services/SceneLoadServiceConfig", order = 0)]
    public class SceneLoadServiceConfig : ScriptableObject
    {
        [SerializeField] private List<SceneAssetConfig> sceneAssetConfigs;
        
        public List<SceneReference> GetSceneData(string sceneId)
        {
            var sceneConfig = sceneAssetConfigs.FirstOrDefault(x => x.sceneId == sceneId);

            var sceneReferences = new List<SceneReference>();

            if (!sceneConfig)
            {
                ConditionalDebug.LogError($"GetSceneData failed: config not found for sceneId '{sceneId}'.");
                return sceneReferences;
            }
            
            var sceneDataList = sceneConfig.sceneDataList;

            foreach (var groupSceneData in sceneDataList)
            {
                sceneReferences.Add(groupSceneData.sceneReference);
            }

            return sceneReferences;
        }
        
        public bool TryGetActiveSceneById(string sceneId, out Scene scene)
        {
            scene = default;
            
            var sceneConfig = sceneAssetConfigs.FirstOrDefault(x => x.sceneId == sceneId);

            if (!sceneConfig)
            {
                ConditionalDebug.LogError($"TryGetActiveSceneById failed: config not found for sceneId '{sceneId}'.");
                return false;
            }
            
            var sceneDataList = sceneConfig.sceneDataList;

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