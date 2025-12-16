using System;
using UnityEngine;

namespace Core.Grid
{
    [Serializable]
    public struct GridLayout
    {
        public float cellSize;
        public Vector3 originOffset;
        [HideInInspector] public float screenSidePaddingRatio;
        [HideInInspector] public float cellSpacingRatio;
    }
}