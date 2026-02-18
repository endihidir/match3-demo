using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
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

        [Header("Initial Board Randomization")]
        [Tooltip("Prevents automatic matches on level start by re-rolling only randomly generated cells.")]
        public bool preventInitialMatches;

        [Tooltip("Uses a deterministic random seed so the same level always starts with the same random layout.")]
        public bool useSeededPattern;

        [Tooltip("Optional seed override. Set to 0 to use level-based seed.")]
        public int seedOverride = 0;
        
        public string GetResourcePath(int level) => $"{resourcesFolder}/{string.Format(fileNameFormat, level)}";
        private bool IsAddressable => sourceType == LevelSourceType.Addressables;
        private bool IsResources => sourceType == LevelSourceType.Resources;
    }
    
    public enum LevelSourceType
    {
        Resources,
        Addressables
    }
}