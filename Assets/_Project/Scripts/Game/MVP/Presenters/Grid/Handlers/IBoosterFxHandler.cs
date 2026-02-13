using System;
using Core.Views;

namespace Core.Handlers
{
    public interface IBoosterFxHandler
    {
        float PlayBoosterFx(BoosterActionContext action, IGridView view, Action onComplete);
    }
}