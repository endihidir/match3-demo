using System;
using System.Collections.Generic;
using Core.MVPContext.Interfaces;
using Core.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.MVPContext
{
    public interface IMVPContextService
    {
        IMVPContext GetContext(Component owner);
        IMVPContext GetContext(GameObject owner);
        IMVPContext GetContext(int ownerID);
        bool Release(Component owner);
        bool Release(GameObject owner);
        bool Release(int ownerID);
    }

    public interface IMVPContext : IDisposable
    {
        TModel ResolveModel<TModel>() where TModel : class, IModel;
        TModel ResolveGlobalModel<TModel>() where TModel : class, IModel;
        TView ResolveView<TView>() where TView : class, IView;
        TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter;
        void UpdatePresenters();
    }

    public class MVPContextService : IMVPContextService, ITickable
    {
        private readonly Dictionary<int, IMVPContext> _contexts = new();
        private readonly IObjectResolver _objectResolver;

        public MVPContextService(IObjectResolver objectResolver) => _objectResolver = objectResolver;

        public IMVPContext GetContext(Component owner)
        {
            if (!owner)
            {
                EditorLogger.LogError("[ContextFactory] GetContext(Component): owner is null.");
                return null;
            }
            
            return GetContext(owner.GetInstanceID());
        }

        public IMVPContext GetContext(GameObject owner)
        {
            if (!owner)
            {
                EditorLogger.LogError("[ContextFactory] GetContext(GameObject): owner is null.");
                return null;
            }
            
            return GetContext(owner.GetInstanceID());
        }

        public IMVPContext GetContext(int ownerID)
        {
            if (!_contexts.TryGetValue(ownerID, out var context))
            {
                context = new MVPContext(_objectResolver);
                _contexts[ownerID] = context;
            }

            return context;
        }

        public bool Release(Component owner)
        {
            if (!owner)
            {
                EditorLogger.LogError("[ContextFactory] Release(Component): owner is null.");
                return false;
            }
            
            return Release(owner.GetInstanceID());
        }

        public bool Release(GameObject owner)
        {
            if (!owner)
            {
                EditorLogger.LogError("[ContextFactory] Release(GameObject): owner is null.");
                return false;
            }
            
            return Release(owner.GetInstanceID());
        }

        public bool Release(int ownerID)
        {
            if (_contexts.TryGetValue(ownerID, out var ownerContext))
            {
                ownerContext?.Dispose();
            }
            
            return _contexts.Remove(ownerID);
        }
        
        public void Tick()
        {
            foreach (var ownerContext in _contexts.Values)
            {
                ownerContext.UpdatePresenters();
            }
        }

        private sealed class MVPContext : IMVPContext
        {
            private readonly IMVPContextGroup _mvpContextGroup;

            public MVPContext(IObjectResolver objectResolver)
            {
                _mvpContextGroup = new MVPContextGroup(objectResolver);
            }

            public TModel ResolveModel<TModel>() where TModel : class, IModel => _mvpContextGroup.ResolveModel<TModel>();
            public TModel ResolveGlobalModel<TModel>() where TModel : class, IModel
            {
                var globalModelContainer = _mvpContextGroup.ObjectResolver.Resolve<IGlobalModelService>();
                return globalModelContainer.Resolve<TModel>();
            }

            public TView ResolveView<TView>() where TView : class, IView => _mvpContextGroup.ResolveView<TView>();
            public TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter => _mvpContextGroup.ResolvePresenter<TPresenter>();
            public void UpdatePresenters() => _mvpContextGroup.UpdatePresenters();
            public void Dispose() => _mvpContextGroup.Dispose();
        }
    }
}