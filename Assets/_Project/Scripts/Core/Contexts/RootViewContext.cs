using Core.MVPContext;
using NaughtyAttributes;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public abstract class RootViewContext : MonoBehaviour, IContextOwner
    {
        [field: SerializeField] private ManagedViewContext[] ManagedContexts { get; set; }

        private IMVPContextService _mvpContextService;
        public IMVPContext MVPContext { get; private set; }
        private bool IsPlaying => Application.isPlaying;

        [Inject]
        private void Construct(IMVPContextService mvpContextService)
        {
            _mvpContextService = mvpContextService;
            MVPContext = _mvpContextService.GetContext(this);

            Initialize();          
            InitializeChildren();
        }

        protected abstract void Initialize();
        protected virtual void InitializeChildren()
        {
            foreach (var child in ManagedContexts)
            {
                child?.Construct(_mvpContextService);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var child in ManagedContexts)
            {
                _mvpContextService?.Release(child);
            }
            
            _mvpContextService?.Release(this);
        }
        
        [Button, HideIf(nameof(IsPlaying))]
        private void PullChildContexts() => ManagedContexts = GetComponentsInChildren<ManagedViewContext>();
    }
}