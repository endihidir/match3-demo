using System.Linq;
using Core.MVPContext;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public abstract class RootViewContext : MonoBehaviour, IContextOwner
    {
        [field: SerializeField] private ManagedViewContext[] ManagedContexts { get; set; }
        private IMVPContextService MvpContextService { get; set; }
        public IMVPContext OwnerContext { get; private set; }
        private bool IsPlaying => Application.isPlaying;
        
        [Inject]
        private void Construct(IMVPContextService mvpContextService)
        {
            MvpContextService = mvpContextService;
            
            OwnerContext = MvpContextService.GetContext(this);

            ConstructAsync().Forget();
        }

        private async UniTask ConstructAsync()
        {
            await Initialize();
            await InitializeChildren();
        }

        protected abstract UniTask Initialize();
        protected virtual async UniTask InitializeChildren()
        {
            foreach (var child in ManagedContexts)
            {
                var childContext = MvpContextService.GetContext(child);
                
                await child.Construct(OwnerContext, childContext);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var child in ManagedContexts)
            {
                MvpContextService.Release(child);
            }
            
            MvpContextService.Release(this);
        }
        
        public bool TryGetChildContext<T>(out T context) where T : IContextOwner
        {
            var selectedContext = ManagedContexts.FirstOrDefault(x => x is T);
         
            if (selectedContext is T contextOwner)
            {
                context = contextOwner;
                return true;
            }
            
            context = default;
            return false;
        }

        [Button, HideIf(nameof(IsPlaying))]
        private void PullChildContexts() => ManagedContexts = GetComponentsInChildren<ManagedViewContext>();
    }
}