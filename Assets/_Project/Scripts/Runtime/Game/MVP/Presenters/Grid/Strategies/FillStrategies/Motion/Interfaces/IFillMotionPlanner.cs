using Cysharp.Threading.Tasks;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public interface IFillMotionPlanner
    {
        void Begin();
        void RecordMove(BaseGridObject item, Vector2Int from, Vector2Int to);
        void RecordSpawn(BaseGridObject item, int column, int stackIndex, Vector2Int target);
        UniTask Commit();
    }
}