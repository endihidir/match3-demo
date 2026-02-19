using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "LevelDataServiceConfig", menuName = "Game/App/Services/LevelDataServiceConfig")]
    public sealed class LevelDataServiceConfigSO : ScriptableObject
    {
        [field: SerializeField] public LevelSourceType SourceType { get; private set; }
        [field: SerializeField, ShowIf(nameof(IsResources))] public string ResourcesFolder { get; private set; }= "Levels";
        [field: SerializeField, ShowIf(nameof(IsResources))] public string FileNameFormat { get; private set; }= "level_{0}";
        
        [field: SerializeField, ShowIf(nameof(IsAddressable))] public string AddressableLabel { get; private set; }
        
        [Tooltip("Prevents automatic matches on level start by re-rolling only randomly generated cells.")]
        [field: SerializeField] public bool PreventInitialMatches { get; private set; } = true;

        [Tooltip("Uses a deterministic random seed so the same level always starts with the same random layout.")]
        [field: SerializeField] public bool UseSeededPattern { get; private set; } = true;

        [Tooltip("Optional seed override. Set to 0 to use level-based seed.")]
        [field: SerializeField] public int SeedOverride { get; private set; } = 0;
        
        public string GetResourcePath(int level) => $"{ResourcesFolder}/{string.Format(FileNameFormat, level)}";
        private bool IsAddressable => SourceType == LevelSourceType.Addressables;
        private bool IsResources => SourceType == LevelSourceType.Resources;
    }
    
    public enum LevelSourceType
    {
        Resources,
        Addressables
    }
}