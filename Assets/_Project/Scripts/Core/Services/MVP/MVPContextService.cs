using System;
using System.Collections.Generic;
using Core.MVPContext.Interfaces;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.MVPContext
{
    public interface IMVPContextService
    {
        IMVPContext GetContext(int ownerID);
        IMVPContext GetContext(Component owner) => GetContext(owner.GetInstanceID());
        IMVPContext GetContext(GameObject owner) => GetContext(owner.GetInstanceID());
        bool Release(int ownerID);
        bool Release(Component owner) => Release(owner.GetInstanceID());
        bool Release(GameObject owner) => Release(owner.GetInstanceID());
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

        public IMVPContext GetContext(int ownerID)
        {
            if (_contexts.TryGetValue(ownerID, out var context)) return context;
            context = new MVPContext(_objectResolver);
            _contexts[ownerID] = context;
            return context;
        }

        public bool Release(int ownerID)
        {
            if (_contexts.TryGetValue(ownerID, out var ownerContext)) 
                ownerContext?.Dispose();
            
            return _contexts.Remove(ownerID);
        }
        
        public void Tick()
        {
            foreach (var ownerContext in _contexts.Values) 
                ownerContext.UpdatePresenters();
        }

        private sealed class MVPContext : IMVPContext
        {
            private readonly IMVPContextGroup _mvpContextGroup;
            public MVPContext(IObjectResolver objectResolver) => _mvpContextGroup = new MVPContextGroup(objectResolver);

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