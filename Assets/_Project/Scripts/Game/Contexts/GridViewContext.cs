using Core.Models;
using Core.Presenters;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public class GridViewContext : ManagedViewContext
    {
        [SerializeField] private Transform _pivotPoint;
        
        [SerializeField] private MeshFilter _boardMeshFilter;
        
        protected override async UniTask Initialize()
        {
            var modelResolved = ObjectResolver.TryResolve<IMatch3GridModel>(out var match3GridModel);

            var presenterResolved = ObjectResolver.TryResolve<IMatch3GridPresenter>(out var match3GridPresenter);

            if (modelResolved && presenterResolved)
            {
                match3GridPresenter.Initialize(match3GridModel, _boardMeshFilter, _pivotPoint);
            }
            
            await UniTask.CompletedTask;
        }
    }
}