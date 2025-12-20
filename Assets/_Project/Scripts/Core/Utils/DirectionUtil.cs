using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils
{
    public static class DirectionUtil
    {
        public static readonly Vector2Int[] MainDirections =
        { 
            new (0, 1),
            new (1, 0), 
            new (-1, 0), 
            new (0,-1) 
        };
        
        public static readonly Vector2Int[] AllDirections =
        {
            new (0, 0), 
            new (1, 0),
            new (-1, 0),
            new (0, -1),
            new (0, 1),
            new (1, -1),
            new (-1, -1),
            new (1, 1),
            new (-1, 1)
        };
        
        public static readonly Dictionary<Direction2D, Vector2Int> DirectionOffsets = new()
        {
            { Direction2D.Self, new Vector2Int(0, 0) },
            { Direction2D.Right, new Vector2Int(1, 0) },
            { Direction2D.Left, new Vector2Int(-1, 0) },
            { Direction2D.Up, new Vector2Int(0, -1) },
            { Direction2D.Down, new Vector2Int(0, 1) },
            { Direction2D.RightUp, new Vector2Int(1, -1) },
            { Direction2D.LeftUp, new Vector2Int(-1, -1) },
            { Direction2D.RightDown, new Vector2Int(1, 1) },
            { Direction2D.LeftDown, new Vector2Int(-1, 1) }
        };
    }
    
    public enum Direction2D
    {
        None = 0,
        Self = 1,
        Up = 2,
        Down = 3,
        Right = 4,
        Left = 5,
        LeftDown = 6,
        LeftUp = 7,
        RightUp = 8,
        RightDown = 9
    }
}