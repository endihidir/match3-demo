using Core.Configs;
using Core.Models;
using Core.Views;
using UnityEngine;

namespace Core.Handlers
{
    public interface IBoosterFxHandler
    {
        void PlayBoosterFx(BoosterActionBase boosterAction, Vector2Int originCoord, IGridModel model, IGridView view);
    }
}