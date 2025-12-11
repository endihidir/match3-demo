using Core.Models;
using Core.Presenters;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public class GridViewContext : ManagedViewContext
    {
        [field: SerializeField] private GridView GridView { get; set; }
        
        protected override async UniTask Initialize()
        {
            var modelResolved = ObjectResolver.TryResolve<IGridModel>(out var gridModel);

            var presenterResolved = ObjectResolver.TryResolve<IGridPresenter>(out var gridPresenter);

            if (modelResolved && presenterResolved)
            {
                gridPresenter.Initialize(gridModel, GridView);
            }
            
            await UniTask.CompletedTask;
        }
    }
}