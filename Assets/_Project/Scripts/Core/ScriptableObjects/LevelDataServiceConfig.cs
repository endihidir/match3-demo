using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "LevelDataConfig", menuName = "Match3/LevelDataConfig")]
    public class LevelDataServiceConfig : ScriptableObject
    {
        public LevelSourceType sourceType;

        [Header("Resources Settings")]
        [ShowIf(nameof(IsResources))]
        public string resourcesFolder = "Levels";
        [ShowIf(nameof(IsResources))]
        public string fileNameFormat = "00";
      
        [Header("Addressables Settings")]
        [ShowIf(nameof(IsAddressable))]
        public string addressablesLabel;
        
        public string GetResourcePath(int level) => resourcesFolder + level.ToString(fileNameFormat);
        private bool IsAddressable => sourceType == LevelSourceType.Addressables;
        private bool IsResources => sourceType == LevelSourceType.Resources;
    }
    
    public enum LevelSourceType
    {
        Resources,
        Addressables
    }
}