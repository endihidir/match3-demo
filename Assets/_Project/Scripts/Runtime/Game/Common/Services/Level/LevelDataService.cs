using System.Collections.Generic;
using Game.Configs;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Level.Data;
using Game.Utils;
using UnityEngine;

namespace Game.Level.Services
{
    public sealed class LevelDataService : ILevelDataService
    {
        private readonly LevelDataServiceConfigSO _levelDataServiceConfig;
        public bool IsInitialized { get; private set; }
        public bool IsOverrideLevelIndex => _levelDataServiceConfig.IsOverrideLevelIndex;
        public int LevelIndexOverride => Mathf.Max(_levelDataServiceConfig.LevelIndexOverride, LevelSize - 1);
        public int LevelSize => LevelDefinitions?.Length ?? 0;
        public LevelDefinition[] LevelDefinitions { get; private set; }
        
        private bool PreventInitialMatches => _levelDataServiceConfig.PreventInitialMatches;
        private bool UseSeededPattern => _levelDataServiceConfig.UseSeededPattern;
        private int SeedOverride => _levelDataServiceConfig.SeedOverride;

        public LevelDataService(LevelDataServiceConfigSO levelDataServiceConfig)
        {
            _levelDataServiceConfig = levelDataServiceConfig;
        }

        public async UniTask<bool> InitializeAsync()
        {
            switch (_levelDataServiceConfig.SourceType)
            {
                case LevelSourceType.Resources:
                    InitializeFromResources();
                    break;
                case LevelSourceType.Addressables:
                    EditorLogger.LogError("Addressables are not supported yet!");
                    return false;
            }
            
            await UniTask.Yield();
            
            IsInitialized = true;
            
            return IsInitialized;
        }
        
        private void InitializeFromResources()
        {
            var assets = Resources.LoadAll<TextAsset>(_levelDataServiceConfig.ResourcesFolder);
            var list = new List<LevelDefinition>(assets.Length);

            foreach (var asset in assets)
            {
                var definition = LevelJsonRuntimeUtils.ParseToLevelDefinition(
                    asset, PreventInitialMatches, UseSeededPattern, SeedOverride);
                list.Add(definition);
            }

            list.Sort(static (a, b) => a.LevelNumber.CompareTo(b.LevelNumber));
            LevelDefinitions = list.ToArray();
        }
        
        public async UniTask<LevelDefinition> LoadLevelDefinitionAsync(int level)
        {
            var textAsset = await LoadTextAssetAsync(level);
            return !textAsset ? null : LevelJsonRuntimeUtils.ParseToLevelDefinition(textAsset, PreventInitialMatches, UseSeededPattern, SeedOverride);
        }

        private async UniTask<TextAsset> LoadTextAssetAsync(int level)
        {
            var path = _levelDataServiceConfig.GetResourcePath(level);
            var request = Resources.LoadAsync<TextAsset>(path);
            await request.ToUniTask();
            return request.asset as TextAsset;
        }
        
        public LevelDefinition GetLevelDefinition(int index) => LevelDefinitions[index];
    }
}