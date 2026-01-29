using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public sealed class ParticleFXViewFactory : IParticleFXViewFactory
    {
        private readonly IFXViewFactory _fxFactory;

        public ParticleFXViewFactory(IFXViewFactory fxFactory) => _fxFactory = fxFactory;
        
        public T GetParticleFX<T>() where T : BaseParticleFXView => _fxFactory.GetFX<T>();
        public void ReleaseParticleFX(BaseParticleFXView particleFX) => _fxFactory.ReleaseFX(particleFX);
        public void ReleaseParticleFX(Transform particleFX) => _fxFactory.ReleaseFX(particleFX);
        public void ReleaseParticleFXByType<T>() where T : BaseParticleFXView => _fxFactory.ReleaseFXByType<T>();
        public void RemoveParticleFXPoolByType<T>() where T : BaseParticleFXView => _fxFactory.RemoveFXPoolByType<T>();
    }
}