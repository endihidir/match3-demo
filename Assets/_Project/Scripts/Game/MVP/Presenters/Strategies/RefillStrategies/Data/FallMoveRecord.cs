using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct FallMoveRecord
    {
        public readonly BaseGridObject Obj;
        public readonly Vector3 FinalWorld;
        public readonly float DurMul;
        public readonly float Delay;

        public FallMoveRecord(BaseGridObject obj, Vector3 finalWorld, float durMul, float delay)
        {
            Obj = obj;
            FinalWorld = finalWorld;
            DurMul = durMul;
            Delay = delay;
        }
    }
}