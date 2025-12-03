using Core.MVPContext;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public abstract class SelfViewContext : MonoBehaviour, IContextOwner
    {
        private IMVPContextService _mvpContextService;
        public IMVPContext MVPContext { get; private set; }

        [Inject]
        private void Construct(IMVPContextService mvpContextService)
        {
            _mvpContextService = mvpContextService;
            
            MVPContext = _mvpContextService.GetContext(this);
            
            Initialize();
        }
        
        protected abstract void Initialize();
        protected virtual void OnDestroy() => _mvpContextService?.Release(this);
    }
}