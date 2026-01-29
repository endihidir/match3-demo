using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IParticleFXViewFactory
    {
        public T GetParticleFX<T>() where T : BaseParticleFXView;
        public void ReleaseParticleFX(BaseParticleFXView particleFX);
        public void ReleaseParticleFX(Transform particleFX);
        public void ReleaseParticleFXByType<T>() where T : BaseParticleFXView;
        public void RemoveParticleFXPoolByType<T>() where T : BaseParticleFXView;
    }
}