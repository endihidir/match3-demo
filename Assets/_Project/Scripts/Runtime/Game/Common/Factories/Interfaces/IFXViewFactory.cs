using Game.Views;
using UnityEngine;

namespace Game.View.Factories
{
    public interface IFXViewFactory
    {
        public T GetFX<T>(bool activate = true) where T : BaseFxView;
        public void ReleaseFX(BaseFxView fx);
        public void ReleaseFX(Transform fx);
        public void ReleaseFXesByType<T>() where T : BaseFxView;
        public void RemoveFXPoolByType<T>() where T : BaseFxView;
    }
}