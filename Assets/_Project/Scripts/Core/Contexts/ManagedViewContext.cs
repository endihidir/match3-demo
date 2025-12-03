using Core.MVPContext;
using UnityEngine;

namespace Core.Context
{
    public abstract class ManagedViewContext : MonoBehaviour, IContextOwner
    { 
        public IMVPContext MVPContext { get; private set; }
        public void Construct(IMVPContextService mvpContextService)
        {
            MVPContext = mvpContextService.GetContext(this);
        
            Initialize();
        }

        protected abstract void Initialize();
    }
}