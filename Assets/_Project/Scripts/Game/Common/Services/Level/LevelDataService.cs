using System.Collections.Generic;
using Game.Configs;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Level.Data;
using Game.Utils;
using UnityEngine;

namespace Game.Level.Services
{
    public class LevelDataService : ILevelDataService
    {
        private readonly LevelDataServiceConfigSO _levelDataServiceConfigSo;
        public bool IsInitialized { get; private set; }
        public int LevelSize => LevelDefinitions?.Length ?? 0;
        public LevelDefinition[] LevelDefinitions { get; private set; }
        
        private bool PreventInitialMatches => _levelDataServiceConfigSo.PreventInitialMatches;
        private bool UseSeededPattern => _levelDataServiceConfigSo.UseSeededPattern;
        private int SeedOverride => _levelDataServiceConfigSo.SeedOverride;

        public LevelDataService(LevelDataServiceConfigSO levelDataServiceConfigSo)
        {
            _levelDataServiceConfigSo = levelDataServiceConfigSo;
        }

        public async UniTask<bool> InitializeAsync()
        {
            switch (_levelDataServiceConfigSo.SourceType)
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
            var assets = Resources.LoadAll<TextAsset>(_levelDataServiceConfigSo.ResourcesFolder);
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
            var path = _levelDataServiceConfigSo.GetResourcePath(level);
            var request = Resources.LoadAsync<TextAsset>(path);
            await request.ToUniTask();
            return request.asset as TextAsset;
        }
        
        public LevelDefinition GetLevelDefinition(int index) => LevelDefinitions[index];
    }
}