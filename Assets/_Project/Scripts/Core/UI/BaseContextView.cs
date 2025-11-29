using Core.MVPContext;
using UnityEngine;
using VContainer;

namespace Core.UI
{
    public interface IContextView
    {
        public IMVPContext MVPContext { get; }
    }
    
    public abstract class BaseContextView : MonoBehaviour, IContextView
    {
        private IMVPContextService _mvpContextService;
        public IMVPContext MVPContext { get; private set; }

        [Inject]
        private void Construct(IMVPContextService uiContextContainer)
        {
            _mvpContextService = uiContextContainer;
            
            MVPContext = _mvpContextService.GetContext(this);
            
            Initialize();
        }
        
        protected abstract void Initialize();
        protected virtual void OnDestroy() => _mvpContextService?.Release(this);
    }
}