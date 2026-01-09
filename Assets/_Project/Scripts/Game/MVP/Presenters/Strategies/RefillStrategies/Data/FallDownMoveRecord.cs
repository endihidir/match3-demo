using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct FallDownMoveRecord
    {
        public readonly BaseGridObject Obj;
        public readonly Vector3 FinalWorld;
        public readonly int ColumnX;
        public readonly bool IsSpawn;

        public FallDownMoveRecord(BaseGridObject obj, Vector3 finalWorld, int columnX, bool isSpawn)
        {
            Obj = obj;
            FinalWorld = finalWorld;
            ColumnX = columnX;
            IsSpawn = isSpawn;
        }
    }
}