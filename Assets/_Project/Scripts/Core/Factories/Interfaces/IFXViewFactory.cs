using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IFXViewFactory
    {
        public T GetFX<T>(bool activate = true) where T : BaseFxView;
        public void ReleaseFX(BaseFxView fx);
        public void ReleaseFX(Transform fx);
        public void ReleaseFXByType<T>() where T : BaseFxView;
        public void RemoveFXPoolByType<T>() where T : BaseFxView;
    }
}