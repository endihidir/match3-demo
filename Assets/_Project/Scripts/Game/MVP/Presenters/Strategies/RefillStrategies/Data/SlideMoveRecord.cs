using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct SlideMoveRecord
    {
        public readonly BaseGridObject Obj;
        public readonly Vector3 SlidePos;
        public readonly Vector3 FinalWorld;
        public readonly float DurMul;
        public readonly float Delay;

        public SlideMoveRecord(BaseGridObject obj, Vector3 slidePos, Vector3 finalWorld, float durMul, float delay)
        {
            Obj = obj;
            SlidePos = slidePos;
            FinalWorld = finalWorld;
            DurMul = durMul;
            Delay = delay;
        }
    }
}