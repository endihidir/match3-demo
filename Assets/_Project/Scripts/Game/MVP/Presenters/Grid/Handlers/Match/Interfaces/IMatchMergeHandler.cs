using Cysharp.Threading.Tasks;
using Game.Grid.Item;
using Game.Models;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface IMatchMergeHandler
    {
        UniTask PlayMergeAnimationAsync(BaseGridObject[] mergeObjs, Vector2Int centerCoord);
        void SpawnBooster(Vector2Int pos, BoosterType boosterType);
    }
}
