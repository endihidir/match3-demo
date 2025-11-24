using System;
using System.Collections.Generic;
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
        bool TryGetPresenter<TPresenter>(out TPresenter value) where TPresenter : class, IPresenter;
        void UpdatePresenters();
    }

    public class MVPContextGroup : IMVPContextGroup
    {
        private readonly IDictionary<Type, IView> _views = new Dictionary<Type, IView>();
        private readonly IDictionary<Type, IModel> _models = new Dictionary<Type, IModel>();
        private readonly IDictionary<Type, IPresenter> _presenters = new Dictionary<Type, IPresenter>();

        public IObjectResolver ObjectResolver { get; }

        public MVPContextGroup(IObjectResolver objectResolver) => ObjectResolver = objectResolver;

        public TView ResolveView<TView>() where TView : class, IView => GetOrCreate<IView, TView>(_views);
        public TModel ResolveModel<TModel>() where TModel : class, IModel => GetOrCreate<IModel, TModel>(_models);
        public TPresenter ResolvePresenter<TPresenter>() where TPresenter : class, IPresenter => GetOrCreate<IPresenter, TPresenter>(_presenters);

        private TImpl GetOrCreate<TIFace, TImpl>(IDictionary<Type, TIFace> map) where TIFace : class where TImpl : class, TIFace
        {
            var key = typeof(TImpl);
            
            if (!map.TryGetValue(key, out var obj))
            {
                obj = ObjectResolver.CreateInstance<TImpl>();
                map[key] = obj;
                IndexAssignableInterfaces(map, obj);
              
            }

            return obj as TImpl;
        }

        private static void IndexAssignableInterfaces<TIFace>(IDictionary<Type, TIFace> map, TIFace obj) where TIFace : class
        {
            var implType = obj.GetType();
            var ifaces = implType.GetInterfaces();
            
            for (int i = 0; i < ifaces.Length; i++)
            {
                var iface = ifaces[i];
                if (typeof(TIFace).IsAssignableFrom(iface))
                    map.TryAdd(iface, obj);
            }
        }
        
        public bool TryGetPresenter<TPresenter>(out TPresenter value) where TPresenter : class, IPresenter => TryGet(_presenters, out value);
        public void UpdatePresenters()
        {
            foreach (var presentersValue in _presenters.Values)
            {
                if (presentersValue is IUpdater updater)
                {
                    updater.Update();
                }
            }
        }

        private static bool TryGet<TIFace, TImpl>(IDictionary<Type, TIFace> map, out TImpl typed) where TIFace : class where TImpl : class, TIFace
        {
            if (map.TryGetValue(typeof(TImpl), out var obj))
            {
                typed = obj as TImpl;
                return typed != null;
            }

            foreach (var v in map.Values)
            {
                if (v is TImpl casted) { typed = casted; return true; }
            }

            typed = null;
            return false;
        }
        
        public void Dispose()
        {
            DisposeAll(_models.Values);
            DisposeAll(_presenters.Values);
            DisposeAll(_views.Values);

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
