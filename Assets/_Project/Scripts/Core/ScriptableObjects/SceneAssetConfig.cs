using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = "Match3/SceneConfigs/SceneAssetConfig")]
    public class SceneAssetConfig : ScriptableObject
    {
        public string sceneId;
        public List<GroupSceneData> sceneDataList;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneDataList.Count < 1)
            {
                sceneId = string.Empty;
            }
            else if (sceneDataList.Count == 1)
            {
                var sceneData = sceneDataList[0];
                
                if (sceneData.sceneReference != null)
                {
                    sceneData.isActiveByDefault = true;
                    sceneId = sceneData.sceneReference.Name;
                }

                sceneDataList[0] = sceneData;
            }
            
            EditorUtility.SetDirty(this);
        }
#endif
    }

    [Serializable]
    public struct GroupSceneData
    {
        public bool isActiveByDefault;
        public SceneReference sceneReference;
    }
}