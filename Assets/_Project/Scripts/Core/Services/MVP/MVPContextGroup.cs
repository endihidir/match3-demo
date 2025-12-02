using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using Core.MVPContext.Interfaces;
using VContainer;

namespace Core.MVPContext
{
    public interface IMVPContextGroup : IDisposable
    {
        public IObjectResolver ObjectResolver { get; }
        TView ResolveView<TView>() where TView : class, IView;
        TModel ResolveModel<TModel>() where TModel : class, IModel;
        TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter;
        void UpdatePresenters();
    }

    public class MVPContextGroup : IMVPContextGroup
    {
        private readonly List<IModel> _models = new();
        private readonly List<IView> _views = new();
        private readonly List<IPresenter> _presenters = new();

        public IObjectResolver ObjectResolver { get; }
        public MVPContextGroup(IObjectResolver objectResolver) => ObjectResolver = objectResolver;

        public TModel ResolveModel<TModel>() where TModel : class, IModel => GetOrCreate<IModel, TModel>(_models);
        public TView ResolveView<TView>() where TView : class, IView => GetOrCreate<IView, TView>(_views, false);
        public TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter => GetOrCreate<IPresenter, TPresenter>(_presenters);

        private TImpl GetOrCreate<TIFace, TImpl>(List<TIFace> map, bool useResolver = true) where TIFace : class where TImpl : class, TIFace
        {
            var context = map.FirstOrDefault(x=> x is TImpl);

            if (context != null) return context as TImpl;
            
            var result = useResolver ? ObjectResolver.CreateInstance<TImpl>() : Activator.CreateInstance<TImpl>();
            
            map.Add(result);

            return result;
        }
        
        public void UpdatePresenters()
        {
            foreach (var presenter in _presenters)
            {
                if (presenter is IUpdater updater)
                {
                    updater.Update();
                }
            }
        }
        
        public void Dispose()
        {
            DisposeAll(_models);
            DisposeAll(_presenters);
            DisposeAll(_views);
            
            _models.Clear();
            _presenters.Clear();
            _views.Clear();
        }

        private static void DisposeAll<T>(IEnumerable<T> items)
        {
            foreach (var it in items)
                if (it is IDisposable d) d.Dispose();
        }
    }
}