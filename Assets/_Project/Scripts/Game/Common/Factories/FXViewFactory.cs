using Core.Pool.Services;
using Game.Views;
using UnityEngine;

namespace Game.View.Factories
{
    public sealed class FXViewFactory : IFXViewFactory
    {
        private readonly IObjectPoolService _objectPoolService;

        public FXViewFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetFX<T>(bool activate = true) where T : BaseFxView => _objectPoolService.GetObject<T>(activate);
        public void ReleaseFX(BaseFxView fx) => _objectPoolService.ReturnObject(fx);
        public void ReleaseFX(Transform fx) => _objectPoolService.ReturnObject(fx);
        public void ReleaseFXesByType<T>() where T : BaseFxView => _objectPoolService.ReturnObjectsByType<T>();
        public void RemoveFXPoolByType<T>() where T : BaseFxView => _objectPoolService.RemovePoolsByType<T>();
    }
}