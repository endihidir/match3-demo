using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Core.Level
{
    public interface ILevelDataService
    {
        bool HasInit { get; }
        LevelDefinition[] LevelDefinitions { get; }
        LevelDefinition SerializeToLevelDefinition(int level);
    }
    
    public class LevelDataService : ILevelDataService, IInitializable
    {
        private readonly LevelDataServiceConfig _levelDataServiceConfig;
        public bool HasInit { get; private set; }
        public LevelDefinition[] LevelDefinitions { get; private set; }

        public LevelDataService(AppConfigContainer appConfigContainer)
        {
            _levelDataServiceConfig = appConfigContainer.levelDataServiceConfig;
        }
        
        public void Initialize()
        {
            Init().Forget();
        }

        private async UniTask Init()
        {
            switch (_levelDataServiceConfig.sourceType)
            {
                case LevelSourceType.Resources:
                    InitializeFromResources();
                    break;
                case LevelSourceType.Addressables:
                    EditorDebug.LogError("Addressables are not supported yet!");
                    return;
            }

            await UniTask.Yield();
            
            HasInit = true;
        }
        
        private void InitializeFromResources()
        {
            var assets = Resources.LoadAll<TextAsset>(_levelDataServiceConfig.resourcesFolder);
            
            var list = new List<LevelDefinition>(assets.Length);

            foreach (var asset in assets)
            {
                var levelDefinition = LevelJsonUtility.ParseToLevelDefinition(asset);
                list.Add(levelDefinition);
            }
            
            list.Sort((a, b) => a.LevelNumber.CompareTo(b.LevelNumber));

            LevelDefinitions = list.ToArray();
        }
        
        public LevelDefinition SerializeToLevelDefinition(int level)
        {
            try
            {
                var jsonFile = Resources.Load<TextAsset>(_levelDataServiceConfig.GetResourcePath(level));
                return LevelJsonUtility.ParseToLevelDefinition(jsonFile);
            }
            catch (Exception e)
            {
                EditorDebug.Log("JSON error:" + e);
                throw;
            }
        }
    }
}