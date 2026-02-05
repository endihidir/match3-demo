using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public partial class SlideDownFillStrategy
    {
        // Records & path nodes
        // =========================================================

        private void AddStep(BaseGridObject item, Vector2Int coord, bool isSpawn)
        {
            var index = _recordBuffer.AddOrGet(item, new SlideDownMoveRecord(item));
            ref var record = ref _recordBuffer.GetRef(index);

            record.IsSpawn |= isSpawn;

            // Avoid pushing the same coord twice in a row
            if (record.TailNode >= 0 && _pathPool.GetCoord(record.TailNode) == coord)
            {
                record.FinalCoord = coord;
                return;
            }

            var node = _pathPool.Allocate(coord);
            record.AppendNode(node, coord, ref _pathPool);
        }
    }
}