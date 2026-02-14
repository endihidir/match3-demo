using System;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IBoosterFxHandler
    {
        UniTask PlayBoosterFxAsync(BoosterActionContext action, IGridModel model, IGridView view, out float animSpeed);
    }
}