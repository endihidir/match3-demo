using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "SceneAsset", menuName = "Game/App/Assets/SceneAssetConfig")]
    public class SceneAssetConfigSO : ScriptableObject
    {
        [field: SerializeField, ShowIf(nameof(HasMultipleSceneData))] 
        private string SceneGroupId { get; set; }
        [field: SerializeField] public List<GroupSceneData> SceneDataList { get;  private set; }
        private bool HasMultipleSceneData => SceneDataList.Count > 1;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(SceneGroupId)) return;
            
            SceneGroupId = GetSceneGroupId();
                
            EditorUtility.SetDirty(this);
        }
#endif

        public string GetSceneGroupId()
        {
            if (SceneDataList.Count < 1)
            {
                SceneGroupId = string.Empty;
            }
            else if (SceneDataList.Count == 1)
            {
                var sceneData = SceneDataList[0];
                
                if (sceneData.sceneReference != null)
                {
                    sceneData.isActiveByDefault = true;
                    SceneGroupId = sceneData.sceneReference.Name;
                }

                SceneDataList[0] = sceneData;
            }
            
            return SceneGroupId;
        }
    }

    [Serializable]
    public struct GroupSceneData
    {
        public bool isActiveByDefault;
        public SceneReference sceneReference;
    }
}