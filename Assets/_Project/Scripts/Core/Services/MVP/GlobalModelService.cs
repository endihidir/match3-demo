using System;
using System.Collections.Generic;
using Core.Extensions;
using Core.MVPContext.Interfaces;
using Core.SaveSystem;
using VContainer;

namespace Core.MVPContext
{
    public interface IGlobalModelService
    {
        TModel Resolve<TModel>() where TModel : class, IModel;
    }
    
    public class GlobalModelService : IGlobalModelService
    {
        private readonly Dictionary<Type, IModel> _models = new();
        
        private readonly IObjectResolver _objectResolver;
        
        private static readonly HashSet<ISaveData> SaveData = new();

        public GlobalModelService(IObjectResolver objectResolver) => _objectResolver = objectResolver;

        public TModel Resolve<TModel>() where TModel : class, IModel
        {
            var t = typeof(TModel);
            
            if (!_models.TryGetValue(t, out var m))
            {
                m = _objectResolver.CreateInstance<TModel>();
                
                _models[t] = m;
                
                if (m is ISaveData sd)
                {
                    SaveData.Add(sd);
                }
            }
            
            return m as TModel;
        }

        [InvokeOnQuit]
        public static void SaveAll()
        {
            foreach (var sd in SaveData) sd?.Save();
        }
    }
}