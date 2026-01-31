using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Core.Level
{
    public class LevelDataService : IInitializable, ILevelDataBootState, ILevelSerializer, ILevelDataReader
    {
        private readonly LevelDataServiceConfig _levelDataServiceConfig;
        public bool IsInitialized { get; private set; }
        public int LevelSize => LevelDefinitions?.Length ?? 0;
        public LevelDefinition[] LevelDefinitions { get; private set; }
        
        private bool PreventInitialMatches => _levelDataServiceConfig.preventInitialMatches;
        private bool UseSeededPattern => _levelDataServiceConfig.useSeededPattern;
        private int SeedOverride => _levelDataServiceConfig.seedOverride;

        public LevelDataService(AppConfigContainer appConfigContainer)
        {
            _levelDataServiceConfig = appConfigContainer.levelDataServiceConfig;
        }
        public void Initialize() => Init().Forget();

        private async UniTask Init()
        {
            switch (_levelDataServiceConfig.sourceType)
            {
                case LevelSourceType.Resources:
                    InitializeFromResources();
                    break;
                case LevelSourceType.Addressables:
                    EditorLogger.LogError("Addressables are not supported yet!");
                    return;
            }

            await UniTask.Yield();
            
            IsInitialized = true;
        }
        
        private void InitializeFromResources()
        {
            var assets = Resources.LoadAll<TextAsset>(_levelDataServiceConfig.resourcesFolder);
            var list = new List<LevelDefinition>(assets.Length);
            
            foreach (var asset in assets)
            {
                var levelDefinition = LevelJsonRuntimeUtils.ParseToLevelDefinition(asset, PreventInitialMatches, UseSeededPattern, SeedOverride);
                list.Add(levelDefinition);
            }
            
            list.Sort(static (a, b) => a.LevelNumber.CompareTo(b.LevelNumber));
            LevelDefinitions = list.ToArray();
        }
        
        public LevelDefinition SerializeToLevelDefinition(int level)
        {
            try
            {
                var jsonFile = Resources.Load<TextAsset>(_levelDataServiceConfig.GetResourcePath(level));
                return LevelJsonRuntimeUtils.ParseToLevelDefinition(jsonFile, PreventInitialMatches, UseSeededPattern, SeedOverride);
            }
            catch (Exception e)
            {
                EditorLogger.LogError("JSON error:" + e);
                return null;
            }
        }
        
        public async UniTask WaitUntilInitializedAsync()
        {
            while (!IsInitialized)
            {
                await UniTask.Yield();
            }
        }
        
        public LevelDefinition GetLevelDefinition(int index) => LevelDefinitions[index];
    }
}