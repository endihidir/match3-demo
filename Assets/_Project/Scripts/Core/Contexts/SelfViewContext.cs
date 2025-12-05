using Core.MVPContext;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public abstract class SelfViewContext : MonoBehaviour, IContextOwner
    {
        private IMVPContextService MvpContextService { get; set; }
        public IMVPContext OwnerContext { get; private set; }

        [Inject]
        private void Construct(IMVPContextService mvpContextService)
        {
            MvpContextService = mvpContextService;
            
            OwnerContext = MvpContextService.GetContext(this);
            
            Initialize();
        }
        
        protected abstract void Initialize();
        protected virtual void OnDestroy() => MvpContextService?.Release(this);
    }
}