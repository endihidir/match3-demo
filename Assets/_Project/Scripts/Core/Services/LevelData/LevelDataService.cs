using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Core.Level
{
    public interface ILevelDataBootState
    {
        bool IsInitialized { get; } 
        UniTask WaitUntilInitializedAsync(); 
    }

    public interface ILevelSerializer
    {
        LevelDefinition SerializeToLevelDefinition(int level);
    }

    public interface ILevelDataReader
    {
        int LevelSize { get; }
        LevelDefinition GetLevelDefinition(int index);
    }
    
    public class LevelDataService : IInitializable, ILevelDataBootState, ILevelSerializer, ILevelDataReader
    {
        private readonly LevelDataServiceConfig _levelDataServiceConfig;
        public bool IsInitialized { get; private set; }
        public int LevelSize => LevelDefinitions?.Length ?? 0;
        public LevelDefinition[] LevelDefinitions { get; private set; }

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

            var preventInitialMatches = _levelDataServiceConfig.preventInitialMatches;
            var useSeededPattern  = _levelDataServiceConfig.useSeededPattern;
            var seedOverride = _levelDataServiceConfig.seedOverride;
            
            foreach (var asset in assets)
            {
                var levelDefinition = LevelJsonRuntimeUtils.ParseToLevelDefinition(asset, preventInitialMatches, useSeededPattern, seedOverride);
                list.Add(levelDefinition);
            }
            
            list.Sort((a, b) => a.LevelNumber.CompareTo(b.LevelNumber));

            LevelDefinitions = list.ToArray();
        }
        
        public LevelDefinition SerializeToLevelDefinition(int level)
        {
            try
            {
                var preventInitialMatches = _levelDataServiceConfig.preventInitialMatches;
                var useSeededPattern  = _levelDataServiceConfig.useSeededPattern;
                var seedOverride = _levelDataServiceConfig.seedOverride;
                var jsonFile = Resources.Load<TextAsset>(_levelDataServiceConfig.GetResourcePath(level));
                return LevelJsonRuntimeUtils.ParseToLevelDefinition(jsonFile, preventInitialMatches, useSeededPattern, seedOverride);
            }
            catch (Exception e)
            {
                EditorLogger.LogError("JSON error:" + e);
                throw;
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