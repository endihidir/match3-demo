using Core.MVPContext;
using UnityEngine;
using VContainer;

namespace Core.UI
{
    public interface IViewContext
    {
        public IMVPContext MVPContext { get; }
    }
    
    public abstract class BaseViewContext : MonoBehaviour, IViewContext
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