using Core.Pool;
using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public sealed class FXViewFactory : IFXViewFactory
    {
        private readonly IObjectPoolService _objectPoolService;

        public FXViewFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetFX<T>(bool show = true) where T : BaseFxView => _objectPoolService.GetObject<T>(show);

        public void ReleaseFX(BaseFxView fx) => _objectPoolService.ReturnObject(fx);
        public void ReleaseFX(Transform fx) => _objectPoolService.ReturnObject(fx);
        public void ReleaseFXByType<T>() where T : BaseFxView => _objectPoolService.ReturnObjectsByType<T>();
        public void RemoveFXPoolByType<T>() where T : BaseFxView => _objectPoolService.RemovePoolsByType<T>();
    }
}