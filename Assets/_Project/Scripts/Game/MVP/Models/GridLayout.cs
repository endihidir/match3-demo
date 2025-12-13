using System;
using UnityEngine;

namespace Core.Grid
{
    [Serializable]
    public struct GridLayout
    {
        public float cellSize;
        public Vector3 originOffset;
        public float screenSidePaddingRatio;
        public float cellSpacingRatio;
    }
}