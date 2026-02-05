using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct SlideDownMoveRecord
    {
        public readonly BaseGridObject Item;
        
        // Path linked list (stored in external PathNodePool)
        public int HeadNode;
        public int TailNode;
        public int PathCount;
        
        // Final state
        public Vector2Int FinalCoord;
        public bool IsSpawn;
        public bool IsSlide;

        public SlideDownMoveRecord(BaseGridObject item)
        {
            Item = item;
            HeadNode = -1;
            TailNode = -1;
            PathCount = 0;
            FinalCoord = default;
            IsSpawn = false;
            IsSlide = false;
        }

        public void AppendNode(int node, Vector2Int coord, ref PathNodePool pool)
        {
            if (HeadNode < 0)
            {
                HeadNode = node;
            }
            else
            {
                pool.Link(TailNode, node);
            }

            TailNode = node;
            PathCount++;
            FinalCoord = coord;
        }

        public void MarkAsSlide(ref PathNodePool pool)
        {
            if (PathCount == 0 || HeadNode < 0) return;
            
            var startCoord = pool.GetCoord(HeadNode);
            IsSlide = startCoord.x != FinalCoord.x;
        }
    }
}